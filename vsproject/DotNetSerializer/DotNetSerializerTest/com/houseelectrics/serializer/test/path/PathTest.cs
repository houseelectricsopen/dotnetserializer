using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using com.houseelectrics.serializer.path;

namespace com.houseelectrics.serializer.test.path
{
    public class PathTest
    {
        public class PathTestType
        {
            public class Subtype
            {
                public class MoreSub {
                public object TheItem {get; set;}
                }
                public Subtype() { TheSubSubs = new List<MoreSub>(); }
                public List<MoreSub> TheSubSubs { get; set; }
            }
            public Subtype TheSub { get; set; }
            public PathTestType() { TheSub = new Subtype(); }


        }
        [Test]
        public void testPath()
        {
            PathTestType testItem = new PathTestType();
            List<PathTestType.Subtype.MoreSub> subsubs = testItem.TheSub.TheSubSubs;
            PathTestType.Subtype.MoreSub moreSub;
            object object2find = new object();

            moreSub = new PathTestType.Subtype.MoreSub();
            moreSub.TheItem = new object();
            subsubs.Add(moreSub);

            moreSub = new PathTestType.Subtype.MoreSub();
            moreSub.TheItem = object2find;
            subsubs.Add(moreSub);

            moreSub = new PathTestType.Subtype.MoreSub();
            moreSub.TheItem = new object();
            subsubs.Add(moreSub);

            PathUtil util = new PathUtil();

            List<InstancePathElement> path = util.findSingleObjectPath(testItem, object2find);
            //path is TheSub.TheSubSubs.[1]
            string strPath = util.ToString(path);
            System.Console.WriteLine("path= " + strPath);
            Assert.AreEqual(4, path.Count, "expected path");
            string expectedPathString = "TheSub.TheSubSubs.[1].TheItem";
            Assert.AreEqual(expectedPathString, strPath, "expected path string ");

        }
    }
}
