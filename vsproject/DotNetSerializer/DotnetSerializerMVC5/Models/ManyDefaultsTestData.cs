using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotnetSerializerMVC5.Models
{
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
}