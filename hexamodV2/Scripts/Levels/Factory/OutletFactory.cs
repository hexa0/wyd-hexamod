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
            outlet.explosion = Levels.StaticAssets.outletExplosion;
            outlet.shockSound = Levels.StaticAssets.outletShockSound;
        }
    }
}
