using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodInfo = System.Reflection.MethodInfo;
using PropertyInfo = System.Reflection.PropertyInfo;
using ICollection = System.Collections.ICollection;
using IEnumerable = System.Collections.IEnumerable;
using IEnumerator = System.Collections.IEnumerator;
using IDictionaryNonGeneric = System.Collections.IDictionary;
namespace com.houseelectrics.serializer
{
    public class ObjectExplorerImpl : Explorer
    {
        public NodeExpander NodeExpander { get; set; }
        /** carries out exploration
         if o is a leaf leaf is called         
         if o is a standard object down is called the item is expanded anf then up is called
         if o is a dictionary or indexed object (e.g. array) every contained object is treated as a
         leaf or standard object
         * 
         * determination of leaf is in isLeaf and is based on c# properties
         * 
         * determination if indexed type is in getEnumeratorIfIndexedType
         * 
         * determination of map type is in toDictionary
        **/
        public void explore(object o, MoveAway down, MoveBack up, OnLeaf leaf)
        {
            if (NodeExpander == null) throw new Exception("explore is not possible unless a node expander is specified");
            OnChildNode onChildNode = null;
            onChildNode = delegate(Object from, String name, Object to)
             {
                 IEnumerator en = null;
                 Type toType= to==null?null:to.GetType();
                 IDictionaryNonGeneric asDictionary=null;
                 if (isLeaf(to, toType))
                 {
                     leaf(from, name, to);
                 }
                 else if (null!=(asDictionary=toDictionary(to, toType)))
                 {
                     bool doExpand =
                     down(from, name, to, false);
                     en = asDictionary.Keys.GetEnumerator();
                     for (int done = 0; doExpand && en.MoveNext(); done++)
                     {
                         Object oKey = en.Current;
                         string strKey = dictionaryKey2String(oKey); 
                         Object oVal = asDictionary[oKey];
                         Type oValType = oVal == null ? null : oVal.GetType();
                         if (isLeaf(oVal, oValType))
                             leaf(to, strKey, oVal);
                         else
                         {
                             bool doSubExpand = down(to, strKey, oVal, false, done);
                             if (doSubExpand) NodeExpander.expand(oVal, onChildNode);
                             up(from, strKey, oVal, false);
                         }
                     }
                     up(from, name, to, false);
                 }
                 else if (null!=(en=getEnumeratorIfIndexedType(to)))
                 {
                     bool doExpand =
                     down(from, name, to, true);

                     for  (int done=0;doExpand && en.MoveNext();done++)
                     {
                         Object oVal = en.Current;
                         if (isLeaf(oVal, oVal==null?null:oVal.GetType()))
                             leaf(to, null, oVal);
                         else
                         {
                             bool doSubExpand = down(null, null, oVal, false, done);
                             if (doSubExpand) NodeExpander.expand(oVal, onChildNode);
                             up(null, null, oVal, false);
                         }
                     }
                     up(from, name, to, true);
                 }
                 else
                 {
                     bool doExpand = down(from, name, to, false);
                     if (doExpand) NodeExpander.expand(to, onChildNode);
                     up(from, name, to, false);
                 }
             };
            onChildNode(null, null, o);
        }

        IEnumerator getEnumeratorIfIndexedType(object o)
        {
            if (isArray(o, o.GetType()))
            {
                Array arrTo = (Array)o;
                return arrTo.GetEnumerator();
            }
            else if (typeof(ICollection).IsAssignableFrom(o.GetType())
            || typeof(ICollection<>).IsAssignableFrom(o.GetType()))
            {
               return ((IEnumerable)o).GetEnumerator();
            }
            else return null;
        }

        private bool isLeaf(Object o, Type theclass)
        {
            if (o == null) { return true; }
            if (theclass == typeof(decimal) || theclass == typeof(Decimal)) return true;
            if (theclass.IsPrimitive) return true;
            if (theclass==typeof (String)) return true;
            return false;
        }

        private bool isArray(Object o, Type theclass)
        {
            if (o == null) { return false; }
            return theclass.IsArray;
        }

        private bool isList(Object o, Type theclass)
        {
            return (theclass.IsGenericType &&
                 (theclass.GetGenericTypeDefinition() == typeof(List<>)));
        }

        private IDictionaryNonGeneric toDictionary(Object o, Type theclass)
        {
            return o != null && (typeof(IDictionaryNonGeneric)).IsAssignableFrom(theclass) ?
                (IDictionaryNonGeneric)o : null;
         }
        string dictionaryKey2String(object key)
        {
            string str = key.ToString();
            return str;
        }


    }
}
