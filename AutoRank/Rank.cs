﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Newtonsoft.Json;
//using TShockAPI.DB;

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
			return list.FindIndex(r => r == this);
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
			return Config.config.Ranks.FirstOrDefault(r => r.ParentGroup() == this.Group());
		}

		public List<Rank> FindNextRanks(Money value)
		{
			var list = new List<Rank>();
			long stack = 0L;
			var rank = this;
			while (stack < value)
			{
				rank = rank.FindNext();
				if (rank == null)
					return list;
				stack += rank.Cost();
				list.Add(rank);
			}
			list.RemoveAt(list.Count - 1);
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
			foreach (string str in levelupcommands)
			{
				var args = Utils.ParseParameters(str);
				args[0] = args[0].Remove(0, 1);
				var cmd = Commands.ChatCommands.FirstOrDefault(_ => _.HasAlias(args[0]));
				string cmdText = string.Join(" ", args);
				args.RemoveAt(0);
				if (cmd != null)
				{
					cmd.RunWithoutPermissions(cmdText, ply, args);
				}
			}
		}
	}
}
