using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SteinsGateNSB_Extractor
{
    class Program
    {
        private const string _NSBExt = ".nsb";
        private const string _TextExt = ".txt";
        static void Main(string[] args)
        {
            Console.Title = "SteinsGate NSB Extractor by LeHieu - VietHoaGame";
            if (args.Length > 0)
            {
                foreach (string input in args)
                {
                    string ext = Path.GetExtension(input).ToLower();
                    string name = Path.GetFileName(input);
                    switch (ext)
                    {
                        case _NSBExt:
                            List<string> output = NSB.Unpack(input);
                            string outFile = Path.Combine(Path.GetDirectoryName(input), $"{Path.GetFileNameWithoutExtension(input)}.{_TextExt.TrimStart((char)46)}");
                            File.WriteAllLines(outFile, output.ToArray());
                            Console.WriteLine($"Unpacked: {name} -> {Path.GetFileName(outFile)}");
                            break;
                        case _TextExt:
                            string nsbFile = Path.Combine(Path.GetDirectoryName(input), $"{Path.GetFileNameWithoutExtension(input)}.{_NSBExt.TrimStart((char)46)}");
                            if (!File.Exists(nsbFile))
                            {
                                Console.WriteLine($"Skip: {name}. Reason: NSB file not found.");
                            }
                            else
                            {
                                byte[] bytes = NSB.Repack(input, nsbFile);
                                File.WriteAllBytes($"{nsbFile}", bytes);
                                Console.WriteLine($"Repacked: {name} -> {Path.GetFileName(nsbFile)}");
                            }    
                            break;
                        default:
                            Console.WriteLine($"Skip: {name}. Reason: The file format is not supported.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Please drag and drop files into this tool to unpack/repack.");
            }    
            Console.ReadKey();
        }
    }
}
