using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using KirbyLib;
using KirbyLib.IO;

namespace CndXML
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                string outFile = (Path.GetDirectoryName(args[1]) + "\\" + Path.GetFileNameWithoutExtension(args[1])).TrimStart('\\');

                if (args.Contains("-o"))
                {
                    int index = args.ToList().IndexOf("-o");
                    if (args.Length < index + 1)
                    {
                        Console.WriteLine("Error: No output file specified.");
                        return;
                    }

                    outFile = args[index + 1];
                }

                if (args[0] == "-d")
                {
                    if (!outFile.EndsWith(".xml"))
                        outFile += ".xml";

                    if (!File.Exists(args[1]))
                    {
                        Console.WriteLine("Error: File does not exist!");
                        return;
                    }

                    Console.WriteLine("Reading CND Binary...");

                    if (args.Contains("--ksa"))
                    {
                        CinemoKSA cnd;
                        using (FileStream stream = new FileStream(args[1], FileMode.Open, FileAccess.Read))
                        using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                            cnd = new CinemoKSA(reader);

                        Console.WriteLine("Creating XML file...");

                        new CinemoKSAConverter().Convert(cnd).Save(outFile);
                    }
                    else
                    {
                        Cinemo cnd;
                        using (FileStream stream = new FileStream(args[1], FileMode.Open, FileAccess.Read))
                        using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                            cnd = new Cinemo(reader);

                        Console.WriteLine("Creating XML file...");

                        new CinemoConverter().Convert(cnd).Save(outFile);
                    }

                    Console.WriteLine($"Written XML file to {outFile}");
                }
                else if (args[0] == "-a")
                {
                    if (!outFile.EndsWith(".cndbin"))
                        outFile += ".cndbin";

                    if (!File.Exists(args[1]))
                    {
                        Console.WriteLine("Error: File does not exist!");
                        return;
                    }

                    Console.WriteLine("Reading XML file...");

                    XmlDocument doc = new XmlDocument();
                    doc.Load(args[1]);

                    if (args.Contains("--ksa"))
                    {
                        CinemoKSA cnd = new CinemoKSAConverter().Convert(doc);

                        using (FileStream stream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
                        using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                            cnd.Write(writer);
                    }
                    else
                    {
                        Cinemo cnd = new CinemoConverter().Convert(doc);

                        using (FileStream stream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
                        using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                            cnd.Write(writer);
                    }

                    Console.WriteLine($"Written CND binary to {outFile}");
                }
                else
                {
                    PrintHelp();
                }
            }
            else
            {
                PrintHelp();
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage: CndXML.exe <-d|-a> <in path> [options]");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  --ksa:     Sets to old Cinemo mode (Star Allies, Super Kirby Clash)");
            Console.WriteLine("  -o <path>: Sets output filepath");
        }
    }
}
