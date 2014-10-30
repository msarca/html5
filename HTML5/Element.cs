using System;

namespace HTML5
{
    internal class Element<T> where T : class
    {
        public T Value;
        public ElementToken Token;
        public string TagName, Namespace;
        public Element<T> Next, Prev;
        public Entry<T> Entry;
        public bool InStack, InList;

        public Element(T element, ElementToken token, string namespaceURI)
        {
            Value = element;
            TagName = token.TagName;
            Token = token;
            Namespace = namespaceURI;
        }

        public Element(T element, string tagName, string namespaceURI)
        {
            Value = element;
            TagName = tagName;
            Namespace = namespaceURI;
            Token = new ElementToken(tagName);
        }

        public override string ToString()
        {
            return TagName;
        }
    }
}
