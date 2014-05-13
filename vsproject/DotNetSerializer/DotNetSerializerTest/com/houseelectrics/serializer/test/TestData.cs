using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.houseelectrics.serializer.test
{
    class TestData
    {
        Sub sub = new Sub();
        public Sub TheSub { get { return sub; } }
        public class Sub
        {
            public SubSub subSub = new SubSub();
            public SubSub TheSubSub { get { return subSub; } }
            public class SubSub
            {
                string greeting = "hi";
                public String Greeting { get { return greeting; } }
                private int ageYears = 32;
                public int AgeYears { get { return ageYears; } }
                public double heightMetres = 1.23;
                public double HeightMetres { get { return heightMetres; } }
                string[] seasons = { "Spring", "Summer", "Autumn", "Winter" };
                public String[] Seasons { get { return seasons; } }
                private List<int> goodYears = new List<int> ();
                public List<int> GoodYears {get {return goodYears;}}
                public SubSub()
                {
                    goodYears.Add(1966);
                    goodYears.Add(1973);
                }
            }
        }
    }
}
