using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test = NUnit.Framework.TestAttribute;
using Assert = NUnit.Framework.Assert;

namespace com.houseelectrics.serializer.test
{
    public class TestReflectiveNodeExpander
    {
        public class Data { private String greeting = "hi"; public String Greeting { get { return greeting; } } }
        [Test]
        public void simpleExpandByProperty()
        {
            simpleExpand(new PropertyReflectionNodeExpander(), "Greeting");
        }

        [Test]
        public void simpleExpandByField()
        {
            simpleExpand(new FieldReflectionNodeExpander(), "greeting");
        }

        public void simpleExpand(NodeExpander nodeExpander, string propname)
        {
            Data data = new Data();
            bool foundhi = false;
            OnChildNode l =
                delegate(Object from, String name, Object to)
                  {
                      if (name.Equals(propname) && to.Equals(data.Greeting))
                      {
                          foundhi = true;
                      }
                  };
            nodeExpander.expand(data, l);
            Assert.AreEqual(true, foundhi, "expected to find greeting");
        }

       
    }
}
