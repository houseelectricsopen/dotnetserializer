using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldInfo = System.Reflection.FieldInfo;
using BindingFlags = System.Reflection.BindingFlags;
using CustomAttributeData = System.Reflection.CustomAttributeData;
using DataContract = System.Runtime.Serialization.DataContractAttribute;
using DataMember = System.Runtime.Serialization.DataMemberAttribute;
using PropertyInfo = System.Reflection.PropertyInfo;

namespace com.houseelectrics.serializer.datacontract
{
    public class DataContractFieldNodeExpander : NodeExpander
    {
        public int expand(object o, OnChildNode listener)
        {
            if (o == null) { return 0; }
            Type theclass = o.GetType();
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Default;
            FieldInfo[] fis = theclass.GetFields( bf );
            List<DataMember> dataMembers = new List<DataMember>();
            Dictionary<string, FieldInfo> dataMemberName2FieldInfo = new Dictionary<string, FieldInfo>();

            foreach (FieldInfo fi in fis)
            {
                 object[] customAttributes =fi.GetCustomAttributes(typeof(DataMember), true);
                if (customAttributes.Length == 0) continue;
                DataMember dm = (DataMember)customAttributes[0];                

                if (dm.Name == null) dm.Name = fi.Name;
                dataMembers.Add(dm);
                dataMemberName2FieldInfo[dm.Name] = fi;                    
            }

            Dictionary<string, PropertyInfo> dataMemberName2PropertyInfo = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] pis = theclass.GetProperties(bf);
            foreach (PropertyInfo pi in pis)
            {
                object[] customAttributes = pi.GetCustomAttributes(typeof(DataMember), true);
                if (customAttributes.Length == 0) continue;
                DataMember dm = (DataMember)customAttributes[0];

                if (dm.Name == null) dm.Name = pi.Name;
                dataMembers.Add(dm);
                dataMemberName2PropertyInfo[dm.Name] = pi;                    
            }

            Comparison<DataMember> sorter = (dm, dm2) => 
               {
                   if ( dm.Order == dm2.Order)  return 0;
                   else return  dm.Order>dm2.Order ? 1 : -1;
                   };
            dataMembers.Sort(sorter);
            for ( int done=0 ; done<dataMembers.Count; done++)
            {
                DataMember dm = dataMembers[done];                
                Object value;
                if (dataMemberName2FieldInfo.ContainsKey(dm.Name))
                {
                    FieldInfo fi = dataMemberName2FieldInfo[dm.Name];
                    value = fi.GetValue(o);
                }
                else
                {
                    PropertyInfo pi = dataMemberName2PropertyInfo[dm.Name];
                    value = pi.GetValue(o);
                }
                listener(o, dm.Name, value);                
            }

            return dataMembers.Count;
        }
    }
   
}
