//#define teston
//#define rescue

//using System.Collections.Generic;
//using QSharp.Scheme.Classical.Trees;
using QSharpTest.Scheme.Classical.Hash;
//using QSharpTest.Scheme.Classical.Trees;
using QSharpTest.String.Compiler;

namespace QSharpTest
{
    class Program
    {
        static void Main()
        {
#if rescue
            // 88,71,78,69,3,0,14,19,60,89,47,18,44,7,82,54,26,0,
            /*
            BTree<int> btree = new BTree<int>(5);
            BTree_Test.BTreeWrappedAsBinaryTree<int> btreeWrapper
                = new BTree_Test.BTreeWrappedAsBinaryTree<int>(btree);*/

            var tree = new AvlTree<int>();
            var test = new SearchTreeTest(tree);

            var track = new List<int> {  
                88,71,78,69,3,0,14,19,60,89,47,18,44,7,82,54,26,0,
            };
            test.TestEntry_Rescue(30, track);
            return;
#endif


            var met = new MathExpressionTest();
            met.Test();

#if false
    //QSharp.String.Compiler.IntegratedTest.TestEntry();
            
            var t = new SplitOrderedHashConcurrentTest();
            //t.SoHashLinearAddItemPressureTest();
            //t.SoHashDynamicAddItemPressureTest();
            t.SoHashLinearClearPressureTest();
#endif
        }
    }
}


// test compiler

//QSharp.String.Compiler.IntegratedTest.TestEntry();
//QSharp.String.Rex.RexCreator_Test.TestEntry();

//QSharp.String.ZMatch_Test.SampleMain(new String[]{ "abcabddt", "1" });
// adafsaaabcabcabddtadf
