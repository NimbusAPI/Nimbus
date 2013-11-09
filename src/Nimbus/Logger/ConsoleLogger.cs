using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void Debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(Exception exc, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine(exc.ToString());
        }
    }
}