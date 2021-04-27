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
            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    Console.WriteLine("Reading CND Binary...");

                    CNDBin cnd = new CNDBin(args[0]);

                    Console.WriteLine("Creating XML file...");

                    XmlDocument xml = new XmlDocument();
                    xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", ""));

                    XmlElement root = xml.CreateElement("CND");
                    root.SetAttribute("name", cnd.Name);
                    root.SetAttribute("type", cnd.Type);

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

                    xml.Save(Path.GetDirectoryName(args[0]) + "\\" + Path.GetFileNameWithoutExtension(args[0]) + ".xml");

                    Console.WriteLine($"Written XML file to {Path.GetDirectoryName(args[0])}\\{Path.GetFileNameWithoutExtension(args[0])}.xml");
                }
                else
                {
                    Console.WriteLine("Error: File does not exist!");
                }
            }
            else
            {
                Console.WriteLine("Usage: CndXML.exe <cndbin path>");
            }
        }
    }
}
