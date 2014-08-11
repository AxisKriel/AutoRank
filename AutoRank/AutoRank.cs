using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using System.Timers;

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

		async void BankTransferCompleted(object sender, Wolfje.Plugins.SEconomy.Journal.BankTransferEventArgs args)
		{
			try
			{
				// Null check the instance - Thanks Wolfje
				if (SEconomyPlugin.Instance == null)
					return;

				// The world account does not have a rank
				if (args.ReceiverAccount.IsSystemAccount || args.ReceiverAccount == null)
					return;

				TSPlayer ply = TShock.Players.FirstOrDefault(p => p != null && p.IsLoggedIn &&
					p.UserAccountName.Equals(args.ReceiverAccount.UserAccountName, StringComparison.OrdinalIgnoreCase));
				if (ply == null)
					return;

				var rank = ply.FindRank();
				if (rank != null)
				{
					var ranks = rank.FindNextRanks(args.ReceiverAccount.Balance);
					if (ranks.Count > 0)
					{
						var user = TShock.Users.GetUserByID(ply.UserID);
						if (user == null)
							return;

						Money cost = 0L;
						foreach (Rank rk in ranks)
						{
							cost += rk.Cost();
						}
						await args.ReceiverAccount.TransferToAsync(SEconomyPlugin.Instance.WorldAccount, cost,
							Wolfje.Plugins.SEconomy.Journal.BankAccountTransferOptions.None,
							null, String.Format("{0} paid {1} to rank up with AutoRank.", ply.Name,
							cost.ToString())).ContinueWith((task) =>
								{
									if (!task.Result.TransferSucceeded)
									{
										ply.SendErrorMessage(
											"Your transaction could not be completed. Start a new transaction to retry.");
										return;
									}

									foreach (Rank rk in ranks)
										rk.PerformCommands(ply);

									var lastrank = ranks.LastOrDefault();
									if (lastrank == null || !lastrank.GroupExists())
									{
										Log.ConsoleError(Error.Group(lastrank == null ? "NULL" : lastrank.group));
										return;
									}
									TShock.Users.SetUserGroup(user, lastrank.Group().Name);
									ply.SendSuccessMessage(MsgParser.Parse(Config.RankUpMessage, ply, lastrank));
								});
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

		void RankCheck(CommandArgs args)
		{
			Rank rank = args.Player.FindRank();
			if (rank == null)
			{
				args.Player.SendInfoMessage("You are currently not assigned to a rank line.");
				return;
			}

			var ranktree = Utils.MakeRankTree(rank);
			if (ranktree != null)
			{
				var str = MsgParser.ParseRankTree(
					ranktree,
					rank.GetIndex(ranktree),
					SEconomyPlugin.Instance.GetBankAccount(args.Player));

				if (str != null)
				{
					args.Player.SendInfoMessage(str);
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
