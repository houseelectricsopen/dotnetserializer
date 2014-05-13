using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer.test
{
    public class TestTreeNode
    {
        public string name;
        public String Name { get { return name; } set { name = value; } } 
        public List<TestTreeNode> branches = new List<TestTreeNode>();
        public List<TestTreeNode> Branches { get { return branches; } }

        /*creates a tree structure of specified depth and width*/
        public static TestTreeNode createTestHierarchy(int requiredDepth, int branchesPerParent)
        {
            List<TestTreeNode> nodes2Expand;
            nodes2Expand = new List<TestTreeNode>();
            TestTreeNode rootNode = new TestTreeNode();
            nodes2Expand.Add(rootNode);
            for (int level = 0; level < requiredDepth; level++)
            {
                List<TestTreeNode> nextNodesToExpand = new List<TestTreeNode>();
                for (int done = 0; done < nodes2Expand.Count; done++)
                {
                    TestTreeNode node = nodes2Expand[done];
                    for (int bdone = 0; bdone < branchesPerParent; bdone++)
                    {
                        TestTreeNode subNode = new TestTreeNode();
                        subNode.Name = "level" + level + "_index" + bdone;
                        node.Branches.Add(subNode);
                        nextNodesToExpand.Add(subNode);
                    }
                }
                nodes2Expand = nextNodesToExpand;
            }
            return rootNode;
        }    
    }

    public class TestTreeLeaf
    {
        string name;
        public String Name { get { return name; } set { name = value; } } 
    }



}
