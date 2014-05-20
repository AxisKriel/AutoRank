using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
//using TShockAPI.DB;

namespace AutoRank
{
	public class Rank
	{
		public string name { get; set; }
		public string parentgroup { get; set; }
		public string group { get; set; }
		public string cost { get; set; }
		public string RankLine { get; set; }

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

		public bool IsGroup()
		{
			if (this.Group() == null)
				return false;
			else
				return true;
		}
	}
}
