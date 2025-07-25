using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using HexaMod.API.Util.Extensions;
using HexaMod.API.Util.Migration;
using HexaMod.API.Util.WhosYourDaddy;
using HexaMod.Scripts.Multiplayer.Lobby;
using HexaMod.Scripts.Persistent;
using HexaMod.SDK.Levels.Scripts.System;
using UnityEngine;
using Random = System.Random;

namespace HexaMod.SDK.Levels.Scripts.Spawning.Player
{
	[Serializable]
	public struct TeamSpawnEntry
	{
		public string gamemode;
		public GameObject value;
	}

	[UnityMigrationIdentifier("HexaMod.ff3e0c29-6047-4b94-b030-a67b8d3b651a")]
	public class TeamSpawn : MigratableMonoBehavior
	{
		[SerializeField]
		private TeamSpawnEntry[] spawns;

		[SerializeField]
		[HideInInspector]
		private string[] _serializedGamemodes = new string[0];
		[SerializeField]
		[HideInInspector]
		private GameObject[] _serializedSpawns = new GameObject[0];

		static readonly GamemodeSelector<GameObject, TeamSpawnEntry> selector = new GamemodeSelector<GameObject, TeamSpawnEntry>();

		private List<Transform> spawnSpots;

		void Awake()
		{
			if (!Application.isEditor)
			{
				selector.Deserialize(_serializedGamemodes, _serializedSpawns, out spawns);
			}
		}

		void OnValidate()
		{
			if (Application.isEditor)
			{
				selector.Serialize(out _serializedGamemodes, out _serializedSpawns, spawns);
			}
		}

		/// <summary>
		///		Gets the spawn point for the given teamNumber.
		/// </summary>
		/// <param name="teamPlayerId">the number for their team, counts up from 0 for each team separately</param>
		/// <returns></returns>
		public Transform GetSpawn(int teamPlayerId)
		{
			FetchSpawns();
			return spawnSpots[teamPlayerId % spawnSpots.Count];
		}

		public ushort GetSpawnCount()
		{
			FetchSpawns();
			return (ushort)spawnSpots.Count;
		}

		public void FetchSpawns()
		{
			LobbySettings lobby = HexaPersistentLobby.instance.lobbySettings;
			GameMode gamemode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];

			spawnSpots = new List<Transform>();
			GameObject parent = selector.Select(spawns);

			Random shuffleRNG = new Random(lobby.seed + lobby.roundNumber);
			
			Dictionary<int, uint> teamTotals = new Dictionary<int, uint>();

			foreach (PhotonPlayer player in PhotonNetwork.playerList)
			{
				Hashtable hash = player.CustomProperties;
				hash.TryGetValue("Team", out object teamValue);
				string team = (string)teamValue;
				int teamId = gamemode.teams.GetTeamId(team);

				if (!teamTotals.ContainsKey(teamId))
				{
					teamTotals[teamId] = 0;
				}

				teamTotals[teamId]++;
			}

			uint idealUniqueSpawns = teamTotals.Min(pair => pair.Value);

			if (idealUniqueSpawns == 0)
			{
				// if a team is empty we override it with the max instead for single team modes & for test games with only one team
				idealUniqueSpawns = teamTotals.Max(pair => pair.Value);
			}

			if (lobby.spawnMode == SpawnLocationMode.Vanilla)
			{
				idealUniqueSpawns = 1;
			}
			else if (lobby.spawnMode == SpawnLocationMode.All)
			{
				idealUniqueSpawns = (uint)parent.transform.childCount;
			}

			GameObject[] usedSpawns = ObjectUtils.GetChildren(parent)
				.Take((int)idealUniqueSpawns)
				.ToArray();
			GameObject[] shuffledSpawns = usedSpawns
				.OrderBy(_ => shuffleRNG.Next())
				.ToArray();

			foreach (var spawnSpot in shuffledSpawns)
			{
				spawnSpots.Add(spawnSpot.transform);
			}
		}
	}
}
