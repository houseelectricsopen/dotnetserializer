using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public class DefaultJsonMapping : JsonMapping
    {
        private Json2Object json2Object;
        private Object2Json object2Json;
        public DefaultJsonMapping()
        {
            json2Object = new Json2Object();
            //json2Object.setToUseFields();
            object2Json = new Object2Json();
            object2Json.NodeExpander = new FieldReflectionNodeExpander();
            object2Json.IndentSize = 2;

            //todo json 2 object should understand TypeAliaser
            object2Json.TypeAliaser = (t) => { return t.FullName; };
            object2Json.TypeAliasProperty = json2Object.TypeSpecifier;
        }

        public Json2Object getJson2Object()
        {
            return json2Object;
        }

        public Object2Json getObject2Json()
        {
            return object2Json;
        }
    }
}
