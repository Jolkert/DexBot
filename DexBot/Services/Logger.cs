using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace DexBot
{
	public static class Logger
	{// This thing is probably not very good, but I'm gonna use it anyway because I am stubborn and don't want to use another package -Jolkert 2021-05-06
		private static string _logFile;
		private static FileStream _stream;
		private static bool _streamOpen;
		private const int LogSaveDelayMs = 1000;

		static Logger()
		{
			StartStream();
			StartAutoRestart(LogSaveDelayMs);
		}

		public static async Task LogToFileAsync(string log) => await _stream.WriteAsync(Encoding.UTF8.GetBytes($"{log}\n"));

		public static void Close()
		{
			_stream.Close();
		}

		private static void StartStream()
		{
			 _streamOpen = false;
			if (!Directory.Exists("Resources/logs"))
				Directory.CreateDirectory("Resources/logs");

			string nowString = DateTime.Now.ToLocalTime().ToString().Replace(':', '-').Replace(' ', '_');
			_logFile = $"Resources/logs/{nowString}.txt";
			_stream = new FileStream(_logFile, FileMode.Append);

			_stream.Write(Encoding.UTF8.GetBytes($"Starting Log: {nowString}\n"));
			_streamOpen = true;
		}
		private static void RestartStream()
		{
			Close();
			_stream = new FileStream(_logFile, FileMode.Append);
		}

		private static void StartAutoRestart(int millis = 10000)
		{
			new Thread(async () =>
			{
				Thread.CurrentThread.IsBackground = true;
				while (true)
				{
					await Task.Delay(millis);
					if (_streamOpen)
						RestartStream();
				}

			}).Start();
		}
	}
}
