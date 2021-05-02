using Discord;
using Discord.Commands;
using PokeAPI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("data"), Alias("dt", "d"), Name("Data")]
	public class DataModule : ModuleBase<SocketCommandContext>
	{
		private static readonly IParsableModule[] _modules = new IParsableModule[]
		{
			new AbilityModule(),
			new MoveModule(),
			new ItemModule(),
			new TypeModule(),
			new PokemonModule()
		};

		private const string Source = "Data";

		[Command, Name("Data")]
		public async Task DataCommand([Remainder] string input)
		{// This feels kinda weird but it works. Can't come up with a better way off the top of my head so I'm gonna leave it -Jolkert 2021-07-27
		 // Parallelization working but like kinda weird i guess? maybe look at it later -2021-04-29
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			await Program.LogAsync($"Attempting to get data for [{input}]...", Source);


			NamedApiObject apiObject = null;
			IParsableModule useModule = null;

			List<Task<bool>> tasks = new List<Task<bool>>();
			Parallel.ForEach(_modules, module =>
			{
				tasks.Add(Task.Run(async () =>
				{
					NamedApiObject tempObj = await module.ParseAsync(input);
					if (tempObj != null)
					{
						apiObject = tempObj;
						useModule = module;
						return true;
					}

					return false;
				}));
			});

			Task<bool> task;
			do
			{
				task = await Task.WhenAny(tasks);
				tasks.Remove(task);
			} while (!task.Result && tasks.Count > 0);


			Embed embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Data not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Unable to find any matching object in the Pokédex!")
					.WithColor(Util.DefaultColor)
					.Build();
			if (apiObject != null)
				embed = useModule.GetData(apiObject);


			await Util.ReplaceEmbedAsync(message, embed);
		}
	}
}
