using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace AutoRank
{
	public static class Utils
	{
		/// <summary>
		/// AutoRank Extension: Gets the player's rank.
		/// </summary>
		/// <param name="ply"></param>
		/// <returns></returns>
		public static Rank FindRank(this TSPlayer ply)
		{
			return Config.config.Ranks.FirstOrDefault(r => r.Group == ply.Group);
		}

		//public static List<Rank> MakeRankTree(Rank rank)
		//{
		//	if (rank.RankLine == null)
		//		return null;

		//	return new List<Rank>(Config.config.Ranks.Where(r => r.RankLine == rank.RankLine));
		//}

		public static List<Rank> MakeRankTree2(Rank rank)
		{
			var list = new List<Rank>();
			var ranks = Config.config.Ranks;

			for (int i = rank.GetIndex(Config.config.Ranks); i > 0; i--)
			{
				if (ranks[i].Group == rank.ParentGroup)
				{
					rank = ranks[i];
				}
			}

			for (int i = 0; i < Config.config.Ranks.Count; i++)
			{
				list.Add(rank);
				rank = rank.FindNext();
				if (rank == null)
					break;
			}

			return list;
		}

		public static bool IsLastRankInLine(Rank rank, List<Rank> line)
		{
			if (line.FindIndex(r => r.Equals(rank)) == line.Count - 1)
				return true;
			else
				return false;
		}
	}
}
