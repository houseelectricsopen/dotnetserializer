using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamWriter = System.IO.StreamWriter;
using TextWriter = System.IO.TextWriter;
using StringWriter = System.IO.StringWriter;
using ObjectIDGenerator = System.Runtime.Serialization.ObjectIDGenerator;
//search for ^ reg ex- gives 1776 29 july 2013 of which 1026 test
//search for ^ reg ex- gives 1082 14 june 2013 of which 657 test
//search for ^ reg ex- gives 949 16 june 2013 of which 521 test, 428 serializer
//TODO add unit test for AlwaysDoubleQuotePropertyNames

namespace com.houseelectrics.serializer 
{
    public class Object2Json
    {
        private bool alwaysDoubleQuotePropertyNames = false;
        public bool AlwaysDoubleQuotePropertyNames
           {
              get { return alwaysDoubleQuotePropertyNames; }
              set { this.alwaysDoubleQuotePropertyNames = value; }
           }

        private TypeAliaser typeAliaser=null;
        public TypeAliaser TypeAliaser { get { return typeAliaser; } set { typeAliaser = value; } }

        private bool omitMarkAsArrayFunction = true;
        public bool OmitMarkAsArrayFunction { get { return omitMarkAsArrayFunction; } set { omitMarkAsArrayFunction = value; } }

        
        private bool omitDefaultLeafValuesInJs = false;
        public bool OmitDefaultLeafValuesInJs { get { return omitDefaultLeafValuesInJs; } set { omitDefaultLeafValuesInJs = value; } }

        private LeafDefaultSet leafDefaultSet = null;
        public LeafDefaultSet LeafDefaultSet { get { return leafDefaultSet; } set { leafDefaultSet = value; } }


        string objectResolverFunctionName = "resolveRefs";
        public string ObjectResolverFunctionName { get { return objectResolverFunctionName; } set { this.objectResolverFunctionName = value; } }

        string leafDefaultResolverFunctionName = "_resolveLeafDefaults";
        public string LeafDefaultResolverFunctionName { get { return leafDefaultResolverFunctionName; } set { this.leafDefaultResolverFunctionName = value; } }

        string attachId2ArrayFunctionName="attachId2Array";
        public string AttachId2ArrayFunctionName { get { return attachId2ArrayFunctionName; } set { attachId2ArrayFunctionName = value; } }
        public String getAttachId2ArrayJSFunction()
        {
            return "function " + AttachId2ArrayFunctionName +  "(id, arr) {arr." + idTag + "=id; arr." + isArrayIndicator + "='y'; return arr;}";
        }

        string markAsArrayFunctionName = "_a_";
        public string MarkAsArrayFunctionName { get { return markAsArrayFunctionName; } set { markAsArrayFunctionName = value; } }
        public String getMarkAsArrayJSFunction()
        {
            return Object2JsonJavascript.getMarkAsArrayJSFunction(MarkAsArrayFunctionName); 
        }   

        string idTag = "_id_";
        string IdTag { get { return idTag; } set { idTag = value; } }
        string referenceTag = "_ref_";
        string ReferenceTag { get { return referenceTag; } set { referenceTag = value; } }
        bool useReferences = false;
        public bool UseReferences { get { return useReferences; } set { useReferences = value; } }
        const string NewLine = "\r\n";
        public NodeExpander NodeExpander {get; set;}
        public string toJson(Object o)
        {
            return toJson(o, TypeAliaser);
        }

        public string toJson(Object o, TypeAliaser typeAliaser)
        {
            /**
                o2J.OmitDefaultLeafValuesInJs = true;
                o2J.LeafDefaultSet = lds;
            //  to do throw exception if this is not the case
            //  o2J.OmitMarkAsArrayFunction = false;
           **/

         
            StringWriter sw = new StringWriter();
            writeAsJson(o, sw, typeAliaser);
            sw.Flush();
            String str = sw.ToString();
            sw.Close();
            return str;
        }

        Func<Explorer> explorerFactory = () => { return new ObjectExplorerImpl(); };

        public Func<Explorer> ExplorerFactory { get { return explorerFactory; } set { explorerFactory = value; } }

        private void writePropertyName(string propertyName, TextWriter writer)
        {
            bool escapeRequired = this.AlwaysDoubleQuotePropertyNames || (propertyName.IndexOf('.') >= 0);
            if (escapeRequired) writer.Write('\"');
            writer.Write(propertyName);
            if (escapeRequired) writer.Write('\"');
        }

        class ExploreStackFrame { public int propertyCount;}
        public void writeAsJson(Object o, TextWriter writer, TypeAliaser typeAliaser=null)
        {
            if (LeafDefaultSet != null && OmitDefaultLeafValuesInJs && OmitMarkAsArrayFunction)
            {
                throw new Exception("Leaf defaulting requires Array marker for js code");
            }


            ObjectIDGenerator idGenerator = null;

            if (UseReferences)
            {
                idGenerator=new ObjectIDGenerator();
            }
            Stack<ExploreStackFrame> exploreStack = new Stack<ExploreStackFrame>();
            Explorer explorerImpl = ExplorerFactory();
            ((Explorer)explorerImpl).NodeExpander = NodeExpander;
            MoveAway down = delegate (Object from, string propertyName, Object to, bool isIndexed, int? index)
            {
                ExploreStackFrame currentFrame = exploreStack.Count>0 ? exploreStack.Peek():null;
                if (currentFrame != null)
                {
                    if (currentFrame.propertyCount > 0) writer.Write(", ");
                    currentFrame.propertyCount++;
                }
                if (from != null && propertyName != null)
                {
                    writePropertyName(propertyName, writer);
                    writer.Write(":");
                }
                ExploreStackFrame childFrame = new ExploreStackFrame();
                exploreStack.Push(childFrame);
                writeIndent(writer, exploreStack);
                if (UseReferences)
                {
                    bool firstTime;
                    long objectid = idGenerator.GetId(to, out firstTime);
                    if (firstTime)
                    {
                        // could be done like this ! (function() {var x=[1,2]; x.id="uuu";return x;})()
                        if (!isIndexed)
                        {
                            writer.Write("{" + this.IdTag + ":" + objectid + ' ');
                            childFrame.propertyCount++;
                            childFrame.propertyCount += writeTypeAliasProperty(writer, to, typeAliaser, childFrame.propertyCount);
                            
                        }
                        else
                        {
                            // no need for type alias
                            writer.Write(AttachId2ArrayFunctionName + "(" + objectid + ",[");
                        }
                        
                    }
                    else
                    {
                        writer.Write("{" + this.ReferenceTag + ":" + objectid);
                        return false;
                    }
                }
                else // !Use References
                {
                    if (!isIndexed)
                    {
                        writer.Write('{');
                        // todo -- check this out ............
                        childFrame.propertyCount += writeTypeAliasProperty(writer, to, typeAliaser, childFrame.propertyCount);
                    }
                    else
                    {
                        if (!OmitMarkAsArrayFunction)
                        {
                            writer.Write(markAsArrayFunctionName);
                            writer.Write("([");
                        }
                        else
                        {
                            writer.Write("[");
                        }
                    }
                }

                return true;
            };

            MoveBack up = (from, propertyName, to, isIndexed) => 
                {
                    if (!isIndexed) writer.Write('}');
                    else
                    {
                        writer.Write(']');
                        // is there a function wrapper ?
                        if (!OmitMarkAsArrayFunction || UseReferences) writer.Write(")");
                    }
                    exploreStack.Pop();
                    writeIndent(writer, exploreStack);
                };

            OnLeaf leaf = (from, propertyName, to, index) => 
              {
                  //check for default leaf values
                  if (!this.OmitDefaultLeafValuesInJs ||
                      !isDefaultLeafValue(from, propertyName, to, LeafDefaultSet))
                  {
                     ExploreStackFrame currentFrame = exploreStack.Peek();
                     if (currentFrame.propertyCount > 0) writer.Write(", ");
                     currentFrame.propertyCount++;
                     if (propertyName!=null)
                     {
                         writePropertyName(propertyName, writer);
                         writer.Write(":");                     
                     }
                  writeLeafValue(writer, to, propertyName);
                  }
              };
            explorerImpl.explore(o, down, up, leaf);
        }

        int  writeTypeAliasProperty(TextWriter writer, Object to, TypeAliaser typeAliaser, int currentFramePropertyCount)
        {
            if (typeAliaser != null)
            {
                if (currentFramePropertyCount > 0) writer.Write(',');
                writer.Write(this.TypeAliasProperty + ":'" + typeAliaser(to.GetType()) + "'");
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public Func<object, string, object, LeafDefaultSet, bool> isDefaultLeafValue = isDefaultLeafValueDefault;

        static bool isDefaultLeafValueDefault(object from, string propertyName, object leafValue, LeafDefaultSet LeafDefaultSet)
        {
            if ( LeafDefaultSet == null)
            {
                throw new Exception("OmitDefaultLeafValuesInJs==true but LeafDefaultSet is unspecified - unable to determine what values are default !");
            }

            Type t = from.GetType();
            if (LeafDefaultSet.Type2Defaults.ContainsKey(t))
            {
                Defaults4Class defaults = LeafDefaultSet.Type2Defaults[t];
                if (!defaults.PropertyName2DefaultValue.ContainsKey(propertyName))
                {
                    return false;
                }
                Object defaultValue = defaults.PropertyName2DefaultValue[propertyName];
                return (leafValue == defaultValue) || leafValue != null && leafValue.Equals(defaultValue);
            }
            {
                // is this an error ?
                return false;
            }
        }

        void writeIndent(TextWriter writer, Stack<ExploreStackFrame> exploreStack)
        {
            if (IndentSize != null)
            {
                writer.Write(NewLine);
                int fullIndentSize = ((int)IndentSize) * exploreStack.Count;
                for (int done = 0; done < fullIndentSize; done++)
                {
                    writer.Write(' ');
                }
            }
        }

        private string isArrayIndicator = "_a_";

        public Action<TextWriter, Object, string> writeLeafValue = defaultWriteLeafValue;

        public static Action<TextWriter, Object, string> defaultWriteLeafValue = (writer, to, propertyName) =>
        {
            if (to == null)
            {
                writer.Write("null");
            }
            else if (to.GetType() == typeof(string) || to.GetType() == typeof(Char))
            {
                writer.Write("\"");
                writer.Write(to.ToString());
                writer.Write("\"");
            }//todo simplify below
            else if (to.GetType() == typeof(Byte))
            {
                writer.Write((Byte)to);
            }
            else if (to.GetType() == typeof(Int16))
            {
                writer.Write((Int16)to);
            }
            else if (to.GetType() == typeof(Int32))
            {
                writer.Write((Int32)to);
            }
            else if (to.GetType() == typeof(Int64))
            {
                writer.Write((Int64)to);
            }
            else if (to.GetType() == typeof(UInt16))
            {
                writer.Write((UInt16)to);
            }
            else if (to.GetType() == typeof(UInt32))
            {
                writer.Write((UInt32)to);
            }
            else if (to.GetType() == typeof(UInt64))
            {
                writer.Write((UInt64)to);
            }
            else if (to.GetType() == typeof(Double))
            {
                writer.Write((Double)to);
            }
            else if (to.GetType() == typeof(Single))
            {
                writer.Write((Single)to);
            }
            else if (to.GetType() == typeof(Boolean))
            {
                writer.Write(((Boolean)to).CompareTo(true) == 0 ? "true" : "false");
            }

            else throw new Exception("not implemented writeLeafValue:" + to.GetType() + " " + propertyName);


        };

        private int? indentSize=null;
        public int? IndentSize { get { return indentSize; } set { indentSize = value; } }

    public string  getObjectResolverJS()
    {
        return Object2JsonJavascript.getObjectResolverJS(ObjectResolverFunctionName, IdTag, ReferenceTag);
    }

    string typeAliasProperty = "_t_";
    public string TypeAliasProperty { get { return typeAliasProperty; } set { this.typeAliasProperty = value; } }

    public string getLeafDefaultResolverJS()
    {
        return getLeafDefaultResolverJS(TypeAliaser);
    }

    public string getLeafDefaultResolverJS(TypeAliaser typeAliaser)
    {
        //switch off type aliasing
        
        string defaultsJS = toJson( LeafDefaultSet.getAlias2Defaults(typeAliaser), null);
        string defaultsVarName = "_typeAlias2LeafDefaults_";

        string js =
@"var "+ defaultsVarName +" = " + defaultsJS + @";
" + Object2JsonJavascript.ResolveLeafDefaultFunction;           
        return js;
    }
    }
}

/*
 * DONE
 improve browser test page 
 refactor js functions
 refactor test js functions
 done implement defaulting
 done fix array detection in Object resolver js
 done fit test results onto 1 test / example page
 done deserializer
 done implement security
 done asp examples
** TODO
   doneuserguide      
   replace use of ms script engine for tests Microsoft.JScript.Vsa.VsaEngine, Eval.JScriptEvaluate(strJS, vsaEngine);
   test on different browsers
       : testing todds.me.uk, ftp user u48650861, http://browsershots.org/
   review parameterization of js
   integration with NSpring
   maven build
   open source submission
   redo for java - more popular ?
 */
