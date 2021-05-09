using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DexBot
{
	public static class Logger
	{// This thing is probably not very good, but I'm gonna use it anyway because I am stubborn and don't want to use another package -Jolkert 2021-05-06
		private static string _logFile;
		private static FileStream _stream;

		static Logger() => StartStream();


		public static async Task LogToFileAsync(string log)
		{
			await _stream.WriteAsync(Encoding.UTF8.GetBytes($"{log}\n"));
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
