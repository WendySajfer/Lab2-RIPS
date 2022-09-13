using System;
using System.Threading;
using System.Diagnostics;

namespace Application
{
    class CriticalZone
    {
        public static double Sum = 0;
        public static Mutex Mtx = new Mutex();
    }
    class FuncThread
    {
        double buf_Sum, n_length, m_length, a, c;
        int number_iteration, first_count, n;
        public Thread Thrd;

        public FuncThread(int num_thread_, int first_count_, double n_length_, double m_length_, double a_, int n_, double c_, int number_iteration_)
        {
            Thrd = new Thread(this.Run);
            first_count = first_count_;
            n_length = n_length_;
            m_length = m_length_; 
            a = a_; 
            n = n_;
            c = c_;
            number_iteration = number_iteration_;
            Thrd.Name = "Thread " + num_thread_;
            Thrd.Start();
        }
        private double function(double x, double y) //функция f(x,y)
        {
            return Math.Exp(x) * Math.Sin(y);
        }

        // Точка входа в поток.
        void Run()
        {
            int buf_n = first_count % n, buf_m = (first_count - buf_n) / n;
            double x0 = a + buf_n * n_length, y0 = c + buf_m * m_length;
            double S = m_length * n_length;
            for(int i=0; i < number_iteration; i++)
            {
                buf_n++;
                buf_Sum += function(x0 + n_length/2, y0 + m_length/2) * S;
                if(buf_n < n)
                {
                    x0 += n_length;
                }
                else
                {
                    buf_n = 0;
                    x0 = a;
                    y0 += m_length;
                }
            }
            // Получить мьютекс.
            CriticalZone.Mtx.WaitOne();
            //Console.WriteLine(Thrd.Name + " buf_Sum = " + buf_Sum);
            CriticalZone.Sum += buf_Sum;
            // Освободить мьютекс.
            CriticalZone.Mtx.ReleaseMutex();
        }
    }

    class Program
    {
        public static void Main()
        {
            double a, b, c, d;
            int K, n, m;
            string buf;
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
                Console.WriteLine("Input number n."); // n-частей
                buf = Console.ReadLine();
                if (int.TryParse(buf, out n) && n > 0) break;
                else Console.WriteLine("Invalid number. Try again.");
            }
            while (true)
            {
                Console.WriteLine("Input number m."); // m-частей
                buf = Console.ReadLine();
                if (int.TryParse(buf, out m) && m > 0) break;
                else Console.WriteLine("Invalid number. Try again.");
            }
            while (true)
            {
                Console.WriteLine("Input number a.");
                buf = Console.ReadLine();
                if (double.TryParse(buf, out a)) break;
                else Console.WriteLine("Invalid number. Try again.");
            }
            while (true)
            {
                Console.WriteLine("Input number b.");
                buf = Console.ReadLine();
                if (double.TryParse(buf, out b) && b > a) break;
                else Console.WriteLine("Invalid number. Try again.");
            }
            while (true)
            {
                Console.WriteLine("Input number c.");
                buf = Console.ReadLine();
                if (double.TryParse(buf, out c)) break;
                else Console.WriteLine("Invalid number. Try again.");
            }
            while (true)
            {
                Console.WriteLine("Input number d.");
                buf = Console.ReadLine();
                if (double.TryParse(buf, out d) && d > c) break;
                else Console.WriteLine("Invalid number. Try again.");
            }
            }

            double ab = b - a, cd = d - c, n_length = ab/n,  m_length = cd/m;
            int count_nm = n * m, r_thread = count_nm % K, K_min = (count_nm - r_thread) / K;
            int count_first = 0;
            stopwatch.Start();
            FuncThread[] mt = new FuncThread[K];
            for (int i = 0; i < r_thread; i++)
            {
                mt[i] = new FuncThread(i + 1, count_first, n_length, m_length, a, n, c, K_min + 1);
                count_first += (K_min + 1);
            }
            for (int i = r_thread; i < K; i++)
            {
                mt[i] = new FuncThread(i + 1 , count_first, n_length, m_length, a, n, c, K_min);
                count_first += K_min;
            }

            for(int i = 0; i < K; i++)
            {
                mt[i].Thrd.Join();
            }
            stopwatch.Stop();
            Console.WriteLine("Time:" + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("CriticalZone.Sum = " + CriticalZone.Sum);
        }
    }
}
/*
4
5
4
0
5
0
4

16
1000
1000
0
5
0
5
 */