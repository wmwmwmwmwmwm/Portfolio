using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SingletonManager;

public partial class LobbyController
{
	[Header("상점")]
	public GameObject _Store;
	public UI_Button _Store_HomeButton;
	public Transform _Store_CharacterPosition;
	public UI_Button _Store_LeftButton, _Store_RightButton;
	public UI_Button _Store_LevelUpButton;
	public List<UI_Card> _Store_Cards;
	public TMP_Text _Store_Gold, _Store_Chip, _Store_LevelUpCost;
	public TMP_Text _Store_Attack, _Store_Critical;
	public List<GameObject> _Store_Solds;

	CharacterName _Store_CharacterName;

	void Store_Show(bool show)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			ViewSetting(ViewType.Main);
			Main_Refresh();
			_Store.SetActive(false);
			return;
		}

		ViewSetting(ViewType.Store);
		_Store.SetActive(true);
		UI.FadeIn(0.3f);
		Store_Refresh();
	}

	void Store_Refresh()
	{
		// 텍스트
		_Store_Gold.text = UserData._Gold.ToString();
		_Store_Chip.text = UserData._Chip.ToString();
		_Store_LevelUpCost.text = Store_GetLevelUpCost().ToString();
		_Store_Attack.text = $"ATK :   {Data.GetCharacterAttack(_Store_CharacterName)}";
		_Store_Critical.text = $"CRITICAL :   {Data.GetCharacterCritChance(_Store_CharacterName).ToPercentString()} / {Data.GetCharacterCritDamage(_Store_CharacterName).ToPercentString()}";

		// 카드
		for (int i = 0; i < _Store_Cards.Count; i++)
		{
			UI_Card card = _Store_Cards[i];
			CardName cardName = i switch
			{
				0 => CardName.BlueAttack,
				1 => CardName.BlueFireRate,
				_ => CardName.BlueReload
			};
			card.Init(cardName);
			bool sold = UserData._StoreSoldCards.Contains(card._CardName);
			card._Button.interactable = !sold;
			_Store_Solds[i].SetActive(sold);
		}

		// 모델
		HideModels();
		ShowModel(ViewType.Store, _Store_CharacterName);
	}

	void Store_CharacterChangeButton(int delta)
	{
		_Store_CharacterName += delta;
		_Store_CharacterName = (CharacterName)Util.Mod((int)_Store_CharacterName, _CharInfos.Count);
		Store_Refresh();
	}

	void Store_LevelUpButton()
	{
		UserData._Gold -= Store_GetLevelUpCost();
		User.SetWeaponLevel(_Store_CharacterName, User.GetWeaponLevel(_Store_CharacterName) + 1);
		Store_Refresh();
		RefreshCurrency();
	}

	void Store_CardButton(UI_Card card)
	{
		UserData._Chip -= 50;
		UserData._StoreSoldCards.Add(card._CardName);
		UserData._Cards.Add(card._CardName);
		Store_Refresh();
		RefreshCurrency();
	}

	int Store_GetLevelUpCost()
	{
		return 50 + User.GetWeaponLevel(_Store_CharacterName) * 25;
	}
}
