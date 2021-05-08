using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DexBot.Services
{
	class CommandHandler
	{
		public static CommandService Commands { get; private set; }

		private readonly CommandService _commands;
		private readonly DiscordSocketClient _client;
		private readonly IServiceProvider _services;

		private const string Source = "CommandHandler";

		public CommandHandler(IServiceProvider services)
		{
			_commands = services.GetRequiredService<CommandService>();
			_client = services.GetRequiredService<DiscordSocketClient>();
			_services = services;

			_commands.CommandExecuted += CommandExecutedAsync;
			_client.MessageReceived += MessageReceivedAsync;

			Commands = _commands;
		}

		public async Task InitializeAsync()
		{
			await _commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), _services);
		}

		private async Task MessageReceivedAsync(SocketMessage rawMessage)
		{
			if (!(rawMessage is SocketUserMessage message))
				return;

			if (message.Source != MessageSource.User)
				return;

			var argPos = 0;
			if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Bot.CommandPrefix, ref argPos)))
				return;

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var context = new SocketCommandContext(_client, message);
			IResult result = await _commands.ExecuteAsync(context, argPos, _services);
			stopwatch.Stop();

			if (result.IsSuccess)
				await Program.LogAsync($"Command took {stopwatch.ElapsedMilliseconds} ms", Source);
		}

		private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
		{
			if (!command.IsSpecified)
			{
				if (context.Guild != null)
					await Program.LogAsync($"Unknown Command! [{context.User.Username}#{context.User.Discriminator}] in [{context.Guild.Name}/#{context.Channel.Name}] / [{context.Message}]", Source);
				else
					await Program.LogAsync($"Unknown Command! [{context.User.Username}#{context.User.Discriminator}] in [{context.Channel.Name}] / [{context.Message}]", Source);
				return;
			}

			if (result.IsSuccess)
			{
				if (context.Guild != null)
					await Program.LogAsync($"[{context.User.Username}#{context.User.Discriminator}] ran [{command.Value.Name}] in [{context.Guild.Name}/#{context.Channel.Name}]", Source);
				else
					await Program.LogAsync($"[{context.User.Username}#{context.User.Discriminator}] ran [{command.Value.Name}] in [{context.Channel.Name}]", Source);
				return;
			}

			if (context.Guild != null)	
				await Program.LogAsync($"Something has gone terribly wrong! [{context.User.Username}#{context.User.Discriminator}] in [{context.Guild.Name}/#{context.Channel.Name}] / [{result}]", Source);
			else
				await Program.LogAsync($"Something has gone terribly wrong! [{context.User.Username}#{context.User.Discriminator}] in [{context.Channel.Name}] / [{result}]", Source);
		}
	}
}

