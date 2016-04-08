using System;
using System.Reflection;


namespace com.houseelectrics.serializer
{
    public class PropertyReflectionNodeExpander : NodeExpander
    {
        private bool excludeReadOnlyProperties = false;
        public bool ExcludeReadOnlyProperties { get { return excludeReadOnlyProperties; } set { this.excludeReadOnlyProperties = value; } }
        public int expand(object o, OnChildNode onChildNode)
        {
            //treat dates differently !
            if (o.GetType() == (typeof(DateTime)))
            {
                if (o.GetType() == (typeof(DateTime)))
                {
                    Int64 ewokTime = NodeExpanderConstants.extractEpochTimeMillis((DateTime)o);
                    onChildNode(o, NodeExpanderConstants.unixEpochTimeMillisPropertyName, ewokTime);
                    return 1;
                }
            }
            PropertyInfo []pis= o.GetType().GetProperties();
            int setAbleCount = 0;
            foreach (PropertyInfo pi in pis)
            {
                if (ExcludeReadOnlyProperties && pi.GetSetMethod() == null) { continue; }
                Object value = null;
                try
                {
                    value = pi.GetValue(o);
                    if (pi.PropertyType.IsEnum)
                    {
                        value = Enum.GetName(pi.PropertyType, value);
                    }
                    setAbleCount++;
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
            return setAbleCount;
        }
    }
}
