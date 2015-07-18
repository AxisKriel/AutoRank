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
		#region Format
		private static string Format = @"Formatting wildcards to be used with the config custom strings.

RankUpMessage avaliable wildcards:
%NAME% - The new rank's name.
%GROUP% - The player's new Group Name (From TSHOCK)
%PARENT% - The player's new Group ParentName [I. E. the rank they previously had] (From TSHOCK)

RankCmdMsg & MaxRankMsg available wildcards:
The tokens below refer to the player's current rank.
					-> $rankindex - The index in the rank tree. [Natural-based: starts at 1]
					-> $rankname - The name attribute.
					-> $rankgroup - The TShock group's name.
					-> $rankparent - The parentgroup attribute.
					-> $rankcost - The cost attribute parsed as SEconomy Money.ToLongString(false).
The tokens below refer to the player's next rank in the tree.
					-> $nextindex - The index in the rank tree. [Natural-based: starts at 1]
					-> $nextname - The name attribute.
					-> $nextgroup - The TShock group's name.
					[No need for '$nextparent' since that's pretty much '$rankname']
					-> $nextcost - The cost attribute parsed as SEconomy Money.ToLongString(false).
$rankcount - The number of ranks in the rank tree.
$remainder - The currency left to reach the next rank (NextRank's cost - Player Bank Account's balance)
$balance - The player's balance parsed as SEconomy Money.ToLongString(true).

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
				TShock.Log.ConsoleError(
						"[AutoRank] ArgumentOutOfRangeException at 'MakeRankTree': {0}\nCheck logs for details.",
						ex.Message);
				TShock.Log.Error(ex.ToString());
				return null;
			}
		}

		public static bool IsLastRankInLine(Rank rank, List<Rank> line)
		{
			return rank.FindNext() == null;
		}

		public static List<string> ParseParameters(string text)
		{
			text = text.Trim();
			var args = new List<string>();
			StringBuilder sb = new StringBuilder();
			bool quote = false;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				//if (c == '\\' && i++ < text.Length)
				//{
				//	char cc = text[i];
				//	if (cc != '"' && cc != '\\')
				//	{
				//		sb.Append(c);
				//	}
				//	sb.Append(cc);
				//}
				if (Char.IsWhiteSpace(c) && !quote)
				{
					args.Add(sb.ToString());
					sb.Clear();
				}
				else if (c == '"')
				{
					quote = !quote;
				}
				else
				{
					sb.Append(c);
				}
			}
			args.Add(sb.ToString());
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
