using System;
using System.Threading;
using QSharp.Scheme.Threading;

namespace QSharpTestDNC
{
    class Program
    {
        delegate void ThreadFunc();

        const int MaxThreads = 10;
        const int ShareRate = 3;
        const string Dummy = "";

        static void ActionMutex(Action action)
        {
            lock(Dummy)
            {
                action();
            }   
        }

        static void TestCondition2()
        {
            int inc = 0;
            int curr = 0;
            var cond = new Condition2();
            ThreadStart producerFunc = ()=>
            {
                var required = MaxThreads / ShareRate + 1;
                for (var sig = 1; sig <= required; sig++)
                {
                    Thread.Sleep(1000);
                    cond.ChangeAndNotify(()=>
                    {
                        ActionMutex(()=>Console.WriteLine($"Signalling {sig}"));
                        curr = sig;
                        Monitor.PulseAll(cond);
                    });
                }
            };
            ThreadStart consumerFun = ()=>
            {
                var myId = Interlocked.Increment(ref inc);
                int waitId = (myId-1)/ShareRate+1;
                ActionMutex(()=>Console.WriteLine($"Thread {myId} waiting on {waitId}..."));
                cond.WaitUntil(()=>curr == waitId);
                ActionMutex(()=>Console.WriteLine($"Thread {myId} obtained {waitId}."));
            };
            var consumers = new Thread[MaxThreads];
            for (var i = 0; i < consumers.Length; i++)
            {
                consumers[i] = new Thread(consumerFun);
                consumers[i].Start();
            }     
            var producer = new Thread(producerFunc);
            producer.Start();

            producer.Join();
            foreach (var c in consumers)
            {
                c.Join();
            }
            Console.WriteLine("Test completed");
        }
        
        static void Main()
        {
            Console.WriteLine("Hello, we are now testing Condition2");
            TestCondition2();
        }
    }
}