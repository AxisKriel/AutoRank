using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Economy;
using System.Timers;

namespace AutoRank
{
	[ApiVersion(1, 16)]
    public class AutoRank : TerrariaPlugin
    {
		//Timer UpdateTimer;
		Config.Cfg cfg;

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
			Config.ReadConfig();
			cfg = Config.config;

			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);

			Commands.ChatCommands.Add(new Command(RankCheck, cfg.RankCmdAlias));
			Commands.ChatCommands.Add(new Command("autorank.reload", Reload, "rank-reload"));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				//UpdateTimer.Elapsed -= UpdateTimerTick;
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
			//UpdateTimer = new Timer(cfg.UpdateInterval);
			//UpdateTimer.Elapsed += UpdateTimerTick;
			//UpdateTimer.Start();
			SEconomyPlugin.RunningJournal.BankTransferCompleted += BankTransferCompleted;
		}

		void BankTransferCompleted(object sender, Wolfje.Plugins.SEconomy.Journal.BankTransferEventArgs args)
		{
			EconomyPlayer plr = args.ReceiverAccount.Owner;

			var rank = plr.TSPlayer.FindRank();
			if (rank != null && rank.FindNext() != null)
			{
				var newrank = rank.FindNext();
				if (plr.BankAccount.Balance > newrank.Cost())
				{
					var user = TShock.Users.GetUserByID(plr.TSPlayer.UserID);

					plr.BankAccount.Balance -= newrank.Cost();
					TShock.Users.SetUserGroup(user, newrank.Group().ToString());
					plr.TSPlayer.SendSuccessMessage(MsgParser.Parse(cfg.RankUpMessage, plr.TSPlayer));
				}
			}

			//foreach (EconomyPlayer plr in SEconomyPlugin.EconomyPlayers)
			//{
			//	var rank = plr.TSPlayer.FindRank();
			//	if (rank != null && rank.FindNext() != null)
			//	{
			//		var newrank = rank.FindNext();
			//		if (plr.BankAccount.Balance > newrank.Cost())
			//		{
			//			plr.BankAccount.Balance -= newrank.Cost();
			//			plr.TSPlayer.Group = newrank.Group();
			//			plr.TSPlayer.SendSuccessMessage(MsgParser.Parse(cfg.RankUpMessage, plr.TSPlayer));
			//		}
			//	}
			//}
		}

		void RankCheck(CommandArgs args)
		{
			Rank rank = args.Player.FindRank();
			if (rank == null)
			{
				args.Player.SendInfoMessage("You are currently not assigned to a rank line.");
				return;
			}

			var ranktree = Utils.MakeRankTree2(rank);
			args.Player.SendInfoMessage(MsgParser.ParseRankTree(ranktree, rank.GetIndex(ranktree),
  				SEconomyPlugin.EconomyPlayers.Find(e => e.TSPlayer.Equals(args.Player))));
		}

		void Reload(CommandArgs args)
		{
			if (Config.ReadConfig())
				args.Player.SendSuccessMessage("[AutoRank] Config reloaded successfully.");
			else
				args.Player.SendErrorMessage("[AutoRank] Failed to reload config. Check logs for details.");
		}
    }
}
