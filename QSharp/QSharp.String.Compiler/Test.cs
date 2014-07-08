/**
 * <vendor>
 *  Copyright 2009 Quanben Tech.
 * </vendor>
 */

#define SWITCH_SyntaxParser_Backtracking
#define SWITCH_SyntaxParser_RecursiveDescent
#define SWITCH_SyntaxParser_LL1_Test
#define SWITCH_SyntaxParser_LRTable_Test
#define SWITCH_SyntaxParser_LR0_Test

#if TEST_String_Compiler

#define ENABLED


using System;


namespace QSharp.String.Compiler
{
    public class TextualTestcase
    {
        public delegate bool TestMethod(string[] bnfText, string sInput);

        public string Name = "";
        public string[] BnfText = null;
        public string[] SamplesPassed = null;
        public string[] SamplesFailed = null;

        public static int FailedTotal;
        public static int ExTotal;
        public static int TestTotal;

        public static TextualTestcase Jchzh062 = new TextualTestcase(
            "Jchzh062",
            new[]
            {
                "E -> T Ep",
                "Ep -> '+' T Ep | ",
                "T -> F Tp",
                "Tp -> '*' F Tp | ",
                "F -> 'i' | '(' E ')' "
            },
            new[]
            {
                "i+i*i",
                "i +"
            },
            new[]
            {
                "iiiii"
            }
            );

        public static TextualTestcase Jchzh071 = new TextualTestcase(
            "Jchzh071",
            new[]
            {
                "Z-> 'b' M 'b'",
                "M-> 'a' | '(' L",
                "L-> M 'a'')'"
            },
            new[]
            {
                "b(aa)b"
            },
            null
            );

        public static TextualTestcase Jchzh078 = new TextualTestcase(
            "Jchzh078",
            new[]
            {
                "Z -> 'a' A 'c' ",
                "A -> 'b' B | 'b' 'a' ",
                "B -> 'd' B | 'e'"
            },
            null,
            null
            );

        public static TextualTestcase Jchzh084 = new TextualTestcase(
            "Jchzh084",
            new[]
            {
                "W -> B B ",
                "B -> 'a' B",
                "B -> 'b'"
            },
            null,
            new[]
            {
                "ababbbbb",
            }
            );

        public static TextualTestcase Jchzh086 = new TextualTestcase(
            "Jchzh086",
            new[]
            {
                "M -> T",
                "T -> F '*' T",
                "T -> F",
                "F -> 'a'",
                "F -> 'b'",
            },
            null,
            null
            );

        // NOTE this may not be an LL1
        public static TextualTestcase Quanben001 = new TextualTestcase(
            "Quanben001",
            new[]
            {
                "Z -> 'b' A B",
                "A -> A B | 'a' | 'a' A",
                "B -> 'b' B | 'b'"
            },
            new[]
            {
                "baabbbbb"
            },
            null
            );

        public static TextualTestcase Quanben002 = new TextualTestcase(
            "Quanben002",
            new[]
            {
                " P -> 'bP'| A ",
                " A -> 'd' A"
            },
            null,
            null
            );

        public static TextualTestcase Quanben003 = new TextualTestcase(
            "Quanben003",
            new[]
            {
                "N -> D | D N", /* N->D|N D, left recursive, not parsable by LR  */
                "D -> '0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9'"
            },
            new[]
            {
                "43210123",
            },
            new[]
            {
                "",
            }
            );

        public static TextualTestcase Quanben003_bad01 = new TextualTestcase(
            "Quanben003_bad01",
            new[]
            {
                "N->D|ND",
                "D->'0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9'"
            },
            null,
            null
            );

        public static TextualTestcase Quanben004 = new TextualTestcase(
            "Quanben004",
            new[]
            {
                " P ->'a'| A ",
                " A->'d' A | 'b' "
            },
            new[]
            {
                "b"
            },
            null
            );

        public static TextualTestcase[] All =
        {
            Jchzh062,
            Jchzh071,
            Jchzh078,
            Jchzh084,
            Jchzh086,
            Quanben001,
            Quanben002,
            Quanben003,
            Quanben003_bad01,
            Quanben004
        };

        public TextualTestcase(string name, string[] bnfText, string[] samplesPassed, string[] samplesFailed)
        {
            Name = name;
            BnfText = bnfText;
            SamplesPassed = samplesPassed;
            SamplesFailed = samplesFailed;
        }

        public string GetSampleAtRandom()
        {
            var nPassed = (SamplesPassed != null) ? SamplesPassed.Length : 0;
            var nFailed = (SamplesFailed != null) ? SamplesFailed.Length : 0;
            var nTotal = nPassed + nFailed;
            if (nTotal == 0)
            {
                return null;
            }
            var rand = new Random(DateTime.Now.Millisecond);
            var index = rand.Next(nTotal);
            if (index >= nPassed)
            {
                index -= nPassed;
                return SamplesFailed[index];
            }
            return SamplesPassed[index];
        }

        public string GetSamplePassedAtRandom()
        {
            if (SamplesPassed == null)
            {
                return null;
            }
            var nSamplePassed = SamplesPassed.Length;
            var rand = new Random(DateTime.Now.Millisecond);
            return SamplesPassed[rand.Next(nSamplePassed)];
        }

        public string GetSampleFailedAtRandom()
        {
            if (SamplesFailed == null)
            {
                return null;
            }
            var nSamplesFailed = SamplesFailed.Length;
            var rand = new Random(DateTime.Now.Millisecond);
            return SamplesFailed[rand.Next(nSamplesFailed)];
        }

        public static implicit operator string[](TextualTestcase bts)
        {
            return bts.BnfText;
        }

        public static void TestAll(TestMethod testMethod)
        {
            FailedTotal = 0;
            ExTotal = 0;
            TestTotal = 0;
            foreach (TextualTestcase testcase in All)
            {
                var bnfText = testcase.BnfText;
                var nFailedTestcases = 0;
                var nExTestcases = 0;
                var nTestcases = 0;
                int i;
                if (testcase.SamplesPassed != null)
                {
                    i = 0;
                    foreach (string sPassed in testcase.SamplesPassed)
                    {
                        try
                        {
                            bool bRes = testMethod(bnfText, sPassed);
                            if (bRes == false)
                            {
                                Console.WriteLine("! Failed at '{0}' passed sample {1}", testcase.Name, i);
                                nFailedTestcases++;
                                FailedTotal++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("! Test {0} passed sample{1} failed with exception '{2}'",
                                testcase.Name, i, e.Message);
                            nExTestcases++;
                            ExTotal++;
                        }
                        nTestcases++;
                        TestTotal++;
                        i++;
                    }
                }
                if (testcase.SamplesFailed != null)
                {
                    i = 0;
                    foreach (var sFailed in testcase.SamplesFailed)
                    {
                        try
                        {
                            var bRes = testMethod(bnfText, sFailed);
                            if (bRes)
                            {
                                Console.WriteLine("! Failed at '{0}' failed sample {1}", testcase.Name, i);
                                nFailedTestcases++;
                                FailedTotal++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("! Test {0} failed sample{1} failed with exception '{2}'",
                                testcase.Name, i, e.Message);
                            nExTestcases++;
                            ExTotal++;
                        }
                        nTestcases++;
                        TestTotal++;
                        i++;
                    }
                }

                Console.WriteLine(
                    ": Testcase '{0}' finished with {1} failed case(s), {2} exceptional case(s) out of totally {3} test(s)",
                    testcase.Name, nFailedTestcases, nExTestcases, nTestcases);
            }
            Console.WriteLine(
                ": Test completed with {0} failed case(s), {1} exceptional case(s) out of totally {2} test(s)",
                FailedTotal, ExTotal, TestTotal);
        }
    } /* class TesxtualTestcase */

    public static class IntegratedTest
    {
        public static void TestEntry()
        {
            int nFailed = 0;
            int nEx = 0;
            int nTests = 0;

#if SWITCH_SyntaxParser_Backtracking
            SyntaxParser_Backtracking_Test.MainTest();
            nFailed += TextualTestcase.FailedTotal;
            nEx += TextualTestcase.ExTotal;
            nTests += TextualTestcase.TestTotal;
#endif

#if SWITCH_SyntaxParser_RecursiveDescent
            SyntaxParser_RecursiveDescent_Test.MainTest();
            nFailed += TextualTestcase.FailedTotal;
            nEx += TextualTestcase.ExTotal;
            nTests += TextualTestcase.TestTotal;
#endif

#if SWITCH_SyntaxParser_LL1_Test
            SyntaxParser_LL1_Test.MainTest();
            nFailed += TextualTestcase.FailedTotal;
            nEx += TextualTestcase.ExTotal;
            nTests += TextualTestcase.TestTotal;
#endif

#if SWITCH_SyntaxParser_LRTable_Test
            SyntaxParser_LRTable_Test.MainTest();
            nFailed += TextualTestcase.FailedTotal;
            nEx += TextualTestcase.ExTotal;
            nTests += TextualTestcase.TestTotal;
#endif

#if SWITCH_SyntaxParser_LR0_Test
            SyntaxParser_LR0_Test.MainTest();
            nFailed += TextualTestcase.FailedTotal;
            nEx += TextualTestcase.ExTotal;
            nTests += TextualTestcase.TestTotal;
#endif

            Console.WriteLine("=== Final Report ===");
            Console.WriteLine(" {0} tests carried out", nTests);
            Console.WriteLine(" {0} exceptional cases", nEx);
            Console.WriteLine(" {0} failed cases", nFailed);
        }
    } /* class IntegratedTest */

} /* namespace QSharp.String.Compiler */

#endif
