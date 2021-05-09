using Discord;
using Discord.Commands;
using PokeAPI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("pokemon"), Alias("poke", "mon", "p"), Name(Source)]
	public class PokemonModule : ModuleBase<SocketCommandContext>, IModuleWithHelp, IParsableModule
	{
		private const string Source = "Pokemon";
		public string ModuleName => Source;

		[Command("help"), Alias("?"), Name(Source + " Help"), Priority(1)]
		public async Task HelpCommand()
		{
			string description = "Gets data about the specified pokémon. If no form is specified, default will be assumed. " +
										"All forms of any pokémon which has at least one form with different stats or abilities are supported.";
			string usage = "<pokémon>";

			EmbedBuilder embed = Util.CreateHelpEmbed(description, usage, this)
				.AddField(new EmbedFieldBuilder().WithName("Subcommand").WithValue("`random`"));

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		// If you add a form alias to this list, you must add the PokeAPI name of the form to the switch in GetFormString() if you want it to work! -Jolkert 2021-04-27
		private static readonly List<string> _formStrings = new List<string>()
		{
			"mega",
			"m",

			"gigantamax",
			"gmax",

			"alolan",
			"alola",
			"a",

			"galarian",
			"galar",
			"g",

			"totem",


			"shield-down",
			"shields-down",

			"shadow",
			"ice-rider",
			"shadow-rider",

			"cosplay",
			"rock-star",
			"belle",
			"pop-star",
			"phd",
			"libre",

			"original-cap",
			"hoenn-cap",
			"sinnoh-cap",
			"unova-cap",
			"kalos-cap",
			"alola-cap",
			"partner-cap",
			"partner-cap",

			"sunny",
			"rainy",
			"snowy",

			"primal",

			"attack",
			"defense",
			"speed",

			"plant",
			"sandy",
			"trash",

			"heat",
			"wash",
			"frost",
			"fan",
			"mow",

			"altered",
			"origin",

			"land",
			"sky",

			"red-striped",
			"red-stripe",
			"blue-striped",
			"blue-stripe",

			"standard",
			"zen-mode",
			"zen",

			"incarnate",
			"i",
			"therian",
			"t",

			"black",
			"white",

			"ordinary",
			"resolute",

			"aria",
			"pirouette",

			"ash",

			"eternal-flower",
			"eternal",

			"shield",
			"blade",

			"average",
			"small",
			"large",
			"super",

			"10-",
			"10",
			"dog",
			"50-",
			"50",
			"100-",
			"100",
			"complete",

			"confined",
			"unbound",

			"baile",
			"pom-pom",
			"pompom",
			"pa-u",
			"pau",
			"sensu",

			"midday",
			"midnight",
			"dusk",

			"solo",
			"school",

			"red",
			"orange",
			"yellow",
			"green",
			"blue",
			"indigo",
			"violet",
			"meteor",
			"core",

			"disguised",
			"busted",

			"dawn-wings",
			"dawn",
			"dusk-mane",
			"dusk",
			"ultra",

			"original",

			"amped",
			"low-key",

			"noice-face",
			"no-ice-face",
			"noice",
			"no-ice",
			"ice-face",
			"ice",



			"hero-of-many-battles",
			"hero",
			"crowned",

			"eternamax",

			"single-strike-style",
			"single-strike",
			"single",

			"rapid-strike-style",
			"rapid-strike",
			"rapid"
		};
		private static readonly Regex _allFormsRegex = new Regex($@"(-|^|(?<=\W))(?<form>{string.Join('|', _formStrings)})(-|$|(?=\W))");
		private const int NationalDexCount = 898;

		[Command, Name(Source)]
		public async Task PokemonCommand([Remainder] string input)
		{
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			Embed embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Pokémon not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Unable to find a matching pokémon in the Pokédex!")
					.WithColor(Util.DefaultColor)
					.Build();

			FullPokemon pokemon = await ParsePokemonAsync(input);
			if (pokemon != null)
				embed = PokemonData(pokemon);

			await Util.ReplaceEmbedAsync(message, embed);
		}

		public static async Task<FullPokemon> ParsePokemonAsync(string parse)
		{
			parse = Util.TrimNonWords(parse.ToLowerInvariant()).Replace('é', 'e').Replace(' ', '-');
			FullPokemon pokemon = null;

			await Program.LogAsync($"Trying to find pokemon [{parse}]", Source);
			List<Task<FullPokemon>> tasks = new List<Task<FullPokemon>>()
			{
				Task.Run(() => GetPokemonAsync(parse).Result),
				Task.Run(() => GetPokemonAsSpeciesAsync(parse).Result),
				Task.Run(() => GetAltFormAsync(parse).Result)
			};

			bool shouldContinue = true;
			while (shouldContinue)
			{
				Task<FullPokemon> task = await Task.WhenAny(tasks);
				FullPokemon foundPokemon = task.Result;

				if (foundPokemon != null)
					pokemon = foundPokemon;

				tasks.Remove(task);
				shouldContinue = tasks.Count > 0;
			}

			return await GetFullPokemonAsync(pokemon);
		}

		public static Embed PokemonData(FullPokemon pokemon)
		{
			return PokemonData(pokemon.Pokemon, pokemon.Species);
		}
		public static Embed PokemonData(Pokemon pokemon, PokemonSpecies species)
		{
			string name = Util.GetName(pokemon, species);

			// get en genus
			string genus = "";
			foreach (Genus g in species.Genera)
				if (g.Language.Name == "en")
					genus = g.Name;

			// get types
			string types = $"{TypeModule.GetTypeEmote(pokemon.Types[0].Type.Name)} {Util.FixName(pokemon.Types[0].Type.Name)}";
			if (pokemon.Types.Length > 1)
				types += $"\n{TypeModule.GetTypeEmote(pokemon.Types[1].Type.Name)} {Util.FixName(pokemon.Types[1].Type.Name)}";

			// get abilities
			(string normalAbilities, string hiddenAbility) = GetAbilityNames(pokemon.Abilities);

			// stats
			string stats = "```";

			int bst = 0;
			for (int i = 0; i < pokemon.Stats.Length; i++)
			{
				int stat = pokemon.Stats[i].BaseValue;
				stats += $"{(stat < 10 ? " " : "")}{(stat < 100 ? " " : "")}{stat} {(i < pokemon.Stats.Length - 1 ? "│ " : "")}";
				bst += stat;
			}
			stats += $"║ {bst} ";
			stats += "\n HP │ Atk │ Def │ SpA │ SpD │ Spe ║ BST ```";

			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle(name)
				.WithDescription(genus)
				.WithColor(Util.GetColor(pokemon))
				.WithThumbnailUrl(Util.GetMonSprite(pokemon))
				.WithFields(new EmbedFieldBuilder[]
				{
					new EmbedFieldBuilder().WithName("Type:").WithValue(types).WithIsInline(false),
					new EmbedFieldBuilder().WithName("Abilities:").WithValue(normalAbilities).WithIsInline(true),
					new EmbedFieldBuilder().WithName("Hidden Ability:").WithValue(hiddenAbility).WithIsInline(true),
					new EmbedFieldBuilder().WithName("Base Stats").WithValue(stats).WithIsInline(false)
				})
				.WithFooter(Util.PokeApiFooter);

			return embed.Build();
		}

		public static async Task<FullPokemon> GetFullPokemonAsync(Pokemon pokemon)
		{
			return new FullPokemon(pokemon, await GetSpeciesAsync(pokemon));
		}
		public static async Task<FullPokemon> GetFullPokemonAsync(PokemonSpecies species)
		{
			return new FullPokemon(await GetDefaultFormAsync(species), species);
		}
		public static async Task<FullPokemon> GetFullPokemonAsync(FullPokemon pokemon)
		{
			if (pokemon == null)
				return null;
			if (pokemon.Pokemon != null && pokemon.Species != null)
				return pokemon;
			else if (pokemon.Species == null && pokemon.Pokemon != null)
				return await GetFullPokemonAsync(pokemon.Pokemon);
			else if (pokemon.Pokemon == null && pokemon.Species != null)
				return await GetFullPokemonAsync(pokemon.Species);
			else
				return null;

		}

		private static async Task<FullPokemon> GetPokemonAsync(string parse)
		{
			try
			{
				return await GetFullPokemonAsync(await DataFetcher.GetNamedApiObject<Pokemon>(parse));
			}
			catch (HttpRequestException)
			{
				return null;
			}
		}
		private static async Task<FullPokemon> GetPokemonAsSpeciesAsync(string parse)
		{
			try
			{
				return await GetFullPokemonAsync(await DataFetcher.GetNamedApiObject<PokemonSpecies>(parse));
			}
			catch (HttpRequestException)
			{
				return null;
			}
		}
		private static async Task<FullPokemon> GetAltFormAsync(string parse)
		{// This method is a disaster but it works fine I guess -Jolkert 2021-04-27
			FullPokemon pokemon = null;

			MatchCollection matches = _allFormsRegex.Matches(parse);
			if (matches.Count > 0)
			{
				string baseSpeciesName = _allFormsRegex.Replace(parse, "", matches.Count);
				if (baseSpeciesName != "")
				{
					PokemonSpecies species = null;
					try
					{
						species = await DataFetcher.GetNamedApiObject<PokemonSpecies>(baseSpeciesName);
					}
					catch (HttpRequestException) { }

					if (species != null)
					{
						List<string> foundForms = new List<string>();
						foreach (Match match in matches)
						{
							if (match.Groups.TryGetValue("form", out Group group))
							{
								switch (species.Name)
								{
									case "basculin":
										switch (group.Value)
										{
											case "red":
												foundForms.Add("red-striped");
												break;
											case "blue":
												foundForms.Add("blue-striped");
												break;

											default:
												foundForms.Add(GetFormString(group.Value));
												break;
										}
										break;

									case "calyrex":
										switch (group.Value)
										{
											case "ice":
												foundForms.Add("ice-rider");
												break;

											default:
												foundForms.Add(GetFormString(group.Value));
												break;
										}
										break;

									default:
										foundForms.Add(GetFormString(group.Value));
										break;
								}
							}
						}

						if (species.Name == "minior" && !foundForms.Contains("meteor"))
							foundForms.Add("core");

						Regex foundFormsRegex = new Regex($@"\b({string.Join('|', foundForms)})\b");


						string bestMatch = "";
						int numBestMatches = 0;
						int bestMatchIndex = -1;
						Parallel.For(0, species.Varieties.Length, i =>
						{
							PokemonSpeciesVariety variant = species.Varieties[i];
							string variantName = variant.Pokemon.Name;
							if (species.Name == "minior" && !variant.Pokemon.Name.Contains("meteor")) // stupid fuckin api works like this and doest list minior right :((
								variantName += "-core";

							MatchCollection formMatches = foundFormsRegex.Matches(variantName);
							if (formMatches.Count > numBestMatches && i > bestMatchIndex)
							{
								numBestMatches = formMatches.Count;
								bestMatch = variant.Pokemon.Name;
								bestMatchIndex = i;
							}
						});

						if (bestMatch != "")
							pokemon = await GetFullPokemonAsync(await DataFetcher.GetNamedApiObject<Pokemon>(bestMatch));
					}
				}
			}


			return pokemon;
		}
		private static string GetFormString(string input)
		{
			switch (input)
			{
				case "m":
					return "mega";

				case "gigantamax":
					return "gmax";

				case "alolan":
				case "a":
					return "alola";

				case "galarian":
				case "g":
					return "galar";

				case "red-stripe":
					return "red-striped";
				case "blue-stripe":
					return "blue-striped";

				case "zen-mode":
					return "zen";

				case "i":
					return "incarnate";
				case "t":
					return "therian";

				case "eternal-flower":
					return "eternal";

				case "10-":
				case "dog":
					return "10";
				case "50-":
					return "50";
				case "100-":
				case "100":
					return "complete";

				case "pompom":
					return "pom-pom";
				case "pa-u":
					return "pau";

				case "shields-down":
				case "shield-down":
					return "core";

				case "dawn-wings":
					return "dawn";
				case "dusk-mane":
					return "dusk";

				case "ice-face":
					return "ice";
				case "noice-face":
				case "no-ice-face":
				case "no-ice":
					return "noice";

				case "hero-of-many-battles":
					return "hero";

				case "single-strike-style":
				case "single":
					return "single-strike";
				case "rapid-strike-style":
				case "rapid":
					return "rapid-strike";

				case "shadow":
					return "shadow-rider";

				case string str when _formStrings.Contains(str):
					return str;

				default:
					return "";
			}
		}

		public static async Task<PokemonSpecies> GetSpeciesAsync(Pokemon pokemon)
		{
			await Program.LogAsync("Fetching species", Source);
			return (await DataFetcher.GetNamedApiObject<PokemonSpecies>(pokemon.Species.Name));
		}
		public static async Task<Pokemon> GetDefaultFormAsync(PokemonSpecies species)
		{
			foreach (PokemonSpeciesVariety variety in species.Varieties)
				if (variety.IsDefault)
				{
					await Program.LogAsync("Fetching pokemon", Source);
					return await DataFetcher.GetNamedApiObject<Pokemon>(variety.Pokemon.Name);
				}

			throw new NoDefaultFormException(species);
		}
		public static string GetDefaultFormName(PokemonSpecies species)
		{
			foreach (PokemonSpeciesVariety variety in species.Varieties)
				if (variety.IsDefault)
					return variety.Pokemon.Name;

			throw new NoDefaultFormException(species);
		}

		private static (string, string) GetAbilityNames(PokemonAbility[] abilities)
		{
			(string Normal, string Hidden) outputs = (Normal: "", Hidden: "");

			foreach (PokemonAbility ability in abilities)
			{
				if (!ability.IsHidden)
				{
					if (outputs.Normal == "")
						outputs.Normal = Util.FixName(ability.Ability.Name);
					else
						outputs.Normal += $"\n{Util.FixName(ability.Ability.Name)}";
				}
				else
					outputs.Hidden = Util.FixName(ability.Ability.Name);
			}

			if (outputs.Hidden == "")
				outputs.Hidden = "None";

			return outputs;
		}
		public static async Task<List<(Ability, bool)>> GetAbilitiesAsync(Pokemon pokemon)
		{
			return await GetAbilitiesAsync(pokemon.Abilities);
		}
		public static async Task<List<(Ability, bool)>> GetAbilitiesAsync(PokemonAbility[] abilities)
		{
			List<(Ability, bool)> list = new List<(Ability, bool)>();

			foreach (PokemonAbility pokemonAbility in abilities)
			{
				Ability ability = await DataFetcher.GetNamedApiObject<Ability>(pokemonAbility.Ability.Name);
				list.Add((ability, pokemonAbility.IsHidden));
			}

			return list;
		}

		// IParsableModule implementations
		public async Task<NamedApiObject> ParseAsync(string parse) => await ParsePokemonAsync(parse);
		public Embed GetData(NamedApiObject apiObject) => PokemonData(apiObject as FullPokemon);

		[Group("random"), Alias("rand", "roll", "rng"), Name(Source)]
		public class RandomPokemonModule : PokemonModule, IModuleWithHelp
		{
			public new const string Source = "Random Pokemon";
			public new string ModuleName => Source;

			[Command("help"), Alias("?"), Name(Source + " Help"), Priority(2)]
			public new async Task HelpCommand()
			{
				string description = "Gets the same information from the main command about a random pokémon. 1 to 6 pokémon can be generated, " +
										"and/or a generation can be specified *(1 pokémon and all generations is default)*";
				string usage = "[number]|[gen#]";

				await Context.Channel.SendMessageAsync("", false, Util.CreateHelpEmbed(description, usage, this).Build());
			}

			[Command, Name(Source), Priority(1)]
			public Task RandomPokemonCommand([Remainder] string parse = "")
			{
				parse = parse.ToLowerInvariant();
				Regex numberRegex = new Regex(@"(\b\d\b)");
				MatchCollection matches = numberRegex.Matches(parse);

				int pokemonToGenerate;
				if (matches.Count > 0)
					pokemonToGenerate = int.Parse(matches[0].Value);
				else
					pokemonToGenerate = 1;

				if (pokemonToGenerate > 6)
					pokemonToGenerate = 6;


				Regex genRegex = new Regex(@"gen(?<gen>\d+)");
				int gen = -1;
				if (parse.Contains("gen"))
				{
					MatchCollection genMatches = genRegex.Matches(parse);
					if (genMatches.Count > 0 && genMatches[0].Groups.TryGetValue("gen", out Group value))
						gen = int.Parse(value.Value);
				}

				int lowerBound;
				int upperBound;
				switch (gen)
				{
					case 1:
						lowerBound = 1;
						upperBound = 151;
						break;
					case 2:
						lowerBound = 152;
						upperBound = 251;
						break;
					case 3:
						lowerBound = 252;
						upperBound = 386;
						break;
					case 4:
						lowerBound = 387;
						upperBound = 493;
						break;
					case 5:
						lowerBound = 494;
						upperBound = 649;
						break;
					case 6:
						lowerBound = 650;
						upperBound = 721;
						break;
					case 7:
						lowerBound = 722;
						upperBound = 809;
						break;
					case 8:
						lowerBound = 810;
						upperBound = 898;
						break;

					default:
						lowerBound = 1;
						upperBound = NationalDexCount;
						break;
				}


				Random rand = new Random();
				List<int> dexNumbers = new List<int>();
				for (int i = 0; i < pokemonToGenerate; i++)
				{
					int toAdd = rand.Next(lowerBound, upperBound + 1);
					if (!dexNumbers.Contains(toAdd))
						dexNumbers.Add(toAdd);
					else
						i--;
				}

				Parallel.ForEach(dexNumbers, async dexNumber => await PokemonCommand($"{dexNumber}"));
				return Task.CompletedTask;
			}
		}
	}




	public class FullPokemon : NamedApiObject
	{
		public Pokemon Pokemon { get; set; }
		public PokemonSpecies Species { get; set; }

		public FullPokemon(Pokemon pokemon, PokemonSpecies species = null)
		{
			Pokemon = pokemon;
			Species = species;
		}
	}

	class NoDefaultFormException : Exception
	{// All pokemon species should have a default form. If none is found throw this exception
		public PokemonSpecies Species { get; }

		public NoDefaultFormException(PokemonSpecies species) : base($"Species [{species.Name}] has no default form!")
		{
			Species = species;
		}
	}
}
