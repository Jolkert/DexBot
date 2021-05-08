using Discord;
using Discord.Commands;
using PokeAPI;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("item"), Alias("i"), Name(Source)]
	public class ItemModule : ModuleBase<SocketCommandContext>, IModuleWithHelp, IParsableModule
	{
		private const string Source = "Item";
		public string ModuleName => Source;

		[Command("help"), Alias("?"), Name(Source + " Help"), Priority(1)]
		public async Task HelpCommand()
		{
			string description = "Gets data about the specified item";
			string usage = "<item>";

			await Context.Channel.SendMessageAsync("", false, Util.CreateHelpEmbed(description, usage, this).Build());
		}

		[Command, Name(Source)]
		public async Task ItemCommand([Remainder] string input)
		{
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			Embed embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Item not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Unable to find a matching item in the Pokédex!")
					.WithDescription("(Most items added in Gen VIII are not yet in the database, if you are looking for a Gen VIII item, this might not be your fault)")
					.WithColor(Util.DefaultColor)
					.Build();

			Item item = await ParseItemAsync(input);
			if (item != null)
				embed = ItemData(item);

			await Util.ReplaceEmbedAsync(message, embed);
		}

		public static async Task<Item> ParseItemAsync(string parse)
		{
			parse = Util.TrimNonWords(parse.ToLowerInvariant()).Replace('é', 'e').Replace(' ', '-');
			Regex ballFix = new Regex(@"(?<=\w)ball\b");
			parse = ballFix.Replace(parse, "-ball");

			Item item = null;
			try
			{
				await Program.LogAsync($"Trying to get item [{parse}]", Source);
				item = await DataFetcher.GetNamedApiObject<Item>(parse);
			}
			catch (HttpRequestException) { }

			return item;
		}
		public static Embed ItemData(Item item)
		{
			string itemName = Util.GetName(item);
			string itemEffect = GetItemEffect(item);

			return new EmbedBuilder()
				.WithTitle(itemName)
				.WithDescription(itemEffect)
				.WithThumbnailUrl(item.Sprites.Default)
				.WithColor(Util.DexColor)
				.WithFooter(Util.PokeApiFooter)
				.Build();
		}

		private static string GetItemEffect(Item item, bool getShortEffect = false)
		{
			foreach (VerboseEffect effect in item.Effects)
				if (effect.Language.Name == "en")
					return getShortEffect ? effect.ShortEffect : effect.Effect;

			throw new NoEnglishNameException(item);
		}



		// IParsableModule implementations
		public async Task<NamedApiObject> ParseAsync(string parse) => await ParseItemAsync(parse);
		public Embed GetData(NamedApiObject apiObject) => ItemData(apiObject as Item);
	}
}
