using HexaMod.API.Util.Migration;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.9cf97616-79b1-45a5-987e-946706bbedb4")]
	public class OutletFactory : MigratableMonoBehavior
	{
		void Start()
		{
			var outlet = gameObject.AddComponent<PowerOutlet>();

			outlet.tag = "Use";
			outlet.explosion = Assets.StaticAssets.outletExplosion;
			outlet.shockSound = Assets.StaticAssets.outletShockSound;
			outlet.coverPrefab = Assets.StaticAssets.outletCoverPrefab;

			Destroy(this);
		}
	}
}
