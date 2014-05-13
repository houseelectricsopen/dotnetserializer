<%@ Application Language="C#" %>
<%@ Import Namespace="com.houseelectrics.serializer" %>

<script runat="server">
    
    public class ManyDefaultsTestData
    {
        internal int i1 = 12345;
        internal int i2 = 234;
        public int i4 = 432;
        public class StringValues
        {
            public string str1 = "abcd";
            internal string str2 = "bcde";
            internal string str3 = null;            
            internal string aReallyReallyReallyLongName = null;
        }
        internal StringValues stringValues = new StringValues();
        internal StringValues[] stringValuesArr = { new StringValues(), new StringValues(), new StringValues() };
    }

    // todo provide WebActivator version
    public static Object2Json Object2Json { get; private set; }

    void Application_Start(object sender, EventArgs e) 
    {
        // Code that runs on application startup
        // create global serialiser here
        Object2Json = new Object2Json();
        Object2Json.UseReferences = true;
        Object2Json.NodeExpander = new FieldReflectionNodeExpander();
        
        // add in defaults
        Object2Json.LeafDefaultSet = new LeafDefaultSet();
        Object2Json.TypeAliaser = TypeAliaserUtils.createNumericTypeNameAliaser();
        
        DefaultFinder df = new DefaultFinder();        
        ManyDefaultsTestData md = new ManyDefaultsTestData();
        //TypeAliaserUtils
        LeafDefaultSet lds = df.getDefaultsForAllLinkedObjects(md, Object2Json.NodeExpander);
        Object2Json.LeafDefaultSet.Add(lds);
        
        
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs

    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
