using System.Collections.Generic;
using HexaMod;
using UnityEngine;

namespace HexaMapAssemblies
{
    public class PrefabFactory : MonoBehaviour
    {
        void Start()
        {
            var instantiated = Instantiate((GameObject)Resources.Load(PrefabName, typeof(GameObject)), transform.position, transform.rotation);
            instantiated.name = instantiated.name.Replace("(Clone)", "");

            if (Assets.loadedLevelInstance)
            {
                instantiated.transform.SetParent(Assets.loadedLevelInstance);
            }

			GlobalPhotonFactory.Register(instantiated);

            Destroy(gameObject);
        }

        public string PrefabName = "Baby Gate";
    }
}
