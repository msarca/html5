using System;
using System.Text;

namespace HTML5
{
    public class DoctypeToken
    {
        internal StringBuilder DoctypeName, DoctypePublicId, DocktypeSystemId;
        bool forceQuirq = false, emptyName = false, emptyPublicId = false, emptySystemId = false;

        public DoctypeToken()
        {
            DoctypeName = new StringBuilder();
            DoctypePublicId = new StringBuilder();
            DocktypeSystemId = new StringBuilder();
        }

        public void NewDoctype()
        {
            DoctypeName.Remove(0, DoctypeName.Length);
            DoctypePublicId.Remove(0, DoctypePublicId.Length);
            DocktypeSystemId.Remove(0, DocktypeSystemId.Length);
            forceQuirq = false;
            emptyName = false;
            emptyPublicId = false;
            emptySystemId = false;
        }

        public string Name
        {
            get
            {
                if (DoctypeName.Length == 0)
                    return emptyName ? string.Empty : null;
                return DoctypeName.ToString();
            }
        }

        public string PublicId
        {
            get
            {
                if (DoctypePublicId.Length == 0)
                    return emptyPublicId ? string.Empty : null;
                return DoctypePublicId.ToString();
            }
        }

        public string SystemId
        {
            get
            {
                if (DocktypeSystemId.Length == 0)
                    return emptySystemId ? string.Empty: null;
                return DocktypeSystemId.ToString();
            }
        }

        public bool ForceQuirks
        {
            get { return forceQuirq; }
            set { forceQuirq = value; }
        }

        public bool EmptyName
        {
            get { return emptyName; }
            set { emptyName = value; }
        }

        public bool EmptyPublicId
        {
            get { return emptyPublicId; }
            set { emptyPublicId = value; }
        }

        public bool EmptySystemId
        {
            get { return emptySystemId; }
            set { emptySystemId = value; }
        }
    }
}