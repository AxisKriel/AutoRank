using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace AutoRank.Extensions
{
	public static class TSPlayerExtensions
	{
		/// <summary>
		/// Gets the player's rank.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static Rank GetRank(this TSPlayer player)
		{
			return AutoRank.Config.Ranks.Find((rank) => rank.Group() == player.Group);
		}

		private static void RankUp(TSPlayer player, Rank rank)
		{
			try
			{
				if (player == null)
					throw new NullReferenceException("TSPlayer object cannot be null.");
				if (rank == null)
					throw new NullReferenceException("Rank object cannot be null.");

				rank.PerformCommands(player);
				TShock.Users.SetUserGroup(TShock.Users.GetUserByID(player.UserID), rank.Group().Name);
				player.SendSuccessMessage(MsgParser.Parse(AutoRank.Config.RankUpMessage, player, rank));
			}
			catch (Exception ex)
			{
				Log.ConsoleError("[SBPlanet Package] Exception at 'RankUp': {0}\nCheck logs for details.",
					ex.Message);
				Log.Error(ex.ToString());
			}
		}

		public static Task RankUpAsync(this TSPlayer player, Rank rank)
		{
			return Task.Factory.StartNew(() => RankUp(player, rank));
		}

		public static Task RankUpAsync(this TSPlayer player, List<Rank> line)
		{
			return Task.Factory.StartNew(() =>
				{
					if (line.Count < 1)
						return;

					foreach (Rank rank in new List<Rank>(line.Where(r => r != line.Last())))
					{
						rank.PerformCommands(player);
					}
					RankUp(player, line.Last());
				});
		}
	}
}
