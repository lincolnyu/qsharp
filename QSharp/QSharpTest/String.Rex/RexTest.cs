using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.String.Stream;
using QSharp.String.Rex;

namespace QSharpTest.String.Rex
{
    [TestClass]
    public class RexTest
    {
        #region Nested types

        public class RexChecker
        {
            public string Rex;
            public string Target;
            public int MatchEnd;

            public RexChecker(string rex, string target, int matchEnd)
            {
                Rex = rex;
                Target = target;
                MatchEnd = matchEnd;
            }

            public bool Check()
            {
                var creator = new Creator<StringStream>(true);
                Machine<StringStream> machine;
                try
                {
                    machine = creator.Create(Rex);
                }
                catch (Creator<StringStream>.Exception e)
                {
                    var pos = e.Pos;
                    return (pos + MatchEnd + 1 == 0);
                }

                var ssTarget = new StringStream(Target);
                var b = machine.Verify(ssTarget);
                if (MatchEnd == 0)
                {
                    return !b;
                }
                if (!b) return false;
                var pos2 = ((StringStream.Position)ssTarget.Pos).ToInt();
                return pos2 == MatchEnd;
            }
        }

        #endregion

        #region Fields

        static readonly RexChecker[] Checkers = new[] {
            new RexChecker(@"(c*)*d", "d", 1),
            new RexChecker(@"(c*)*c", "ccccc", 5),
            new RexChecker(@"a(b|d|c)*cd", "abbdcbddccdc", 11),
            new RexChecker(@"a(b*|d*|c*)*c", "abbdcbddccdc", 12),
            new RexChecker(@"a(?<a>b(?<a>k)*b\k<a>)*c\k<a>d", "abkbkbkkbkcbkkbkdt", 17),
            new RexChecker(@"a(.*)\k<1>", "a1231234", 7),
            new RexChecker(@"a(.*)\k<1>", "a12351234", 1),
            new RexChecker(@"a(.*)\k<1>", "a1514", 1),
            new RexChecker(@"a\k<1>(.*)", "a1514", -1-1),
            new RexChecker(@"a(.*)", "a1514", 5),
            new RexChecker(@"a{2-3,7-9}b{1-3}", "aaaabbb", 0),
            new RexChecker(@"a{2-3,7-9}b{1-3}", "aaaaaaabbb", 10),
            new RexChecker(@"(a{2-3,7-9}b{1-3}){1-2}", "aaabbbaabb", 10),
            new RexChecker(@"a{2-3,7-9}b{1-3}", "aaaaaaaaabbb", 12),
            new RexChecker(@"([^a]|a[^b]|ab[^c])*abc", "aa abaa  abc abc abc", 12),
            new RexChecker(@"([^b])(\k<1>)* abc", "aaa abcd", 7),
            new RexChecker(@"([^b])(\k<2>)* abc", "aaa abcd", -1-7),
            new RexChecker(@"([^-]*)-([^-]*)(\.[^.]*|)", "Node34-Node46", 13), 
            new RexChecker(@"([^-]*)-([^-]*)(\.[^.]*|)", "Node34-Node46.1.12", 18)
        };

        #endregion

        #region Methods

        [TestMethod]
        public void TestCase001()
        {
            for (var iTest = 0; iTest < Checkers.Length; iTest++)
            {
                var checker = Checkers[iTest];
                var b = checker.Check();
                Assert.IsTrue(b, string.Format("Test {0} failed", iTest));
            }
        }

        [TestMethod]
        public void TestCase002()
        {
            var creator = new Creator<StringStream>(true);
            const string rex = @"([^-]*)-([^-]*)(\.[^.]*|)";
            var machine = creator.Create(rex);
            var ssTarget = new StringStream("Node34-Node46.1");
            machine.Verify(ssTarget);
            var match1 = creator.GetMatch("1");
            var match2 = creator.GetMatch("2");
            var match3 = creator.GetMatch("3");
            var pos1S = (StringStream.Position)match1.Start;
            var pos1E = (StringStream.Position)match1.End;
            var pos2S = (StringStream.Position)match2.Start;
            var pos2E = (StringStream.Position)match2.End;
            var pos3S = (StringStream.Position)match3.Start;
            var pos3E = (StringStream.Position)match3.End;
            Assert.IsTrue(pos1S.ToInt() == 0);
            Assert.IsTrue(pos1E.ToInt() == 6);
            Assert.IsTrue(pos2S.ToInt() == 7);
            Assert.IsTrue(pos2E.ToInt() == 15);
            Assert.IsTrue(pos3S.ToInt() == 15);
            Assert.IsTrue(pos3E.ToInt() == 15);
        }

        [TestMethod]
        public void TestCase003()
        {
            var creator = new Creator<StringStream>(true);
            const string rex = @"([^-]*)-(([^-]*)\.[^.]*|([^-.]*))";
            var machine = creator.Create(rex);
            var ssTarget = new StringStream("Node34-Node46.1.1");
            machine.Verify(ssTarget);
            var match0 = creator.GetMatch("0");
            Assert.IsTrue(match0 == null);
            var match1 = creator.GetMatch("1");
            var match2 = creator.GetMatch("2");
            var match3 = creator.GetMatch("3");
            var match4 = creator.GetMatch("4");
            var pos1S = (StringStream.Position)match1.Start;
            var pos1E = (StringStream.Position)match1.End;
            var pos2S = (StringStream.Position)match2.Start;
            var pos2E = (StringStream.Position)match2.End;
            var pos3S = (StringStream.Position)match3.Start;
            var pos3E = (StringStream.Position)match3.End;
            Assert.IsTrue(pos1S.ToInt() == 0);
            Assert.IsTrue(pos1E.ToInt() == 6);
            Assert.IsTrue(pos2S.ToInt() == 7);
            Assert.IsTrue(pos2E.ToInt() == 17);
            Assert.IsTrue(pos3S.ToInt() == 7);
            Assert.IsTrue(pos3E.ToInt() == 15);
            Assert.IsTrue(match1.Matched);
            Assert.IsTrue(match2.Matched);
            Assert.IsTrue(match3.Matched);
            Assert.IsTrue(!match4.Matched);
        }

        [TestMethod]
        public void TestCase004()
        {
            var creator = new Creator<StringStream>(true);
            const string rex = @"abc";
            var machine = creator.Create(rex);
            var ssTarget = new StringStream("abcde");
            var res = machine.Verify(ssTarget);
            Assert.IsTrue(res);
            Assert.IsTrue(((StringStream.Position)ssTarget.Pos).ToInt()==3);
        }

        #endregion
    }
}
