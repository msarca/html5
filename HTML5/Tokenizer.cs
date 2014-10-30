using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace HTML5
{
    public class Tokenizer<T> where T: class
    {
        #region STATES
        internal const byte DATA_STATE = 1;
        internal const byte CHARACTER_REFERENCE_IN_DATA_STATE = 2;
        internal const byte RCDATA_STATE = 3;
        internal const byte CHARACTER_REFERENCE_IN_RCDATA_STATE = 4;
        internal const byte RAWTEXT_STATE = 5;
        internal const byte SCRIPT_DATA_STATE = 6;
        internal const byte PLAINTEXT_STATE = 7;
        internal const byte TAG_OPEN_STATE = 8;
        internal const byte END_TAG_OPEN_STATE = 9;
        internal const byte TAG_NAME_STATE = 10;
        internal const byte RCDATA_LESS_THEN_SIGN_STATE = 11;
        internal const byte RCDATA_END_TAG_OPEN_STATE = 12;
        internal const byte RCDATA_END_TAG_NAME_STATE = 13;
        internal const byte RAWTEXT_LESS_THAN_SIGN_STATE = 14;
        internal const byte RAWTEXT_END_TAG_OPEN_STATE = 15;
        internal const byte RAWTEXT_END_TAG_NAME_STATE = 16;
        internal const byte SCRIPT_DATA_LESS_THAN_SIGN_STATE = 17;
        internal const byte SCRIPT_DATA_END_TAG_OPEN_STATE = 18;
        internal const byte SCRIPT_DATA_END_TAG_NAME_STATE = 19;
        internal const byte SCRIPT_DATA_ESCAPE_START_STATE = 20;
        internal const byte SCRIPT_DATA_ESCAPE_START_DASH_STATE = 21;
        internal const byte SCRIPT_DATA_ESCAPED_STATE = 22;
        internal const byte SCRIPT_DATA_ESCAPED_DASH_STATE = 23;
        internal const byte SCRIPT_DATA_ESCAPED_DASH_DASH_STATE = 24;
        internal const byte SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE = 25;
        internal const byte SCRIPT_DATA_ESCAPED_END_TAG_OPEN_STATE = 26;
        internal const byte SCRIPT_DATA_ESCAPED_END_TAG_NAME_STATE = 27;
        internal const byte SCRIPT_DATA_DOUBLE_ESCAPE_START_STATE = 28;
        internal const byte SCRIPT_DATA_DOUBLE_ESCAPED_STATE = 29;
        internal const byte SCRIPT_DATA_DOUBLE_ESCAPED_DASH_STATE = 30;
        internal const byte SCRIPT_DATA_DOUBLE_ESCAPED_DASH_DASH_STATE = 31;
        internal const byte SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE = 32;
        internal const byte SCRIPT_DATA_DOUBLE_ESCAPE_END_STATE = 33;
        internal const byte BEFORE_ATTRIBUTE_NAME_STATE = 34;
        internal const byte ATTRIBUTE_NAME_STATE = 35;
        internal const byte AFTER_ATTRIBUTE_NAME_STATE = 36;
        internal const byte BEFORE_ATTRIBUTE_VALUE_STATE = 37;
        internal const byte ATTRIBUTE_VALUE_DOUBLE_QUOTED_STATE = 38;
        internal const byte ATTRIBUTE_VALUE_SINGLE_QUOTED_STATE = 39;
        internal const byte ATTRIBUTE_VALUE_UNQUOTED_STATE = 40;
        internal const byte CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE = 41;
        internal const byte AFTER_ATTRIBUTE_VALUE_QUOTED_STATE = 42;
        internal const byte SELF_CLOSING_START_TAG_STATE = 43;
        internal const byte BOGUS_COMMENT_STATE = 44;
        internal const byte MARKUP_DECLARATION_OPEN_STATE = 45;
        internal const byte COMMENT_START_STATE = 46;
        internal const byte COMMENT_START_DASH_STATE = 47;
        internal const byte COMMENT_STATE = 48;
        internal const byte COMMENT_END_DASH_STATE = 49;
        internal const byte COMMENT_END_STATE = 50;
        internal const byte COMMENT_END_BANG_STATE = 51;
        internal const byte DOCTYPE_STATE = 52;
        internal const byte BEFORE_DOCTYPE_NAME_STATE = 53;
        internal const byte DOCTYPE_NAME_STATE = 54;
        internal const byte AFTER_DOCTYPE_NAME_STATE = 55;
        internal const byte AFTER_DOCTYPE_PUBLIC_KEYWORD_STATE = 56;
        internal const byte BEFORE_DOCTYPE_PUBLIC_IDENTIFIER_STATE = 57;
        internal const byte DOCTYPE_PUBLIC_IDENTIFIER_DOUBLE_QUOTED_STATE = 58;
        internal const byte DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE = 59;
        internal const byte AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE = 60;
        internal const byte BETWEEN_DOCTYPE_PUBLIC_AND_SYSTEM_IDENTIFIERS_STATE = 61;
        internal const byte AFTER_DOCTYPE_SYSTEM_KEYWORD_STATE = 62;
        internal const byte BEFORE_DOCTYPE_SYSTEM_IDENTIFIER_STATE = 63;
        internal const byte DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE = 64;
        internal const byte DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE = 65;
        internal const byte AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE = 66;
        internal const byte BOGUS_DOCTYPE_STATE = 67;
        internal const byte CDATA_SECTION_STATE = 68;
        #endregion

        #region Constants

		private static byte CHAR_ALPHA_LOW = 1; //alpha chars
		private static byte CHAR_ALPHA_UP = 2;

        private static char[] X_LT_SOLIDUS = new char[] { '<', '/' };
        private static char[] X_MINUS2 = new char[] { '-', '-' };
        private static char[] X_MINUS2_REPLACEMENT = new char[] { '-', '-', '\ufffd' };
        private static char[] X_LT_EXCLAMATION = new char[] { '<', '!' };
        private static char[] X_MINUS_REPLACEMENT = new char[] { '-', '\ufffd' };
        private static char[] X_MINUS2_EXCLAMATION = new char[] { '-', '-', '!' };
        private static char[] X_MINUS2_EXCLAMATION_REPLACEMENT = new char[] { '-', '-', '!', '\ufffd' };
        private static char[] R_REPLACEMENT = new char[] { '\ufffd'};
        private static char[] R_CR = new char[] { '\u000d' };
        private static char[][] R_NO_CHARS = new char[][] {
            new char[] {'a'}
        };
        #endregion

        char[] TempBuffer, TagName, DataBuffer, CommentBuffer, AttrNameBuffer, AttrValueBuffer;
        int TempBufferPtr = 0, DataBufferPtr = 0, TagNamePtr = 0, CommentBufferPtr = 0, AttrNameBufferPtr = 0,
            AttrValueBufferPtr = 0;
        int TempBufferLength = 1024, DataBufferLength = 1024, TagNameLength = 1024, CommentBufferLength = 1024,
            AttrNameBufferLength = 1024, AttrValueBufferLength = 1024, AttrCount = 0;


        internal byte STATE;
        DoctypeToken doctype;
        string LastTagName = string.Empty;
        bool TagIsSelfClosing = false, TagEndTag = false, iscapital = false;
        AttributeEntry AttrList = null;
        bool lastWasCR = false;
        int parsingCalls = 0;
        internal Decoder ByteDecoder, ByteDecoderCopy;
        internal char[] pendingBuffer = null, pendingBufferCopy = null;
        internal int pendingBufferLength = 0, pendingBufferCopyLength = 0;

        byte LAST_STATE;
        char ADDITIONAL_ALLOWED = '\u0000';
        TreeBuilder<T> builder;

        public Tokenizer(TreeBuilder<T> builder)
        {
            this.builder = builder;
            TagName = new char[TagNameLength];
            TempBuffer = new char[TempBufferLength];
            DataBuffer = new char[DataBufferLength];
            CommentBuffer = new char[CommentBufferLength];
            AttrNameBuffer = new char[AttrNameBufferLength];
            AttrValueBuffer = new char[AttrValueBufferLength];
            doctype = new DoctypeToken();
            ByteDecoder = Encoding.UTF8.GetDecoder();
            STATE = DATA_STATE;
        }

        #region Emit

        private void EmitTagToken()
        {

            if (DataBufferPtr > 0)
                FlushCharTokens();
            
            if (TagEndTag)
            {
                byte mode = 18; //TokenProcessingRule();
                if (builder.O.Current == null || builder.O.Current.Namespace == TreeBuilder<T>.NS_HTML)
                    mode = 0;
                /*if (builder.STACK.Count == 0 || builder.STACK.PEEK.NamespaceURI == TreeBuilder<T>.NS_HTML)
                    mode = 0;*/
                builder.EndTagToken(new string(TagName,0, TagNamePtr), mode);
            }
            else
            {
                LastTagName = new string(TagName, 0, TagNamePtr);
                CheckAttribute();
                byte mode = 18;// StartTagTokenProcessingRule();
                if (builder.O.Current == null ||
                   builder.O.Current.Namespace == TreeBuilder<T>.NS_HTML ||
                   (LastTagName != "mglyph" && LastTagName != "malignmark" && builder.MathMLIntergartionPoint()) ||
                   (LastTagName == "svg" && builder.O.Current.TagName == "annotation-xml") ||
                   builder.HTMLIntegrationPoint())
                    mode = 0;
                /*
                if (builder.STACK.Count == 0
                    || builder.STACK.PEEK.NamespaceURI == TreeBuilder<T>.NS_HTML
                    || (builder.MathMLIntergartionPoint() && LastTagName != "mglyph" && LastTagName != "malignmark")
                    || (builder.STACK.PEEK.Name == "annotation-xml" && LastTagName == "svg")
                    || builder.HTMLIntegrationPoint())
                    mode = 0;
                 */
                ElementToken token = new ElementToken(LastTagName, AttrList, AttrCount, TagIsSelfClosing);
                builder.StartTagToken(ref token, mode);
                AttrList = null;
                AttrCount = 0;
            }
            
        }

        private void EmitCommentToken()
        {

            if (DataBufferPtr > 0)
                FlushCharTokens();

            byte mode = 18;// TokenProcessingRule();
            if (builder.O.Current == null || builder.O.Current.Namespace == TreeBuilder<T>.NS_HTML)
                mode = 0;
            builder.CommentToken(new string(CommentBuffer, 0, CommentBufferPtr), mode);
        }

        private void EmitDoctypeToken()
        {

            if (DataBufferPtr > 0)
                FlushCharTokens();

            byte mode = 18;//TokenProcessingRule();
            if (builder.O.Current == null || builder.O.Current.Namespace == TreeBuilder<T>.NS_HTML)
                mode = 0;
            builder.DoctypeToken(doctype.Name, doctype.PublicId, doctype.SystemId, doctype.ForceQuirks, mode);
        }

        private void EmitEofToken()
        {
            
            if (DataBufferPtr > 0)
                FlushCharTokens();
            //return;
            //System.Windows.Forms.MessageBox.Show("Parse: " + timer.Elapsed);
            builder.EofToken(0);

        }

        private void EmitTmpBuffer()
        {
            for (int i = 0; i < TempBufferPtr; i++)
            {
                if (DataBufferPtr == DataBufferLength)
                    DataBufferDoubleSize();
                DataBuffer[DataBufferPtr++] = TempBuffer[i];
            }
            TempBufferPtr = 0;
        }
       
        private void FlushCharTokens()
        {
            //send chars to TreeBuilder
            int length = DataBufferPtr;
            if (length == 0)
                return;
            //clear the buffer
            DataBufferPtr = 0;
            //emit the token
            byte mode = 18;// CharTokenProcessingRule();
            if (builder.O.Current == null ||
                builder.O.Current.Namespace == TreeBuilder<T>.NS_HTML ||
                builder.MathMLIntergartionPoint() ||
                builder.MathMLIntergartionPoint())
                mode = 0;
            builder.CharToken(ref DataBuffer, 0, length, mode);
        }

        /*
        private byte TokenProcessingRule()
        {
            if (builder.O.Current == null || builder.O.Current.Namespace == TreeBuilder<T>.NS_HTML)
                return 0;
            return 18;
        }

        private byte CharTokenProcessingRule()
        {
            if (builder.O.Current == null ||
                builder.O.Current.Namespace == TreeBuilder<T>.NS_HTML ||
                builder.MathMLIntergartionPoint() ||
                builder.MathMLIntergartionPoint())
                return 0;
            return 18;
        }
        */

        #endregion

        private void NewAttribute(char c)
        {
            if (AttrNameBufferPtr > 0)
            {
                string attr = new string(AttrNameBuffer, 0, AttrNameBufferPtr);
                if (AttrList == null)
                {
                    AttrList = new AttributeEntry(attr, new string(AttrValueBuffer, 0, AttrValueBufferPtr));
                    AttrCount = 1;
                }
                else
                {
                    bool hasAttribute = false;
                    for(AttributeEntry p = AttrList; p != null; p = p.PrevAttribute)
                        if (p.Name == attr)
                        {
                            hasAttribute = true;
                            break;
                        }
                    if (!hasAttribute)
                    {
                        AttributeEntry entry = new AttributeEntry(attr, new string(AttrValueBuffer, 0, AttrValueBufferPtr));
                        entry.PrevAttribute = AttrList;
                        AttrList.NextAttribute = entry;
                        AttrList = entry;
                        AttrCount++;
                    }
                }
            }
            unchecked
            {
                AttrNameBuffer[0] = c;
                AttrNameBufferPtr = 1;
                AttrValueBufferPtr = 0;
            }
        }

        private void CheckAttribute()
        {
            if (AttrNameBufferPtr > 0)
            {
                string attr = new string(AttrNameBuffer, 0, AttrNameBufferPtr);
                if (AttrList == null)
                {
                    AttrList = new AttributeEntry(attr, new string(AttrValueBuffer, 0, AttrValueBufferPtr));
                    AttrCount = 1;
                }
                else
                {
                    bool hasAttribute = false;
                    for (AttributeEntry p = AttrList; p != null; p = p.PrevAttribute)
                        if (p.Name == attr)
                        {
                            hasAttribute = true;
                            break;
                        }
                    if (!hasAttribute)
                    {
                        AttributeEntry entry = new AttributeEntry(attr, new string(AttrValueBuffer, 0, AttrValueBufferPtr));
                        entry.PrevAttribute = AttrList;
                        AttrList.NextAttribute = entry;
                        AttrList = entry;
                        AttrCount++;
                    }
                }
            }
            if (AttrList != null) //reset attribute pointer
            {
                while (AttrList.PrevAttribute != null)
                    AttrList = AttrList.PrevAttribute;
            }
            AttrNameBufferPtr = 0;
            AttrValueBufferPtr = 0;
        }

        private void DataBufferDoubleSize()
        {
            DataBufferLength = DataBufferLength << 1;
            char[] tmp = new char[DataBufferLength];
            Array.Copy(DataBuffer, tmp, DataBufferPtr);
            DataBuffer = tmp;
        }

        private void CommentBufferDoubleSize()
        {
            CommentBufferLength = CommentBufferLength << 1;
            char[] tmp = new char[CommentBufferLength];
            Array.Copy(CommentBuffer, tmp, CommentBufferPtr);
            CommentBuffer = tmp;
        }

        private void AttrValueBufferDoubleSize()
        {
            AttrValueBufferLength = AttrValueBufferLength << 1;
            char[] tmp = new char[AttrValueBufferLength];
            Array.Copy(AttrValueBuffer, tmp, AttrValueBufferPtr);
            AttrValueBuffer = tmp;
        }

        private char[] charcterReference(int ENDOFSTREAM)
        {
            /*
            char c = '\u0000';
            
            //TODO: Continue implementation of characterReference
            if (pointer + 1 >= ENDOFSTREAM)
                return null;
            
            c = stream[pointer + 1];
            if (isWhitespace(ref c) || c == '<' || c == '&' || (c == ADDITIONAL_ALLOWED && ADDITIONAL_ALLOWED != '\u0000'))
                return null;
            if (c == '#')
            {
                pointer++;//consume
                if (pointer + 1 >= ENDOFSTREAM)
                {
                     pointer--;
                    return null;
                }
                string number = string.Empty;
                bool hex = false;
                c = stream[pointer + 1];
                if (c == 'x' || c == 'X')
                {
                    hex = true;
                    pointer++;
                }
                while (true)
                {
                    if (pointer + 1 >= ENDOFSTREAM)
                        break;
                    c = stream[pointer + 1];
                    if (hex)
                    {
                        if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                        {
                            number += c;
                            pointer++;//consume current char
                        }
                        break;
                    }
                    else
                    {
                        if (c >= '0' && c <= '9')
                        {
                            number += c;
                            pointer++;//consume current char
                        }
                        break;
                    }
                }
                if (number == string.Empty)
                {
                    pointer -= hex ? 2 : 1;
                    return null;
                }
                if (pointer + 1 < ENDOFSTREAM && stream[pointer + 1] == ';')
                    pointer++;
                int refno = int.Parse(number, hex ? System.Globalization.NumberStyles.HexNumber 
                    : System.Globalization.NumberStyles.Integer);
                if (refno >= 0x80 && refno <= 0x9F)
                    return R_NO_CHARS[refno - 0x80];
                if (refno == 0x00)
                    return R_REPLACEMENT;
                if (refno == 0x0D)
                    return R_CR;
                if ((refno >= 0xD800 && refno <= 0xDFFF) || refno > 0x10FFFF)
                    return R_REPLACEMENT;
                return new char[] { (char)refno }; 
            }*/
            return null;
        }

        internal void Parse(byte[] chunk, int chunkLength)
        {
            parsingCalls++;
            char[] cbuff = new char[pendingBufferLength + chunkLength];
            if (pendingBufferLength > 0)
            {
                Array.Copy(pendingBuffer, cbuff, pendingBufferLength);
                pendingBuffer = null;
            }
            int length = ByteDecoder.GetChars(chunk, 0, chunkLength, cbuff, pendingBufferLength);
            pendingBufferLength = 0;
            for (int pointer = 0; pointer < length; pointer++)
            {
                char c = unchecked(cbuff[pointer]);
                #region iterate
                switch (c)
                {
                    case '\n':
                        if (lastWasCR)
                        {
                            lastWasCR = false;
                            continue;
                        }
                        break;
                    case '\r':
                        {
                            c = '\n';
                            lastWasCR = true;
                        }
                        break;
                    case '\t':
                        lastWasCR = false;
                        break;
                    case '\ufeff':
                        if (pointer == 0 && parsingCalls == 0)
                            continue;
                        lastWasCR = false;
                        break;
                    default:
                        lastWasCR = false;
                        if (Char.IsControl(c))
                            continue;
                        break;
                }

                switch (STATE)
                {
                    case DATA_STATE:
                        switch (c)
                        {
                            case '&':
                                STATE = CHARACTER_REFERENCE_IN_DATA_STATE;
                                continue;
                            case '<':
                                STATE = TAG_OPEN_STATE;
                                continue;
                            case '\u0000':
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\u0000';
                                }
                                //DataBuffer.Append('\u0000');
                                continue;
                            default:
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append(c);
                                continue;
                        }
                    case CHARACTER_REFERENCE_IN_DATA_STATE:
                        {
                            STATE = DATA_STATE;
                            unchecked
                            {
                                if (DataBufferPtr == DataBufferLength)
                                    DataBufferDoubleSize();
                                DataBuffer[DataBufferPtr++] = '&';
                            }
                            //DataBuffer.Append('&');
                            pointer--;
                            //TODO: remove comments when ready
                            /*
                            ADDITIONAL_ALLOWED = '\u0000';
                            char[] ca = charcterReference();
                            if (ca == null)
                                DataBuffer.Append('&');
                            else
                                DataBuffer.Append(ca);*/
                        }
                        break;
                    case RCDATA_STATE:
                        switch (c)
                        {
                            case '&':
                                STATE = CHARACTER_REFERENCE_IN_RCDATA_STATE;
                                continue;
                            case '<':
                                STATE = RCDATA_LESS_THEN_SIGN_STATE;
                                continue;
                            case '\u0000':
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                //DataBuffer.Append('\ufffd');
                                continue;
                            default:
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append(c);
                                continue;
                        }

                    case CHARACTER_REFERENCE_IN_RCDATA_STATE:
                        {
                            STATE = RCDATA_STATE;
                            unchecked
                            {
                                if (DataBufferPtr == DataBufferLength)
                                    DataBufferDoubleSize();
                                DataBuffer[DataBufferPtr++] = '&';
                            }
                            //DataBuffer.Append('&');
                            pointer--;
                            //TODO: remove comment when ready
                            /*
                            ADDITIONAL_ALLOWED = '\u0000';
                            char[] ca = charcterReference();
                            if (ca == null)
                                DataBuffer.Append('&');
                            else
                                DataBuffer.Append(ca);*/
                        }
                        break;
                    case RAWTEXT_STATE:
                        switch (c)
                        {
                            case '<':
                                STATE = RAWTEXT_LESS_THAN_SIGN_STATE;
                                continue;
                            case '\u0000':
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                //DataBuffer.Append('\ufffd');
                                continue;
                            default:
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append(c);
                                continue;
                        }

                    case SCRIPT_DATA_STATE:
                        switch (c)
                        {
                            case '<':
                                STATE = SCRIPT_DATA_LESS_THAN_SIGN_STATE;
                                continue;
                            case '\u0000':
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                //DataBuffer.Append('\ufffd');
                                continue;
                            default:
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append(c);
                                continue;
                        }
                    case PLAINTEXT_STATE:
                        switch (c)
                        {
                            case '\u0000':
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                //DataBuffer.Append('\ufffd');
                                continue;
                            default:
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append(c);
                                continue;
                        }
                    case TAG_OPEN_STATE:
                        switch (c)
                        {
                            case '!':
                                STATE = MARKUP_DECLARATION_OPEN_STATE;
                                continue;
                            case '/':
                                STATE = END_TAG_OPEN_STATE;
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //initTag(true, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = (char)(c + 0x0020);
                                TagNamePtr = 1;
                                TagEndTag = false;
                                STATE = TAG_NAME_STATE;
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //initTag(true, c);
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = c;
                                TagNamePtr = 1;
                                TagEndTag = false;
                                STATE = TAG_NAME_STATE;
                                continue;
                            case '?':
                                STATE = BOGUS_COMMENT_STATE;
                                continue;
                            default:
                                STATE = DATA_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                }
                                //DataBuffer.Append('<');
                                pointer--;//reconsume
                                continue;
                        }
                    case END_TAG_OPEN_STATE:
                        switch (c)
                        {
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //initTag(false, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = c;
                                TagNamePtr = 1;
                                TagEndTag = true;
                                STATE = TAG_NAME_STATE;
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //initTag(false, c);
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = c;
                                TagNamePtr = 1;
                                TagEndTag = true;
                                STATE = TAG_NAME_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                continue;
                            default:
                                STATE = BOGUS_COMMENT_STATE;
                                break;
                        }
                        break;
                    case TAG_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                continue;
                            case '/':
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TagName.Append((char)(c + 0x0020));
                                unchecked
                                {
                                    TagName[TagNamePtr++] = (char)(c + 0x0020);
                                }
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TagName.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = c;
                                }
                                continue;
                            case '\u0000':
                                //TagName.Append('\ufffd');
                                unchecked
                                {
                                    TagName[TagNamePtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                //TagName.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = c;
                                }
                                continue;
                        }

                    case RCDATA_LESS_THEN_SIGN_STATE:
                        if (c == '/')
                        {
                            //TempBuffer.Remove(0, TempBuffer.Length);
                            TempBufferPtr = 0;
                            STATE = RCDATA_END_TAG_OPEN_STATE;
                        }
                        else
                        {
                            STATE = RCDATA_STATE;
                            unchecked
                            {
                                if (DataBufferPtr == DataBufferLength)
                                    DataBufferDoubleSize();
                                DataBuffer[DataBufferPtr++] = '<';
                            }
                            //DataBuffer.Append('<');
                            pointer--;
                        }
                        break;
                    case RCDATA_END_TAG_OPEN_STATE:
                        switch (c)
                        {
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //initTag(false,(char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = (char)(c + 0x0020);
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = RCDATA_END_TAG_NAME_STATE;
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //initTag(false,c);
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = c;
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = RCDATA_END_TAG_NAME_STATE;
                                continue;
                            default:
                                STATE = RCDATA_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                pointer--;
                                continue;
                        }
                    case RCDATA_END_TAG_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                break;
                            case '/':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                break;
                            case '>':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                break;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = (char)(c + 0x0020);
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TagName.Append(c);
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = c;
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            default:
                                STATE = RCDATA_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                EmitTmpBuffer();
                                pointer--;
                                continue;
                        }
                        break;
                    case RAWTEXT_LESS_THAN_SIGN_STATE:
                        if (c == '/')
                        {
                            //TempBuffer.Remove(0, TempBuffer.Length);
                            TempBufferPtr = 0;
                            STATE = RAWTEXT_END_TAG_OPEN_STATE;
                        }
                        else
                        {
                            STATE = RAWTEXT_STATE;
                            unchecked
                            {
                                if (DataBufferPtr == DataBufferLength)
                                    DataBufferDoubleSize();
                                DataBuffer[DataBufferPtr++] = '<';
                            }
                            //DataBuffer.Append('<');
                            pointer--;
                        }
                        break;
                    case RAWTEXT_END_TAG_OPEN_STATE:
                        switch (c)
                        {
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //initTag(false, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = (char)(c + 0x0020);
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = RAWTEXT_END_TAG_NAME_STATE;
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //initTag(false, c);
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = c;
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = RAWTEXT_END_TAG_NAME_STATE;
                                continue;
                            default:
                                STATE = RAWTEXT_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                pointer--;
                                continue;
                        }

                    case RAWTEXT_END_TAG_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                continue;
                            case '/':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                continue;
                            case '>':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = (char)(c + 0x0020);
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TagName.Append(c);
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = c;
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            default:
                                STATE = RAWTEXT_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                EmitTmpBuffer();
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_LESS_THAN_SIGN_STATE:
                        switch (c)
                        {
                            case '/':
                                //TempBuffer.Remove(0, TempBuffer.Length);
                                TempBufferPtr = 0;
                                STATE = SCRIPT_DATA_END_TAG_OPEN_STATE;
                                continue;
                            case '!':
                                STATE = SCRIPT_DATA_ESCAPE_START_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '!';
                                }
                                //DataBuffer.Append(X_LT_EXCLAMATION);
                                continue;
                            default:
                                STATE = SCRIPT_DATA_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                }
                                //DataBuffer.Append('<');
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_END_TAG_OPEN_STATE:
                        switch (c)
                        {
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //initTag(false,(char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = (char)(c + 0x0020);
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = SCRIPT_DATA_END_TAG_NAME_STATE;
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //initTag(false, c);
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = c;
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = SCRIPT_DATA_END_TAG_NAME_STATE;
                                continue;
                            default:
                                STATE = SCRIPT_DATA_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_END_TAG_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                continue;
                            case '/':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                continue;
                            case '>':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = (char)(c + 0x0020);
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TagName.Append(c);
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = c;
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                EmitTmpBuffer();
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_ESCAPE_START_STATE:
                        if (c == '-')
                        {
                            STATE = SCRIPT_DATA_ESCAPE_START_DASH_STATE;
                            //DataBuffer.Append('-');
                            unchecked
                            {
                                if (DataBufferPtr == DataBufferLength)
                                    DataBufferDoubleSize();
                                DataBuffer[DataBufferPtr++] = '-';
                            }
                        }
                        else
                        {
                            STATE = SCRIPT_DATA_STATE;
                            pointer--;
                        }
                        break;
                    case SCRIPT_DATA_ESCAPE_START_DASH_STATE:
                        if (c == '-')
                        {
                            STATE = SCRIPT_DATA_ESCAPED_DASH_DASH_STATE;
                            unchecked
                            {
                                if (DataBufferPtr == DataBufferLength)
                                    DataBufferDoubleSize();
                                DataBuffer[DataBufferPtr++] = '-';
                            }
                            //DataBuffer.Append('-');
                        }
                        else
                        {
                            STATE = SCRIPT_DATA_STATE;
                            pointer--;
                        }
                        break;
                    case SCRIPT_DATA_ESCAPED_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = SCRIPT_DATA_ESCAPED_DASH_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                //DataBuffer.Append('-');
                                continue;
                            case '<':
                                STATE = SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE;
                                continue;
                            case '\u0000':
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                //DataBuffer.Append('\ufffd');
                                continue;
                            default:
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append(c);
                                continue;
                        }

                    case SCRIPT_DATA_ESCAPED_DASH_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = SCRIPT_DATA_ESCAPED_DASH_DASH_STATE;
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                continue;
                            case '<':
                                STATE = SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE;
                                continue;
                            case '\u0000':
                                STATE = SCRIPT_DATA_ESCAPED_STATE;
                                //DataBuffer.Append('\ufffd');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_ESCAPED_STATE;
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                        }

                    case SCRIPT_DATA_ESCAPED_DASH_DASH_STATE:
                        switch (c)
                        {
                            case '-':
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                continue;
                            case '<':
                                STATE = SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE;
                                continue;
                            case '>':
                                STATE = SCRIPT_DATA_STATE;
                                //DataBuffer.Append('>');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '>';
                                }
                                continue;
                            case '\u0000':
                                STATE = SCRIPT_DATA_ESCAPED_STATE;
                                //DataBuffer.Append('\ufffd');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_ESCAPED_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append(c);
                                continue;
                        }

                    case SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE:
                        switch (c)
                        {
                            case '/':
                                //TempBuffer.Remove(0, TempBuffer.Length);
                                TempBufferPtr = 0;
                                STATE = SCRIPT_DATA_ESCAPED_END_TAG_OPEN_STATE;
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TempBuffer.Remove(0, TempBuffer.Length);
                                //TempBuffer.Append((char)(c + 0x0020));
                                unchecked
                                {
                                    TempBufferPtr = 0;
                                    TempBuffer[TempBufferPtr++] = (char)(c + 0x0020);
                                }
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPE_START_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append('<');
                                //DataBuffer.Append(c);
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TempBuffer.Remove(0, TempBuffer.Length);
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBufferPtr = 0;
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPE_START_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                //DataBuffer.Append('<');
                                //DataBuffer.Append(c);
                                continue;
                            default:
                                STATE = SCRIPT_DATA_STATE;
                                //DataBuffer.Append('<');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                }
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_ESCAPED_END_TAG_OPEN_STATE:
                        switch (c)
                        {
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //initTag(false, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = (char)(c + 0x0020);
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = SCRIPT_DATA_ESCAPED_END_TAG_NAME_STATE;
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //initTag(false, c);
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
                                TagName[0] = c;
                                TagNamePtr = 1;
                                TagEndTag = true;
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                STATE = SCRIPT_DATA_ESCAPED_END_TAG_NAME_STATE;
                                continue;
                            default:
                                STATE = SCRIPT_DATA_ESCAPED_STATE;
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_ESCAPED_END_TAG_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                continue;
                            case '/':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                continue;
                            case '>':
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = (char)(c + 0x0020);
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TagName.Append(c);
                                //TempBuffer.Append(c);
                                unchecked
                                {
                                    TagName[TagNamePtr++] = c;
                                    TempBuffer[TempBufferPtr++] = c;
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_ESCAPED_STATE;
                                //DataBuffer.Append(X_LT_SOLIDUS);
                                unchecked
                                {
                                    if (DataBufferPtr + 2 >= DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                    DataBuffer[DataBufferPtr++] = '/';
                                }
                                EmitTmpBuffer();
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_DOUBLE_ESCAPE_START_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                            case '/':
                            case '>':
                                if (new string(TempBuffer, 0, TempBufferPtr) == "script")
                                    STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                                else
                                    STATE = SCRIPT_DATA_ESCAPED_STATE;
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TempBuffer.Append((char)(c + 0x0020));
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = (char)(c + 0x0020);
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TempBuffer.Append(c);
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_ESCAPED_STATE;
                                pointer--;
                                continue;
                        }

                    case SCRIPT_DATA_DOUBLE_ESCAPED_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_DASH_STATE;
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                continue;
                            case '<':
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE;
                                //DataBuffer.Append('<');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                }
                                continue;
                            case '\u0000':
                                //DataBuffer.Append('\ufffd');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                        }

                    case SCRIPT_DATA_DOUBLE_ESCAPED_DASH_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_DASH_DASH_STATE;
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case '<':
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE;
                                //DataBuffer.Append('<');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case '\u0000':
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                                //DataBuffer.Append('\ufffd');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                        }

                    case SCRIPT_DATA_DOUBLE_ESCAPED_DASH_DASH_STATE:
                        switch (c)
                        {
                            case '-':
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case '<':
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE;
                                //DataBuffer.Append('<');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case '>':
                                STATE = SCRIPT_DATA_STATE;
                                //DataBuffer.Append('>');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case '\u0000':
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                                //DataBuffer.Append('\ufffd');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                        }

                    case SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE:
                        if (c == '/')
                        {
                            //TempBuffer.Remove(0, TempBuffer.Length);
                            TempBufferPtr = 0;
                            STATE = SCRIPT_DATA_DOUBLE_ESCAPE_END_STATE;
                            //DataBuffer.Append('/');
                            unchecked
                            {
                                if (DataBufferPtr == DataBufferLength)
                                    DataBufferDoubleSize();
                                DataBuffer[DataBufferPtr++] = c;
                            }
                        }
                        else
                        {
                            STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                            pointer--;
                        }
                        continue;
                    case SCRIPT_DATA_DOUBLE_ESCAPE_END_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                            case '/':
                            case '>':
                                //if (TempBuffer.ToString() == "script")
                                if (new string(TempBuffer, 0, TempBufferPtr) == "script")
                                    STATE = SCRIPT_DATA_ESCAPED_STATE;
                                else
                                    STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //TempBuffer.Append((char)(c + 0x0020));
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = (char)(c + 0x0020);
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                                //TempBuffer.Append(c);
                                //DataBuffer.Append(c);
                                unchecked
                                {
                                    TempBuffer[TempBufferPtr++] = c;
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            default:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                                pointer--;
                                continue;
                        }

                    case BEFORE_ATTRIBUTE_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '/':
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                NewAttribute((char)(c + 0x0020));
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case '\u0000':
                                NewAttribute('\ufffd');
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case '"':
                            case '\'':
                            case '<':
                            case '=':
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            default:
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                        }

                    case ATTRIBUTE_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = AFTER_ATTRIBUTE_NAME_STATE;
                                continue;
                            case '/':
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case '=':
                                STATE = BEFORE_ATTRIBUTE_VALUE_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                //AttrName.Append((char)(c + 0x0020));
                                unchecked
                                {
                                    AttrNameBuffer[AttrNameBufferPtr++] = (char)(c + 0x0020);
                                }
                                continue;
                            case '\u0000':
                                //AttrName.Append('\ufffd');
                                unchecked
                                {
                                    AttrNameBuffer[AttrNameBufferPtr++] = '\ufffd';
                                }
                                continue;
                            case '"':
                            case '\'':
                            case '<':
                                //AttrName.Append(c);
                                unchecked
                                {
                                    AttrNameBuffer[AttrNameBufferPtr++] = c;
                                }
                                continue;
                            default:
                                //AttrName.Append(c);
                                unchecked
                                {
                                    AttrNameBuffer[AttrNameBufferPtr++] = c;
                                }
                                continue;
                        }

                    case AFTER_ATTRIBUTE_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '/':
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case '=':
                                STATE = BEFORE_ATTRIBUTE_VALUE_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                NewAttribute((char)(c + 0x0020));
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case '\u0000':
                                NewAttribute('\ufffd');
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case '"':
                            case '\'':
                            case '<':
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            default:
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                        }

                    case BEFORE_ATTRIBUTE_VALUE_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '"':
                                STATE = ATTRIBUTE_VALUE_DOUBLE_QUOTED_STATE;
                                continue;
                            case '&':
                                STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                pointer--;
                                continue;
                            case '\'':
                                STATE = ATTRIBUTE_VALUE_SINGLE_QUOTED_STATE;
                                continue;
                            case '\u0000':
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = '\ufffd';
                                }
                                //AttrValue.Append('\ufffd');
                                STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case '<':
                            case '=':
                            case '`':
                                //AttrValue.Append(c);
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = c;
                                }
                                STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                continue;
                            default:
                                //AttrValue.Append(c);
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = c;
                                }
                                STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                continue;
                        }

                    case ATTRIBUTE_VALUE_DOUBLE_QUOTED_STATE:
                        switch (c)
                        {
                            case '"':
                                STATE = AFTER_ATTRIBUTE_VALUE_QUOTED_STATE;
                                continue;
                            case '&':
                                LAST_STATE = ATTRIBUTE_VALUE_DOUBLE_QUOTED_STATE;
                                STATE = CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE;
                                ADDITIONAL_ALLOWED = '"';
                                continue;
                            case '\u0000':
                                //AttrValue.Append('\ufffd');
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                //AttrValue.Append(c);
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = c;
                                }
                                continue;
                        }

                    case ATTRIBUTE_VALUE_SINGLE_QUOTED_STATE:
                        switch (c)
                        {
                            case '\'':
                                STATE = AFTER_ATTRIBUTE_VALUE_QUOTED_STATE;
                                continue;
                            case '&':
                                LAST_STATE = ATTRIBUTE_VALUE_SINGLE_QUOTED_STATE;
                                STATE = CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE;
                                ADDITIONAL_ALLOWED = '\'';
                                continue;
                            case '\u0000':
                                //AttrValue.Append('\ufffd');
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = '\ufffd';
                                }
                                continue;
                            default:
                                //AttrValue.Append(c);
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = c;
                                }
                                continue;
                        }

                    case ATTRIBUTE_VALUE_UNQUOTED_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                continue;
                            case '&':
                                LAST_STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                STATE = CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE;
                                ADDITIONAL_ALLOWED = '>';
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case '\u0000':
                                //AttrValue.Append('\ufffd');
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = '\ufffd';
                                }
                                continue;
                            case '"':
                            case '\'':
                            case '<':
                            case '=':
                            case '`':
                                //AttrValue.Append(c);
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = c;
                                }
                                continue;
                            default:
                                //AttrValue.Append(c);
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = c;
                                }
                                continue;
                        }

                    case CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE:
                        //TODO: implement char refference when ready
                        //AttrValue.Append('&');
                        unchecked
                        {
                            if (AttrValueBufferPtr == AttrValueBufferLength)
                                AttrValueBufferDoubleSize();
                            AttrValueBuffer[AttrValueBufferPtr++] = '&';
                        }
                        STATE = LAST_STATE;
                        pointer--;
                        continue;
                    case AFTER_ATTRIBUTE_VALUE_QUOTED_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                continue;
                            case '/':
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            default:
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                pointer--;
                                continue;
                        }

                    case SELF_CLOSING_START_TAG_STATE:
                        switch (c)
                        {
                            case '>':
                                TagIsSelfClosing = true;
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            default:
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                pointer--;
                                continue;
                        }

                    case BOGUS_COMMENT_STATE: //NEED TO CHANGE THIS
                        //Comment.Remove(0, Comment.Length);
                        CommentBufferPtr = 0;
                        while (pointer < length)
                        {
                            c = cbuff[pointer];
                            if (c == '\u0000')
                                c = '\ufffd';
                            else
                                if (c == '>')
                                {
                                    EmitCommentToken();
                                    STATE = DATA_STATE;
                                    break;
                                }
                            unchecked
                            {
                                if (CommentBufferPtr == CommentBufferLength)
                                    CommentBufferDoubleSize();
                                CommentBuffer[CommentBufferPtr++] = c;
                            }
                            pointer++;
                        }
                        continue;
                    case MARKUP_DECLARATION_OPEN_STATE:
                        if (pointer + 1 >= length)
                        {
                            pendingBufferLength = 1;
                            pendingBuffer = new char[] { c };
                            return;
                        }
                        if (c == '-' && unchecked(cbuff[pointer + 1]) == '-')
                        {
                            pointer++;
                            CommentBufferPtr = 0;
                            STATE = COMMENT_START_STATE;
                            continue;
                        }

                        if (pointer + 6 >= length)
                        {
                            pendingBufferLength = length - pointer;
                            pendingBuffer = new char[pendingBufferLength];
                            Array.Copy(cbuff, pointer, pendingBuffer, 0, pendingBufferLength);
                            return;
                        }
                        {
                            string nextValue = new string(cbuff, pointer, 7);
                            if (nextValue.ToLower() == "doctype")
                            {
                                pointer += 6;
                                STATE = DOCTYPE_STATE;
                                continue;
                            }
                            if (nextValue == "[CDATA[")
                            {
                                pointer += 6;
                                STATE = CDATA_SECTION_STATE;
                                continue;
                            }
                        }
                        STATE = BOGUS_COMMENT_STATE;
                        continue;
                    case COMMENT_START_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = COMMENT_START_DASH_STATE;
                                continue;
                            case '\u0000':
                                //Comment.Append('\ufffd');
                                unchecked
                                {
                                    if (CommentBufferPtr == CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '\ufffd';
                                }
                                STATE = COMMENT_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitCommentToken();
                                continue;
                            default:
                                //Comment.Append(c);
                                unchecked
                                {
                                    if (CommentBufferPtr == CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                STATE = COMMENT_STATE;
                                continue;
                        }

                    case COMMENT_START_DASH_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = COMMENT_END_STATE;
                                continue;
                            case '\u0000':
                                //Comment.Append(X_MINUS2_REPLACEMENT);
                                unchecked
                                {
                                    if (CommentBufferPtr + 3 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '\ufffd';
                                }
                                STATE = COMMENT_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitCommentToken();
                                continue;
                            default:
                                //Comment.Append('-');
                                //Comment.Append(c);
                                unchecked
                                {
                                    if (CommentBufferPtr + 2 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                STATE = COMMENT_STATE;
                                continue;
                        }

                    case COMMENT_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = COMMENT_END_DASH_STATE;
                                continue;
                            case '\u0000':
                                //Comment.Append('\ufffd');
                                unchecked
                                {
                                    if (CommentBufferPtr == CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                continue;
                            default:
                                //Comment.Append(c);
                                unchecked
                                {
                                    if (CommentBufferPtr == CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                continue;
                        }

                    case COMMENT_END_DASH_STATE:
                        switch (c)
                        {
                            case '-':
                                STATE = COMMENT_END_STATE;
                                continue;
                            case '\u0000':
                                //Comment.Append(X_MINUS_REPLACEMENT);
                                unchecked
                                {
                                    if (CommentBufferPtr + 2 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '\ufffd';
                                }
                                STATE = COMMENT_STATE;
                                continue;
                            default:
                                //Comment.Append('-');
                                //Comment.Append(c);
                                unchecked
                                {
                                    if (CommentBufferPtr + 2 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                STATE = COMMENT_STATE;
                                continue;
                        }

                    case COMMENT_END_STATE:
                        switch (c)
                        {
                            case '>':
                                STATE = DATA_STATE;
                                EmitCommentToken();
                                continue;
                            case '\u0000':
                                //Comment.Append(X_MINUS2_REPLACEMENT);
                                unchecked
                                {
                                    if (CommentBufferPtr + 3 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '\ufffd';
                                }
                                STATE = COMMENT_STATE;
                                continue;
                            case '!':
                                STATE = COMMENT_END_BANG_STATE;
                                continue;
                            case '-':
                                //Comment.Append('-');
                                unchecked
                                {
                                    if (CommentBufferPtr == CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                continue;
                            default:
                                //Comment.Append(X_MINUS2);
                                //Comment.Append(c);
                                unchecked
                                {
                                    if (CommentBufferPtr + 3 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                STATE = COMMENT_STATE;
                                continue;
                        }

                    case COMMENT_END_BANG_STATE:
                        switch (c)
                        {
                            case '-':
                                //Comment.Append(X_MINUS2_EXCLAMATION);
                                unchecked
                                {
                                    if (CommentBufferPtr + 3 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '!';
                                }
                                STATE = COMMENT_END_DASH_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitCommentToken();
                                continue;
                            case '\u0000':
                                //Comment.Append(X_MINUS2_EXCLAMATION_REPLACEMENT);
                                unchecked
                                {
                                    if (CommentBufferPtr + 4 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '!';
                                    CommentBuffer[CommentBufferPtr++] = '\ufffd';
                                }
                                STATE = COMMENT_STATE;
                                continue;
                            default:
                                //Comment.Append(X_MINUS2_EXCLAMATION);
                                //Comment.Append(c);
                                unchecked
                                {
                                    if (CommentBufferPtr + 4 >= CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '-';
                                    CommentBuffer[CommentBufferPtr++] = '!';
                                    CommentBuffer[CommentBufferPtr++] = c;
                                }
                                STATE = COMMENT_STATE;
                                continue;
                        }

                    case DOCTYPE_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = BEFORE_DOCTYPE_NAME_STATE;
                                continue;
                            default:
                                STATE = BEFORE_DOCTYPE_NAME_STATE;
                                pointer--;
                                continue;
                        }

                    case BEFORE_DOCTYPE_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                doctype.NewDoctype();
                                doctype.DoctypeName.Append((char)(c + 0x0020));
                                STATE = DOCTYPE_NAME_STATE;
                                continue;
                            case '\u0000':
                                doctype.NewDoctype();
                                doctype.DoctypeName.Append('\ufffd');
                                STATE = DOCTYPE_NAME_STATE;
                                continue;
                            case '>':
                                doctype.NewDoctype();
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.NewDoctype();
                                doctype.DoctypeName.Append(c);
                                STATE = DOCTYPE_NAME_STATE;
                                continue;
                        }

                    case DOCTYPE_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = AFTER_DOCTYPE_NAME_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                                doctype.DoctypeName.Append((char)(c + 0x0020));
                                continue;
                            case '\u0000':
                                doctype.DoctypeName.Append('\ufffd');
                                continue;
                            default:
                                doctype.DoctypeName.Append(c);
                                continue;
                        }

                    case AFTER_DOCTYPE_NAME_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                if (pointer + 5 >= length)
                                {
                                    pendingBufferLength = length - pointer;
                                    pendingBuffer = new char[pendingBufferLength];
                                    Array.Copy(cbuff, pointer, pendingBuffer, 0, pendingBufferLength);
                                    return;
                                }
                                else
                                {
                                    string text = (new string(cbuff, pointer, 6)).ToLower();
                                    switch (text)
                                    {
                                        case "public":
                                            pointer += 5;
                                            STATE = AFTER_DOCTYPE_PUBLIC_KEYWORD_STATE;
                                            continue;
                                        case "system":
                                            pointer += 5;
                                            STATE = AFTER_DOCTYPE_SYSTEM_KEYWORD_STATE;
                                            continue;
                                    }
                                }
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case AFTER_DOCTYPE_PUBLIC_KEYWORD_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = BEFORE_DOCTYPE_PUBLIC_IDENTIFIER_STATE;
                                continue;
                            case '"':
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case '\'':
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case BEFORE_DOCTYPE_PUBLIC_IDENTIFIER_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '"':
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case '\'':
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case DOCTYPE_PUBLIC_IDENTIFIER_DOUBLE_QUOTED_STATE:
                        switch (c)
                        {
                            case '"':
                                STATE = AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE;
                                continue;
                            case '\u0000':
                                doctype.DoctypePublicId.Append('\ufffd');
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DoctypePublicId.Append(c);
                                continue;
                        }

                    case DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE:
                        switch (c)
                        {
                            case '\'':
                                STATE = AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE;
                                continue;
                            case '\u0000':
                                doctype.DoctypePublicId.Append('\ufffd');
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DoctypePublicId.Append(c);
                                continue;
                        }

                    case AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = BETWEEN_DOCTYPE_PUBLIC_AND_SYSTEM_IDENTIFIERS_STATE;
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            case '"':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case '\'':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case BETWEEN_DOCTYPE_PUBLIC_AND_SYSTEM_IDENTIFIERS_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            case '"':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case '\'':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case AFTER_DOCTYPE_SYSTEM_KEYWORD_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                STATE = BEFORE_DOCTYPE_SYSTEM_IDENTIFIER_STATE;
                                continue;
                            case '"':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case '\'':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case BEFORE_DOCTYPE_SYSTEM_IDENTIFIER_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '"':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case '\'':
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE:
                        switch (c)
                        {
                            case '"':
                                STATE = AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE;
                                continue;
                            case '\u0000':
                                doctype.DocktypeSystemId.Append('\ufffd');
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DocktypeSystemId.Append(c);
                                continue;
                        }

                    case DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE:
                        switch (c)
                        {
                            case '\'':
                                STATE = AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE;
                                continue;
                            case '\u0000':
                                doctype.DocktypeSystemId.Append('\ufffd');
                                continue;
                            case '>':
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DocktypeSystemId.Append(c);
                                continue;
                        }

                    case AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE:
                        switch (c)
                        {
                            case '\u0009':
                            case '\u000a':
                            case '\u000c':
                            case '\u0020':
                                continue;
                            case '>':
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case BOGUS_DOCTYPE_STATE:
                        if (c == '>')
                        {
                            STATE = DATA_STATE;
                            EmitDoctypeToken();
                            continue;
                        }
                        continue;
                    case CDATA_SECTION_STATE:
                        while (pointer < length)
                        {
                            c = cbuff[pointer];
                            if (c == ']')
                            {
                                if (pointer + 2 >= length)
                                {
                                    if (pointer + 1 >= length)
                                    {
                                        pendingBufferLength = 1;
                                        pendingBuffer = new char[] { ']' };
                                        return;
                                    }
                                    unchecked
                                    {
                                        if (cbuff[pointer + 1] == ']')
                                        {
                                            pendingBufferLength = 2;
                                            pendingBuffer = new char[] { ']', ']' };
                                            return;
                                        }
                                        else
                                        {
                                            if (DataBufferPtr + 2 >= DataBufferLength)
                                                DataBufferDoubleSize();
                                            DataBuffer[DataBufferPtr++] = ']';
                                            DataBuffer[DataBufferPtr++] = cbuff[pointer + 1];
                                            pointer++;
                                        }
                                    }
                                }
                                else
                                {
                                    unchecked
                                    {
                                        if (cbuff[pointer + 1] == ']' && cbuff[pointer + 2] == '>')
                                        {
                                            STATE = DATA_STATE;
                                            break;
                                        }
                                        else
                                        {
                                            if (DataBufferPtr + 3 >= DataBufferLength)
                                                DataBufferDoubleSize();
                                            DataBuffer[DataBufferPtr++] = ']';
                                            DataBuffer[DataBufferPtr++] = cbuff[pointer + 1];
                                            DataBuffer[DataBufferPtr++] = cbuff[pointer + 2];
                                            pointer += 2;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                            }
                            pointer++;
                        }
                        continue;
                }

            }
                #endregion

        }

        internal void ParseEof()
        {
            if (pendingBufferLength > 0)
            {
                unchecked
                {
                    if (DataBufferPtr + pendingBufferLength >= DataBufferLength)
                        DataBufferDoubleSize();
                    for (int i = 0; i < pendingBufferLength; i++)
                        DataBuffer[DataBufferPtr++] = pendingBuffer[i];
                }
                pendingBuffer = null;
                pendingBufferLength = 0;
            }
            REPROCESS:
            switch (STATE)
            {
                case DATA_STATE:
                case RCDATA_STATE:
                case RAWTEXT_STATE:
                case SCRIPT_DATA_STATE:
                case PLAINTEXT_STATE:
                    EmitEofToken();
                    break;
                case CHARACTER_REFERENCE_IN_RCDATA_STATE:
                    STATE = RCDATA_STATE;
                    goto REPROCESS;
                case TAG_OPEN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr == DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                    }
                    STATE = DATA_STATE;
                    goto REPROCESS;
                case END_TAG_OPEN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                    }
                    STATE = DATA_STATE;
                    goto REPROCESS;
                case RCDATA_LESS_THEN_SIGN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr == DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                    }
                    STATE = RCDATA_STATE;
                    goto REPROCESS;
                case RCDATA_END_TAG_OPEN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                    }
                    STATE = RCDATA_STATE;
                    goto REPROCESS;
                case RCDATA_END_TAG_NAME_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 + TempBufferPtr >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                        EmitTmpBuffer();
                    }
                    STATE = RCDATA_STATE;
                    goto REPROCESS;
                case RAWTEXT_LESS_THAN_SIGN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr == DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                    }
                    STATE = RAWTEXT_STATE;
                    goto REPROCESS;
                case RAWTEXT_END_TAG_OPEN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                    }
                    STATE = RAWTEXT_STATE;
                    goto REPROCESS;
                case RAWTEXT_END_TAG_NAME_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 + TempBufferPtr >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                        EmitTmpBuffer();
                    }
                    STATE = RAWTEXT_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_LESS_THAN_SIGN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr == DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                    }
                    STATE = SCRIPT_DATA_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_END_TAG_OPEN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                    }
                    STATE = SCRIPT_DATA_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_END_TAG_NAME_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                        EmitTmpBuffer();
                    }
                    STATE = SCRIPT_DATA_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_ESCAPE_START_STATE:
                case SCRIPT_DATA_ESCAPE_START_DASH_STATE:
                    STATE = SCRIPT_DATA_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr == DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                    }
                    STATE = SCRIPT_DATA_ESCAPED_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_ESCAPED_END_TAG_OPEN_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                    }
                    STATE = SCRIPT_DATA_ESCAPED_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_ESCAPED_END_TAG_NAME_STATE:
                    unchecked
                    {
                        if (DataBufferPtr + 2 >= DataBufferLength)
                            DataBufferDoubleSize();
                        DataBuffer[DataBufferPtr++] = '<';
                        DataBuffer[DataBufferPtr++] = '/';
                        EmitTmpBuffer();
                    }
                    STATE = SCRIPT_DATA_ESCAPED_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_DOUBLE_ESCAPE_START_STATE:
                    STATE = SCRIPT_DATA_ESCAPED_STATE;
                    goto REPROCESS;
                case SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE:
                case SCRIPT_DATA_DOUBLE_ESCAPE_END_STATE:
                    STATE = SCRIPT_DATA_DOUBLE_ESCAPED_STATE;
                    goto REPROCESS;
                case CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE:
                    STATE = LAST_STATE;
                    goto REPROCESS;
                case MARKUP_DECLARATION_OPEN_STATE:
                    STATE = BOGUS_COMMENT_STATE;
                    goto REPROCESS;
                case COMMENT_START_STATE:
                case COMMENT_START_DASH_STATE:
                case COMMENT_STATE:
                case COMMENT_END_DASH_STATE:
                case COMMENT_END_STATE:
                case COMMENT_END_BANG_STATE:
                    EmitCommentToken();
                    STATE = DATA_STATE;
                    goto REPROCESS;
                case DOCTYPE_STATE:
                case BEFORE_DOCTYPE_NAME_STATE:
                case DOCTYPE_NAME_STATE:
                case AFTER_DOCTYPE_NAME_STATE:
                    doctype.NewDoctype();
                    doctype.ForceQuirks = true;
                    EmitDoctypeToken();
                    STATE = DATA_STATE;
                    goto REPROCESS;
                case AFTER_DOCTYPE_PUBLIC_KEYWORD_STATE:
                case BEFORE_DOCTYPE_PUBLIC_IDENTIFIER_STATE:
                case DOCTYPE_PUBLIC_IDENTIFIER_DOUBLE_QUOTED_STATE:
                case DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE:
                case AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE:
                case BETWEEN_DOCTYPE_PUBLIC_AND_SYSTEM_IDENTIFIERS_STATE:
                case AFTER_DOCTYPE_SYSTEM_KEYWORD_STATE:
                case BEFORE_DOCTYPE_SYSTEM_IDENTIFIER_STATE:
                case DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE:
                case DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE:
                case AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE:
                    doctype.ForceQuirks = true;
                    EmitDoctypeToken();
                    STATE = DATA_STATE;
                    goto REPROCESS;
                case BOGUS_DOCTYPE_STATE:
                    EmitDoctypeToken();
                    STATE = DATA_STATE;
                    goto REPROCESS;
                case CHARACTER_REFERENCE_IN_DATA_STATE:
                case TAG_NAME_STATE:
                case SCRIPT_DATA_ESCAPED_STATE:
                case SCRIPT_DATA_ESCAPED_DASH_STATE:
                case SCRIPT_DATA_ESCAPED_DASH_DASH_STATE:
                case SCRIPT_DATA_DOUBLE_ESCAPED_STATE:
                case SCRIPT_DATA_DOUBLE_ESCAPED_DASH_STATE:
                case SCRIPT_DATA_DOUBLE_ESCAPED_DASH_DASH_STATE:
                case BEFORE_ATTRIBUTE_NAME_STATE:
                case ATTRIBUTE_NAME_STATE:
                case AFTER_ATTRIBUTE_NAME_STATE:
                case BEFORE_ATTRIBUTE_VALUE_STATE:
                case ATTRIBUTE_VALUE_DOUBLE_QUOTED_STATE:
                case ATTRIBUTE_VALUE_SINGLE_QUOTED_STATE:
                case ATTRIBUTE_VALUE_UNQUOTED_STATE:
                case AFTER_ATTRIBUTE_VALUE_QUOTED_STATE:
                case SELF_CLOSING_START_TAG_STATE:
                case BOGUS_COMMENT_STATE:
                case CDATA_SECTION_STATE:
                    STATE = DATA_STATE;
                    goto REPROCESS;
            }

        }


    }

}
