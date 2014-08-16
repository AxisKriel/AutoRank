using AutoRank.Extensions;
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
		public static string Parse(string msg, TSPlayer ply, Rank rank)
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

		public static Tuple<string, Money> Parse(string msg, List<Rank> tree, Rank rank, IBankAccount account)
		{
			if (account == null)
				return null;

			Money curleft = rank.FindNext().Cost() - account.Balance;
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
				{"%CURLEFT%", curleft.ToLongString(true)},
				{"%BALANCE%", account.Balance.ToLongString(true)}
				
			};

			string parsed = msg;

			foreach (var wc in parsers)
			{
				try
				{
					if (parsed.Contains(wc.Key))
						parsed = parsed.Replace(wc.Key, wc.Value.ToString());
				}
				catch (Exception ex)
				{
					Log.ConsoleError(
						"[AutoRank] Exception at 'MsgParser.Parse2': {0}\nCheck logs for details.",
						ex.Message);
					Log.Error(ex.ToString());
				}
			}

			return Tuple.Create<string, Money>(parsed, curleft);
		}

		public static Tuple<string, Money> ParseRankTree(List<Rank> tree, int index, IBankAccount account)
		{
			// Should no longer return exceptions with this check
			if (index < 0 || index >= tree.Count)
				return null;

			Rank rank = tree[index];
			return Parse((Utils.IsLastRankInLine(rank, tree) ? AutoRank.Config.MaxRankMsg :
				AutoRank.Config.RankCmdMsg), tree, rank, account);
		}

		public static string ParseCommand(string cmd, TSPlayer plr)
		{
			var replacements = new Dictionary<string, object>()
			{
				{ "NAME", plr.Name },
				{ "INDEX", plr.Index },
				{ "IP", plr.IP },
				{ "GROUP", plr.Group.Name },
				{ "RANK", plr.GetRank() == null ? "None" : plr.GetRank().name }
			};

			string parsed = cmd;
			foreach (var word in replacements)
			{
				parsed = parsed.Replace(String.Format("%PLAYER_{0}%", word.Key), word.Value.ToString());
			}
			return parsed;
		}
	}
}
