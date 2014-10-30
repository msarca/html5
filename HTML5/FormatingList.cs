using System;
using System.Collections.Generic;
using System.Text;

namespace HTML5
{
    internal class FormatingList<T> where T: class
    {
        public Entry<T> First;
        public Entry<T> Last;

        public void Mark()
        {
            Entry<T> entry = new Entry<T>();
            if (Last == null)
            {
                First = entry;
                Last = entry;
            }
            else
            {
                Last.Next = entry;
                entry.Prev = Last;
                Last = entry;
            }
        }

        public void Add(Element<T> element)
        {
            Entry<T> entry = new Entry<T>(element);
            element.InList = true;
            if (Last == null)
            {
                Last = entry;
                First = entry;
            }
            else
            {
                Last.Next = entry;
                entry.Prev = Last;
                Last = entry;
            }
        }

        public void AddAfter(Entry<T> reference, Element<T> element)
        {
            Entry<T> entry = new Entry<T>(element);
            element.InList = true;
            if (reference == null)
                reference = Last;
            if (Last == reference)
            {
                Last.Next = entry;
                entry.Prev = Last;
                Last = entry;
            }
            else
            {
                entry.Next = reference.Next;
                entry.Prev = reference;
                reference.Next.Prev = entry;
                reference.Next = entry;
            }
        }

        public void AddBefore(Entry<T> reference, Element<T> element)
        {
            Entry<T> entry = new Entry<T>(element);
            element.InList = true;

            if (reference == null)
                reference = First;
            if (First == reference)
            {
                if (First == null)
                {
                    First = entry;
                    Last = entry;
                }
                else
                {
                    First.Prev = entry;
                    entry.Next = First;
                    First = entry;
                }
            }
            else
            {
                entry.Prev = reference.Prev;
                reference.Prev.Next = entry;
                entry.Next = reference;
                reference.Prev = entry;
            }
        }

        public void Remove(Entry<T> entry)
        {
            if (entry == First)
            {
                if (entry == Last)
                {
                    First = null;
                    Last = null;
                }
                else
                {
                    First = First.Next;
                    First.Prev = null;
                }
            }
            else
            {
                if (entry == Last)
                {
                    Last = Last.Prev;
                    Last.Next = null;
                }
                else
                {
                    entry.Prev.Next = entry.Next;
                    entry.Next.Prev = entry.Prev;
                }
            }

            if (!entry.IsMark)
            {
                entry.Element.InList = false;
                entry.Element.Entry = null;
            }
            entry.Next = null;
            entry.Prev = null;
        }

        public void Replace(Entry<T> entry, Element<T> element)
        {
            if (entry.IsMark)
                entry.IsMark = false;
            else
            {
                entry.Element.InList = false;
                entry.Element.Entry = null;
            }
            element.InList = true;
            element.Entry = entry;
            entry.Element = element;
        }

        public void ClearToMark()
        {
            if (Last == null)
                return;
            bool done = false;
            while (!done)
            {
                if (Last.IsMark)
                {
                    Last.IsMark = false;
                    done = true;
                }
                else
                {
                    Last.Element.InList = false;
                    Last.Element.Entry = null;
                }
                Entry<T> entry = Last;
                Last = Last.Prev;
                entry.Prev = null;
                entry.Next = null;

                if (Last == null)
                    First = null;
                else
                    Last.Next = null;
            }
        }

    }

    internal class Entry<T> where T : class
    {
        public Element<T> Element;
        public bool IsMark;
        public Entry<T> Prev, Next;
       
        public Entry()
        {
            IsMark = true;
        }

        public Entry(Element<T> element)
        {
            Element = element;
            element.Entry = this;
        }

    }

}
