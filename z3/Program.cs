using System;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Application
{
    class GeneralZone
    {
        public static double[,] A_matrix;
        public static double[,] B_matrix;
        public static double[,] C_matrix;
        public static Mutex Mtx = new Mutex();
    }
    class FuncThread
    {
        int number_iteration, first_count, n;
        public Thread Thrd;

        public FuncThread(int num_thread_, int first_count_, int n_, int number_iteration_)
        {
            Thrd = new Thread(this.Run);
            first_count = first_count_;
            n = n_;
            number_iteration = number_iteration_;
            Thrd.Name = "Thread " + num_thread_;
            Thrd.Start();
        }
        private double function(int i_index, int j_index) //произедение i строки и j столбца
        {
            double sum = 0;
            for(int i = 0; i < n; i++)
            {
                sum += GeneralZone.A_matrix[i_index, i] * GeneralZone.B_matrix[i, j_index];
            }
            return sum;
        }

        // Точка входа в поток.
        void Run()
        {
            int buf_n = first_count % n, buf_m = (first_count - buf_n) / n;
            for (int i = 0; i < number_iteration; i++)
            {
                GeneralZone.C_matrix[buf_n, buf_m] = function(buf_n, buf_m);
                /*{ 
                GeneralZone.Mtx.WaitOne();
                Console.WriteLine(Thrd.Name + " function" + buf_n + "" + buf_m + " = " + GeneralZone.C_matrix[buf_n, buf_m]);
                // Освободить мьютекс.
                GeneralZone.Mtx.ReleaseMutex();
                }*/
                buf_n++;
                if (buf_n == n)
                {
                    buf_n = 0;
                    buf_m++;
                }
            }
        }
    }

    class Program
    {
        public static void Main()
        {
            int K, n;
            string buf;
            int count_first = 0;
            StreamWriter file, file1;
            try
            {
                file = new StreamWriter("K_Threads.txt");
            }
            catch (IOException exc)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(exc.ToString());
                return;
            }
            try
            {
                file1 = new StreamWriter("1_Thread.txt");
            }
            catch (IOException exc)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(exc.ToString());
                return;
            }
            Stopwatch stopwatch = new Stopwatch();
            {
                while (true)
                {
                    Console.WriteLine("Input number K."); // число потоков
                    buf = Console.ReadLine();
                    if (int.TryParse(buf, out K) && K > 0 && K <= 40) break;
                    else Console.WriteLine("Invalid number. Try again.");
                }
                while (true)
                {
                    Console.WriteLine("Input number n."); // n-размерность
                    buf = Console.ReadLine();
                    if (int.TryParse(buf, out n) && n > 1) break;
                    else Console.WriteLine("Invalid number. Try again.");
                }
            }
            { 
            GeneralZone.A_matrix = new double[n, n];
            GeneralZone.B_matrix = new double[n, n];
            GeneralZone.C_matrix = new double[n, n];
            Random random = new Random();
            double rand;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    rand = random.Next(0, 100);
                    GeneralZone.A_matrix[i, j] = rand;
                    rand = random.Next(0, 100);
                    GeneralZone.B_matrix[i, j] = rand;
                }
            }
            /*Console.WriteLine("Matrix A:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(GeneralZone.A_matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Matrix B:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(GeneralZone.B_matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }*/
            }
            int count_n = n * n, r_thread = count_n % K, K_min = (count_n - r_thread) / K;
            stopwatch.Start();
            FuncThread[] mt = new FuncThread[K];
            for (int i = 0; i < r_thread; i++)
            {
                mt[i] = new FuncThread(i + 1, count_first, n, K_min + 1);
                count_first += (K_min + 1);
            }
            for (int i = r_thread; i < K; i++)
            {
                mt[i] = new FuncThread(i + 1, count_first, n, K_min);
                count_first += K_min;
            }

            for (int i = 0; i < K; i++)
            {
                mt[i].Thrd.Join();
            }
            stopwatch.Stop();
            Console.WriteLine("Time:" + stopwatch.ElapsedMilliseconds);
            file.WriteLine("Matrix C:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    file.Write(GeneralZone.C_matrix[i, j] + "\t");
                }
                file.WriteLine();
            }
            file.Close();

            stopwatch = new Stopwatch();
            stopwatch.Start();
            mt[0] = new FuncThread(1, 0, n, n*n);
            mt[0].Thrd.Join();
            stopwatch.Stop();
            Console.WriteLine("Time-1:" + stopwatch.ElapsedMilliseconds);
            file1.WriteLine("Matrix C:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    file1.Write(GeneralZone.C_matrix[i, j] + "\t");
                }
                file1.WriteLine();
            }
            file1.Close();
        }
    }
}
/*
16
500
 */