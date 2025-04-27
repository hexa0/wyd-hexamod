using System;
using System.Collections.Generic;

namespace HexaMod.Util
{
    public enum StockGameMode : int
    {
        regular = 0,
        familyGathering = 1,
        hungryGames = 2,
        dadlympics = 3,
        daddysNightmare = 4
    }

    public class GameMode
    {
        public int id = -1;
        public string internalName = "gamemode";
        public string name = "Gamemode";
        public string description = "A Gamemode.";
        public string tag = "GM";
        public string hostMenuName = "Family Gathering-Host";
        public string respawnRPC = "RespawnLotsOfPlayers";
        public bool twoPlayer = false;
        public bool canAlternate = true;
        public bool defaultTeamIsDad = false;
        public bool twoTeams = true;

        public static implicit operator global::GameModeInfo(GameMode v)
        {
            throw new NotImplementedException();
        }
    }

    public static class GameModes
    {
        public static Dictionary<int, GameMode> gameModes = new Dictionary<int, GameMode>();
        public static Dictionary<string, GameMode> named = new Dictionary<string, GameMode>();

        public static void DefineGameMode(GameMode info)
        {
            gameModes[info.id] = info;
            named[info.internalName] = info;
        }

        public static bool IsModes(int mode, string[] gamemodeNames)
        {
            foreach (var gamemodeName in gamemodeNames)
            {
                if (mode == named[gamemodeName].id)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsMode(int mode, string gamemodeName)
        {
            return named[gamemodeName].id == mode;
        }

        public static void DefineStandardGameModes()
        {
            {
                var info = new GameMode();

                info.id =           (int)StockGameMode.regular;
                info.internalName = "regular";
                info.name =         "Original";
                info.description =  "regular desc";
                info.tag =          null;
                info.hostMenuName = "WaitMenu-Original";
                info.respawnRPC =   null;
                info.twoPlayer =    true;

                DefineGameMode(info);
            }

            {
                var info = new GameMode();

                info.id =           (int)StockGameMode.familyGathering;
                info.internalName = "familyGathering";
                info.name =         "Family Gathering";
                info.description =  "familyGathering desc";
                info.tag =          "FG";
                info.hostMenuName = "Family Gathering-Host";
                info.respawnRPC =   "RespawnLotsOfPlayers";

                DefineGameMode(info);
            }

            {
                var info = new GameMode();

                info.id =           (int)StockGameMode.hungryGames;
                info.internalName = "hungryGames";
                info.name =         "The Hungry Games";
                info.description =  "hungryGames desc";
                info.tag =          "THG";
                info.hostMenuName = "HungryGames";
                info.respawnRPC =   "RespawnHungryPlayers";
                info.canAlternate =   false;
                info.twoTeams =     false;

                DefineGameMode(info);
            }

            {
                var info = new GameMode();

                info.id =               (int)StockGameMode.dadlympics;
                info.internalName =     "dadlympics";
                info.name =             "The Great Dadlympics";
                info.description =      "dadlympics desc";
                info.tag =              "TGD";
                info.hostMenuName =     "Dadlympics";
                info.respawnRPC =       "RespawnDadlympians";
                info.canAlternate =       false;
                info.defaultTeamIsDad = true;
                info.twoTeams =         false;

                DefineGameMode(info);
            }

            {
                var info = new GameMode();

                info.id =           (int)StockGameMode.daddysNightmare;
                info.internalName = "daddysNightmare";
                info.name =         "Daddy's Nightmare";
                info.description =  "daddysNightmare desc";
                info.tag =          "DNM";
                info.hostMenuName = "DaddysNightmare";
                info.respawnRPC =   "RespawnLotsOfPlayers";

                DefineGameMode(info);
            }
        }
    }
}
