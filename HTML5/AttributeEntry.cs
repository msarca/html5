using System;
using System.Collections.Generic;
using System.Text;

namespace HTML5
{
    public class AttributeEntry
    {
        public AttributeEntry NextAttribute;
        internal AttributeEntry PrevAttribute;
        public string Name, Value, Namespace;
        public AttributeEntry(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public AttributeEntry(string name, string value, string namespaceURI)
        {
            Name = name;
            Value = value;
            Namespace = namespaceURI;
        }


    }
}
