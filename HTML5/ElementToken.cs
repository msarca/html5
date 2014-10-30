using System;

namespace HTML5
{

    internal struct ElementToken
    {
        public string TagName;
        public readonly AttributeEntry Attributes;
        public readonly int AttrCount;
        public readonly bool SelfClosing;
        public bool Acknowledged;

        public ElementToken(string tagName, AttributeEntry attributes, int attrCount, bool selfClosing)
        {
            TagName = tagName;
            Attributes = attributes;
            AttrCount = attrCount;
            SelfClosing = selfClosing;
            Acknowledged = false;
        }
        
        public ElementToken(string tagName)
        {
            TagName = tagName;
            Attributes = null;
            AttrCount = 0;
            SelfClosing = false;
            Acknowledged = false;
        }
    }
}
