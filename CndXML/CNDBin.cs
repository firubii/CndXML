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

        public CNDBin(string name, string type, Endianness endianness)
        {
            XData = new XData(endianness);
            Name = name;
            Type = type;
            VisualSection = new List<CNDStruct>();
            RenderSection = new List<CNDStruct>();
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
                        Buffer.BlockCopy(BitConverter.GetBytes(v[0]), 0, Data, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(v[1]), 0, Data, 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(v[2]), 0, Data, 8, 4);
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
