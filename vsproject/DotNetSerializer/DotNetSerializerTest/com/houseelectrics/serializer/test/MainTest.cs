using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContractSupportTest = com.houseelectrics.serializer.test.datacontract.DataContractSupportTest;
using SecurityTest = com.houseelectrics.serializer.test.security.SecurityTest;
using PathTest = com.houseelectrics.serializer.test.path.PathTest;
namespace com.houseelectrics.serializer.test
{

    public class MainTest
    {
        public static void PauseWithMessage(string message)
        {
            System.Console.WriteLine(message);
            System.Console.ReadLine();
        }

        public static void Main(string[] args)
        {
            (new PathTest()).testPath();
            (new TestExplorer()).testExploration();
            

            (new JSExecuteUtilTest()).testExtractValues();
            (new JSExecuteUtilTest()).test();
            (new JSExecuteUtilTest()).testFunctionUse();
            
            (new TestReflectiveNodeExpander()).simpleExpandByField();
            (new TestReflectiveNodeExpander()).simpleExpandByProperty();

            TestToJson toJsonTest = new TestToJson();
            
            toJsonTest.demoDefaultingForUserguide();
            toJsonTest.testIndexedWithNull();
            toJsonTest.testDemoIndexedPropertiesForUserguide();
            toJsonTest.testDemoHashedPropertiesForUserguide();
            toJsonTest.testToJsonFields();
            toJsonTest.testToJsonProperties();
            toJsonTest.testToJsonLeafTypesViaProperties();
            toJsonTest.testToJsonLeafTypesViaFields();
            toJsonTest.testToJsonPropertiesIndexed();
            toJsonTest.testSimpleMap();
            toJsonTest.testToJsonHierarchy();
            toJsonTest.testUseReferencesSimple();
            toJsonTest.testUseReferences();
            toJsonTest.testDefaulting();
            toJsonTest.testDefaultingWritingDefaultsBit();
            toJsonTest.testEnumWrite();            

            JsonExplorationTest explorationTest = new JsonExplorationTest();

            explorationTest.testArrayWithfunction();
            explorationTest.testEmptyObject();
            explorationTest.testEscapedDoubleQuoteJsonValue();
            explorationTest.testDoubleQuoteJsonValue();
            explorationTest.testSingleQuoteJsonValue();
            explorationTest.testBasicJsonExplore();
            explorationTest.testJsonExploreBasicLeaf();
            explorationTest.testBasicJsonPropertyExplore();
            explorationTest.testJsonExploreNestedObjects();
            explorationTest.testArray();
            explorationTest.testEmbeddedFunction();
            explorationTest.testArrayWithfunction();

            TestFromJson fromJsonTest = new TestFromJson(); 
            fromJsonTest.testSimpleObjectNoTypeHints();
            fromJsonTest.testSimpleObjectHintAsParameter();
            fromJsonTest.testSimpleObjectHintAsAttribute();
            fromJsonTest.testSimpleObjectIncorrectHintAsAttribute();
            fromJsonTest.testFields();
            fromJsonTest.testFields4UserGuide();
            fromJsonTest.testFields4UserGuideTypeHint();
            fromJsonTest.testProperties();
            fromJsonTest.testToJsonLeafTypesViaFields();
            fromJsonTest.testToJsonLeafTypesViaProperties();
            fromJsonTest.testUnknownFunction();
            fromJsonTest.testToJsonEmbedded();
            fromJsonTest.testToJsonArray();
            fromJsonTest.testFromJsonHierarchy();
            fromJsonTest.testFromJsonHierarchyDeep();
            fromJsonTest.testTypeInference();

            // todo make this work or remove
            // (new TestFromJson()).testMixedListField();
            fromJsonTest.testMapObjects();
            fromJsonTest.testGenericListPrimitiveFields();
            fromJsonTest.testGenericListObjectFields();
            fromJsonTest.testNotifications();

            DataContractSupportTest dataContractSupportTest = (new DataContractSupportTest());
            dataContractSupportTest.testDataMemberSimple();
            dataContractSupportTest.testOrdering();
            dataContractSupportTest.testMandatory();
            dataContractSupportTest.testDataContractAttribute();
            dataContractSupportTest.testProperties();
            dataContractSupportTest.testNested();
            dataContractSupportTest.testEmitDefaultFalse();
            dataContractSupportTest.testEmitDefaultForUserguide();

            SecurityTest securityTest = new SecurityTest();
            securityTest.testPropertyDenialException();
            securityTest.testPropertyReferenceDenialNoException();
            securityTest.testPropertyLeafDenialNoException();
            securityTest.testFieldReferenceDenialNoException();
            securityTest.testFieldLeafDenialNoException();
            securityTest.testLeafDenialException();
            securityTest.testFieldLeafDenialNoExceptionForUserGuide();
            securityTest.testFieldLeafDenialWithExceptionForUserGuide();

            toJsonTest.Util.writeTestTablePage();
            PauseWithMessage("press return to complete !");
        }

    }
}
