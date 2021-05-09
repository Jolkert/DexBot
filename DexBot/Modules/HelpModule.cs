using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("help"), Alias("?"), Name(Source)]
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		private const string Source = "Help";

		private const string HelpDocUrl = "https://docs.google.com/document/d/1CvEa6Fp4mHlToc9yQRZRLk2hBGOj5TWleEEyx73gxCU/edit?usp=sharing";
		private const string OAuthUrl = "https://discord.com/api/oauth2/authorize?client_id=832840741931843614&permissions=8&scope=bot";
		private const string GitHubUrl = "https://github.com/Jolkert/DexBot";
		private const ulong OwnerId = 227916147540885505;

		[Command, Name(Source), Priority(1)]
		public async Task HelpCommand()
		{
			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Dexbot help!")
				.WithDescription($"Dexbot command prefix is `{Config.Bot.CommandPrefix}`\n" +
									$"For more information about a specific command, run `{Config.Bot.CommandPrefix}help <command>`")
				.WithFields(new EmbedFieldBuilder[]
				{
					new EmbedFieldBuilder().WithName("Helpful links").WithValue($"[Command Help]({HelpDocUrl})\n" +
																				$"[Add to your own server!]({OAuthUrl})\n" +
																				$"[GitHub Repository]({GitHubUrl})")
				})
				.WithColor(Util.DexColor)
				.WithFooter(new EmbedFooterBuilder()
					.WithIconUrl(Program.Client.GetUser(OwnerId).GetAvatarUrl())
					.WithText("Bot author: @Jolkert#2991"));

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command, Name(Source)]
		public async Task HelpCommand([Remainder] string command)
		{
			//Regex regex = new Regex(@"\W");
			command = command/*regex.Replace(command, "")*/.ToLowerInvariant();

			ModuleInfo module = Services.CommandHandler.Commands.Modules.Where(mod => mod.Aliases.Contains(command)).FirstOrDefault();
			CommandInfo helpCommand = module?.Commands.Where(cmd => cmd.Aliases.Contains($"{command} help")).FirstOrDefault();

			if (helpCommand == null)
				await HelpCommand();
			else
				await helpCommand.ExecuteAsync(Context, new List<object>(), new List<object>(), null);
		}
	}
}
