using AutoRank.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace AutoRank
{
	[ApiVersion(1, 16)]
    public class AutoRank : TerrariaPlugin
    {
		public static Config Config { get; set; }

		public override Version Version
		{
			get
			{
				return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			}
		}
		public override string Name
		{
			get
			{
				return "AutoRank";
			}
		}

		public override string Author
		{
			get
			{
				return "Enerdy";
			}
		}

		public override string Description
		{
			get
			{
				return "Auto-ranking system for Wolfje's SEconomy plugin.";
			}
		}

		public override void Initialize()
		{
			Config = Config.Read();
			Utils.WriteFiles();

			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);

			Commands.ChatCommands.Add(new Command(RankCheck, Config.RankCmdAlias));
			Commands.ChatCommands.Add(new Command("autorank.reload", Reload, "rank-reload"));
			
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				if (SEconomyPlugin.Instance != null)
					SEconomyPlugin.Instance.RunningJournal.BankTransferCompleted -= BankTransferCompleted;
			}
			base.Dispose(disposing);
		}

		public AutoRank(Terraria.Main game)
			: base(game)
		{
			Order = 20001;
		}

		void OnInitialize(EventArgs e)
		{
			if (SEconomyPlugin.Instance != null)
				SEconomyPlugin.Instance.RunningJournal.BankTransferCompleted += BankTransferCompleted;
		}

		async void BankTransferCompleted(object sender, BankTransferEventArgs args)
		{
			try
			{
				// Null check the instance - Thanks Wolfje
				if (SEconomyPlugin.Instance == null)
					return;

				// The world account does not have a rank
				if (args.ReceiverAccount == null ||
					!args.ReceiverAccount.IsAccountEnabled ||
					args.ReceiverAccount.IsSystemAccount ||
					SEconomyPlugin.Instance.WorldAccount == null)
					return;

				TSPlayer ply = TShock.Players.FirstOrDefault(p => p != null && p.IsLoggedIn &&
					p.UserAccountName.Equals(args.ReceiverAccount.UserAccountName, StringComparison.OrdinalIgnoreCase));
				if (ply == null)
					return;

				var rank = ply.GetRank();
				if (rank != null)
				{
					var ranks = rank.FindNextRanks(args.ReceiverAccount.Balance);
					if (ranks.Count > 0)
					{
						Money cost = 0L;
						foreach (Rank rk in ranks)
						{
							cost += rk.Cost();
						}
						var task = await args.ReceiverAccount.TransferToAsync(SEconomyPlugin.Instance.WorldAccount, cost,
							BankAccountTransferOptions.SuppressDefaultAnnounceMessages, String.Empty,
							String.Format("{0} paid {1} to rank up with AutoRank.", ply.Name, cost.ToString()));
						if (!task.TransferSucceeded)
						{
							if (task.Exception != null)
							{
								Log.ConsoleError("SEconomy Exception: " + task.Exception.Message);
								Log.ConsoleError(task.Exception.ToString());
							}
							ply.SendErrorMessage(
								"Your transaction could not be completed. Start a new transaction to retry.");
							return;
						}
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
			catch (Exception ex)
			{
				Log.ConsoleError("[AutoRank] Exception at 'BankTransferCompleted': {0}\nCheck logs for details.",
					ex.Message);
				Log.Error(ex.ToString());
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
	}
}
