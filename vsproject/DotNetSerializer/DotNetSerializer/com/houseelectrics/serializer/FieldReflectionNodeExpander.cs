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

                listener(o, fi.Name, value);
            }
            return fis.Length;
        }
    }
}
