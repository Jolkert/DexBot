using Discord;
using Discord.Commands;
using PokeAPI;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("ability"), Alias("abil", "a"), Name(Source)]
	public class AbilityModule : ModuleBase<SocketCommandContext>, IModuleWithHelp, IParsableModule
	{
		private const string Source = "Ability";
		public string ModuleName => Source;

		[Command("help"), Alias("?"), Name(Source + " Help"), Priority(1)]
		public async Task HelpCommand()
		{
			string description = "Gets information about the specified ability or about the specified pokémon's abilitie(s)";
			string usage = "<ability|pokémon>";

			await Context.Channel.SendMessageAsync("", false, Util.CreateHelpEmbed(description, usage, this).Build());
		}

		[Command, Name(Source)]
		public async Task AbilityCommand([Remainder] string input)
		{
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			Embed embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Ability/Pokémon not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Unable to find a matching ability or pokémon in the Pokédex!")
					.WithColor(Util.DefaultColor)
					.Build();

			Ability ability = await ParseAbilityAsync(input);
			if (ability != null)
				embed = AbilityData(ability);
			else
			{
				FullPokemon pokemon = await PokemonModule.ParsePokemonAsync(input);
				if (pokemon != null)
					embed = await AbiliityData(pokemon);
			}

			await Util.ReplaceEmbedAsync(message, embed);
		}
		public static async Task<Ability> ParseAbilityAsync(string parse)
		{
			parse = Util.TrimNonWords(parse.ToLowerInvariant()).Replace(' ', '-');

			Ability ability = null;
			try
			{
				await Program.LogAsync($"Trying to find ability [{parse}]...", Source);
				ability = await DataFetcher.GetNamedApiObject<Ability>(parse);
			}
			catch (HttpRequestException) { }

			return ability;
		}

		public static Embed AbilityData(Ability ability)
		{
			string abilityName = Util.GetName(ability);

			string abilityEffect = GetAbilityEffect(ability);

			Pokemon displayMon = DataFetcher.GetNamedApiObject<Pokemon>(ability.Pokemon[Util.Random.Next(ability.Pokemon.Length)].Pokemon.Name).Result;

			return new EmbedBuilder()
					.WithTitle(abilityName)
					.WithDescription(abilityEffect)
					.WithThumbnailUrl(Util.GetMonSprite(displayMon))
					.WithColor(Util.GetColor(displayMon)).WithFooter(Util.PokeApiFooter)
					.Build();
		}
		public static async Task<Embed> AbiliityData(FullPokemon pokemon)
		{
			return await AbilityData(pokemon.Pokemon, pokemon.Species);
		}
		public static async Task<Embed> AbilityData(Pokemon pokemon, PokemonSpecies species = null)
		{
			List<(Ability, bool)> abilityList = await PokemonModule.GetAbilitiesAsync(pokemon);
			List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

			foreach ((Ability, bool) set in abilityList)
			{
				fields.Add(new EmbedFieldBuilder()
					.WithName($"{(set.Item2 ? "*" : "")}{Util.GetName(set.Item1)}{(set.Item2 ? "*" : "")}")
					.WithValue(GetAbilityEffect(set.Item1, true))
					.WithIsInline(false));
			}

			return new EmbedBuilder()
				.WithTitle($"{Util.GetName(pokemon, species)}\'s Abilities")
				.WithDescription("(*Italics* indicates hidden ability)")
				.WithThumbnailUrl(Util.GetMonSprite(pokemon))
				.WithColor(Util.GetColor(pokemon))
				.WithFields(fields)
				.WithFooter(Util.PokeApiFooter)
				.Build();
		}

		private static string GetAbilityEffect(Ability ability, bool getShortEffect = false)
		{
			foreach (VerboseEffect effect in ability.Effects)
				if (effect.Language.Name == "en")
					return getShortEffect ? effect.ShortEffect : effect.Effect;

			throw new NoEnglishNameException(ability);
		}



		// IParsableModule implementations
		public async Task<NamedApiObject> ParseAsync(string parse) => await ParseAbilityAsync(parse);
		public Embed GetData(NamedApiObject apiObject) => AbilityData(apiObject as Ability);
	}
}
