using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionRequirementAttribute = com.houseelectrics.serializer.security.PermissionRequirementAttribute;
using PermissionFilterHelper = com.houseelectrics.serializer.security.PermissionFilterHelper;
using PermissionFilterExplorer = com.houseelectrics.serializer.security.PermissionFilterExplorer;
using com.houseelectrics.serializer.security;
using NUnit.Framework;
using SecurityException = System.Security.SecurityException;


namespace com.houseelectrics.serializer.test.security
{
    // 
    public class SecurityTest
    {
        
        public class LogonDetails
        {
            [PermissionRequirementAttribute("ViewPasswords")]
            public string password;
            [PermissionRequirementAttribute("ViewPasswords")]
            public string Password { get; set; }

            public string username;
            public string Username { get; set; }
        }

        public class User
        {
            public string id;
            public string Id { get; set; }
            public string nickname;
            public string Nickname { get; set; }
            [PermissionRequirementAttribute("PasswordAdministration")]
            public LogonDetails logonDetails;
            [PermissionRequirementAttribute("PasswordAdministration")]
            public LogonDetails LogonDetails { get; set; }


        }
        
        Object2Json createSecureObjectToJson(string[] currentPermissions, bool throwPermissionOnDenial, NodeExpander nodeExpander)
        {
            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = nodeExpander;
            PermissionFilterExplorer pfe = new PermissionFilterExplorer();
            pfe.ThrowException = throwPermissionOnDenial;
            o2j.ExplorerFactory = () =>
            {
                pfe.UnderlyingExplorer = new ObjectExplorerImpl();
                pfe.CheckPermissionByName = (strPermission) =>
                {
                    return currentPermissions.Contains(strPermission);
                };
                return pfe;
            };
            return o2j;
        }

        public class LogonDetailsUserguideTestData
        {
            [PermissionRequirementAttribute("ViewPasswords")]
            public string password;

            [PermissionRequirementAttribute("ViewUsernames")]
            public string username;
        }

        
        [Test]
        public void testFieldLeafDenialNoExceptionForUserGuide()
        {

            //create sensitive data
            LogonDetailsUserguideTestData sensitiveData = new LogonDetailsUserguideTestData();
            sensitiveData.password = "secret123";
            sensitiveData.username = "Andrew Eldritch";

            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new FieldReflectionNodeExpander();

            // inject a simple security check
            string[] currentPermissions=null;
            Func<string, bool> permissionCheck = (permission) => { return currentPermissions.Contains(permission); };
            bool throwExceptionOnPermissionDenial = false;
            o2j.injectPermissionFilter(permissionCheck, throwExceptionOnPermissionDenial);

            currentPermissions = new string[] { "ViewUsernames" };
            string json = o2j.toJson(sensitiveData);
            System.Console.WriteLine("json=" + json);
            Assert.IsTrue(json.IndexOf(sensitiveData.username)>0);
            Assert.IsTrue(json.IndexOf(sensitiveData.password) < 0);

            currentPermissions = new string[] { "ViewUsernames", "ViewPasswords" };
            //create simple permission check for test purposes
            json = o2j.toJson(sensitiveData);
            System.Console.WriteLine("json=" + json);
            Assert.IsTrue(json.IndexOf(sensitiveData.username) > 0);
            Assert.IsTrue(json.IndexOf(sensitiveData.password) > 0);

        }
        
        [Test]
        public void testFieldLeafDenialWithExceptionForUserGuide()
        {

            //create sensitive data
            LogonDetailsUserguideTestData sensitiveData = new LogonDetailsUserguideTestData();
            sensitiveData.password = "secret432";
            sensitiveData.username = "Tyrion Lannister";

            Object2Json o2j = new Object2Json();
            o2j.NodeExpander = new FieldReflectionNodeExpander();

            // inject a simple security check
            string[] currentPermissions=null;
            Func<string, bool> permissionCheck = (permission) => { return currentPermissions.Contains(permission); };
            bool throwExceptionOnPermissionDenial = true;
            o2j.injectPermissionFilter(permissionCheck, throwExceptionOnPermissionDenial);

            currentPermissions = new string[] { "ViewUsernames" };

            Exception exception;
            object returnValue;
            TestUtil.run(out returnValue, out exception, () =>
            {
                string json = o2j.toJson(sensitiveData);
                return null;
            });
            /*expecting an exception like this: 
 cannot access property com.houseelectrics.serializer.test.security.SecurityTest+LogonDetailsUserguideTestData.password 
             *    without permisson ViewPasswords
            */
            Assert.AreEqual(typeof(SecurityException), exception.GetType(), "expect security exception");
            Assert.IsTrue(exception.Message.IndexOf("ViewPasswords") >= 0);
            System.Console.WriteLine("failed with exception: " + exception.Message);



        }

        [Test]
        public void testFieldLeafDenialNoException()
        {
            string []strPermissions = {"ViewUsernames"};
            Object2Json o2j;

            LogonDetails sensitiveData = new LogonDetails();
            sensitiveData.password = "secret123";
            sensitiveData.username = "Andrew Eldritch";
            //to do check no check code specified situation
            string json;
            JsonExpectationBlock expectation;

            o2j = createSecureObjectToJson(strPermissions, false, new FieldReflectionNodeExpander());
            json = o2j.toJson(sensitiveData);
            //test json structure            
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("username", sensitiveData.username, true);
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testFieldLeafDenialNoException (deny)");

            //check that the right permission gives access
            strPermissions = new string[] {"ViewPasswords"};
            o2j = createSecureObjectToJson(strPermissions, false, new FieldReflectionNodeExpander());
            json = o2j.toJson(sensitiveData);
            //test json structure            
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("password", sensitiveData.password, true);
                theMock.JsonLeaf("username", sensitiveData.username, true);
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testFieldLeafDenialNoException (permit)");
        
        }

        [Test]
        public void testPropertyLeafDenialNoException()
        {
            string[] strPermissions = { "ViewUsernames" };
            Object2Json o2j;

            LogonDetails sensitiveData = new LogonDetails();
            sensitiveData.Password = "secret123";
            sensitiveData.Username = "Andrew Eldritch";
            //to do check no check code specified situation
            string json;
            JsonExpectationBlock expectation;

            o2j = createSecureObjectToJson(strPermissions, false, new PropertyReflectionNodeExpander());
            json = o2j.toJson(sensitiveData);
            //test json structure            
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("Username", sensitiveData.Username, true);
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testPropertyLeafDenialNoException (deny)");

            //check that the right permission gives access
            strPermissions = new string[] { "ViewPasswords" };
            o2j = createSecureObjectToJson(strPermissions, false, new PropertyReflectionNodeExpander());
            json = o2j.toJson(sensitiveData);
            //test json structure            
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("Password", sensitiveData.Password, true);
                theMock.JsonLeaf("Username", sensitiveData.Username, true);
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testPropertyLeafDenialNoException (permit)");

        }


        [Test]
        public void testLeafDenialException()
        {
            string[] strPermissions = { "viewUsernames" };
            Object2Json o2j = createSecureObjectToJson(strPermissions, true, new FieldReflectionNodeExpander());

            LogonDetails sensitiveData = new LogonDetails();
            sensitiveData.password = "secret123";
            //to do check no check code specified situation
            object returnValue;
            Exception exception;
            TestUtil.run(out returnValue, out exception, () =>
                {
                    string json = o2j.toJson(sensitiveData);
                    return null;
                });
            Assert.AreEqual(typeof(SecurityException), exception.GetType(), "expect security exception");
            Assert.IsTrue(exception.Message.IndexOf("ViewPasswords")>=0);

        }

        [Test]
        public void testPropertyDenialException()
        {
            string[] strPermissions = { "viewUsernames" };
            Object2Json o2j = createSecureObjectToJson(strPermissions, true, new PropertyReflectionNodeExpander());

            LogonDetails sensitiveData = new LogonDetails();
            sensitiveData.Password = "secret123";
            //to do check no check code specified situation
            object returnValue;
            Exception exception;
            TestUtil.run(out returnValue, out exception, () =>
            {
                string json = o2j.toJson(sensitiveData);
                return null;
            });
            Assert.AreEqual(typeof(SecurityException), exception.GetType(), "expect security exception");
            Assert.IsTrue(exception.Message.IndexOf("ViewPasswords") >= 0);

        }

        [Test]
        public void testFieldReferenceDenialNoException()
        {
            string[] strPermissions = { "ViewUsernames" };
            Object2Json o2j;

            LogonDetails sensitiveData = new LogonDetails();
            sensitiveData.password = "secret123";
            sensitiveData.username = "Andrew Eldritch";
            User user = new User();
            user.id = "123";
            user.nickname = "Dark Lord";
            user.logonDetails = sensitiveData;
            //to do check no check code specified situation
            string json;
            JsonExpectationBlock expectation;

            o2j = createSecureObjectToJson(strPermissions, false, new FieldReflectionNodeExpander());
            json = o2j.toJson(user);
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("id", user.id, true);
                theMock.JsonLeaf("nickname", user.nickname, true);
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testReferenceDenialNoException (deny)");

            //check that the right permission gives access
            strPermissions = new string[] { "PasswordAdministration" };
            o2j = createSecureObjectToJson(strPermissions, false, new FieldReflectionNodeExpander());
            json = o2j.toJson(user);
            //test json structure            
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("id", user.id, true);
                theMock.JsonLeaf("nickname", user.nickname, true);
                theMock.JsonStartObject("logonDetails", json.IndexOf('{', 2));
                theMock.JsonLeaf("username", user.logonDetails.username, true);
                theMock.JsonEndObject(testjson.IndexOf("}"));
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testReferenceDenialNoException (permit)");
        }

        [Test]
        public void testPropertyReferenceDenialNoException()
        {
            string[] strPermissions = { "ViewUsernames" };
            Object2Json o2j;

            LogonDetails sensitiveData = new LogonDetails();
            sensitiveData.Password = "secret123";
            sensitiveData.Username = "Andrew Eldritch";
            User user = new User();
            user.Id = "123";
            user.Nickname = "Dark Lord";
            user.LogonDetails = sensitiveData;
            //to do check no check code specified situation
            string json;
            JsonExpectationBlock expectation;

            o2j = createSecureObjectToJson(strPermissions, false, new PropertyReflectionNodeExpander());
            json = o2j.toJson(user);
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("Id", user.Id, true);
                theMock.JsonLeaf("Nickname", user.Nickname, true);
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testPropertyReferenceDenialNoException (deny)");

            //check that the right permission gives access
            strPermissions = new string[] { "PasswordAdministration" };
            o2j = createSecureObjectToJson(strPermissions, false, new PropertyReflectionNodeExpander());
            json = o2j.toJson(user);
            //test json structure            
            expectation = (theMock, testjson) =>
            {
                theMock.JsonStartObject(null, 0);
                theMock.JsonLeaf("Id", user.Id, true);
                theMock.JsonLeaf("Nickname", user.Nickname, true);
                theMock.JsonStartObject("LogonDetails", json.IndexOf('{', 2));
                theMock.JsonLeaf("Username", user.LogonDetails.Username, true);
                //theMock.JsonLeaf("password", user.logonDetails.password, true);
                theMock.JsonEndObject(testjson.IndexOf("}"));
                theMock.JsonEndObject(testjson.LastIndexOf("}"));
            };
            TestUtil.testJsonStructure(json, expectation, "testPropertyReferenceDenialNoException (permit)");
        }
    }
}
