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
			return Config.config.Ranks.FirstOrDefault(r => r.Group() == ply.Group);
		}

		public static List<Rank> MakeRankTree(Rank rank)
		{
			if (rank.RankLine == null)
				return null;

			return new List<Rank>(Config.config.Ranks.Where(r => r.RankLine == rank.RankLine));
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
