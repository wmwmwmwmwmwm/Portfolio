using GameCreator.Shooter;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static SingletonManager;

public partial class LobbyController : SingleInstance<LobbyController>, IBackButton
{
	public enum ViewType
	{
		Main,
		Stage,
		Character,
		Store,
		Troop,
		Cards,
		Multi,
	}

	[Serializable]
	public class ViewInfo
	{
		public ViewType ViewType;
		public bool _UseCharacterCamera, _UseMirrorCamera;
		public float _LensShiftX;
		public bool _UseHomeButton, _UseCurrency;
	}
	public List<ViewInfo> _ViewInfos;

	[Serializable]
	public class CharInfo
	{
		public CharacterName _Name;
		public RuntimeAnimatorController _AnimatorAsset;
		public Sprite _CharacterSlotActiveSprite;
		public Weapon _WeaponData;
		public Transform _WeaponPosition;
		public bool _HasGunPose;

		[HideInInspector] public GameObject _ModelInst, _WeaponModelInst;
		public AsyncOperationHandle<GameObject> _LoadHandle;
		[HideInInspector] public bool _Troop_GunAnimFlag;

		public Animator GetAnimator() => _ModelInst.GetComponent<Animator>();
	}
	[Header("캐릭터 정보")]
	public List<CharInfo> _CharInfos;

	[Header("공통")]
	public GameObject _Currency;
	public UI_Button _HomeButton;
	public Camera _CharacterCamera;
	public Camera _MirrorCamera;
	public TMP_Text _Gold, _Chip;

	[Header("메인")]
	public GameObject _Main;
	public CanvasGroup _RightMenu;
	//public UI_Room_Element _StoryCharElementPrefab;
	//public Transform _StoryCharElementParent;
	//public UI_StoryElement _Main_StoryPrefab;
	//public Transform _Main_StoryParent;
	//public UI_Button _Main_Char_BackButton;
	//public UI_Button _Main_Story_BackButton;
	public UI_Button _ConfigButton;
	public CanvasGroup _Message;
	public UI_Button _CharacterChangeLeftButton, _CharacterChangeRightButton;
	public UI_Button _StoryModeButton;
	public UI_Button _MultiplayerButton;
	public UI_Button _CharactersButton;
	public UI_Button _StoreButton;
	public UI_Button _ItemButton;
	public UI_Button _RoomButton;
	public Transform _Main_CharacterPosition;
	public UI_Button _ExitButton;
	public DraggableUI _CharacterDraggable;

	CharacterName _Main_CharacterName;

	void Start()
	{
		_Char_Slots = new();
		_Cards_Cards = new();
		_Multi_Lobbys = new();
		_Multi_LobbyElements = new();
		_Multi_MemberElements = new();
		_Multi_ChatElements = new();
		_Multi_StageElements = new();

		// 초기화
		_Main.SetActive(false);
		_StageSelect.SetActive(false);
		_Characters.SetActive(false);
		_Store.SetActive(false);
		_Troop.SetActive(false);
		_Cards.SetActive(false);
		_HomeButton.gameObject.SetActive(false);
		_Message.gameObject.SetActive(false);
		//_StoryCharElementPrefab.gameObject.SetActive(false);
		_Character_SlotPrefab.gameObject.SetActive(false);
		//_Main_StoryPrefab.gameObject.SetActive(false);
		_Troop_DragHandle.gameObject.SetActive(false);
		_Cards_CardPrefab.gameObject.SetActive(false);
		_Multi_Lobby.SetActive(false);
		_Multi_Room.SetActive(false);
		_Multi_StageSelectPopup.SetActive(false);
		_Multi_LobbyPrefab.gameObject.SetActive(false);
		_Multi_MemberPrefab.gameObject.SetActive(false);
		_Multi_ChatPrefab.gameObject.SetActive(false);
		_Multi_StagePrefab.gameObject.SetActive(false);

		// 버튼
		_HomeButton.onClick.AddListener(() => OnBackButton());
		_ConfigButton.onClick.AddListener(ConfigButton);
		_CharacterChangeLeftButton.onClick.AddListener(() => Main_CharacterChangeButton(-1));
		_CharacterChangeRightButton.onClick.AddListener(() => Main_CharacterChangeButton(1));
		_StoryModeButton.onClick.AddListener(() => Stage_Show(true));
		_MultiplayerButton.onClick.AddListener(() => Multi_LobbyShow(true));
		_CharactersButton.onClick.AddListener(() =>
		{
			_Char_ShowFromMain = true;
			Char_Show(true);
		});
		_StoreButton.onClick.AddListener(() => Store_Show(true));
		_RoomButton.onClick.AddListener(() => Main_RoomButton());
		_ItemButton.onClick.AddListener(() => Main_ItemButton());
		//_Main_Char_BackButton.onClick.AddListener(() => OnBackButton());
		//_Main_Story_BackButton.onClick.AddListener(() => OnBackButton());
		_ExitButton.onClick.AddListener(() => Game.Quit());
		_CharacterDraggable.DragCallback = (_, eventData) => CharacterDrag(ViewType.Main, eventData);
		_CharacterDraggable.ClickCallback = (_, _) => CharacterClick();
		_Stage_EnterButton.onClick.AddListener(() => Stage_EnterButton());
		_Character_CharacterDraggable.DragCallback = (_, eventData) => CharacterDrag(ViewType.Character, eventData);
		//_Character_StatusTabButton.onClick.AddListener(() => Char_TabButton(true));
		//_Character_StoryTabButton.onClick.AddListener(() => Char_TabButton(false));
		_Character_TroopEquipButton.onClick.AddListener(() => Char_TroopEquipButton());
		_Store_HomeButton.onClick.AddListener(() => OnBackButton());
		_Store_LeftButton.onClick.AddListener(() => Store_CharacterChangeButton(-1));
		_Store_RightButton.onClick.AddListener(() => Store_CharacterChangeButton(1));
		_Store_LevelUpButton.onClick.AddListener(() => Store_LevelUpButton());
		foreach (UI_Card card in _Store_Cards)
		{
			card._Button.onClick.AddListener(() => Store_CardButton(card));
		}
		_Troop_EnterButton.onClick.AddListener(() => Troop_EnterButton());
		for (int i = 0; i < _Troop_CharDraggables.Count; i++)
		{
			DraggableUI charDraggable = _Troop_CharDraggables[i];
			charDraggable.BeginDragCallback = Troop_CharBeginDrag;
			charDraggable.DragCallback = Troop_CharDrag;
			charDraggable.EndDragCallback = Troop_CharEndDrag;
			int index = i;
			charDraggable.ClickCallback = (_, _) => Troop_CharClick(index);
		}
		_Cards_EnterButton.onClick.AddListener(() => Cards_EnterButton());
		_Multi_LobbyBackButton.onClick.AddListener(() => Multi_LobbyBackButton());
		_Multi_RoomBackButton.onClick.AddListener(() => Multi_RoomBackButton());
		_Multi_RefreshButton.onClick.AddListener(() => Multi_LobbyRefresh());
		_Multi_CreateRoomButton.onClick.AddListener(() => Multi_CreateRoomButton());
		_Multi_SendChatButton.onClick.AddListener(() => Multi_SendChatButton());
		_Multi_StageSelectButton.onClick.AddListener(() => Multi_StageSelectButton());
		_Multi_TroopButton.onClick.AddListener(() => Multi_TroopButton());
		_Multi_ReadyButton.onClick.AddListener(() => Multi_ReadyButton());
		_Multi_StageSelectCloseButton.onClick.AddListener(() => Multi_StageSelectCloseButton());
		_Multi_ChatInputField.onSubmit.AddListener(_ => Multi_SendChatButton());

		// 이벤트
		Multiplayer._OnLobbyChanged = Multi_OnLobbyChanged;
		Multiplayer._OnChat = Multi_OnChat;

		// 시작
		RefreshCurrency();
		UI.FadeIn(0.3f);
		Util.PanelTween(_RightMenu, Vector2.right);
		Sound.PlayBgm(SoundManager.BgmType.Lobby);
		Main_Show(true);
	}

	void OnDestroy()
	{
		foreach (CharInfo info in _CharInfos)
		{
			if (info._LoadHandle.IsValid())
			{
				Addressables.Release(info._LoadHandle);
			}
		}

		// 이벤트
		Multiplayer._OnLobbyChanged = null;
		Multiplayer._OnChat = null;
	}

	void Update()
	{
		UpdateMultiplayer();
	}

	public bool OnBackButton()
	{
		if (_Characters.activeSelf)
		{
			Char_Show(false);
			return true;
		}
		else if (_Cards.activeSelf)
		{
			Cards_Show(false, _Cards_EquipMode);
			return true;
		}
		else if (_Troop.activeSelf)
		{
			Troop_Show(false);
			return true;
		}
		else if (_Store.activeSelf)
		{
			Store_Show(false);
			return true;
		}
		else if (_StageSelect.activeSelf)
		{
			Stage_Show(false);
			return true;
		}

		return false;
	}
	
	void ViewSetting(ViewType viewType)
	{
		ViewInfo info = _ViewInfos.Find(x => x.ViewType == viewType);
		_CharacterCamera.gameObject.SetActive(info._UseCharacterCamera);
		_MirrorCamera.gameObject.SetActive(info._UseMirrorCamera);
		_CharacterCamera.lensShift = new(info._LensShiftX, 0f);
		_Currency.SetActive(info._UseCurrency);
		_HomeButton.gameObject.SetActive(info._UseHomeButton);
	}

	void RefreshCurrency()
	{
		_Gold.text = UserData._Gold.ToString();
		_Chip.text = UserData._Chip.ToString();
	}

	void Main_Show(bool show)
	{
		if (!show)
		{
			_Main.SetActive(false);
			return;
		}

		ViewSetting(ViewType.Main);
		_Main.SetActive(true);
		Util.PanelTween(_RightMenu, Vector2.right);
		UI.FadeIn(0.3f);
		Main_Refresh();
	}

	void Main_Refresh()
	{
		HideModels();
		ShowModel(ViewType.Main, _Main_CharacterName);

		// 레드닷
		bool anyNotSeenStory = false;
		foreach (DataManager.Character charInfo in Data._Characters)
		{
			UserInfoManager.SaveData.CharacterInfo userData = UserData.GetCharacterInfo(charInfo._Name);
			for (int storyIndex = 0; storyIndex < charInfo._StoryScripts.Count; storyIndex++)
			{
				if (!userData._HaveSeenStorys.Contains(storyIndex))
				{
					anyNotSeenStory = true;
					break;
				}
			}
		}
		_RoomButton.RedDot.SetActive(anyNotSeenStory);
	}

	void ConfigButton()
	{
		// UI._Config.Show(true);
		UI._Config.SetActive(true);
	}

	void Main_CharacterChangeButton(int delta)
	{
		_Main_CharacterName += delta;
		_Main_CharacterName = (CharacterName)Util.Mod((int)_Main_CharacterName, _CharInfos.Count);
		Main_Refresh();
	}

	void Main_RoomButton()
	{
		Game.LoadRoomScene();
	}

	void Main_ItemButton()
	{
		Cards_Show(true, false);
	}

	CharInfo GetCharInfo(CharacterName charName) => _CharInfos[(int)charName];
}
