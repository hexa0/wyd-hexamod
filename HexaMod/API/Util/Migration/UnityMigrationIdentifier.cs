using System;
using UnityEngine;

namespace HexaMod.API.Util.Migration
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class UnityMigrationIdentifier : Attribute
	{
		public string Id { get; private set; }

		public UnityMigrationIdentifier(string id)
		{
			Id = id;
		}
	}

	public interface ISerializableMigrationIdentifier
	{
		string RuntimeMigrationId { get; set; }
	}

	public class MigratableMonoBehavior : MonoBehaviour, ISerializableMigrationIdentifier, ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		public string _serializedTypeId;
		public string RuntimeMigrationId { get => _serializedTypeId; set { _serializedTypeId = value; } }
		public void OnBeforeSerialize() => this.BeforeMigrateSerialize((id) => _serializedTypeId = id);
		public void OnAfterDeserialize() => this.AfterMigrateSerialize();
	}

	public class MigratablePhotonMonoBehavior : Photon.MonoBehaviour, ISerializableMigrationIdentifier, ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		public string _serializedTypeId;
		public string RuntimeMigrationId { get => _serializedTypeId; set { _serializedTypeId = value; } }
		public void OnBeforeSerialize() => this.BeforeMigrateSerialize((id) => _serializedTypeId = id);
		public void OnAfterDeserialize() => this.AfterMigrateSerialize();
	}

	public class MigratableScriptableObject : ScriptableObject, ISerializableMigrationIdentifier, ISerializationCallbackReceiver
	{
		[SerializeField]
		// [HideInInspector]
		public string _serializedTypeId;
		public string RuntimeMigrationId { get => _serializedTypeId; set { _serializedTypeId = value; } }
		public void OnBeforeSerialize() => this.BeforeMigrateSerialize((id) => _serializedTypeId = id);
		public void OnAfterDeserialize() => this.AfterMigrateSerialize();
	}
}