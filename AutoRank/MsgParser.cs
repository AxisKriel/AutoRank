using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Economy;

namespace AutoRank
{
	public class MsgParser
	{
		public static string Parse(string msg, TSPlayer ply, Rank rank = null)
		{
			string parsed = msg;

			if (parsed.Contains("%NAME%"))
			{
				parsed = parsed.Replace("%NAME%", rank.name);
			}

			if (parsed.Contains("%GROUP%"))
			{
				parsed = parsed.Replace("%GROUP%", ply.Group.Name);
			}

			if (parsed.Contains("%PARENT%"))
			{
				parsed = parsed.Replace("%PARENT%", ply.Group.ParentName);
			}

			return parsed;
		}

		public static string Parse(string msg, List<Rank> tree, Rank rank)
		{
			string parsed = msg;

			if (parsed.Contains("%CUR_INDEX%"))
				parsed = parsed.Replace("%CUR_INDEX%", (rank.GetIndex(tree) + 1).ToString());

			if (parsed.Contains("%CUR_NAME%"))
				parsed = parsed.Replace("%CUR_NAME%", rank.name);

			if (parsed.Contains("%CUR_GROUP%"))
				parsed = parsed.Replace("%CUR_GROUP%", rank.group);

			if (parsed.Contains("%CUR_PARENT%"))
				parsed = parsed.Replace("%CUR_PARENT%", rank.parentgroup);

			if (parsed.Contains("%MAX%"))
				parsed = parsed.Replace("%MAX%", tree.Count.ToString());

			if (parsed.Contains("%NEXT_INDEX%"))
				parsed = parsed.Replace("%NEXT_INDEX%", (rank.FindNext().GetIndex(tree) + 1).ToString());

			if (parsed.Contains("%NEXT_NAME%"))
				parsed = parsed.Replace("%NEXT_NAME%", rank.FindNext().name);

			if (parsed.Contains("%NEXT_GROUP%"))
				parsed = parsed.Replace("%NEXT_GROUP%", rank.FindNext().group);

			return parsed;
		}

		public static string ParseRankTree(List<Rank> tree, int index, EconomyPlayer plr)
		{
			try
			{
				Rank rank = tree[index];
				//return string.Format("[{0}/{1}] Current Rank: {2}.{3}", index + 1, tree.Count, rank.name,
				//	(Utils.IsLastRankInLine(rank, tree) ? string.Empty : string.Format(" Next rank in {0}.",
				//	new Money((rank.FindNext().Cost - plr.BankAccount.Balance)).ToLongString(true))));
				return Parse((Utils.IsLastRankInLine(rank, tree) ? Config.config.MaxRankMsg :
					Config.config.RankCmdMsg), tree, rank);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Log.ConsoleError(ex.Message);
				return ex.ToString();
			}
		}
	}
}
