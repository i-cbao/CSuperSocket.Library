using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dynamic.Core.Xml
{

    public static class XElementExtensions
    {
        public static string GetXPathWithoutNamespace(this XElement node)
        {
            Stack<String> nodeNames = new Stack<string>();
            nodeNames.Push(node.Name.LocalName);
            XElement p = node.Parent;
            while (p != null)
            {
                nodeNames.Push(p.Name.LocalName);
                p = p.Parent;
            }

            StringBuilder sb = new StringBuilder();
            while (nodeNames.Any())
            {
                if (sb.Length > 0)
                {
                    sb.Append("/");
                }
                sb.Append(nodeNames.Pop());
            }

            return sb.ToString();
        }

        public static bool IsExistsChild(this XElement node, string path)
        {
            return getChild(node, path) != null;
        }

        public static XElement GetChild(this XElement node, String path)
        {
            return getChild(node, path);
        }

        private static XElement getChild(XElement node, String path)
        {
            string[] ps = path.Split('/');

            XElement child = node;
            for (int i = 0; i < ps.Length; i++)
            {
                child = child.Elements().FirstOrDefault(x => x.Name.LocalName == ps[i]);
                if (child == null)
                    break;
            }

            return child;
        }

        public static void RemoveAttributes(this XElement node, Func<XAttribute, bool> predicate)
        {
            node.Attributes().Where(predicate).Remove();

            foreach (XElement ele in node.Elements())
            {
                ele.RemoveAttributes(predicate);
            }
        }

        public static void ChangeNamespace(this XElement node, XNamespace ns)
        {
            if (ns == XNamespace.None)
            {
                node.Attributes().Where(x => x.Name.LocalName == "xmlns").Remove();
                node.Name = node.Name.LocalName;
            }
            else
            {
                node.Name = ns + node.Name.LocalName;
            }
            foreach (XElement ele in node.Elements())
            {
                ele.ChangeNamespace(ns);
            }
        }
    }
}
