using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Defaults4Class = com.houseelectrics.serializer.Defaults4Class;
using TypeAliaser = com.houseelectrics.serializer.TypeAliaser;
using TypeAliaserUtils = com.houseelectrics.serializer.TypeAliaserUtils;
using MethodBase = System.Reflection.MethodBase;
using x= NUnit.Framework.TestAttribute;

namespace com.houseelectrics.serializer.test
{
    public class TestToJson{
    
        JSExcuteUtil util = new JSExcuteUtil();
        public JSExcuteUtil Util { get { return util; } }

        public class IndexedTestContainer
        {
            internal string[] strings;
            internal List<int> ints;
        }

        [Test(Description="properties demo for userguide")]
        public void testDemoIndexedPropertiesForUserguide()
        {
            IndexedTestContainer testData = new IndexedTestContainer();
            testData.strings = new string[] { "eee", "ddd", "ccc", "bbb" };
            testData.ints = new List<int>(new int[] {6,5,4,3,2,1} );
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = new FieldReflectionNodeExpander();
            // make the format prettier
            o2J.IndentSize = 2;
            string json = o2J.toJson(testData);
            System.Console.WriteLine("json=" + json);
            string[] expressions = { "strings[3]", "strings[0]", "ints[0]", "ints[5]"};
            object[] expectedValues = {testData.strings[3], testData.strings[0], testData.ints[0], testData.ints[5] };
            validateJSON(json, expressions, expectedValues, "testDemoIndexedPropertiesForUserguide");
        }

        [Test(Description = "properties demo for userguide")]
        public void testIndexedWithNull()
        {
            IndexedTestContainer testData = new IndexedTestContainer();
            testData.strings = new string[] { null, "ddd", "ccc", null };
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = new FieldReflectionNodeExpander();
            // make the format prettier
            o2J.IndentSize = 2;
            string json = o2J.toJson(testData);
            System.Console.WriteLine("json=" + json);
            string[] expressions = { "strings[0]", "strings[1]", "strings[2]", "strings[3]" };
            object[] expectedValues = { testData.strings[0], testData.strings[1], testData.strings[2], testData.strings[3] };
            validateJSON(json, expressions, expectedValues, "testIndexedWithNull");
        }


        public class HashedTestContainer
        {
            public IDictionary<string, int> Name2Number { get; set; }
            public IDictionary<int, string> Number2Name { get; set; }            
        }


        [Test(Description = "hashed properties demo for userguide")]
        public void testDemoHashedPropertiesForUserguide()
        {
            HashedTestContainer testData = new HashedTestContainer();
            testData.Name2Number = new Dictionary<string, int>();
            testData.Name2Number["one"] = 1;
            testData.Name2Number["two"] = 2;
            testData.Name2Number["three"] = 3;
            testData.Name2Number["four"] = 4;

            testData.Number2Name = new Dictionary<int, string>();
            testData.Number2Name[1]="one";
            testData.Number2Name[2]="two";
            testData.Number2Name[3]="three";
            testData.Number2Name[4]="four";

            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = new PropertyReflectionNodeExpander();
            // make the format prettier
            o2J.IndentSize = 2;
            string json = o2J.toJson(testData);
            System.Console.WriteLine("json=" + json);
            string[] expressions = { "Name2Number['one']"};
            object[] expectedValues = { testData.Name2Number["one"]};
            validateJSON(json, expressions, expectedValues, "testDemoHashedPropertiesForUserguide");
        }

        [Test(Description="try different property types")]
        public void testToJsonLeafTypesViaProperties()
        {
            // todo - this fails with FieldReflectionNodeExpander
            NodeExpander nodeExpander = new PropertyReflectionNodeExpander();
            AllPrimitiveLeafTypes testData = new AllPrimitiveLeafTypes();
            String[] expressions = AllPrimitiveLeafTypes.testPropertyExpressions; 
            Object[] expectedValues = AllPrimitiveLeafTypes.testExpectedPropertyValues(testData); 
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(testData);
            validateJSON(json, expressions, expectedValues, "testToJsonLeafTypesViaProperties");
        }


        [Test(Description = "try different field types")]
        public void testToJsonLeafTypesViaFields()
        {
            // todo - this fails with FieldReflectionNodeExpander
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            AllPrimitiveLeafTypes testData = new AllPrimitiveLeafTypes();
            String[] expressions = AllPrimitiveLeafTypes.testFieldExpressions;
            Object[] expectedValues = AllPrimitiveLeafTypes.testExpectedFieldValues(testData);
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(testData);
            validateJSON(json, expressions, expectedValues, "testToJsonLeafTypesViaFields");
        }


        [Test(Description = "try fields")]
        public void testToJsonFields()
        {
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            TestData testData = new TestData();
            String[] expressions = { "theDate." + NodeExpanderConstants.unixEpochTimeMillisFieldName, "sub.subSub.greeting", "sub.subSub.ageYears", "sub.subSub.heightMetres" };
            Object[] expectedValues = { NodeExpanderConstants.extractEpochTimeMillis( testData.TheDate) , testData.TheSub.TheSubSub.Greeting, testData.TheSub.TheSubSub.AgeYears, testData.TheSub.TheSubSub.HeightMetres };
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(testData);
            validateJSON(json, expressions, expectedValues, "testToJsonFields");
        }

        [Test(Description = "try properties")]
        public void testToJsonProperties()
        {
            NodeExpander nodeExpander = new PropertyReflectionNodeExpander();
            TestData testData = new TestData();
            String[] expressions = { "TheSub.TheSubSub.Greeting", "TheSub.TheSubSub.AgeYears", "TheSub.TheSubSub.HeightMetres" };
            Object[] expectedValues = { testData.TheSub.TheSubSub.Greeting, testData.TheSub.TheSubSub.AgeYears, testData.TheSub.TheSubSub.HeightMetres };
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(testData);
            validateJSON(json, expressions, expectedValues, "testToJsonProperties");
        }


        public void testToJsonMany2Ones()
        {
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            TestData testData = new TestData();
            String[] expressions = { "sub.subSub.seasons" };
            Object[] expectedValues = { testData.TheSub.TheSubSub.Seasons };
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(testData);
            validateJSON(json, expressions, expectedValues, "testToJsonMany2Ones");
        }


        [Test (Description="try indexed properties")]
        public void testToJsonPropertiesIndexed()
        {
            NodeExpander nodeExpander = new PropertyReflectionNodeExpander();
            TestData testData = new TestData();
            String[] expressions = { "TheSub.TheSubSub.Seasons[1]", "TheSub.TheSubSub.GoodYears[1]" };
            Object[] expectedValues = {  testData.TheSub.TheSubSub.Seasons[1], testData.TheSub.TheSubSub.GoodYears[1] };
            Object2Json o2J = new Object2Json();
            //o2J.OmitMarkAsArrayFunction = true;
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(testData);
            validateJSON(json, expressions, expectedValues, "testToJsonPropertiesIndexed");
        }

        protected void validateJSON(String json, String[] expressions, Object[] expectedValues, String testid, String significance = null)
        {
            validateJSON(json, null, expressions, expectedValues ,  testid, significance);
        }

        protected void validateJSON(String json, String []functions, String[] expressions, Object[] expectedValues, String testid
            , String significance =null)
        {

            JSExcuteUtil.JsonValueSet jvset = new JSExcuteUtil.JsonValueSet();
            jvset.varname = "testdata";
            for (int expdone = 0; expdone < expressions.Length; expdone++)
            {
                jvset.expressions2ExpectedValue[expressions[expdone]]=expectedValues[expdone];
            }

            if (functions!=null)
            {
                jvset.extraFunctions = functions.ToList();
            }
            
            jvset.json = json;
            jvset.significance = significance;
            Dictionary<string, object> results = util.extractValuesFromJson(jvset, GetType().Name + "." +testid);
            for (int done = 0; done < expressions.Length; done++ )
            {
                Object expectedValue = expectedValues[done];
                String expression = expressions[done];
                object actualValue = results[expression];
                if (expectedValue !=null && (expectedValue is Char || expectedValue is char))
                {
                    expectedValue = expectedValue.ToString();
                }
                Assert.AreEqual(expectedValue, actualValue, "json=" + json + " expression=" + expression);
            }            
        }

        [Test(Description="try hierarchical structure")]
        public void testToJsonHierarchy()
        {
            int requiredDepth =  4;
            int branchesPerParent = 2;
            TestTreeNode rootNode = TestTreeNode.createTestHierarchy(requiredDepth, branchesPerParent);
            String[] expressions = { "branches[0].branches[0].branches[0].name" };
            Object[] expectedValues = { rootNode.Branches[0].Branches[0].Branches[0].Name };
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(rootNode);
            validateJSON(json, expressions, expectedValues, "testToJsonHierarchy");
        }


        class ObjectWithRefs
        {
            public PotentiallyHugeSubObject subo1;
            public PotentiallyHugeSubObject subo2;
            public PotentiallyHugeSubObject subo3;
        };
        class PotentiallyHugeSubObject
        {
            public String stringVal;
            public int intVal;
        };

        [Test(Description = "try references for Guide - repeated objects are included by reference")]
        public void testUseReferencesSimple()
        {
            ObjectWithRefs root = new ObjectWithRefs();
            root.subo1 = new PotentiallyHugeSubObject();
            root.subo1.stringVal="123456";
            root.subo1.intVal =4321;
            root.subo2=root.subo1;
            root.subo3=root.subo2;
            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new FieldReflectionNodeExpander();
            o2j.UseReferences = true;
            string json = o2j.toJson(root);

            o2j.UseReferences = false;
            string jsonWithoutReferenceSupport = o2j.toJson(root);
            System.Console.WriteLine("testUseReferences1 without references: " + jsonWithoutReferenceSupport);

            //Object value;
            JSExcuteUtil.JsonValueSet valueSet = new JSExcuteUtil.JsonValueSet();
            valueSet.json = o2j.ObjectResolverFunctionName + "(" + json + ")";
            valueSet.varname = "root";
            valueSet.expressions2ExpectedValue["subo3.intVal"] = root.subo3.intVal;
            valueSet.expressions2ExpectedValue["subo3.stringVal"] = root.subo3.stringVal;
            valueSet.extraFunctions.Add(o2j.getObjectResolverJS());
            Dictionary<string, object> values = util.extractValuesFromJson(valueSet, GetType().Name + "." + "testUseReferencesSimple");
            valueSet.expressions2ExpectedValue.Select(
                p => {
                     string expression = p.Key;
                     object expectedValue = valueSet.expressions2ExpectedValue[expression];
                     object actualValue = values[expression];
                     Assert.AreEqual(expectedValue, values[expression], "expression=" + expression + " with reference json= " + json);
                     return expression;
                     }
                );            
        }



        [Test(Description="try references - repeated objects are included by reference")]
        public void testUseReferences()
        {
            TestTreeNode root = new TestTreeNode();
            root.Name = "root";
            TestTreeNode child = TestTreeNode.createTestHierarchy(3, 3);
            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new PropertyReflectionNodeExpander();
            root.Branches.Add(child);
            root.Branches.Add(child);
            root.Branches.Add(child);
            o2j.IndentSize = 2;
            o2j.UseReferences = false;
            string noRefJson = o2j.toJson(root);
            o2j.UseReferences = true;

            string withRefJson = o2j.toJson(root);
            double minimumCompression = 2;
            double compression=((double)noRefJson.Length)/((double)withRefJson.Length);
            System.Console.WriteLine("withRefJson=" + withRefJson);
            Assert.IsTrue((compression>minimumCompression),
                "noRefJson length= " + noRefJson.Length +
                " withRefJson length=" + withRefJson.Length + 
                " compression ratio = " + compression + " minumum ratio=" + minimumCompression);
            
            Object expectedValue = root.Branches[2].Branches[1].Name;
            string expression = "Branches[2].Branches[1].Name";
            //Object value;
            JSExcuteUtil.JsonValueSet valueSet = new JSExcuteUtil.JsonValueSet();
            valueSet.json = o2j.ObjectResolverFunctionName + "(" + withRefJson + ")";
            valueSet.varname = "withRefRoot";
            valueSet.expressions2ExpectedValue[expression]=expectedValue;
            valueSet.extraFunctions.Add(o2j.getObjectResolverJS());
            valueSet.extraFunctions.Add(o2j.getAttachId2ArrayJSFunction());
            valueSet.extraFunctions.Add(o2j.getMarkAsArrayJSFunction());
            Dictionary<string, object> values = util.extractValuesFromJson(valueSet, GetType().Name + "." + "testUseReferences");
            Assert.AreEqual(expectedValue, values[expression], "expression=" + expression + " with reference json= " + withRefJson);
        }

        [Test(Description="try a simple map structure (c# dictionary)")]
        public void testSimpleMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            String[] expressions = { "abc", "xyz", "cba", "ooo" };
            string[] expectedValues = {"hello", "goodday", "goodnight", null };
            for (int done = 0; done < expressions.Length; done++ )
            {
                map[expressions[done]]=expectedValues[done];
            }
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
                        Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(map);
            validateJSON(json, expressions, expectedValues, "testSimpleMap");
 
        }

        class ManyDefaultsTestData
        {
            internal int i1 = 123456789;
            internal int i2 = 23456789;
            public int i4 = 432102345;
            public class StringValues
            { 
                public string str1="abcdefghijk";
                internal string str2="bcdefghijklmno";
                internal string str3=null;
                internal string aReallyReallyReallyLongName="John Malcom Eldritch Ebeneezer Ozwald Smith";                
            }
            internal StringValues stringValues = new StringValues();
            internal StringValues[] stringValuesArr = { new StringValues(), new StringValues(), new StringValues() };
        }

        [Test(Description="check writing of defaults works")]
        public void testDefaultingWritingDefaultsBit()
        {
            //todo autoreject invalid js aliases e.g. numberot dot contained
            Object2Json o2j = new Object2Json();
            DefaultFinder df = new DefaultFinder();
            ManyDefaultsTestData md = new ManyDefaultsTestData();
            TypeAliaser aliaser = delegate(Type type)
            {
                return type.FullName.Replace('.', '_').Replace('+', '_');
            };
            // need to pass in class name aliasing scheme
            // this will fail if aliases are not valid js propert names
            Dictionary<string, Defaults4Class> defaultsJS;
            String[] expressions;
            object[] expectedValues;
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            LeafDefaultSet lds = df.getDefaultsForAllLinkedObjects(md, new FieldReflectionNodeExpander());
            defaultsJS = lds.getAlias2Defaults(aliaser); 
            expressions = new string[] { aliaser(md.GetType()) + ".propertyName2DefaultValue"  +".i1",
                                     aliaser(typeof(ManyDefaultsTestData.StringValues)) + ".propertyName2DefaultValue"  + ".str3"};
            expectedValues= new object[] { md.i1, md.stringValues.str3 };
            //check that the defaults serialise OK first
            Object2Json o2J = new Object2Json();
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(defaultsJS);
            validateJSON(json, expressions, expectedValues, "testDefaultingWritingDefaultsBit.simplealias","using a verbose class aliaser");
            // try a numeric aliaser  
            aliaser =  TypeAliaserUtils.createNumericTypeNameAliaser();
            lds = df.getDefaultsForAllLinkedObjects(md, new FieldReflectionNodeExpander());
            defaultsJS = lds.getAlias2Defaults(aliaser);
            expressions = new string[] { aliaser(md.GetType()) + ".propertyName2DefaultValue"  +".i1",
                                     aliaser(typeof(ManyDefaultsTestData.StringValues)) + ".propertyName2DefaultValue"  + ".str3"};
            o2J.IndentSize = 2;
            json = o2J.toJson(defaultsJS);
            validateJSON(json, expressions, expectedValues, "testDefaultingWritingDefaultsBit.numericliaser", "using a compact numeric aliaser");

        }

    


        //check that defaults can be written out ok.
        [Test(Description="test use of defaults")]
        public void testDefaulting()
        {
            //todo autoreject invalid js aliases e.g. numberot dot contained
            //Object2Json o2j = new Object2Json();
            DefaultFinder df = new DefaultFinder();
            ManyDefaultsTestData md = new ManyDefaultsTestData();
            TypeAliaser aliaser = delegate(Type type)
            {
                return type.FullName.Replace('.', '_').Replace('+', '_');
            };
            // need to pass in class name aliasing scheme
            // this will fail if aliases are not valid js propert names
            Dictionary<string, Defaults4Class> defaultsJS;
            String[] expressions;
            object[] expectedValues;
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            LeafDefaultSet lds = df.getDefaultsForAllLinkedObjects(md, new FieldReflectionNodeExpander());
            defaultsJS = lds.getAlias2Defaults(aliaser); 
            expressions = new string[] { aliaser(md.GetType()) + ".propertyName2DefaultValue"  +".i1",
                                     aliaser(typeof(ManyDefaultsTestData.StringValues)) + ".propertyName2DefaultValue"  + ".str3"};
            expectedValues = new object[] { md.i1, md.stringValues.str3 };
            //check that the defaults serialise OK first
            Object2Json o2J = new Object2Json();
            // should test with this as false
            o2J.UseReferences = true;
            o2J.NodeExpander = nodeExpander;
            o2J.IndentSize = 2;
            String json = o2J.toJson(defaultsJS);
            validateJSON(json, expressions, expectedValues, "testDefaulting.defaultMap",  " check access to serialised defaults, class names are aliased replacing '.' with '_'");
            aliaser = TypeAliaserUtils.createNumericTypeNameAliaser();
            lds = df.getDefaultsForAllLinkedObjects(md, new FieldReflectionNodeExpander() );
            defaultsJS = lds.getAlias2Defaults(aliaser); 
            expressions = new string[] { aliaser(md.GetType()) + ".propertyName2DefaultValue"  +".i1",
                                     aliaser(typeof(ManyDefaultsTestData.StringValues)) + ".propertyName2DefaultValue"  + ".str3"};
            o2J.IndentSize = 2;
            json = o2J.toJson(defaultsJS);
            validateJSON(json, expressions, expectedValues, "testDefaulting", "check access to serialised defaults, classnames are aliases with numbers");
            
            // serialise with defaults
            o2J.OmitDefaultLeafValuesInJs = false;
            String withDefaultLeafValuesJS = o2J.toJson(md);
            // serialise without defaults
            o2J.OmitDefaultLeafValuesInJs = true;
            o2J.LeafDefaultSet = lds;
            o2J.OmitMarkAsArrayFunction = false;
            o2J.TypeAliaser = aliaser;
            String withoutDefaultsLeafValuesJS = o2J.toJson(md);
            float minimumDefaultingCompression = 2.0F;
            // check sizes is much smaller with defaults
            int maximumCompressedLength= (int) (((float) withDefaultLeafValuesJS.Length) / minimumDefaultingCompression   );
            object []vals = {withDefaultLeafValuesJS.Length};
            Assert.IsTrue(withoutDefaultsLeafValuesJS.Length < maximumCompressedLength, "without default compression length=" + withDefaultLeafValuesJS.Length +
                  " without default leaf values length=" + withoutDefaultsLeafValuesJS.Length + " but expected less than " + maximumCompressedLength + " (compression better than " +minimumDefaultingCompression +")");
            // check defaulted values are undefined if undefault is not called
            expressions = new string[] { "i2", "stringValues.str3", "stringValues.str2", "stringValues.str1", "i1", "i4", "stringValuesArr[0].str1" };            
            expectedValues = new object[expressions.Length]; // assume initialised to null !!
            md.i2 = 6789;
            expectedValues[0] = md.i2;
            withoutDefaultsLeafValuesJS = o2J.toJson(md);
            string[] extraFunctions = { o2J.getAttachId2ArrayJSFunction() };
            validateJSON(withoutDefaultsLeafValuesJS, extraFunctions, expressions, expectedValues, "testDefaulting.unrestoredvalues", "serialise ommitting defaults and in js test do not restore defaults");

             expectedValues = new object[] {md.i2, md.stringValues.str3, md.stringValues.str2, md.stringValues.str1, md.i1, md.i4, md.stringValuesArr[0].str1 };
             //Object value;
             JSExcuteUtil.JsonValueSet valueSet = new JSExcuteUtil.JsonValueSet();
             valueSet.json = o2J.LeafDefaultResolverFunctionName + "(" + withoutDefaultsLeafValuesJS + ")";
             valueSet.varname = "defaulted";
             for (int done = 0; done < expressions.Length; done++)
             {
                 valueSet.expressions2ExpectedValue[expressions[done]] = expectedValues[done];
             }
            valueSet.extraFunctions.Add(o2J.getLeafDefaultResolverJS(aliaser));
            valueSet.extraFunctions.Add(o2J.getMarkAsArrayJSFunction());
            valueSet.extraFunctions.Add(o2J.getAttachId2ArrayJSFunction());
            valueSet.significance = "serialised without default values and restore with standard restore js function";
             Dictionary<string, object> values = util.extractValuesFromJson(valueSet, GetType().Name + "." + "testDefaulting.resolveLeaves");

             for (int done = 0; done < expressions.Length; done++)
             {
                 string expression = expressions[done];
                 object expectedValue = expectedValues[done];
                 object actualValue = valueSet.expressions2ExpectedValue[expression] = values[expression];
                 //"expression=" + expression + " with reference json= " + withoutDefaultsLeafValuesJS
                 Assert.AreEqual(expectedValue, actualValue, "valueSet==" + valueSet.ToString());
             }

        }

        [Test]
        public void demoDefaultingForUserguide()
        {
            
            // create the test data
            ManyDefaultsTestData md = new ManyDefaultsTestData();
            
            // create an aliaser to create identifiers for types
            // must emit valid js property name distinct for each class 
            // typically a number is used to reduce size
            TypeAliaser aliaser = TypeAliaserUtils.createNumericTypeNameAliaser();

            //focus on serializing fields
            NodeExpander nodeExpander = new FieldReflectionNodeExpander();
            // find the defaults - this is a one off
            // only necessary once per schemachange
            DefaultFinder df = new DefaultFinder();
            // this method just searches through the type structure
            LeafDefaultSet lds = df.getDefaultsForAllLinkedObjects(md, nodeExpander);
            // create a dictionary of default values - this is a one off per schemachange
            Dictionary<string, Defaults4Class> defaultValues = lds.getAlias2Defaults(aliaser);

            Object2Json o2J = new Object2Json();
            o2J.TypeAliaser = aliaser;
            o2J.NodeExpander = nodeExpander;
            // make the format prettier
            o2J.IndentSize = 2;
            // create javascript representing the dictionary of defaults
            String defaultValuesJson = o2J.toJson(defaultValues);
            System.Console.WriteLine("defaultValuesJson=" + defaultValuesJson);

            // serialise without defaults
            o2J.OmitDefaultLeafValuesInJs = true;
            o2J.LeafDefaultSet = lds;
            o2J.OmitMarkAsArrayFunction = false;
            String withoutDefaultsLeafValuesJS = o2J.toJson(md);
            System.Console.WriteLine("withoutDefaultsLeafValuesJS=" + withoutDefaultsLeafValuesJS);

            // serialise with defaults
            o2J.OmitDefaultLeafValuesInJs = false;
            o2J.LeafDefaultSet = lds;
            o2J.TypeAliaser = aliaser;
            String withDefaultLeafValuesJS = o2J.toJson(md);

            System.Console.WriteLine("withDefaultLeafValuesJS=" + withDefaultLeafValuesJS);
            
            JSExcuteUtil.JsonValueSet valueSet = new JSExcuteUtil.JsonValueSet();
            valueSet.json = o2J.LeafDefaultResolverFunctionName + "(" + withoutDefaultsLeafValuesJS + ")";
            valueSet.varname = "defaulted";
            valueSet.extraFunctions.Add(o2J.getLeafDefaultResolverJS(aliaser));
            valueSet.extraFunctions.Add(o2J.getMarkAsArrayJSFunction());
            valueSet.significance = "serialised without default values and restore with standard restore js function";
            valueSet.expressions2ExpectedValue["stringValuesArr[1].str2"] = md.stringValuesArr[1].str2;
            valueSet.expressions2ExpectedValue["stringValuesArr[2].aReallyReallyReallyLongName"] = md.stringValuesArr[2].aReallyReallyReallyLongName;
            Dictionary<string, object> values = util.extractValuesFromJson(valueSet, GetType().Name + "." + "demoDefaultingForUserguide");
        }
    }

}
