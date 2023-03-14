using System;
using Yippy;

namespace YippyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("File to compile (Example: test.yp): ");
            string fileName = Console.ReadLine();
            Compiler compiler = new Compiler();
            compiler.Compile(fileName);
            Console.ReadKey();
        }
    }
}
