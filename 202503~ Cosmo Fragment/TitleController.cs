using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static SingletonManager;

public partial class TitleController : SingleInstance<TitleController>, IBackButton
{
	[Header("타이틀")]
	public AudioSource _Bgm2;
	public Animator _Title_Animator;
	public Button _Title_StartButton, _Title_OptionButton, _Title_ExitButton;
	public GameObject _Menus;

	[Header("로드")]
	public GameObject _LoadUserData;
	public UI_Load_Slot _Load_SlotPrefab;
	public Transform _Load_SlotParent;
	public Button _Load_DeleteButton, _Load_ExitButton;
	public Sprite _Load_DeleteNormalSprite, _Load_DeleteActiveSprite;

	bool _Load_DeleteMode;
	List<UI_Load_Slot> _Load_Slots;

	void Start()
	{
		_Load_Slots = new();
		_LoadUserData.SetActive(false);
		_Load_SlotPrefab.gameObject.SetActive(false);
		_Title_StartButton.onClick.AddListener(Title_StartButton);
		_Title_OptionButton.onClick.AddListener(Title_OptionButton);
		_Title_ExitButton.onClick.AddListener(Game.Quit);
		_Load_DeleteButton.onClick.AddListener(Load_DeleteButton);
		_Load_ExitButton.onClick.AddListener(Load_ExitButton);
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			UI._Fader.alpha = 0f;
			Sound.PlayBgm(SoundManager.BgmType.Title);
			_Title_Animator.Play("Animation");
			yield return new WaitUntil(() => _Title_Animator.IsComplete());
		}
	}

	public bool OnBackButton()
	{
		if (_LoadUserData.activeSelf)
		{
			Load_Show(false);
			return true;
		}
		return false;
	}

	void Title_OptionButton()
	{
		// UI._Config.Show(true);
		UI._Config.SetActive(true);
	}

	void Load_Show(bool show)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			_Load_DeleteMode = false;
			_Menus.SetActive(true);
			_LoadUserData.SetActive(false);
			return;
		}

		_Menus.SetActive(false);
		_LoadUserData.SetActive(true);
		Load_Refresh();
	}

	void Load_Refresh()
	{
		_Load_Slots.DestroyElements();
		for (int i = 0; i < 10; i++)
		{
			int profile = i + 1;
			if (_Load_DeleteMode && !User.CheckProfile(profile)) continue;

			UI_Load_Slot slot = Instantiate(_Load_SlotPrefab, _Load_SlotParent);
			slot.gameObject.SetActive(true);
			slot._Profile = profile;
			slot._SelectButton.onClick.AddListener(() => Load_SelectSlotButton(profile));
			slot._DeleteButton.onClick.AddListener(() => Load_DeleteSlotButton(profile));
			_Load_Slots.Add(slot);
		}

		foreach (UI_Load_Slot slot in _Load_Slots)
		{
			bool exist = User.CheckProfile(slot._Profile);

			// 썸네일
			if (exist)
			{
				slot._Thumbnail.sprite = slot._ThumbnailOnSprite;
			}
			else
			{
				slot._Thumbnail.sprite = slot._ThumbnailOffSprite;
			}

			// 텍스트
			StringBuilder text = new();
			text.AppendLine($"[ SLOT {slot._Profile:00} ]");
			if (exist)
			{
				GameManager.SharedData.LoadPreview profileInfo = Game._SharedData._LoadPreviews.Find(x => x._Profile == slot._Profile);
				DateTime time = DateTime.FromOADate(profileInfo._LastPlayedTime);
				text.AppendLine($"{time}");
				text.AppendLine($"CHAPTER 001");// profileInfo.note;
			}
			else
			{
				text.AppendLine($"New Game");
				text.AppendLine($"-- Empty --");
			}
			slot._Text.text = text.ToString();

			// 삭제 모드
			slot._DeleteButton.gameObject.SetActive(_Load_DeleteMode);
		}
		_Load_DeleteButton.GetComponent<Image>().sprite = _Load_DeleteMode ? _Load_DeleteActiveSprite : _Load_DeleteNormalSprite;
	}

	void Title_StartButton()
	{
		Load_Show(true);
	}

	void Load_SelectSlotButton(int profile)
	{
		User.Load(profile);
		Game.LoadLobbyScene();
	}

	void Load_DeleteSlotButton(int profile)
	{
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			yield return StartCoroutine(UI.ShowTwoButtonPopup("Do you really want to delete it?", "Delete", "Cancel"));
			if (UI._TwoButton_Result)
			{
				User.DeleteData(profile);
				_Load_DeleteMode = false;
				Load_Refresh();
			}
		}
	}

	void Load_DeleteButton()
	{
		_Load_DeleteMode = !_Load_DeleteMode;
		Load_Refresh();
	}

	void Load_ExitButton()
	{
		Load_Show(false);
	}
}
