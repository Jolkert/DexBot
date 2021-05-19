using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DexBot
{
	public static class Logger
	{// This thing is probably not very good, but I'm gonna use it anyway because I am stubborn and don't want to use another package -Jolkert 2021-05-06
		private static string _logFile;
		private static FileStream _stream;
		private static readonly Queue<string> _writeQueue;
		private static Thread _writeThread;

		static Logger()
		{
			_writeQueue = new Queue<string>();
			StartStream();
		}


		public static void LogToFile(string log)
		{
			_writeQueue.Enqueue(log);

			if (_writeThread == null || _writeThread.ThreadState == ThreadState.Stopped)
			{
				_writeThread = new Thread(new ThreadStart(async () =>
				{
					while (_writeQueue.Count > 0)
						await LogToFileFromQueueAsync();
				}));
				_writeThread.Start();
			}


		}
		private static async Task LogToFileFromQueueAsync()
		{
			await _stream.WriteAsync(Encoding.UTF8.GetBytes($"{_writeQueue.Dequeue()}\n"));
			if (_writeQueue.Count == 0)
				RestartStream();
		}

		public static void Close() => _stream.Close();

		private static void StartStream()
		{
			if (!Directory.Exists("Resources/logs"))
				Directory.CreateDirectory("Resources/logs");

			string nowString = DateTime.Now.ToLocalTime().ToString().Replace(':', '-').Replace(' ', '_');
			_logFile = $"Resources/logs/{nowString}.txt";
			_stream = new FileStream(_logFile, FileMode.Append);

			_stream.Write(Encoding.UTF8.GetBytes($"Starting Log: {nowString}\n"));
		}
		private static void RestartStream()
		{
			Close();
			_stream = new FileStream(_logFile, FileMode.Append);
		}
	}
}
