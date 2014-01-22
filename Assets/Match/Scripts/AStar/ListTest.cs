using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

namespace AStar
{
    public class ListTest
    {
        static int kFillIterations = 10000;
        static int kRandomFillIterations = 10000;

        List<string> _list = new List<string>();

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

            DebugUtils.log("List Ticks is: " + sw.ElapsedTicks);
        }

        void initialFilling()
        {
            for (int i = 0; i < kRandomFillIterations; i++)
            {
                _list.Add("item");
            }
        }

        void randomFilling()
        {
            for (int i = 0; i < kFillIterations; i++)
            {
                int r = Random.Range(0, _list.Count);

                fillAtPos(r);
            }
        }

        void fillAtPos(int pos)
        {
            int counter = 0;

            for (int i = 0; i < _list.Count; i++)
            {
                // access the item
                string item = _list[i];

                if (counter == pos)
                {
                    // add the item
                    _list.Insert(i, "randomItem");

                    break;
                }
            }

        }
    }

}
