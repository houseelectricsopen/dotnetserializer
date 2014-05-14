using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.houseelectrics.serializer;

namespace DotnetSerializerMVC5
{
    public class JsonConfig
    {
        public static Object2Json DefaultObject2Json(Object testData)
        {
            Object2Json Object2Json;
            // Code that runs on application startup
            // create global serialiser here
            Object2Json = new Object2Json();
            Object2Json.UseReferences = true;
            Object2Json.NodeExpander = new FieldReflectionNodeExpander();

            // add in defaults
            Object2Json.LeafDefaultSet = new LeafDefaultSet();
            Object2Json.TypeAliaser = TypeAliaserUtils.createNumericTypeNameAliaser();

            DefaultFinder df = new DefaultFinder();
            //TypeAliaserUtils
            LeafDefaultSet lds = df.getDefaultsForAllLinkedObjects(testData, Object2Json.NodeExpander);
            Object2Json.LeafDefaultSet.Add(lds);
            return Object2Json;
        }
    }
}