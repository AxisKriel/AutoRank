using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Newtonsoft.Json;

namespace AutoRank
{
	public class Rank
	{
		public string name { get; set; }
		public string parentgroup { get; set; }
		public string group { get; set; }
		public string cost { get; set; }
		public string[] levelupcommands { get; set; }

		public int GetIndex(List<Rank> list)
		{
			return list.IndexOf(this);
		}

		public Rank(string Name)
		{
			this.name = Name;
		}

		public Group Group()
		{
			return TShock.Groups.GetGroupByName(group);
		}

		public Group ParentGroup()
		{
			return TShock.Groups.GetGroupByName(parentgroup);
		}

		public Money Cost()
		{
			return Money.Parse(cost);
		}

		public Rank FindNext()
		{
			return AutoRank.Config.Ranks.Find(r => r.ParentGroup() == this.Group());
		}

		public List<Rank> FindNextRanks(Money value)
		{
			var list = new List<Rank>();
			long stack = 0L;
			var rank = this;
			Rank nextRank;
			while (true)
			{
				nextRank = rank.FindNext();
				if (nextRank == null || nextRank.Cost() == 0L)
					break;
				stack += nextRank.Cost();
				if (stack > value)
					break;
				list.Add(rank);
			}
			return list;
		}

		public bool GroupExists()
		{
			if (this.Group() == null)
				return false;
			else
				return true;
		}

		public void PerformCommands(TSPlayer ply)
		{
			// Null check <.<
			if (levelupcommands == null || ply == null)
				return;

			string text;
			List<string> args;
			Command cmd;
			string cmdText;
			foreach (string str in levelupcommands)
			{
				text = MsgParser.ParseCommand(str, ply);
				args = Utils.ParseParameters(text);
				args[0] = args[0].Substring(1);
				cmd = Commands.ChatCommands.Find((command) => command.HasAlias(args[0]));
				cmdText = string.Join(" ", args);
				args.RemoveAt(0);
				if (cmd != null)
				{
					cmd.RunWithoutPermissions(cmdText, ply, args);
				}
			}
		}
	}
}
