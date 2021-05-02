using DexBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DexBot
{
	class Program
	{
		public static DiscordSocketClient Client;
		private static readonly string _version = "1.0.1";

		static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

		public async Task StartAsync()
		{
			using (var services = ConfigureServices())
			{
				if (Config.bot.token == null)
					return;

				await LogAsync($"Starting DexBot v{_version}", "DexBot");
				Client = services.GetRequiredService<DiscordSocketClient>();
				Client.Log += LogAsync;

				services.GetRequiredService<CommandService>().Log += LogAsync;
				await Client.LoginAsync(TokenType.Bot, Config.bot.token);
				await Client.StartAsync();
				await services.GetRequiredService<CommandHandler>().InitializeAsync();
				await Client.SetGameAsync($"{Config.bot.cmdPrefix}help", null, ActivityType.Listening);

				await Task.Delay(-1);
			}
		}

		private static Task LogAsync(LogMessage log)
		{
			Console.WriteLine($"[{log.Severity}] {log.ToString()}");
			return Task.CompletedTask;
		}

		public static Task LogAsync(string message, string source)
		{
			return LogAsync(new LogMessage(LogSeverity.Info, source, message));
		}

		private ServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandler>()
				.BuildServiceProvider();
		}
	}
}
