using Discord;
using Discord.Commands;
using PokeAPI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("levelset"), Alias("lvlset", "levelup", "lvlup", "lvl", "levelmoves", "levelmv", "lvlmv"), Name(Source)]
	public class LevelsetModule : ModuleBase<SocketCommandContext>, IModuleWithHelp
	{
		private const string Source = "Levelset";
		public string ModuleName => Source;

		public static readonly List<string> _gameNames = new List<string>()
		{
			"fire-red",
			"leaf-green",

			"heart-gold",
			"soul-silver",

			"omega-ruby",
			"alpha-sapphire",

			"red",
			"blue",
			"rb",
			"yellow",

			"gold",
			"silver",
			"gs",
			"crystal",

			"ruby",
			"sapphire",
			"rs",
			"emerald",
			"firered",
			"leafgreen",
			"frlg",

			"diamond",
			"pearl",
			"dp",
			"platinum",
			"plat",
			"heartgold",
			"soulsilver",
			"hgss",

			"black-2",
			"white-2",
			"black2",
			"white2",
			"black",
			"white",
			"bw",
			"b2b2",
			"bw2",

			"x",
			"y",
			"xy",
			"omegaruby",
			"alphasapphire",
			"oras",

			"ultra-sun",
			"ultra-moon",
			"ultrasun",
			"ultramoon",
			"sun",
			"moon",
			"sm",
			"sumo",
			"usum",

			"colosseum",
			"xd"
		};
		private static readonly Regex _gamesRegex = new Regex($@"(-|^|(?<=\W))(?<game>{string.Join('|', _gameNames)})(-|$|(?=\W))");

		[Command("help"), Alias("?"), Name(Source + " Help"), Priority(1)]
		public async Task HelpCommand()
		{
			string description = "Gets the specified pokémon's levelup moveset in the specified game";
			string usage = "<pokemon> <game>";

			await Context.Channel.SendMessageAsync("", false, Util.CreateHelpEmbed(description, usage, this).Build());
		}

		[Command]
		public async Task LevelsetCommand([Remainder] string input)
		{
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			Embed embed;

			input = Util.TrimNonWords(input.ToLowerInvariant()).Replace('é', 'e').Replace(' ', '-');

			MatchCollection matches = _gamesRegex.Matches(input);
			if (matches.Count > 0 && matches[0].Groups.TryGetValue("game", out Group group))
			{
				string gameId = GetGameId(group.Value);
				FullPokemon pokemon = await PokemonModule.ParsePokemonAsync(IsolatePokemon(input));


				if (pokemon != null)
				{
					if (pokemon.Species.Generation.ID > GetGameGen(gameId))
					{
						embed = new EmbedBuilder()
							.WithAuthor(new EmbedAuthorBuilder().WithName("Error!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
							.WithTitle($"{Util.GetName(pokemon)} does not exist in Gen {GetGameGen(gameId)}!")
							.WithColor(Util.DefaultColor)
							.Build();
					}
					else
						embed = LevelsetData(pokemon, gameId);
				}
				else
				{
					embed = new EmbedBuilder()
						.WithAuthor(new EmbedAuthorBuilder().WithName("Pokémon not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
						.WithTitle("A matching pokémon could not be found!")
						.WithColor(Util.DefaultColor)
						.Build();
				}

			}
			else
			{
				embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Game not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Make sure to specify a game when looking for levelup moves!")
					.WithColor(Util.DefaultColor)
					.Build();
			}

			await Util.ReplaceEmbedAsync(message, embed);
		}

		private static Embed LevelsetData(FullPokemon pokemon, string gameId)
		{
			List<(int, string)> levelset = GetLevelset(pokemon.Pokemon, gameId);
			levelset.Sort();

			string levelsetString = "```\n" +
									"Lvl ║ Move\n" +
									"════╬═════════════\n";
			foreach ((int level, string move) in levelset)
				levelsetString += $"{(level < 10 ? " " : "")}{(level < 100 ? " " : "")}{level} ║ {move}\n";
			levelsetString += "```";

			return new EmbedBuilder()
				.WithTitle(Util.GetName(pokemon.Pokemon))
				.WithDescription($"Levelup moveset in {Util.FixName(gameId)}\n" +
								 $"{levelsetString}\n" +
								 $"*(Note that level up moves may be different between games in the same generation. Make sure you are looking at the right game!)*")
				.WithThumbnailUrl(Util.GetMonSprite(pokemon.Pokemon))
				.WithColor(Util.GetColor(pokemon.Pokemon))
				.WithFooter(Util.PokeApiFooter)
				.Build();
		}
		private static List<(int, string)> GetLevelset(Pokemon pokemon, string gameId)
		{
			return (from PokemonMove move in pokemon.Moves
					from MoveVersionGroupDetails details in move.VersionGroupDetails
					where details.VersionGroup.Name == gameId && details.LearnMethod.Name == "level-up"
					select (details.LearnedAt, Util.FixName(move.Move.Name))).ToList();
		}
		private static string IsolatePokemon(string input)
		{
			return _gamesRegex.Replace(input, "");
		}
		private static string GetGameId(string input)
		{
			switch (input)
			{
				case "red":
				case "blue":
				case "rb":
					return "red-blue";

				case "gold":
				case "silver":
				case "gs":
					return "gold-silver";

				case "ruby":
				case "sapphire":
				case "rs":
					return "ruby-sapphire";

				case "fire-red":
				case "leaf-green":
				case "firered":
				case "leafgreen":
				case "frlg":
					return "firered-leafgreen";

				case "diamond":
				case "pearl":
				case "dp":
					return "diamond-pearl";

				case "platinum":
				case "plat":
					return "platinum";

				case "heartgold":
				case "soulsilver":
				case "heart-gold":
				case "soul-silver":
				case "hgss":
					return "heartgold-soulsilver";

				case "black":
				case "white":
				case "bw":
					return "black-white";

				case "black-2":
				case "white-2":
				case "black2":
				case "white2":
				case "b2b2":
				case "bw2":
					return "black-2-white-2";

				case "x":
				case "y":
				case "xy":
					return "x-y";

				case "omega-ruby":
				case "alpha-sapphire":
				case "omegaruby":
				case "alphasapphire":
				case "oras":
					return "omega-ruby-alpha-sapphire";

				case "sun":
				case "moon":
				case "sm":
				case "sumo":
					return "sun-moon";

				case "ultra-sun":
				case "ultra-moon":
				case "ultrasun":
				case "ultramoon":
				case "usum":
					return "ultra-sun-ultra-moon";

				case string str when _gameNames.Contains(str):
					return input;


				default:
					return "";
			}
		}
		private static int GetGameGen(string gameId)
		{
			switch (gameId)
			{
				case "red-blue":
				case "yellow":
					return 1;

				case "gold-silver":
				case "crystal":
					return 2;

				case "ruby-sapphire":
				case "emerald":
				case "firered-leafgreen":
					return 3;

				case "diamond-pearl":
				case "platinum":
				case "heartgold-soulsilver":
					return 4;

				case "black-white":
				case "black-2-white-2":
					return 5;

				case "x-y":
				case "omega-ruby-alpha-sapphire":
					return 6;

				case "sun-moon":
				case "ultra-sun-ultra-moon":
					return 7;

				case "colosseum":
				case "xd":
					return 1;

				default:
					return -1;
			}
		}
	}
}
