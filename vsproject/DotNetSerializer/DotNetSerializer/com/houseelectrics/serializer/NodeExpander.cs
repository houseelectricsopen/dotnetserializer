using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public delegate void OnChildNode(Object from, String name, Object to);

    public static class NodeExpanderConstants
    {
        // implement from json support + java support
        public const String unixEpochTimeMillisFieldName = "unixEpochTimeMillis";
        public const String unixEpochTimeMillisPropertyName = "UnixEpochTimeMillis";
        public static Int64 extractEpochTimeMillis(DateTime o)
        {
            Int64 ewokTime = (o.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            return ewokTime;
        }
    }

    public interface NodeExpander
    {
        int expand(Object o, OnChildNode listener);
    }
}
