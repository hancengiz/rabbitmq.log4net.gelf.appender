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
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ForeverLoggingClass()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(LogSomeStuff, _cancellationTokenSource.Token);
        }

        private void LogSomeStuff()
        {
            int count = 0;
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Log.Info(string.Format("info message : {0}", ++count));
                Log.Error(new Exception(string.Format("some random exception {0}", ++count)));
                Thread.Sleep(new Random().Next(1, 10) * 1000);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
