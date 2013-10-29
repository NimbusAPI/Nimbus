using System;

namespace Nimbus.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void Verbose(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Verbose(Exception exception, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine(exception.ToString());
        }

        public void Debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine(exception.ToString());
        }

        public void Information(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Information(Exception exception, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine(exception.ToString());
        }

        public void Warning(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Warning(Exception exception, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine(exception.ToString());
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine(exception.ToString());
        }

        public void Fatal(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine(exception.ToString());
        }
    }
}