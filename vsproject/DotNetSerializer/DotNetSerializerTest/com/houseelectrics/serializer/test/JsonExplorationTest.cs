using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using JSONExplorerImpl = com.houseelectrics.serializer.JSONExplorerImpl;
using JsonExploreListener = com.houseelectrics.serializer.JsonExploreListener;
using Rhino.Mocks;
using TextWriterExpectationLogger = Rhino.Mocks.Impl.TextWriterExpectationLogger; 

namespace com.houseelectrics.serializer.test
{
    public class JsonExplorationTest
    {
        public class JsonExploreEvent
        {

        }

        public class LeafEvent : JsonExploreEvent
        {
            public string propertyName;
            public string value;
            public bool isQuoted;
            public LeafEvent(string propertyName, string value, bool isQuoted) { this.propertyName = propertyName; this.value = value; this.isQuoted = isQuoted; }
        }

        [Test]
        public void testEscapedDoubleQuoteJsonValue()
        {
            LeafEvent[] argsjsonexp;
            argsjsonexp = new LeafEvent[] { new LeafEvent("a", "\"1 \"2", true) };
            testJsonExplorationParameterised("{ a:\"\\\"1 \\\"2\" }", 1, 1, 1, argsjsonexp);
        }

        [Test]
        public void testDoubleQuoteJsonValue()
            {
            LeafEvent[] argsjsonexp;         
            argsjsonexp = new LeafEvent[] { new LeafEvent("a", "2 3", true) };
            testJsonExplorationParameterised("{ a:\"2 3\" }", 1, 1, 1, argsjsonexp);
            }

        [Test]
        public void testSingleQuoteJsonValue()
            {
            LeafEvent[] argsjsonexp;
            argsjsonexp = new LeafEvent[] { new LeafEvent("a", "2 3", true) };
            testJsonExplorationParameterised("{ a:'2 3' }", 1, 1, 1, argsjsonexp);
            }

        [Test]
        public void testBasicJsonExplore()
            {
            testJsonExplorationParameterised("{}", 1, 1, 0);
            }

        [Test]
        public void testJsonExploreBasicLeaf()
             {
             testJsonExplorationParameterised("{a:2}", 1, 1, 1);
             }

        [Test]
        public void testBasicJsonPropertyExplore()
            {
               LeafEvent[] argsjsonexp;
               argsjsonexp  = new LeafEvent[] { new LeafEvent("a", "2", false), new LeafEvent("b", "3", false) };
               testJsonExplorationParameterised("{a:2, b:3}", 1, 1, 2, argsjsonexp);
            }

        [Test]
        public void testJsonExploreNestedObjects()
            {
                LeafEvent[] argsjsonexp;
                argsjsonexp = new LeafEvent[] { new LeafEvent("a", "2", false), new LeafEvent("d", "5", false) };
                testJsonExplorationParameterised("{a:2, b:{ c: { d:5} } }", 3, 3, 2, argsjsonexp);
            }

        [Test]
        public void testEmptyObject()
        {
            string json = "{ }";
            var mocks = new MockRepository();
            var theMock = mocks.StrictMock<JsonExploreListener>();
            RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);
            using (mocks.Ordered())
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonEndObject(2);
            }
            theMock.Replay();

            JSONExplorerImpl jsonExplorerImpl = new JSONExplorerImpl();
            jsonExplorerImpl.explore(json, theMock);
            theMock.VerifyAllExpectations();
        }

        [Test]
        public void testArrayWithfunction()
        {
            string classname = "TestClassName";
            string json = @"{@class:'" + classname+ @"',
 id:'123', 
objectListPropA:null, 
intListPropB:    _a_([3, 4, 5])  }";
            var mocks = new MockRepository();
            var theMock = mocks.StrictMock<JsonExploreListener>();
            RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);
            Console.WriteLine("***testArrayWithfunction json: " + json);

            using (mocks.Ordered())
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("@class", classname, true);
                theMock.JsonLeaf("id", "123", true);
                theMock.JsonLeaf("objectListPropA", null, false);
                theMock.JsonStartFunction("_a_", json.IndexOf("("), "intListPropB" );
                theMock.JsonStartArray(null, json.IndexOf("["));
                theMock.JsonLeaf(null, "3", false);
                theMock.JsonLeaf(null, "4", false);
                theMock.JsonLeaf(null, "5", false);
                theMock.JsonEndArray(json.IndexOf("]"));
                theMock.JsonEndFunction(json.IndexOf(")"));
                theMock.JsonEndObject(json.IndexOf("}"));
            }
            theMock.Replay();

            JSONExplorerImpl jsonExplorerImpl = new JSONExplorerImpl();
            jsonExplorerImpl.explore(json, theMock);
            
            //todo - verify expectations
        }

        [Test]
        public void testArray()
        {
            string json = "{ propB:[1,2,3, \"abc\"]}";
            var mocks = new MockRepository();
            var theMock = mocks.StrictMock<JsonExploreListener>();
            RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);
            Console.WriteLine("***testArray json: " + json);
            using (mocks.Ordered())
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonStartArray("propB", json.IndexOf("["));
                theMock.JsonLeaf(null, "1", false);
                theMock.JsonLeaf(null, "2", false);
                theMock.JsonLeaf(null, "3", false);
                theMock.JsonLeaf(null, "abc", true);
                theMock.JsonEndArray(json.LastIndexOf("]"));
                theMock.JsonEndObject(json.Length - 1);
            }
            theMock.Replay();
            JSONExplorerImpl jsonExplorerImpl = new JSONExplorerImpl();
            jsonExplorerImpl.explore(json, theMock);
            theMock.VerifyAllExpectations();            
        }

        [Test]
        public void testEmbeddedFunction()
        {
            string functionName = "funcA";
            string propertyName = "propA";
            string json = "{ " + propertyName + ":" + functionName + "({ a:3, b:\"a b c\" }) }";
            
            var mocks = new MockRepository();
            var theMock = mocks.StrictMock<JsonExploreListener>();
            RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);
            Console.WriteLine("***testEmbeddedFunction json: " + json);
            using (mocks.Ordered())
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonStartFunction(functionName, json.IndexOf(functionName) + functionName.Length, propertyName);
                theMock.JsonStartObject(null, json.LastIndexOf("{"));
                theMock.JsonLeaf("a", "3", false);
                theMock.JsonLeaf("b", "a b c", true);
                theMock.JsonEndObject(json.IndexOf("}"));
                theMock.JsonEndFunction(json.IndexOf(")"));                
                theMock.JsonEndObject(json.Length - 1);
            }
            theMock.Replay();

            JSONExplorerImpl jsonExplorerImpl = new JSONExplorerImpl();
            jsonExplorerImpl.explore(json, theMock);
            theMock.VerifyAllExpectations();
        }


        class ExplorationTestListener : JsonExploreListener
        {
            public List<LeafEvent> actualLeafEvents = new List<LeafEvent>();
            public int startCount = 0, endCount = 0, leafCount = 0;
            public void JsonStartObject(string propertyName, int pos)
            {
                Console.WriteLine(String.Format("down {0} {1}", propertyName, pos));
                startCount++;
            }
            public void JsonLeaf(string propertyName, string value, bool isQuoted)
            {
                actualLeafEvents.Add(new LeafEvent(propertyName, value, isQuoted));
                Console.WriteLine(String.Format("leaf {0}=={1} isQuoted:{2}", propertyName, value, isQuoted));
                leafCount++;
            }
            public void JsonEndObject(int pos)
            {
                Console.WriteLine(String.Format("up {0} ", pos));
                endCount++;
            }
            public void JsonStartFunction(string functionName, int pos, string propertyName)
            {

            }
            public void JsonEndFunction(int pos)
            {

            }

            public void JsonStartArray(string propertyName, int pos)
            {

            }
            public void JsonEndArray(int pos)
            {

            }

        }

        public void testJsonExplorationParameterised(string json, int expectedStartCount, int expectedEndCount, int expectedLeafCount, LeafEvent[] expectedLeafEvents = null)
        {
            Console.WriteLine(String.Format("json: {0}", json));
            JSONExplorerImpl jsonExplorerImpl = new JSONExplorerImpl();
            ExplorationTestListener etl = new ExplorationTestListener();
            jsonExplorerImpl.explore(json, etl);
            Assert.AreEqual(expectedStartCount, etl.startCount, String.Format("expected {0} object start", expectedStartCount));
            Assert.AreEqual(expectedEndCount, etl.endCount, String.Format("expected {0} object end", expectedEndCount));
            Assert.AreEqual(expectedLeafCount, etl.leafCount, String.Format("expected {0} leaves", expectedLeafCount));
            if (expectedLeafEvents!=null)
            {
              for ( int done=0; done<expectedLeafEvents.Length; done++)
                 {
                     Assert.AreEqual(expectedLeafEvents[done].propertyName, etl.actualLeafEvents[done].propertyName, String.Format("leaf name {0}", done));
                     Assert.AreEqual(expectedLeafEvents[done].value, etl.actualLeafEvents[done].value, String.Format("leaf value {0}", done));
                     Assert.AreEqual(expectedLeafEvents[done].isQuoted, etl.actualLeafEvents[done].isQuoted, String.Format("leaf isQuoted {0} ", done));
                 }
            }
        }

    }
}
