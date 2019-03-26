using System;
using System.Collections.Generic;
using System.Configuration;

namespace EndOfLineCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Engine e = new Engine(args);
            }
            else
            {
                List<string> testArgs = new List<string>();
                testArgs.Add("del=" + ConfigurationManager.AppSettings["del"]);
                testArgs.Add("exc=" + ConfigurationManager.AppSettings["exc"]);
                testArgs.Add("dir=" + ConfigurationManager.AppSettings["dir"]);
                Engine e = new Engine(testArgs.ToArray());
            }

        }
    }
}
