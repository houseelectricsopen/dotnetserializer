using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.houseelectrics.serializer;

namespace com.houseelectrics.serializer.path
{
    public class InstancePathElement
    {
        public bool IsIndexed { get; set; }
    }

    public class NamePathElement : InstancePathElement
        {
        public override string ToString()
        {
            return Name;
        }
            public string Name { get; set; }
        }
    public class IndexPathElement : InstancePathElement
        {
            public int Index { get; set; }
            public override string ToString()
            {
                return "[" + Index + "]";
            }    
    }

    public class PathUtil
    {
        public List<InstancePathElement> findSingleObjectPath(object root, object object2Find)
        {
            bool found=false;
            ObjectExplorerImpl explorer = new ObjectExplorerImpl();
            Stack<InstancePathElement> currentPath = new Stack<InstancePathElement>();

            MoveAway down = delegate(Object from, string propertyName, Object to, bool isIndexed, int? index)
{
    if (found) return false;
    InstancePathElement element;
    bool parentIsIndexed = currentPath.Count > 0 && currentPath.Peek().IsIndexed;
    if (parentIsIndexed)
    {
        IndexPathElement ielement = new IndexPathElement();
        ielement.Index = (int)index;
        element = ielement;
    }
    else
    {
        NamePathElement nElement = new NamePathElement();
        nElement.Name = propertyName;
        element = nElement;
    }
    element.IsIndexed = isIndexed;
    currentPath.Push(element);
    found = object2Find == to;
    return (!found);
};
MoveBack up = (from, propertyName, to, isIndexed) =>
{
    if (!found) currentPath.Pop();
};

OnLeaf onLeaf = (from, propertyName, to, index) =>
    {
    };

    PropertyReflectionNodeExpander expander = new PropertyReflectionNodeExpander();
    expander.ExcludeReadOnlyProperties = true;
    explorer.NodeExpander = expander;

    explorer.explore(root, down, up, onLeaf);
    List<InstancePathElement> result = null; 
            if (found)
            {
                result = new List<InstancePathElement>(currentPath.ToArray().Reverse());
                // remove the first element because it is to the root
                result.RemoveAt(0);
            }
            return result;
        }

        public string ToString(List<InstancePathElement> path)
{
    StringBuilder sb = new StringBuilder();
    for (int done =0 ; done<path.Count; done++)
    {
        if (done != 0) sb.Append('.');
        sb.Append(path[done]);
    }
    return sb.ToString();
}
    
    }



}
