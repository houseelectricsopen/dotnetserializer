using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{
    /**
     * called to indicating imminent downward movement
     */
    public delegate bool MoveAway(Object from, String propertyName, Object to, bool isIndexed, int ?index=0);
    public delegate void MoveBack(Object from, String propertyName, Object to, bool isIndexed);
    public delegate void OnLeaf(Object from, String propertyName, Object to, int? index = 0);
    /**
     * explore the root object supplied - the root is considered be at the top -
     * callback <b>down</b> is called prior to moving down into a related object.
     * Downward movement can be vetoed by down returning false
     */
    public interface Explorer
    {
        void explore(Object root, MoveAway down, MoveBack up, OnLeaf leaf);
        NodeExpander NodeExpander { get; set; }

    }
}
