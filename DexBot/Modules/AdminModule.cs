using Discord.Commands;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("admin"), RequireOwner, Name(Source)]
	public class AdminModule : ModuleBase<SocketCommandContext>
	{
		private const string Source = "Admin";

		[Command("stop"), RequireOwner, Name("Stop")]
		public async Task StopBot()
		{
			await Program.LogAsync($"{Context.User.Username}#{Context.User.Discriminator} is stopping the bot...", Source);
			Program.Stop();
		}
	}
}
