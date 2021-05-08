using Newtonsoft.Json;
using System.IO;

namespace DexBot
{
	static class Config
	{
		private const string ResourcesFile = "Resources";
		private const string ConfigFile = "config.json";

		public static BotConfig Bot;

		static Config()
		{
			if (!Directory.Exists(ResourcesFile))
				Directory.CreateDirectory(ResourcesFile);
			if (!File.Exists(ResourcesFile + "/" + ConfigFile))
			{
				Bot = new BotConfig("BOT_TOKEN_GOES_HERE", "d!");
				string json = JsonConvert.SerializeObject(Bot, Formatting.Indented);
				File.WriteAllText(ResourcesFile + "/" + ConfigFile, json);
			}
			else
			{
				string json = File.ReadAllText(ResourcesFile + "/" + ConfigFile);
				Bot = JsonConvert.DeserializeObject<BotConfig>(json);
			}
		}

		public static void ChangePrefix(string newPrefix)
		{
			Bot.CommandPrefix = newPrefix;

			string json = JsonConvert.SerializeObject(Bot, Formatting.Indented);
			File.WriteAllText(ResourcesFile + "/" + ConfigFile, json);
		}
	}

	public struct BotConfig
	{
		public string Token;
		public string CommandPrefix;

		public BotConfig(string token, string prefix)
		{
			this.Token = token;
			this.CommandPrefix = prefix;
		}
	}
}
