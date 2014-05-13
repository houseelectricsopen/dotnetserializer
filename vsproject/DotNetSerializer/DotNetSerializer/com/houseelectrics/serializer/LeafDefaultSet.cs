using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public delegate string TypeAliaser(Type type);
    public delegate Type TypeAntiAliaser(string alias);  

    public static class TypeAliaserUtils
    {
        public static TypeAntiAliaser createSimpleTypeAntiAliaser()
        {
            TypeAntiAliaser result = (string value) =>
                {
                    IEnumerable<Type> typeHints = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(value)).Where(t => t != null);
                    Type type = typeHints.Count() == 0 ? null : typeHints.First();
                    return type;
                };
            return result;
        }


        public static TypeAliaser createNumericTypeNameAliaser()
        {
            Dictionary<Type, string> type2Alias = new Dictionary<Type, string>();
            // try a different  
            TypeAliaser aliaser = delegate(Type type)
            {
                string alias;
                if (!type2Alias.ContainsKey(type))
                {
                    alias = "_" + type2Alias.Keys.Count;
                    type2Alias[type] = alias;
                }
                else
                {
                    alias = type2Alias[type];
                }
                return alias;
            };
            return aliaser;
        }
    }

    /*
     * Not intended for serialising 
    */
    public class LeafDefaultSet
    {
        //private TypeAliaser typeAliaser;
        //public TypeAliaser TypeAliaser { get { return typeAliaser; } }

        Dictionary<Type, Defaults4Class> type2Defaults = new Dictionary<Type,Defaults4Class>();
        public Dictionary<Type, Defaults4Class> Type2Defaults { get { return type2Defaults; } set { type2Defaults = value; } }

        public Dictionary<string, Defaults4Class> getAlias2Defaults(TypeAliaser typeAliaser)
        {
            Dictionary<string, Defaults4Class> result = new Dictionary<string, Defaults4Class>();
            foreach (Type type in Type2Defaults.Keys)
            {
                string alias = typeAliaser(type);
                result[alias] = Type2Defaults[type];
            }
            return result;
        }

        public LeafDefaultSet Add(LeafDefaultSet right)
        {
            foreach (Type t in  right.type2Defaults.Keys)
            {
                this.type2Defaults[t] = right.type2Defaults[t];
            }
            return this;
        }

    }
}
