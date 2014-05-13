using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public interface JsonMapping
    {
        Json2Object getJson2Object();
        Object2Json getObject2Json();
    }
}
