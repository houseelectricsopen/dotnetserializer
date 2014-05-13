using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer.test
{
    public class AllPrimitiveLeafTypes
    {
        public AllPrimitiveLeafTypes()
        {
            ALong = 123;
            ALongRef = 234;
            AShort = 124;
            ALongRef = 456;
            AChar = 'c';
            AString2 = "hi";
            AUint32 = 3232;
            AUint64 = 6464;
        }

        public long aLong = 123;
        public short aShort = 124;
        public long? aLongRef = 456;
        public string aString, aString2 = "hi";
        public char aChar = 'c';

        public UInt32 aUint32 = 3232;
        public UInt64 aUint64 = 6464;


        public long ALong { get; set; }
        public short AShort { get; set; }
        public long ALongRef { get; set; }
        public long? ALongRefNullable { get; set; }
        public string AString { get; set; }
        public string AString2 { get; set; }
        public char AChar { get; set; }

        public UInt32 AUint32 { get; set; }
        public UInt64 AUint64 { get; set; }

        public static String[] testFieldExpressions = { "aLong", "aShort", "aLongRef", "aString", "aChar", "aString2", "aUint32", "aUint64" };
        public static Object[] testExpectedFieldValues(AllPrimitiveLeafTypes template)
        {
            return new Object[] { template.aLong, template.aShort, template.aLongRef, template.aString, template.aChar.ToString(), template.aString2, template.aUint32, template.aUint64 };
        }
        public static String[] testPropertyExpressions = { "ALong", "AShort", "ALongRef", "ALongRefNullable", "AString", "AChar", "AString2", "AUint32", "AUint64" };
        public static Object[] testExpectedPropertyValues(AllPrimitiveLeafTypes template)
        {
            return new Object[] { template.ALong, template.AShort, template.ALongRef, template.ALongRefNullable, template.AString, template.AChar/*.ToString()*/, template.AString2, template.AUint32, template.AUint64 };
        }


    }
}
