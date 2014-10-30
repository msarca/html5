using System;
using System.Collections.Generic;
using System.Text;

namespace HTML5
{
    internal class ElementStack<T> where T:class
    {
        public Element<T> Current, Oldest;
        public TreeBuilder<T> builder;

        public ElementStack(TreeBuilder<T> treeBuilder)
        {
            this.builder = treeBuilder;
        }

        public Element<T> Push(Element<T> element)
        {
            element.InStack = true;
            if (Current == null)
            {
                Current = element;
                Oldest = element;
            }
            else
            {
                element.Prev = Current;
                Current.Next = element;
                Current = element;
            }
            return element;
        }

        public void AddAfter(Element<T> reference, Element<T> element)
        {
            element.InStack = true;
            if (reference == null)
            {
                if (Current == null)
                {
                    Current = element;
                    Oldest = element;
                    return;
                }
                reference = Current;
            }
            if (reference == Current)
            {
                Current.Next = element;
                element.Prev = Current;
                Current = element;
            }
            else
            {
                element.Next = reference.Next;
                element.Prev = reference;
                reference.Next.Prev = element;
                reference.Next = element;
            }
        }

        public void AddBefore(Element<T> reference, Element<T> element)
        {
            element.InStack = true;
            if (reference == null)
            {
                if (Oldest == null)
                {
                    Current = element;
                    Oldest = element;
                    return;
                }
                reference = Oldest;
            }
            if (Oldest == reference)
            {
                Oldest.Prev = element;
                element.Next = Oldest;
                Oldest = element;
            }
            else
            {
                element.Prev = reference.Prev;
                element.Next = reference;
                reference.Prev.Next = element;
                reference.Prev = element;
            }
        }

        public void Replace(Element<T> oldElement, Element<T> newElement)
        {
            if (Current == oldElement)
            {
                if (Oldest == oldElement)
                {
                    Current = newElement;
                    Oldest = newElement;
                }
                else
                {
                    Current.Prev.Next = newElement;
                    newElement.Prev = Current.Prev;
                    Current = newElement;
                }
            }
            else
            {
                if (Oldest == oldElement)
                {
                    Oldest.Next.Prev = newElement;
                    newElement.Next = Oldest.Next;
                    Oldest = newElement;
                }
                else
                {
                    oldElement.Next.Prev = newElement;
                    oldElement.Prev.Next = newElement;
                    newElement.Next = oldElement.Next;
                    newElement.Prev = oldElement.Prev;
                }
            }
            newElement.InStack = true;
            oldElement.Next = null;
            oldElement.Prev = null;
            oldElement.InStack = false;
        }

        public Element<T> Pop()
        {
            Element<T> element = Current;
            if (Current == Oldest)
            {
                Current = null;
                Oldest = null;
            }
            else
            {
                Current = element.Prev;
                Current.Next = null;
            }
            element.Next = null;
            element.Prev = null;
            element.InStack = false;
            return element;
        }

        public Element<T> GetLastTable()
        {
            for (Element<T> element = Current; element != null; element = element.Prev)
                if (element.TagName == "table" && element.Namespace == TreeBuilder<T>.NS_HTML)
                    return element;
            return null;
        }

        public void Remove(Element<T> element)
        {
            if (Current == element)
            {
                if (Oldest == element)
                {
                    Current = null;
                    Oldest = null;
                }
                else
                {
                    Current = element.Prev;
                    Current.Next = null;
                }
            }
            else
            {
                if (element == Oldest)
                {
                    Oldest = element.Next;
                    Oldest.Prev = null;
                }
                else
                {
                    element.Prev.Next = element.Next;
                    element.Next.Prev = element.Prev;
                }
            }
            element.Prev = null;
            element.Next = null;
            element.InStack = false;
        }

        public void RemoveTo(Element<T> element, bool inclusive)
        {
            bool done = false;
            while (!done)
            {
                if (Current == element)
                {
                    if (inclusive)
                        done = true;
                    else
                        return;
                }
                Element<T> node = Current;
                Current = node.Prev;
                node.Prev = null;
                node.InStack = false;
                if (Current == null)
                {
                    Oldest = null;
                    break;
                }
                else
                {
                    Current.Next = null;
                }
            }
        }

        public void ClearToTableContext()
        {

        }

        public void ClearToTableBodyContext()
        {

        }

        public void ClearToElement(string tag, string namespaceURI)
        {
            bool done = false;
            while (!done)
            {
                if (Current.TagName == tag && Current.Namespace == namespaceURI)
                    done = true;
                Element<T> element = Current;
                Current = Current.Prev;
                element.Prev = null;
                element.InStack = false;
                if (Current == null)
                {
                    Oldest = null;
                    break;
                }
                else
                {
                    Current.Next = null;
                }
            }
        }

        public void ClearToTag(string tag)
        {
            bool done = false;
            while (!done)
            {
                if (Current.TagName == tag)
                    done = true;
                Element<T> element = Current;
                Current = Current.Prev;
                element.Prev = null;
                element.InStack = false;
                if (Current == null)
                {
                    Oldest = null;
                    break;
                }
                else
                {
                    Current.Next = null;
                }
            }
        }

        public void ClearToHeading()
        {
            bool done = false;
            while (!done)
            {
                switch (Current.TagName)
                {
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        done = true;
                        break;
                }
                Element<T> element = Current;
                Current = element.Prev;
                element.Prev = null;
                element.InStack = false;
                if (Current == null)
                {
                    Oldest = null;
                    break;
                }
                else
                {
                    Current.Next = null;
                }
            }
        }

        public void GenerateImpliedEndTags(string exclude)
        {
            while (true)
            {
                switch (Current.TagName)
                {
                    case "dd":
                    case "dt":
                    case "li":
                    case "option":
                    case "optgroup":
                    case "p":
                    case "rp":
                    case "rt":
                        if (Current.TagName == exclude)
                            return;
                        Element<T> element = Current;
                        Current = Current.Prev;
                        Current.Next = null;
                        element.Prev = null;
                        element.InStack = false;
                        continue;
                    default:
                        return;
                }
            }
        }

        public override string ToString()
        {
            if (Oldest == null)
                return "#empty";
            StringBuilder sb = new StringBuilder();
            for (Element<T> node = Oldest; node != null; node = node.Next)
                sb.Append(node.ToString() + ", ");
            return sb.ToString();
        }
    }


}
