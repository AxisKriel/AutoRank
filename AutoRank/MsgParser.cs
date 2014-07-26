using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

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

		public static string Parse(string msg, List<Rank> tree, Rank rank, IBankAccount account)
		{
			if (account == null)
				return null;

			var parsers = new Dictionary<string, object>()
			{
				{"%CUR_INDEX%", (rank.GetIndex(tree) + 1)},
				{"%CUR_NAME%", rank.name},
				{"%CUR_GROUP%", rank.group},
				{"%CUR_PARENT%", rank.parentgroup},
				{"%CUR_COST%", rank.Cost().ToLongString()},
				{"%MAX%", tree.Count.ToString()},
				{"%NEXT_INDEX%", (rank.FindNext().GetIndex(tree) + 1)},
				{"%NEXT_NAME%", rank.FindNext().name},
				{"%NEXT_GROUP%", rank.FindNext().group},
				{"%NEXT_COST%", rank.FindNext().Cost().ToLongString()},
				{"%CURLEFT%", new Money(rank.FindNext().Cost() - account.Balance).ToLongString(true)},
				{"%BALANCE%", account.Balance.ToLongString(true)}
				
			};

			string parsed = msg;

			foreach (var wc in parsers)
			{
				if (parsed.Contains(wc.Key))
					parsed = parsed.Replace(wc.Key, wc.Value.ToString());
			}

			return parsed;
		}

		public static string ParseRankTree(List<Rank> tree, int index, IBankAccount account)
		{
			try
			{
				Rank rank = tree[index];

				return Parse((Utils.IsLastRankInLine(rank, tree) ? Config.config.MaxRankMsg :
					Config.config.RankCmdMsg), tree, rank, account);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Log.ConsoleError(ex.ToString());
				return null;
			}
		}

		public static string ParseCommand(string cmd, TSPlayer plr)
		{
			var plrprs = new Dictionary<string, object>()
			{
				{"NAME", plr.Name},
				{"INDEX", plr.Index.ToString()},
				{"IP", plr.IP},
				{"GROUP", plr.Group.Name},
				{"RANK", plr.FindRank() == null ? "None" : plr.FindRank().name}
			};

			string parsed = cmd;
			string plrparser;

			foreach (var wc in plrprs)
			{
				plrparser = string.Format("%PLAYER_{0}%", wc.Key);
				parsed = parsed.Replace(plrparser, wc.Value.ToString());
			}
			return parsed;
		}
	}
}
