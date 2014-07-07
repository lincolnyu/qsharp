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

        public static int gFailedTotal;
        public static int gExTotal;
        public static int gTestTotal;

        public static TextualTestcase gJchzh062 = new TextualTestcase(
                    "gJchzh062",
                    new string[]
                {
                    "E -> T Ep",
                    "Ep -> '+' T Ep | ",
                    "T -> F Tp",
                    "Tp -> '*' F Tp | ",
                    "F -> 'i' | '(' E ')' "
                },
                    new string[]
                {
                    "i+i*i",
                    "i +"
                },
                    new string[]
                {
                    "iiiii"
                }
                    );
        public static TextualTestcase gJchzh071 = new TextualTestcase(
             "gJchzh071",
            new string[]
                {
                    "Z-> 'b' M 'b'",
                    "M-> 'a' | '(' L",
                    "L-> M 'a'')'"
                },
            new string[]
                {
                    "b(aa)b"
                },
                null
            );
        public static TextualTestcase gJchzh078 = new TextualTestcase(
            "gJchzh078",
            new string[]
                {
                    "Z -> 'a' A 'c' ",
                    "A -> 'b' B | 'b' 'a' ",
                    "B -> 'd' B | 'e'"
                },
            null,
            null
            );

        public static TextualTestcase gJchzh084 = new TextualTestcase(
            "gJchzh084",
            new string[]
                {
                    "W -> B B ",
                    "B -> 'a' B",
                    "B -> 'b'"
                },
            null,
            new string[]
                {
                    "ababbbbb",
                }
            );
        public static TextualTestcase gJchzh086 = new TextualTestcase(
            "gJchzh086",
            new string[]
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
        public static TextualTestcase gQuanben001 = new TextualTestcase(
            "gQuanben001",
            new string[] 
                { 
                    "Z -> 'b' A B", 
                    "A -> A B | 'a' | 'a' A", 
                    "B -> 'b' B | 'b'" 
                },
            new string[]
                {
                    "baabbbbb"
                },
            null
            );
        public static TextualTestcase gQuanben002 = new TextualTestcase(
            "gQuanben002",
            new string[] 
                { 
                    " P -> 'bP'| A ", 
                    " A -> 'd' A"
                },
            null,
            null
            );
        public static TextualTestcase gQuanben003 = new TextualTestcase(
            "gQuanben003",
            new string[] 
                { 
                    "N -> D | D N", /* N->D|N D, left recursive, not parsable by LR  */
                    "D -> '0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9'"
                },
            new string[]
                {
                    "43210123",
                },
            new string[]
                {
                    "",
                }
            );
        public static TextualTestcase gQuanben003_bad01 = new TextualTestcase(
            "gQuanben003_bad01",
            new string[] 
                { 
                    "N->D|ND", 
                    "D->'0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9'"
                },
            null,
            null
            );
        public static TextualTestcase gQuanben004 = new TextualTestcase(
            "gQuanben004",
            new string[] 
                { 
                    " P ->'a'| A ", 
                    " A->'d' A | 'b' "
                },
            new string[]
                {
                    "b"
                },
            null
            );

        public static TextualTestcase[] gAll = new TextualTestcase[]
            {
                gJchzh062,
                gJchzh071,
                gJchzh078,
                gJchzh084,
                gJchzh086,
                gQuanben001,
                gQuanben002,
                gQuanben003,
                gQuanben003_bad01,
                gQuanben004,
            };

        public TextualTestcase(string name, string[] bnfText, string[] samplesPassed, string[] samplesFailed)
        {
            Name = name; BnfText = bnfText;
            SamplesPassed = samplesPassed;
            SamplesFailed = samplesFailed;
        }

        public string GetSampleAtRandom()
        {
            int nPassed = (SamplesPassed != null) ? SamplesPassed.Length : 0;
            int nFailed = (SamplesFailed != null) ? SamplesFailed.Length : 0;
            int nTotal = nPassed + nFailed;
            if (nTotal == 0)
            {
                return null;
            }
            Random rand = new Random(DateTime.Now.Millisecond);
            int index = rand.Next(nTotal);
            if (index >= nPassed)
            {
                index -= nPassed;
                return SamplesFailed[index];
            }
            else
            {
                return SamplesPassed[index];
            }
        }

        public string GetSamplePassedAtRandom()
        {
            if (SamplesPassed == null)
            {
                return null;
            }
            int nSamplePassed = SamplesPassed.Length;
            Random rand = new Random(DateTime.Now.Millisecond);
            return SamplesPassed[rand.Next(nSamplePassed)];
        }

        public string GetSampleFailedAtRandom()
        {
            if (SamplesFailed == null)
            {
                return null;
            }
            int nSamplesFailed = SamplesFailed.Length;
            Random rand = new Random(DateTime.Now.Millisecond);
            return SamplesFailed[rand.Next(nSamplesFailed)];
        }

        public static implicit operator string[](TextualTestcase bts)
        {
            return bts.BnfText;
        }

        public static void TestAll(TestMethod testMethod)
        {
            gFailedTotal = 0;
            gExTotal = 0;
            gTestTotal = 0;
            foreach (TextualTestcase testcase in TextualTestcase.gAll)
            {
                string[] bnfText = testcase.BnfText;
                int nFailedTestcases = 0;
                int nExTestcases = 0;
                int nTestcases = 0;
                int i = 0;
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
                                gFailedTotal++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("! Test {0} passed sample{1} failed with exception '{2}'", 
                                testcase.Name, i, e.Message);
                            nExTestcases++;
                            gExTotal++;
                        }
                        nTestcases++;
                        gTestTotal++;
                        i++;
                    }
                }
                if (testcase.SamplesFailed != null)
                {
                    i = 0;
                    foreach (string sFailed in testcase.SamplesFailed)
                    {
                        try
                        {
                            bool bRes = testMethod(bnfText, sFailed);
                            if (bRes == true)
                            {
                                Console.WriteLine("! Failed at '{0}' failed sample {1}", testcase.Name, i);
                                nFailedTestcases++;
                                gFailedTotal++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("! Test {0} failed sample{1} failed with exception '{2}'", 
                                testcase.Name, i, e.Message);
                            nExTestcases++;
                            gExTotal++;
                        }
                        nTestcases++;
                        gTestTotal++;
                        i++;
                    }
                }

                Console.WriteLine(": Testcase '{0}' finished with {1} failed case(s), {2} exceptional case(s) out of totally {3} test(s)", 
                    testcase.Name, nFailedTestcases, nExTestcases, nTestcases);
            }
            Console.WriteLine(": Test completed with {0} failed case(s), {1} exceptional case(s) out of totally {2} test(s)", gFailedTotal, gExTotal, gTestTotal);
        }
    }   /* class TesxtualTestcase */

    public static class IntegratedTest
    {
        public static void TestEntry()
        {
            int nFailed = 0;
            int nEx = 0;
            int nTests = 0;

#if SWITCH_SyntaxParser_Backtracking
            SyntaxParser_Backtracking_Test.MainTest();
            nFailed += TextualTestcase.gFailedTotal;
            nEx += TextualTestcase.gExTotal;
            nTests += TextualTestcase.gTestTotal;
#endif

#if SWITCH_SyntaxParser_RecursiveDescent
            SyntaxParser_RecursiveDescent_Test.MainTest();
            nFailed += TextualTestcase.gFailedTotal;
            nEx += TextualTestcase.gExTotal;
            nTests += TextualTestcase.gTestTotal;
#endif

#if SWITCH_SyntaxParser_LL1_Test
            SyntaxParser_LL1_Test.MainTest();
            nFailed += TextualTestcase.gFailedTotal;
            nEx += TextualTestcase.gExTotal;
            nTests += TextualTestcase.gTestTotal;
#endif

#if SWITCH_SyntaxParser_LRTable_Test
            SyntaxParser_LRTable_Test.MainTest();
            nFailed += TextualTestcase.gFailedTotal;
            nEx += TextualTestcase.gExTotal;
            nTests += TextualTestcase.gTestTotal;
#endif

#if SWITCH_SyntaxParser_LR0_Test
            SyntaxParser_LR0_Test.MainTest();
            nFailed += TextualTestcase.gFailedTotal;
            nEx += TextualTestcase.gExTotal;
            nTests += TextualTestcase.gTestTotal;
#endif

            Console.WriteLine("=== Final Report ===");
            Console.WriteLine(" {0} tests carried out", nTests);
            Console.WriteLine(" {0} exceptional cases", nEx);
            Console.WriteLine(" {0} failed cases", nFailed);
        }
    }   /* class IntegratedTest */

}   /* namespace QSharp.String.Compiler */

#endif
