using System.Collections.Generic;
using UnityEngine;

namespace HexaMapAssemblies
{
    public static class GlobalPhotonFactory
    {
        public static int startingNetId = 10000;
        public static int currentNetId = startingNetId;
    }

    public class PhotonFactory : MonoBehaviour
    {
        void Start()
        {
            var view = gameObject.AddComponent<PhotonView>();
            view.viewID = GlobalPhotonFactory.currentNetId;
            GlobalPhotonFactory.currentNetId += 1;
            view.ObservedComponents = new List<Component>(0);
        }
    }
}