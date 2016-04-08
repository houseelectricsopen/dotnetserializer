using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using T = NUnit.Framework.Assert;
using Test = NUnit.Framework.TestAttribute;
using Assert = NUnit.Framework.Assert;
using ExpectedException =  NUnit.Framework.ExpectedExceptionAttribute;
using Json2Object = com.houseelectrics.serializer.Json2Object;
using NoClueForTypeException = com.houseelectrics.serializer.Json2Object.NoClueForTypeException;
using UnknownFunctionException = com.houseelectrics.serializer.Json2Object.UnknownFunctionException;

using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using TextWriterExpectationLogger = Rhino.Mocks.Impl.TextWriterExpectationLogger; 


namespace com.houseelectrics.serializer.test
{
    public class TestFromJson
    {
        public class SimpleTestObject
        {
            public string stringFieldA;
            public string stringPropertyB;
            public string StringPropertyB { get { return stringPropertyB; } set { this.stringPropertyB = value; } }

            public int intFieldC;
            public int intPropertyD;
            public int IntPropertyD { get { return intPropertyD; } set { this.intPropertyD = value; } }
        }

        [Test ]
        public void testSimpleObjectNoTypeHints()
        {
            Exception ex;
            object result;
            string json = "{}";
            Func<object> f = () => { Json2Object j2O = new Json2Object(); object o = j2O.toObject(json); return o; };
            TestUtil.run(out result, out ex, f);
            Assert.AreEqual(typeof(NoClueForTypeException), ex == null ? null : ex.GetType(), "expected no type hint indication");
        }


        [Test]
        public void testSimpleObjectHintAsParameter()
        {
            string json = "{}";
            Json2Object j2O = new Json2Object();
            Type type = typeof(SimpleTestObject);
            object o = j2O.toObject(json, type );
            Assert.AreEqual(type, o.GetType(), "expected type");
        }

        [Test]
        public void testSimpleObjectHintAsAttribute()
        {
            Type type = typeof(SimpleTestObject);
            Json2Object j2O = new Json2Object();
            string json = "{" + j2O.TypeSpecifier + ":" + type.FullName  + "}";
            System.Console.Out.WriteLine("TestSimpleObjectHintAsAttribute json:" + json);
            object o = j2O.toObject(json);
            Assert.AreEqual(type, o.GetType(), "expected type");
        }

        // test as above but with incorrect classname
        [Test]
        public void testSimpleObjectIncorrectHintAsAttribute()
        {
            Exception ex=null;
            Object result;
            Json2Object j2O = new Json2Object();
            Type type = typeof(SimpleTestObject);
            string json = "{" + j2O.TypeSpecifier + ":" + type.FullName + ".invalid}";
            System.Console.Out.WriteLine("TestSimpleObjectHintAsAttribute json:" + json);
            Func<object> f = () => { result = j2O.toObject(json); return result; };
            TestUtil.run(out result, out ex, f);
            Assert.AreEqual(typeof(NoClueForTypeException), ex == null ? null : ex.GetType(), "expected no type hint indication");
        }

        [Test]
        public void testFields()
        {
            SimpleTestObject result;
            Json2Object j2O = new Json2Object();
            Type type = typeof(SimpleTestObject);
            string stringFieldAvalue = "abcd";
            int intFieldValue = 9876;
            string json = "{" + j2O.TypeSpecifier + ":" + type.FullName + " stringFieldA:\"" + stringFieldAvalue + "\"  intFieldC:" + intFieldValue+ " }";
            System.Console.Out.WriteLine("TestSimpleObjectHintAsAttribute json:" + json);
            result = (SimpleTestObject) j2O.toObject(json);
             
            Assert.AreEqual(result.stringFieldA, stringFieldAvalue, "matching string value");
            Assert.AreEqual(result.intFieldC, intFieldValue, "matching int value");

        }

        [Test(Description="Derived from testFields test")]
        public void testFields4UserGuide()
        {
            Json2Object j2O = new Json2Object();
            string json = "{@class:com.houseelectrics.serializer.test.TestFromJson+SimpleTestObject stringFieldA:\"abcd\"  intFieldC:9876 }";
            SimpleTestObject result = (SimpleTestObject)j2O.toObject(json);
            Assert.AreEqual(result.stringFieldA, "abcd", "matching string value");
            Assert.AreEqual(result.intFieldC, 9876, "matching int value");
        }

        [Test(Description = "Derived from testFields test")]
        public void testFields4UserGuideTypeHint()
        {
            Json2Object j2O = new Json2Object();
            string json = "{stringFieldA:\"abcd\"  intFieldC:9876 }";
            SimpleTestObject result = (SimpleTestObject)j2O.toObject(json, typeof(SimpleTestObject));
            Assert.AreEqual(result.stringFieldA, "abcd", "matching string value");
            Assert.AreEqual(result.intFieldC, 9876, "matching int value");
        }



        [Test]
        public void testProperties()
        {
            SimpleTestObject result;
            Json2Object j2O = new Json2Object();
            //j2O.setToUseProperties();
            Type type = typeof(SimpleTestObject);
            string stringPropertyBvalue = "abcd123";
            int intPropertyValue = 98765;
            string json = "{" + j2O.TypeSpecifier + ":" + type.FullName + " StringPropertyB:\"" + stringPropertyBvalue + "\" IntPropertyD:" + intPropertyValue +" }";
            System.Console.Out.WriteLine("TestSimpleObjectHintAsAttribute json:" + json);
            result = (SimpleTestObject)j2O.toObject(json);
            Assert.AreEqual(stringPropertyBvalue, result.StringPropertyB, "propertyB value");
            Assert.AreEqual(intPropertyValue, result.IntPropertyD, "IntPropertyD value");
        }


        [Test(Description = "try different Property types")]
        public void testToJsonLeafTypesViaProperties()
        {
            AllPrimitiveLeafTypes template = new AllPrimitiveLeafTypes();
            String[] expressions = AllPrimitiveLeafTypes.testPropertyExpressions;
            //{ "ALong", "AShort", "ALongRef", "AString", "AChar", "AString2" };
            Object[] expectedValues = AllPrimitiveLeafTypes.testExpectedPropertyValues(template);
            //{ template.ALong, template.AShort, template.ALongRef, template.AString, template.AChar, template.AString2, template.AUint32, template.AUint64 };

            AllPrimitiveLeafTypes result;
            Json2Object j2O = new Json2Object();
            //j2O.setToUseProperties();
            Type type = typeof(AllPrimitiveLeafTypes);
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = new PropertyReflectionNodeExpander();
            //todo json 2 object should understand TypeAliaser
            o2J.TypeAliaser = (t) => {return t.FullName; };
            o2J.TypeAliasProperty = j2O.TypeSpecifier;
            string json = o2J.toJson(template);
            System.Console.Out.WriteLine("testToJsonLeafTypesViaFields json:" + json);
            result = (AllPrimitiveLeafTypes)j2O.toObject(json);

            for (int done = 0; done < expressions.Length; done++ )
            {
                string expression = expressions[done];
                object value = type.GetProperty(expression).GetValue(result);
                Assert.AreEqual(expectedValues[done], value, expression + " value");
            }
        
        }

        class SubObject
        {
            public string fieldA;
        }
        class MasterObject
        {
            public SubObject subObject = null;
            public object []theTopArray=null;
            public string[] theStringArray = null;
        }
     

        [Test(Description="try embedded Object")]
        public void testToJsonEmbedded()
        {
            MasterObject masterObject = new MasterObject();
            masterObject.subObject = new SubObject();
            masterObject.subObject.fieldA = "abc";

            MasterObject result;
            Json2Object j2O = new Json2Object();
            //j2O.setToUseFields();
            Type type = typeof(MasterObject);
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = new FieldReflectionNodeExpander();
            //todo json 2 object should understand TypeAliaser
            o2J.TypeAliaser = (t) => { return t.FullName; };
            o2J.TypeAliasProperty = j2O.TypeSpecifier;
            string json = o2J.toJson(masterObject);
            System.Console.Out.WriteLine("testToJsonLeafTypesViaFields json:" + json);
            result = (MasterObject)j2O.toObject(json);

            string expression = "subObject.fieldA";
            Assert.AreEqual(masterObject.subObject.fieldA, result.subObject.fieldA, expression + " value");

        }

        [Test(Description="unknown Function")]
        public void testUnknownFunction()
        {
            string json = "{ propa:nonexistantF() }";
            Json2Object j2O = new Json2Object();
            object result;
            Exception ex;
            Func<object> f = () => { return j2O.toObject(json, typeof(object)); };
            TestUtil.run(out result, out ex, f);
            Assert.AreEqual(typeof(UnknownFunctionException), ex == null ? null : ex.GetType(), "expected UnknownFunctionException");
        }

        [Test(Description="try array Object")]
        public void testToJsonArray()
        { 
            MasterObject masterObject = new MasterObject();
            masterObject.theTopArray = new string[] {"a", "b", "b"};
            masterObject.theStringArray = new string[] { "iii", "iv", "v", "vi"};

            MasterObject result;
            Json2Object j2O = new Json2Object();
            //j2O.setToUseFields();
            Type type = typeof(MasterObject);
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = new FieldReflectionNodeExpander();
            //todo json 2 object should understand TypeAliaser
            o2J.TypeAliaser = (t) => { return t.FullName; };
            o2J.TypeAliasProperty = j2O.TypeSpecifier;
            string json = o2J.toJson(masterObject);
            System.Console.Out.WriteLine("testToJsonLeafTypesViaFields json:" + json);
            result = (MasterObject)j2O.toObject(json);

            string expression = "theTopArray[0]";
            Assert.AreEqual(masterObject.theTopArray.Length, result.theTopArray.Length, " Length");
            Assert.AreEqual(masterObject.theTopArray[0], result.theTopArray[0], expression + " value");
            Assert.AreEqual(masterObject.theStringArray.Length, result.theStringArray.Length, " Length");
            Assert.AreEqual(masterObject.theStringArray[3], result.theStringArray[3], expression + " value");

        }



        [Test(Description = "try different field types")]
        public void testToJsonLeafTypesViaFields()
        {
            AllPrimitiveLeafTypes template = new AllPrimitiveLeafTypes();

            String []expressions = AllPrimitiveLeafTypes.testFieldExpressions;
            Object[] expectedValues = AllPrimitiveLeafTypes.testExpectedFieldValues(template);

            AllPrimitiveLeafTypes result;
            Json2Object j2O = new Json2Object();
            //j2O.setToUseFields();
            Type type = typeof(AllPrimitiveLeafTypes);
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = new FieldReflectionNodeExpander();
            //todo json 2 object should understand TypeAliaser
            o2J.TypeAliaser = (t) => { return t.FullName; };
            o2J.TypeAliasProperty = j2O.TypeSpecifier;
            string json = o2J.toJson(template);
            System.Console.Out.WriteLine("testToJsonLeafTypesViaFields json:" + json);
            result = (AllPrimitiveLeafTypes)j2O.toObject(json);

            for (int done = 0; done < expressions.Length; done++)
            {
                string expression = expressions[done];
                object value = type.GetField(expression).GetValue(result);
                if (value != null)
                {
                    Type underlyingType = value.GetType();
                    if (Nullable.GetUnderlyingType(underlyingType)!=null)
                    {
                        underlyingType = Nullable.GetUnderlyingType(underlyingType);
                    }
                    if (underlyingType == typeof(Char) || underlyingType == typeof(char)) value = value.ToString();
                }
                Assert.AreEqual(expectedValues[done], value, expression + " value");
            }

        }


        [Test(Description = "try hierarchical structure - relates to TestToJson.testToJsonHierarchy")]
        public void testFromJsonHierarchy()
        {

            TestTreeNode rootNode = new TestTreeNode();
            rootNode.Name = "top";
            rootNode.branches = new List<TestTreeNode>();
            TestTreeNode child = new TestTreeNode();
            child.Name = "child";
            child.branches = null;
            rootNode.branches.Add(child);
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            o2J.TypeAliaser = (t) => { return t.FullName; };
            Json2Object j2O = new Json2Object();
            o2J.TypeAliasProperty = j2O.TypeSpecifier;
            String json = o2J.toJson(rootNode);
            System.Console.Out.WriteLine("testToJsonLeafTypesViaFields json:" + json);
            TestTreeNode result = (TestTreeNode) j2O.toObject(json, typeof(TestTreeNode));
            Func<TestTreeNode, String> extractValue = (tn) => {return tn.Branches[0].Name;};
            Assert.AreEqual(extractValue(rootNode), extractValue(result), "check nested values" );

        }

        JsonMapping mapping = new DefaultJsonMapping();

        public class GenericListContainer
        {
            public GenericListContainer(string id) { this.id = id; }
            public GenericListContainer() : this("0") { }
            public string id;
            public List<object> objectListPropA;
            public List<int> intListPropB;
            public override string ToString() { return id; }
            public Dictionary<string, string> stringToStringMapPropC;
            public Dictionary<int, string> intToStringMapPropC;
        }

        [Test(Description = "test Map objects")]
        public void testMapObjects()
        {
            GenericListContainer input = new GenericListContainer("");

            input.stringToStringMapPropC = new Dictionary<string, string>();
            input.stringToStringMapPropC["0"]= "zero";
            input.stringToStringMapPropC["1"] = "one";
            input.stringToStringMapPropC["2"] = "two";
            input.stringToStringMapPropC["3"] = "three";

            input.intToStringMapPropC = new Dictionary<int, string>();
            input.intToStringMapPropC[0] = "zero";
            input.intToStringMapPropC[1] = "one";
            input.intToStringMapPropC[2] = "two";
            input.intToStringMapPropC[3] = "three";
            

            string json = mapping.getObject2Json().toJson(input);

            GenericListContainer output = (GenericListContainer)mapping.getJson2Object().toObject(json, typeof(GenericListContainer));

            Func<GenericListContainer, string, string> extractValue;

            extractValue = (glc, strKey) => { return glc.stringToStringMapPropC[strKey]; };
            foreach (string strKey in  input.stringToStringMapPropC.Keys)
            {
                Assert.AreEqual(extractValue(input, strKey), extractValue(output, strKey), "checking key " + strKey);
            }

            Func<GenericListContainer, int, string> extractIValue;
            extractIValue = (glc, iKey) => { return glc.intToStringMapPropC[iKey]; };
            foreach (int iKey in input.intToStringMapPropC.Keys)
            {
                Assert.AreEqual(extractIValue(input, iKey), extractIValue(output, iKey), "checking key " + iKey);
            }


        }


        [Test(Description="test Generic List Object fields") ]
        public void testGenericListObjectFields()
        {
            GenericListContainer input = new GenericListContainer("");

            input.objectListPropA = new List<object>();
            input.objectListPropA.Add(new GenericListContainer("0"));
            input.objectListPropA.Add(new GenericListContainer("b"));
            input.objectListPropA.Add(new GenericListContainer("iii"));
            string json = mapping.getObject2Json().toJson(input);

            GenericListContainer output = (GenericListContainer)mapping.getJson2Object().toObject(json, typeof(GenericListContainer));

            Func<GenericListContainer, int, string> extractValue = (glc, index) =>
                { return glc.objectListPropA[index].ToString(); };    
            
            for (int done=0; done<output.objectListPropA.Count; done++)
            {
                Assert.AreEqual(extractValue(input, done), extractValue(output, done), "checking index " + done);
            }

        }

        [Test(Description = "test Generic List primitive fields")]
        public void testGenericListPrimitiveFields()
        {
            GenericListContainer input = new GenericListContainer("");

            input.intListPropB = new List<int>();
            input.intListPropB.Add(3);
            input.intListPropB.Add(4);
            input.intListPropB.Add(5);
            string json = mapping.getObject2Json().toJson(input);

            GenericListContainer output = (GenericListContainer)mapping.getJson2Object().toObject(json, typeof(GenericListContainer));

            Func<GenericListContainer, int, int> extractValue = (glc, index) =>
            { return glc.intListPropB[index]; };

            for (int done = 0; done < output.intListPropB.Count; done++)
            {
                Assert.AreEqual(extractValue(input, done), extractValue(output, done), "checking index " + done);
            }
        }



        //[Test(Description = "test mixed list")]
        public void testMixedListField()
        {
            GenericListContainer input = new GenericListContainer("");

            input.objectListPropA = new List<object>();
            //input.objectListPropA.Add('3');
            //input.objectListPropA.Add(4);
            input.objectListPropA.Add("5");
            Object2Json o2j = mapping.getObject2Json();
            //o2j.ExplorerFactory = () => { return new TypeHintingObjectExplorerImpl(); };
            o2j.writeLeafValue = (writer, to, propertyName) =>
                {
                if (to!=null)
                {
                    writer.Write("/*");
                    writer.Write(to.GetType().Name);
                    writer.Write("*/");
                }
                Object2Json.defaultWriteLeafValue(writer, to, propertyName);
                };

            string json = o2j.toJson(input);

           
            GenericListContainer output = (GenericListContainer)mapping.getJson2Object().toObject(json, typeof(GenericListContainer));

            Func<GenericListContainer, int, object> extractValue = (glc, index) =>
            { return glc.objectListPropA[index]; };

            for (int done = 0; done < output.objectListPropA.Count; done++)
            {
                Assert.AreEqual(extractValue(input, done), extractValue(output, done), "checking index " + done);
            }
        }


        [Test(Description = "try hierarchical structure - relates to TestToJson.testToJsonHierarchy")]
        public void testFromJsonHierarchyDeep()
        {
            TestTreeNode rootNode = TestTreeNode.createTestHierarchy(4, 3);
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            o2J.TypeAliaser = (t) => { return t.FullName; };
            Json2Object j2O = new Json2Object();
            o2J.TypeAliasProperty = j2O.TypeSpecifier;
            String json = o2J.toJson(rootNode);
            System.Console.Out.WriteLine("testToJsonLeafTypesViaFields json:" + json);
            TestTreeNode result = (TestTreeNode)j2O.toObject(json, typeof(TestTreeNode));
            Func<TestTreeNode, String> extractValue = (tn) => { return tn.Branches[2].Branches[2].Branches[2].Name; };
            Assert.AreEqual(extractValue(rootNode), extractValue(result), "check nested values");

        }

      [Test]
      public void testNotifications()
      {
          SimpleTestObject sto = new SimpleTestObject();
          sto.stringFieldA = "AA";
          string json = "{ stringFieldA:AA }";

          var mocks = new MockRepository();
          var theMock = mocks.StrictMock<DeserializationListener>();
          RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);
          theMock.Expect(x => theMock.onCreateObject(new SimpleTestObject())).Constraints(Is.TypeOf(typeof(SimpleTestObject)));
          theMock.Expect(x => theMock.onSetValue(sto, "stringFieldA", "AA")).Constraints(Is.TypeOf(typeof(SimpleTestObject)), Is.Equal("stringFieldA"), Is.Equal("AA"));
          theMock.Expect(x => theMock.onEndObject(new SimpleTestObject())).Constraints(Is.TypeOf(typeof(SimpleTestObject)));

          theMock.Replay();
          Json2Object j2o = new Json2Object();
          j2o.Add(theMock);
          j2o.toObject(json, typeof(SimpleTestObject));
          
          theMock.VerifyAllExpectations();



      }

      public class TypeInferenceTest
        {
            public class TypeInferenceTestSub
            {
                public string Name { get; set; }
            }
            public List<TypeInferenceTestSub> Items { get; set; } 
            public TypeInferenceTest(){Items = new List<TypeInferenceTestSub>();}
        }


      [Test]
      public void testTypeInference()
        {
            //""Name"":""itema""
            String strJson = @"{ ""Items"":[{""Name"":""itema""}] }";
            Json2Object j2o = new Json2Object();
            TypeInferenceTest result = (TypeInferenceTest) j2o.toObject(strJson, typeof(TypeInferenceTest));
            Assert.AreEqual(result.Items[0].Name, "itema");
        }

    }
}
