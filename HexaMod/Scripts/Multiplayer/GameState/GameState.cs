using UnityEngine;
using UnityEngine.Events;

namespace HexaMod.Scripts.Multiplayer.GameState
{
	public enum LobbyState : byte
	{
		InLobby,
		Syncing,
		InGame,
	}

	[RequireComponent(typeof(PhotonView))]
	public class LobbyStateController : Photon.MonoBehaviour
	{
		public LobbyState State => state;
		public float ChangeTimer => changeTimer;
		public LobbyStateController instance;

		internal LobbyState state = LobbyState.InLobby;
		internal LobbyState nextState = LobbyState.InLobby;
		internal float changeTimer = -1f;
		internal float nextChangeTimer = -1f;

		public UnityEvent onAdvancedState;

		public void Awake()
		{
			instance = this;
		}

		// Calculated on FixedUpdate instead of Update as this doesn't need to be super precise
		public void FixedUpdate()
		{
			if (photonView.isMine && changeTimer != -1f)
			{
				changeTimer -= Time.fixedDeltaTime;

				if (changeTimer <= 0f)
				{
					state = nextState;
					changeTimer = nextChangeTimer;
					onAdvancedState.Invoke();
				}
			}
		}

		public void ChangeState(LobbyState state, float length = -1f)
		{
			this.state = state;
			changeTimer = length;
			nextChangeTimer = -1f;

			if (PhotonNetwork.isMasterClient)
			{
				photonView.RPC("RPC_ChangeState", PhotonTargets.OthersBuffered, (byte)state);
			}
		}

		public void QueueNextState(LobbyState state, float length = -1f)
		{
			nextState = state;
			nextChangeTimer = length;

			if (PhotonNetwork.isMasterClient)
			{
				photonView.RPC("RPC_ChangeState", PhotonTargets.OthersBuffered, (byte)nextState);
			}
		}

		[PunRPC]
		public void RPC_ChangeState(byte newState)
		{
			ChangeState((LobbyState)newState);
		}
	}
}
