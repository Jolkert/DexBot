using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace DexBot
{
	static class AppData
	{
		private const string ResourcesFolder = "Resources";
		private const string ConfigFile = "appdata-path.txt";
		private const string Source = "AppData";

		private static readonly string _folderPath;
		private static readonly string _typeIconsPath;
		public static readonly Dictionary<string, string> TypeIconUrls;

		static AppData()
		{
			TypeIconUrls = new Dictionary<string, string>()
			{
				{ "normal", "https://i.imgur.com/BpKNSSR.png" },
				{ "fighting", "https://i.imgur.com/O5wEC0O.png" },
				{ "flying", "https://i.imgur.com/HpwCW93.png" },
				{ "poison", "https://i.imgur.com/O7YK7bR.png" },
				{ "ground", "https://i.imgur.com/xsVFIbd.png" },
				{ "rock", "https://i.imgur.com/JvhYf8m.png" },
				{ "bug", "https://i.imgur.com/aZJaJ6r.png" },
				{ "ghost", "https://i.imgur.com/uIK1c6h.png" },
				{ "steel", "https://i.imgur.com/Y0AAbJC.png" },
				{ "fire", "https://i.imgur.com/trfErM5.png" },
				{ "water", "https://i.imgur.com/5ozzNl5.png" },
				{ "grass", "https://i.imgur.com/stZrBBW.png" },
				{ "electric", "https://i.imgur.com/il5BtuO.png" },
				{ "psychic", "https://i.imgur.com/iNKGrOv.png" },
				{ "ice", "https://i.imgur.com/E94FSH9.png" },
				{ "dragon", "https://i.imgur.com/N6MaPas.png"},
				{ "dark", "https://i.imgur.com/4PLrgh0.png" },
				{ "fairy", "https://i.imgur.com/p4Rk2d8.png" }

			};

			if (!Directory.Exists(ResourcesFolder))
				Directory.CreateDirectory(ResourcesFolder);

			if (!File.Exists(ConfigFile))
			{
				_folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\.dexbot";
				File.WriteAllText($"{ResourcesFolder}\\{ConfigFile}", _folderPath);
			}
			else
				_folderPath = File.ReadAllText($"{ResourcesFolder}\\{ConfigFile}");

			if (!Directory.Exists(_folderPath))
				Directory.CreateDirectory(_folderPath);

			_typeIconsPath = $"{_folderPath}\\TypeIcons";
			if (!Directory.Exists(_typeIconsPath) || Directory.GetFiles(_typeIconsPath).Length < 18)
				DownloadAllTypeIcons();
		}

		public static string GetTypeIconPath(string id)
		{
			string path = $"{_typeIconsPath}\\{id}.png";
			return path;
		}
		public static string GetOrDownloadTypeIconPath(string id)
		{
			string path = $"{_typeIconsPath}\\{id}.png";
			Program.LogAsync($"Attempting to get file {path}", Source);
			if (!File.Exists(path))
			{
				Program.LogAsync($"Could not find file, attempting to download...", Source);
				path = DownloadTypeIcon(id);
			}

			return path;
		}

		private static void DownloadAllTypeIcons()
		{
			DownloadTypeIcons(new string[]
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
			});
		}
		private static void DownloadTypeIcons(string[] ids)
		{
			using (WebClient client = new WebClient())
			{
				foreach (string id in ids)
					client.DownloadFile($"{TypeIconUrls[id]}", $"{_typeIconsPath}\\{id}.png");
			}
		}
		private static string DownloadTypeIcon(string id)
		{
			DownloadTypeIcons(new string[] { id });
			return $"{_typeIconsPath}\\{id}.png";
		}
	}
}
