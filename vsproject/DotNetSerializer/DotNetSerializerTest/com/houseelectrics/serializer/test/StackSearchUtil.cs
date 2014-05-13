using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodBase = System.Reflection.MethodBase;
using StackFrame = System.Diagnostics.StackFrame;
using StackTrace = System.Diagnostics.StackTrace;
using TestAttribute = NUnit.Framework.TestAttribute;
using Assert = NUnit.Framework.Assert;

namespace com.houseelectrics.serializer.test
{
    

    public class StackSearchUtil
    {
        public static IEnumerable<TestAttribute> FindJunitAttributesInStack()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();
            Func<StackFrame, TestAttribute[]> select2 = delegate(StackFrame sf)
            {
                TestAttribute []tas = (TestAttribute []) sf.GetMethod().GetCustomAttributes(typeof(TestAttribute), true);
                return tas;
            };

            //return frames.Select(select2).Where(delegate(TestAttribute[] tas) { return tas.Length > 0; }).
            //      Select(delegate(TestAttribute[] tas) {return  tas[0];});
            return frames.SelectMany((sf) => { return (TestAttribute[])sf.GetMethod().GetCustomAttributes(typeof(TestAttribute), true); });
        }
    }

    public class StackSearchUtilTest
    {
        public void goodBye()
        {
            StackSearchUtil util = new StackSearchUtil();
            testAttributesFound = StackSearchUtil.FindJunitAttributesInStack();            
        }

        public void goodMorning()
        {
            goodBye();
        }

        public void hello()
        {
            goodMorning();
        }

        IEnumerable<TestAttribute> testAttributesFound;
        const string testDescription = "hi i am a little testy";
        [TestAttribute( Description=testDescription)]
        public void testSearch()
        {
            testAttributesFound = null;
            hello();
            Assert.IsTrue(testAttributesFound.Count()>0, "expected to find a test attribute");
            Assert.AreEqual(testAttributesFound.First().Description, testDescription);
        }
    }

}
