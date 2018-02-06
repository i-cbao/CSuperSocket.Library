using System;
using System.Xml.Linq;

namespace Dynamic.Core.Xml
{
    public  class EmptyXmlTester : XmlTester
    {
        public override bool Test(XElement root, XElement current)
        {
            return true;
        }
    }
}
