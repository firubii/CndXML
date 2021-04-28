using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CndXML
{
    public static class WriteUtil
    {
        public static void WriteString(EndianBinaryWriter writer, string str)
        {
            writer.Write(str.Length);
            writer.Write(Encoding.UTF8.GetBytes(str));
            writer.Write(0);
            while ((writer.BaseStream.Length & 0xF) != 0x0
                && (writer.BaseStream.Length & 0xF) != 0x4
                && (writer.BaseStream.Length & 0xF) != 0x8
                && (writer.BaseStream.Length & 0xF) != 0xC)
                writer.Write((byte)0);
        }
    }
}
