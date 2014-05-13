using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{

    public interface JsonExploreListener
    {
        void JsonStartObject(string propertyName, int pos);
        void JsonLeaf(string propertyName, string value, bool isQuoted);
        void JsonEndObject(int pos);
        void JsonStartFunction(string functionName, int pos, string propertyName);
        void JsonEndFunction(int pos);
        void JsonStartArray(string propertyName, int pos);
        void JsonEndArray(int pos);
    }

    public class JSONExplorerImpl
    {
        public const string JavaScriptNullDesignator = "null";

        const char ObjectStart = '{';
        const char ObjectEnd = '}';

        int? firstNonWhiteSpaceCharacterIndex(string str, int startPos = 0, int? endPos = null)
        {
            return str.firstCharPosition(c => !Char.IsWhiteSpace(c), startPos, endPos);
        }

        int? lastNonWhiteSpaceCharacterIndex(string str)
        {
            return str.lastCharPosition(c => !Char.IsWhiteSpace(c));
        }

        public void explore(string json, JsonExploreListener listener)
        {
            int? firstNWSCharIndex = firstNonWhiteSpaceCharacterIndex(json);
            int? lastNonWSCharIndex = lastNonWhiteSpaceCharacterIndex(json);
            if (firstNWSCharIndex == null || lastNonWSCharIndex == null || json[firstNWSCharIndex.Value] != ObjectStart || json[lastNonWSCharIndex.Value] != ObjectEnd)
            {
                throw new Exception(String.Format("expected json to start with {0} and end with {1}", ObjectStart, ObjectEnd));
            }
            listener.JsonStartObject(null, 0);
            explore(json, firstNWSCharIndex.Value + 1, lastNonWSCharIndex.Value, listener);
        }


        //look for a propertyName terminated by :
        //look for a leaf value or object start
        // or continuation , or object end

        int? readEscapedQuotedString(string json, int valueStartPos, char quoteChar, int to, StringBuilder result)
        {
            //StringBuilder sb = new StringBuilder();
            int pos = valueStartPos;
            for (; pos < to; pos++)
            {
                char c = json[pos];
                if (c == '\\')
                {
                    pos++;
                    if (pos >= to) break;
                    else c = json[pos];
                }
                else
                {
                    if (c == quoteChar) break;
                }
                result.Append(c);
            }

            if (pos == to) return null;
            else return pos;
        }

        internal int exploreFunctionCall(string json, int from, int to, JsonExploreListener listener,
              string functionName)
        {
            return exploreList(json, from, to, listener, ')', (p) => { listener.JsonEndFunction(p); return true; }, "function call");
        }

        internal int exploreArray(string json, int from, int to, JsonExploreListener listener)
        {
            return exploreList(json, from, to, listener, ']', (p) => { listener.JsonEndArray(p); return true; }, "array");
        }

        internal int exploreList(string json, int from, int to, JsonExploreListener listener, char terminatingChar,
               Func<int, bool> terminate, string listType)
        {            
            // read arguments
            for (int pos = from+1; pos <= to; pos++)
            {
                int? nextPos = json.firstCharPosition(cin => !Char.IsWhiteSpace(cin), pos, to);
                if (nextPos == null)
                {
                    string strError = String.Format("expected whitespace char after char {0}", pos);
                    throw new Exception(strError);
                }
                pos = nextPos.Value;
                char c = json[pos];
                if (c==',')
                {
                    //throw new Exception("multiple javascript list items not supported");
                    continue;
                }
                if (c == terminatingChar)
                {
                    //listener.JsonEndFunction(pos);
                    terminate(pos);
                    return pos;
                }
                else if (c == ObjectStart)
                {
                    listener.JsonStartObject(/*currentPropertyName*/null, pos);
                    pos = explore(json, pos + 1, to, listener, null/*currentPropertyName*/);
                }
                else // process a leaf !
                {
                    pos = readNonObject(json, pos, to, listener, null);
                }
            }
            throw new Exception(listType + " not terminated");
        }

        internal int readNonObject(string json, int valueStartPos, int to, JsonExploreListener listener, string currentPropertyName)
        {
            int pos=-1;
            int? valueEndPos;
            //Predicate<char> valueEndCondition;
            Predicate<char> isQuoteChar = cin => cin == '"' || cin == '\'';
            StringUtils.Match valueEndCondition;
            //Predicate<char> isQuoteChar = cin=> cin=='"' || cin == '\'';
            char c = json[valueStartPos];
            bool isQuoted = isQuoteChar(c);
            string value = null;
            bool isFunctionCall = false;
            bool isArray = false;
            if (!isQuoted)
            {
                valueEndCondition = (cin, strin, posin) => Char.IsWhiteSpace(cin) || cin == ',' 
                      || cin == ObjectEnd || cin == '(' || cin==')' || cin==']';
                valueEndPos = json.firstCharPosition(valueEndCondition, valueStartPos, to);

                if (valueEndPos != null)
                {
                    value = json.Substring(valueStartPos, valueEndPos.Value - valueStartPos);
                    // is this a function start ?
                    char lastchar = json[valueEndPos.Value];
                    if (lastchar == '(')
                    {
                        listener.JsonStartFunction(value, valueEndPos.Value, currentPropertyName);
                        isFunctionCall = true;
                        pos = exploreFunctionCall(json, valueEndPos.Value, to, listener, value);
                    }
                    else if (value[0] == '[')
                    {
                        listener.JsonStartArray(currentPropertyName, valueStartPos);
                        isArray = true;
                        pos = exploreArray(json, valueStartPos, to, listener);
                    }
                    if (value.Equals(JavaScriptNullDesignator)) value = null;
                }
            }
            else
            {
                // this is wrong - whabout double, triple escaping !
                // read escaped quoted value
                // todo - remove object creation !
                StringBuilder sbValue = new StringBuilder();
                valueEndPos = readEscapedQuotedString(json, valueStartPos + 1, c, to, sbValue);
                //valueEndCondition = (cin, strin, posin) => isQuoteChar(cin) && !(posin>0 && strin[posin-1]=='\\' );
                //valueStartPos++;
                if (valueEndPos != null) value = sbValue.ToString();
            }

            if (null == valueEndPos)
            {
                string strError = String.Format("no end  found for property {0} at position {1} in json {2}", currentPropertyName, valueStartPos, json);
                throw new Exception(strError);
            }

            // check here if it is a function call
            if (!isFunctionCall && !isArray)
            {
                listener.JsonLeaf(currentPropertyName, value, isQuoted);
                pos = valueEndPos.Value - 1; // point to the last char processed
            }
            if (pos == -1) throw new Exception("pos not assigned correctly");
            if (isQuoted) pos++;
            return pos;
        }

        public int explore(string json, int from, int to, JsonExploreListener listener,
              string currentPropertyName = null)
        {
            int pos = from;
            for (; pos <= to; pos++)
            {
                int? nextPos = json.firstCharPosition(cin => cin != ',' && !Char.IsWhiteSpace(cin), pos, to); ;//firstNonWhiteSpaceCharacterIndex(json, pos, to);
                if (nextPos == null)
                {
                    string strError = String.Format("expected whitespace char after char {0}", pos);
                    throw new Exception(strError);
                }
                pos = nextPos.Value;
                char c = json[pos];
                if (c == '}')
                {
                    listener.JsonEndObject(pos);
                    return pos;
                }
                // read a property Name

                int? propertyNameEnd = json.firstCharPosition(cin => cin == ':', pos, to);
                if (null == propertyNameEnd)
                {
                    throw new Exception(string.Format("no end found to property name at char {0} property starts with {1} in json {2}", pos, json.Substring(pos), json.Substring(from)));
                }
                currentPropertyName = json.Substring(pos, propertyNameEnd.Value - pos);
                if (currentPropertyName[0] == '"') currentPropertyName = currentPropertyName.Substring(1);
                if (currentPropertyName.EndsWith("\"")) currentPropertyName = currentPropertyName.Substring(0, currentPropertyName.Length - 1);

                pos = propertyNameEnd.Value + 1;
                int? valueStartPos = firstNonWhiteSpaceCharacterIndex(json, pos, to);
                if (valueStartPos == null)
                {
                    string strError = String.Format("no value found for property {0} at position {1} in json {2}", currentPropertyName, pos, json);
                    throw new Exception(strError);
                }
                c = json[valueStartPos.Value];
                if (c == ObjectStart)
                {
                    listener.JsonStartObject(currentPropertyName, valueStartPos.Value);
                    pos = explore(json, valueStartPos.Value + 1, to, listener, currentPropertyName);
                }
                else // read a non object
                {
                    pos = readNonObject(json, valueStartPos.Value, to, listener, currentPropertyName);
                }

            }
            return pos;
        }
    }
}
