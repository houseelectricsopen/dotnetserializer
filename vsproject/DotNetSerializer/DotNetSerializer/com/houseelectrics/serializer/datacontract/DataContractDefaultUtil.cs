using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeafDefaultSet = com.houseelectrics.serializer.LeafDefaultSet;
using FieldInfo = System.Reflection.FieldInfo;
using DataMemberAttribute = System.Runtime.Serialization.DataMemberAttribute;

namespace com.houseelectrics.serializer.datacontract
{
    public class DataContractDefaultUtil
    {
    private static HashSet<Type> NumberTypes =
        new HashSet<Type>
    {
        typeof(Int16),
        typeof(Int32),
        typeof(Int64),
        typeof(UInt16),
        typeof(UInt32),
        typeof(UInt64),
        typeof(Double),
        typeof(double),
        typeof(Decimal),
        typeof(decimal),
        typeof(Single),
        typeof(float)
    };


      public static bool isDefaultLeafValue(object from, string propertyName, object leafValue, LeafDefaultSet LeafDefaultSet)
        {
          // check the attribute
            FieldInfo fieldInfo = from.GetType().GetField(propertyName);
            if (fieldInfo == null) return false;
            object[] dmas = fieldInfo.GetCustomAttributes(typeof(DataMemberAttribute), true);
            //check whether we are emitting default values   
            if (dmas.Length == 0 || ((DataMemberAttribute)dmas[0]).EmitDefaultValue) return false;

            if (leafValue == null) return true;
            Type leafType=leafValue.GetType();
            if (Nullable.GetUnderlyingType(leafType)!=null)
                {
                leafType=Nullable.GetUnderlyingType(leafType);
                }
            if (NumberTypes.Contains(leafType))
                return leafValue.Equals((int)0) || leafValue.Equals((double)0.0) || leafValue.Equals((uint)0) || leafValue.Equals((float)0);
            if (leafType == typeof(bool) || leafType == typeof(Boolean)) return !((bool)leafValue);
            return false;
        }


    }
}
