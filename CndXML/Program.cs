using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;

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

                    CNDBin cnd = new CNDBin(args[1]);

                    Console.WriteLine("Creating XML file...");

                    XmlDocument xml = new XmlDocument();
                    xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", ""));

                    XmlElement root = xml.CreateElement("CND");
                    root.SetAttribute("name", cnd.Name);
                    root.SetAttribute("type", cnd.Type);
                    root.SetAttribute("isKF2", cnd.NewVersion.ToString());

                    XmlElement visSection = xml.CreateElement("VisualSection");
                    for (int i = 0; i < cnd.VisualSection.Count; i++)
                    {
                        XmlElement cndStruct = xml.CreateElement("Struct");
                        cndStruct.SetAttribute("name", cnd.VisualSection[i].Name);
                        cndStruct.SetAttribute("type", cnd.VisualSection[i].Type);
                        cndStruct.SetAttribute("flags", cnd.VisualSection[i].Flags.ToString());

                        for (int g = 0; g < cnd.VisualSection[i].Groups.Count; g++)
                        {
                            XmlElement group = xml.CreateElement("Group");
                            group.SetAttribute("name", cnd.VisualSection[i].Groups[g].Name);

                            for (int v = 0; v < cnd.VisualSection[i].Groups[g].Variables.Count; v++)
                            {
                                XmlElement variable = xml.CreateElement(cnd.VisualSection[i].Groups[g].Variables[v].Type);
                                variable.SetAttribute("name", cnd.VisualSection[i].Groups[g].Variables[v].Name);
                                variable.SetAttribute("flags", cnd.VisualSection[i].Groups[g].Variables[v].Flags.ToString());

                                if (cnd.VisualSection[i].Groups[g].Variables[v].Type == "Color4")
                                {
                                    Color c = (Color)cnd.VisualSection[i].Groups[g].Variables[v].GetValue();
                                    XmlElement cR = xml.CreateElement("R");
                                    cR.InnerText = c.R.ToString();
                                    XmlElement cG = xml.CreateElement("G");
                                    cG.InnerText = c.G.ToString();
                                    XmlElement cB = xml.CreateElement("B");
                                    cB.InnerText = c.B.ToString();
                                    XmlElement cA = xml.CreateElement("A");
                                    cA.InnerText = c.A.ToString();

                                    variable.AppendChild(cR);
                                    variable.AppendChild(cG);
                                    variable.AppendChild(cB);
                                    variable.AppendChild(cA);
                                }
                                else if (cnd.VisualSection[i].Groups[g].Variables[v].Type == "Vec3")
                                {
                                    float[] vec = cnd.VisualSection[i].Groups[g].Variables[v].GetValue() as float[];
                                    XmlElement vX = xml.CreateElement("X");
                                    vX.InnerText = vec[0].ToString();
                                    XmlElement vY = xml.CreateElement("Y");
                                    vY.InnerText = vec[1].ToString();
                                    XmlElement vZ = xml.CreateElement("Z");
                                    vZ.InnerText = vec[2].ToString();

                                    variable.AppendChild(vX);
                                    variable.AppendChild(vY);
                                    variable.AppendChild(vZ);
                                }
                                else
                                    variable.InnerText = cnd.VisualSection[i].Groups[g].Variables[v].GetValue().ToString();

                                group.AppendChild(variable);
                            }

                            cndStruct.AppendChild(group);
                        }

                        visSection.AppendChild(cndStruct);
                    }

                    XmlElement rndSection = xml.CreateElement("RenderSection");
                    for (int i = 0; i < cnd.RenderSection.Count; i++)
                    {
                        XmlElement cndStruct = xml.CreateElement("Struct");
                        cndStruct.SetAttribute("name", cnd.RenderSection[i].Name);
                        cndStruct.SetAttribute("type", cnd.RenderSection[i].Type);
                        cndStruct.SetAttribute("flags", cnd.RenderSection[i].Flags.ToString());

                        for (int g = 0; g < cnd.RenderSection[i].Groups.Count; g++)
                        {
                            XmlElement group = xml.CreateElement("Group");
                            group.SetAttribute("name", cnd.RenderSection[i].Groups[g].Name);

                            for (int v = 0; v < cnd.RenderSection[i].Groups[g].Variables.Count; v++)
                            {
                                XmlElement variable = xml.CreateElement(cnd.RenderSection[i].Groups[g].Variables[v].Type);
                                variable.SetAttribute("name", cnd.RenderSection[i].Groups[g].Variables[v].Name);
                                variable.SetAttribute("flags", cnd.RenderSection[i].Groups[g].Variables[v].Flags.ToString());

                                if (cnd.RenderSection[i].Groups[g].Variables[v].Type == "Color4")
                                {
                                    Color c = (Color)cnd.RenderSection[i].Groups[g].Variables[v].GetValue();
                                    XmlElement cR = xml.CreateElement("R");
                                    cR.InnerText = c.R.ToString();
                                    XmlElement cG = xml.CreateElement("G");
                                    cG.InnerText = c.G.ToString();
                                    XmlElement cB = xml.CreateElement("B");
                                    cB.InnerText = c.B.ToString();
                                    XmlElement cA = xml.CreateElement("A");
                                    cA.InnerText = c.A.ToString();

                                    variable.AppendChild(cR);
                                    variable.AppendChild(cG);
                                    variable.AppendChild(cB);
                                    variable.AppendChild(cA);
                                }
                                else if (cnd.RenderSection[i].Groups[g].Variables[v].Type == "Vec3")
                                {
                                    float[] vec = cnd.RenderSection[i].Groups[g].Variables[v].GetValue() as float[];
                                    XmlElement vX = xml.CreateElement("X");
                                    vX.InnerText = vec[0].ToString();
                                    XmlElement vY = xml.CreateElement("Y");
                                    vY.InnerText = vec[1].ToString();
                                    XmlElement vZ = xml.CreateElement("Z");
                                    vZ.InnerText = vec[2].ToString();

                                    variable.AppendChild(vX);
                                    variable.AppendChild(vY);
                                    variable.AppendChild(vZ);
                                }
                                else
                                    variable.InnerText = cnd.RenderSection[i].Groups[g].Variables[v].GetValue().ToString();

                                group.AppendChild(variable);
                            }

                            cndStruct.AppendChild(group);
                        }

                        rndSection.AppendChild(cndStruct);
                    }

                    root.AppendChild(visSection);
                    root.AppendChild(rndSection);

                    xml.AppendChild(root);

                    xml.Save(outFile);

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

                    XmlDocument xml = new XmlDocument();
                    xml.Load(args[1]);
                    string name = xml["CND"].GetAttribute("name");
                    string type = xml["CND"].GetAttribute("type");
                    bool newVer = bool.Parse(xml["CND"].GetAttribute("isKF2"));

                    Console.WriteLine("Creating CND binary...");

                    CNDBin cnd = new CNDBin(name, type, newVer, Endianness.Little);

                    XmlElement cSection = xml["CND"]["VisualSection"];
                    for (int i = 0; i < cSection.ChildNodes.Count; i++)
                    {
                        XmlNode structNode = cSection.ChildNodes[i];
                        CNDStruct cStruct = new CNDStruct(structNode.Attributes["name"].Value, structNode.Attributes["type"].Value, uint.Parse(structNode.Attributes["flags"].Value));
                        for (int g = 0; g < structNode.ChildNodes.Count; g++)
                        {
                            XmlNode groupNode = structNode.ChildNodes[g];
                            CNDGroup cGroup = new CNDGroup(groupNode.Attributes["name"].Value);
                            for (int v = 0; v < groupNode.ChildNodes.Count; v++)
                            {
                                XmlNode varNode = groupNode.ChildNodes[v];
                                string vName = varNode.Attributes["name"].Value;
                                string vType = varNode.Name;
                                uint vFlags = uint.Parse(varNode.Attributes["flags"].Value);
                                object vValue;
                                switch (vType)
                                {
                                    case "Int":
                                        {
                                            vValue = int.Parse(varNode.InnerText);
                                            break;
                                        }
                                    case "Float":
                                        {
                                            vValue = float.Parse(varNode.InnerText);
                                            break;
                                        }
                                    case "Bool":
                                        {
                                            vValue = bool.Parse(varNode.InnerText);
                                            break;
                                        }
                                    case "String":
                                        {
                                            vValue = varNode.InnerText;
                                            break;
                                        }
                                    case "Vec3":
                                        {
                                            vValue = new float[3] {
                                                float.Parse(varNode["X"].InnerText),
                                                float.Parse(varNode["Y"].InnerText),
                                                float.Parse(varNode["Z"].InnerText),
                                            };
                                            break;
                                        }
                                    case "Color4":
                                        {
                                            vValue = Color.FromArgb(byte.Parse(varNode["A"].InnerText),
                                                    byte.Parse(varNode["R"].InnerText),
                                                    byte.Parse(varNode["G"].InnerText),
                                                    byte.Parse(varNode["B"].InnerText));
                                            break;
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException("Unknown CND variable type");
                                        }
                                }
                                cGroup.Variables.Add(new CNDVariable(vName, vType, vFlags, vValue));
                            }
                            cStruct.Groups.Add(cGroup);
                        }
                        cnd.VisualSection.Add(cStruct);
                    }

                    cSection = xml["CND"]["RenderSection"];
                    for (int i = 0; i < cSection.ChildNodes.Count; i++)
                    {
                        XmlNode structNode = cSection.ChildNodes[i];
                        CNDStruct cStruct = new CNDStruct(structNode.Attributes["name"].Value, structNode.Attributes["type"].Value, uint.Parse(structNode.Attributes["flags"].Value));
                        for (int g = 0; g < structNode.ChildNodes.Count; g++)
                        {
                            XmlNode groupNode = structNode.ChildNodes[g];
                            CNDGroup cGroup = new CNDGroup(groupNode.Attributes["name"].Value);
                            for (int v = 0; v < groupNode.ChildNodes.Count; v++)
                            {
                                XmlNode varNode = groupNode.ChildNodes[v];
                                string vName = varNode.Attributes["name"].Value;
                                string vType = varNode.Name;
                                uint vFlags = uint.Parse(varNode.Attributes["flags"].Value);
                                object vValue;
                                switch (vType)
                                {
                                    case "Int":
                                        {
                                            vValue = int.Parse(varNode.InnerText);
                                            break;
                                        }
                                    case "Float":
                                        {
                                            vValue = float.Parse(varNode.InnerText);
                                            break;
                                        }
                                    case "Bool":
                                        {
                                            vValue = bool.Parse(varNode.InnerText);
                                            break;
                                        }
                                    case "String":
                                        {
                                            vValue = varNode.InnerText;
                                            break;
                                        }
                                    case "Vec3":
                                        {
                                            vValue = new float[3] {
                                                float.Parse(varNode["X"].InnerText),
                                                float.Parse(varNode["Y"].InnerText),
                                                float.Parse(varNode["Z"].InnerText),
                                            };
                                            break;
                                        }
                                    case "Color4":
                                        {
                                            vValue = Color.FromArgb(byte.Parse(varNode["A"].InnerText),
                                                    byte.Parse(varNode["R"].InnerText),
                                                    byte.Parse(varNode["G"].InnerText),
                                                    byte.Parse(varNode["B"].InnerText));
                                            break;
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException("Unknown CND variable type");
                                        }
                                }
                                cGroup.Variables.Add(new CNDVariable(vName, vType, vFlags, vValue));
                            }
                            cStruct.Groups.Add(cGroup);
                        }
                        cnd.RenderSection.Add(cStruct);
                    }

                    cnd.Write(outFile);
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
            Console.WriteLine("Usage: CndXML.exe <-d|-a> <cndbin path> [options]");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  -o <path>: Sets output filepath");
        }
    }
}
