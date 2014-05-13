using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract = System.Runtime.Serialization.DataContractAttribute;
using DataMember = System.Runtime.Serialization.DataMemberAttribute;
using SerializationException = System.Runtime.Serialization.SerializationException;
using FieldInfo = System.Reflection.FieldInfo;
using BindingFlags = System.Reflection.BindingFlags;

namespace com.houseelectrics.serializer.datacontract
{
    public class DataContractMandatoryEnforcementListener : DeserializationListener
    {
        class ObjectComparer : System.Collections.Generic.IEqualityComparer<object>
        {
            new public bool Equals(object x, object y)
            {
                return Object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }

        Dictionary<object, Dictionary<string, object>> object2PropertyValues =
              new Dictionary<object, Dictionary<string, object>>(new ObjectComparer());

        // expose internals for testing !
        public Dictionary<object, Dictionary<string, object>>  Object2PropertyValues { get { return object2PropertyValues; } }

        public void onCreateObject(object o)
        {
            if (object2PropertyValues.ContainsKey(o))
            {
                throw new SerializationException("on create object called for already existant object");
            }
            object2PropertyValues[o] = new Dictionary<string, object>();
        }
        public void onEndObject(object o)
        {
            Type objectType = o.GetType();
            Dictionary<string, object> propertyValues = object2PropertyValues[o];
            if (propertyValues==null)
            {
                throw new SerializationException("unknown object terminated: " + o.GetType().Name + " " + o.ToString());
            }
            // for each field if its mandatory check it exists
            foreach (FieldInfo fi in objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Default))
            {
                object []memberinfo = fi.GetCustomAttributes(typeof(DataMember), true);
                if (memberinfo.Count()>1)
                {
                    throw new SerializationException("field " + objectType.Name + "." + fi.Name + " found " + memberinfo.Count() + " DataMember s!" );
                }
                else if (memberinfo.Count()==1)
                {
                    DataMember dm = (DataMember)memberinfo[0];
                    
                    if (dm.IsRequired)
                    {
                        if (!propertyValues.ContainsKey(fi.Name))
                         {
                             throw new SerializationException("mandatory (DataMember.IsRequired) field " + objectType.Name + "." + fi.Name + " not specified ");
                         }
                    }
                }
            }

        }
        public void onSetValue(object o, string propertyName, object value)
        {
            if (!object2PropertyValues.ContainsKey(o))
            {
                throw new SerializationException("set value called for unkown object");
            }
            object2PropertyValues[o][propertyName] = value;
        }
    }
}
