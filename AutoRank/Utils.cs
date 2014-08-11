using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace AutoRank
{
	public static class Utils
	{
		/// <summary>
		/// AutoRank Extension: Gets the player's rank.
		/// </summary>
		/// <param name="ply"></param>
		/// <returns></returns>
		public static Rank FindRank(this TSPlayer ply)
		{
			return AutoRank.Config.Ranks.Find((rank) => rank.Group() == ply.Group);
		}

		#region Format
		private static string Format = @"Formatting wildcards to be used with the config custom strings.

RankUpMessage avaliable wildcards:
%NAME% - The new rank's name.
%GROUP% - The player's new Group Name (From TSHOCK)
%PARENT% - The player's new Group ParentName [I. E. the rank they previously had] (From TSHOCK)

RankCmdMsg & MaxRankMsg available wildcards:
%CUR_<branch>% - The current rank. Must use one of the following branches:
					-> %CUR_INDEX% - The index in the rank tree. [Natural-based: starts at 1]
					-> %CUR_NAME% - The name attribute.
					-> %CUR_GROUP% - The TShock group's name.
					-> %CUR_PARENT% - The parentgroup attribute.
					-> %CUR_COST% - The cost attribute parsed as SEconomy Money.ToLongString(true).
%NEXT_<branch>% - The next rank. Must use one of the following branches:
					-> %NEXT_INDEX% - The index in the rank tree. [Natural-based: starts at 1]
					-> %NEXT_NAME% - The name attribute.
					-> %NEXT_GROUP% - The TShock group's name.
					[No need for %NEXT_PARENT% since that's pretty much '%CUR_NAME%']
					-> %NEXT_COST% - The cost attribute parsed as SEconomy Money.ToLongString(true).
%MAX% - The number of ranks in the rank tree.
%CURLEFT% - The currency left to reach the next rank (NextRank's cost - Player Bank Account's balance)
%BALANCE% - The player's balance parsed as SEconomy Money.ToLongString(true).

levelupcommands available wildcards:
%PLAYER_<obj>% - Returns information regarding the player who just ranked up. Use one of the following as <obj>:
					-> NAME
					-> INDEX
					-> IP
					-> GROUP
					-> RANK

If you have any wildcard that you'd like to see added, please submit your request as feedback in the OP.";
		#endregion

		public static List<Rank> MakeRankTree(Rank rank)
		{
			try
			{
				var list = new List<Rank>();
				var ranks = AutoRank.Config.Ranks;

				for (int i = rank.GetIndex(ranks); i > 0; i--)
				{
					if (ranks[i].Group() == rank.ParentGroup())
					{
						rank = ranks[i];
					}
				}

				for (int i = 0; i < ranks.Count; i++)
				{
					list.Add(rank);
					rank = rank.FindNext();
					if (rank == null)
						break;
				}

				return list;
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Log.ConsoleError(
					"[AutoRank] ArgumentOutOfRangeException at 'MakeRankTree': {0}\nCheck logs for details.",
					ex.Message);
				Log.Error(ex.ToString());
				return null;
			}
		}

		public static bool IsLastRankInLine(Rank rank, List<Rank> line)
		{
			if (rank == line.Last())
				return true;
			else
				return false;
		}

		public static List<string> ParseParameters(string cmd)
		{
			var args = new List<string>();
			args.AddRange(cmd.Split(' '));
			return args;
		}

		public static void WriteFiles()
		{
			// Screw this, swallowing exceptions
			try
			{
				File.WriteAllText(Path.Combine(TShock.SavePath, "AutoRank", "Formatting.txt"), Format);
			}
			catch (Exception) { }
		}
	}
}
