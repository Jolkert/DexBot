using Newtonsoft.Json;
using System.IO;

namespace DexBot
{
	static class Config
	{
		private const string ResourcesFile = "Resources";
		private const string ConfigFile = "config.json";

		public static BotConfig bot;

		static Config()
		{
			if (!Directory.Exists(ResourcesFile))
				Directory.CreateDirectory(ResourcesFile);
			if (!File.Exists(ResourcesFile + "/" + ConfigFile))
			{
				bot = new BotConfig();
				string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
				File.WriteAllText(ResourcesFile + "/" + ConfigFile, json);
			}
			else
			{
				string json = File.ReadAllText(ResourcesFile + "/" + ConfigFile);
				bot = JsonConvert.DeserializeObject<BotConfig>(json);
			}
		}

		public static void ChangePrefix(string newPrefix)
		{
			bot.cmdPrefix = newPrefix;

			string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
			File.WriteAllText(ResourcesFile + "/" + ConfigFile, json);
		}
	}

	public struct BotConfig
	{
		public string token;
		public string cmdPrefix;
	}
}
