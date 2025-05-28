using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static SingletonManager;

public partial class LobbyController 
{
	[Header("캐릭터")]
	public GameObject _Characters;
	public CanvasGroup _Character_RightMenu;
	public Transform _Character_CharacterPosition;
	public UI_Character_Slot _Character_SlotPrefab;
	public Transform _Character_SlotParent;
	public DraggableUI _Character_CharacterDraggable;
	public TMP_Text _Character_NameText;
	public TMP_Text _Character_PowerText;
	public TMP_Text _Character_HPText;
	public TMP_Text _Character_AttackText;
	public TMP_Text _Character_DefenceText;
	public TMP_Text _Character_CritChanceText;
	public UI_Button _Character_AchievementButton, _Character_TroopEquipButton;
	public GameObject _Character_TroopAlreadyText;
	public Sprite _Character_TabActiveSprite;

	CharacterName _Char_CharacterName;
	List<UI_Character_Slot> _Char_Slots;
	bool _Char_TroopSelected;
	bool _Char_ShowFromMain;

	void Char_Show(bool show, CharacterName selected = CharacterName.Lin)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			if (_Char_ShowFromMain)
			{
				ViewSetting(ViewType.Main);
				Main_Refresh();
			}
			else
			{
				ViewSetting(ViewType.Troop);
				Troop_Refresh();
			}
			_Characters.SetActive(false);
			return;
		}

		// 메인, 편성 분기
		_Character_AchievementButton.gameObject.SetActive(_Char_ShowFromMain);
		_Character_TroopEquipButton.gameObject.SetActive(!_Char_ShowFromMain);
		_Character_TroopAlreadyText.gameObject.SetActive(!_Char_ShowFromMain);

		ViewSetting(ViewType.Character);
		_Characters.SetActive(true);
		_Char_CharacterName = selected;
		UI.FadeIn(0.3f);
		Char_Refresh();
	}

	void Char_Refresh()
	{
		// 캐릭터 리스트
		_Char_Slots.DestroyElements();
		foreach (CharInfo info in _CharInfos) 
		{
			CharacterName name = info._Name;
			UI_Character_Slot slot = Instantiate(_Character_SlotPrefab, _Character_SlotParent);
			slot.gameObject.SetActive(true);
			slot._Name = name;
			slot._Button.onClick.AddListener(() => Char_SlotButton(slot));
			slot._Thumbnail.sprite = Data._Characters.Find(x => x._Name == name)._Thumbnail;
			slot._Highlight.sprite = slot._Highlight2.sprite = info._CharacterSlotActiveSprite;
			slot._Highlight2.gameObject.SetActive(name == _Char_CharacterName);
			slot._NameText.text = name.ToString();
			slot._LevelText.text = "1";
			slot._EquipMark.SetActive(!_Char_ShowFromMain && UserData.Troop.Contains(name));
			_Char_Slots.Add(slot);
		}

		// 상세 정보
		DataManager.Character data = Data.GetCharacter(_Char_CharacterName);
		_Character_NameText.text = _Char_CharacterName.ToString();
		_Character_PowerText.text = Data.GetCharacterPower(_Char_CharacterName).ToString();
		_Character_HPText.text = data._HP.ToString();
		_Character_AttackText.text = Data.GetCharacterAttack(_Char_CharacterName).ToString();
		_Character_DefenceText.text = data._Defence.ToString();
		_Character_CritChanceText.text = Data.GetCharacterCritChance(_Char_CharacterName).ToPercentString();
		Util.PanelTween(_Character_RightMenu, Vector2.right);

		// 메인에서 진입 시
		if (_Char_ShowFromMain)
		{
			_Char_Slots.Sort((a, b) => a._Name - b._Name);
		}
		// 편성에서 진입 시
		else
		{
			bool already = UserData.Troop.Contains(_Char_CharacterName);
			_Character_TroopEquipButton.gameObject.SetActive(!already);
			_Character_TroopAlreadyText.gameObject.SetActive(already);
			_Char_Slots.Sort((a, b) =>
			{
				int a1 = (int)a._Name;
				int b1 = (int)b._Name;
				if (UserData.Troop.Contains(a._Name)) a1 -= 10000;
				if (UserData.Troop.Contains(b._Name)) b1 -= 10000;
				return a1 - b1;
			});
		}

		// 정렬
		foreach (UI_Character_Slot slot in _Char_Slots)
		{
			slot.transform.SetAsLastSibling();
		}

		// 모델
		HideModels();
		ShowModel(ViewType.Character, _Char_CharacterName);
		CharInfo charInfo = GetCharInfo(_Char_CharacterName);
		if (charInfo._HasGunPose)
		{
			charInfo.GetAnimator().Play("GunPose", 0, 0f);
		}
	}

	void Char_SlotButton(UI_Character_Slot slot)
	{
		if (_Char_CharacterName == slot._Name) return;

		_Char_CharacterName = slot._Name;
		Char_Refresh();
	}

	void CharacterDrag(ViewType viewType, PointerEventData eventData)
	{
		Transform charPos = GetModelParent(viewType);
		float amount = eventData.delta.x / Screen.width * 1000f;
		charPos.eulerAngles = charPos.eulerAngles.WithY(charPos.eulerAngles.y - amount);
	}

	void CharacterClick()
	{
		if (_Message.gameObject.activeSelf) return;
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			Sound.PlaySfx(SoundManager.SfxType.Click);
			_Message.gameObject.SetActive(true);
			_Message.GetComponentInChildren<TMP_Text>().TextColorTween(0.3f);
			_Message.alpha = 0.5f;
			yield return _Message.DOFade(1f, 0.3f).WaitForCompletion();
			yield return new WaitForSeconds(3f);
			yield return _Message.DOFade(0f, 1.2f).WaitForCompletion();
			_Message.gameObject.SetActive(false);
		}
	}

	void Char_TroopEquipButton()
	{
		_Char_TroopSelected = true;
		Char_Show(false);
	}
}
