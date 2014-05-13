using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    
    public static class StringUtils
    {
        public delegate bool Match(char c, string str, int pos);

        public static int? firstCharPosition(this string str, Match match, int startPos = 0, int? endPos = 0)
        {
            if (endPos == null) endPos = str.Length;
            for (int pos = startPos; pos < endPos + 1; pos++)
            {
                if (match(str[pos], str, pos)) return pos;
            }
            return null;
        }
        
        public static int? firstCharPosition(this string str, Predicate<char> match, int startPos = 0, int? endPos = 0)
        {
            if (endPos == null) endPos = str.Length;
            for (int pos = startPos; pos < endPos+1; pos++)
            {
                if (match(str[pos])) return pos;
            }
            return null;
        }

        public static int? lastCharPosition(this string str, Predicate<char> match, int? startPos = null, int endPos = 0)
        {
            if (startPos == null) startPos = str.Length;
            for (int pos = startPos.Value-1; pos >= endPos; pos--)
            {
                if (match(str[pos])) return pos;
            }
            return null;
        }
    
    }

     





}
