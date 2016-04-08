using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldInfo = System.Reflection.FieldInfo;
using BindingFlags = System.Reflection.BindingFlags;

namespace com.houseelectrics.serializer
{
    public class FieldReflectionNodeExpander : NodeExpander
    {
        bool isPropertyAutoField(FieldInfo fi)
        {
            return fi.Name.IndexOf("k__BackingField") >= 0;
        }
        public int expand(object o, OnChildNode listener)
        {
            if (o == null) { return 0; }
            if (o.GetType() == (typeof(DateTime)))
            {
                Int64 ewokTime = NodeExpanderConstants.extractEpochTimeMillis((DateTime)o);
                listener(o, NodeExpanderConstants.unixEpochTimeMillisFieldName, ewokTime);
                return 1;
            }

            Type theclass = o.GetType();
            FieldInfo[] fis = theclass.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Default);
            foreach (FieldInfo fi in fis)
            {
                // ignore property backing fields!
                if (isPropertyAutoField(fi)) 
                {
                    continue;
                }
                Object value = fi.GetValue(o);                
                if (fi.FieldType.IsEnum)
                {
                    value = Enum.GetName(fi.FieldType, value);
                }

                listener(o, fi.Name, value);
            }
            return fis.Length;
        }
    }
}
