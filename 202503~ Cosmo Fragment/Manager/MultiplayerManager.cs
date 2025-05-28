using Naninovel;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static SingletonManager;

public class MultiplayerManager : Singleton<MultiplayerManager>
{
	[ReadOnly] public bool _Available;
	[ReadOnly] public Lobby _Lobby;
	[ReadOnly] public Action _OnLobbyChanged;
	[ReadOnly] public Action<Friend, string> _OnChat;

	public const int SteamAppId = 480;
	public const int MaxPlayerCount = 4;
	public const string Key_LobbyName = "LobbyName";
	public const string Key_LobbyState = "LobbyState";
	public const string Key_LobbyOwnerId = "LobbyOwnerId";
	public const string Key_LobbyMap = "LobbyMap";
	public const string Key_Troop = "Troop";
	public const string Key_ReadyState = "ReadyState";
	public const string Value_Wait = "Wait";
	public const string Value_Ready = "Ready";
	public const string Value_Play = "Play";

	public string UserName => Steamworks.SteamClient.Name;
	public SteamId UserId => Steamworks.SteamClient.SteamId;
	public Friend Self => _Lobby.Members.FirstOrDefault(x => x.IsMe);

	protected override void Init()
	{
		if (_Available) return;
		try
		{
			Steamworks.SteamClient.Init(SteamAppId);

			SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
			SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
			SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
			SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
			SteamMatchmaking.OnLobbyMemberDataChanged -= OnLobbyMemberDataChanged;
			SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
			SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
			SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
			SteamMatchmaking.OnChatMessage -= OnChatMessage;
			SteamMatchmaking.OnChatMessage += OnChatMessage;
			//SteamMatchmaking.OnLobbyInvite -= OnLobbyInviteHandler;
			//SteamMatchmaking.OnLobbyInvite += OnLobbyInviteHandler;
			//SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreatedHandler;
			//SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedHandler;
			//SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberLeaveHandler;
			//SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberLeaveHandler;
		}
		catch
		{
			Debug.Log("스팀을 실행해주세요.");
		}
		_Available = true;
	}

	void OnDestroy()
	{
		if (_Lobby.Id.IsValid)
		{
			LeaveLobby();
		}
		Steamworks.SteamClient.Shutdown();
	}

	public void TryInit()
	{
		Init();
	}

	public IEnumerator SendLobbyQuery(Action<List<Lobby>> callback)
	{
		LobbyQuery lobbyQuery = new();
		lobbyQuery.WithMaxResults(10); // 최대 10개의 결과를 가져옵니다.
		lobbyQuery.WithSlotsAvailable(1); // 최소 1자리 빈 로비 검색
		lobbyQuery.FilterDistanceClose(); // 근거리만
		lobbyQuery.WithKeyValue("Game", "TSJK"); // GAME 키값이 TSJK 인것만
		lobbyQuery.WithKeyValue(Key_LobbyState, Value_Wait); // 대기 중인 로비
		Lobby[] result = null;
		yield return lobbyQuery.RequestAsync().AsUniTask().ToCoroutine((x) => result = x);

		List<Lobby> list = new();
		if (result != null)
		{
			foreach (Lobby lobby in result)
			{
				if (!CheckValidLobby(lobby)) continue;

				list.Add(lobby);
			}
		}
		callback(list);
	}

	public IEnumerator CreateLobby(Action<bool> callback)
	{
		// 로비 생성 요청: 최대 4명의 플레이어, 공개 로비
		Lobby? result = null;
		yield return SteamMatchmaking.CreateLobbyAsync(MaxPlayerCount).AsUniTask().ToCoroutine(x => result = x);

		if (!result.HasValue) yield break;
		Lobby lobby = result.Value;
		Debug.Log($"로비 생성 성공. Lobby ID: {lobby.Id}");

		// 로비 설정 추가
		lobby.SetData("Game", "TSJK");
		lobby.SetData(Key_LobbyName, $"{UserName}의 방");
		lobby.SetData(Key_LobbyState, Value_Wait);
		lobby.SetData(Key_LobbyOwnerId, UserId.ToString());
		lobby.SetData(Key_LobbyMap, Data._Stages.First()._Name);

		_Lobby = lobby;
		SetMemberData(false);

		// 생성된 로비의 정보를 사용할 수 있음
		SteamP2PRelayTransport.Singleton.serverId = 0;
		NetworkConnectManager.Singleton.maxConnectedPlayers = MaxPlayerCount;
		callback(true);
	}

	public IEnumerator JoinLobby(Lobby lobby, Action<bool> callback)
	{
		if (!CheckValidLobby(lobby)) yield break;

		Lobby? result = null;
		yield return SteamMatchmaking.JoinLobbyAsync(lobby.Id).AsUniTask().ToCoroutine(x => result = x);

		if (!result.HasValue) yield break;

		_Lobby = result.Value;
		Debug.Log($"로비 참가 성공. Lobby ID: {_Lobby.Id}");
		SetMemberData(false);

		// 서버 아이디 셋팅!
		SteamP2PRelayTransport.Singleton.serverId = _Lobby.Owner.Id;
		callback(true);
	}

	public void SetMemberData(bool ready)
	{
		// 멤버 데이터
		_Lobby.SetMemberData(Key_ReadyState, ready ? Value_Ready : Value_Wait);
		StringBuilder str = new();
		for (int i = 0; i < UserData.Troop.Count; i++)
		{
			CharacterName charName = UserData.Troop[i];
			str.Append(charName.ToString());
			if (i < UserData.Troop.Count - 1)
			{
				str.Append(", ");
			}
		}
		_Lobby.SetMemberData(Key_Troop, str.ToString());
	}

	public void LeaveLobby()
	{
		if (!_Lobby.Id.IsValid) return;
		SteamP2PRelayTransport.Singleton.serverId = 0;
		_Lobby.Leave();
		_Lobby = default;
	}

	bool CheckValidLobby(Lobby lobby)
	{
		bool valid = true;
		valid &= lobby.Id.IsValid;
		valid &= lobby.GetData(Key_LobbyState) == Value_Wait;
		valid &= lobby.MemberCount > 0;
		valid &= lobby.GetData(Key_LobbyOwnerId) != UserId.ToString();
		return valid;
	}

	void OnLobbyDataChanged(Lobby lobby)
	{
		if (lobby.Id != _Lobby.Id) return;
		_OnLobbyChanged?.Invoke();
	}

	void OnLobbyMemberJoined(Lobby lobby, Friend friend)
	{
		if (lobby.Id != _Lobby.Id) return;
		_OnLobbyChanged?.Invoke();
	}

	void OnLobbyMemberDataChanged(Lobby lobby, Friend friend)
	{
		if (lobby.Id != _Lobby.Id) return;
		_OnLobbyChanged?.Invoke();
	}

	void OnLobbyMemberLeave(Lobby lobby, Friend friend)
	{
		if (lobby.Id != _Lobby.Id) return;
		_OnLobbyChanged?.Invoke();

		// 방장이 나가서 방장이 변경되었다면
		if (lobby.Owner.Id == Multiplayer.UserId)
		{
			lobby.SetData(Key_LobbyName, $"{Multiplayer.UserName}의 방");
			lobby.SetData(Key_LobbyOwnerId, Multiplayer.UserId.ToString());
			SteamP2PRelayTransport.Singleton.serverId = 0;
		}
		else
		{
			SteamP2PRelayTransport.Singleton.serverId = lobby.Owner.Id;
		}
		NetworkConnectManager.Singleton.DisconnectPlayer("SteamId." + friend.Id.ToString());
	}

	void OnChatMessage(Lobby lobby, Friend friend, string message)
	{
		if (lobby.Id != _Lobby.Id) return;
		_OnChat?.Invoke(friend, message);
	}

}
