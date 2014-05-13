using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public class PropertyReflectionNodeExpander : NodeExpander
    {
        public int expand(object o, OnChildNode onChildNode)
        {
            PropertyInfo []pis= o.GetType().GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                Object value = null;
                try
                {
                    value = pi.GetValue(o);
                }
                catch (Exception ex)
                {
                    Exception exx = new Exception("failed to get value for property " + pi.Name + " declared in "+ pi.DeclaringType, ex);
                    throw exx;
                }
                
                String name = Char.ToLower(pi.Name[0]) + pi.Name.Substring(1);
                name = pi.Name;
                onChildNode(o, name, value);
            }
            return pis.Length;
        }
    }
}
