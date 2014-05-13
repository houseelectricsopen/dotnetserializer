using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer.test
{
    public class JSExecuteUtilJavascript
    {
        public const string ShowSourceFunctionName = "showjssource";
       public const string ShowSourceFunction =
           @"function " + ShowSourceFunctionName + @"(f)
{
var w = window.open('','','width=400, resizable=yes, scrollbars');
w.document.write('<pre>' + f + '</pre>');
w.document.close();
w.focus();
};";
       public const string AssertEqualFunction =
@"var assertEqual = function(expected, actual, message)
{
    if (expected!=actual)
       {
            throw message +  ' expected:' + expected + ' actual:' + actual;
       }
};";

       public static string getRunTest4TableRowFunction(string TablePageRunTestFunctionName, string crossimagesrc, string tickimagesrc, string questionmarkimagesrc) 
       {
return @"function " + TablePageRunTestFunctionName + @"(passimageid, testFunction, clear)
{
var theimage = document.getElementById(passimageid);
if (clear)
  {
     theimage.src=runtest.questionmarkimagesrc;
     return;
  }
try
{
testFunction();
theimage.src=runtest.tickimagesrc;
}
catch (err)
{
theimage.src=runtest.crossimagesrc;
}
}
runtest.crossimagesrc='" + crossimagesrc + @"';
runtest.tickimagesrc='" + tickimagesrc + @"';
runtest.questionmarkimagesrc='" + questionmarkimagesrc + @"';
";
      }

    }



}
