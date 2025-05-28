using Naninovel;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static SingletonManager;
using Image = UnityEngine.UI.Image;

public partial class LobbyController
{
	[Header("멀티플레이어 로비")]
	public GameObject _Multi_Lobby;
	public UI_Button _Multi_LobbyBackButton;
	public UI_Multi_LobbyElement _Multi_LobbyPrefab;
	public Transform _Multi_LobbyParent;
	public UI_Button _Multi_RefreshButton;
	public UI_Button _Multi_CreateRoomButton;

	[Header("멀티플레이어 룸")]
	public GameObject _Multi_Room;
	public UI_Button _Multi_RoomBackButton;
	public TMP_InputField _Multi_ChatInputField;
	public UI_Button _Multi_SendChatButton;
	public UI_Button _Multi_StageSelectButton;
	public UI_Button _Multi_TroopButton;
	public UI_Button _Multi_ReadyButton;
	public Transform _Multi_ChatPrefab;
	public Transform _Multi_ChatParent;
	public UI_Multi_MemberElement _Multi_MemberPrefab;
	public Transform _Multi_MemberParent;
	public TMP_Text _Multi_StageNumber, _Multi_StageName;
	public Image _Multi_StageThumbnail;
	public Sprite _Multi_ButtonActiveSprite;

	[Header("멀티플레이어 룸/스테이지 선택")]
	public GameObject _Multi_StageSelectPopup;
	public UI_Button _Multi_StageSelectCloseButton;
	public UI_Multi_StageElement _Multi_StagePrefab;
	public Transform _Multi_StageParent;

	List<Lobby> _Multi_Lobbys;
	float _Multi_LobbyRequestTime;
	List<UI_Multi_LobbyElement> _Multi_LobbyElements;
	List<UI_Multi_MemberElement> _Multi_MemberElements;
	List<Transform> _Multi_ChatElements;
	List<UI_Multi_StageElement> _Multi_StageElements;
	UI_Multi_StageElement _Multi_StageSelectedElement;

	void Multi_LobbyShow(bool show)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			Game._IsSingleMode = true;
			ViewSetting(ViewType.Main);
			Main_Refresh();
			_Multi_Lobby.SetActive(false);
			return;
		}

		// 초기화
		Multiplayer.TryInit();
		if (!Multiplayer._Available) return;

		Game._IsSingleMode = false;
		ViewSetting(ViewType.Multi);
		_Multi_Lobby.SetActive(true);
		UI.FadeIn(0.3f);
	}

	void UpdateMultiplayer()
	{
		if (!Multiplayer._Available) return;

		// 1초마다 로비 새로고침
		if (_Multi_Lobby.activeSelf)
		{
			if (Time.time - _Multi_LobbyRequestTime > 1f)
			{
				Multi_LobbyRefresh();
			}
		}
	}

	void Multi_LobbyRefresh()
	{
		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			// 로비 목록 불러오기
			_Multi_LobbyRequestTime = Time.time;
			yield return StartCoroutine(Multiplayer.SendLobbyQuery(x => _Multi_Lobbys = x));

			_Multi_LobbyElements.DestroyElements();
			foreach (Lobby lobby in _Multi_Lobbys)
			{
				UI_Multi_LobbyElement element = Instantiate(_Multi_LobbyPrefab, _Multi_LobbyParent);
				element.gameObject.SetActive(true);
				element._Lobby = lobby;
				element._Name.text = lobby.GetData(MultiplayerManager.Key_LobbyName);
				element._PlayerCount.text = $"{lobby.MemberCount} / {MultiplayerManager.MaxPlayerCount}";
				element._Button.onClick.AddListener(() => Multi_RoomButton(element));
				_Multi_LobbyElements.Add(element);
			}
		}
	}

	void Multi_LobbyBackButton()
	{
		Multi_LobbyShow(false);
	}

	void Multi_CreateRoomButton()
	{
		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			UI.ShowNetworkLoading(true);
			bool success = false;
			yield return StartCoroutine(Multiplayer.CreateLobby(x => success = x));
			UI.ShowNetworkLoading(false);
			if (success)
			{
				Multi_RoomShow(true);
			}
			else
			{
				StartCoroutine(UI.ShowOneButtonPopup("로비를 생성할 수 없습니다"));
			}
		}
	}

	void Multi_RoomButton(UI_Multi_LobbyElement element)
	{
		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			UI.ShowNetworkLoading(true);
			bool success = false;
			yield return StartCoroutine(Multiplayer.JoinLobby(element._Lobby, x => success = x));
			UI.ShowNetworkLoading(false);

			if (success)
			{
				Multi_RoomShow(true);
			}
			else
			{
				StartCoroutine(UI.ShowOneButtonPopup("로비에 참가할 수 없습니다"));
			}
		}
	}

	void Multi_RoomShow(bool show)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			_Multi_Room.SetActive(false);
			return;
		}

		_Multi_ChatElements.DestroyElements();
		_Multi_Room.SetActive(true);
		UI.FadeIn(0.3f);
		Multi_OnLobbyChanged();
	}

	void Multi_RoomBackButton()
	{
		Multiplayer.LeaveLobby();
		Multi_RoomShow(false);
	}

	void Multi_SendChatButton()
	{
		if (string.IsNullOrWhiteSpace(_Multi_ChatInputField.text)) return;
		Multiplayer._Lobby.SendChatString(_Multi_ChatInputField.text);
		_Multi_ChatInputField.text = "";
		_Multi_ChatInputField.ActivateInputField();
	}

	void Multi_StageSelectButton()
	{
		_Multi_StageElements.DestroyElements();
		foreach (DataManager.Stage stage in Data._Stages)
		{
			UI_Multi_StageElement element = Instantiate(_Multi_StagePrefab, _Multi_StageParent);
			element.gameObject.SetActive(true);
			element._Stage = stage;
			element._Button.onClick.AddListener(() => Multi_StageElementButton(element));
			_Multi_StageElements.Add(element);
		}

		string mapName = Multiplayer._Lobby.GetData(MultiplayerManager.Key_LobbyMap);
		_Multi_StageSelectedElement = _Multi_StageElements.Find(x => x._Stage._Name == mapName);
		Multi_StageSelectRefresh();
		_Multi_StageSelectPopup.SetActive(true);
	}

	void Multi_StageSelectRefresh()
	{
		foreach (UI_Multi_StageElement element in _Multi_StageElements)
		{
			element._Name.text = $"{element._Stage._StageNumber} - {element._Stage._DisplayName}";
			element._Selected.SetActive(element == _Multi_StageSelectedElement);
		}
	}

	void Multi_StageElementButton(UI_Multi_StageElement element)
	{
		_Multi_StageSelectedElement = element;
		Multi_StageSelectRefresh();
	}

	void Multi_StageSelectCloseButton()
	{
		Multiplayer._Lobby.SetData(MultiplayerManager.Key_LobbyMap, _Multi_StageSelectedElement._Stage._Name);
		_Multi_StageSelectPopup.SetActive(false);
		//Multi_OnLobbyChanged();
	}

	void Multi_TroopButton()
	{
		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			Troop_Show(true);
			yield return new WaitUntil(() => !_Troop.activeSelf);
			Multiplayer.SetMemberData(IsReady(Multiplayer.Self));
		}
	}

	void Multi_ReadyButton()
	{
		// 방장일 때
		if (Multiplayer._Lobby.IsOwnedBy(Multiplayer.Self.Id))
		{
			Multiplayer._Lobby.SetData(MultiplayerManager.Key_LobbyState, MultiplayerManager.Value_Play);
			Game.LoadBattleScene(GetStage()._SceneName, false);
		}
		// 방장이 아닐 때
		else
		{
			Multiplayer.SetMemberData(!IsReady(Multiplayer.Self));
		}
	}

	void Multi_OnLobbyChanged()
	{
		// 멤버 리스트
		Lobby lobby = Multiplayer._Lobby;
		_Multi_MemberElements.DestroyElements();
		foreach (Friend member in lobby.Members)
		{
			UI_Multi_MemberElement element = Instantiate(_Multi_MemberPrefab, _Multi_MemberParent);
			element.gameObject.SetActive(true);
			element._Name.text = member.Name;
			element._Troop.text = lobby.GetMemberData(member, MultiplayerManager.Key_Troop);
			element._State.gameObject.SetActive(!Multiplayer._Lobby.IsOwnedBy(member.Id));
			element._State.text = lobby.GetMemberData(member, MultiplayerManager.Key_ReadyState);
			_Multi_MemberElements.Add(element);
		}

		// 스테이지 정보
		DataManager.Stage stageInfo = GetStage();
		_Multi_StageNumber.text = $"{stageInfo._StageNumber:00}";
		_Multi_StageThumbnail.sprite = stageInfo._Thumbnail;
		_Multi_StageName.text = stageInfo._DisplayName;

		// 버튼
		bool isOwner = lobby.IsOwnedBy(Multiplayer.UserId);
		_Multi_StageSelectButton.gameObject.SetActive(isOwner);
		_Multi_ReadyButton.interactable = true;
		_Multi_ReadyButton.image.overrideSprite = null;
		TMP_Text readyText = _Multi_ReadyButton.GetComponentInChildren<TMP_Text>();
		if (isOwner)
		{
			readyText.text = "게임시작";
			_Multi_ReadyButton.interactable = lobby.Members.All(x => IsReady(x));
		}
		else
		{
			readyText.text = "게임준비";
			bool isReady = IsReady(Multiplayer.Self);
			_Multi_ReadyButton.image.overrideSprite = isReady ? _Multi_ButtonActiveSprite : null;

			if (isReady && lobby.GetData(MultiplayerManager.Key_LobbyState) == MultiplayerManager.Value_Play)
				Game.LoadBattleScene("", false, true);
		}
	}

	void Multi_OnChat(Friend member, string message)
	{
		Transform element = Instantiate(_Multi_ChatPrefab, _Multi_ChatParent);
		element.gameObject.SetActive(true);
		element.Find("Name").GetComponent<TMP_Text>().text = member.Name;
		element.Find("Message").GetComponent<TMP_Text>().text = message;
		_Multi_ChatElements.Add(element);
	}

	bool IsReady(Friend friend)
	{
		bool ready = Multiplayer._Lobby.GetMemberData(friend, MultiplayerManager.Key_ReadyState) == MultiplayerManager.Value_Ready;
		ready |= Multiplayer._Lobby.IsOwnedBy(friend.Id);
		return ready;
	}

	DataManager.Stage GetStage()
	{
		string mapName = Multiplayer._Lobby.GetData(MultiplayerManager.Key_LobbyMap);
		return Data._Stages.Find(x => x._Name == mapName);
	}
}
