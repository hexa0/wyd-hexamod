using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HexaMapAssemblies;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod.SerializableObjects
{
	// [XmlRoot("MatchStartObject", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	public class MatchStartObject
	{
		public Dictionary<uint, ushort> spawns = new Dictionary<uint, ushort>();

		public MatchStartObject DetermineSpawns(LobbySettings lobby)
		{
			// we could seed this with lobby.roundNumber but that would be bad
			System.Random shuffleRNG = new System.Random();

			uint totalDads = 0u;
			uint totalBabies = 0u;

			foreach (var item in HexaMod.persistentLobby.dads)
			{
				if (item.Value)
				{
					totalDads++;
				}
				else
				{
					totalBabies++;
				}
			}

			// we limit the amount of spawns we can pick from based on this to keep it fair
			uint idealUniqueSpawns = Math.Min(totalDads, totalBabies);

			// if a team is empty we override it with the max instead for single team modes & for test games with only one team
			if (totalBabies == 0)
			{
				idealUniqueSpawns = totalDads;
			}
			else if (totalDads == 0)
			{
				idealUniqueSpawns = totalBabies;
			}

			uint dadId = 0u;
			uint babyId = 0u;

			TeamSpawn dadSpawnObject = Assets.dadSpawn;
			TeamSpawn babySpawnObject = Assets.babySpawn;

			if (Assets.babySpawn.hgSpawns != null && HexaMod.networkManager.curGameMode == GameModes.named["hungryGames"].id)
			{
				babySpawnObject = Assets.babySpawn.hgSpawns;
			}

			Transform[] dadSpawns = new Transform[Math.Min(dadSpawnObject.GetSpawnCount(), idealUniqueSpawns)];
			Transform[] babySpawns = new Transform[Math.Min(babySpawnObject.GetSpawnCount(), idealUniqueSpawns)];

			for (int i = 0; i < dadSpawns.Length; i++)
			{
				dadSpawns[i] = dadSpawnObject.GetSpawn(i);
			}

			for (int i = 0; i < babySpawns.Length; i++)
			{
				babySpawns[i] = babySpawnObject.GetSpawn(i);
			}

			Transform[] shuffledDadSpawns = new Transform[dadSpawns.Length];
			Transform[] shuffledBabySpawns = new Transform[babySpawns.Length];

			Array.Copy(dadSpawns, shuffledDadSpawns, dadSpawns.Length);
			Array.Copy(babySpawns, shuffledBabySpawns, babySpawns.Length);

			shuffleRNG.Shuffle(shuffledDadSpawns);
			shuffleRNG.Shuffle(shuffledBabySpawns);

			foreach (var item in HexaMod.persistentLobby.dads)
			{
				if (item.Value)
				{
					Transform spawn = shuffledDadSpawns[dadId % shuffledDadSpawns.Length];
					spawns.Add((uint)item.Key, (ushort)Array.FindIndex(dadSpawns, element => element == spawn));
					dadId++;
				}
				else
				{
					Transform spawn = shuffledBabySpawns[babyId % shuffledBabySpawns.Length];
					spawns.Add((uint)item.Key, (ushort)Array.FindIndex(babySpawns, element => element == spawn));
					babyId++;
				}
			}

			return this;
		}

		public static MatchStartObjectSerializer serializer = new MatchStartObjectSerializer();
	}

	public class MatchStartObjectSerializer
	{
		public byte[] Serialize(MatchStartObject state)
		{
			SerializationHelper writer = new SerializationHelper();

			writer.Write((ushort)state.spawns.Count);
			foreach (var item in state.spawns)
			{
				Mod.Warn(item.Key, " > ", item.Value);
				writer.Write(item.Key);
				writer.Write(item.Value);
			}

			return writer.data.ToArray();
		}

		public MatchStartObject Deserialize(byte[] serializedBytes)
		{
			SerializationHelper reader = new SerializationHelper()
			{
				data = serializedBytes.ToList()
			};

			MatchStartObject state = new MatchStartObject();

			ushort count = reader.ReadUshort();
			for (int i = 0; i < count; i++)
			{
				state.spawns[reader.ReadUint()] = reader.ReadUshort();
			}

			return state;
		}

		public MatchStartObject MakeUnique(MatchStartObject toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
