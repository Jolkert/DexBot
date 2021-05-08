using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PokeAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("type"), Alias("t"), Name(Source)]
	public class TypeModule : ModuleBase<SocketCommandContext>, IModuleWithHelp, IParsableModule
	{
		private const string Source = "Type";
		public string ModuleName => Source;

		private const int ImageSize = 512;
		private const ulong ImageDumpServerId = 390334803972587530;
		private const ulong ImageDumpChannelId = 833784145284038686;

		private static readonly Dictionary<string, ulong> _typeEmoteIds = new Dictionary<string, ulong>()
		{
			{ "normal", 836473654752575511 },
			{ "fighting", 836473654191456257 },
			{ "flying", 836473655046569994 },
			{ "poison", 836473655172792350 },
			{ "ground", 836473654556360735 },
			{ "rock", 836473654744317953 },
			{ "bug", 836473652358283284 },
			{ "ghost", 836473654849437707 },
			{ "steel", 836473654820470846 },
			{ "fire", 836473655047225375 },
			{ "water", 836473655159816202 },
			{ "grass", 836473655185113099 },
			{ "electric", 836473654577332244 },
			{ "psychic", 836473655181180938 },
			{ "ice", 836473654925459456 },
			{ "dragon", 836473653590753300 },
			{ "dark", 836473652391575589 },
			{ "fairy", 836473655005020170 }
		};

		[Command("help"), Alias("?"), Name(Source + " Help"), Priority(1)]
		public async Task HelpCommand()
		{
			string description = "Gets data about the specified type or the type matchups of the specified pokémon";
			string usage = "<type|pokémon>";

			await Context.Channel.SendMessageAsync("", false, Util.CreateHelpEmbed(description, usage, this).Build());
		}

		[Command, Name("Type")]
		public async Task TypeCommand([Remainder] string input)
		{
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			Embed embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Type/Pokémon not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Unable to find a matching type or pokémon in the pokédex!")
					.WithColor(Util.DefaultColor)
					.Build();

			PokemonType type = await ParseTypeAsync(input);
			if (type != null)
				embed = TypeData(type);
			else
			{
				FullPokemon pokemon = await PokemonModule.ParsePokemonAsync(input);
				if (pokemon.Pokemon != null)
					embed = await WeaknessModule.WeakPokemonAsync(pokemon.Pokemon);
			}

			await Util.ReplaceEmbedAsync(message, embed);
		}

		public static async Task<PokemonType> ParseTypeAsync(string parse)
		{
			parse = Util.TrimNonWords(parse.ToLowerInvariant());

			PokemonType type = null;
			try
			{
				await Program.LogAsync($"Trying to get type [{parse}]...", Source);
				type = await DataFetcher.GetNamedApiObject<PokemonType>(parse);
			}
			catch (HttpRequestException) { }

			return type;
		}
		public static Embed TypeData(PokemonType type)
		{
			string typeName = Util.GetName(type);
			(string offenseMatchups, string defenseMatchups) = GetMatchups(type);

			return new EmbedBuilder()
				.WithTitle(typeName)
				.WithThumbnailUrl(GetImageUrl(type))
				.WithColor(Util.GetColor(type))
				.WithFields(new EmbedFieldBuilder[]
				{
					new EmbedFieldBuilder().WithName("Offense:").WithValue(offenseMatchups).WithIsInline(false),
					new EmbedFieldBuilder().WithName("Defense:").WithValue(defenseMatchups).WithIsInline(false)
				})
				.WithFooter(Util.PokeApiFooter)
				.Build();
		}

		private static (string, string) GetMatchups(PokemonType type)
		{
			string offense = $"2x Damage: {GetTypesString(type.DamageRelations.DoubleDamageTo)}\n" +
							 $"½x Damage: {GetTypesString(type.DamageRelations.HalfDamageTo)}\n" +
							 $"Immune: {GetTypesString(type.DamageRelations.NoDamageTo)}";

			string defense = $"2x Damage: {GetTypesString(type.DamageRelations.DoubleDamageFrom)}\n" +
							 $"½x Damage: {GetTypesString(type.DamageRelations.HalfDamageFrom)}\n" +
							 $"Immune: {GetTypesString(type.DamageRelations.NoDamageFrom)}";

			return (offense, defense);
		}

		private static string GetTypesString(NamedApiResource<PokemonType>[] types)
		{
			if (types.Length == 0)
				return "None";

			List<string> typeStrings = new List<string>();
			foreach (NamedApiResource<PokemonType> type in types)
				typeStrings.Add(Util.FixName(type.Name));

			return string.Join('/', typeStrings);
		}

		public static string GetImageUrl(PokemonType type)
		{
			return GetImageUrl(type.Name);
		}
		public static string GetImageUrl(string type)
		{
			return AppData.TypeIconUrls[type];
		}
		public static string GetImageUrl(List<PokemonType> types)
		{
			List<string> typesStrings = new List<string>();
			foreach (PokemonType type in types)
				typesStrings.Add(type.Name);

			return GetImageUrl(typesStrings);
		}
		public static string GetImageUrl(List<string> types)
		{
			using (Bitmap image = new Bitmap(ImageSize * (types.Count > 1 ? 2 : 1), ImageSize * (int)Math.Ceiling(types.Count / 2.0)))
			{
				using (Graphics graphics = Graphics.FromImage(image))
				{
					for (int i = 0; i < types.Count; i++)
					{
						using (Bitmap icon = new Bitmap(System.Drawing.Image.FromFile(AppData.GetOrDownloadTypeIconPath(types[i]))))
						{
							if (i == types.Count - 1 && i % 2 == 0)
								graphics.DrawImage(icon, image.Width / 2 - ImageSize / 2, ImageSize * (i / 2), ImageSize, ImageSize);
							else
								graphics.DrawImage(icon, ImageSize * (i % 2), ImageSize * (i / 2), ImageSize, ImageSize);
						}
					}
				}

				image.Save(AppData.GetTypeIconPath("send"), System.Drawing.Imaging.ImageFormat.Png);
			}

			string url = ((SocketTextChannel)Program.Client.GetGuild(ImageDumpServerId).GetChannel(ImageDumpChannelId)).SendFileAsync(AppData.GetTypeIconPath("send"), "")
						.Result.Attachments.ToArray()[0].ProxyUrl;
			File.Delete(AppData.GetTypeIconPath("send"));
			return url;
		}

		public static Emote GetTypeEmote(string id)
		{
			return Emote.Parse($"<:type_{id}:{_typeEmoteIds[id]}>");
		}



		public async Task<NamedApiObject> ParseAsync(string parse) => await ParseTypeAsync(parse);
		public Embed GetData(NamedApiObject apiObject) => TypeData(apiObject as PokemonType);
	}

	[Group("weakness"), Alias("weak", "wk", "w"), Name("Weakness")]
	public class WeaknessModule : ModuleBase<SocketCommandContext>, IModuleWithHelp
	{
		private const string Source = "Weakness";
		public string ModuleName => Source;
		
		private static readonly string[] _typeNames = new string[]
			{
				"normal",
				"fighting",
				"flying",
				"poison",
				"ground",
				"rock",
				"bug",
				"ghost",
				"steel",
				"fire",
				"water",
				"grass",
				"electric",
				"psychic",
				"ice",
				"dragon",
				"dark",
				"fairy"
			};

		[Command("help"), Alias("?"), Name(Source + " Help"), Priority(1)]
		public async Task HelpCommand()
		{
			string description = "Gets the type matchups of the specified pokémon or of a pokémon that is all of the types specified.";
			string usage = "<pokémon|(type1 type2 ...)>";

			await Context.Channel.SendMessageAsync("", false, Util.CreateHelpEmbed(description, usage, this).Build());
		}

		[Command, Name("Weakness")]
		public async Task WeakCommand([Remainder] string input)
		{
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			Embed embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Pokémon/Types not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Unable to find a matching pokémon or types in the pokédex!")
					.WithColor(Util.DefaultColor)
					.Build();

			await Program.LogAsync($"Attempting to parse types from string [{input}]", Source);
			if (ParseTypesFromString(input, out List<PokemonType> types))
				embed = WeakTypes(types);
			else
			{
				FullPokemon pokemon = await PokemonModule.ParsePokemonAsync(input);
				if (pokemon != null)
					embed = await WeakPokemonAsync(pokemon.Pokemon);
			}

			await Util.ReplaceEmbedAsync(message, embed);
		}

		public static async Task<Embed> WeakPokemonAsync(Pokemon pokemon)
		{
			List<PokemonType> types = new List<PokemonType>();
			foreach (PokemonTypeMap type in pokemon.Types)
				types.Add(await DataFetcher.GetNamedApiObject<PokemonType>(type.Type.Name));

			MultiTypeRelations relations = new MultiTypeRelations(types);

			string weaknesses = GetWeaknessesString(relations);
			string resistances = GetResistancesString(relations);
			string immunities = GetImmunitiesString(relations);

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle($"{Util.GetName(pokemon)} matchup chart (ignoring abilities!)")
				.WithDescription("Bold indicates double!")
				.WithColor(Util.GetColor(pokemon))
				.WithThumbnailUrl(Util.GetMonSprite(pokemon))
				.WithFields(new EmbedFieldBuilder[]
				{
					new EmbedFieldBuilder().WithName("Weaknesses:").WithValue(weaknesses != "" ? weaknesses : "N/A").WithIsInline(false),
					new EmbedFieldBuilder().WithName("Resistances:").WithValue(resistances != "" ? resistances : "N/A").WithIsInline(false),
					new EmbedFieldBuilder().WithName("Immunities:").WithValue(immunities != "" ? immunities : "N/A").WithIsInline(false)
				})
				.WithFooter(Util.PokeApiFooter);

			return embed.Build();
		}
		public static Embed WeakTypes(List<PokemonType> types)
		{
			MultiTypeRelations relations = new MultiTypeRelations(types);

			string weaknesses = GetWeaknessesString(relations);
			string resistances = GetResistancesString(relations);
			string immunities = GetImmunitiesString(relations);

			List<string> typeNames = new List<string>();
			foreach (PokemonType type in relations.Types)
				typeNames.Add(Util.GetName(type));

			string typeStr = string.Join('/', typeNames);

			Discord.Color[] typeColors = new Discord.Color[relations.Types.Count];
			for (int i = 0; i < typeColors.Length; i++)
				typeColors[i] = Util.GetColor(relations.Types[i].Name);
			Discord.Color avgColor = Util.AverageColor(typeColors);

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle($"{typeStr} matchup chart (ignoring abilities!)")
				.WithDescription("Bold indicates double!")
				.WithColor(avgColor)
				.WithThumbnailUrl(TypeModule.GetImageUrl(relations.Types))
				.WithFields(new EmbedFieldBuilder[]
				{
					new EmbedFieldBuilder().WithName("Weaknesses:").WithValue(weaknesses != "" ? weaknesses : "None").WithIsInline(false),
					new EmbedFieldBuilder().WithName("Resistances:").WithValue(resistances != "" ? resistances : "None").WithIsInline(false),
					new EmbedFieldBuilder().WithName("Immunities:").WithValue(immunities != "" ? immunities : "None").WithIsInline(false)
				})
				.WithFooter(Util.PokeApiFooter);

			return embed.Build();
		}


		private static bool ParseTypesFromString(string input, out List<PokemonType> types)
		{
			types = new List<PokemonType>();


			Regex regex = new Regex(@$"\b({string.Join('|', _typeNames)})\b", RegexOptions.IgnoreCase);

			foreach (string match in regex.Matches(input)
										.OfType<Match>()
										.Select(m => m.Value.ToLowerInvariant())
										.Distinct())
				types.Add(DataFetcher.GetNamedApiObject<PokemonType>(match).Result);

			return types.Count > 0;
		}

		private static string GetWeaknessesString(MultiTypeRelations relations)
		{
			List<string> weaknesses = new List<string>();
			foreach (KeyValuePair<string, int> pair in relations.Weaknesses)
			{
				string name = Util.FixName(pair.Key);
				switch (pair.Value)
				{
					case int n when n > 3:
						weaknesses.Add($"***{name} (x{(int)Math.Pow(2, n)})***");
						break;
					case 3:
						weaknesses.Add($"***{name}***");
						break;
					case 2:
						weaknesses.Add($"**{name}**");
						break;
					default:
						weaknesses.Add(name);
						break;
				}
			}

			return string.Join('/', weaknesses);
		}
		private static string GetResistancesString(MultiTypeRelations relations)
		{
			List<string> resistances = new List<string>();
			foreach (KeyValuePair<string, int> pair in relations.Resistances)
			{
				string name = Util.FixName(pair.Key);
				switch (pair.Value)
				{
					case int n when n < -3:
						resistances.Add($"***{name} (x1/{(int)Math.Pow(2, Math.Abs(n))})***");
						break;
					case -3:
						resistances.Add($"***{name}***");
						break;
					case -2:
						resistances.Add($"**{name}**");
						break;
					default:
						resistances.Add(name);
						break;
				}
			}

			return string.Join('/', resistances);
		}
		private static string GetImmunitiesString(MultiTypeRelations relations)
		{
			List<string> immunities = new List<string>();
			foreach (string immunity in relations.Immunities)
				immunities.Add(Util.FixName(immunity));

			return string.Join('/', immunities);
		}

		

		public struct MultiTypeRelations
		{
			public List<PokemonType> Types { get; }

			public List<KeyValuePair<string, int>> Weaknesses { get; }
			public List<KeyValuePair<string, int>> Resistances { get; }
			public List<string> Immunities { get; }

			private static readonly Dictionary<string, int> _typeSort = new Dictionary<string, int>
			{
				{ "normal", 0 },
				{ "fighting", 1 },
				{ "flying", 2 },
				{ "poison", 3 },
				{ "ground", 4 },
				{ "rock", 5 },
				{ "bug", 6 },
				{ "ghost", 7 },
				{ "steel", 8 },
				{ "fire", 9 },
				{ "water", 10 },
				{ "grass", 11 },
				{ "electric", 12 },
				{ "psychic", 13 },
				{ "ice", 14 },
				{ "dragon", 15 },
				{ "dark", 16 },
				{ "fairy", 17 }
			};

			public MultiTypeRelations(List<PokemonType> types)
			{
				this.Types = new List<PokemonType>();
				foreach (PokemonType type in types)
					this.Types.Add(type);

				this.Weaknesses = new List<KeyValuePair<string, int>>();
				this.Resistances = new List<KeyValuePair<string, int>>();
				this.Immunities = new List<string>();
				this.SetRelations();
			}

			private void SetRelations()
			{// I really hate this entire method. It looks gross and it feels gross, and there's definitely a better way to do it, I just can't think of it -Jolkert 2021-04-27
			 // TODO: Make a better one :((
				Dictionary<string, int> temp = new Dictionary<string, int>();

				// Add all types to the temp dictionary
				foreach (PokemonType type in this.Types)
				{
					TypeRelations relations = type.DamageRelations;

					foreach (NamedApiResource<PokemonType> weakType in relations.DoubleDamageFrom)
					{
						string typeName = weakType.Name;
						if (temp.TryGetValue(typeName, out int value))
						{
							if (temp[typeName] > int.MinValue)
								temp[typeName]++;
						}
						else
							temp.Add(typeName, 1);
					}

					foreach (NamedApiResource<PokemonType> resistType in relations.HalfDamageFrom)
					{
						string typeName = resistType.Name;
						if (temp.TryGetValue(typeName, out int value))
						{
							if (temp[typeName] > int.MinValue)
								temp[typeName]--;
						}
						else
							temp.Add(typeName, -1);
					}

					foreach (NamedApiResource<PokemonType> immuneType in relations.NoDamageFrom)
					{
						string typeName = immuneType.Name;
						if (temp.TryGetValue(typeName, out int value))
							temp[typeName] = int.MinValue;
						else
							temp.Add(typeName, int.MinValue);
					}
				}

				// Move temp dictionary into correct fields
				foreach (KeyValuePair<string, int> pair in temp)
				{
					switch (pair.Value)
					{
						case int.MinValue:
							this.Immunities.Add(pair.Key);
							break;
						case int i when i > 0:
							this.Weaknesses.Add(pair);
							break;
						case int i when i < 0:
							this.Resistances.Add(pair);
							break;

						default: break;
					}
				}

				this.Weaknesses.Sort(CompareDescending);
				this.Resistances.Sort(CompareAscending);
				this.Immunities.Sort();
			}

			private static int CompareAscending(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
			{
				if (a.Value == b.Value)
					return _typeSort[a.Key].CompareTo(_typeSort[b.Key]);
				else
					return a.Value.CompareTo(b.Value);
			}
			private static int CompareDescending(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
			{
				if (a.Value == b.Value)
					return _typeSort[a.Key].CompareTo(_typeSort[b.Key]);
				else
					return b.Value.CompareTo(a.Value);
			}
		}
	}
}
