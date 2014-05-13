using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldInfo = System.Reflection.FieldInfo;
using PropertyInfo = System.Reflection.PropertyInfo;
using MemberInfo = System.Reflection.MemberInfo;
using MethodInfo = System.Reflection.MethodInfo;
using ConstructorInfo = System.Reflection.ConstructorInfo;
using ParameterInfo = System.Reflection.ParameterInfo;
using IDictionaryNonGeneric = System.Collections.IDictionary;
using IListNonGeneric = System.Collections.IList;
using MemberTypes = System.Reflection.MemberTypes;

namespace com.houseelectrics.serializer
{
    /*
     * created originally so DataContract 
     * support can be added non-intrusively
     */
    public interface DeserializationListener
    {
        void onCreateObject(object o);
        void onEndObject(object o);
        void onSetValue(object o, string propertyName, object value);
    }
    
    abstract class StackFrame
    {
        public abstract void submitObjectToStack(string propertyName, object value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener);
        public abstract void submitLeafToStack(string propertyName, string value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener);
    }
 
    class ObjectFrame : StackFrame
    {
        public string propertyName;
        public object theObject;
        public override void submitObjectToStack(string propertyName, object value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener)
        {
            string strError = 
            setValueinObject(this.theObject, propertyName, value, listener);
            if (strError != null) throw new Exception(strError);
        }
        public override void submitLeafToStack(string propertyName, string value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener)
        {
            string strError =
            setValueinObject(this.theObject, propertyName, value, listener);
            if (strError != null) throw new Exception(strError);
        }
    }

    class ArrayFrame : StackFrame
    {
        public List<object> objectList;
        public string propertyName;

        public override void submitObjectToStack(string propertyName, object value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener)
        {
            objectList.Add(value);
        }

        public override void submitLeafToStack(string propertyName, string value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener)
        {
            this.objectList.Add(value);
        }


    }

    class FunctionHolder : StackFrame
    {
        public List<object> paramList;
        public string functionName;
        public string propertyName;

        public override void submitObjectToStack(string propertyName, object value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener)
        {
            this.paramList.Add(value);
        }

        public override void submitLeafToStack(string propertyName, string value, Func<object, string, object, DeserializationListener, string> setValueinObject, DeserializationListener listener)
        {
            throw new Exception("FunctionHolder.submitLeafToStack not implemented");
        }
    }

    class BroadcastDeserializationListener : DeserializationListener
      {
        private List<DeserializationListener> listeners = new List<DeserializationListener>();
        public void Add(DeserializationListener listener)
        {
            listeners.Add(listener);
        }
        public void Remove(DeserializationListener listener)
        {
            listeners.Remove(listener);
        }

        public void onCreateObject(object o)
        {
            foreach(DeserializationListener listener in listeners)
            {
                listener.onCreateObject(o);
            }
        }
        public void onEndObject(object o)
        {
            foreach (DeserializationListener listener in listeners)
            {
                listener.onEndObject(o);
            }

        }
        public void onSetValue(object o, string propertyName, object value)
        {
            foreach (DeserializationListener listener in listeners)
            {
                listener.onSetValue(o, propertyName, value);
            }
        }

      }
    
    public class Json2Object : JsonExploreListener
    {


        BroadcastDeserializationListener listener = new BroadcastDeserializationListener();
        public void Add(DeserializationListener l)  { listener.Add(l); }
        public void Remove(DeserializationListener l) { listener.Remove(l); }

        public Json2Object()
        {
            //setToUseFields();
            //todo connect this function name with serialisation implementation
            string fname = "_a_";
            Func<List<object>, object> _a_ = (args) =>
                {
                    if (args.Count != 1)
                        throw new Exception("expected 1 argument to " + fname);
                    Type aType = typeof(object[]);
                    object arg0 = args[0];
                    if (arg0 != null && aType != arg0.GetType())
                    {
                        throw new Exception(fname + " expected arg0 to be type " + aType.FullName + "  but was " + arg0.GetType().FullName);
                    }
                    return arg0;
                };
            addFunction(fname, _a_);
        }

        private TypeAntiAliaser typeAntiAliaser = TypeAliaserUtils.createSimpleTypeAntiAliaser();
        public TypeAntiAliaser TypeAntiAliaser
        {
            get { return typeAntiAliaser; }
            set { typeAntiAliaser = value; }
        }

        public const string defaultTypeSpecifier = "@class";

        private string typeSpecifier = defaultTypeSpecifier;
        public string TypeSpecifier
        {
            get { return typeSpecifier; }
            set { this.typeSpecifier = value; }
        }

        public class UnknownFunctionException : Exception
        {
            public UnknownFunctionException(string message)
                : base(message)
            {

            }
        }

        public class NoClueForTypeException : Exception
        {
            public NoClueForTypeException(string message)
                : base(message)
            {

            }
        }

        public class UnterminatedObjectException : Exception
        {
            public UnterminatedObjectException(string message)
                : base(message)
            {

            }
        }

        public class OverTerminatedObjectException : Exception
        {
            public OverTerminatedObjectException(string message)
                : base(message)
            {

            }
        }

        /**
         * parameters:
         * target propertyName valueAsString - return true if success
         **/
        Func<object, string, object, DeserializationListener, string> setValueinObject = setValueinObjectAsMember;

     /*   public void setToUseFields()
        {
            //setValueinObject = setValueinObjectAsField;
            setValueinObject = setValueinObjectAsMember;
        }

        public void setToUseProperties()
        {
            //setValueinObject = setValueinObjectAsProperty;
            setValueinObject = setValueinObjectAsMember;
        }
        */
        static bool setValueAsMap(object target, string propertyName, object propertyValue)
        {
            if (!typeof(IDictionaryNonGeneric).IsAssignableFrom(target.GetType())) return false;
            IDictionaryNonGeneric dict = (IDictionaryNonGeneric)target;
            Type []gTypes = target.GetType().GetGenericArguments();
            object key = propertyName;
            if (gTypes.Length==2 && gTypes[0]!=typeof(object))
            {
                key = Convert.ChangeType(key, gTypes[0]);
            }
            dict.Add(key, propertyValue);
            return true;
        }

        static Func<object, string, object, DeserializationListener, string> setValueinObjectAsMember = (target, propertyName, propertyValue, listener) =>
        {
            if (setValueAsMap(target, propertyName, propertyValue))
            {
                return null;
            }
            Type targetType = target.GetType();
            MemberInfo[] mis = targetType.GetMember(propertyName);
            MemberInfo memberInfo=null;
            bool isField=false;

            Type fieldType = null; ;
            for (int done = 0; done < mis.Count(); done++)
            {
                MemberInfo mi = mis[done];
                MemberTypes memberType = mi.MemberType;
                if (memberType==MemberTypes.Field) 
                   {
                       isField = true;
                       memberInfo = mi;
                       fieldType = ((FieldInfo)memberInfo).FieldType;
                       break;
                   }
                if (memberType == MemberTypes.Property)
                {
                    isField = false;
                    memberInfo = mi;
                    fieldType = ((PropertyInfo)memberInfo).PropertyType;
                    break;
                }
            }

            if (memberInfo == null)
                {
                    return "cant find field or property " + propertyName + " in " + targetType.FullName;
                }

            Type underlyingType = Nullable.GetUnderlyingType(fieldType);
            if (underlyingType == null)
                underlyingType = fieldType;
            object value = null;
            if (propertyValue == null) value = null;
            else if (!convertType(ref value, underlyingType, propertyValue))
                value = Convert.ChangeType(propertyValue, underlyingType);
            if (isField)
                ((FieldInfo)memberInfo).SetValue(target, value);
            else if (memberInfo is PropertyInfo)
                ((PropertyInfo)memberInfo).SetValue(target, value);
                       
            if (listener != null) listener.onSetValue(target, propertyName, value);
            return null;
        };


        // todo decide whether to remove this - its not in use but its propbably quicker than setValueinObjectAsMember
        //
        static Func<object, string, object, DeserializationListener, string> setValueinObjectAsField = (target, propertyName, propertyValue, listener) =>
            {                
                if (setValueAsMap(target, propertyName, propertyValue))
                {
                    return null;
                }
                Type targetType = target.GetType();
                FieldInfo fieldInfo = targetType.GetField(propertyName);
                if (fieldInfo == null)
                {
                    return "cant find field " + propertyName + " in " + targetType.FullName;
                }
                Type underlyingType = Nullable.GetUnderlyingType(fieldInfo.FieldType);
                if (underlyingType == null)
                    underlyingType = fieldInfo.FieldType;
                else
                {
                    System.Console.WriteLine("here");
                }
                    object value = null;
                    if (propertyValue == null) value = null;
                    else if (!convertType(ref value, underlyingType, propertyValue))
                        value = Convert.ChangeType(propertyValue, underlyingType);
                fieldInfo.SetValue(target, value);
                if (listener != null) listener.onSetValue(target, propertyName, value); 
                return null;
            };

        static bool isListType(Type type)
        {
            Type[] genericArguments = type.GetGenericArguments();
            return typeof(IListNonGeneric).IsAssignableFrom(type);
        }

        static bool convertType(ref object value, Type type, object inValue)
        {
            bool bIsListType =  isListType(type);
            if (bIsListType && type.IsArray)
            {
                IListNonGeneric iInValue = (IListNonGeneric) inValue;
                Array arr = Array.CreateInstance(type.GetElementType(), iInValue.Count);
                for (int done = 0; done < iInValue.Count; done++ )
                {
                    arr.SetValue(iInValue[done], done);
                }
                value = arr;
                return true;
            }
            if (bIsListType)
            {
                value = Json2Object.newInstance(type);

                object[] arrInValue = (object[])inValue;

                MethodInfo mi = type.GetMethod("Add");
                
                for (int done = 0; done < arrInValue.Length; done++)
                {
                    object indexValue = arrInValue[done];
                    ParameterInfo pi = mi.GetParameters()[0];

                    if (indexValue!=null && !pi.ParameterType.IsAssignableFrom(indexValue.GetType()))
                    {
                         indexValue = Convert.ChangeType(indexValue, pi.ParameterType);
                    }
                    mi.Invoke(value, new Object[] { indexValue });
                }
                return true;
            }
            return false;
        }

        Stack<StackFrame> objects;
        Object lastObject;

        Type typeHint;
        string json;

        static object newInstance(Type type)
        {
            ConstructorInfo co = type.GetConstructor(new Type[0]);
            if (co==null)
            {
                throw new Exception("no default constructor for " + type.FullName);
            }
            object newObject = co.Invoke(new Object[0]);
            return newObject;
        }

        public bool objectTypePending = false;
        public void JsonStartObject(string propertyName, int pos)
        {
             // if the typeHint is not null create theobject 
            // otherwise defer until a class attribute property is available
            //todo restruct typeHint usage based on whetherits thetop object
            ObjectFrame objectHolder = new ObjectFrame();
            objectHolder.propertyName = propertyName;
            objects.Push(objectHolder);
            if (typeHint != null)
            {
                object newObject = newInstance(typeHint);
                objectHolder.theObject = newObject;
                listener.onCreateObject(newObject);
                objectTypePending = false;
                typeHint = null;
                return;
            }
            objectTypePending = true;

        }

        public void JsonLeaf(string propertyName, string value, bool isQuoted)
        {
            if (objectTypePending)
            {
                if (!propertyName.Equals(TypeSpecifier))
                {
                    string message = String.Format("encountered a property in subobject \"{0}\" at but dont know parent object type yet ! json=={1}", propertyName, json);
                    NoClueForTypeException ex = new NoClueForTypeException(message);
                    throw ex;
                }
                else
                {
                    Type attributeTypeHint = TypeAntiAliaser(value);

                    if (attributeTypeHint == null)
                    {
                        string message = String.Format("cant load type: \"{0}\" in json:{1}", value, json);
                        NoClueForTypeException ex = new NoClueForTypeException(message);
                        throw ex;
                    }

                    object newObject =  newInstance(attributeTypeHint);
                    ObjectFrame objectHolder = (ObjectFrame)objects.Peek();
                    objectHolder.theObject = newObject;
                    listener.onCreateObject(newObject);
                    objectTypePending = false;
                    return;
                }
            }

            if (objectTypePending || objects.Count == 0)
            {
                string message = string.Format("unable to set property {0} with value {1} because type is not found yet ", propertyName, value);
                throw new Exception(message);
            }

            string strError = null;

            if (TypeSpecifier != null && propertyName != null && propertyName.Equals(TypeSpecifier))
            {
                // todo raise a warning here or throw an exception in strict mode
            }
            else
            {
                objects.Peek().submitLeafToStack(propertyName, value, setValueinObject, listener);
            }

            if (strError != null) throw new Exception(strError);

        }
        public void JsonEndObject(int pos)
        {
            if (objectTypePending)
            {
                string strMessage = String.Format("unable to determine type of object terminated at pos {0} in {1}", pos, json);
                throw new NoClueForTypeException(strMessage);
            }
            if (objects.Count < 1)
            {
                string strMessage = String.Format("too many object terminations at pos {0} in {1}", pos, json);
                throw new OverTerminatedObjectException(strMessage);
            }
            lastObject = objects.Pop();
            ObjectFrame lastFrame =  lastObject as ObjectFrame;
            if (lastFrame==null)
                {
                    throw new UnterminatedObjectException("unexpected non Object frame " + lastFrame.GetType().Name);
                }
            listener.onEndObject(lastFrame.theObject);
            if (objects.Count() > 0)
                objects.Peek().submitObjectToStack(((ObjectFrame)lastObject).propertyName, ((ObjectFrame)lastObject).theObject, setValueinObject, listener);

        }

        public void JsonStartFunction(string functionName, int pos, string propertyName)
        {
            FunctionHolder holder = new FunctionHolder();
            holder.paramList = new List<object>();
            holder.functionName = functionName;
            holder.propertyName = propertyName;
            objects.Push(holder);
        }

        Dictionary<string, Func<List<object>, object>> functionNameToFunction = new Dictionary<string, Func<List<object>, object>>();

        public void addFunction(string name, Func<List<object>, object> function)
        {
            functionNameToFunction[name] = function;
        }

        public void JsonEndFunction(int pos)
        {
            FunctionHolder holder = (FunctionHolder)objects.Pop();
            // call the function handler !
            if (!functionNameToFunction.ContainsKey(holder.functionName))
            {
                throw new UnknownFunctionException("unknown function: \"" + holder.functionName + "\"");
            }
            Func<List<object>, object> f = functionNameToFunction[holder.functionName];
            object value = f(holder.paramList);
            objects.Peek().submitObjectToStack(holder.propertyName, value, setValueinObject, listener);
            //submitObjectToStack(objects.Peek(), holder.propertyName, value);
        }

        public void JsonStartArray(string propertyName, int pos)
        {
            ArrayFrame holder = new ArrayFrame();
            holder.objectList = new List<Object>();
            holder.propertyName = propertyName;
            objects.Push(holder);
        }

        //todo - check if can be subsumed into submit to stack - to avoid casting
        public void JsonEndArray(int pos)
        {
            ArrayFrame holder = (ArrayFrame)objects.Pop();
            object[] value = new object[holder.objectList.Count];
            for (int done = 0; done < holder.objectList.Count; done++)
            {
                value[done] = holder.objectList[done];
            }
            StackFrame context = objects.Peek();

            if (context is FunctionHolder)
            {
                ((FunctionHolder)context).paramList.Add(value);
            }
            else if (context is ObjectFrame)
            {
                string strError = setValueinObject(((ObjectFrame)context).theObject, holder.propertyName, value, listener);
                if (strError != null) throw new Exception("failed to set value because " + strError);                   
            }
            else 
            {
                throw new Exception("unexpected StackFrame: " + context.GetType().FullName + " for JsonEndArray pos=" +pos);
            }

        }

        /**
         * the current type will be inferred either for the navigation context or the first type !
         * otherwise a full parse will be necessary
         */
        public Object toObject(string json, Type typeHint = null)
        {
            //todo revise this - it makes the serializer single threaded !
            this.json = json;
            if (setValueinObject == null)
            {
                throw new Exception("setValueInObject unset !");
            }

            JSONExplorerImpl impl = new JSONExplorerImpl();
            this.typeHint = typeHint;
            this.objects = new Stack<StackFrame>();
            this.lastObject = null;
            impl.explore(json, this);
            if (lastObject is ObjectFrame) lastObject = ((ObjectFrame)lastObject).theObject;
            return lastObject;
        }
    }
}
