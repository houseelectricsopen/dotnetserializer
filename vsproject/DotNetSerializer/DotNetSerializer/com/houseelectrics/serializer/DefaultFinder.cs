using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer
{

    public class DefaultFinder
    {
        //, TypeAliaser aliaser
        public LeafDefaultSet getDefaultsForAllLinkedObjects(Object root, NodeExpander nodeExpander)
        {
            LeafDefaultSet result = new LeafDefaultSet();

            ObjectExplorerImpl explorer = new ObjectExplorerImpl();
                MoveAway down = ( from,  propertyName,  to,  isIndexed, index) =>
                {
                    return (isIndexed || (to != null && !result.Type2Defaults.ContainsKey(to.GetType())));
                };
             MoveBack  up = (from, propertyName, to, isIndexed) =>
                {

                };

             OnLeaf leaf = (from, propertyName, to, index) =>
                {
                    Defaults4Class defaults = null;
                    if (from==null) {return;}
                    Type fromType = from.GetType();
                    if (!result.Type2Defaults.ContainsKey(fromType))
                    {
                        defaults = new Defaults4Class();
                        defaults.FullClassName = fromType.FullName;
                        result.Type2Defaults[fromType] = defaults;
                    }
                    else
                    {
                        defaults = result.Type2Defaults[fromType];
                    }
                    defaults.PropertyName2DefaultValue[propertyName] = to;
                };

             explorer.NodeExpander = nodeExpander;
                explorer.explore(root, down, up, leaf);
                return result;
        }
    }
}
