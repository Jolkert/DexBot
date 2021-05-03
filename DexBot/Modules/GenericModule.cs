using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DexBot.Modules
{
	public class GenericModule : ModuleBase<SocketCommandContext>
	{
		private const string HelpDocUrl = "https://docs.google.com/document/d/1CvEa6Fp4mHlToc9yQRZRLk2hBGOj5TWleEEyx73gxCU/edit?usp=sharing";
		private const string OAuthUrl = "https://discord.com/api/oauth2/authorize?client_id=832840741931843614&permissions=8&scope=bot";
		private const string GitHubUrl = "https://github.com/Jolkert/DexBot";
		private const ulong OwnerId = 227916147540885505;

		[Command("help"), Alias("?")]
		public async Task HelpCommand([Remainder] string _ = "") 
		{
			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("Dexbot help!")
				.WithDescription($"Dexbot command prefix is `{Config.bot.cmdPrefix}`")
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
	}
}
