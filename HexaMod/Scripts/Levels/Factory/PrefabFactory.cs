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

            var view = instantiated.GetComponent<PhotonView>();
            if (view)
            {
                view.viewID = GlobalPhotonFactory.currentNetId;
                GlobalPhotonFactory.currentNetId += 1;
                view.ObservedComponents = new List<Component>(0);
            }

            Destroy(gameObject);
        }

        public string PrefabName = "Baby Gate";
    }
}
