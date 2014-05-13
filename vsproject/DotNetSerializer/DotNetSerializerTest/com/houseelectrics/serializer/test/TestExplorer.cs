using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace com.houseelectrics.serializer.test
{
    public class TestExplorer
    {
        
        [Test]
        public void testExploration()
        {
            
            NodeExpander nodeExpander = new PropertyReflectionNodeExpander();
            TestData testData = new TestData();
            ObjectExplorerImpl explorer = new ObjectExplorerImpl();
            explorer.NodeExpander = nodeExpander;
            Object valueFound = null;
            int depth = 0;
            int? depthFound=null;
            MoveBack up = delegate(Object from, String propertyName, Object to, bool isIndexed)
                 {
                     depth--;
                 };
            MoveAway down = delegate(Object from, String propertyName, Object to, bool isIndexed, int ?index)
            {
                depth++;
                return true;
            };
            OnLeaf leaf = delegate(Object from, String propertyName, Object to, int? index)
            {
                if (propertyName!=null && propertyName.Equals("Greeting") )
                {
                    valueFound = to;
                    depthFound = depth + 1;
                }

            };
            explorer.explore(testData, down, up, leaf);
            Assert.AreEqual(testData.TheSub.TheSubSub.Greeting, valueFound, "expected greeting ");
            Assert.AreEqual(4, depthFound, "expected depth");

        }
    }
}
