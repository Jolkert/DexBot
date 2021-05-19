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
		private static readonly string _version = "1.1.3";

		static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

		public async Task StartAsync()
		{
			using (var services = ConfigureServices())
			{
				if (Config.Bot.Token == null || Config.Bot.Token == "BOT_TOKEN_GOES_HERE")
				{
					await LogAsync("Bot token not found. Make sure you have your bot token set in Resources/config.json", "Startup");
					Stop();
				}
				if (Config.Bot.CommandPrefix == null || Config.Bot.CommandPrefix == "")
				{
					await LogAsync("Command prefix not found. Make sure you have your command prefix set in Resources/config.json", "Startup");
					Stop();
				}

				await LogAsync($"Starting DexBot v{_version}", "Startup");
				Client = services.GetRequiredService<DiscordSocketClient>();
				Client.Log += LogAsync;
				Client.Ready += OnReadyAsync;
				Client.JoinedGuild += async (SocketGuild guild) => await LogAsync($"Joined {guild.Name} ({guild.Id})", "ServerJoin");

				services.GetRequiredService<CommandService>().Log += LogAsync;
				await Client.LoginAsync(TokenType.Bot, Config.Bot.Token);
				await Client.StartAsync();
				await services.GetRequiredService<CommandHandler>().InitializeAsync();

				await Task.Delay(-1);
			}
		}

		private async Task OnReadyAsync()
		{
			await Client.SetGameAsync($"{Config.Bot.CommandPrefix}help", null, ActivityType.Listening);
			foreach (SocketGuild guild in Client.Guilds)
				await LogAsync($"Connected to {guild.Name} ({guild.Id})", "Startup");

			await LogAsync($"Bot is active in {Client.Guilds.Count} servers!", "Startup");
		}

		private static Task LogAsync(LogMessage log)
		{
			string write = $"[{log.Severity}] {log.ToString()}";
			Console.WriteLine(write);
			if (Config.Bot.OutputLogsToFile)
				Logger.LogToFile(write);
			return Task.CompletedTask;
		}

		public static Task LogAsync(string message, string source)
		{
			return LogAsync(new LogMessage(LogSeverity.Info, source, message));
		}

		public static void Stop()
		{
			if (Config.Bot.OutputLogsToFile)
				Logger.Close();
			Environment.Exit(Environment.ExitCode);
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
