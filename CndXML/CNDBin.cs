using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace CndXML
{
    public class CNDBin
    {
        public XData XData;

        public string Name;
        public string Type;
        public List<CNDStruct> VisualSection;
        public List<CNDStruct> RenderSection;

        public bool NewVersion;

        public CNDBin(string name, string type, bool isNewVersion, Endianness endianness)
        {
            XData = new XData(endianness);
            Name = name;
            Type = type;
            VisualSection = new List<CNDStruct>();
            RenderSection = new List<CNDStruct>();
            NewVersion = isNewVersion;
        }

        public CNDBin(string filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
                Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData = new XData(reader);
            if (reader.ReadUInt32() != 10100)
                throw new InvalidDataException("Invalid CND Binary");

            reader.BaseStream.Seek(0x20, SeekOrigin.Begin);
            uint visOffs = reader.ReadUInt32();
            uint unk1 = reader.ReadUInt32();
            uint nameOffs = reader.ReadUInt32();
            uint typeOffs = reader.ReadUInt32();
            uint rndOffs = reader.ReadUInt32();

            reader.BaseStream.Seek(nameOffs, SeekOrigin.Begin);
            Name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
            reader.BaseStream.Seek(typeOffs, SeekOrigin.Begin);
            Type = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

            VisualSection = new List<CNDStruct>();
            reader.BaseStream.Seek(visOffs, SeekOrigin.Begin);
            uint visCount = reader.ReadUInt32();
            for (int i = 0; i < visCount; i++)
            {
                reader.BaseStream.Seek(visOffs + 4 + (i * 4), SeekOrigin.Begin);
                reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);
                VisualSection.Add(new CNDStruct(reader));
            }

            RenderSection = new List<CNDStruct>();
            reader.BaseStream.Seek(rndOffs, SeekOrigin.Begin);
            uint rndCount = reader.ReadUInt32();
            for (int i = 0; i < rndCount; i++)
            {
                reader.BaseStream.Seek(rndOffs + 4 + (i * 4), SeekOrigin.Begin);
                reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);
                RenderSection.Add(new CNDStruct(reader));
            }

            if (VisualSection.Count > 0)
                if (VisualSection[0].Groups[0].Name != "")
                    NewVersion = true;
            else if (RenderSection.Count > 0)
                if (RenderSection[0].Groups[0].Name != "")
                    NewVersion = true;
        }

        public void Write(string filePath)
        {
            using (EndianBinaryWriter writer = new EndianBinaryWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write)))
                Write(writer);
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.Write(writer);
            writer.Write(10100);
            writer.Write(0);
            writer.Write(0x24);
            writer.Write(-1);
            writer.Write(0);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(0x34);

            writer.Write(RenderSection.Count);
            long pos = writer.BaseStream.Position;
            for (int i = 0; i < RenderSection.Count; i++)
            {
                writer.Write(-1);
            }
            List<long> rndStructOffsets = new List<long>();
            List<long[]> rndGroupOffsets = new List<long[]>();
            List<long[][]> rndVarOffsets = new List<long[][]>();
            for (int i = 0; i < RenderSection.Count; i++)
            {
                List<long> groups = new List<long>();
                List<long[]> allVars = new List<long[]>();

                writer.BaseStream.Seek(pos + (i * 4), SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Length);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                rndStructOffsets.Add(writer.BaseStream.Position);
                writer.Write(-1);
                writer.Write(-1);
                writer.Write(RenderSection[i].Flags);
                writer.Write((uint)writer.BaseStream.Length + 4);

                if (NewVersion)
                {
                    writer.Write(RenderSection[i].Groups.Count);
                    long gPos = writer.BaseStream.Position;
                    for (int g = 0; g < RenderSection[i].Groups.Count; g++)
                    {
                        writer.Write(-1);
                    }
                    for (int g = 0; g < RenderSection[i].Groups.Count; g++)
                    {
                        List<long> vars = new List<long>();

                        writer.BaseStream.Seek(gPos + (g * 4), SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        groups.Add(writer.BaseStream.Position);
                        writer.Write(-1);
                        writer.Write((uint)writer.BaseStream.Length + 4);

                        writer.Write(RenderSection[i].Groups[g].Variables.Count);
                        long vPos = writer.BaseStream.Position;
                        for (int v = 0; v < RenderSection[i].Groups[g].Variables.Count; v++)
                        {
                            writer.Write(-1);
                        }
                        for (int v = 0; v < RenderSection[i].Groups[g].Variables.Count; v++)
                        {
                            writer.BaseStream.Seek(vPos + (v * 4), SeekOrigin.Begin);
                            writer.Write((uint)writer.BaseStream.Length);
                            writer.BaseStream.Seek(0, SeekOrigin.End);

                            vars.Add(writer.BaseStream.Position);
                            writer.Write(-1);
                            writer.Write(-1);
                            writer.Write(RenderSection[i].Groups[g].Variables[v].Flags);

                            if (RenderSection[i].Groups[g].Variables[v].Type == "String")
                            {
                                writer.Write(0);
                                writer.Write(RenderSection[i].Groups[g].Variables[v].Data.Length);
                                writer.Write(RenderSection[i].Groups[g].Variables[v].Data);
                                writer.Write(0);
                                while ((writer.BaseStream.Length & 0xF) != 0x0
                                    && (writer.BaseStream.Length & 0xF) != 0x4
                                    && (writer.BaseStream.Length & 0xF) != 0x8
                                    && (writer.BaseStream.Length & 0xF) != 0xC)
                                    writer.Write((byte)0);
                            }
                            else
                            {
                                writer.Write(RenderSection[i].Groups[g].Variables[v].Data.Length);
                                writer.Write(RenderSection[i].Groups[g].Variables[v].Data);
                            }
                        }
                        allVars.Add(vars.ToArray());
                    }
                    rndGroupOffsets.Add(groups.ToArray());
                }
                else
                {
                    List<long> vars = new List<long>();
                    writer.Write(RenderSection[i].Groups[0].Variables.Count);
                    long vPos = writer.BaseStream.Position;
                    for (int v = 0; v < RenderSection[i].Groups[0].Variables.Count; v++)
                    {
                        writer.Write(-1);
                    }
                    for (int v = 0; v < RenderSection[i].Groups[0].Variables.Count; v++)
                    {
                        writer.BaseStream.Seek(vPos + (v * 4), SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        vars.Add(writer.BaseStream.Position);
                        writer.Write(-1);
                        writer.Write(-1);
                        writer.Write(RenderSection[i].Groups[0].Variables[v].Flags);

                        if (RenderSection[i].Groups[0].Variables[v].Type == "String")
                        {
                            writer.Write(0);
                            writer.Write(RenderSection[i].Groups[0].Variables[v].Data.Length);
                            writer.Write(RenderSection[i].Groups[0].Variables[v].Data);
                            writer.Write(0);
                            while ((writer.BaseStream.Length & 0xF) != 0x0
                                && (writer.BaseStream.Length & 0xF) != 0x4
                                && (writer.BaseStream.Length & 0xF) != 0x8
                                && (writer.BaseStream.Length & 0xF) != 0xC)
                                writer.Write((byte)0);
                        }
                        else
                        {
                            writer.Write(RenderSection[i].Groups[0].Variables[v].Data.Length);
                            writer.Write(RenderSection[i].Groups[0].Variables[v].Data);
                        }
                    }
                    allVars.Add(vars.ToArray());
                }
                rndVarOffsets.Add(allVars.ToArray());
            }

            writer.BaseStream.Seek(0x20, SeekOrigin.Begin);
            writer.Write((uint)writer.BaseStream.Length);
            writer.BaseStream.Seek(0, SeekOrigin.End);

            writer.Write(VisualSection.Count);
            pos = writer.BaseStream.Position;
            for (int i = 0; i < VisualSection.Count; i++)
            {
                writer.Write(-1);
            }
            List<long> visStructOffsets = new List<long>();
            List<long[]> visGroupOffsets = new List<long[]>();
            List<long[][]> visVarOffsets = new List<long[][]>();
            for (int i = 0; i < VisualSection.Count; i++)
            {
                List<long> groups = new List<long>();
                List<long[]> allVars = new List<long[]>();

                writer.BaseStream.Seek(pos + (i * 4), SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Length);
                writer.BaseStream.Seek(0, SeekOrigin.End);

                visStructOffsets.Add(writer.BaseStream.Position);
                writer.Write(-1);
                writer.Write(-1);
                writer.Write(VisualSection[i].Flags);
                writer.Write((uint)writer.BaseStream.Length + 4);

                if (NewVersion)
                {
                    writer.Write(VisualSection[i].Groups.Count);
                    long gPos = writer.BaseStream.Position;
                    for (int g = 0; g < VisualSection[i].Groups.Count; g++)
                    {
                        writer.Write(-1);
                    }
                    for (int g = 0; g < VisualSection[i].Groups.Count; g++)
                    {
                        List<long> vars = new List<long>();

                        writer.BaseStream.Seek(gPos + (g * 4), SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        groups.Add(writer.BaseStream.Position);
                        writer.Write(-1);
                        writer.Write((uint)writer.BaseStream.Length + 4);

                        writer.Write(VisualSection[i].Groups[g].Variables.Count);
                        long vPos = writer.BaseStream.Position;
                        for (int v = 0; v < VisualSection[i].Groups[g].Variables.Count; v++)
                        {
                            writer.Write(-1);
                        }
                        for (int v = 0; v < VisualSection[i].Groups[g].Variables.Count; v++)
                        {
                            writer.BaseStream.Seek(vPos + (v * 4), SeekOrigin.Begin);
                            writer.Write((uint)writer.BaseStream.Length);
                            writer.BaseStream.Seek(0, SeekOrigin.End);

                            vars.Add(writer.BaseStream.Position);
                            writer.Write(-1);
                            writer.Write(-1);
                            writer.Write(VisualSection[i].Groups[g].Variables[v].Flags);

                            if (VisualSection[i].Groups[g].Variables[v].Type == "String")
                            {
                                writer.Write(0);
                                writer.Write(VisualSection[i].Groups[g].Variables[v].Data.Length);
                                writer.Write(VisualSection[i].Groups[g].Variables[v].Data);
                                writer.Write(0);
                                while ((writer.BaseStream.Length & 0xF) != 0x0
                                    && (writer.BaseStream.Length & 0xF) != 0x4
                                    && (writer.BaseStream.Length & 0xF) != 0x8
                                    && (writer.BaseStream.Length & 0xF) != 0xC)
                                    writer.Write((byte)0);
                            }
                            else
                            {
                                writer.Write(VisualSection[i].Groups[g].Variables[v].Data.Length);
                                writer.Write(VisualSection[i].Groups[g].Variables[v].Data);
                            }
                        }
                        allVars.Add(vars.ToArray());
                    }
                    visGroupOffsets.Add(groups.ToArray());
                }
                else
                {
                    List<long> vars = new List<long>();
                    writer.Write(VisualSection[i].Groups[0].Variables.Count);
                    long vPos = writer.BaseStream.Position;
                    for (int v = 0; v < VisualSection[i].Groups[0].Variables.Count; v++)
                    {
                        writer.Write(-1);
                    }
                    for (int v = 0; v < VisualSection[i].Groups[0].Variables.Count; v++)
                    {
                        writer.BaseStream.Seek(vPos + (v * 4), SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);

                        vars.Add(writer.BaseStream.Position);
                        writer.Write(-1);
                        writer.Write(-1);
                        writer.Write(VisualSection[i].Groups[0].Variables[v].Flags);

                        if (VisualSection[i].Groups[0].Variables[v].Type == "String")
                        {
                            writer.Write(0);
                            writer.Write(VisualSection[i].Groups[0].Variables[v].Data.Length);
                            writer.Write(VisualSection[i].Groups[0].Variables[v].Data);
                            writer.Write(0);
                            while ((writer.BaseStream.Length & 0xF) != 0x0
                                && (writer.BaseStream.Length & 0xF) != 0x4
                                && (writer.BaseStream.Length & 0xF) != 0x8
                                && (writer.BaseStream.Length & 0xF) != 0xC)
                                writer.Write((byte)0);
                        }
                        else
                        {
                            writer.Write(VisualSection[i].Groups[0].Variables[v].Data.Length);
                            writer.Write(VisualSection[i].Groups[0].Variables[v].Data);
                        }
                    }
                    allVars.Add(vars.ToArray());
                }
                visVarOffsets.Add(allVars.ToArray());
            }

            writer.BaseStream.Seek(0x28, SeekOrigin.Begin);
            writer.Write((uint)writer.BaseStream.Length);
            writer.BaseStream.Seek(0, SeekOrigin.End);
            WriteUtil.WriteString(writer, Name);

            writer.BaseStream.Seek(0x2C, SeekOrigin.Begin);
            writer.Write((uint)writer.BaseStream.Length);
            writer.BaseStream.Seek(0, SeekOrigin.End);
            WriteUtil.WriteString(writer, Type);

            for (int i = 0; i < rndStructOffsets.Count; i++)
            {
                writer.BaseStream.Seek(rndStructOffsets[i], SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Length);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                WriteUtil.WriteString(writer, RenderSection[i].Name);

                writer.BaseStream.Seek(rndStructOffsets[i] + 4, SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Length);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                WriteUtil.WriteString(writer, RenderSection[i].Type);

                if (NewVersion)
                {
                    for (int g = 0; g < rndGroupOffsets[i].Length; g++)
                    {
                        writer.BaseStream.Seek(rndGroupOffsets[i][g], SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        WriteUtil.WriteString(writer, RenderSection[i].Groups[g].Name);

                        for (int v = 0; v < rndVarOffsets[i][g].Length; v++)
                        {
                            writer.BaseStream.Seek(rndVarOffsets[i][g][v], SeekOrigin.Begin);
                            writer.Write((uint)writer.BaseStream.Length);
                            writer.BaseStream.Seek(0, SeekOrigin.End);
                            WriteUtil.WriteString(writer, RenderSection[i].Groups[g].Variables[v].Name);

                            writer.BaseStream.Seek(rndVarOffsets[i][g][v] + 4, SeekOrigin.Begin);
                            writer.Write((uint)writer.BaseStream.Length);
                            writer.BaseStream.Seek(0, SeekOrigin.End);
                            WriteUtil.WriteString(writer, RenderSection[i].Groups[g].Variables[v].Type);
                        }
                    }
                }
                else
                {
                    for (int v = 0; v < rndVarOffsets[i][0].Length; v++)
                    {
                        writer.BaseStream.Seek(rndVarOffsets[i][0][v], SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        WriteUtil.WriteString(writer, RenderSection[i].Groups[0].Variables[v].Name);

                        writer.BaseStream.Seek(rndVarOffsets[i][0][v] + 4, SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        WriteUtil.WriteString(writer, RenderSection[i].Groups[0].Variables[v].Type);
                    }
                }
            }

            for (int i = 0; i < visStructOffsets.Count; i++)
            {
                writer.BaseStream.Seek(visStructOffsets[i], SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Length);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                WriteUtil.WriteString(writer, VisualSection[i].Name);

                writer.BaseStream.Seek(visStructOffsets[i] + 4, SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Length);
                writer.BaseStream.Seek(0, SeekOrigin.End);
                WriteUtil.WriteString(writer, VisualSection[i].Type);

                if (NewVersion)
                {
                    for (int g = 0; g < visGroupOffsets[i].Length; g++)
                    {
                        writer.BaseStream.Seek(visGroupOffsets[i][g], SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        WriteUtil.WriteString(writer, VisualSection[i].Groups[g].Name);

                        for (int v = 0; v < visVarOffsets[i][g].Length; v++)
                        {
                            writer.BaseStream.Seek(visVarOffsets[i][g][v], SeekOrigin.Begin);
                            writer.Write((uint)writer.BaseStream.Length);
                            writer.BaseStream.Seek(0, SeekOrigin.End);
                            WriteUtil.WriteString(writer, VisualSection[i].Groups[g].Variables[v].Name);

                            writer.BaseStream.Seek(visVarOffsets[i][g][v] + 4, SeekOrigin.Begin);
                            writer.Write((uint)writer.BaseStream.Length);
                            writer.BaseStream.Seek(0, SeekOrigin.End);
                            WriteUtil.WriteString(writer, VisualSection[i].Groups[g].Variables[v].Type);
                        }
                    }
                }
                else
                {
                    for (int v = 0; v < visVarOffsets[i][0].Length; v++)
                    {
                        writer.BaseStream.Seek(visVarOffsets[i][0][v], SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        WriteUtil.WriteString(writer, VisualSection[i].Groups[0].Variables[v].Name);

                        writer.BaseStream.Seek(visVarOffsets[i][0][v] + 4, SeekOrigin.Begin);
                        writer.Write((uint)writer.BaseStream.Length);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        WriteUtil.WriteString(writer, VisualSection[i].Groups[0].Variables[v].Type);
                    }
                }
            }

            XData.UpdateFilesize(writer);
            XData.WriteFooter(writer);
        }
    }

    public class CNDStruct
    {
        public string Name;
        public string Type;
        public uint Flags;
        public List<CNDGroup> Groups;

        public CNDStruct(string name, string type, uint flags)
        {
            Name = name;
            Type = type;
            Flags = flags;
            Groups = new List<CNDGroup>();
        }

        public CNDStruct(EndianBinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            uint nameOffs = reader.ReadUInt32();
            uint typeOffs = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            uint grpOffs = reader.ReadUInt32();

            reader.BaseStream.Seek(nameOffs, SeekOrigin.Begin);
            Name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
            reader.BaseStream.Seek(typeOffs, SeekOrigin.Begin);
            Type = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

            Groups = new List<CNDGroup>();
            reader.BaseStream.Seek(grpOffs, SeekOrigin.Begin);
            uint grpCount = reader.ReadUInt32();
            for (int i = 0; i < grpCount; i++)
            {
                reader.BaseStream.Seek(grpOffs + 4 + (i * 4), SeekOrigin.Begin);
                uint dataOffs = reader.ReadUInt32();
                reader.BaseStream.Seek(dataOffs + 4, SeekOrigin.Begin);

                //Check version based on group data offset
                //Because the file doesn't have a version indicator in the header, we have to do this to
                //detect the version instead.

                //If group offset is 8 bytes away from the given offset, treat the file as the KF2 format.
                //If not, treat the file as the KSA/SKC format.
                if (reader.ReadUInt32() == dataOffs + 8)
                {
                    reader.BaseStream.Seek(dataOffs, SeekOrigin.Begin);
                    Groups.Add(new CNDGroup(reader));
                }
                else
                {
                    reader.BaseStream.Seek(dataOffs, SeekOrigin.Begin);
                    if (Groups.Count == 0)
                        Groups.Add(new CNDGroup(""));

                    Groups[0].Variables.Add(new CNDVariable(reader));
                }
            }
        }
    }

    public class CNDGroup
    {
        public string Name;
        public List<CNDVariable> Variables;

        public CNDGroup(string name)
        {
            Name = name;
            Variables = new List<CNDVariable>();
        }

        public CNDGroup(EndianBinaryReader reader)
        {
            uint nameOffs = reader.ReadUInt32();
            uint dataOffs = reader.ReadUInt32();

            reader.BaseStream.Seek(nameOffs, SeekOrigin.Begin);
            Name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

            Variables = new List<CNDVariable>();
            reader.BaseStream.Seek(dataOffs, SeekOrigin.Begin);
            uint varCount = reader.ReadUInt32();
            for (int i = 0; i < varCount; i++)
            {
                reader.BaseStream.Seek(dataOffs + 4 + (i * 4), SeekOrigin.Begin);
                reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);
                Variables.Add(new CNDVariable(reader));
            }
        }
    }

    public class CNDVariable
    {
        public string Name;
        public string Type;
        public uint Flags;
        public byte[] Data;

        public CNDVariable(string name, string type, uint flags, object value)
        {
            Name = name;
            Type = type;
            Flags = flags;
            SetValue(value);
        }

        public CNDVariable(EndianBinaryReader reader)
        {
            long pos = reader.BaseStream.Position;

            uint nameOffs = reader.ReadUInt32();
            uint typeOffs = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            int dataSize = reader.ReadInt32();

            reader.BaseStream.Seek(nameOffs, SeekOrigin.Begin);
            Name = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
            reader.BaseStream.Seek(typeOffs, SeekOrigin.Begin);
            Type = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));

            reader.BaseStream.Seek(pos + 0x10, SeekOrigin.Begin);
            if (Type != "String")
                Data = reader.ReadBytes(dataSize);
            else
                Data = reader.ReadBytes(reader.ReadInt32());
        }

        public object GetValue()
        {
            switch (Type)
            {
                case "Int":
                    {
                        return BitConverter.ToInt32(Data, 0);
                    }
                case "Float":
                    {
                        return BitConverter.ToSingle(Data, 0);
                    }
                case "Bool":
                    {
                        return Convert.ToBoolean(BitConverter.ToInt32(Data, 0));
                    }
                case "Color4":
                    {
                        return Color.FromArgb(Data[3], Data[0], Data[1], Data[2]);
                    }
                case "Vec3":
                    {
                        return new float[3] {
                            BitConverter.ToSingle(Data, 0),
                            BitConverter.ToSingle(Data, 4),
                            BitConverter.ToSingle(Data, 8),
                        };
                    }
                case "String":
                    {
                        return Encoding.UTF8.GetString(Data);
                    }
                default:
                    {
                        throw new NotImplementedException("Unknown CND Variable type");
                    }
            }
        }

        public void SetValue(object value)
        {
            switch (Type)
            {
                case "Int":
                    {
                        Data = BitConverter.GetBytes((int)value);
                        break;
                    }
                case "Float":
                    {
                        Data = BitConverter.GetBytes((float)value);
                        break;
                    }
                case "Bool":
                    {
                        Data = BitConverter.GetBytes(Convert.ToInt32((bool)value));
                        break;
                    }
                case "Color4":
                    {
                        Color c = (Color)value;
                        Data = new byte[] { c.R, c.G, c.B, c.A };
                        break;
                    }
                case "Vec3":
                    {
                        float[] v = value as float[];
                        Data = new byte[12];
                        Array.Copy(BitConverter.GetBytes(v[0]), 0, Data, 0, 4);
                        Array.Copy(BitConverter.GetBytes(v[1]), 0, Data, 4, 4);
                        Array.Copy(BitConverter.GetBytes(v[2]), 0, Data, 8, 4);
                        break;
                    }
                case "String":
                    {
                        Data = Encoding.UTF8.GetBytes(value as string);
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException("Unknown CND Variable type");
                    }
            }
        }
    }
}
