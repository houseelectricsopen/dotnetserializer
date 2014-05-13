using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace com.houseelectrics.serializer.test
{
    public class JSExecuteUtilTest
    {
        JSExcuteUtil executor = new JSExcuteUtil();
    
        [Test]
        public void testExtractValues()
        {
            JSExcuteUtil.JsonValueSet valueSet= new JSExcuteUtil.JsonValueSet();
            valueSet.varname = "o";
            String expectedValue = "hello";
            String expectedValue2 = "goodbye";
            String json = "{a:{b:{c:{d:'" + expectedValue + "',e:'" + expectedValue2 + "' }}}}";
            valueSet.json = json;
            valueSet.expressions2ExpectedValue["a.b.c.d"]=expectedValue;
            valueSet.expressions2ExpectedValue["a.b.c.e"]=expectedValue2;
            Dictionary<string, object> expression2Value= executor.extractValuesFromJson(valueSet, GetType().Name + ".testExtractValues");
            Assert.NotNull(expression2Value, "should getSomeValues");
            Assert.AreEqual(expectedValue, expression2Value["a.b.c.d"]);
            Assert.AreEqual(expectedValue2, expression2Value["a.b.c.e"]);

        }

        [Test]
        public void testFunctionUse()
        {
            String js;
            object result;

            js =
                @"( function() {" +
                @"    var testfunc = function() { return 321;}
                    function testfunc2() {return 432;}
                    var result = testfunc2();
                    return result;
                 " +
@"}) (); ";

            result = executor.execute(js);
            System.Console.WriteLine("result=" +  result);
        }

        [Test]
        public void test()
        {
            String js;
            String expectedResult;
            object result;
            
            expectedResult = "abc";
            js = "'" + expectedResult + "'";
            result = executor.execute(js);
            Assert.AreEqual(expectedResult, result, "expected input:" + expectedResult + "==" + "output:" + result);

            
            js = @"(
                    function () 
                       {
                       var test = function() {return 4;}
                       var val = {a: {b: {c : { d:'" + expectedResult +@"'}}} };
                       return val.a.b.c.d; 
                        }
                   )();
                   ";
            result = executor.execute(js);
            Assert.AreEqual(expectedResult, result, "expected input:" + expectedResult + "==" + "output:" + result);

        }
    }
}
