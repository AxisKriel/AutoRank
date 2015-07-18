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
		public static string Parse(string message, TSPlayer player, Rank rank)
		{
			if (String.IsNullOrEmpty(message))
				throw new ArgumentNullException("message cannot be null or empty.");
			if (player == null)
				throw new ArgumentNullException("player cannot be null.");
			if (rank == null)
				throw new ArgumentNullException("rank cannot be null.");

			string parsed = message;

			if (parsed.Contains("%NAME%"))
			{
				parsed = parsed.Replace("%NAME%", rank.name);
			}

			if (parsed.Contains("%GROUP%"))
			{
				parsed = parsed.Replace("%GROUP%", player.Group.Name);
			}

			if (parsed.Contains("%PARENT%"))
			{
				parsed = parsed.Replace("%PARENT%", player.Group.ParentName);
			}

			return parsed;
		}

		public static Tuple<string, Money> Parse(string message, List<Rank> tree, Rank rank, IBankAccount account)
		{
			if (String.IsNullOrEmpty(message))
				throw new ArgumentNullException("message cannot be null or empty.");
			if (tree == null)
				throw new ArgumentNullException("tree cannot be null.");
			if (rank == null)
				throw new ArgumentNullException("rank cannot be null.");
			if (account == null)
				throw new ArgumentNullException("account cannot be null.");

			Money remainder = 0;
			var replacements = new Dictionary<string, object>();

			#region Old Tags
			replacements.Add("%CUR_INDEX%", rank.GetIndex(tree) + 1);
			replacements.Add("%CUR_NAME%", rank.name);
			replacements.Add("%CUR_GROUP%", rank.group);
			replacements.Add("%CUR_PARENT%", rank.parentgroup);
			replacements.Add("%CUR_COST%", rank.Cost().ToLongString());
			replacements.Add("%MAX%", tree.Count);
			if (!Utils.IsLastRankInLine(rank, tree))
			{
				replacements.Add("%NEXT_INDEX%", rank.FindNext().GetIndex(tree) + 1);
				replacements.Add("%NEXT_NAME%", rank.FindNext().name);
				replacements.Add("%NEXT_GROUP%", rank.FindNext().group);

				remainder = rank.FindNext().Cost() - account.Balance;
				replacements.Add("%CURLEFT%", remainder.ToLongString(true));
			}
			replacements.Add("%BALANCE%", account.Balance.ToLongString(true));
			#endregion

			#region New Replacement System
			replacements.Add("$rankindex", rank.GetIndex(tree) + 1);
			replacements.Add("$rankname", rank.name);
			replacements.Add("$rankgroup", rank.group);
			replacements.Add("$rankparent", rank.parentgroup);
			replacements.Add("$rankcost", rank.Cost().ToLongString());
			replacements.Add("$rankcount", tree.Count);
			if (!Utils.IsLastRankInLine(rank, tree))
			{
				replacements.Add("$nextindex", rank.FindNext().GetIndex(tree) + 1);
				replacements.Add("$nextname", rank.FindNext().name);
				replacements.Add("$nextgroup", rank.FindNext().group);

				Money cost = rank.FindNext().Cost();
				replacements.Add("$nextcost", cost.ToLongString());

				remainder = cost - account.Balance;
				replacements.Add("$remainder", remainder.ToLongString(true));
			}
			replacements.Add("$balance", account.Balance.ToLongString(true));
			#endregion

			//var parsers = new Dictionary<string, object>()
			//{
			//	{"%CUR_INDEX%", (rank.GetIndex(tree) + 1)},
			//	{"%CUR_NAME%", rank.name},
			//	{"%CUR_GROUP%", rank.group},
			//	{"%CUR_PARENT%", rank.parentgroup},
			//	{"%CUR_COST%", rank.Cost().ToLongString()},
			//	{"%MAX%", tree.Count.ToString()},
			//	{"%NEXT_INDEX%", rank.FindNext() != null ? (rank.FindNext().GetIndex(tree) + 1).ToString() : ""},
			//	{"%NEXT_NAME%", rank.FindNext().name},
			//	{"%NEXT_GROUP%", rank.FindNext().group},
			//	{"%NEXT_COST%", rank.FindNext().Cost().ToLongString()},
			//	{"%CURLEFT%", curleft.ToLongString(true)},
			//	{"%BALANCE%", account.Balance.ToLongString(true)}

			//};

			string parsed = message;

			foreach (var wc in replacements)
			{
				try
				{
					if (parsed.Contains(wc.Key))
						parsed = parsed.Replace(wc.Key, wc.Value.ToString());
				}
				catch (Exception ex)
				{
					TShock.Log.ConsoleError(
							"[AutoRank] Exception at 'MsgParser.Parse2': {0}\nCheck logs for details.",
							ex.Message);
					TShock.Log.Error(ex.ToString());
				}
			}

			return Tuple.Create<string, Money>(parsed, remainder);
		}

		public static Tuple<string, Money> ParseRankTree(List<Rank> tree, int index, IBankAccount account)
		{
			// Should no longer return exceptions with this check
			if (index < 0 || index >= tree.Count)
				return null;

			Rank rank = tree[index];
			try
			{
				return Parse((Utils.IsLastRankInLine(rank, tree) ? AutoRank.Config.MaxRankMsg :
					AutoRank.Config.RankCmdMsg), tree, rank, account);
			}
			catch (ArgumentNullException ex)
			{
				return new Tuple<string, Money>(ex.Message, new Money());
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError(ex.ToString());
				return null;
			}
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
