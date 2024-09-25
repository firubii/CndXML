using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CndXML
{
    public static class Util
    {
        public static XmlElement CreateElementWithText(this XmlDocument doc, string name, string text)
        {
            var element = doc.CreateElement(name);
            element.InnerText = text;
            return element;
        }
    }
}
