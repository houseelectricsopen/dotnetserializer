using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Mocks;
using TextWriterExpectationLogger = Rhino.Mocks.Impl.TextWriterExpectationLogger;

namespace com.houseelectrics.serializer.test
{
    public delegate void JsonExpectationBlock(JsonExploreListener jsonListener, string json);
    
    public class TestUtil
    {
        public static void run(out object returnValue, out Exception exception, Func<object> code)
        {
            returnValue = null;
            exception = null;
            try
            {
                returnValue = code.Invoke();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }



        public static void testJsonStructure(string json, JsonExpectationBlock expectation, string testname)
        {
            RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);
            System.Console.WriteLine("test: " +  testname + "testing json=" + json);
            var mocks = new MockRepository();
            JsonExploreListener theMock = mocks.StrictMock<JsonExploreListener>();
            using (mocks.Ordered())
            {
                expectation(theMock, json);
            }
            theMock.Replay();
            (new JSONExplorerImpl()).explore(json, theMock);
            theMock.VerifyAllExpectations();
        }


    }
}
