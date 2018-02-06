using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dynamic.Core.Xml
{
    public class XmlComputer
    {
        public virtual object Compute(XElement current, XElement root, object context)
        {
            return null;
        }

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