using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

namespace AStar
{
    public class LinkedListTest
    {
        static int kFillIterations = 10000;
        static int kRandomFillIterations = 10000;

        LinkedList<string> _list = new LinkedList<string>();

        public void test()
        {
            initialFilling();

            //GC.Collect();
            //GC.WaitForPendingFinalizers();

            randomIsertionTest();
            randomIsertionTest();
            randomIsertionTest();
        }

        void randomIsertionTest()
        {
            Stopwatch sw = Stopwatch.StartNew();
            randomFilling();
            sw.Stop();

            DebugUtils.log("Ticks is: " + sw.ElapsedTicks);
        }

        void initialFilling()
        {
            for (int i = 0; i < kFillIterations; i++)
            {
                _list.AddLast("item");
            }
        }

        void randomFilling()
        {
            for (int i = 0; i < kRandomFillIterations; i++)
            {
                int r = Random.Range(0, _list.Count);

                fillAtPos(r);
            }
        }

        void fillAtPos(int pos)
        {
            int counter = 0;

            for (LinkedListNode<string> it = _list.First;
                it != null; it = it.Next)
            {
                // access the item
                string item= it.Value;

                if (counter == pos)
                {
                    // add the item
                    _list.AddBefore(it, "randomItem");

                    break;
                }
            }
            
        }
    }

}
