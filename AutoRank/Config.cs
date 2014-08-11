using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Newtonsoft.Json;

namespace AutoRank
{
	public class Config
	{
		private static string savepath = Path.Combine(TShock.SavePath, "AutoRank");
		private static string filepath = Path.Combine(savepath, "AutoRank.json");

		public string RankUpMessage = "[AutoRank] You've been auto-ranked to %GROUP%!";
		public string RankCmdAlias = "rank";
		public string RankCmdMsg = "[%CUR_INDEX%/%MAX%] Current Rank: %CUR_NAME%. Next rank in %CURLEFT%.";
		public string MaxRankMsg = "[%CUR_INDEX%/%MAX%] Current Rank: %CUR_NAME%.";
		public List<Rank> Ranks = new List<Rank>()
		{
			new Rank("TestRank")
			{
				parentgroup = "Parent",
				group = "Group",
				cost = "100"
			}
		};

		private static void Write(Config file)
		{
			try
			{
				File.WriteAllText(filepath, JsonConvert.SerializeObject(file, Formatting.Indented));
			}
			catch (Exception ex)
			{
				Log.ConsoleError("[AutoRank] Exception at 'Config.Write': {0}\nCheck logs for details.",
					ex.Message);
				Log.Error(ex.ToString());
			}
		}

		public static Config Read()
		{
			if (!Directory.Exists(savepath))
				Directory.CreateDirectory(savepath);

			Config file = new Config();
			try
			{
				if (!File.Exists(filepath))
				{
					Write(file);
				}
				else
				{
					file = JsonConvert.DeserializeObject<Config>(filepath);
				}
			}
			catch (Exception ex)
			{
				Log.ConsoleError("[AutoRank] Exception at 'Config.Read': {0}\nCheck logs for details.",
					ex.Message);
				Log.Error(ex.ToString());
			}
			return file;
		}
	}
}
