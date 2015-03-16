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

		private static void RankUp(TSPlayer player, Rank rank, bool silent = false)
		{
			bool debug = true;

			try
			{
				if (player == null)
					throw new NullReferenceException("TSPlayer object cannot be null.");
				if (rank == null)
					throw new NullReferenceException("Rank object cannot be null.");
				if (rank.Group() == null)
					throw new NullReferenceException("Group object cannot be null.");

				rank.PerformCommands(player);
				if (!silent)
				{
					TShock.Users.SetUserGroup(TShock.Users.GetUserByID(player.UserID), rank.Group().Name);
					player.SendSuccessMessage(MsgParser.Parse(AutoRank.Config.RankUpMessage, player, rank));
				}

				if (debug)
					TShock.Log.ConsoleInfo("[AutoRank] Ranked '{0}' to '{1}' (silent: {2})".SFormat(player.Name, rank.name, silent));
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError("[SBPlanet Package] Exception at 'RankUp': {0}\nCheck logs for details.",
						ex.Message);
				TShock.Log.Error(ex.ToString());
			}
		}

		public static Task RankUpAsync(this TSPlayer player, Rank rank)
		{
			return Task.Run(() => RankUp(player, rank));
		}

		public static async Task RankUpAsync(this TSPlayer player, List<Rank> line)
		{
			if (line.Count < 1)
				return;

			Money cost = 0;
			for (int i = 0; i < line.Count; i++)
			{
				cost += line[i].Cost();
			}

			if (SEconomyPlugin.Instance == null)
				return;

			IBankAccount account = SEconomyPlugin.Instance.GetBankAccount(player);
			if (account != null && SEconomyPlugin.Instance.WorldAccount != null)
			{
				Money balance = account.Balance;
				var task = await account.TransferToAsync(SEconomyPlugin.Instance.WorldAccount, cost,
					BankAccountTransferOptions.SuppressDefaultAnnounceMessages, "",
					"{0} paid {1} to rank up with AutoRank.".SFormat(player.UserAccountName, cost));

				if (!task.TransferSucceeded)
				{
					// Returning the money; This transaction may fail, but I see no other way.
					await SEconomyPlugin.Instance.WorldAccount.TransferToAsync(account,
						balance - account.Balance, BankAccountTransferOptions.SuppressDefaultAnnounceMessages,
						"", "");
					player.SendErrorMessage(
						"Your transaction could not be completed. Start a new transaction to retry.");
					return;
				}
			}
			else
				player.SendErrorMessage("Invalid bank account!");

			for (int i = 0; i < line.Count; i++)
			{
				if (i == line.Count - 1)
					RankUp(player, line[i], false);
				else
					RankUp(player, line[i], true);
			}
		}
	}
}
