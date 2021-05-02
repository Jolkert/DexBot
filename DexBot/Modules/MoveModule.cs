using Discord;
using Discord.Commands;
using PokeAPI;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	[Group("move"), Alias("mv", "m"), Name("Move")]
	public class MoveModule : ModuleBase<SocketCommandContext>, IParsableModule
	{
		private const string Source = "Move";

		[Command, Name("Move")]
		public async Task MoveCommand([Remainder] string input)
		{
			IUserMessage message = await Util.SendSearchMessageAsync(Context.Channel);
			Embed embed = new EmbedBuilder()
					.WithAuthor(new EmbedAuthorBuilder().WithName("Move not found!").WithIconUrl(Util.ExclaimationMarkUnownUrl))
					.WithTitle("Unable to find a matching move in the Pokédex!")
					.WithDescription("(Some Gen VIII moves are not yet in the database, if you are looking for a Gen VIII move, this might not be your fault)")
					.WithColor(Util.DefaultColor)
					.Build();

			Move move = await ParseMoveAsync(input);
			if (move != null)
				embed = MoveData(move);

			await Util.ReplaceEmbedAsync(message, embed);
		}

		public static Embed MoveData(Move move)
		{
			string moveName = Util.GetName(move);
			string moveEffect = null;
			try
			{
				moveEffect = GetMoveEffect(move);
			}
			catch (NoEnglishNameException) { }

			string damageClass = Util.FixName(move.DamageClass.Name);
			int? basePower = move.Power;
			int? accuracy = (int?)move.Accuracy;
			int pp = move.PP.Value;


			return new EmbedBuilder()
				.WithTitle(moveName)
				.WithDescription(moveEffect ?? "Unable to find effect!\n*(Many new Gen VIII moves do not yet have their effects listed in the database. Sorry about that!)*")
				.WithThumbnailUrl(TypeModule.GetImageUrl(move.Type.Name))
				.WithColor(Util.GetColor(move.Type.Name))
				.WithFields(new EmbedFieldBuilder[]
				{
					new EmbedFieldBuilder().WithName("Type").WithValue(Util.FixName(move.Type.Name)).WithIsInline(true),
					new EmbedFieldBuilder().WithName("Category").WithValue(damageClass).WithIsInline(true),
					new EmbedFieldBuilder().WithName("PP").WithValue($"{pp} (max: {pp * 1.6})").WithIsInline(true),
					new EmbedFieldBuilder().WithName("Base Power").WithValue(basePower == null ? "——" : $"{basePower}").WithIsInline(true),
					new EmbedFieldBuilder().WithName("Accuracy").WithValue(accuracy == null ? "——" : $"{accuracy}").WithIsInline(true)
				})
				.WithFooter(Util.PokeApiFooter)
				.Build();
		}

		public static async Task<Move> ParseMoveAsync(string parse)
		{
			parse = Util.TrimNonWords(parse.ToLowerInvariant()).Replace('é', 'e').Replace(' ', '-');

			Move move = null;
			try
			{
				parse = TestForSpecialCase(parse);
				await Program.LogAsync($"Trying to find move [{parse}]...", Source);
				move = await DataFetcher.GetNamedApiObject<Move>(parse);
			}
			catch (HttpRequestException) { }

			return move;
		}
		private static string TestForSpecialCase(string input)
		{
			switch (input)
			{
				case "rocks":
					return "stealth-rock";
				case "tbolt":
					return "thunderbolt";
				case "ddance":
					return "dragon-dance";
				case "eq":
					return "earthquake";

				default:
					return input;
			}
		}

		private static string GetMoveEffect(Move move, bool getShortEffect = false)
		{
			foreach (var effect in move.Effects)
				if (effect.Language.Name == "en")
				{
					string moveEffect = getShortEffect ? effect.ShortEffect : effect.Effect;
					return moveEffect.Replace("$effect_chance", $"{move.EffectChance}");
				}

			throw new NoEnglishNameException(move);
		}



		// IParsableModule implementations
		public async Task<NamedApiObject> ParseAsync(string parse) => await ParseMoveAsync(parse);
		public Embed GetData(NamedApiObject apiObject) => MoveData(apiObject as Move);
	}
}
