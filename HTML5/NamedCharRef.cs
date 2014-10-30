using System;
using System.Collections.Generic;
using System.Text;

namespace HTML5
{
    public class NamedCharRef
    {
        private Dictionary<string, string> charValues;
        private Dictionary<int, char> numValues;
        private NamedCharRef()
        {

        }

        private static NamedCharRef _table = new NamedCharRef();

        private static List<char> noChar = new List<char>(){

        };

        private static List<string> wordString = new List<string>(){
        
        };

        public static bool ContainsCharReference(ref string name, char c)
        {
            lock (_table)
            {
                if (_table.charValues == null)
                    _table.Construct();
                return _table.charValues.ContainsKey(name + c);
            }
           
        }

        public static bool ContainsNumberReference(int number)
        {
            lock (_table)
            {
                if (_table.charValues == null)
                    _table.Construct();
                return _table.numValues.ContainsKey(number);
            }
        }

        public static char[] GetCharByIndex(int index)
        {
            return new char[] { noChar[index] };
        }

        private void Construct()
        {
         
        }

    }
}
