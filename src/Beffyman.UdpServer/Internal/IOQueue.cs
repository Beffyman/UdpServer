using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Threading;

namespace Beffyman.UdpServer.Internal
{
	internal sealed class IOQueue : PipeScheduler
	{
		private static readonly WaitCallback _doWorkCallback = s => ((IOQueue)s).DoWork();

		private readonly object _workSync = new object();
		private readonly ConcurrentQueue<Work> _workItems = new ConcurrentQueue<Work>();
		private bool _doingWork;

		public override void Schedule(Action<object> action, object state)
		{
			var work = new Work(action, state);

			_workItems.Enqueue(work);

			lock (_workSync)
			{
				if (!_doingWork)
				{
					System.Threading.ThreadPool.UnsafeQueueUserWorkItem(_doWorkCallback, this);
					_doingWork = true;
				}
			}
		}

		private void DoWork()
		{
			while (true)
			{
				while (_workItems.TryDequeue(out Work item))
				{
					item.Callback(item.State);
				}

				lock (_workSync)
				{
					if (_workItems.IsEmpty)
					{
						_doingWork = false;
						return;
					}
				}
			}
		}

		private readonly struct Work
		{
			public readonly Action<object> Callback;
			public readonly object State;

			public Work(Action<object> callback, object state)
			{
				Callback = callback;
				State = state;
			}
		}
	}
}
