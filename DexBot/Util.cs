using DexBot.Modules;
using Discord;
using PokeAPI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DexBot
{
	public static class Util
	{
		public static readonly Color DefaultColor;
		public static readonly Color DexColor;
		public static readonly EmbedFooterBuilder PokeApiFooter;
		public static readonly Random Random;
		public const string QuestionMarkUnownUrl = "https://i.imgur.com/nyt4vzr.png";
		public const string ExclaimationMarkUnownUrl = "https://i.imgur.com/zgrSxKR.png";

		private static readonly Dictionary<string, Color> _colors;
		private const int ShinyOdds = 4096;

		static Util()
		{
			_colors = new Dictionary<string, Color>
			{
				{ "black", new Color(0x000000) },
				{ "blue", new Color(0x0000ff) },
				{ "brown", new Color(0xa5682a) },
				{ "gray", new Color(0x808080) },
				{ "green", new Color(0x008000) },
				{ "pink", new Color(0xffc0cb) },
				{ "purple", new Color(0x800080) },
				{ "red", new Color(0xff0000) },
				{ "white", new Color(0xfefefe) },
				{ "yellow", new Color(0xffff00) },

				{ "p_normal", new Color(0xa8a878) },
				{ "p_fighting", new Color(0xc03028) },
				{ "p_flying", new Color(0xa890f0) },
				{ "p_poison", new Color(0xa040a0) },
				{ "p_ground", new Color(0xe0c068) },
				{ "p_rock", new Color(0xb8a038) },
				{ "p_bug", new Color(0xa8b820) },
				{ "p_ghost", new Color(0x705898) },
				{ "p_steel", new Color(0xb8b8d0) },
				{ "p_fire", new Color(0xf08030) },
				{ "p_water", new Color(0x6890f0) },
				{ "p_grass", new Color(0x78c850) },
				{ "p_electric", new Color(0xf8d030) },
				{ "p_psychic", new Color(0xf85888) },
				{ "p_ice", new Color(0x98d8d8) },
				{ "p_dragon", new Color(0x7038f8) },
				{ "p_dark", new Color(0x705848) },
				{ "p_fairy", new Color(0xee99ac) },

				{ "normal", new Color(0x919AA2) },
				{ "fighting", new Color(0xE0306A) },
				{ "flying", new Color(0x89AAE3) },
				{ "poison", new Color(0xB567CE) },
				{ "ground", new Color(0xE87236) },
				{ "rock", new Color(0xC8B686) },
				{ "bug", new Color(0x83C300) },
				{ "ghost", new Color(0x4C6AB2) },
				{ "steel", new Color(0x5A8EA2) },
				{ "fire", new Color(0xFF9741) },
				{ "water", new Color(0x3692DC) },
				{ "grass", new Color(0x38BF4B) },
				{ "electric", new Color(0xFBD100) },
				{ "psychic", new Color(0xFF6675) },
				{ "ice", new Color(0x4CD1C0) },
				{ "dragon", new Color(0x006FC9) },
				{ "dark", new Color(0x5B5466) },
				{ "fairy", new Color(0xFB89EB) }
			};
			DefaultColor = new Color(0x68a090);
			DexColor = new Color(0xe04040);

			PokeApiFooter = new EmbedFooterBuilder().WithText($"Retrieved from pokeapi.co").WithIconUrl("https://raw.githubusercontent.com/PokeAPI/media/master/logo/pokeapi_256.png");
			Random = new Random();
		}

		public static string FixName(string input)
		{
			string[] split = input.Split('-');

			for (int i = 0; i < split.Length; i++)
				split[i] = InitialCaps(split[i]);

			return string.Join(' ', split);

		}
		public static string TrimNonWords(string input)
		{
			Regex nonWordsAtBoundary = new Regex(@"(^\W+|\W+$)");
			Regex nonWords = new Regex(@"\W");


			return nonWords.Replace(nonWordsAtBoundary.Replace(input, ""), "-");
		}

		public static string GetName(FullPokemon pokemon)
		{
			return GetName(pokemon.Pokemon, pokemon.Species);
		}
		public static string GetName(Pokemon pokemon, PokemonSpecies species = null)
		{
			if (species == null)
				species = PokemonModule.GetSpeciesAsync(pokemon).Result;

			string speciesName = null;
			foreach (ResourceName name in species.Names)
			{
				if (name.Language.Name == "en")
				{
					speciesName = name.Name;
					break;
				}
			}
			if (speciesName == null)
				throw new NoEnglishNameException(species);


			if (speciesName.ToLowerInvariant() == pokemon.Name || PokemonModule.GetDefaultFormAsync(species).Result.Name == pokemon.Name)
				return speciesName;
			else
			{
				if (PokemonModule.GetDefaultFormName(species) == pokemon.Name)
					return speciesName;

				int variantStartIndex = pokemon.Name.IndexOf($"{species.Name.ToLowerInvariant()}-") + $"{speciesName.ToLowerInvariant()}-".Length;

				string variantName = FixName(pokemon.Name[variantStartIndex..]);
				return $"{speciesName} - {variantName}";
			}
		}
		public static string GetName(PokemonType type)
		{
			foreach (ResourceName name in type.Names)
				if (name.Language.Name == "en")
					return name.Name;

			throw new NoEnglishNameException(type);
		}
		public static string GetName(Ability ability)
		{
			foreach (ResourceName name in ability.Names)
				if (name.Language.Name == "en")
					return name.Name;

			throw new NoEnglishNameException(ability);
		}
		public static string GetName(Move move)
		{
			foreach (ResourceName name in move.Names)
				if (name.Language.Name == "en")
					return name.Name;

			throw new NoEnglishNameException(move);
		}
		public static string GetName(Item item)
		{
			foreach (ResourceName name in item.Names)
				if (name.Language.Name == "en")
					return name.Name;

			throw new NoEnglishNameException(item);
		}

		private static string InitialCaps(string input)
		{
			return $"{$"{input[0]}".ToUpper()}{input[1..]}";
		}

		public static string GetMonSprite(Pokemon pokemon)
		{
			Random rand = new Random();
			return rand.Next(ShinyOdds) > 0 ? pokemon.Sprites.FrontMale : pokemon.Sprites.FrontShinyMale;
		}

		public static Color GetColor(string key)
		{
			return _colors.TryGetValue(key, out Color color) ? color : DefaultColor;
		}
		public static Color GetColor(PokemonType type)
		{
			return GetColor(type.Name);
		}
		public static Color GetColor(Pokemon pokemon)
		{
			return GetColor($"p_{pokemon.Types[0].Type.Name}");
		}

		public static async Task<IUserMessage> SendSearchMessageAsync(IMessageChannel channel)
		{
			EmbedBuilder embed = new EmbedBuilder()
				.WithAuthor(new EmbedAuthorBuilder().WithName("Searching...").WithIconUrl(QuestionMarkUnownUrl))
				.WithTitle("Depending on your search, it might take a while")
				.WithColor(DefaultColor);
			IUserMessage message = await channel.SendMessageAsync("", false, embed.Build());
			return message;
		}
		public static async Task ReplaceEmbedAsync(IUserMessage message, Embed embed)
		{
			await message.ModifyAsync(m => m.Embed = embed);
		}

		public static Color AverageColor(params Color[] colors)
		{
			int r = 0;
			int g = 0;
			int b = 0;
			foreach (Color color in colors)
			{
				r += color.R;
				b += color.B;
				g += color.G;
			}

			r /= colors.Length;
			g /= colors.Length;
			b /= colors.Length;

			return new Color(r, g, b);
		}
	}

	class NoEnglishNameException : Exception
	{
		NamedApiObject ApiObject { get; }

		public NoEnglishNameException(NamedApiObject apiObject) : base($"ApiObject [{apiObject.Name}] has no english name!")
		{
			ApiObject = apiObject;
		}
	}
}
