using System.Collections.Generic;

namespace HexaMod.API.Util.WhosYourDaddy
{
	// TODO: teams will be integers indexing the teams class instead of booleans (to allow for custom gamemodes with new team types)
	public struct Team
	{
		public string displayName;
		public string selectorName;
		public string prefabName;
		// if this team can be buttered, if true when buttered they cannot pick up items for a specific time
		public bool canBeButtered;
		public int maxPlayers;
	}

	public class Teams
	{
		public Team[] teams;

		public static Team dadTeam = new Team()
		{
			displayName = "Dad",
			selectorName = "D",
			prefabName = "dadV2",
			canBeButtered = false,
			maxPlayers = 0
		};

		public static Team babyTeam = new Team()
		{
			displayName = "Baby",
			selectorName = "B",
			prefabName = "babyV2",
			canBeButtered = true,
			maxPlayers = 0
		};

		public Team GetTeamFromId(int team)
		{
			if (teams.Length == 0)
			{
				return default;
			}

			if (team < 0 || team >= teams.Length)
			{
				return teams[0];
			}

			return teams[team];
		}

		public int GetTeamId(string selector)
		{
			for (int i = 0; i < teams.Length; i++)
			{
				if (teams[i].selectorName == selector)
				{
					return i;
				}
			}

			return -1;
		}

		public Team GetTeamFromString(string selector)
		{
			foreach (Team team in teams)
			{
				if (team.selectorName == selector)
				{
					return team;
				}
			}

			return default;
		}

		public int defaultTeamId = 0;
		public int hostDefaultTeamId = -1; // -1 means no default team for host, use the defaultTeamId instead
	}

	public class TwoTeams : Teams
	{
		public TwoTeams()
		{
			teams = new Team[]
			{
				dadTeam,
				babyTeam
			};

			defaultTeamId = 1;
			hostDefaultTeamId = 0;
		}
	}

	public class OneTeamDad : Teams
	{
		public OneTeamDad()
		{
			teams = new Team[] {
				dadTeam
			};

			defaultTeamId = 0;
		}
	}

	public class OneTeamBaby : Teams
	{
		public OneTeamBaby()
		{
			teams = new Team[] {
				babyTeam
			};

			defaultTeamId = 1;
		}
	}

	public class IntroMessage
	{
		public string title = "Replace this message in GameModes.cs!";
		public string body = "you idiot";
	}

	public class GameMode
	{
		public int id = -1;
		public string internalName = "gamemode";
		public string name = "Gamemode";
		public string description = "A Gamemode.";
		public string tag = "GM";
		public string hostMenuName = "Family Gathering-Host";
		public IntroMessage[] introMessages = new IntroMessage[] {
			new IntroMessage()
			{
				title = "Watch Your Son!",
				body = "Mom should be home by 4"
			},
			new IntroMessage()
			{
				title = "Avoid daddy's love!",
				body = "Mommy will be home by 4 to help him"
			}
		};
		public Teams teams = new TwoTeams();
		public float[] waitTimes = new float[] { 0f, 5f };
		public bool lightsOut = false;
		public bool spawnWeapons = false;
		public bool spawnOVWeapons = false;
		public bool spawnTraps = false;
		public bool spawnPickups = false;
		public bool spawnArenaStartItems = false;
		public bool waitAsCountdown = false;
		public bool twoPlayer = false;
		public bool canShuffle = true;
		public bool hostDefaultTeamIsDad = false;
		public bool defaultTeamIsDad = false;
		public bool twoTeams = true;
		public bool babiesCanDie = true;
		public bool petsAllowed = false;

		public IntroMessage GetIntroMessage(int team)
		{
			if (introMessages.Length == 0)
			{
				return default;
			}

			if (team < 0 || team >= introMessages.Length)
			{
				return introMessages[0];
			}

			return introMessages[team];
		}
	}

	public static class GameModes
	{
		public static Dictionary<int, GameMode> gameModes = new Dictionary<int, GameMode>();
		public static Dictionary<string, GameMode> named = new Dictionary<string, GameMode>();

		public static void DefineGameMode(GameMode info)
		{
			if (info.id < 0)
			{
				info.id = gameModes.Count;
			}

			gameModes[info.id] = info;
			named[info.internalName] = info;
		}

		public static int GetId(string gameModeInternalName)
		{
			return named[gameModeInternalName].id;
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
			DefineGameMode(new GameMode
			{
				id = 0,
				internalName = "regular",
				name = "Original",
				description = "regular desc",
				tag = "",
				hostMenuName = "WaitMenu-Original",
				twoPlayer = true,
				petsAllowed = true
			});

			DefineGameMode(new GameMode
			{
				id = 1,
				internalName = "familyGathering",
				name = "Family Gathering",
				description = "familyGathering desc",
				tag = "FG",
				hostMenuName = "Family Gathering-Host",
				hostDefaultTeamIsDad = true,
				petsAllowed = true
			});

			DefineGameMode(new GameMode
			{
				id = 2,
				internalName = "hungryGames",
				name = "The Hungry Games",
				description = "hungryGames desc",
				tag = "THG",
				hostMenuName = "HungryGames",
				canShuffle = false,
				twoTeams = false,
				waitAsCountdown = true,
				spawnOVWeapons = true,
				spawnTraps = true,
				spawnPickups = true,
				spawnArenaStartItems = true,
				teams = new OneTeamBaby(),
				waitTimes = new float[] { 0f, 10f },
				introMessages = new IntroMessage[] {
					new IntroMessage()
					{
						title = "",
						body = ""
					}
				}
			});

			DefineGameMode(new GameMode
			{
				id = 3,
				internalName = "dadlympics",
				name = "The Great Dadlympics",
				description = "dadlympics desc",
				tag = "TGD",
				hostMenuName = "Dadlympics",
				canShuffle = false,
				defaultTeamIsDad = true,
				twoTeams = false,
				teams = new OneTeamDad(),
				introMessages = new IntroMessage[] {
					new IntroMessage()
					{
						title = "Complete chores!",
						body = "You have until 4"
					}
				}
			});

			DefineGameMode(new GameMode
			{
				id = 4,
				internalName = "daddysNightmare",
				name = "Daddy's Nightmare",
				description = "daddysNightmare desc",
				tag = "DNM",
				hostMenuName = "DaddysNightmare",
				hostDefaultTeamIsDad = true,
				babiesCanDie = false,
				lightsOut = true,
				spawnWeapons = true,
				introMessages = new IntroMessage[] {
					new IntroMessage()
					{
						title = "Get the power on!",
						body = "Mom should be home by 4"
					},
					new IntroMessage()
					{
						title = "Get Daddy!",
						body = "Hurry before Mommy gets home!"
					}
				}
			});
		}

		public static void DefineDefaultModdedGameModes()
		{
			// these are placeholder and may change
			// currently they don't do anything as custom host and play online menus aren't implemented yet
			// i have a LOT of work ahead of me before any of these can even remotely exist and be playable
			DefineGameMode(new GameMode
			{
				internalName = "luigisMansion",
				name = "Luigi's Mansion",
				description = "reacreation of the Nintendo Land Luigi's Mansion Attraction",
				tag = "LM",
				hostMenuName = "Family Gathering",
				canShuffle = true,
				defaultTeamIsDad = true,
				twoTeams = true,
				teams = new Teams()
				{
					teams = new Team[] {
						new Team()
						{
							displayName = "Luigis",
							selectorName = "L",
							prefabName = "luigiObj",
							canBeButtered = false
						},
						new Team()
						{
							displayName = "Ghost",
							selectorName = "G",
							prefabName = "ghostObj",
							canBeButtered = false,
							maxPlayers = 1
						}
					},
					defaultTeamId = 0,
					hostDefaultTeamId = 1
				},
				introMessages = new IntroMessage[] {
					new IntroMessage()
					{
						title = "You're a ghost!",
						body = "You are invisible as long as you aren't in light, sneak up on players to catch them"
					},
					new IntroMessage()
					{
						title = "Catch the ghost!",
						body = "You have until 4"
					}
				}
			});

			DefineGameMode(new GameMode
			{
				internalName = "propHunt",
				name = "Prop Hunt",
				description = "",
				tag = "LM",
				hostMenuName = "Family Gathering",
				canShuffle = true,
				defaultTeamIsDad = true,
				twoTeams = true,
				teams = new Teams()
				{
					teams = new Team[] {
						new Team()
						{
							displayName = "Seekers",
							selectorName = "D",
							prefabName = "dadObj",
							canBeButtered = false
						},
						new Team()
						{
							displayName = "Props",
							selectorName = "P",
							prefabName = "propObj",
							canBeButtered = false
						}
					},
					defaultTeamId = 0,
					hostDefaultTeamId = 1
				},
				introMessages = new IntroMessage[] {
					new IntroMessage()
					{
						title = "You're a ghost!",
						body = "You are invisible as long as you aren't in light, sneak up on players to catch them"
					},
					new IntroMessage()
					{
						title = "Catch the ghost!",
						body = "You have until 4"
					}
				}
			});
		}
	}
}
