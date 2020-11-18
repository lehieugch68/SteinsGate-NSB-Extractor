using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SteinsGateNSB_Extractor
{
    public class NSB
    {
        private static short _StringType = 216;
        public static List<string> Unpack(string input)
        {
            List<string> result = new List<string>();
            BinaryReader reader = new BinaryReader(File.OpenRead(input));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int index = reader.ReadInt32();
                short type = reader.ReadInt16();
                short strCount = reader.ReadInt16();
                if (strCount == 0) continue;
                for (int i = 0; i < strCount; i++)
                {
                    int strLen = reader.ReadInt32();
                    if (type == _StringType)
                    {
                        string str = Encoding.Unicode.GetString(reader.ReadBytes(strLen));
                        result.Add($"/*{index}-{i}*/");
                        result.Add(str);
                    }
                    else
                    {
                        reader.BaseStream.Position += strLen;
                    }
                }
            }
            reader.Close();
            return result;
        }

        public static byte[] Repack(string txtFile, string nsbFile)
        {
            MemoryStream result = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(result);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            StreamReader txtReader = new StreamReader(File.OpenRead(txtFile));
            string line = string.Empty;
            while (!txtReader.EndOfStream)
            {
                if (line.StartsWith("/*") && line.EndsWith("*/"))
                {
                    string key = line;
                    List<string> str = new List<string>();
                    try
                    {
                        while (!(line = txtReader.ReadLine()).StartsWith("/*"))
                        {
                            str.Add(line);
                        }
                    }
                    catch { }
                    dict.Add(key, string.Join("\n", str.ToArray()));
                }
                else
                {
                    line = txtReader.ReadLine();
                }    
            }
            txtReader.Close();
            BinaryReader nsbReader = new BinaryReader(File.OpenRead(nsbFile));
            while (nsbReader.BaseStream.Position < nsbReader.BaseStream.Length)
            {
                int index = nsbReader.ReadInt32();
                short type = nsbReader.ReadInt16();
                short strCount = nsbReader.ReadInt16();
                writer.Write(index);
                writer.Write(type);
                writer.Write(strCount);
                for (int i = 0; i < strCount; i++)
                {
                    int strLen = nsbReader.ReadInt32();
                    byte[] strBytes = nsbReader.ReadBytes(strLen);
                    string newStr;
                    if (dict.TryGetValue($"/*{index}-{i}*/", out newStr))
                    {
                        byte[] utf8Bytes = Encoding.UTF8.GetBytes(newStr);
                        byte[] utf16Bytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8Bytes);
                        writer.Write(utf16Bytes.Length);
                        writer.Write(utf16Bytes);
                    }
                    else
                    {
                        writer.Write(strLen);
                        writer.Write(strBytes);
                    }                        
                }
            }
            nsbReader.Close();
            writer.Close();
            return result.ToArray();
        }
    }
}
