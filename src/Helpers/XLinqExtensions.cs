using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace AzureLetsEncrypt.Helpers
{
    public static class XLinqExtensions
    {
        public static XElement GetOrAdd(this XElement element, string name)
        {
            if (element.Element(name) == null) element.Add(new XElement(name));
            return element.Element(name);
        }

        public static XElement GetOrAdd(this XDocument element, string name)
        {
            if (element.Element(name) == null) element.Add(new XElement(name));
            return element.Element(name);
        }
    }
}
