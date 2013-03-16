using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace sample.console.client
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            using (new ForeverLoggingClass())
            {
                Console.WriteLine("Will keep logging a message per second.");
                Console.ReadLine();
            }
            LogManager.Shutdown();
        }
    }

    public class ForeverLoggingClass : IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(ForeverLoggingClass));
        private readonly CancellationTokenSource cancellationTokenSource;

        public ForeverLoggingClass()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(LogSomeStuff, cancellationTokenSource.Token);
        }

        private void LogSomeStuff()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Log.Info("info message");
                Log.Error(new Exception("some random exception"));
                Thread.Sleep(10000);
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
