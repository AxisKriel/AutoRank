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
		public static Cfg config;
		public class Cfg
		{
			//public int UpdateInterval = 1000;
			public string RankUpMessage = "[AutoRank] You've been auto-ranked to %GROUP%!";
			public string RankCmdAlias = "rank";
			public string RankCmdMsg = "[%CUR_INDEX%/%MAX%] Current Rank: %CUR_NAME%. Next rank in %NEXT_COST%";
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
		}

		public static void CreateConfig()
		{
			try
			{
				using (var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Write))
				{
					using (var sr = new StreamWriter(stream))
					{
						config = new Cfg();
						var obj = JsonConvert.SerializeObject(config, Formatting.Indented);
						sr.Write(obj);
					}
				}
				Log.ConsoleInfo("Created AutoRank.json.");
			}
			catch (Exception ex)
			{
				Log.ConsoleError(ex.Message);
				config = new Cfg();
			}
		}

		public static bool ReadConfig()
		{
			if (!Directory.Exists(savepath))
				Directory.CreateDirectory(savepath);

			if (!File.Exists(filepath))
			{
				try
				{
					CreateConfig();
				}
				catch
				{
					return false;
				}
				return true;
			}

			try
			{
				using (var sr = new StreamReader(filepath))
				{
					config = JsonConvert.DeserializeObject<Cfg>(sr.ReadToEnd());
				}
			}
			catch (Exception ex)
			{
				Log.ConsoleError(ex.Message);
				return false;
			}
			return true;
		}
	}
}
