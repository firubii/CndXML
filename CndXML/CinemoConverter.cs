using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using KirbyLib;
using KirbyLib.IO;

namespace CndXML
{
    internal class CinemoConverter
    {
        XmlDocument xml;

        public XmlDocument Convert(Cinemo cinemo)
        {
            xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", ""));

            XmlElement xdataRoot = xml.CreateElement("XData");
            xdataRoot.SetAttribute("version", $"{cinemo.XData.Version[0]}.{cinemo.XData.Version[1]}");
            xdataRoot.SetAttribute("endianness", cinemo.XData.Endianness.ToString());

            XmlElement cinemoRoot = xml.CreateElement("CinemoDynamics");
            cinemoRoot.SetAttribute("type", cinemo.Type);
            cinemoRoot.SetAttribute("name", cinemo.Name);

            cinemoRoot.AppendChild(CreateObjectSectionElement(cinemo.VisualSection, "VisualSection"));
            cinemoRoot.AppendChild(CreateObjectSectionElement(cinemo.RenderSection, "RenderSection"));

            xdataRoot.AppendChild(cinemoRoot);

            xml.AppendChild(xdataRoot);

            return xml;
        }

        public Cinemo Convert(XmlDocument doc)
        {
            Cinemo cinemo = new Cinemo();

            var xdataRoot = doc["XData"];
            string[] xVer = xdataRoot.Attributes["version"].Value.Split('.');
            cinemo.XData.Version = new byte[]
            {
                byte.Parse(xVer[0]),
                byte.Parse(xVer[1])
            };
            cinemo.XData.Endianness = Enum.Parse<Endianness>(xdataRoot.Attributes["endianness"].Value);

            var cinemoRoot = xdataRoot["CinemoDynamics"];
            cinemo.Type = cinemoRoot.Attributes["type"].Value;
            cinemo.Name = cinemoRoot.Attributes["name"].Value;

            cinemo.VisualSection = CreateObjectFromXml(cinemoRoot["VisualSection"]);
            cinemo.RenderSection = CreateObjectFromXml(cinemoRoot["RenderSection"]);

            return cinemo;
        }

        XmlElement CreateObjectSectionElement(List<Cinemo.CinemoObject> cinemoObject, string name)
        {
            XmlElement section = xml.CreateElement(name);
            for (int i = 0; i < cinemoObject.Count; i++)
            {
                var cndObj = cinemoObject[i];

                XmlElement xObj = xml.CreateElement("Object");
                xObj.SetAttribute("type", cndObj.Type);
                xObj.SetAttribute("name", cndObj.Name);

                for (int s = 0; s < cndObj.Structs.Count; s++)
                {
                    var cndStruct = cndObj.Structs[s];

                    XmlElement xStruct = xml.CreateElement("Struct");
                    xStruct.SetAttribute("name", cndStruct.Name);

                    for (int v = 0; v < cndStruct.Variables.Count; v++)
                    {
                        var cndVar = cndStruct.Variables[v];

                        XmlElement xVar = xml.CreateElement(cndVar.Type.ToString());
                        xVar.SetAttribute("name", cndVar.Name);

                        switch (cndVar.Type)
                        {
                            default:
                                xVar.InnerText = cndVar.GetValue().ToString();
                                break;
                            case CinemoType.Color4:
                                var color4 = cndVar.AsColor4();
                                xVar.AppendChild(xml.CreateElementWithText("R", color4.R.ToString()));
                                xVar.AppendChild(xml.CreateElementWithText("G", color4.G.ToString()));
                                xVar.AppendChild(xml.CreateElementWithText("B", color4.B.ToString()));
                                xVar.AppendChild(xml.CreateElementWithText("A", color4.A.ToString()));
                                break;
                            case CinemoType.Vec3:
                                var vec3 = cndVar.AsVec3();
                                xVar.AppendChild(xml.CreateElementWithText("X", vec3.X.ToString()));
                                xVar.AppendChild(xml.CreateElementWithText("Y", vec3.Y.ToString()));
                                xVar.AppendChild(xml.CreateElementWithText("Z", vec3.Z.ToString()));
                                break;
                        }

                        xStruct.AppendChild(xVar);
                    }

                    xObj.AppendChild(xStruct);
                }

                section.AppendChild(xObj);
            }

            return section;
        }

        List<Cinemo.CinemoObject> CreateObjectFromXml(XmlElement element)
        {
            List<Cinemo.CinemoObject> objects = new List<Cinemo.CinemoObject>();

            for (int i = 0; i < element.ChildNodes.Count; i++)
            {
                var objElement = element.ChildNodes[i];
                if (objElement.Name != "Object")
                    continue;

                Cinemo.CinemoObject cndObj = new Cinemo.CinemoObject();
                cndObj.Type = objElement.Attributes["type"].Value;
                cndObj.Name = objElement.Attributes["name"].Value;

                cndObj.Structs = new List<Cinemo.CinemoStruct>();

                for (int s = 0; s < objElement.ChildNodes.Count; s++)
                {
                    var structElement = objElement.ChildNodes[s];
                    if (structElement.Name != "Struct")
                        continue;

                    Cinemo.CinemoStruct cndStruct = new Cinemo.CinemoStruct();
                    cndStruct.Name = structElement.Attributes["name"].Value;

                    cndStruct.Variables = new List<CinemoVariable>();

                    for (int v = 0; v < structElement.ChildNodes.Count; v++)
                    {
                        var varElement = structElement.ChildNodes[v];
                        if (!Enum.TryParse(varElement.Name, out CinemoType type))
                            continue;

                        CinemoVariable cndVar;
                        switch (type)
                        {
                            case CinemoType.Int:
                                cndVar = new CinemoVariable(int.Parse(varElement.InnerText));
                                break;
                            case CinemoType.Float:
                                cndVar = new CinemoVariable(float.Parse(varElement.InnerText));
                                break;
                            case CinemoType.Bool:
                                cndVar = new CinemoVariable(bool.Parse(varElement.InnerText));
                                break;
                            default:
                            case CinemoType.String:
                                cndVar = new CinemoVariable(varElement.InnerText);
                                break;
                            case CinemoType.Color4:
                                cndVar = new CinemoVariable(Color.FromArgb(
                                    byte.Parse(varElement["A"].InnerText),
                                    byte.Parse(varElement["R"].InnerText),
                                    byte.Parse(varElement["G"].InnerText),
                                    byte.Parse(varElement["B"].InnerText)
                                ));
                                break;
                            case CinemoType.Vec3:
                                cndVar = new CinemoVariable(new Vector3(
                                    float.Parse(varElement["X"].InnerText),
                                    float.Parse(varElement["Y"].InnerText),
                                    float.Parse(varElement["Z"].InnerText)
                                ));
                                break;
                        }

                        cndVar.Name = varElement.Attributes["name"].Value;

                        cndStruct.Variables.Add(cndVar);
                    }

                    cndObj.Structs.Add(cndStruct);
                }

                objects.Add(cndObj);
            }

            return objects;
        }
    }
}
