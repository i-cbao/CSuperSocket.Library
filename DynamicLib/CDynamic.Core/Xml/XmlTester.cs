using System;
using System.Xml.Linq;
using System.Linq;

namespace Dynamic.Core.Xml
{
    public  abstract class XmlTester
    {
        public abstract bool Test(XElement root, XElement current);

        public XElement Child(XElement current, String path)
        {
            return current.GetChild(path);
        }

        public XAttribute Attribute(XElement current, String attributeName)
        {
            return current.Attributes().FirstOrDefault(x => x.Name.LocalName == attributeName);
        }
    }
}