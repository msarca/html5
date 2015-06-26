using System;
using System.Text;

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
		private const byte CHAR_ALPHA_LOW = 1; //alpha chars
		private const byte CHAR_ALPHA_UP = 2;
		private const byte CHAR_NULL = 3;
		private const byte CHAR_LT = 4;
		private const byte CHAR_GT = 5;
		private const byte CHAR_SOLIDUS = 6;
		private const byte CHAR_MINUS = 7;
		private const byte CHAR_AMPER = 8;
		private const byte CHAR_EQUAL = 9;
		private const byte CHAR_GRAVE = 10;
		private const byte CHAR_LF = 11;
		private const byte CHAR_REPLACEMENT = 12;
		private const byte CHAR_SQUOTE = 13;
		private const byte CHAR_DQUOTE = 14;
		private const byte CHAR_EXCLAMATION = 15;
		private const byte CHAR_QUESTION = 16;
		private const byte CHAR_WS = 17;
		private const byte CHAR_EOF = 18;
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
		char[] ADDITIONAL_ALLOWED;
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

		private char[] charcterReference(char[] cbuff, int pointer, int length, ref bool wait, ref int move)
        {

			if (pointer + 1 >= length) 
			{
				wait = true;
				return null;
			}

			bool number = false;
			char c = cbuff [pointer];

			if (ADDITIONAL_ALLOWED != null && ADDITIONAL_ALLOWED [0] == c) 
			{
				return null;
			}

				switch (c) 
				{
				case '\n':
				case '\f':
				case '\t':
				case ' ':
				case '<':
				case '&':
					return null;
				case '#':
					number = true;
					goto charcterReference_NUMBER;
			}

			return NamedCharRef.matchReference (cbuff, pointer, length, ref wait, ref move);

			charcterReference_NUMBER:

			if (pointer + 2 >= length) 
			{
				wait = true;
				return null;
			}

			c = cbuff [pointer + 2];
			if (c == 'x' || c == 'X') {
				return NamedCharRef.matchHexDigits (cbuff, pointer + 3, length, ref wait, ref move);
			}

			return NamedCharRef.matchDecDigits (cbuff, pointer + 3, length, ref wait, ref move);

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
				byte crtc = 0;
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
						crtc = CHAR_WS;
	                    break;
	                case '\r':
	                    {
	                        c = '\n';
							crtc = CHAR_WS;
	                        lastWasCR = true;
	                    }
	                    break;
					case '\t':
					case '\f':
					case ' ':
						lastWasCR = false;
						crtc = CHAR_WS;
						break;
					case '\ufeff':
						lastWasCR = false;
						if (pointer == 0 && parsingCalls == 1)
						    continue;
						crtc = CHAR_REPLACEMENT;
						break;
					case '&':
						lastWasCR = false;
						crtc = CHAR_AMPER;
						break;
					case '<':
						lastWasCR = false;
						crtc = CHAR_LT;
						break;
					case '>':
						lastWasCR = false;
						crtc = CHAR_GT;
						break;
					case '!':
						lastWasCR = false;
						crtc = CHAR_EXCLAMATION;
						break;
					case '?':
						lastWasCR = false;
						crtc = CHAR_QUESTION;
						break;
					case '/':
						lastWasCR = false;
						crtc = CHAR_SOLIDUS;
						break;
					case '"':
						lastWasCR = false;
						crtc = CHAR_DQUOTE;
						break;
					case '\'':
						lastWasCR = false;
						crtc = CHAR_SQUOTE;
						break;
					case '`':
						lastWasCR = false;
						crtc = CHAR_GRAVE;
						break;
					case '-':
						lastWasCR = false;
						crtc = CHAR_MINUS;
						break;
					case '=':
						lastWasCR = false;
						crtc = CHAR_EQUAL;
						break;
					case '\u0000':
						lastWasCR = false;
						crtc = CHAR_NULL;
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
						lastWasCR = false;
						crtc = CHAR_ALPHA_UP;
						break;
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
						lastWasCR = false;
						crtc = CHAR_ALPHA_LOW;
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
                        switch (crtc)
                        {
                            case CHAR_AMPER:
                                STATE = CHARACTER_REFERENCE_IN_DATA_STATE;
                                continue;
                            case CHAR_LT:
                                STATE = TAG_OPEN_STATE;
                                continue;
                            case CHAR_NULL:
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
							ADDITIONAL_ALLOWED = null;
							bool wait = false;		
							char[] data = charcterReference(cbuff, pointer, length, ref wait, ref pointer);
							
							if(data == null)
							{
								if(wait)
								{
									pendingBufferLength = length - pointer;
									pendingBuffer = new char[pendingBufferLength];
									Array.Copy(cbuff, pointer, pendingBuffer, 0, pendingBufferLength);
									STATE = CHARACTER_REFERENCE_IN_DATA_STATE;
									return;
								}

								if (DataBufferPtr == DataBufferLength)
									DataBufferDoubleSize();
								DataBuffer[DataBufferPtr++] = '&';
								pointer--;
								continue;
							}
							
							int dlen = data.Length;
							if (DataBufferPtr + dlen == DataBufferLength)
								DataBufferDoubleSize();
							
							for(int k = 0; k < dlen; k++)
							{
								DataBuffer[DataBufferPtr++] = data[k];
							}
							
                        }
                        break;
                    case RCDATA_STATE:
						switch (crtc)
                        {
                            case CHAR_AMPER:
                                STATE = CHARACTER_REFERENCE_IN_RCDATA_STATE;
                                continue;
                            case CHAR_LT:
                                STATE = RCDATA_LESS_THEN_SIGN_STATE;
                                continue;
                            case CHAR_NULL:
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
							
							ADDITIONAL_ALLOWED = null;
							bool wait = false;		
							char[] data = charcterReference(cbuff, pointer, length, ref wait, ref pointer);

							if(data == null)
							{
								if(wait)
								{
									pendingBufferLength = length - pointer;
									pendingBuffer = new char[pendingBufferLength];
									Array.Copy(cbuff, pointer, pendingBuffer, 0, pendingBufferLength);
									STATE = CHARACTER_REFERENCE_IN_RCDATA_STATE;
									return;
								}

								if (DataBufferPtr == DataBufferLength)
									DataBufferDoubleSize();
								DataBuffer[DataBufferPtr++] = '&';
								pointer--;
								continue;
							}

							int dlen = data.Length;
							if (DataBufferPtr + dlen == DataBufferLength)
								DataBufferDoubleSize();

							for(int k = 0; k < dlen; k++)
							{
								DataBuffer[DataBufferPtr++] = data[k];
							}
                        }
                        break;
                    case RAWTEXT_STATE:
                        switch (crtc)
                        {
                            case CHAR_LT:
                                STATE = RAWTEXT_LESS_THAN_SIGN_STATE;
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_LT:
                                STATE = SCRIPT_DATA_LESS_THAN_SIGN_STATE;
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_EXCLAMATION:
                                STATE = MARKUP_DECLARATION_OPEN_STATE;
                                continue;
                            case CHAR_SOLIDUS:
                                STATE = END_TAG_OPEN_STATE;
                                continue;
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //initTag(true, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
								TagName[0] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
                                TagNamePtr = 1;
                                TagEndTag = false;
                                STATE = TAG_NAME_STATE;
                                continue;
                            case CHAR_QUESTION:
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
                        switch (crtc)
                        {
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //initTag(false, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
								TagName[0] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
                                TagNamePtr = 1;
                                TagEndTag = true;
                                STATE = TAG_NAME_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                continue;
                            default:
                                STATE = BOGUS_COMMENT_STATE;
                                break;
                        }
                        break;
                    case TAG_NAME_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_SOLIDUS:
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TagName.Append((char)(c + 0x0020));
                                unchecked
                                {
									TagName[TagNamePtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
                                }
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //initTag(false,(char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
								TagName[0] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                break;
                            case CHAR_SOLIDUS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                break;
                            case CHAR_GT:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                break;
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
									TagName[TagNamePtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //initTag(false, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
								TagName[0] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                continue;
                            case CHAR_SOLIDUS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                continue;
                            case CHAR_GT:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                continue;
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
									TagName[TagNamePtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_SOLIDUS:
                                //TempBuffer.Remove(0, TempBuffer.Length);
                                TempBufferPtr = 0;
                                STATE = SCRIPT_DATA_END_TAG_OPEN_STATE;
                                continue;
                            case CHAR_EXCLAMATION:
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
                        switch (crtc)
                        {
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //initTag(false,(char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
								TagName[0] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                continue;
                            case CHAR_SOLIDUS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                continue;
                            case CHAR_GT:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                continue;
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
									TagName[TagNamePtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = SCRIPT_DATA_ESCAPED_DASH_STATE;
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                //DataBuffer.Append('-');
                                continue;
                            case CHAR_LT:
                                STATE = SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE;
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = SCRIPT_DATA_ESCAPED_DASH_DASH_STATE;
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                continue;
                            case CHAR_LT:
                                STATE = SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE;
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                continue;
                            case CHAR_LT:
                                STATE = SCRIPT_DATA_ESCAPED_LESS_THAN_SIGN_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = SCRIPT_DATA_STATE;
                                //DataBuffer.Append('>');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '>';
                                }
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_SOLIDUS:
                                //TempBuffer.Remove(0, TempBuffer.Length);
                                TempBufferPtr = 0;
                                STATE = SCRIPT_DATA_ESCAPED_END_TAG_OPEN_STATE;
                                continue;
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TempBuffer.Remove(0, TempBuffer.Length);
                                //TempBuffer.Append((char)(c + 0x0020));
                                unchecked
                                {
                                    TempBufferPtr = 0;
									TempBuffer[TempBufferPtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //initTag(false, (char)(c + 0x0020));
                                TagIsSelfClosing = false;
                                AttrList = null;
                                AttrCount = 0;
								TagName[0] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                else
                                    goto default;
                                continue;
                            case CHAR_SOLIDUS:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                    STATE = SELF_CLOSING_START_TAG_STATE;
                                else
                                    goto default;
                                continue;
                            case CHAR_GT:
                                //if (isAppropriate())
                                if (LastTagName != null && (LastTagName == new string(TagName, 0, TagNamePtr)))
                                {
                                    STATE = DATA_STATE;
                                    EmitTagToken();
                                }
                                else
                                    goto default;
                                continue;
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TagName.Append((char)(c + 0x0020));
                                //TempBuffer.Append(c);
                                unchecked
                                {
									TagName[TagNamePtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                            case CHAR_SOLIDUS:
                            case CHAR_GT:
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
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TempBuffer.Append((char)(c + 0x0020));
                                //DataBuffer.Append(c);
                                unchecked
                                {
									TempBuffer[TempBufferPtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_DASH_STATE;
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '-';
                                }
                                continue;
                            case CHAR_LT:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE;
                                //DataBuffer.Append('<');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = '<';
                                }
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_DASH_DASH_STATE;
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case CHAR_LT:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE;
                                //DataBuffer.Append('<');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                //DataBuffer.Append('-');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case CHAR_LT:
                                STATE = SCRIPT_DATA_DOUBLE_ESCAPED_LESS_THAN_SIGN_STATE;
                                //DataBuffer.Append('<');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case CHAR_GT:
                                STATE = SCRIPT_DATA_STATE;
                                //DataBuffer.Append('>');
                                unchecked
                                {
                                    if (DataBufferPtr == DataBufferLength)
                                        DataBufferDoubleSize();
                                    DataBuffer[DataBufferPtr++] = c;
                                }
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                            case CHAR_SOLIDUS:
                            case CHAR_GT:
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
                            case CHAR_ALPHA_UP:
							case CHAR_ALPHA_LOW:
                                //TempBuffer.Append((char)(c + 0x0020));
                                //DataBuffer.Append(c);
                                unchecked
                                {
									TempBuffer[TempBufferPtr++] = (crtc == CHAR_ALPHA_UP ? (char)(c + 0x0020) : c);
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_SOLIDUS:
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case CHAR_ALPHA_UP:
                                NewAttribute((char)(c + 0x0020));
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_NULL:
                                NewAttribute('\ufffd');
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_SQUOTE:
                            case CHAR_DQUOTE:
                            case CHAR_LT:
                            case CHAR_EQUAL:
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            default:
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                        }

                    case ATTRIBUTE_NAME_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = AFTER_ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_SOLIDUS:
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case CHAR_EQUAL:
                                STATE = BEFORE_ATTRIBUTE_VALUE_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case CHAR_ALPHA_UP:
                                //AttrName.Append((char)(c + 0x0020));
                                unchecked
                                {
                                    AttrNameBuffer[AttrNameBufferPtr++] = (char)(c + 0x0020);
                                }
                                continue;
                            case CHAR_NULL:
                                //AttrName.Append('\ufffd');
                                unchecked
                                {
                                    AttrNameBuffer[AttrNameBufferPtr++] = '\ufffd';
                                }
                                continue;
                            case CHAR_DQUOTE:
                            case CHAR_SQUOTE:
                            case CHAR_LT:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_SOLIDUS:
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case CHAR_EQUAL:
                                STATE = BEFORE_ATTRIBUTE_VALUE_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case CHAR_ALPHA_UP:
                                NewAttribute((char)(c + 0x0020));
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_NULL:
                                NewAttribute('\ufffd');
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_DQUOTE:
                            case CHAR_SQUOTE:
                            case CHAR_LT:
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                            default:
                                NewAttribute(c);
                                STATE = ATTRIBUTE_NAME_STATE;
                                continue;
                        }

                    case BEFORE_ATTRIBUTE_VALUE_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_DQUOTE:
                                STATE = ATTRIBUTE_VALUE_DOUBLE_QUOTED_STATE;
                                continue;
                            case CHAR_AMPER:
                                STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                pointer--;
                                continue;
                            case CHAR_SQUOTE:
                                STATE = ATTRIBUTE_VALUE_SINGLE_QUOTED_STATE;
                                continue;
                            case CHAR_NULL:
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = '\ufffd';
                                }
                                //AttrValue.Append('\ufffd');
                                STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case CHAR_LT:
                            case CHAR_EQUAL:
                            case CHAR_GRAVE:
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
                        switch (crtc)
                        {
                            case CHAR_DQUOTE:
                                STATE = AFTER_ATTRIBUTE_VALUE_QUOTED_STATE;
                                continue;
                            case CHAR_AMPER:
                                LAST_STATE = ATTRIBUTE_VALUE_DOUBLE_QUOTED_STATE;
                                STATE = CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE;
								ADDITIONAL_ALLOWED = new char[] {'"'};
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_SQUOTE:
                                STATE = AFTER_ATTRIBUTE_VALUE_QUOTED_STATE;
                                continue;
                            case CHAR_AMPER:
                                LAST_STATE = ATTRIBUTE_VALUE_SINGLE_QUOTED_STATE;
                                STATE = CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE;
								ADDITIONAL_ALLOWED = new char[]{'\''};
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_AMPER:
                                LAST_STATE = ATTRIBUTE_VALUE_UNQUOTED_STATE;
                                STATE = CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE;
								ADDITIONAL_ALLOWED = new char[]{ '>' };
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            case CHAR_NULL:
                                //AttrValue.Append('\ufffd');
                                unchecked
                                {
                                    if (AttrValueBufferPtr == AttrValueBufferLength)
                                        AttrValueBufferDoubleSize();
                                    AttrValueBuffer[AttrValueBufferPtr++] = '\ufffd';
                                }
                                continue;
                            case CHAR_DQUOTE:
                            case CHAR_SQUOTE:
                            case CHAR_LT:
                            case CHAR_EQUAL:
                            case CHAR_GRAVE:
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
						{
							bool wait = false;		
							char[] data = charcterReference(cbuff, pointer, length, ref wait, ref pointer);

							if(data == null)
							{
								if(wait)
								{
									pendingBufferLength = length - pointer;
									pendingBuffer = new char[pendingBufferLength];
									Array.Copy(cbuff, pointer, pendingBuffer, 0, pendingBufferLength);
									STATE = CHARACTER_REFERENCE_IN_ATTRIBUTE_VALUE_STATE;
									return;
								}

								if (AttrValueBufferPtr == AttrValueBufferLength)
									AttrValueBufferDoubleSize();
								AttrValueBuffer[AttrValueBufferPtr++] = '&';
								STATE = LAST_STATE;
								pointer--;
								continue;
							}

							int dlen = data.Length;
							
							if (AttrValueBufferPtr == AttrValueBufferLength)
								AttrValueBufferDoubleSize();

							for(int k = 0; k < dlen; k++)
							{
								AttrValueBuffer[AttrValueBufferPtr++] = data[k];
							}
						}
                        break;
                    case AFTER_ATTRIBUTE_VALUE_QUOTED_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                continue;
                            case CHAR_SOLIDUS:
                                STATE = SELF_CLOSING_START_TAG_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitTagToken();
                                continue;
                            default:
                                STATE = BEFORE_ATTRIBUTE_NAME_STATE;
                                pointer--;
                                continue;
                        }

                    case SELF_CLOSING_START_TAG_STATE:
                        switch (crtc)
                        {
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = COMMENT_START_DASH_STATE;
                                continue;
                            case CHAR_NULL:
                                //Comment.Append('\ufffd');
                                unchecked
                                {
                                    if (CommentBufferPtr == CommentBufferLength)
                                        CommentBufferDoubleSize();
                                    CommentBuffer[CommentBufferPtr++] = '\ufffd';
                                }
                                STATE = COMMENT_STATE;
                                continue;
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = COMMENT_END_STATE;
                                continue;
                            case CHAR_NULL:
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
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = COMMENT_END_DASH_STATE;
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
                                STATE = COMMENT_END_STATE;
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitCommentToken();
                                continue;
                            case CHAR_NULL:
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
                            case CHAR_EXCLAMATION:
                                STATE = COMMENT_END_BANG_STATE;
                                continue;
                            case CHAR_MINUS:
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
                        switch (crtc)
                        {
                            case CHAR_MINUS:
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
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitCommentToken();
                                continue;
                            case CHAR_NULL:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = BEFORE_DOCTYPE_NAME_STATE;
                                continue;
                            default:
                                STATE = BEFORE_DOCTYPE_NAME_STATE;
                                pointer--;
                                continue;
                        }

                    case BEFORE_DOCTYPE_NAME_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_ALPHA_UP:
                                doctype.NewDoctype();
                                doctype.DoctypeName.Append((char)(c + 0x0020));
                                STATE = DOCTYPE_NAME_STATE;
                                continue;
                            case CHAR_NULL:
                                doctype.NewDoctype();
                                doctype.DoctypeName.Append('\ufffd');
                                STATE = DOCTYPE_NAME_STATE;
                                continue;
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = AFTER_DOCTYPE_NAME_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            case CHAR_ALPHA_UP:
                                doctype.DoctypeName.Append((char)(c + 0x0020));
                                continue;
                            case CHAR_NULL:
                                doctype.DoctypeName.Append('\ufffd');
                                continue;
                            default:
                                doctype.DoctypeName.Append(c);
                                continue;
                        }

                    case AFTER_DOCTYPE_NAME_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = BEFORE_DOCTYPE_PUBLIC_IDENTIFIER_STATE;
                                continue;
                            case CHAR_DQUOTE:
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case CHAR_SQUOTE:
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_DQUOTE:
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case CHAR_SQUOTE:
                                doctype.EmptyPublicId = true;
                                STATE = DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_DQUOTE:
                                STATE = AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE;
                                continue;
                            case CHAR_NULL:
                                doctype.DoctypePublicId.Append('\ufffd');
                                continue;
                            case CHAR_GT:
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DoctypePublicId.Append(c);
                                continue;
                        }

                    case DOCTYPE_PUBLIC_IDENTIFIER_SINGLE_QUOTED_STATE:
                        switch (crtc)
                        {
                            case CHAR_SQUOTE:
                                STATE = AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE;
                                continue;
                            case CHAR_NULL:
                                doctype.DoctypePublicId.Append('\ufffd');
                                continue;
                            case CHAR_GT:
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DoctypePublicId.Append(c);
                                continue;
                        }

                    case AFTER_DOCTYPE_PUBLIC_IDENTIFIER_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = BETWEEN_DOCTYPE_PUBLIC_AND_SYSTEM_IDENTIFIERS_STATE;
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            case CHAR_DQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case CHAR_SQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case BETWEEN_DOCTYPE_PUBLIC_AND_SYSTEM_IDENTIFIERS_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_GT:
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            case CHAR_DQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case CHAR_SQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            default:
                                doctype.ForceQuirks = true;
                                STATE = BOGUS_DOCTYPE_STATE;
                                continue;
                        }

                    case AFTER_DOCTYPE_SYSTEM_KEYWORD_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                STATE = BEFORE_DOCTYPE_SYSTEM_IDENTIFIER_STATE;
                                continue;
                            case CHAR_DQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case CHAR_SQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_DQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_DOUBLE_QUOTED_STATE;
                                continue;
                            case CHAR_SQUOTE:
                                doctype.EmptySystemId = true;
                                STATE = DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE;
                                continue;
                            case CHAR_GT:
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
                        switch (crtc)
                        {
                            case CHAR_DQUOTE:
                                STATE = AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE;
                                continue;
                            case CHAR_NULL:
                                doctype.DocktypeSystemId.Append('\ufffd');
                                continue;
                            case CHAR_GT:
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DocktypeSystemId.Append(c);
                                continue;
                        }

                    case DOCTYPE_SYSTEM_IDENTIFIER_SINGLE_QUOTED_STATE:
                        switch (crtc)
                        {
                            case CHAR_SQUOTE:
                                STATE = AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE;
                                continue;
                            case CHAR_NULL:
                                doctype.DocktypeSystemId.Append('\ufffd');
                                continue;
                            case CHAR_GT:
                                doctype.ForceQuirks = true;
                                STATE = DATA_STATE;
                                EmitDoctypeToken();
                                continue;
                            default:
                                doctype.DocktypeSystemId.Append(c);
                                continue;
                        }

                    case AFTER_DOCTYPE_SYSTEM_IDENTIFIER_STATE:
                        switch (crtc)
                        {
                            case CHAR_WS:
                                continue;
                            case CHAR_GT:
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
