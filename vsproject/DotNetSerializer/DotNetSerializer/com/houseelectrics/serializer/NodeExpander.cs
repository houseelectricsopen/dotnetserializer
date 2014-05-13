using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    public delegate void OnChildNode(Object from, String name, Object to);

    public interface NodeExpander
    {
        int expand(Object o, OnChildNode listener);

    }
}
