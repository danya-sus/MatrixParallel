using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MatrixParallel
{
    class Program
    {
        public static int Rows = 2000;
        public static int Columns = 2000;

        public class Matrix
        {
            public int[,] matrix;
            public int Rows;
            public int Columns;
            public Matrix(int rows, int colums)
            {
                this.Rows = rows;
                this.Columns = colums;
                matrix = new int[Rows, Columns];
            }
            public void Create()
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        matrix[i, j] = rand.Next(0, 10);
                    }
                }
            }
            public void Show()
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        Console.Write($"{matrix[i, j]} - ");
                    }
                    Console.WriteLine();
                }
            }
            public int GetElement(int i, int j)
            {
                return matrix[i, j];
            }
            public void ChangeElement(int i, int j, int new_element)
            {
                matrix[i, j] = new_element;
            }
            public void Clear()
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        matrix[i, j] = 0;
                    }
                }
            }
        }

        public static Matrix first_matrix = new Matrix(Rows, Columns);
        public static Matrix second_matrix = new Matrix(Columns, Rows);
        public static Matrix result_matrix = new Matrix(first_matrix.Rows, second_matrix.Columns);
        public static Random rand = new Random();

        public class Data_ThreadPool
        {
            public int i;
            public int j;
            public CountdownEvent countdown { get; }
            public Data_ThreadPool(int _i, int _j, CountdownEvent _countdown)
            {
                i = _i;
                j = _j;
                countdown = _countdown;
            }
        }

        public class Data
        {
            public int i;
            public int j;
            public Data(int _i, int _j)
            {
                i = _i;
                j = _j;
            }
        }

        public static void Parallel_EasyWay()
        {
            var countdown = new CountdownEvent(result_matrix.Rows);
            for (int i = 0; i < result_matrix.Rows; i++)
            {
                ThreadPool.QueueUserWorkItem(ParallelColumn_EasyWay, new Data_ThreadPool(i, 0, countdown));
            }

            countdown.Wait();
        }

        public static void ParallelColumn_EasyWay(object _data)
        {
            var data = (Data_ThreadPool)_data;
            for (int j = 0; j < result_matrix.Columns; j++)
            {
                for (int g = 0; g < result_matrix.Rows; g++)
                {
                    result_matrix.matrix[data.i, data.j] += first_matrix.GetElement(data.i, g) * second_matrix.GetElement(g, data.j);
                }
            }
            data.countdown.Signal();
        }

        public static void Parallel_ThreadPool()
        {
            var countdown = new CountdownEvent(result_matrix.Rows);
            for (int i = 0; i < result_matrix.Rows; i++)
            {
                ThreadPool.QueueUserWorkItem(ParallelColumn_ThreadPool, new Data_ThreadPool(i, 0, countdown));
            }

            countdown.Wait();
        }

        public static void ParallelColumn_ThreadPool(object _data)
        {
            var data = (Data_ThreadPool)_data;
            var countdown = new CountdownEvent(result_matrix.Columns);
            for (int j = 0; j < result_matrix.Columns; j++)
            {
                ThreadPool.QueueUserWorkItem(Solving_ThreadPool, new Data_ThreadPool(data.i, j, countdown));
            }

            countdown.Wait();

            data.countdown.Signal();
        }

        
        public static void Parallels()
        {
            Thread[] rows_threads = new Thread[result_matrix.Rows];
            for (int i = 0; i < result_matrix.Rows; i++)
            {
                rows_threads[i] = new Thread(new ParameterizedThreadStart(ParallelsColumns));
            }
            for (int i = 0; i < result_matrix.Rows; i++)
            {
                rows_threads[i].Start(new Data(i, 0));
            }
            foreach (var r in rows_threads)
            {
                r.Join();
            }
        }

        public static void ParallelsColumns(object _data)
        {
            var data = (Data)_data;
            Thread[] columns_thread = new Thread[result_matrix.Columns];
            for (int j = 0; j < result_matrix.Columns; j++)
            {
                columns_thread[j] = new Thread(new ParameterizedThreadStart(Solving));
            }
            for (int j = 0; j < result_matrix.Columns; j++)
            {
                columns_thread[j].Start(new Data(data.i, j));
            }
            foreach (var c in columns_thread)
            {
                c.Join();
            }
        }
        

        public static void Consistens()
        {
            for (int i = 0; i < result_matrix.Rows; i++)
            {
                for (int j = 0; j < result_matrix.Columns; j++)
                {
                    Solving(new Data(i, j));
                }
            }
        }

        public static void Solving(object _data)
        {
            var data = (Data)_data;

            for (int g = 0; g < result_matrix.Rows; g++)
            {
                result_matrix.matrix[data.i, data.j] += first_matrix.GetElement(data.i, g) * second_matrix.GetElement(g, data.j);
            }
        }

        public static void Solving_ThreadPool(object _data)
        {
            var data = (Data_ThreadPool)_data;

            for (int g = 0; g < result_matrix.Rows; g++)
            {
                result_matrix.matrix[data.i, data.j] += first_matrix.GetElement(data.i, g) * second_matrix.GetElement(g, data.j);
            }

            data.countdown.Signal();
        }

        static void Main(string[] args)
        {
            first_matrix.Create();
            second_matrix.Create();
            result_matrix.Clear();

            Stopwatch stopwatch = new Stopwatch();

            Thread.Sleep(5000);

            /*
            //Consistent
            Console.WriteLine($"A consistent solution. ");

            stopwatch.Start();
            Consistens();
            stopwatch.Stop();

            Console.WriteLine($"Time: {(double)stopwatch.ElapsedMilliseconds / 1000} sec.");

            Console.WriteLine("Result matrix:");

            result_matrix.Clear();
            */

            /*
            //Parallel
            Console.WriteLine($"A parallel solution: ");

            stopwatch.Reset();
            stopwatch.Start();

            Parallels();

            stopwatch.Stop();

            Console.WriteLine($"Time: {(double)stopwatch.ElapsedMilliseconds / 1000} sec.");

            result_matrix.Clear();
            */

            /*
            //Parallel (ThreadPool)
            Console.WriteLine($"A parallel solution (ThreadPool): ");

            stopwatch.Reset();
            stopwatch.Start();

            Parallel_ThreadPool();

            stopwatch.Stop();

            Console.WriteLine($"Time: {(double)stopwatch.ElapsedMilliseconds / 1000} sec.");
            */

            /*
            Console.WriteLine($"A parallel solution (EasyWay): ");

            stopwatch.Reset();
            stopwatch.Start();

            Parallel_EasyWay();

            stopwatch.Stop();

            Console.WriteLine($"Time: {(double)stopwatch.ElapsedMilliseconds / 1000} sec.");
            */
        }
    }
}