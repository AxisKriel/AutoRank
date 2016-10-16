using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;

namespace AutoRank
{
	public class Rank
	{
		public string name { get; set; }
		public string parentgroup { get; set; }
		public string group { get; set; }
		public string cost { get; set; }
		public string message { get; set; }
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
			return cost == null ? new Money() : Money.Parse(cost);
		}

		public Rank FindNext()
		{
			return AutoRank.Config.Ranks.Find(r => r.ParentGroup() == this.Group());
		}

		public List<Rank> FindNextRanks(Money value)
		{
			// Value can't be negative or zero
			if (value <= 0)
				return null;

			var ranks = Utils.MakeRankTree(this);

			Money cost = 0;
			var list = new List<Rank>();
			for (int i = this.GetIndex(ranks) + 1; i < ranks.Count; i++)
			{
				cost += ranks[i].Cost();
				if (cost > value)
					break;
				list.Add(ranks[i]);
			}
			return list;
		}

		public void PerformCommands(TSPlayer ply)
		{
			// Null check <.<
			if (levelupcommands == null || ply == null)
				return;

			string text;
			List<string> args;
			Command cmd;
			foreach (string str in levelupcommands)
			{
				text = MsgParser.ParseCommand(str, ply);
				args = Utils.ParseParameters(text);
				args[0] = args[0].Substring(1);
				cmd = Commands.ChatCommands.Find((command) => command.HasAlias(args[0]));
				args.RemoveAt(0);
				if (cmd != null)
				{
					cmd.RunWithoutPermissions(text, ply, args);
				}
			}
		}
	}
}
