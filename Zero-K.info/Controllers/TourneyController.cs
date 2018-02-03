﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZkData;
using ZkLobbyServer;

namespace ZeroKWeb.Controllers
{
    public class TourneyController : Controller
    {
        public class TourneyModel
        {
            public List<TourneyBattle> Battles;
            public List<int> Team1Ids { get; set; } = new List<int>();
            public List<int> Team2Ids { get; set; } = new List<int>();
            public string Title { get; set; }
        }

        // GET: Tourney
        public ActionResult Index()
        {
            var tourneyBattles = Global.Server.Battles.Values.Where(x => x != null).OfType<TourneyBattle>().ToList();

            return View("TourneyIndex", new TourneyModel() {Battles = tourneyBattles});
        }

        public ActionResult JoinBattle(string battleHost)
        {
            Global.Server.ForceJoinBattle(Global.Account?.Name, battleHost);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveBattle(int battleid)
        {
            var bat = Global.Server.Battles.Get(battleid);
            if (bat != null) Global.Server.RemoveBattle(bat);
            return RedirectToAction("Index");
        }

        public ActionResult ForceJoinPlayers(int battleid)
        {
            var bat = Global.Server.Battles.Get(battleid) as TourneyBattle;
            if (bat != null)
            {
                foreach (var p in bat.Prototype.TeamPlayers.SelectMany(x => x))
                {
                    Global.Server.ForceJoinBattle(p, bat);
                }
            }
            return RedirectToAction("Index");
        }

        [Auth(Role = AdminLevel.Moderator)]
        public ActionResult AddBattle(TourneyModel model)
        {
            var db = new ZkDataContext();
            {
                var tb = new TourneyBattle(Global.Server, new TourneyBattle.TourneyPrototype()
                {
                    Title = model.Title,
                    TeamPlayers = new List<List<string>>()
                    {
                        model.Team1Ids.Select(x=> db.Accounts.Find(x)?.Name).Where(x=>x!=null).ToList(),
                        model.Team2Ids.Select(x=> db.Accounts.Find(x)?.Name).Where(x=>x!=null).ToList()
                    }
                });
                Global.Server.AddBattle(tb);
            }
            return RedirectToAction("Index");
        }
    }
}