using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract = System.Runtime.Serialization.DataContractAttribute;
using DataMember = System.Runtime.Serialization.DataMemberAttribute;
using DataContractJsonSerializer = System.Runtime.Serialization.Json.DataContractJsonSerializer;
using NUnit.Framework;
using System.IO;
using DataContractFieldNodeExpander = com.houseelectrics.serializer.datacontract.DataContractFieldNodeExpander;
using Rhino.Mocks;
using TextWriterExpectationLogger = Rhino.Mocks.Impl.TextWriterExpectationLogger;
using TestUtil = com.houseelectrics.serializer.test.TestUtil;
using SerialisationException = System.Runtime.Serialization.SerializationException;
using DataContractMandatoryEnforcementListener = com.houseelectrics.serializer.datacontract.DataContractMandatoryEnforcementListener;
using DataContractDefaultUtil = com.houseelectrics.serializer.datacontract.DataContractDefaultUtil;
/**
 * todo: test DataContractAttribute - none of the attributes appear to have an impact on json
 **/

namespace com.houseelectrics.serializer.test.datacontract
{
    [DataContract]
    internal class Person
    {
        [DataMember]
        internal string name;

        [DataMember]
        internal int age;

        [DataMember(Name="salutation")]
        internal string title;

        //this warning relates to unused fields - not relevant in this case
        #pragma warning disable  0649
        //non DataMember field - present to test it is  not serialized
        internal string nonDataContractField;
        #pragma warning restore  0649

    }

    [DataContract]
    internal class OrderingTestObject
    {
        [DataMember(Order=4)]
        public string four;

        [DataMember(Order = 3)]
        public string three;

        [DataMember(Order = 1)]
        public string one;

        [DataMember(Order = 2, IsRequired=true)]
        public string two;
    }
    
    public class DataContractSupportTest
    {
        [Test]
        /**
         * example from 
         * http://msdn.microsoft.com/en-us/library/bb412179(v=vs.110).aspx
         * */
        public void testDataMemberSimple()
        {
            Person p = new Person();
            p.name = "John";
            p.age = 42;
            p.title = "Mr";

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Person));

            ser.WriteObject(stream1, p);

            StreamReader sr = new StreamReader(stream1);

            stream1.Position=0;
            string strBuiltInJson = sr.ReadToEnd();
            //Console.WriteLine(sr.ReadToEnd());
            Console.WriteLine("inbuilt json=" + strBuiltInJson);

            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new DataContractFieldNodeExpander();
            string strJson =  o2j.toJson(p);
            System.Console.WriteLine("json:" + strJson);

            // analyze with JsonExplorer
            JSONExplorerImpl jsonExplorer = new JSONExplorerImpl();
            TestListener inbuiltListener = new TestListener();
            TestListener listener = new TestListener();
            jsonExplorer.explore(strBuiltInJson, inbuiltListener);
            jsonExplorer.explore(strJson, listener);

            Assert.AreEqual(inbuiltListener.leaves.Keys.Count, listener.leaves.Keys.Count, "leaf count");            

            compareMaps(inbuiltListener.leaves, "inbuilt", listener.leaves, "local");
            compareMaps(listener.leaves, "local", inbuiltListener.leaves, "inbuilt");

        }

        [Test]
        public void testOrdering()
        {
            OrderingTestObject orderingTestObject = new OrderingTestObject();
            orderingTestObject.four = "4";
            orderingTestObject.three = "3";
            orderingTestObject.one = "1";
            orderingTestObject.two = "2";

            var mocks = new MockRepository();
            var theMock = mocks.StrictMock<JsonExploreListener>();
            RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);

            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new DataContractFieldNodeExpander();
            string json = o2j.toJson(orderingTestObject);
            System.Console.WriteLine("json:" + json);

            Console.WriteLine("***testArray json: " + json);
            using (mocks.Ordered())
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("one", "1", true);
                theMock.JsonLeaf("two", "2", true);
                theMock.JsonLeaf("three", "3", true);
                theMock.JsonLeaf("four", "4", true);
                theMock.JsonEndObject(json.Length - 1);
            }

            theMock.Replay();
            JSONExplorerImpl jsonExplorerImpl = new JSONExplorerImpl();
            jsonExplorerImpl.explore(json, theMock);
            theMock.VerifyAllExpectations();            

        }


        void compareMaps(Dictionary<string, string> map1, string mapName1, Dictionary<string, string> map2, string mapName2)
        {
            foreach (string key in map1.Keys)
            {
                Console.WriteLine("checking key " + key);
                Assert.IsTrue(map2.Keys.Contains(key),
                       mapName1 + " contains property \"" + key + "\" but "+ mapName2+ " doesnt");
                string actual = map2[key];
                string expected = map1[key];
                Assert.AreEqual(expected, actual, "key value for \"" + key + "\"");
            }
        }

        class TestListener : JsonExploreListener
        {
            public Dictionary<string, string> leaves = new Dictionary<string, string>();
            public void JsonStartObject(string propertyName, int pos) { }
            public void JsonLeaf(string propertyName, string value, bool isQuoted)
                 {
                     leaves[propertyName] = value;
                 }
            public void JsonEndObject(int pos) { }
            public void JsonStartFunction(string functionName, int pos, string propertyName) { }
            public void JsonEndFunction(int pos) { }
            public void JsonStartArray(string propertyName, int pos) { }
            public void JsonEndArray(int pos) { }
        }

        [Test(Description="test Mandatory fields check ms built in exception, check native exception")]
        public void testMandatory()
        {
            OrderingTestObject te = new OrderingTestObject();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(te.GetType());
            Stream stream;
            string json;

            object returnValue;
            Exception exception;
            json = "{ one:2 }";
            TestUtil.run(out returnValue, out exception,
                 () =>
                 {
                     stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                     te = (OrderingTestObject)ser.ReadObject(stream);
                     return te;
                 }
                 );
            Assert.AreEqual(typeof(SerialisationException), exception.GetType(), "should fail w exception");

            DataContractMandatoryEnforcementListener dcListener = new DataContractMandatoryEnforcementListener();
            TestUtil.run(out returnValue, out exception,
                 () =>
                 {
                     Json2Object j2O = new Json2Object();
                     j2O.Add(dcListener);
                     te = (OrderingTestObject) j2O.toObject(json, typeof(OrderingTestObject));
                     return te;
                 }
                 );
            Assert.AreEqual(typeof(SerialisationException), exception.GetType(), "should fail w exception");
            
            //check the enforcement listener is not giving a false positive !
            Dictionary<string, object> propertyMap = dcListener.Object2PropertyValues.First().Value;
            Assert.AreEqual(propertyMap.Count, 1, "enforcement data exists");
            Assert.AreEqual(propertyMap["one"], "2", "enforcment data contains a correct value");


        }

        [DataContract]
        internal class EmitDefaultTest
        {
            [DataMember(EmitDefaultValue = false, Order=0)]
            public string zero = "0";

            [DataMember(Order=1)]
            public string one = "1";

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public int two = 2;

            [DataMember(Order=3)]
            public int three = 3;

            [DataMember(EmitDefaultValue = false, Order = 4)]
            public double four = 4.0;

            [DataMember(Order=5)]
            public double five = 5.0;

            [DataMember(Order = 6, EmitDefaultValue=false)]
            public bool bSix;

            [DataMember(Order = 7, EmitDefaultValue = true)]
            public bool bSeven;

//this warning relates to unused fields - not relevant in this case
#pragma warning disable  0649
            [DataMember(Order = 8, EmitDefaultValue = false)]
            public UInt32 eight;
#pragma warning restore  0649


            [DataMember(Order = 9, EmitDefaultValue = true)]
            public UInt64 nine;

            [DataMember(Order = 10, EmitDefaultValue = false)]
            public int? ten;

            [DataMember(Order = 11, EmitDefaultValue = true)]
            public int? eleven;

            [DataMember(Order = 12, EmitDefaultValue = true)]
            public int? twelve;

            [DataMember(Order = 13, EmitDefaultValue = false)]
            public float thirteen;

            [DataMember(Order = 14, EmitDefaultValue = true)]
            public float forteen;
        }

        public delegate void JsonExpectationBlock(JsonExploreListener jsonListener, string json);
        
        protected void testInBuiltAndNativeJson(Object o, JsonExpectationBlock jsonExpectation, string testDescription)
        {
            System.Console.WriteLine("starting " + testDescription + " ************");

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer( o.GetType());
            ser.WriteObject(stream1, o);

            StreamReader sr = new StreamReader(stream1);
            stream1.Position = 0;
            string strBuiltInJson = sr.ReadToEnd();
            var mocks = new MockRepository();
            JsonExploreListener theMock = null;
            RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);

            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new DataContractFieldNodeExpander();
            o2j.isDefaultLeafValue = DataContractDefaultUtil.isDefaultLeafValue;
            o2j.OmitDefaultLeafValuesInJs = true;
            string nativejson = o2j.toJson(o);
            JSONExplorerImpl jsonExplorerImpl = new JSONExplorerImpl();

            string json = null;
            Func<string> runWithMocks = () =>
            {
                theMock = mocks.StrictMock<JsonExploreListener>();
                using (mocks.Ordered())
                {
                    jsonExpectation(theMock, json);
                }
                theMock.Replay();
                jsonExplorerImpl.explore(json, theMock);
                theMock.VerifyAllExpectations();
                return null;
            };

            Console.WriteLine("testing inbuilt json=" + strBuiltInJson);
            json = strBuiltInJson;
            runWithMocks();

            Console.WriteLine("testing json=" + nativejson);
            json = nativejson;
            runWithMocks();

            System.Console.WriteLine("completed " + testDescription + " ************");
        }

        internal class EmitDefaultsUserGuideExample
        {
            [DataMember(EmitDefaultValue = false, Order = 0)]
            public string zero = "0";

            [DataMember(EmitDefaultValue = false, Order = 1)]
            public string one = "1";

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public int two = 2;

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public int three = 3;            
        }

        [Test(Description="test for EmitDefault control for userguide")]
        public void testEmitDefaultForUserguide()
        {
            //create test data
            EmitDefaultsUserGuideExample o = new EmitDefaultsUserGuideExample();
            o.zero = null;
            o.one = "1one1";
            o.two = 0;
            o.three = 3;

            //setup serializer
            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new DataContractFieldNodeExpander();
            o2j.isDefaultLeafValue = DataContractDefaultUtil.isDefaultLeafValue;
            o2j.OmitDefaultLeafValuesInJs = true;

            //create json
            string json = o2j.toJson(o);
            System.Console.WriteLine("json=" + json);
            Assert.IsTrue(json.IndexOf("zero")==-1, "default valued zero is unspecified");
            Assert.IsTrue(json.IndexOf("one") >0, "non-default valued one is specified");
            Assert.IsTrue(json.IndexOf("two") == -1, "default valued two is unspecified");
            Assert.IsTrue(json.IndexOf("three") > 0 , "non-default valued zero is specified");
        }

        [Test(Description = "Emit default test")]
        public void testEmitDefaultFalse()
        {
            EmitDefaultTest o = new EmitDefaultTest();
            o.zero = null;
            o.one = null;
            o.two = 0;
            o.three = 0;
            o.four = 0.0;
            o.five = 0.0;
            o.bSix = false;
            o.bSeven = false;
            o.nine = 0;
            o.ten = null;
            o.eleven = null;
            o.twelve = 0;
            o.thirteen = 0;
            o.forteen = 0;

            JsonExpectationBlock jsonExpectationBlock = (theMock, json) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("one", null, false);
                theMock.JsonLeaf("three", "0", false);
                theMock.JsonLeaf("five", "0", false);
                theMock.JsonLeaf("bSeven", "false", false);
                theMock.JsonLeaf("nine", "0", false);
                theMock.JsonLeaf("eleven", null, false);
                theMock.JsonLeaf("twelve", "0", false);
                theMock.JsonLeaf("forteen", "0", false);
                theMock.JsonEndObject(json.Length - 1);
            };

            testInBuiltAndNativeJson(o, jsonExpectationBlock, "test EmitDefault s");
        }


        [DataContract]
        public class TestSubObject
        {
            [DataMember]
            public string strField1;
        }

        [DataContract]
        public class TestContainer
        {
            [DataMember]
            public TestSubObject subObject;
        }

        [Test(Description = "TestNested")]
        public void testNested()
        {
            TestContainer o = new TestContainer();
            o.subObject = new TestSubObject();
            o.subObject.strField1 = "hello";
            JsonExpectationBlock jsonExpectation = (theMock, json) =>
                {
                    theMock.JsonStartObject(null, 0);
                    int startSubObject = json.IndexOf('{', 1);
                    theMock.JsonStartObject("subObject", startSubObject);
                    theMock.JsonLeaf("strField1", o.subObject.strField1, true);
                    int endSubObject = json.IndexOf('}', startSubObject);
                    theMock.JsonEndObject(endSubObject);
                    theMock.JsonEndObject(json.Length - 1);
                };
            testInBuiltAndNativeJson(o, jsonExpectation, "test nested json");
        }

        [DataContract]
        public class TestPropertyContainer
        {
            string _str;
            [DataMember]
            public string StringA { get {return _str; } set {_str=value;} }
        }

        [Test(Description = "Test DataMember Properties")]
        public void testProperties()
        {
            TestPropertyContainer o = new TestPropertyContainer();

            o.StringA = "abcd";
            JsonExpectationBlock jsonExpectation = (theMock, json) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("StringA", o.StringA, true);
                theMock.JsonEndObject(json.Length - 1);
            };

            testInBuiltAndNativeJson(o, jsonExpectation, "test Data Member Properties");
        }

        [DataContract(Name="TestDataContractName")]
        public class TestDataContract
        {
            [DataMember]
            public int field1;
        }

        // http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractattribute(v=vs.110).aspx
        [Test(Description = "Test DataContract attribute")]
        public void testDataContractAttribute()
        {
            TestDataContract o = new TestDataContract();

            o.field1 = 321;
            JsonExpectationBlock jsonExpectation = (theMock, json) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("field1", ""+o.field1, false);
                theMock.JsonEndObject(json.Length - 1);
            };

            testInBuiltAndNativeJson(o, jsonExpectation, "test test Data Contract Attributes");
        }
    }
}
