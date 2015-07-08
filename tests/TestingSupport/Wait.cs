namespace tests.TestingSupport
{
	using System;
	using System.Threading;

	namespace tests.TestingSupport
	{
		public class Wait
		{
			private readonly double timeout;
			private readonly int interval;
			private const int DefaultInterval = 100;

			private Wait(double timeout, int interval = DefaultInterval)
			{
				this.interval = interval;
				this.timeout = timeout;
			}

			public static Wait For(TimeSpan timespan)
			{
				return new Wait(timespan.TotalMilliseconds);
			}

			public static Wait For(int seconds)
			{
				return For(new TimeSpan(0, 0, 0, seconds));
			}

			public void Until(Func<bool> condition)
			{
				Until(condition,
					  () =>
					  {
						  throw new TimeoutException(string.Format("Expected {0} after waiting for {1} milliseconds",
																   condition, timeout));
					  });
			}

			public void Until(Func<bool> condition, Action success, Action failure)
			{
				var timeIsUp = DateTime.Now.AddMilliseconds(timeout);
				while (DateTime.Now.CompareTo(timeIsUp) < 0)
				{
					if (condition())
					{
						success();
						return;
					}
					new ManualResetEvent(false).WaitOne(interval);
				}
				failure();
			}

			public void Until(Func<bool> condition, Action failure)
			{
				Until(condition, () => { }, failure);
			}

			public void ThenContinue()
			{
				new ManualResetEvent(false).WaitOne((int)timeout);
			}
		}
	}
}