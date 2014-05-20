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

		public static string ParseRankTree(List<Rank> tree, int index, EconomyPlayer plr)
		{
			Rank rank = tree[index];
			return string.Format("[{0}/{1}] Rank: {2}.{3}", index + 1, tree.Count, rank.name,
				(Utils.IsLastRankInLine(rank, tree) ? string.Empty : string.Format(" Next rank in {0}.",
				new Money((rank.FindNext().Cost() - plr.BankAccount.Balance)).ToLongString())));
		}
	}
}
