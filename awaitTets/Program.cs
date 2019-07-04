using System;
using System.Threading;
using System.Threading.Tasks;

namespace awaitTets
{
    class Program
    {
        static void Main(string[] args)
        {
            write(1);
            write(2);
            write(3);
            Console.WriteLine("fine del programma");
            Console.ReadLine();
        }

        public static async void write(int task)
        {
            await Task.Delay(2000);
            Console.WriteLine(task);
        }
    }
}
