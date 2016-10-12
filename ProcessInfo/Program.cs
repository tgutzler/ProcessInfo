using System;
using ProcessToolsLib;

namespace ProcessInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            var pname = args.Length == 0 ? "explorer" : args[0];

            var proc = new ProcessToMonitor(pname);
            if (proc.IsValid)
            {
                Console.WriteLine(proc.GetInfo().ToJSON());
            }
            else
            {
                Console.WriteLine($"No such process: {pname}");
            }
        }
    }
}
