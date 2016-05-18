using AutoRank.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace AutoRank
{
	[ApiVersion(1, 22)]
	public class AutoRank : TerrariaPlugin
	{
		public static Config Config { get; set; }

		public override string Author
		{
			get { return "Enerdy"; }
		}

		public override string Description
		{
			get { return "Auto-ranking system for Wolfje's SEconomy plugin."; }
		}

		public override string Name
		{
			get { return "AutoRank"; }
		}

		public override Version Version
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SEconomyPlugin.SEconomyLoaded -= SEconomyLoaded;
				SEconomyPlugin.SEconomyUnloaded -= SEconomyUnloaded;
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
			}
		}

		public override void Initialize()
		{
			if (SEconomyPlugin.Instance != null)
			{
				SEconomyPlugin.SEconomyLoaded += SEconomyLoaded;
				SEconomyPlugin.SEconomyUnloaded += SEconomyUnloaded;

				// Initial hooking, as SEconomyLoaded has already been called
				SEconomyPlugin.Instance.RunningJournal.BankTransferCompleted += BankTransferCompleted;
			}
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
		}

		public AutoRank(Main game)
			: base(game)
		{
			Order = 20001;
		}

		void OnInitialize(EventArgs e)
		{
			#region Config
			Config = Config.Read();
			Utils.WriteFiles();
			#endregion
			#region Commands
			Commands.ChatCommands.Add(new Command(RankCheck, Config.RankCmdAlias));
			Commands.ChatCommands.Add(new Command("autorank.reload", Reload, "rank-reload"));
			#endregion
		}

		async void BankTransferCompleted(object sender, BankTransferEventArgs e)
		{
			// Null check the instance - Thanks Wolfje
			if (SEconomyPlugin.Instance == null)
				return;

			// The world account does not have a rank
			if (e.ReceiverAccount == null ||
				!e.ReceiverAccount.IsAccountEnabled ||
				e.ReceiverAccount.IsSystemAccount ||
				SEconomyPlugin.Instance.WorldAccount == null)
				return;

			// Stop chain transfers
			if (e.TransferOptions.HasFlag(BankAccountTransferOptions.SuppressDefaultAnnounceMessages))
				return;

			TSPlayer ply = TShock.Players.FirstOrDefault(p => p != null && p.Active && p.IsLoggedIn &&
				p.User.Name == e.ReceiverAccount.UserAccountName);
			if (ply == null)
				return;

			var rank = ply.GetRank();
			if (rank != null)
			{
				var ranks = rank.FindNextRanks(e.ReceiverAccount.Balance);
				if (ranks != null && ranks.Count > 0)
				{
					//Money cost = 0L;
					//foreach (Rank rk in ranks)
					//{
					//	cost += rk.Cost();
					//}

					//Money balance = e.ReceiverAccount.Balance;
					//var task = await e.ReceiverAccount.TransferToAsync(SEconomyPlugin.Instance.WorldAccount, cost,
					//	BankAccountTransferOptions.SuppressDefaultAnnounceMessages, "",
					//	String.Format("{0} paid {1} to rank up with AutoRank.", ply.Name, cost.ToString()));
					//if (!task.TransferSucceeded)
					//{
					//	if (task.Exception != null)
					//	{
					//		Log.ConsoleError("SEconomy Exception: {0}\nCheck logs for details.", task.Exception.Message);
					//		Log.Error(task.Exception.ToString());
					//	}

					//	// Returning the money; This transaction may fail, but I see no other way.
					//	await SEconomyPlugin.Instance.WorldAccount.TransferToAsync(e.ReceiverAccount,
					//		balance - e.ReceiverAccount.Balance, BankAccountTransferOptions.SuppressDefaultAnnounceMessages,
					//		"", "");
					//	ply.SendErrorMessage(
					//		"Your transaction could not be completed. Start a new transaction to retry.");
					//}
					//else

					await ply.RankUpAsync(ranks);

					//Task.Factory.StartNew(await args.ReceiverAccount.TransferToAsync(SEconomyPlugin.Instance.WorldAccount, cost,
					//	BankAccountTransferOptions.None, String.Empty,
					//	String.Format("{0} paid {1} to rank up with AutoRank.", ply.Name,
					//	cost.ToString())) }); .ContinueWith((task) =>
					//		{
					//			if (!task.Result.TransferSucceeded)
					//			{
					//				ply.SendErrorMessage(
					//					"Your transaction could not be completed. Start a new transaction to retry.");
					//				return;
					//			}
					//			ply.RankUpAsync(ranks);
					//		});
				}
			}
		}

		async void RankCheck(CommandArgs args)
		{
			if (SEconomyPlugin.Instance == null)
				return;

			Rank rank = args.Player.GetRank();
			if (rank == null)
			{
				args.Player.SendInfoMessage("You are currently not assigned to a rank line.");
				return;
			}

			var ranktree = Utils.MakeRankTree(rank);
			if (ranktree != null)
			{
				var tuple = MsgParser.ParseRankTree(
					ranktree,
					rank.GetIndex(ranktree),
					SEconomyPlugin.Instance.GetBankAccount(args.Player));

				if (tuple != null)
				{
					if (!Utils.IsLastRankInLine(rank, ranktree) && tuple.Item2 < 0)
					{
						var ranks = rank.FindNextRanks(
							SEconomyPlugin.Instance.GetBankAccount(args.Player).Balance);

						args.Player.SendWarningMessage("Fixing your rank...");
						await args.Player.RankUpAsync(ranks);
					}
					else
					{
						args.Player.SendInfoMessage(tuple.Item1);
					}
					return;
				}
			}
			args.Player.SendErrorMessage("Failed to send your rank information.");
		}

		void Reload(CommandArgs args)
		{
			Config = Config.Read();
			args.Player.SendSuccessMessage("[AutoRank] Reloaded config!");
		}

		void SEconomyLoaded(object sender, EventArgs e)
		{
			if (SEconomyPlugin.Instance != null)
				SEconomyPlugin.Instance.RunningJournal.BankTransferCompleted += BankTransferCompleted;
		}

		void SEconomyUnloaded(object sender, EventArgs e)
		{
			if (SEconomyPlugin.Instance != null)
				SEconomyPlugin.Instance.RunningJournal.BankTransferCompleted -= BankTransferCompleted;
		}
	}
}
