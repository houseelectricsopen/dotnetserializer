using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JScript;
using VsaEngine = Microsoft.JScript.Vsa.VsaEngine;
using Eval = Microsoft.JScript.Eval;
using StreamWriter = System.IO.StreamWriter;
using StackFrame = System.Diagnostics.StackFrame;
using StackTrace = System.Diagnostics.StackTrace;
using TestAttribute = NUnit.Framework.TestAttribute;
using Directory = System.IO.Directory;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace com.houseelectrics.serializer.test
{
     public class JSExcuteUtil
    {
        public static VsaEngine vsaEngine = VsaEngine.CreateEngine();

        String testdir = "testtemp";

        private List<JsonValueSet> history = new List<JsonValueSet>();

        public class JsonValueSet 
        {
            public StackFrame stackFrame;
            public TestAttribute testAttribute;
            public string getDescription()
            {
                StringBuilder sb = new StringBuilder();

                if (testAttribute != null ) sb.Append(testAttribute.Description);
                if (significance != null) sb.Append(" - " +significance);
                return sb.ToString();
            }
            public string getSourceDescription()
            {
                return stackFrame.GetFileName()  + stackFrame.GetMethod().DeclaringType.FullName +
                     "::" + stackFrame.GetMethod().Name;
            }

            public string testId; 
            public string filename;
            public string varname;
            public string json;
            public string significance;
            public Dictionary<string, object> expressions2ExpectedValue = new Dictionary<string, object>();
            public List<string> extraFunctions = new List<string>();
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("testId=" + testId);
                sb.Append(" filename=" + filename);
                sb.Append(" varname=" + varname);
                sb.Append(" json==" + json);
                sb.Append("\r\n extraFunctions=");
                extraFunctions.ForEach((f) => { sb.Append( "\r\n" + f); });

                return sb.ToString();
            }
        }



        public string testId2FunctionName(string testId)
        {
        return "test_" + testId.Replace('/', '_').Replace(".", "_");
        }

        public void writeTestFunction(StreamWriter sw, JsonValueSet valueSet)
        {
            sw.WriteLine("function " + testId2FunctionName(valueSet.testId) + @"()
                     {");
            sw.WriteLine("var " + valueSet.varname + "=" + valueSet.json + ";");
            valueSet.expressions2ExpectedValue.Keys.ToList().ForEach(
                (expr) =>
                {
                    Object expected = valueSet.expressions2ExpectedValue[expr];
                    if (expected == null) expected = "null";
                    else if (expected.GetType() == typeof(string) || expected.GetType()==typeof(Char)) expected = ("'" + expected + "'");
                    string fullexpression = valueSet.varname + "." + expr;
                    sw.WriteLine("assertEqual(" + expected + "," + fullexpression +
                         ", '" + fullexpression.Replace("'", "\\'") + "');");
                }
                );
            sw.WriteLine(
@"
                } ");
        }


        public void writeTestHtml(JsonValueSet valueSet)
        {
           
            valueSet.filename = testdir + '\\' + valueSet.testId + ".html";
            System.Console.WriteLine("writing test to " + valueSet.filename);
            DirectoryInfo di = System.IO.Directory.CreateDirectory(testdir);
            System.Console.WriteLine("wrote test to " + di.FullName);
            System.IO.FileInfo fi = new System.IO.FileInfo(valueSet.filename);
            fi.Directory.Create();
            StreamWriter sw = new StreamWriter(valueSet.filename);
            sw.WriteLine(
@"<html>
<script>
");
            foreach (string f in valueSet.extraFunctions)
            {
                sw.WriteLine(f);
                sw.WriteLine();
            }

            sw.WriteLine("var " + valueSet.varname + "=" + valueSet.json + ";");
            sw.WriteLine();
            //writeTestFunction(sw, valueSet);


sw.WriteLine(@"</script>
<body>
<pre>");

            sw.WriteLine(valueSet.varname + "=>" + valueSet.json);
            valueSet.expressions2ExpectedValue.Keys.ToList().ForEach(
                (expression) =>
                {
                    string strFullExpression = valueSet.varname + "." + expression;
                    sw.WriteLine(strFullExpression + "==<script>document.write(" + strFullExpression + ")</script>");
                }
                );
            sw.WriteLine();
            sw.WriteLine("</pre>");
            sw.Flush();
            sw.Close();
        }

        // MethodBase mb = MethodBase.GetCurrentMethod();
        // MethodBase.

        public TestAttribute getCurrentNunitTest()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();
            IEnumerable<TestAttribute>  atts =frames.SelectMany((sf) => { return (TestAttribute[])sf.GetMethod().GetCustomAttributes(typeof(TestAttribute), true); });
            return atts.First();
        }

        public StackFrame getCurrentNunitStackFrame()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();
            IEnumerable<StackFrame> sfs = frames.Where((sf) => {return sf.GetMethod().GetCustomAttributes(typeof(TestAttribute), true).Count() > 0;});
            if (sfs.Count() > 1) throw new Exception("nested JUnit Tests not supported");
            return sfs.First();
        }

        public void writeTestTablePage()
        {
            string filename = testdir + '\\' + "testTable" + ".html";
            System.Console.WriteLine("writing test table to " + filename);
            DirectoryInfo di = Directory.CreateDirectory(testdir);
            System.Console.WriteLine("wrote test table to " + di.FullName);
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);
            fi.Directory.Create();
            StreamWriter sw = new StreamWriter(filename);
            writeTestTablePage(sw, history);
            sw.Close();
        }
         
         public void writeTestTablePage(StreamWriter sw, List<JsonValueSet> tests)
         {
sw.WriteLine(
@"<html>
<script>
");
Dictionary<string, string> extraFunctionsPrinted = new Dictionary<string, string>();
sw.WriteLine(JSExecuteUtilJavascript.AssertEqualFunction);

             foreach (JsonValueSet valueSet in tests)
{
    foreach (string f in valueSet.extraFunctions)
    {
        if (!extraFunctionsPrinted.ContainsKey(f))
        {
            extraFunctionsPrinted[f] = f;
            sw.WriteLine(f);
            sw.WriteLine();
        }
    }
    writeTestFunction(sw, valueSet);
    sw.WriteLine();
}

sw.WriteLine();
sw.WriteLine(JSExecuteUtilJavascript.getRunTest4TableRowFunction(TablePageRunTestFunctionName, crossimagesrc, tickimagesrc, questionmarkimagesrc) );
sw.WriteLine("</script>");
string runAllTestsFunctionName = "runAllTests";
sw.WriteLine(@"<body onload=" + "\"" + runAllTestsFunctionName + "(false)\"" +  @">
<h1>
Serializer Browser Compatibility Tests - created by " + GetType().FullName + " at " + (DateTime.Now) + @"
</h1>");
string[] headers = { "pass<br>fail", "name", "description", ".net source", "java script<br> test source" };
List<List<string>> rows = new List<List<string>>();
sw.WriteLine(@"<script>
function " + runAllTestsFunctionName + "(clear) {");
for (int index = 0; index < tests.Count; index++ )
{
    JsonValueSet test = tests[index];
    List<string> row = new List<string>();
    string imageid = "row" + index + ".pass.image";
    string testfunctionname = testId2FunctionName(test.testId);
    string runtestcall = TablePageRunTestFunctionName +"('" + imageid + "', " + testfunctionname + ", clear)";
    sw.WriteLine(runtestcall+";");
    string runrow = "<img id='" + imageid + "' width='20' height='24'/>" +
       "<a href=\"javascript:"+ runtestcall + "\">run</a>";
    row.Add(runrow);
    row.Add(test.testId );
    row.Add(test.getDescription());
    row.Add(test.getSourceDescription());
    
    row.Add("<a href=\"javascript:" + JSExecuteUtilJavascript.ShowSourceFunctionName +  "(" + testfunctionname + ")\">show</a>");
    rows.Add(row);
}
sw.WriteLine(@"}
var clear=false;"
+ JSExecuteUtilJavascript.ShowSourceFunction + @"  
</script>");
sw.WriteLine("<a href=\"javascript:" + runAllTestsFunctionName + "()\">run all tests</a>");
sw.WriteLine("<br>");
sw.WriteLine("<a href=\"javascript:" + runAllTestsFunctionName + "(true)\">clear all tests</a>");
sw.WriteLine("<br>");
writeHTMLtable(sw, headers, rows);
sw.WriteLine();
//page default is to show result
//sw.WriteLine("<script> debugger; " + runAllTestsFunctionName + "(false);</script>");
sw.WriteLine(@"</body>");
sw.WriteLine("</html>");
         }

         public Dictionary<string, object> extractValuesFromJson(JsonValueSet valueSet, string testId)
        {
             // try to look up JunitAttribute in stack here !

            TestAttribute ta = getCurrentNunitTest();
            valueSet.stackFrame = getCurrentNunitStackFrame();
            valueSet.testAttribute = ta;

            history.Add(valueSet);

             valueSet.testId = testId;

            writeTestHtml(valueSet);

          Dictionary<string, object> expressions2ActualValue = new Dictionary<string, object>();

          valueSet.expressions2ExpectedValue.Keys.ToList().ForEach(
              (expression) =>
              {
                  string js;
                  if (valueSet.extraFunctions.Count() > 0)
                  {
                      js = "(function() {";
                      foreach (string f in valueSet.extraFunctions)
                      {
                          js = js + "\r\n" + f;
                      }
                      js = js + "\r\n return (" + valueSet.json + ");";
                      js = js + " })()." + expression;
                  }
                  else
                  {
                      js = "(" + valueSet.json + ")." + expression;
                  }
                  Object result = execute(js);
                  //todo maybe System.DBNull actually means undefined !
                  if (result != null && result.GetType()==(typeof(System.DBNull)) ) result = null;
                  expressions2ActualValue[expression]= (result);
              }
              );
             return expressions2ActualValue;
        }

         //assume only 1 table for time being
         private string getTableCellId(int row, string header)
           {
               return "row" + row + "." + header;
           }

         private void writeHTMLtable(StreamWriter sw, string[] headers, List<List<string>> rows)
         {
             sw.WriteLine(@"
<table border=1>
<tr>");
          foreach (string header in headers)
          {
              sw.Write("<th>" + header + "</th>");
          }
            sw.WriteLine("</tr>");
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++ )
                    {
                        List<String> row=rows[rowIndex];
                        sw.Write("<tr>");
                        for (int done = 0; done < headers.Length; done++)
                        {
                            string value = (row.Count - 1) < done ? "" : row[done];
                            sw.Write("<td>" + value + "</td>");
                        }
                        sw.WriteLine("</tr>");
                    }
            sw.WriteLine("</table>");
         }

    public object execute(String strJS)
    {
        object result;
        try
        {
            
            result = Eval.JScriptEvaluate(strJS, vsaEngine);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("failed to execute:\r\n" + strJS + " because " + ex.Message);
            result = ex;
        }
        return result;
    }

    public static string TablePageRunTestFunctionName="runtest";

    /**derived from media/tick.png derived from tick.svg all put through 
    http://websemantics.co.uk/online_tools/image_to_data_uri_convertor/   **/

     const string tickimagesrc = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAbCAYAAAB836/YAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAAagAAAGoB3Bi5tQAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAHGSURBVEiJpdRfaI1hGADw305alGg3rlYu3MmFO8uFuXDhzm4kXMjCLoQWtbJLuUBNboiWG8Ra5LBShG0yWWlIlAxN8iex2s6ZY+x1s17ny9nOOd/53t6b93m+X8/3fc/zCSGoZWu20KB+WQeQqQkLIZB1WJhdg+5k1HDVHapbrkl7PKg3VFt1D5yP1X3zQZuG9NgpzfJ+RfCe9hCCdBh1nhmI2EevrLEoPXjJnogFQVZrjFWNbdFgzLuIjXqMTHrwruMRmxFc0JKIV4UdtcoPkxF84fZ/OVWBw65FrOC3M9anBnVrMW0mgiN6SuZVhK1U77WnEcuZcszq9GCfjkSbPHJuztyyWIdGX3yN2HfjOq1IDw7pTlR334l58+cNnrZWrmheP/tkt2UVgU7aYaslieBz/Ynq+nSWfaIQAl02GJfz0MUY6NFa1CTBe2+ss7gsaL9GY0bjKN100C5LjXmbqO6yvRV1hGG3Ejfm5PUXTUQQvDSCBZWBN+xTMJ0AiteM4KxtlWD/3uF1baYUSoJPDFSKJb/yVTvl/UxgBX902ZgKDCHQa7ucfNGIZavBSja2KzabMGlCwRFNNYOz6Ca9c/8A5tt/AWXIMuPZc9CQAAAAAElFTkSuQmCC";
     const string crossimagesrc="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAYCAYAAAD6S912AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAAagAAAGoB3Bi5tQAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAIVSURBVDiNndXfa81xHMfxx3fHcCPJr1IjpcXFJHdu/AWyEnPHXFIuFkWZtZqk1SIsUy6IZhdkjGwxy4XQXGzKMooxTqwR2sxm28fNzuns7JzNOZ9617fer56fz/vzfr++HyEEuQbmPeTWbSpRMC2XD/AedYEQCM+5d4iivIFX2TfKRAIYCP30NVCaM/AMW77zIxWWiGHGmqgtkLJqoqikOYoqoyiKSVtHo2hVKZeXsDg9BwsonORXcvcjFPfxOhC6aK1mdSK3mcIXPMh0skS0cT1Z8gGK3tKVKojz4QrbQghaOT8brIvOchaFEChnZQ/PMgl/M9bBjVEms8E+Eq9mfbLL7XOUMlv85M9Ftk+bw3p2DPAtV9g4oZFjM4Z+ahy2fqI/F2AbjRldlPg4Rcn7qS7PFb8YP8HuWYEhBMdY84bn/wMdYvQmJxGb1cv7WdrD4/8tvZNHVazLCqynOJ7jfcaJX6ZsBvA4K97Rnc8IjTB+l7NJL++NooVlNK1lY7pP/zI5yGAmDyfWQmLDjCRP94TGbLu3UHeJDXFeZtN00IyYEII2arMJn9KS6GQNy3u5n655RVcZy0II3OHgWBav9tK9h+Wp97yT+Z1cmJzSfOZLFZuSXW7hYibYF76eTBGmRzuHBxg6x66090Z0i9MTaUPbwM65/uAVbMjqlCaqx5iYIFyjKp/Ha8YcXqXiJtfyhYUQ/AM+zdCyi+sN3QAAAABJRU5ErkJggg==";
     const string questionmarkimagesrc="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAAYCAYAAAAcYhYyAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAAVQAAAFUBceom8QAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAGQSURBVDiNpdS/axRBGMbxz56JMXhY2aRR7AwIsbstgo34J1io2NgottpZCtqp2AviHyBoK9poMSJI/NFoECws0khQ/MGBl9di57jNsXe74gvDPvvOw3femXmZIiL8byw0ZstiAadwFAewjGd4KsVo2l7sqqQs9uMGzuJgA34Ll6R41Awpi0N4jLU8t43nGGIdKzk/wgUpHuyGlEUPb3AMgWu4JcUwL7CIq7lK+I4VKX6BiBADZ2Ig8rgeERrHwN2a7/w438vki/k7xJ2GsxjH/Zo+Mha9qcSmFF/nQOr9sDWBVOexhA94MgcAJ2r681gUnZutLA7jPfp4h+NS7FSVdAMUuJcBI1weA7pDqus9mfVNKV7UJ9u3UxZreIVFvMS6FH/qli6VXMmAHzg3DWiHlMVenM5/D6X41GRrq2QZ+7LemGVqg+yp6f4sU/N7Monfqs58nUdjdG+2OdFWSRXVS7eKL1JsT0+3X3FZ3FZd71uThvtHSNXmS1mvNhm6bOcjdrCJb02GLm3fR0jxc5blL/VsmgRQaoWoAAAAAElFTkSuQmCC";
         
         //"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABEAAAAYCAYAAAAcYhYyAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAAVQAAAFUBceom8QAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAGQSURBVDiNpdS/axRBGMbxz56JMXhY2aRR7AwIsbstgo34J1io2NgottpZCtqp2AviHyBoK9poMSJI/NFoECws0khQ/MGBl9di57jNsXe74gvDPvvOw3femXmZIiL8byw0ZstiAadwFAewjGd4KsVo2l7sqqQs9uMGzuJgA34Ll6R41Awpi0N4jLU8t43nGGIdKzk/wgUpHuyGlEUPb3AMgWu4JcUwL7CIq7lK+I4VKX6BiBADZ2Ig8rgeERrHwN2a7/w438vki/k7xJ2GsxjH/Zo+Mha9qcSmFF/nQOr9sDWBVOexhA94MgcAJ2r681gUnZutLA7jPfp4h+NS7FSVdAMUuJcBI1weA7pDqus9mfVNKV7UJ9u3UxZreIVFvMS6FH/qli6VXMmAHzg3DWiHlMVenM5/D6X41GRrq2QZ+7LemGVqg+yp6f4sU/N7Monfqs58nUdjdG+2OdFWSRXVS7eKL1JsT0+3X3FZ3FZd71uThvtHSNXmS1mvNhm6bOcjdrCJb02GLm3fR0jxc5blL/VsmgRQaoWoAAAAAElFTkSuQmCC"         

/*        
         public static string GetFileStreamFunction=
@"function getFileTextStreamForWriting(filename)
{
var ForReading = 1, ForWriting = 2, ForAppending = 8;
var TristateUseDefault = -2, TristateTrue = -1, TristateFalse = 0;

var fso = new ActiveXObject(""Scripting.FileSystemObject"");

// Create the file, and obtain a file object for the file.
fso.CreateTextFile(filename);
var fileObj = fso.GetFile(filename);

// Open a text stream for output.
var ts = fileObj.OpenAsTextStream(ForWriting, TristateUseDefault);

// Open a text stream for input.
//ts = fileObj.OpenAsTextStream(ForReading, TristateUseDefault);
//while (!ts.AtEndOfStream) {
//    var textLine = ts.ReadLine();
//    WScript.StdOut.WriteLine (textLine + ""<br />"");
return ts;
}";*/ 
    }
 
}
