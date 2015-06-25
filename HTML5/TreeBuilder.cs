using System;
using System.Text;

namespace HTML5
{
    public abstract class TreeBuilder<T> where T: class
    {

        #region INSERTION MODE
        const byte REPROCESS = 0;
        const byte INITIAL = 1;
        const byte BEFORE_HTML = 2;
        const byte BEFORE_HEAD = 3;
        const byte IN_HEAD = 4;
        const byte IN_HEAD_NOSCRIPT = 5;
        const byte AFTER_HEAD = 6;
        const byte IN_BODY = 7;
        const byte TEXT = 8;
        const byte IN_TABLE = 9;
        const byte IN_TABLE_TEXT = 10;
        const byte IN_CAPTION = 11;
        const byte IN_COLUMN_GROUP = 12;
        const byte IN_TABLE_BODY = 13;
        const byte IN_ROW = 14;
        const byte IN_CELL = 15;
        const byte IN_SELECT = 16;
        const byte IN_SELECT_IN_TABLE = 17;
        const byte IN_FOREIGN_CONTENT = 18;
        const byte AFTER_BODY = 19;
        const byte IN_FRAMESET = 20;
        const byte AFTER_FRAMESET = 21;
        const byte AFTER_AFTER_BODY = 22;
        const byte AFTER_AFTER_FRAMESET = 23;
        #endregion

        #region CONSTANTS

        private static string[] publicIds = new string[] 
        {
            "+//Silmaril//dtd html Pro v0r11 19970101//", "-//AdvaSoft Ltd//DTD HTML 3.0 asWedit + extensions//",
            "-//AS//DTD HTML 3.0 asWedit + extensions//", "-//IETF//DTD HTML 2.0 Level 1//",
            "-//IETF//DTD HTML 2.0 Level 2//", "-//IETF//DTD HTML 2.0 Strict Level 1//", 
            "-//IETF//DTD HTML 2.0 Strict Level 2//", "-//IETF//DTD HTML 2.0 Strict//", 
            "-//IETF//DTD HTML 2.0//","-//IETF//DTD HTML 2.1E//", "-//IETF//DTD HTML 3.0//",
            "-//IETF//DTD HTML 3.2 Final//", "-//IETF//DTD HTML 3.2//", 
            "-//IETF//DTD HTML 3//","-//IETF//DTD HTML Level 0//", 
            "-//IETF//DTD HTML Level 1//", "-//IETF//DTD HTML Level 2//", 
            "-//IETF//DTD HTML Level 3//", "-//IETF//DTD HTML Strict Level 0//", 
            "-//IETF//DTD HTML Strict Level 1//", "-//IETF//DTD HTML Strict Level 2//", 
            "-//IETF//DTD HTML Strict Level 3//", "-//IETF//DTD HTML Strict//", 
            "-//IETF//DTD HTML//", "-//Metrius//DTD Metrius Presentational//", 
            "-//Microsoft//DTD Internet Explorer 2.0 HTML Strict//", "-//Microsoft//DTD Internet Explorer 2.0 HTML//", 
            "-//Microsoft//DTD Internet Explorer 2.0 Tables//", "-//Microsoft//DTD Internet Explorer 3.0 HTML Strict//", 
            "-//Microsoft//DTD Internet Explorer 3.0 HTML//", "-//Microsoft//DTD Internet Explorer 3.0 Tables//", 
            "-//Netscape Comm. Corp.//DTD HTML//", "-//Netscape Comm. Corp.//DTD Strict HTML//", 
            "-//O'Reilly and Associates//DTD HTML 2.0//", "-//O'Reilly and Associates//DTD HTML Extended 1.0//", 
            "-//O'Reilly and Associates//DTD HTML Extended Relaxed 1.0//",
            "-//SoftQuad Software//DTD HoTMetaL PRO 6.0::19990601::extensions to HTML 4.0//", 
            "-//SoftQuad//DTD HoTMetaL PRO 4.0::19971010::extensions to HTML 4.0//", 
            "-//Spyglass//DTD HTML 2.0 Extended//","-//SQ//DTD HTML 2.0 HoTMetaL + extensions//", 
            "-//Sun Microsystems Corp.//DTD HotJava HTML//","-//Sun Microsystems Corp.//DTD HotJava Strict HTML//", 
            "-//W3C//DTD HTML 3 1995-03-24//", "-//W3C//DTD HTML 3.2 Draft//", 
            "-//W3C//DTD HTML 3.2 Final//", "-//W3C//DTD HTML 3.2//","-//W3C//DTD HTML 3.2S Draft//", 
            "-//W3C//DTD HTML 4.0 Frameset//", "-//W3C//DTD HTML 4.0 Transitional//", 
            "-//W3C//DTD HTML Experimental 19960712//", "-//W3C//DTD HTML Experimental 970421//", 
            "-//W3C//DTD W3 HTML//","-//W3O//DTD W3 HTML 3.0//", "-//WebTechs//DTD Mozilla HTML 2.0//", 
            "-//WebTechs//DTD Mozilla HTML//"
        };


        public const string NS_HTML = "http://www.w3.org/1999/xhtml";
        public const string NS_MathML = "http://www.w3.org/1998/Math/MathML";
        public const string NS_SVG = "http://www.w3.org/2000/svg";
        public const string NS_XLink = "http://www.w3.org/1999/xlink";
        public const string NS_XML = "http://www.w3.org/XML/1998/namespace";
        public const string NS_XMLNS = "http://www.w3.org/2000/xmlns/";




        #endregion

        #region INTERNAL FIELDS
        byte INSERTION_MODE = 1;
        byte ORIGINAL_INSERTION_MODE = 1;
        bool FRAMESETOK = true, IS_FRAGMENT = false, IGNORE_LF_TOKEN = false, FOSTER_PARENTED = false;
        DocumentQuirkFlag DocMode = DocumentQuirkFlag.NoQuirksMode;
        protected bool SCRIPTING_ENABLED = true;
        Element<T> formPointer = null, headPointer = null, context = null;
        internal ElementStack<T> O;
        FormatingList<T> F;
        Tokenizer<T> tokenizer;
        char[] pendingTokenList = null;
        int pendingTokenListPtr = 0, pendingTokenListCount = 1024;
        #endregion

        #region CONSTRUCTOR
        public TreeBuilder()
        {
            O = new ElementStack<T>(this);
            F = new FormatingList<T>();
            tokenizer = new Tokenizer<T>(this);
            pendingTokenList = new char[pendingTokenListCount];
        }

        public TreeBuilder(T context)
            :this()
        {
            IS_FRAGMENT = true;
            if (context != null)
            {
                DocMode = GetOwnerDocumentQuirkFlag(context);
                string tagName = GetElementName(context);
                string namespaceURI = GetElementNamespace(context);
                ElementToken contextToken = new ElementToken(tagName);
                this.context = new Element<T>(context, contextToken, namespaceURI);
                if (namespaceURI == NS_HTML)
                {
                    switch (tagName)
                    {
                        case "title":
                        case "textarea":
                            tokenizer.STATE = Tokenizer<T>.RCDATA_STATE;
                            break;
                        case "style":
                        case "xmp":
                        case "iframe":
                        case "noembed":
                        case "noframes":
                            tokenizer.STATE = Tokenizer<T>.RAWTEXT_STATE;
                            break;
                        case "script":
                            tokenizer.STATE = Tokenizer<T>.SCRIPT_DATA_STATE;
                            break;
                        case "noscript":
                            if (SCRIPTING_ENABLED)
                                tokenizer.STATE = Tokenizer<T>.RAWTEXT_STATE;
                            else
                                goto default;
                            break;
                        case "plaintext":
                            tokenizer.STATE = Tokenizer<T>.PLAINTEXT_STATE;
                            break;
                        default:
                            tokenizer.STATE = Tokenizer<T>.DATA_STATE;
                            break;
                    }
                }
                T element = CreateElement("html", null, NS_HTML);
                AppendElementToDocument(element);
                O.Push(new Element<T>(element, "html", NS_HTML));
                ResetInsertionMode();
                T form = GetFormAncestor(context);
                if (form != null)
                {
                    ElementToken formToken = new ElementToken("form");
                    formPointer = new Element<T>(form, formToken, NS_HTML);
                }
                
            }
        }
        #endregion

        #region ABSTRACT METHODS
        public abstract T CreateElement(string name, AttributeEntry attribute, string namespaceURI);
        public abstract void AppendText(T parent, string c);
        public abstract void AppendChild(T parent, T child);
        public abstract void AppendChild(T parent, string text);
        public abstract void AppendComment(T parent, string comment);
        public abstract void AppendElementToDocument(T html);
        public abstract void AppendCommentToDocument(string text);
        public abstract void AppendDoctypeToDocument(string name, string publicId, string systemId);
        public abstract void AddAttributes(T owner, AttributeEntry attribute);
        public abstract void SetAttribute(T owner, string name, string value);
        public abstract void InsertBefore(T element, T reference);
        public abstract void InsertBefore(string text, T reference);
        public abstract void RemoveFromParent(T element);
        public abstract T GetElementParent(T element);
        public abstract void MoveChilds(T source, T destination);
        public abstract string GetElementName(T element);
        public abstract string GetElementNamespace(T element);
        public abstract T GetFormAncestor(T element);
        public abstract DocumentQuirkFlag GetOwnerDocumentQuirkFlag(T element);
        public abstract void SetScriptFlag(T script, ScriptElementFlag flag, bool value);
        public abstract bool GetScriptFlag(T script, ScriptElementFlag flag);
        public abstract void DocumentParsed();
        public abstract void DocumentFragmentParsed();
        public abstract void PrepareScript(T script);
        public abstract void SetFormOwner(T element, T form);
        #endregion

        #region PUBLIC METHODS

        public void Feed(byte[] chunck, int length)
        {
            tokenizer.Parse(chunck, length);
        }

        public void Eof()
        {
            tokenizer.ParseEof();
        }

        public void SaveInsertionPoint()
        {
            tokenizer.pendingBufferCopy = tokenizer.pendingBuffer;
            tokenizer.pendingBufferCopyLength = tokenizer.pendingBufferLength;
            tokenizer.ByteDecoderCopy = tokenizer.ByteDecoder;
            tokenizer.pendingBuffer = null;
            tokenizer.pendingBufferLength = 0;
            tokenizer.ByteDecoder = Encoding.UTF8.GetDecoder();
        }

        public void RestoreInsertionPoint()
        {
            tokenizer.pendingBuffer = tokenizer.pendingBufferCopy;
            tokenizer.pendingBufferLength = tokenizer.pendingBufferCopyLength;
            tokenizer.ByteDecoder = tokenizer.ByteDecoderCopy;
            tokenizer.pendingBufferCopy = null;
            tokenizer.pendingBufferCopyLength = 0;
            tokenizer.ByteDecoderCopy = null;
        }

        public void SetEncoding(Encoding encoding)
        {
            tokenizer.ByteDecoder = encoding.GetDecoder();
        }

        public void ScriptingEnabled(bool enabled)
        {
            SCRIPTING_ENABLED = enabled;
        }

        #endregion

        #region INTERNAL METHODS

        private void DoublePendingTokenList()
        {
            pendingTokenListCount = pendingTokenListCount << 1;
            char[] tmp = new char[pendingTokenListCount];
            Array.Copy(pendingTokenList, tmp, pendingTokenListPtr);
            pendingTokenList = tmp;
        }

        private bool ContainsNonWhiteSpace(ref char[] t, int start, int length)
        {
            for (int i = start; i < length; i++)
            {
                unchecked
                {
                    switch (t[i])
                    {
                        case '\u0020':
                        case '\u0009':
                        case '\u000a':
                        case '\u000c':
                        case '\u000d':
                            continue;
                        default:
                            return true;
                    }
                }
            }
            return false;
        }

        internal void ResetInsertionMode()
        {
            for (Element<T> node = O.Current; node != null; node = node.Prev)
            {
                bool last = false;
                if (node == O.Oldest)
                {
                    last = true;
                    node = context;
                }
                switch (node.TagName)
                {
                    case "select":
                        INSERTION_MODE = IN_SELECT;
                        return;
                    case "td":
                    case "th":
                        if (!last)
                        {
                            INSERTION_MODE = IN_CELL;
                            return;
                        }
                        else
                            goto default;
                    case "tr":
                        INSERTION_MODE = IN_ROW;
                        return;
                    case "tbody":
                    case "thead":
                    case "tfoot":
                        INSERTION_MODE = IN_TABLE_BODY;
                        return;
                    case "caption":
                        INSERTION_MODE = IN_CAPTION;
                        return;
                    case "colgroup":
                        INSERTION_MODE = IN_COLUMN_GROUP;
                        return;
                    case "table":
                        INSERTION_MODE = IN_TABLE;
                        return;
                    case "head":
                    case "body":
                        INSERTION_MODE = IN_BODY;
                        return;
                    case "frameset":
                        INSERTION_MODE = IN_FRAMESET;
                        return;
                    case "html":
                        INSERTION_MODE = BEFORE_HEAD;
                        return;
                    default:
                        if (last)
                        {
                            INSERTION_MODE = IN_BODY;
                            return;
                        }
                        break;
                }
            }
        }

        private void GenericRawTextAlgorithm(ref ElementToken token)
        {
            InsertHtmlElement(ref token);
            tokenizer.STATE = Tokenizer<T>.RAWTEXT_STATE;
            ORIGINAL_INSERTION_MODE = INSERTION_MODE;
            INSERTION_MODE = TEXT;
        }

        private void GenericRCDataAlgorithm(ref ElementToken token)
        {
            InsertHtmlElement(ref token);
            tokenizer.STATE = Tokenizer<T>.RCDATA_STATE;
            ORIGINAL_INSERTION_MODE = INSERTION_MODE;
            INSERTION_MODE = TEXT;
        }

        private bool HeadingInScope()
        {
            for (Element<T> node = O.Current; node != null; node = node.Prev)
            {
                switch (node.TagName)
                {
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        if (node.Namespace == NS_HTML) return true;
                        continue;
                    case "applet":
                    case "caption":
                    case "html":
                    case "table":
                    case "td":
                    case "th":
                    case "marquee":
                    case "object":
                        if (node.Namespace == NS_HTML) return false;
                        continue;
                    case "mi":
                    case "mo":
                    case "mn":
                    case "ms":
                    case "mtext":
                    case "annotation-xml":
                        if (node.Namespace == NS_MathML) return false;
                        continue;
                    case "foreignObject":
                    case "desc":
                    case "title":
                        if (node.Namespace == NS_SVG) return false;
                        continue;
                }
            }
            return false;
        }

        private bool ElementInScope(string element)
        {
            for (Element<T> node = O.Current; node != null; node = node.Prev)
            {
                if (node.TagName == element)
                    return true;
                switch (node.TagName)
                {
                    case "applet":
                    case "caption": 
                    case "html":
                    case "table": 
                    case "td":
                    case "th":
                    case "marquee":
                    case "object":
                        if (node.Namespace == NS_HTML) return false;
                        continue;
                    case "mi": 
                    case "mo": 
                    case "mn": 
                    case "ms": 
                    case "mtext":
                    case "annotation-xml":
                        if (node.Namespace == NS_MathML) return false;
                        continue;
                    case "foreignObject":
                    case "desc":
                    case "title":
                        if (node.Namespace == NS_SVG) return false;
                        continue;
                }
            }
            return false;
        }

        private bool ElementInListItemScope(string element)
        {
            for (Element<T> node = O.Current; node != null; node = node.Prev)
            {
                if (node.TagName == element)
                    return true;
                switch (node.TagName)
                {
                    case "applet":
                    case "caption":
                    case "html":
                    case "table":
                    case "td":
                    case "th":
                    case "marquee":
                    case "object":
                    case "ol":
                    case "ul":
                        if (node.Namespace == NS_HTML) return false;
                        continue;
                    case "mi":
                    case "mo":
                    case "mn":
                    case "ms":
                    case "mtext":
                    case "annotation-xml":
                        if (node.Namespace == NS_MathML) return false;
                        continue;
                    case "foreignObject":
                    case "desc":
                    case "title":
                        if (node.Namespace == NS_SVG) return false;
                        continue;
                }
            }
            return false;
        }

        private bool ElementInButtonScope(string element)
        {
            for (Element<T> node = O.Current; node != null; node = node.Prev)
            {
                if (node.TagName == element)
                    return true;
                switch (node.TagName)
                {
                    case "applet":
                    case "caption":
                    case "html":
                    case "table":
                    case "td":
                    case "th":
                    case "marquee":
                    case "object":
                    case "button":
                        if (node.Namespace == NS_HTML) return false;
                        continue;
                    case "mi":
                    case "mo":
                    case "mn":
                    case "ms":
                    case "mtext":
                    case "annotation-xml":
                        if (node.Namespace == NS_MathML) return false;
                        continue;
                    case "foreignObject":
                    case "desc":
                    case "title":
                        if (node.Namespace == NS_SVG) return false;
                        continue;
                }
            }
            return false;
        }

        private bool ElementInTableScope(string element)
        {
            for (Element<T> node = O.Current; node != null; node = node.Prev)
            {
                if (node.TagName == element)
                    return true;
                if ((node.TagName == "table" || node.TagName == "html") && node.Namespace == NS_HTML)
                    return false;
            }
            return false;
        }

        private bool ElementInSelectScope(string element)
        {
            if(O.Current.TagName == element)
                return true;
            if(O.Current.TagName == "option" || O.Current.TagName == "optgroup")
                return O.Current.Namespace == NS_HTML;
            return false;
        }

        private bool IsSpecialElement(Element<T> element)
        {
            switch (element.TagName)
            {
                case "address":
                case "applet":
                case "area":
                case "article":
                case "aside":
                case "base":
                case "basefont":
                case "bgsound":
                case "blockquote":
                case "body":
                case "br":
                case "button":
                case "caption":
                case "center":
                case "col":
                case "colgroup":
                case "command":
                case "dd":
                case "details":
                case "dir":
                case "div":
                case "dl":
                case "dt":
                case "embed":
                case "fieldset":
                case "figcaption":
                case "figure":
                case "footer":
                case "form":
                case "frame":
                case "frameset":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                case "head":
                case "header":
                case "hgroup":
                case "hr":
                case "html":
                case "iframe":
                case "img":
                case "input":
                case "isindex":
                case "li":
                case "link":
                case "listing":
                case "marquee":
                case "menu":
                case "meta":
                case "nav":
                case "noembed":
                case "noframes":
                case "noscript":
                case "object":
                case "ol":
                case "p":
                case "param":
                case "plaintext":
                case "pre":
                case "script":
                case "section":
                case "select":
                case "source":
                case "style":
                case "summary":
                case "table":
                case "tbody":
                case "td":
                case "textarea":
                case "tfoot":
                case "th":
                case "thead":
                case "tr":
                case "track":
                case "ul":
                case "wbr":
                    return element.Namespace == NS_HTML;
                case "mi":
                case "mo":
                case "mn":
                case "ms":
                case "mtext":
                case "annotation-xml":
                    return element.Namespace == NS_MathML;
                case "foreignObject":
                case "desc":
                    return element.Namespace == NS_SVG;
                case "title":
                    return element.Namespace == NS_SVG || element.Namespace == NS_HTML;
                default:
                    return false;
            }
            
        }

        private bool IsFormattingElement(Element<T> element)
        {
            switch (element.TagName)
            {
                case "a":
                case "b":
                case "big":
                case "code":
                case "em":
                case "font":
                case "i":
                case "nobr":
                case "s":
                case "small":
                case "strike":
                case "strong":
                case "tt":
                case "u":
                    return element.Namespace == NS_HTML;
                default:
                    return false;
            }
        }

        private bool IsFormAssociatedElement(Element<T> element)
        {
            switch (element.TagName)
            {
                case "button":
                case "fieldset":
                case "input":
                case "keygen":
                case "label":
                case "meter":
                case "object":
                case "output":
                case "progress":
                case "select":
                case "textarea":
                    return element.Namespace == NS_HTML;
                default:
                    return false;
            }
        }

        private void PushFormattingElement(Element<T> element)
        {
            int count = 0;
            Element<T> erliest = null;
            for (Entry<T> node = F.Last; node != null && !node.IsMark; node = node.Prev)
            {
                if (node.Element.TagName != element.TagName ||
                   node.Element.Namespace != element.Namespace ||
                   node.Element.Token.AttrCount != element.Token.AttrCount)
                    continue;
                bool sameAttributes = true;
                if(node.Element.Token.Attributes != element.Token.Attributes)
                    for (AttributeEntry e1 = node.Element.Token.Attributes; e1 != null; e1 = e1.NextAttribute)
                    {
                        sameAttributes = false;
                        for (AttributeEntry e2 = element.Token.Attributes; e2 != null; e2 = e2.NextAttribute)
                            if (e1.Name == e2.Name && e1.Value == e2.Value && e1.Namespace == e2.Namespace)
                            {
                                sameAttributes = true;
                                break;
                            }
                        if (!sameAttributes)
                            break;
                    }
                if (sameAttributes)
                {
                    erliest = node.Element;
                    count++;
                }
                if (count == 3)
                {
                    F.Remove(erliest.Entry);
                    break;
                }
            }
            F.Add(element);
        }

        private bool ReconstructFormattingElements()
        {
            Entry<T> entry = F.Last;
            if (entry == null || entry.IsMark || entry.Element.InStack)
                return false;
        LABEL4:
            if (entry.Prev == null) goto LABEL8;
            entry = entry.Prev;
            if (!entry.IsMark && !entry.Element.InStack) goto LABEL4;
        LABEL7:
            entry = entry.Next;
        LABEL8:
            Element<T> element = entry.Element.Namespace == NS_HTML ? InsertHtmlElement(ref entry.Element.Token)
                : InsertForeignElement(ref entry.Element.Token, entry.Element.Namespace);
            F.Replace(entry, element);
            if (F.Last != entry) goto LABEL7;
            return true;

        }
        
        internal bool MathMLIntergartionPoint()
        {
            System.Diagnostics.Trace.WriteLine("MathMLIntergartionPoint");
            if (O.Current.Namespace != NS_MathML)
                return false;
            switch (O.Current.TagName)
            {
                case "mi":
                case "mo":
                case "mn":
                case "ms":
                case "mtext":
                    return true;
                default:
                    return false;
            }
        }

        internal bool HTMLIntegrationPoint()
        {
            System.Diagnostics.Trace.WriteLine("HTMLIntegrationPoint");
            switch (O.Current.TagName)
            {
                case "annotation-xml":
                    if (O.Current.Namespace != NS_MathML ||
                        O.Current.Token.Attributes == null)
                        return false;
                    for(AttributeEntry attr = O.Current.Token.Attributes; attr != null; attr = attr.NextAttribute)
                        if(attr.Name == "encoding")
                        {
                            switch(attr.Value.ToLower())
                            {
                                case "text/html":
                                case "application/xhtml+xml":
                                    return true;
                                default:
                                    return false;
                            }
                        }
                    return false;
                case "foreignObject":
                case "desc":
                case "title":
                    return O.Current.Namespace == NS_SVG;
                default:
                    return false;
            }
        }

        private Element<T> InsertHtmlElement(ref ElementToken token)
        {
            T instance = CreateElement(token.TagName, token.Attributes, NS_HTML);
            Element<T> element = new Element<T>(instance, token, NS_HTML);
            if (formPointer != null && IsFormAssociatedElement(formPointer))
            {
                bool contains = false;
                for (AttributeEntry attr = token.Attributes; attr != null; attr = attr.NextAttribute)
                    if (attr.Name == "form")
                    {
                        contains = true;
                        break;
                    }
                if (!contains)
                    SetFormOwner(element.Value, formPointer.Value);
            }

            if (FOSTER_PARENTED && O.Current.Namespace == NS_HTML)
            {
                switch (O.Current.TagName)
                {
                    case "table":
                    case "tbody":
                    case "tfoot":
                    case "thead":
                    case "tr":
                        Element<T> table = O.GetLastTable();
                        if (table == null)
                        {
                            AppendChild(O.Oldest.Value, element.Value);
                        }
                        else
                        {
                            if (GetElementParent(table.Value) != null)
                                InsertBefore(element.Value, table.Value);
                            else
                                AppendChild(table.Prev.Value, element.Value);
                        }
                        break;
                    default:
                        AppendChild(O.Current.Value, element.Value);
                        break;
                }
            }
            else
            {
                AppendChild(O.Current.Value, element.Value);
            }
            return O.Push(element);
        }

        private Element<T> InsertForeignElement(ref ElementToken token, string namespaceURI)
        {
            T instance = CreateElement(token.TagName, token.Attributes, namespaceURI);
            Element<T> element = new Element<T>(instance, token, namespaceURI);

            if (FOSTER_PARENTED && O.Current.Namespace == NS_HTML)
            {
                switch (O.Current.TagName)
                {
                    case "table":
                    case "tbody":
                    case "tfoot":
                    case "thead":
                    case "tr":
                        Element<T> table = O.GetLastTable();
                        if (table == null)
                        {
                            AppendChild(O.Oldest.Value, element.Value);
                        }
                        else
                        {
                            if (GetElementParent(table.Value) != null)
                                InsertBefore(element.Value, table.Value);
                            else
                                AppendChild(table.Prev.Value, element.Value);
                        }
                        break;
                    default:
                        AppendChild(O.Current.Value, element.Value);
                        break;
                }
            }
            else
            {
                AppendChild(O.Current.Value, element.Value);
            }
            return O.Push(element);
        }

        private void InsertCharacters(string token)
        {
            if (FOSTER_PARENTED && O.Current.Namespace == NS_HTML)
            {
                switch (O.Current.TagName)
                {
                    case "table":
                    case "tbody":
                    case "tfoot":
                    case "thead":
                    case "tr":
                        Element<T> table = O.GetLastTable();
                        if (table == null)
                        {
                            AppendChild(O.Oldest.Value, token);
                        }
                        else
                        {
                            if (GetElementParent(table.Value) != null)
                                InsertBefore(token, table.Value);
                            else
                                AppendChild(table.Prev.Value, token);
                        }
                        break;
                    default:
                        AppendChild(O.Current.Value, token);
                        break;
                }
            }
            else
            {
                AppendChild(O.Current.Value, token);
            }
        }

        private void AdjustMathMLAttributes(AttributeEntry attribute)
        {
            while (attribute != null)
            {
                if (attribute.Name == "definitionurl")
                {
                    attribute.Name = "definitionURL";
                    return;
                }
                attribute = attribute.NextAttribute;
            }
        }

        private void AdjustSVGAttributes(AttributeEntry attribute)
        {
            while (attribute != null)
            {
                switch (attribute.Name)
                {
                    case "attributename":
                        attribute.Name = "attributeName";
                        break;
                    case "attributetype":
                        attribute.Name = "attributeType";
                        break;
                    case "basefrequency":
                        attribute.Name = "baseFrequency";
                        break;
                    case "baseprofile":
                        attribute.Name = "baseProfile";
                        break;
                    case "calcmode":
                        attribute.Name = "calcMode";
                        break;
                    case "clippathunits":
                        attribute.Name = "clipPathUnits";
                        break;
                    case "contentscripttype":
                        attribute.Name = "contentScriptType";
                        break;
                    case "contentstyletype":
                        attribute.Name = "contentStyleType";
                        break;
                    case "diffuseconstant":
                        attribute.Name = "diffuseConstant";
                        break;
                    case "edgemode":
                        attribute.Name = "edgeMode";
                        break;
                    case "externalresourcesrequired":
                        attribute.Name = "externalResourcesRequired";
                        break;
                    case "filterres":
                        attribute.Name = "filterRes";
                        break;
                    case "filterunits":
                        attribute.Name = "filterUnits";
                        break;
                    case "glyphref":
                        attribute.Name = "glyphRef";
                        break;
                    case "gradienttransform":
                        attribute.Name = "gradientTransform";
                        break;
                    case "gradientunits":
                        attribute.Name = "gradientUnits";
                        break;
                    case "kernelmatrix":
                        attribute.Name = "kernelMatrix";
                        break;
                    case "kernelunitlength":
                        attribute.Name = "kernelUnitLength";
                        break;
                    case "keypoints":
                        attribute.Name = "keyPoints";
                        break;
                    case "keysplines":
                        attribute.Name = "keySplines";
                        break;
                    case "keytimes":
                        attribute.Name = "keyTimes";
                        break;
                    case "lengthadjust":
                        attribute.Name = "lengthAdjust";
                        break;
                    case "limitingconeangle":
                        attribute.Name = "limitingConeAngle";
                        break;
                    case "markerheight":
                        attribute.Name = "markerHeight";
                        break;
                    case "markerunits":
                        attribute.Name = "markerUnits";
                        break;
                    case "markerwidth":
                        attribute.Name = "markerWidth";
                        break;
                    case "maskcontentunits":
                        attribute.Name = "maskContentUnits";
                        break;
                    case "maskunits":
                        attribute.Name = "maskUnits";
                        break;
                    case "numoctaves":
                        attribute.Name = "numOctaves";
                        break;
                    case "pathlength":
                        attribute.Name = "pathLength";
                        break;
                    case "patterncontentunits":
                        attribute.Name = "patternContentUnits";
                        break;
                    case "patterntransform":
                        attribute.Name = "patternTransform";
                        break;
                    case "patternunits":
                        attribute.Name = "patternUnits";
                        break;
                    case "pointsatx":
                        attribute.Name = "pointsAtX";
                        break;
                    case "pointsaty":
                        attribute.Name = "pointsAtY";
                        break;
                    case "pointsatz":
                        attribute.Name = "pointsAtZ";
                        break;
                    case "preservealpha":
                        attribute.Name = "preserveAlpha";
                        break;
                    case "preserveaspectratio":
                        attribute.Name = "preserveAspectRatio";
                        break;
                    case "primitiveunits":
                        attribute.Name = "primitiveUnits";
                        break;
                    case "refx":
                        attribute.Name = "refX";
                        break;
                    case "refy":
                        attribute.Name = "refY";
                        break;
                    case "repeatcount":
                        attribute.Name = "repeatCount";
                        break;
                    case "repeatdur":
                        attribute.Name = "repeatDur";
                        break;
                    case "requiredextensions":
                        attribute.Name = "requiredExtensions";
                        break;
                    case "requiredfeatures":
                        attribute.Name = "requiredFeatures";
                        break;
                    case "specularconstant":
                        attribute.Name = "specularConstant";
                        break;
                    case "specularexponent":
                        attribute.Name = "specularExponent";
                        break;
                    case "spreadmethod":
                        attribute.Name = "spreadMethod";
                        break;
                    case "startoffset":
                        attribute.Name = "startOffset";
                        break;
                    case "stddeviation":
                        attribute.Name = "stdDeviation";
                        break;
                    case "stitchtiles":
                        attribute.Name = "stitchTiles";
                        break;
                    case "surfacescale":
                        attribute.Name = "surfaceScale";
                        break;
                    case "systemlanguage":
                        attribute.Name = "systemLanguage";
                        break;
                    case "tablevalues":
                        attribute.Name = "tableValues";
                        break;
                    case "targetx":
                        attribute.Name = "targetX";
                        break;
                    case "targety":
                        attribute.Name = "targetY";
                        break;
                    case "textlength":
                        attribute.Name = "textLength";
                        break;
                    case "viewbox":
                        attribute.Name = "viewBox";
                        break;
                    case "viewtarget":
                        attribute.Name = "viewTarget";
                        break;
                    case "xchannelselector":
                        attribute.Name = "xChannelSelector";
                        break;
                    case "ychannelselector":
                        attribute.Name = "yChannelSelector";
                        break;
                    case "zoomandpan":
                        attribute.Name = "zoomAndPan";
                        break;
                }
                attribute = attribute.NextAttribute;
            }
        }

        private void AdjustForeignAttributes(AttributeEntry attribute)
        {
            while (attribute != null)
            {
                switch (attribute.Name)
                {
                    case "xlink:actuate":
                    case "xlink:arcrole":
                    case "xlink:href":
                    case "xlink:role":
                    case "xlink:show":
                    case "xlink:title":
                    case "xlink:type":
                        attribute.Namespace = NS_XLink;
                        break;
                    case "xml:base":
                    case "xml:lang":
                    case "xml:space":
                        attribute.Namespace = NS_XML;
                        break;
                    case "xmlns":
                    case "xmlns:xlink":
                        attribute.Namespace = NS_XMLNS;
                        break;
                }
                attribute = attribute.NextAttribute;
            }
        }

        private string AdjustSVGTagName(string tagName)
        {
            switch (tagName)
            {
                case "altglyph":
                    return "altGlyph";
                case "altglyphdef":
                    return "altGlyphDef";
                case "altglyphitem":
                    return "altGlyphItem";
                case "animatecolor":
                    return "animateColor";
                case "animatemotion":
                    return "animateMotion";
                case "animatetransform":
                    return "animateTransform";
                case "clippath":
                    return "clipPath";
                case "feblend":
                    return "feBlend";
                case "fecolormatrix":
                    return "feColorMatrix";
                case "fecomponenttransfer":
                    return "feComponentTransfer";
                case "fecomposite":
                    return "feComposite";
                case "feconvolvematrix":
                    return "feConvolveMatrix";
                case "fediffuselighting":
                    return "feDiffuseLighting";
                case "fedisplacementmap":
                    return "feDisplacementMap";
                case "fedistantlight":
                    return "feDistantLight";
                case "feflood":
                    return "feFlood";
                case "fefunca":
                    return "feFuncA";
                case "fefuncb":
                    return "feFuncB";
                case "fefuncg":
                    return "feFuncG";
                case "fefuncr":
                    return "feFuncR";
                case "fegaussianblur":
                    return "feGaussianBlur";
                case "feimage":
                    return "feImage";
                case "femerge":
                    return "feMerge";
                case "femergenode":
                    return "feMergeNode";
                case "femorphology":
                    return "feMorphology";
                case "feoffset":
                    return "feOffset";
                case "fepointlight":
                    return "fePointLight";
                case "fespecularlighting":
                    return "feSpecularLighting";
                case "fespotlight":
                    return "feSpotLight";
                case "fetile":
                    return "feTile";
                case "feturbulence":
                    return "feTurbulence";
                case "foreignobject":
                    return "foreignObject";
                case "glyphref":
                    return "glyphRef";
                case "lineargradient":
                    return "linearGradient";
                case "radialgradient":
                    return "radialGradient";
                case "textpath":
                    return "textPath";
            }
            return tagName;
        }

        private void ClearStackBackToTableContext()
        {
            while (true)
            {
                switch (O.Current.TagName)
                {
                    case "table":
                    case "html":
                        return;
                    default:
                        Element<T> element = O.Current;
                        O.Current = O.Current.Prev;
                        O.Current.Next = null;
                        element.Prev = null;
                        element.InStack = false;
                        continue;
                }
            }
        }

        private void ClearStackBackToTableBodyContext()
        {
            while (true)
            {
                switch (O.Current.TagName)
                {
                    case "tbody":
                    case "tfoot":
                    case "thead":
                    case "html":
                        return;
                    default:
                        Element<T> element = O.Current;
                        O.Current = O.Current.Prev;
                        O.Current.Next = null;
                        element.Prev = null;
                        element.InStack = false;
                        continue;
                }
            }
        }

        private void ClearStackBackToTableRowContext()
        {
            while (true)
            {
                switch (O.Current.TagName)
                {
                    case "tr":
                    case "html":
                        return;
                    default:
                        Element<T> element = O.Current;
                        O.Current = O.Current.Prev;
                        O.Current.Next = null;
                        element.Prev = null;
                        element.InStack = false;
                        continue;
                }
            }
        }

        private void CloseTheCell(byte mode)
        {
            if (ElementInTableScope("td"))
                EndTagToken("td", mode);
            else
                EndTagToken("th", mode);
        }

        private void PopInForeign()
        {
            bool done = false;
            while (!done)
            {
                if (O.Current.Namespace == NS_HTML || HTMLIntegrationPoint() || MathMLIntergartionPoint())
                    done = true;
                Element<T> element = O.Current;
                O.Current = O.Current.Prev;
                O.Current.Next = null;
                element.Prev = null;
                element.InStack = false;
            }
        }

        private void StopParsing()
        {
            //O.Clear();
            //F.Clear();
            //tokenizer.RUN = false;
            if (IS_FRAGMENT)
                DocumentFragmentParsed();
            else
                DocumentParsed();
        }

        #endregion

        #region START TAG TOKEN
        internal byte StartTagToken(ref ElementToken token, byte mode)
        {

            EVAL:
            if (mode == REPROCESS)
                mode = INSERTION_MODE;

            switch (mode)
            {
                case INITIAL:
                    DocMode = DocumentQuirkFlag.QuirksMode;
                    INSERTION_MODE = BEFORE_HTML;
                    mode = REPROCESS; goto EVAL;
                case BEFORE_HTML:
                    switch (token.TagName)
                    {
                        case "html":
                            {
                                T element = CreateElement(token.TagName, token.Attributes, NS_HTML);
                                AppendElementToDocument(element);
                                O.Push(new Element<T>(element, token, NS_HTML));
                                //HandleHtmlElement(element.Value);
                                INSERTION_MODE = BEFORE_HEAD;
                            }
                            return 1;
                        default:
                            {
                                T element = CreateElement("html", null, NS_HTML);
                                AppendElementToDocument(element);
                                O.Push(new Element<T>(element, "html", NS_HTML));
                                //HandleHtmlElement(element.Value);
                                INSERTION_MODE = BEFORE_HEAD;
                                mode = REPROCESS; goto EVAL;
                            }
                    }
                   
                case BEFORE_HEAD:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        case "head":
                            headPointer = InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_HEAD;
                            return 1;
                        default:
                            {
								//TODO:Update
                                ElementToken fakeToken = new ElementToken("head");
                                StartTagToken(ref fakeToken, mode);
                                mode = REPROCESS; goto EVAL;
                            }
                    }
                case IN_HEAD:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        case "base":
                        case "basefont":
                        case "bgsound":
                        case "command": //What is this?
                        case "link":
                            InsertHtmlElement(ref token);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            return 1;
                        case "meta":
                            InsertHtmlElement(ref token);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            //HandleMetaElement ??
                            return 1;
                        case "title":
                            GenericRCDataAlgorithm(ref token);
                            return 1;
                        case "noframes":
                        case "style":
                            GenericRawTextAlgorithm(ref token);
                            return 1;
                        case "noscript":
                            if (SCRIPTING_ENABLED)
                            {
                                GenericRawTextAlgorithm(ref token);
                            }
                            else
                            {
                                InsertHtmlElement(ref token);
                                INSERTION_MODE = IN_HEAD_NOSCRIPT;
                            }
                            return 1;
                        case "script":
							{	
								//TODO: Update the algorithm
                                T script = CreateElement(token.TagName, token.Attributes, NS_HTML);
                                SetScriptFlag(script, ScriptElementFlag.ParserInserted, true);
                                SetScriptFlag(script, ScriptElementFlag.ForceAsync, false);
                                if (IS_FRAGMENT)
                                    SetScriptFlag(script, ScriptElementFlag.AlreadyStarted, true);
                                AppendChild(O.Current.Value, script);
                                O.Push(new Element<T>(script, token, NS_HTML));
                                tokenizer.STATE = Tokenizer<T>.SCRIPT_DATA_STATE;
                                ORIGINAL_INSERTION_MODE = INSERTION_MODE;
                                INSERTION_MODE = TEXT;
                            }
                            return 1;
						//TODO: Add support for the <template> element
                        case "head":
                            return 0;//ignored
                        default:
							//TODO: Must be updated
                            EndTagToken("head", mode);
                            mode = REPROCESS; goto EVAL;
                           //StartTagToken(tagName, attributes, selfClosing, REPROCESS);
                            //break;
                    }

                case IN_HEAD_NOSCRIPT:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                            //StartTagToken(tagName, attributes, selfClosing, IN_BODY);
                            //break;
                        case "basefont":
                        case "bgsound":
                        case "link":
                        case "meta":
                        case "noframes":
                        case "style":
                            mode = IN_HEAD; goto EVAL;
                            //StartTagToken(tagName, attributes, selfClosing, IN_HEAD);
                            //break;
                        case "head":
                        case "noscript":
                            return 0;//ignore token
                        default:
							//TODO: Must be updated
                            EndTagToken("noscript", mode);
                            mode = REPROCESS; goto EVAL;
                            //StartTagToken(tagName, attributes, selfClosing, REPROCESS);
                            //break;
                    }
                    //break;
                case AFTER_HEAD:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                            //StartTagToken(tagName, attributes, selfClosing, IN_BODY);
                            //break;
                        case "body":
                            InsertHtmlElement(ref token);
                            FRAMESETOK = false;
                            INSERTION_MODE = IN_BODY;
                            return 1;
                        case "frameset":
                            InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_FRAMESET;
                            return 1;
                        case "base":
                        case "basefont":
                        case "bgsound":
                        case "link":
                        case "meta":
                        case "noframes":
                        case "script":
                        case "style":
                        case "title":
                            {
                                O.Push(headPointer);
                                StartTagToken(ref token, IN_HEAD);
                                O.Remove(headPointer);
                            }
                            return 1;
                        case "head":
                            return 0; //ignore token
                        default:
                            {
                                ElementToken fakeToken = new ElementToken("body");
                                StartTagToken(ref fakeToken, mode);
                                FRAMESETOK = true;
                                mode = REPROCESS; goto EVAL;
                            }
                    }
                    
                case IN_BODY:
                    switch (token.TagName)
                    {
                        case "html":
                            if (token.Attributes != null)
                                AddAttributes(O.Oldest.Value, token.Attributes);
                            return 1;
                        case "base":
                        case "basefont":
                        case "bgsound":
                        case "command":
                        case "link":
                        case "meta":
                        case "noframes":
                        case "script":
                        case "style":
                        case "title":
                            mode = IN_HEAD; goto EVAL;
                        case "body":
                            if (IS_FRAGMENT)
                            {
                                if (O.Oldest == null || O.Oldest.Next == null)
                                    return 0;
                                if (O.Oldest.Next.TagName != "body" || O.Oldest.Next.Namespace != NS_HTML)
                                    return 0;
                            }
                            FRAMESETOK = false;
                            if (token.Attributes != null)
                                AddAttributes(O.Oldest.Value, token.Attributes);
                            return 1;
                        case "frameset":
                            if (IS_FRAGMENT)
                            {
                                if (O.Oldest == null || O.Oldest.Next == null)
                                    return 0;
                                if (O.Oldest.Next.TagName != "body" || O.Oldest.Next.Namespace != NS_HTML)
                                    return 0;
                            }
                            if (!FRAMESETOK)
                                return 0; //ignore token
                            RemoveFromParent(O.Oldest.Next.Value);
                            O.RemoveTo(O.Oldest, false);
                            InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_FRAMESET;
                            return 1;
                        case "address":
                        case "article":
                        case "aside":
                        case "blockquote":
                        case "center":
                        case "details":
                        case "dir":
                        case "div":
                        case "dl":
                        case "fieldset":
                        case "figcaption":
                        case "figure":
                        case "footer":
                        case "header":
                        case "hgroup":
                        case "menu":
                        case "nav":
                        case "ol":
                        case "p":
                        case "section":
                        case "summary":
                        case "ul":
                            if (ElementInButtonScope("p"))
                                EndTagToken("p", mode);
                            InsertHtmlElement(ref token);
                            return 1;
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                            {
                                if (ElementInButtonScope("p"))
                                    EndTagToken("p", mode);
                                switch (O.Current.TagName)
                                {
                                    case "h1":
                                    case "h2":
                                    case "h3":
                                    case "h4":
                                    case "h5":
                                    case "h6":
                                        O.Pop();
                                        break;
                                }
                                InsertHtmlElement(ref token);
                            }
                            return 1;
                        case "pre":
                        case "listing":
                            if (ElementInButtonScope("p"))
                                EndTagToken("p", mode);
                            InsertHtmlElement(ref token);
                            IGNORE_LF_TOKEN = true;
                            FRAMESETOK = false;
                            return 1;
                        case "form":
                            if (formPointer != null)
                                return 0; //ignore token
                            if (ElementInButtonScope("p"))
                                EndTagToken("p", mode);
                            formPointer = InsertHtmlElement(ref token);
                            return 1;
                        case "li":
                            {
                                FRAMESETOK = false;
                                bool done = false;
                                for (Element<T> element = O.Current; element != null && !done; element = element.Prev)
                                {
                                    switch (element.TagName)
                                    {
                                        case "li":
                                            EndTagToken("li", mode);
                                            done = true;
                                            continue;
                                        case "address":
                                        case "div":
                                        case "p":
                                            continue;
                                        default:
                                            done = IsSpecialElement(element);
                                            continue;   
                                    }
                                }
                                if (ElementInButtonScope("p"))
                                    EndTagToken("p", mode);
                                InsertHtmlElement(ref token);
                            } 
                            return 1;
                        case "dd":
                        case "dt":
                            {
                                FRAMESETOK = false;
                                bool done = false;
                                for (Element<T> element = O.Current; element != null && !done; element = element.Prev)
                                {
                                    switch (element.TagName)
                                    {
                                        case "dd":
                                        case "dt":
                                            EndTagToken(element.TagName, mode);
                                            done = true;
                                            continue;
                                        case "address":
                                        case "div":
                                        case "p":
                                            continue;
                                        default:
                                            done = IsSpecialElement(element);
                                            continue;
                                    }
                                }
                                if (ElementInButtonScope("p"))
                                    EndTagToken("p", mode);
                                InsertHtmlElement(ref token);
                            }
                            return 1;
                        case "plaintext":
                            if (ElementInButtonScope("p"))
                                EndTagToken("p", mode);
                            InsertHtmlElement(ref token);
                            tokenizer.STATE = Tokenizer<T>.PLAINTEXT_STATE;
                            return 1;
                        case "button":
                            if (ElementInScope("button"))
                            {
                                EndTagToken("button", mode);
                                mode = REPROCESS; goto EVAL;
                            }
                            else
                            {
                                ReconstructFormattingElements();
                                InsertHtmlElement(ref token);
                                FRAMESETOK = false;
                            }
                            return 1;
                        case "a":
                            {
                                Element<T> element = null;
                                for(Entry<T> entry = F.Last; entry != null && !entry.IsMark; entry = entry.Prev)
                                    if (entry.Element.TagName == "a")
                                    {
                                        element = entry.Element;
                                        break;
                                    }
                                if (element != null)
                                {
                                    EndTagToken("a", mode);
                                    if (element.InList)
                                        F.Remove(element.Entry);
                                    if (element.InStack)
                                        O.Remove(element);
                                }
                                ReconstructFormattingElements();
                                element = InsertHtmlElement(ref token);
                                PushFormattingElement(element);
                            }
                            return 1;
                        case "b":
                        case "big":
                        case "code":
                        case "em":
                        case "font":
                        case "i":
                        case "s":
                        case "small":
                        case "strike":
                        case "strong":
                        case "tt":
                        case "u":
                            {
                                ReconstructFormattingElements();
                                Element<T> element = InsertHtmlElement(ref token);
                                PushFormattingElement(element);
                            }
                            return 1;
                        case "nobr":
                            {
                                ReconstructFormattingElements();
                                if (ElementInScope("nobr"))
                                {
                                    EndTagToken("nobr", mode);
                                    ReconstructFormattingElements();
                                }
                                Element<T> element = InsertHtmlElement(ref token);
                                PushFormattingElement(element);
                            }
                            return 1;
                        case "applet":
                        case "marquee":
                        case "object":
                            ReconstructFormattingElements();
                            InsertHtmlElement(ref token);
                            F.Mark();
                            FRAMESETOK = false;
                            return 1;
                        case "table":
                            if (DocMode != DocumentQuirkFlag.QuirksMode && ElementInButtonScope("p"))
                                EndTagToken("p", mode);
                            InsertHtmlElement(ref token);
                            FRAMESETOK = false;
                            INSERTION_MODE = IN_TABLE;
                            return 1;
                        case "area":
                        case "br":
                        case "embed":
                        case "img":
                        case "keygen":
                        case "wbr":
                            ReconstructFormattingElements();
                            InsertHtmlElement(ref token);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            FRAMESETOK = false;
                            return 1;
                        case "input":
                            {
                                ReconstructFormattingElements();
                                InsertHtmlElement(ref token);
                                O.Pop();
                                if (token.SelfClosing)
                                    token.Acknowledged = true;
                                bool contains = false;
                                for (AttributeEntry attr = token.Attributes; attr != null; attr = attr.NextAttribute)
                                    if (attr.Name == "type")
                                    {
                                        contains = attr.Value.ToLower() == "hidden";
                                        break;
                                    }
                                if (!contains)
                                    FRAMESETOK = false;
                            }
                            return 1;
                        case "param":
                        case "source":
                        case "track":
                            InsertHtmlElement(ref token);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            return 1;
                        case "hr":
                            if (ElementInButtonScope("p"))
                                EndTagToken("p", mode);
                            InsertHtmlElement(ref token);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            FRAMESETOK = false;
                            return 1;
                        case "image":
                            {
                                ElementToken fakeToken = new ElementToken("img", token.Attributes, token.AttrCount, token.SelfClosing);
								return StartTagToken(ref fakeToken, mode);
                            }
                        case "isindex":
                            {
                                if (formPointer != null)
                                    return 0; //ignore token
                                if (token.SelfClosing)
                                    token.Acknowledged = true;
                                ElementToken fakeToken = new ElementToken("form");
                                StartTagToken(ref fakeToken, mode);
                                AttributeEntry extraAttr = new AttributeEntry("name", "isindex");
                                int extraAttrCount = 1;
                                char[] charStream = "This is a searchable index. Enter search keywords:".ToCharArray();
                                if (token.Attributes != null)
                                {
                                    for (AttributeEntry attr = token.Attributes; attr != null; attr = attr.NextAttribute)
                                        switch(attr.Name)
                                        {
                                            case "action":
                                                SetAttribute(formPointer.Value, "action", attr.Value);
                                                continue;
                                            case "prompt":
                                                charStream = attr.Value.ToCharArray();
                                                continue;
                                            case "isindex":
                                            case "name":
                                                continue;
                                            default:
                                                extraAttr.NextAttribute = new AttributeEntry(attr.Name, attr.Value, attr.Namespace);
                                                extraAttrCount++;
                                                extraAttr = extraAttr.NextAttribute;
                                                continue;
                                        }
                                }
                                while(extraAttr.PrevAttribute != null)
                                    extraAttr = extraAttr.PrevAttribute;
                                fakeToken = new ElementToken("hr");
                                StartTagToken(ref fakeToken, mode);
                                fakeToken = new ElementToken("label");
                                StartTagToken(ref fakeToken, mode);
                                CharToken(ref charStream, 0, charStream.Length, mode);
                                fakeToken = new ElementToken("input", extraAttr, extraAttrCount, false);
                                StartTagToken(ref fakeToken, mode);
                                CharToken(ref charStream, 0, 0, mode);
                                EndTagToken("label", mode);
                                fakeToken = new ElementToken("hr");
                                StartTagToken(ref fakeToken, mode);
                                EndTagToken("form", mode);
                            }
                            return 1;
                        case "textarea":
                            InsertHtmlElement(ref token);
                            IGNORE_LF_TOKEN = true;
                            tokenizer.STATE = Tokenizer<T>.RCDATA_STATE;
                            ORIGINAL_INSERTION_MODE = INSERTION_MODE;
                            FRAMESETOK = false;
                            INSERTION_MODE = TEXT;
                            return 1;
                        case "xmp":
                            if (ElementInButtonScope("p"))
                                EndTagToken("p", mode);
                            ReconstructFormattingElements();
                            FRAMESETOK = false;
                            GenericRawTextAlgorithm(ref token);
                            return 1;
                        case "iframe":
                            FRAMESETOK = false;
                            GenericRawTextAlgorithm(ref token);
                            return 1;
                        case "noembed":
                            GenericRawTextAlgorithm(ref token);
                            break;
                        case "noscript":
                            if (SCRIPTING_ENABLED)
                            {
                                GenericRawTextAlgorithm(ref token);
                                return 1;
                            }
                            else
                            {
                                goto default;
                            }
                        case "select":
                            ReconstructFormattingElements();
                            InsertHtmlElement(ref token);
                            FRAMESETOK = false;
                            switch (INSERTION_MODE)
                            {
                                case IN_TABLE:
                                case IN_CAPTION:
                                case IN_TABLE_BODY:
                                case IN_ROW:
                                case IN_CELL:
                                    INSERTION_MODE = IN_SELECT_IN_TABLE;
                                    break;
                                default:
                                    INSERTION_MODE = IN_SELECT;
                                    break;
                            }
                            return 1;
                        case "optgroup":
                        case "option":
                            if (O.Current.TagName == "option")
                                EndTagToken("option", mode);
                            ReconstructFormattingElements();
                            InsertHtmlElement(ref token);
                            return 1;
                        case "rp":
                        case "rt":
                            if (ElementInScope("ruby"))
                                O.GenerateImpliedEndTags(null);
                            InsertHtmlElement(ref token);
                            return 1;
                        case "math":
                            ReconstructFormattingElements();
                            AdjustMathMLAttributes(token.Attributes);
                            AdjustForeignAttributes(token.Attributes);
                            InsertForeignElement(ref token, NS_MathML);
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            return 1;
                        case "svg":
                            ReconstructFormattingElements();
                            AdjustSVGAttributes(token.Attributes);
                            AdjustForeignAttributes(token.Attributes);
                            InsertForeignElement(ref token, NS_SVG);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            return 1;
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "frame":
                        case "head":
                        case "tbody":
                        case "td":
                        case "tfoot":
                        case "th":
                        case "thead":
                        case "tr":
                            return 0; //ignore token
                        default:
                            ReconstructFormattingElements();
                            InsertHtmlElement(ref token);
                            return 1;
                    }
                    break;
                case TEXT:
                    return 1;
                case IN_TABLE:
                    switch (token.TagName)
                    {
                        case "caption":
                            ClearStackBackToTableContext();
                            F.Mark();
                            InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_CAPTION;
                            return 1;
                        case "colgroup":
                            ClearStackBackToTableContext();
                            InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_COLUMN_GROUP;
                            return 1;
                        case "col":
                            {
                                ElementToken fakeToken = new ElementToken("colgroup");
                                StartTagToken(ref fakeToken, mode);
                                mode = REPROCESS; goto EVAL;
                            }

                        case "tbody":
                        case "tfoot":
                        case "thead":
                            ClearStackBackToTableContext();
                            InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_TABLE_BODY;
                            return 1;
                        case "th":
                        case "td":
                        case "tr":
                            {
                                ElementToken fakeToken = new ElementToken("tbody");
                                StartTagToken(ref fakeToken, mode);
                                mode = REPROCESS; goto EVAL;
                            }
                        case "table":
                            if (EndTagToken("table", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                        case "style":
                        case "script":
                            mode = IN_HEAD; goto EVAL;
                        case "input":
                            {
                                bool contains = false;
                                for (AttributeEntry attr = token.Attributes; attr != null; attr = attr.NextAttribute)
                                    if (attr.Name == "type")
                                    {
                                        contains = attr.Value.ToLower() == "hidden";
                                        break;
                                    }
                                if (!contains)
                                    goto default;
                                InsertHtmlElement(ref token);
                                O.Pop();
                            }
                            return 1;
                        case "form":
                            if (formPointer != null)
                                return 0;//ignore token
                            formPointer = InsertHtmlElement(ref token);
                            O.Pop();
                            return 1;
                        default:
                            FOSTER_PARENTED = true;
                            StartTagToken(ref token, IN_BODY);
                            FOSTER_PARENTED = false;
                            return 1;
                    }
                    
                case IN_TABLE_TEXT:
                    {
                        if (ContainsNonWhiteSpace(ref pendingTokenList, 0, pendingTokenListPtr))
                        {
                            FOSTER_PARENTED = true;
                            CharToken(ref pendingTokenList, 0, pendingTokenListPtr, IN_BODY);
                            FOSTER_PARENTED = false;
                        }
                        else
                        {
                            InsertCharacters(new string(pendingTokenList, 0, pendingTokenListPtr));
                        }
                        INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                        mode = REPROCESS; goto EVAL;
                    }
                case IN_CAPTION:
                    switch (token.TagName)
                    {
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "tbody":
                        case "td":
                        case "tfoot":
                        case "th":
                        case "thead":
                        case "tr":
                            if (EndTagToken("caption", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                        default:
                            mode = IN_BODY; goto EVAL;
                    }
                case IN_COLUMN_GROUP:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        case "col":
                            InsertHtmlElement(ref token);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            else
                                O.Pop();
                            return 1;
                        default:
                            if (EndTagToken("colgroup", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                    }

                case IN_TABLE_BODY:
                    switch (token.TagName)
                    {
                        case "tr":
                            ClearStackBackToTableBodyContext();
                            InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_ROW;
                            return 1;
                        case "th":
                        case "td":
                            {
                                ElementToken fakeToken = new ElementToken("tr");
                                StartTagToken(ref fakeToken, mode);
                                mode = REPROCESS; goto EVAL;
                            }
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "tbody":
                        case "tfoot":
                        case "thead":
                            if (!ElementInTableScope("tbody") && !ElementInTableScope("thead")
                                && !ElementInTableScope("tfoot"))
                                return 0; //ignore token
                            ClearStackBackToTableBodyContext();
                            EndTagToken(O.Current.TagName, mode);
                            mode = REPROCESS; goto EVAL;
                        default:
                            mode = IN_TABLE; goto EVAL;
                    }
                    
                case IN_ROW:
                    switch (token.TagName)
                    {
                        case "th":
                        case "td":
                            ClearStackBackToTableRowContext();
                            InsertHtmlElement(ref token);
                            INSERTION_MODE = IN_CELL;
                            F.Mark();
                            return 1;
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "tbody":
                        case "tfoot":
                        case "thead":
                        case "tr":
                            if (EndTagToken("tr", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                        default:
                            mode = IN_TABLE; goto EVAL;
                    }
                    
                case IN_CELL:
                    switch (token.TagName)
                    {
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "tbody":
                        case "td":
                        case "tfoot":
                        case "th":
                        case "thead":
                        case "tr":
                            if (!ElementInTableScope("td") && !ElementInTableScope("th"))
                                return 0; //ignore token
                            CloseTheCell(mode);
                            mode = REPROCESS; goto EVAL;
                        default:
                            mode = IN_BODY; goto EVAL;
                    }
                case IN_SELECT:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        case "option":
                            if (O.Current.TagName == "option")
                                EndTagToken("option", mode);
                            InsertHtmlElement(ref token);
                            return 1;
                        case "optgroup":
                            {
                                if (O.Current.TagName == "option" || O.Current.TagName == "optgroup")
                                    EndTagToken(O.Current.TagName, mode);
                                InsertHtmlElement(ref token);
                            }
                            return 1;
                        case "select":
                            EndTagToken("select", mode);
                            return 1;
                        case "input":
                        case "keygen":
                        case "textarea":
                            if (!ElementInSelectScope("select"))
                                return 0; //ignore token
                            EndTagToken("select", mode);
                            mode = REPROCESS; goto EVAL;
                        case "script":
                            mode = IN_HEAD; goto EVAL;
                        default:
                            return 0; //ignore token  
                    }
                    
                case IN_SELECT_IN_TABLE:
                    switch (token.TagName)
                    {
                        case "caption":
                        case "table":
                        case "tbody":
                        case "tfoot":
                        case "thead":
                        case "tr":
                        case "td":
                        case "th":
                            EndTagToken("select", mode);
                            mode = REPROCESS; goto EVAL;
                        default:
                            mode = IN_SELECT; goto EVAL;
                    }
                    //break;
                case AFTER_BODY:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        default:
                            INSERTION_MODE = IN_BODY;
                            mode = REPROCESS; goto EVAL;
                    }
                    //break;
                case IN_FRAMESET:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        case "frameset":
                            InsertHtmlElement(ref token);
                            break;
                        case "frame":
                            InsertHtmlElement(ref token);
                            O.Pop();
                            if (token.SelfClosing)
                                token.Acknowledged = true;
                            else
                                O.Pop();
                            return 1;
                        case "noframes":
                            mode = IN_HEAD; goto EVAL;
                        default:
                            return 0;//ignore token
                    }
                    break;
                case AFTER_FRAMESET:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        case "noframes":
                            mode = IN_HEAD; goto EVAL;
                        default:
                            return 0; //ignore token
                    }
                case AFTER_AFTER_BODY:
                    if (token.TagName == "html")
                    {
                        mode = IN_BODY; goto EVAL;
                    }
                    else
                    {
                        INSERTION_MODE = IN_BODY;
                        mode = REPROCESS; goto EVAL;
                    }
                case AFTER_AFTER_FRAMESET:
                    switch (token.TagName)
                    {
                        case "html":
                            mode = IN_BODY; goto EVAL;
                        case "noframes":
                            mode = IN_HEAD; goto EVAL;
                        default:
                            return 0; //ignore token
                    }
                case IN_FOREIGN_CONTENT:
                    switch (token.TagName)
                    {
                        case "b":
                        case "big":
                        case "blockquote":
                        case "body":
                        case "br":
                        case "center":
                        case "code":
                        case "dd":
                        case "div":
                        case "dl":
                        case "dt":
                        case "em":
                        case "embed":
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                        case "head":
                        case "hr":
                        case "i":
                        case "img":
                        case "li":
                        case "listing":
                        case "menu":
                        case "meta":
                        case "nobr":
                        case "ol":
                        case "p":
                        case "pre":
                        case "ruby":
                        case "s":
                        case "small":
                        case "span":
                        case "strong":
                        case "strike":
                        case "sub":
                        case "sup":
                        case "table":
                        case "tt":
                        case "u":
                        case "ul":
                        case "var":
                            PopInForeign();
                            mode = REPROCESS; goto EVAL;
                        case "font":
                            {
                                bool foundAttribute = false;
                                if (token.Attributes != null)
                                    for (AttributeEntry attr = token.Attributes; attr != null && !foundAttribute; attr = attr.NextAttribute)
                                        switch (attr.Name)
                                        {
                                            case "color":
                                            case "face":
                                            case "size":
                                                foundAttribute = true;
                                                continue;
                                        }
                                if (foundAttribute)
                                {
                                    PopInForeign();
                                    mode = REPROCESS; goto EVAL;
                                }
                                goto default;
                            }
                        default:
                            if (O.Current.Namespace == NS_MathML)
                                AdjustMathMLAttributes(token.Attributes);
                            if (O.Current.Namespace == NS_SVG)
                            {
                                token.TagName = AdjustSVGTagName(token.TagName);
                                AdjustSVGAttributes(token.Attributes);
                            }
                            AdjustForeignAttributes(token.Attributes);
                            InsertForeignElement(ref token, O.Current.Namespace);
                            if (token.SelfClosing)
                            {
                                O.Pop();
                                token.Acknowledged = true;
                            }
                            return 1;
                    }
                    
            }

            return 1;
        }
        #endregion

        #region END TAG TOKEN
        internal byte EndTagToken(string tagName, byte mode)
        {
            EVAL:
            if (mode == REPROCESS)
                mode = INSERTION_MODE;

            switch (mode)
            {
                case INITIAL:
                    DocMode = DocumentQuirkFlag.QuirksMode;
                    INSERTION_MODE = BEFORE_HTML;
                    mode = REPROCESS; goto EVAL;

                case BEFORE_HTML:
                    switch (tagName)
                    {
                        case "head":
                        case "body":
                        case "html":
                        case "br":
                            {
                                T element = CreateElement("html", null, NS_HTML);
                                AppendElementToDocument(element);
                                O.Push(new Element<T>(element, "html", NS_HTML));
                                INSERTION_MODE = BEFORE_HEAD;
                                mode = REPROCESS; goto EVAL;
                            }
                        default:
                            return 0; //ignore token
                    }

                case BEFORE_HEAD:
                    switch (tagName)
                    {
                        case "head":
                        case "body":
                        case "html":
                        case "br":
                            {
                                ElementToken fakeToken = new ElementToken("head");
                                StartTagToken(ref fakeToken, mode);
                                mode = REPROCESS; goto EVAL;
                            }
                        default:
                            return 0; //ignore token
                    }

                case IN_HEAD:
                    switch (tagName)
                    {
                        case "head":
                            O.Pop();
                            INSERTION_MODE = AFTER_HEAD;
                            return 1;
                        case "body":
                        case "html":
                        case "br":
                            EndTagToken("head", mode);
                            mode = REPROCESS; goto EVAL;
                        default:
                            return 0; //ignore token
                    }
                    
                case IN_HEAD_NOSCRIPT:
                    switch (tagName)
                    {
                        case "noscript":
                            O.Pop();
                            INSERTION_MODE = IN_HEAD;
                            return 1;
                        case "br":
                            EndTagToken("noscript", mode);
                            mode = REPROCESS; goto EVAL;
                        default:
                            return 0; //ignore token
                    }
                    
                case AFTER_HEAD:
                    switch (tagName)
                    {
                        case "body":
                        case "html":
                        case "br":
                            {
                                ElementToken fakeToken = new ElementToken("body");
                                StartTagToken(ref fakeToken, mode);
                                FRAMESETOK = true;
                                mode = REPROCESS; goto EVAL;
                            }
                        default:
                            return 0; //ignore token
                    }

                case IN_BODY:
                    switch (tagName)
                    {
                        case "body":
                            if (!ElementInScope("body"))
                                return 0;//ignore token
                            INSERTION_MODE = AFTER_BODY;
                            return 1;
                        case "html":
                            if (EndTagToken("body", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                        case "address":
                        case "article":
                        case "aside":
                        case "blockquote":
                        case "button":
                        case "center":
                        case "details":
                        case "dir":
                        case "div":
                        case "dl":
                        case "fieldset":
                        case "figcaption":
                        case "figure":
                        case "footer":
                        case "header":
                        case "hgroup":
                        case "listing":
                        case "menu":
                        case "nav":
                        case "ol":
                        case "pre":
                        case "section":
                        case "summary":
                        case "ul":
                            if (!ElementInScope(tagName))
                                return 0; //ignore token
                            O.GenerateImpliedEndTags(null);
                            O.ClearToTag(tagName);
                            return 1;
                        case "form":
                            {
                                Element<T> node = formPointer;
                                formPointer = null;
                                if (node == null || ElementInScope(node.TagName))
                                    return 0; //ignore token
                                O.GenerateImpliedEndTags(null);
                                O.Remove(node);
                            }
                            return 1;
                        case "p":
                            if (!ElementInButtonScope(tagName))
                            {
                                ElementToken fakeToken = new ElementToken(tagName);
                                StartTagToken(ref fakeToken, mode);
                                mode = REPROCESS; goto EVAL;
                            }
                            else
                            {
                                O.GenerateImpliedEndTags(tagName);
                                O.ClearToTag(tagName);
                            }
                            return 1;
                        case "li":
                            if (!ElementInListItemScope(tagName))
                                return 0; //ignore token
                            O.GenerateImpliedEndTags(tagName);
                            O.ClearToTag(tagName);
                            return 1;
                        case "dd":
                        case "dt":
                            if (!ElementInScope(tagName))
                                return 0; //ignore token
                            O.GenerateImpliedEndTags(tagName);
                            O.ClearToTag(tagName);
                            return 1;
                        case "h1":
                        case "h2":
                        case "h3":
                        case "h4":
                        case "h5":
                        case "h6":
                            if (!HeadingInScope())
                                return 0; //ignore token
                            O.GenerateImpliedEndTags(null);
                            O.ClearToHeading();
                            return 1;
                        case "a":
                        case "b":
                        case "big":
                        case "code":
                        case "em":
                        case "font":
                        case "i":
                        case "nobr":
                        case "s":
                        case "small":
                        case "strike":
                        case "strong":
                        case "tt":
                        case "u"://Adoption agency
                            for (int outer_loop_counter = 0; outer_loop_counter < 8; outer_loop_counter++)
                            {
                                Element<T> formattingElement = null;
                                for (Entry<T> entry = F.Last; entry != null && !entry.IsMark; entry = entry.Prev)
                                    if (entry.Element.TagName == tagName)
                                    {
                                        formattingElement = entry.Element;
                                        break;
                                    }
                                if (formattingElement == null)
                                    goto default;
                                if (!formattingElement.InStack)
                                {
                                    F.Remove(formattingElement.Entry);
                                    return 1;
                                }
                                if (!ElementInScope(tagName))
                                    return 0;
                                Element<T> furthestBlock;
                                for (furthestBlock = formattingElement.Next; furthestBlock != null; furthestBlock = furthestBlock.Next)
                                    if (IsSpecialElement(furthestBlock))
                                        break;
                                if (furthestBlock == null)
                                {
                                    O.RemoveTo(formattingElement, true);
                                    F.Remove(formattingElement.Entry);
                                    return 1;
                                }
                                Element<T> commonAncestor = formattingElement.Prev;
                                Entry<T> bookmark = formattingElement.Entry.Prev;
                                Element<T> node = furthestBlock, lastNode = furthestBlock;
                                for (int inner_loop_counter = 0; inner_loop_counter < 3; inner_loop_counter++)
                                {
                                    node = node.Prev;
                                    if (!node.InList)
                                    {
                                        Element<T> tmp = node;
                                        node = node.Next;
                                        O.Remove(tmp);
                                        continue;
                                    }
                                    if (node == formattingElement)
                                        break;
                                    T instance = CreateElement(node.Token.TagName, node.Token.Attributes, node.Namespace);
                                    Element<T> newElement = new Element<T>(instance, node.Token, node.Namespace);
                                    F.Replace(node.Entry, newElement);
                                    O.Replace(node, newElement);
                                    node = newElement;
                                    if (lastNode == furthestBlock)
                                        bookmark = node.Entry.Next;
                                    RemoveFromParent(lastNode.Value);
                                    AppendChild(node.Value, lastNode.Value);
                                    lastNode = node;
                                }
                                RemoveFromParent(lastNode.Value);
                                switch (commonAncestor.TagName)
                                {
                                    case "table":
                                    case "tbody":
                                    case "tfoot":
                                    case "thead":
                                    case "tr":
                                        Element<T> table = O.GetLastTable();
                                        if (table == null)
                                        {
                                            AppendChild(O.Oldest.Value, lastNode.Value);
                                        }
                                        else
                                        {
                                            if (GetElementParent(lastNode.Value) != null)
                                                InsertBefore(lastNode.Value, table.Value);
                                            else
                                                AppendChild(table.Prev.Value, lastNode.Value);
                                        }
                                        break;
                                    default:
                                        AppendChild(commonAncestor.Value, lastNode.Value);
                                        break;
                                }
                                T instance2 = CreateElement(formattingElement.Token.TagName, formattingElement.Token.Attributes, formattingElement.Namespace);
                                Element<T> element = new Element<T>(instance2, formattingElement.Token, formattingElement.Namespace);
                                MoveChilds(furthestBlock.Value, element.Value);
                                AppendChild(furthestBlock.Value, element.Value);
                                F.Remove(formattingElement.Entry);
                                F.AddBefore(bookmark, element);
                                O.Remove(formattingElement);
                                O.AddAfter(furthestBlock, element);
                            }
                            return 1;
                        case "applet":
                        case "marquee":
                        case "object":
                            if (!ElementInScope(tagName))
                                return 0; //ignore token
                            O.GenerateImpliedEndTags(null);
                            O.ClearToTag(tagName);
                            F.ClearToMark();
                            return 1;
                        case "br":
                            {
                                ElementToken fakeToken = new ElementToken("br");
                                StartTagToken(ref fakeToken, mode);
                            }
                            return 0;
                        default:
                            for(Element<T> node = O.Current; node != null; node = node.Prev)
                            {
                                if (node.TagName == tagName)
                                {
                                    O.GenerateImpliedEndTags(tagName);
                                    O.RemoveTo(node, true);
                                    return 1;
                                }
                                if (IsSpecialElement(node))
                                    return 0; //ignore token
                            }
                            return 1;
                    }
                    
                case TEXT:
                    switch (tagName)
                    {
                        case "script":
                            //Provide stable state
                            T script = O.Pop().Value;
                            INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                            PrepareScript(script);
                            return 1;
                        default:
                            O.Pop();
                            INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                            return 1;
                    }
                    
                case IN_TABLE:
                    switch (tagName)
                    {
                        case "table":
                            if (IS_FRAGMENT && !ElementInTableScope(tagName))
                                return 0; //ignore token
                            O.ClearToElement("table", NS_HTML);
                            ResetInsertionMode();
                            return 1;
                        case "body":
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "html":
                        case "tbody":
                        case "td":
                        case "tfoot":
                        case "th":
                        case "thead":
                        case "tr":
                            return 0; //ignore token
                        default:
                            FOSTER_PARENTED = true;
                            EndTagToken(tagName, IN_BODY);
                            FOSTER_PARENTED = false;
                            return 1;
                    }
                    
                case IN_TABLE_TEXT:
                    {
                        if (ContainsNonWhiteSpace(ref pendingTokenList, 0, pendingTokenListPtr))
                        {
                            FOSTER_PARENTED = true;
                            CharToken(ref pendingTokenList, 0, pendingTokenListPtr, IN_BODY);
                            FOSTER_PARENTED = false;
                        }
                        else
                        {
                            InsertCharacters(new string(pendingTokenList, 0, pendingTokenListPtr));
                        }
                        INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                        mode = REPROCESS; goto EVAL;
                    }

                case IN_CAPTION:
                    switch (tagName)
                    {
                        case "caption":
                            if (!ElementInTableScope(tagName))
                                return 0; //ignore token
                            O.GenerateImpliedEndTags(null);
                            O.ClearToElement("caption", NS_HTML);
                            F.ClearToMark();
                            INSERTION_MODE = IN_TABLE;
                            return 1;
                        case "table":
                            if (EndTagToken("caption", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                        case "body":
                        case "col":
                        case "colgroup":
                        case "html":
                        case "tbody":
                        case "td":
                        case "tfoot":
                        case "th":
                        case "thead":
                        case "tr":
                            return 0; //ignore token
                        default:
                            mode = IN_BODY; goto EVAL;
                    }
                    
                case IN_COLUMN_GROUP:
                    switch (tagName)
                    {
                        case "colgroup":
                            if (IS_FRAGMENT && O.Current == O.Oldest)
                                return 0; //ignore token
                            O.Pop();
                            INSERTION_MODE = IN_TABLE;
                            return 1;
                        case "col":
                            return 0; //ignore token
                        default:
                            if (EndTagToken("colgroup", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                    }
                    
                case IN_TABLE_BODY:
                    switch (tagName)
                    {
                        case "tbody":
                        case "tfoot":
                        case "thead":
                            if (!ElementInTableScope(tagName))
                                return 0; //ignore token
                            ClearStackBackToTableBodyContext();
                            O.Pop();
                            INSERTION_MODE = IN_TABLE;
                            return 1;
                        case "table":
                            if (IS_FRAGMENT)
                            {
                                if (!ElementInTableScope("tbody") &&
                                    !ElementInTableScope("thead") &&
                                    !ElementInTableScope("tfoot"))
                                    return 0;//ignore token
                            }
                            ClearStackBackToTableBodyContext();
                            EndTagToken(O.Current.TagName, mode);
                            mode = REPROCESS; goto EVAL;
                        case "body":
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "html":
                        case "td":
                        case "th":
                        case "tr":
                            return 0; //ignore token
                        default:
                            mode = IN_TABLE; goto EVAL;
                    }
                    
                case IN_ROW:
                    switch (tagName)
                    {
                        case "tr":
                            if (!ElementInTableScope(tagName))
                                return 0; //ignore token
                            ClearStackBackToTableRowContext();
                            O.Pop();
                            INSERTION_MODE = IN_TABLE_BODY;
                            return 1;
                        case "table":
                            if (EndTagToken("tr", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                            }
                            return 1;
                        case "tbody":
                        case "tfoot":
                        case "thead":
                            if (!ElementInTableScope(tagName))
                                return 0;//ignore token
                            EndTagToken("tr", mode);
                            mode = REPROCESS; goto EVAL;
                        case "body":
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "html":
                        case "td":
                        case "th":
                            return 0; //ignore token
                        default:
                            mode = IN_TABLE; goto EVAL;
                    }
                    
                case IN_CELL:
                    switch (tagName)
                    {
                        case "td":
                        case "th":
                            if (!ElementInTableScope(tagName))
                                return 0; //return;
                            O.GenerateImpliedEndTags(null);
                            O.ClearToTag(tagName);
                            F.ClearToMark();
                            INSERTION_MODE = IN_ROW;
                            return 1;
                        case "body":
                        case "caption":
                        case "col":
                        case "colgroup":
                        case "html":
                            return 0; //ignore token
                        case "table":
                        case "tbody":
                        case "tfoot":
                        case "thead":
                        case "tr":
                            if (!ElementInTableScope(tagName))
                                return 0;//ignore token
                            CloseTheCell(mode);
                            mode = REPROCESS; goto EVAL;
                        default:
                            mode = IN_BODY; goto EVAL;
                    }
                    
                case IN_SELECT:
                    switch (tagName)
                    {
                        case "optgroup":
                            {
                                Element<T> element = O.Current;
                                if (element.TagName == "optgroup" && element.Prev.TagName == "optgroup")
                                    EndTagToken("option", mode);
                                if (O.Current.TagName == "option")
                                    O.Pop();
                                else
                                    return 0; //ignore token
                            }
                            return 1;
                        case "option":
                            if (O.Current.TagName == "option")
                                O.Pop();
                            else
                                return 0;//ignore token
                            return 1;
                        case "select":
                            if (IS_FRAGMENT && !ElementInSelectScope(tagName))
                                return 0;//ignore token
                            O.ClearToElement("select", NS_HTML);
                            ResetInsertionMode();
                            return 1;
                        default:
                            return 0;//ignore token
                    }
                    
                case IN_SELECT_IN_TABLE:
                    switch (tagName)
                    {
                        case "caption":
                        case "table":
                        case "tbody":
                        case "tfoot":
                        case "thead":
                        case "tr":
                        case "td":
                        case "th":
                            if (ElementInTableScope(tagName))
                            {
                                EndTagToken("select", mode);
                                mode = REPROCESS; goto EVAL;
                            }
                            else
                                return 0;//ignore token
                        default:
                            mode = IN_SELECT; goto EVAL;
                    }

                case AFTER_BODY:
                    if (tagName == "html")
                    {
                        if (IS_FRAGMENT)
                            return 0; //ignore token
                        else
                        {
                            INSERTION_MODE = AFTER_AFTER_BODY;
                            return 1;
                        }
                    }
                    else
                    {
                        INSERTION_MODE = IN_BODY;
                        mode = REPROCESS; goto EVAL;
                    }
                    
                case IN_FRAMESET:
                    if (tagName == "frameset")
                    {
                        if (IS_FRAGMENT && O.Current == O.Oldest)
                            return 0; //ignore token
                        O.Pop();
                        if(!IS_FRAGMENT && O.Current.TagName != "frameset")
                            INSERTION_MODE = AFTER_FRAMESET;
                        return 1;
                    }
                    else
                        return 0; //ignore token
                    
                case AFTER_FRAMESET:
                    if (tagName == "html")
                    {
                        INSERTION_MODE = AFTER_AFTER_FRAMESET;
                        return 1;
                    }
                    else
                        return 0; //ignore token
                    
                case AFTER_AFTER_BODY:
                    INSERTION_MODE = IN_BODY;
                    mode = REPROCESS; goto EVAL;

                case AFTER_AFTER_FRAMESET:
                    return 0;//ignore token
                case IN_FOREIGN_CONTENT:
                    if (tagName == "script" && O.Current.TagName == "script" && O.Current.Namespace == NS_SVG)
                    {
                        O.Pop();
                        return 1;
                    }
                    else
                    {
                        Element<T> node = O.Current;
                        while (true)
                        {
                            if (node.TagName.ToLower() == tagName)
                                return EndTagToken(tagName, INSERTION_MODE);
                            node = node.Prev;
                            if (node.Namespace != NS_HTML)
                                continue;
                            return EndTagToken(tagName, INSERTION_MODE);
                        }

                    }
            }
            return 1;
        }
        #endregion

        #region DOCTYPE TOKEN
        internal byte DoctypeToken(string name, string publicId, string systemId, bool forceQuirks, byte mode)
        {
            EVAL:
            if (mode == REPROCESS)
                mode = INSERTION_MODE;

            switch (mode)
            {
                case INITIAL:
                    {
                        //ignore errors
                        AppendDoctypeToDocument(name, publicId, systemId);
                        bool condition = false;
                        if (forceQuirks && (name == null || name != "html"))
                            condition = true;
                        if (!condition && publicId != null && publicId != string.Empty)
                        {
                            foreach (string p in publicIds)
                                if (publicId.StartsWith(p))
                                {
                                    condition = true;
                                    break;
                                }
                        }
                        if (!condition)
                        {
                            switch (publicId)
                            {
                                case "-//W3O//DTD W3 HTML Strict 3.0//EN//":
                                case "-/W3C/DTD HTML 4.0 Transitional/EN":
                                case "HTML":
                                    condition = true;
                                    break;
                                case null:
                                    break;
                                default:
                                    if (systemId == null)
                                    {
                                        if (publicId.StartsWith("-//W3C//DTD HTML 4.01 Frameset//")
                                            || publicId.StartsWith("-//W3C//DTD HTML 4.01 Transitional//"))
                                            condition = true;
                                    }
                                    else
                                    {
                                        if (systemId == "http://www.ibm.com/data/dtd/v11/ibmxhtml1-transitional.dtd")
                                            condition = true;
                                    }
                                    break;
                            }
                        }
                        //finally
                        if (condition)
                        {
                            DocMode = DocumentQuirkFlag.QuirksMode;
                        }
                        else
                        {
                            if (publicId != null && publicId != string.Empty)
                            {
                                if (publicId.StartsWith("-//W3C//DTD XHTML 1.0 Frameset//") ||
                                   publicId.StartsWith("-//W3C//DTD XHTML 1.0 Transitional//"))
                                    DocMode = DocumentQuirkFlag.LimitedQuirksMode;
                                else
                                {
                                    if (systemId != null &&
                                        (publicId.StartsWith("-//W3C//DTD HTML 4.01 Frameset//")
                                        || publicId.StartsWith("-//W3C//DTD HTML 4.01 Transitional//")))
                                        DocMode = DocumentQuirkFlag.LimitedQuirksMode;
                                }
                            }
                        }
                        INSERTION_MODE = BEFORE_HTML;
                    }
                    break;
                case IN_TABLE_TEXT:
                    {

                        if (ContainsNonWhiteSpace(ref pendingTokenList, 0, pendingTokenListPtr))
                        {
                            FOSTER_PARENTED = true;
                            CharToken(ref pendingTokenList, 0, pendingTokenListPtr, IN_BODY);
                            FOSTER_PARENTED = false;
                        }
                        else
                        {
                            InsertCharacters(new string(pendingTokenList, 0, pendingTokenListPtr));
                        }
                        INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                        mode = REPROCESS; goto EVAL;
                        //DoctypeToken(name, publicId, systemId, forceQuirks, REPROCESS);
                    }
                    //break;
                default:
                    return 0;//ignore token
            }
            return 1;
        }
        #endregion

        #region COMMENT TOKEN
        internal byte CommentToken(string comment, byte mode)
        {
            EVAL:
            if (mode == REPROCESS)
                mode = INSERTION_MODE;

            switch (mode)
            {
                case INITIAL:
                case BEFORE_HTML:
                case AFTER_AFTER_BODY:
                case AFTER_AFTER_FRAMESET:
                    AppendCommentToDocument(comment);
                    break;
                case IN_TABLE_TEXT:
                    {
                        if (ContainsNonWhiteSpace(ref pendingTokenList, 0, pendingTokenListPtr))
                        {
                            FOSTER_PARENTED = true;
                            CharToken(ref pendingTokenList, 0, pendingTokenListPtr, IN_BODY);
                            FOSTER_PARENTED = false;
                        }
                        else
                        {
                            InsertCharacters(new string(pendingTokenList, 0, pendingTokenListPtr));
                        }
                        INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                        mode = REPROCESS; goto EVAL;
                        //CommentToken(comment, REPROCESS);
                    }
                    //break;
                case AFTER_BODY:
                    AppendComment(O.Oldest.Value, comment);
                    break;
                default:
                    AppendComment(O.Current.Value, comment);
                    break;
            }
            return 1;
        }
        #endregion

        #region CHAR TOKEN
        internal byte CharToken(ref char[] t, int start, int length, byte mode)
        {
            EVAL:
            if (mode == REPROCESS)
                mode = INSERTION_MODE;
            switch (mode)
            {
                case INITIAL:
                    for (int i = start; i < length; i++)
                        switch (t[i])
                        {
                            case '\u0020':
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u000d':
                                continue;
                            default:
                                goto INITIAL_DEFAULT;
                        }
                    return 0;
                INITIAL_DEFAULT:
                    {
                        DocMode = DocumentQuirkFlag.QuirksMode;
                        INSERTION_MODE = BEFORE_HTML;
                        mode = REPROCESS; goto EVAL;
                    }
                    
                case BEFORE_HTML:
                    for (int i = start; i < length; i++)
                        switch (t[i])
                        {
                            case '\u0020':
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u000d':
                                continue;
                            default:
                                goto BEFORE_HTML_DEFAULT;
                        }
                    return 0;
                    BEFORE_HTML_DEFAULT:
                    {
                        T element = CreateElement("html", null, NS_HTML);
                        AppendElementToDocument(element);
                        O.Push(new Element<T>(element, "html", NS_HTML));
                        INSERTION_MODE = BEFORE_HEAD;
                        mode = REPROCESS; goto EVAL;
                    }
                    
                case BEFORE_HEAD:
                    for (int i = start; i < length; i++)
                        switch (t[i])
                        {
                            case '\u0020':
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u000d':
                                continue;
                            default:
                                goto BEFORE_HEAD_DEFAULT;

                        }
                    return 0;
                BEFORE_HEAD_DEFAULT:
                    {
                        ElementToken fakeToken = new ElementToken("head");
                        StartTagToken(ref fakeToken, mode);
                        mode = REPROCESS; goto EVAL;
                    }
                    
                case IN_HEAD:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                                switch (t[i])
                                {
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000a':
                                    case '\u000c':
                                    case '\u000d':
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                    default:
                                        goto IN_HEAD_DEFAULT;
                                }
                        }
                        {
                            /*
                            int l = buffer.Length;
                            if (l > 0)
                            {
                                char[] text = new char[l];
                                buffer.CopyTo(0, text, 0, l);
                                buffer = null;
                                InsertCharacters(text);
                            }*/
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            return 1;
                        }
                    IN_HEAD_DEFAULT:
                        {
                            /*
                             int l = buffer.Length;
                             if (l > 0)
                             {
                                 char[] text = new char[l];
                                 buffer.CopyTo(0, text, 0, l);
                                 buffer = null;
                                 InsertCharacters(text);
                             }*/
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            EndTagToken("head", mode);
                            mode = REPROCESS; goto EVAL;
                        }
                        //CharToken(ref t, i, length, REPROCESS);
                        //break;
                    }
                case IN_HEAD_NOSCRIPT:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                                switch (t[i])
                                {
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000a':
                                    case '\u000c':
                                    case '\u000d':
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                    default:
                                        goto IN_HEAD_NOSCRIPT_DEFAULT;
                                }
                        }
                        {
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            return 1;
                        }
                    IN_HEAD_NOSCRIPT_DEFAULT:
                        {
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            EndTagToken("noscript", mode);
                            mode = REPROCESS; goto EVAL;
                            //CharToken(ref t, i, length, REPROCESS);
                            //break;
                        }
                    }
                case AFTER_HEAD:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                                switch (t[i])
                                {
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000a':
                                    case '\u000c':
                                    case '\u000d':
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                    default:
                                        goto AFTER_HEAD_DEFAULT;
                                }
                        }
                        {
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            return 1;
                        }
                    AFTER_HEAD_DEFAULT:
                        {
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            ElementToken fakeToken = new ElementToken("body");
                            StartTagToken(ref fakeToken, mode);
                            FRAMESETOK = true;
                            mode = REPROCESS; goto EVAL;
                        }
                    }
                    
                case IN_BODY:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        bool reconstruct = true;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                            {
                                switch (t[i])
                                {
                                    case '\u0000':
                                        continue;
                                    case '\u000a':
                                        if (IGNORE_LF_TOKEN)
                                            IGNORE_LF_TOKEN = false;
                                        else
                                        {
                                            if (reconstruct)
                                                reconstruct = ReconstructFormattingElements();
                                            buffer[bufferPtr++] = t[i];
                                        }
                                        continue;
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000c':
                                    case '\u000d':
                                        if (reconstruct)
                                            reconstruct = ReconstructFormattingElements();
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                    default:
                                        if (reconstruct)
                                            reconstruct = ReconstructFormattingElements();
                                        FRAMESETOK = false;
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                }
                            }
                        }
                        if (bufferPtr > 0)
                            InsertCharacters(new string(buffer, 0, bufferPtr));
                    }
                    break;
                case TEXT:
                    InsertCharacters(new string(t, start, length));
                    /*
                    if (start == 0)
                    {
                        InsertCharacters(new string(t));
                    }
                    else
                    {
                        //char[] text = new char[length - start];
                        //t.CopyTo(text, start);
                        //Buffer.BlockCopy(t, start, text, 0, length - start);
                        InsertCharacters(new string(t, start, length));
                    }*/
                    break;
                case IN_TABLE:
                    switch (O.Current.TagName)
                    {
                        case "table":
                        case "tbody":
                        case "tfoot":
                        case "thead": 
                        case "tr":
                            pendingTokenListPtr = 0;
                            ORIGINAL_INSERTION_MODE = INSERTION_MODE;
                            INSERTION_MODE = IN_TABLE_TEXT;
                            mode = REPROCESS; goto EVAL;
                        default:
                            FOSTER_PARENTED = true;
                            CharToken(ref t, start, length, IN_BODY);
                            FOSTER_PARENTED = false;
                            break;
                    }
                    break;
                case IN_TABLE_TEXT:
                    while (pendingTokenListPtr + length - start >= pendingTokenListCount)
                        DoublePendingTokenList();
                    unchecked
                    {
                        for (int i = start; i < length; i++)
                            if (t[i] != '\u0000')
                                pendingTokenList[pendingTokenListPtr++] = t[i];
                    }
                    break;
                case IN_CAPTION:
                    mode = IN_BODY; goto EVAL;
                    //CharToken(ref t, start, length, IN_BODY);
                    //break;
                case IN_COLUMN_GROUP:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                                switch (t[i])
                                {
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000a':
                                    case '\u000c':
                                    case '\u000d':
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                    default:
                                        goto IN_COLUMN_GROUP_DEFAULT;
                                }
                        }
                        {
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            return 1;
                        }
                    IN_COLUMN_GROUP_DEFAULT:
                        {
                            if (bufferPtr > 0)
                                InsertCharacters(new string(buffer, 0, bufferPtr));
                            if (EndTagToken("colgroup", mode) != 0)
                            {
                                mode = REPROCESS; goto EVAL;
                                //CharToken(ref t, i, length, REPROCESS);
                            }
                        }
                    }
                    break;
                case IN_TABLE_BODY:
                case IN_ROW:
                    mode = IN_TABLE; goto EVAL;
                    //CharToken(ref t, start, length, IN_TABLE);
                    //break;
                case IN_CELL:
                    mode = IN_BODY; goto EVAL;
                    //CharToken(ref t, start, length, IN_BODY);
                    //break;
                case IN_SELECT:
                case IN_SELECT_IN_TABLE:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                                if (t[i] != '\u0000')
                                    buffer[bufferPtr++] = t[i];
                        }
                        if (bufferPtr > 0)
                            InsertCharacters(new string(buffer, 0, bufferPtr));
                    }
                    break;
                case AFTER_BODY:
                case AFTER_AFTER_BODY:
                    if (ContainsNonWhiteSpace(ref t, start, length))
                    {
                        INSERTION_MODE = IN_BODY;
                        mode = REPROCESS; goto EVAL;
                        //CharToken(ref t, start, length, REPROCESS);
                    }
                    else
                    {
                        mode = IN_BODY; goto EVAL;
                        //CharToken(ref t, start, length, IN_BODY);
                    }
                    //break;
                case IN_FRAMESET:
                case AFTER_FRAMESET:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                                switch (t[i])
                                {
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000a':
                                    case '\u000c':
                                    case '\u000d':
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                }
                        }
                            /*if (isWhitespace(ref t[i]))
                                buffer.Append(t[i]);*/
                        if (bufferPtr > 0)
                            InsertCharacters(new string(buffer, 0, bufferPtr));
                    }
                    break;
                case AFTER_AFTER_FRAMESET:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        bool reconstruct = true;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                            {
                                switch (t[i])
                                {
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000a':
                                    case '\u000c':
                                    case '\u000d':
                                        continue;
                                    default:
                                        if (reconstruct)
                                            reconstruct = ReconstructFormattingElements();
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                }
                                /*if (!isWhitespace(ref t[i]))
                                    continue;
                                if (reconstruct)
                                    reconstruct = ReconstructFormattingElements();
                                    buffer.Append(t[i]);*/
                            }
                        }
                        if (bufferPtr > 0)
                            InsertCharacters(new string(buffer, 0, bufferPtr));
                    }
                    break;
                case IN_FOREIGN_CONTENT:
                    {
                        //StringBuilder buffer = new StringBuilder();
                        char[] buffer = new char[length - start];
                        int bufferPtr = 0;
                        bool reconstruct = true;
                        unchecked
                        {
                            for (int i = start; i < length; i++)
                            {
                                switch (t[i])
                                {
                                    case '\u0000':
                                        buffer[bufferPtr++] = '\ufffd';
                                        continue;
                                    case '\u0020':
                                    case '\u0009':
                                    case '\u000a':
                                    case '\u000c':
                                    case '\u000d':
                                        if (reconstruct)
                                            reconstruct = ReconstructFormattingElements();
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                    default:
                                        if (reconstruct)
                                            reconstruct = ReconstructFormattingElements();
                                        FRAMESETOK = false;
                                        buffer[bufferPtr++] = t[i];
                                        continue;
                                }
                            }
                        }
                        if (bufferPtr > 0)
                            InsertCharacters(new string(buffer, 0, bufferPtr));
                    }
                    break;
            }
            return 1;
        }
        #endregion

        #region EOF TOKEN
        internal byte EofToken(byte mode)
        {
            EVAL:
            if (mode == REPROCESS)
                mode = INSERTION_MODE;

            switch (mode)
            {
                case INITIAL:
                    DocMode = DocumentQuirkFlag.QuirksMode;
                    INSERTION_MODE = BEFORE_HTML;
                    mode = REPROCESS; goto EVAL;
                    //EofToken(REPROCESS);
                    //break;
                case BEFORE_HTML:
                    {
                        T element = CreateElement("html", null, NS_HTML);
                        AppendElementToDocument(element);
                        O.Push(new Element<T>(element, "html", NS_HTML));
                        INSERTION_MODE = BEFORE_HEAD;
                        mode = REPROCESS; goto EVAL;
                       // EofToken(REPROCESS);
                    }
                    //break;
                case BEFORE_HEAD:
                    {
                        ElementToken fakeToken = new ElementToken("head");
                        StartTagToken(ref fakeToken, mode);
                        mode = REPROCESS; goto EVAL;
                    }
                    //EofToken(REPROCESS);
                    //break;
                case IN_HEAD:
                    EndTagToken("head", mode);
                    mode = REPROCESS; goto EVAL;
                    //EofToken(REPROCESS);
                    //break;
                case IN_HEAD_NOSCRIPT:
                    EndTagToken("noscript", mode);
                    mode = REPROCESS; goto EVAL;
                    //EofToken(REPROCESS);
                    //break;
                case AFTER_HEAD:
                    {
                        ElementToken fakeToken = new ElementToken("body");
                        StartTagToken(ref fakeToken, mode);
                        FRAMESETOK = true;
                        mode = REPROCESS; goto EVAL;
                    }
                    //EofToken(REPROCESS);
                    //break;
                case IN_BODY:
                    StopParsing();
                    break;
                case TEXT:
                    if (O.Current.TagName == "script")
                        SetScriptFlag(O.Current.Value, ScriptElementFlag.AlreadyStarted, true);
                    O.Pop();
                    INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                    mode = REPROCESS; goto EVAL;
                    //EofToken(REPROCESS);
                    //break;
                case IN_TABLE:
                    StopParsing();
                    break;
                case IN_TABLE_TEXT:
                    {
                        if (ContainsNonWhiteSpace(ref pendingTokenList, 0, pendingTokenListPtr))
                        {
                            FOSTER_PARENTED = true;
                            CharToken(ref pendingTokenList, 0, pendingTokenListPtr, IN_BODY);
                            FOSTER_PARENTED = false;
                        }
                        else
                        {
                            InsertCharacters(new string(pendingTokenList, 0, pendingTokenListPtr));
                        }
                        INSERTION_MODE = ORIGINAL_INSERTION_MODE;
                        mode = REPROCESS; goto EVAL;
                        //EofToken(REPROCESS);
                    }
                    //break;
                case IN_CAPTION:
                    mode = IN_BODY; goto EVAL;
                    //EofToken(IN_BODY);
                    //break;
                case IN_COLUMN_GROUP:
                    if (O.Current.TagName == "html")
                        StopParsing();
                    else
                    {
                        if (EndTagToken("colgroup", mode) != 0)
                        {
                            mode = REPROCESS; goto EVAL;
                            //EofToken(REPROCESS);
                        }
                    }
                    break;
                case IN_TABLE_BODY:
                    mode = IN_TABLE; goto EVAL;
                    //EofToken(IN_TABLE);
                    //break;
                case IN_ROW:
                    mode = IN_TABLE; goto EVAL;
                    //EofToken(IN_TABLE);
                    //break;
                case IN_CELL:
                    mode = IN_BODY; goto EVAL;
                    //EofToken(IN_BODY);
                    //break;
                case IN_SELECT:
                    StopParsing();
                    break;
                case IN_SELECT_IN_TABLE:
                    mode = IN_SELECT; goto EVAL;
                    //EofToken(IN_SELECT);
                    //break;
                case AFTER_BODY:
                    StopParsing();
                    break;
                case IN_FRAMESET:
                    StopParsing();
                    break;
                case AFTER_FRAMESET:
                    StopParsing();
                    break;
                case AFTER_AFTER_BODY:
                    StopParsing();
                    break;
                case AFTER_AFTER_FRAMESET:
                    StopParsing();
                    break;
                case IN_FOREIGN_CONTENT:
                    StopParsing();
                    break;
            }
            return 1;
        }
        #endregion

    }
}
