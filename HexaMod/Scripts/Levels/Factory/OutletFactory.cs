using HexaMod;
using UnityEngine;

namespace HexaMapAssemblies
{
    public class OutletFactory : MonoBehaviour
    {
        void Start()
        {
            var outlet = gameObject.AddComponent<PowerOutlet>();

            outlet.tag = "Use";
            outlet.explosion = Assets.StaticAssets.outletExplosion;
            outlet.shockSound = Assets.StaticAssets.outletShockSound;
			outlet.coverPrefab = Assets.StaticAssets.outletCoverPrefab;
		}
    }
}
