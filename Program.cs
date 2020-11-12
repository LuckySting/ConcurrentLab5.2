using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConcurrentLab5._2
{
    class Program
    {
        static Random randNum = new Random();
        static int[] array;
        static int[][] blocks;
        static Thread[] threads;
        static int h = 1024;
        static int q = 16;
        static int p = q / 2;

        static void fillBlocks()
        {
            blocks = new int[q][];
            for (int i = 0; i < q; i++)
            {
                blocks[i] = new int[h];
                for (int j = 0; j < h; j++)
                {
                    blocks[i][j] = array[i * h + j];
                }
            }
        }

        static void fillArray()
        {
            for (int i = 0; i < q; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    array[i * h + j] = blocks[i][j];
                }
            }
        }

        static void randomArr()
        {
                array = Enumerable
                    .Repeat(0, h*q)
                    .Select(i => randNum.Next(1, 1000))
                    .ToArray();
        }

        static void ascArr()
        {
                array = Enumerable
                    .Range(0, h * q)
                    .Select(i => i)
                    .ToArray();
        }

        static void descArr()
        {
                array = Enumerable
                    .Range(0, h * q)
                    .Select(i => h * q - i)
                    .ToArray();
        }

        static void localSorting(int threads_count)
        {
            threads = new Thread[threads_count];
            for(int i = 0; i < threads_count; i++)
            {
                threads[i] = new Thread((object o) =>
                {
                    int[] payload = (int[])o;
                    Array.Sort(blocks[payload[0]]);
                    Array.Sort(blocks[payload[1]]);
                });
                threads[i].Start((object) new int[]{ i, threads_count + i });
            }
            foreach(var t in threads)
            {
                t.Join();
            }
        }

        static void mergeSplit(object o)
        {
            int[] payload = (int[])o;
            int[] merged = new int[blocks[payload[0]].Length + blocks[payload[1]].Length];
            Array.Copy(blocks[payload[0]], 0, merged, 0, blocks[payload[0]].Length);
            Array.Copy(blocks[payload[1]], 0, merged, blocks[payload[0]].Length, blocks[payload[1]].Length);
            Array.Sort(merged);
            Array.Copy(merged, 0, blocks[payload[0]], 0, blocks[payload[0]].Length);
            Array.Copy(merged, blocks[payload[0]].Length, blocks[payload[1]], 0, blocks[payload[1]].Length);
        }
        static void mergeSplitAll(int blocks_count, int threads_count)
        {
            threads = new Thread[threads_count];
            for (int j = 0; j < 4; j++)
            {
                int step = (int)Math.Pow(2, j);
                List<int> indexes = new List<int>(Enumerable.Range(0, blocks_count).Select(k => k));
                for (int i = 0; i < threads_count; i += 1)
                {
                    int first = indexes[0];
                    int second = 0;
                    foreach(int idx in indexes)
                    {
                        if ((idx ^ first) == step)
                        {
                            second = idx;
                            break;
                        }
                    }
                    indexes.Remove(first);
                    indexes.Remove(second);
                    threads[i] = new Thread(mergeSplit);
                    threads[i].Start((object)new int[] { first, second });
                }
                foreach (var t in threads)
                {
                    t.Join();
                }
            }
        }

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            for (int s = 10; s < 23; s++)
            {
                int len = (int)Math.Pow(2, s);
                p = 8;
                q = 16;
                h = len / q;
                // randomArr();
                // ascArr();
                descArr();
                fillBlocks();
                sw.Restart();
                localSorting(p);
                mergeSplitAll(q, p);
                fillArray();
                Array.Sort(array);
                sw.Stop();
                Console.WriteLine("{1}", len, Math.Round(sw.Elapsed.TotalMilliseconds));
            }
        }
    }
}
