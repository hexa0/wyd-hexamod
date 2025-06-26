using System.Collections.Generic;
using System.Linq;
using HexaMod.Util;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class TeamSpawn : MonoBehaviour
	{
		[HideInInspector]
		public Transform[] spots;

		public GameObject spawns;
		private List<Transform> spawnSpots;
		private bool initialized = false;

		public Transform GetSpawn(int id)
		{
			return spawnSpots[id];
		}

		public ushort GetSpawnCount()
		{
			return (ushort)spawnSpots.Count;
		}

		public void Init()
		{
			if (initialized)
			{
				return;
			}

			initialized = true;

			if (spawns == null && spots != null)
			{
				spawnSpots = spots.ToList();
			}
			else
			{
				spawnSpots = new List<Transform>();
			}

			if (spawns != null)
			{
				foreach (var spawnSpot in ObjectUtils.GetChildren(spawns))
				{
					spawnSpot.SetActive(false);
					spawnSpots.Add(spawnSpot.transform);
				}
			}
		}
	}
}
