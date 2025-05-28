using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static SingletonManager;

public partial class LobbyController
{
	[Header("카드")]
	public GameObject _Cards;
	public GameObject _Cards_Character, _Cards_Info;
	public List<UI_CharacterCards> _Cards_Chars;
	public TMP_Text _Cards_NameText, _Cards_DescText;
	public UI_Card _Cards_CardPrefab;
	public Transform _Cards_CardParent;
	public UI_Button _Cards_EnterButton;

	List<UI_Card> _Cards_Cards;
	(CharacterName charName, int index) _Card_SelectedCardSlot;
	UI_Card _Cards_SelectedCharacterCard, _Cards_SelectedInventoryCard;
	bool _Cards_EquipMode;

	void Cards_Show(bool show, bool isEquipMode)
	{
		if (!show)
		{
			if (isEquipMode)
			{
				ViewSetting(ViewType.Troop);
				Troop_Refresh();
			}
			else
			{
				ViewSetting(ViewType.Main);
				Main_Refresh();
			}
			_Cards.SetActive(false);
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			return;
		}

		ViewSetting(ViewType.Cards);
		_Cards_EquipMode = isEquipMode;
		_Cards_Character.SetActive(isEquipMode);
		_Cards_Info.SetActive(!isEquipMode);
		_Cards_EnterButton.gameObject.SetActive(isEquipMode);
		_Cards.SetActive(true);
		UI.FadeIn(0.3f);
		Cards_Refresh();
	}

	void Cards_Refresh()
	{
		if (_Cards_EquipMode)
		{
			// 캐릭터 리스트
			for (int troopIndex = 0; troopIndex < UserData.Troop.Count; troopIndex++)
			{
				CharacterName charName = UserData.Troop[troopIndex];
				bool active = charName >= 0;
				UI_CharacterCards charCards = _Cards_Chars[troopIndex];
				charCards.gameObject.SetActive(active);
				if (!active) continue;

				charCards.Init(charName);
				for (int slotIndex = 0; slotIndex < charCards._Cards.Count; slotIndex++)
				{
					UI_Card card = charCards._Cards[slotIndex];
					int slotIndexTemp = slotIndex;
					card._Button.onClick.RemoveAllListeners();
					card._Button.onClick.AddListener(() => Cards_CardButton(card, charName, slotIndexTemp));
				}
			}
		}
		else
		{
			_Cards_Info.SetActive(_Cards_SelectedInventoryCard);
			if (_Cards_SelectedInventoryCard)
			{
				// 카드 설명
				DataManager.Card cardInfo = Data.GetCard(_Cards_SelectedInventoryCard._CardName);
				_Cards_NameText.text = cardInfo._DisplayName;
				_Cards_DescText.text = cardInfo._Desc;
			}
		}

		// 카드 리스트
		_Cards_Cards.DestroyElements();
		foreach (CardName cardName in UserData._Cards)
		{
			UI_Card card = Instantiate(_Cards_CardPrefab, _Cards_CardParent);
			card.Init(cardName);
			card._Button.onClick.AddListener(() => Cards_CardButton(card, (CharacterName)(-1), -1));
			//bool already = UserData._CharacterInfos.Any(x => x._EquippedCards.Contains(cardName));
			//card.Disable(already);
			card.gameObject.SetActive(true);
			_Cards_Cards.Add(card);
		}
	}

	void Cards_CardButton(UI_Card card, CharacterName charName, int slotIndex)
	{
		(CharacterName charName, int index) lastSelectedCharSlot = _Cards_SelectedCharacterCard ? _Card_SelectedCardSlot : ((CharacterName)(-1), -1);
		bool isCharSlotSelected = charName >= 0;
		if (isCharSlotSelected)
		{
			_Card_SelectedCardSlot = (charName, slotIndex);
			SelectCard(ref _Cards_SelectedCharacterCard);
		}
		else
		{
			SelectCard(ref _Cards_SelectedInventoryCard);
		}

		void SelectCard(ref UI_Card selectedCard)
		{
			// 이미 선택된 카드 선택 시
			if (selectedCard == card)
			{
				selectedCard.Select(false);
				selectedCard = null;
				return;
			}
			// 다른 카드는 선택 취소
			else if (selectedCard)
			{
				selectedCard.Select(false);
			}

			selectedCard = card;
			selectedCard.Select(true);
		}

		// 카드편성 UI
		if (_Cards_EquipMode)
		{
			// 둘 다 선택됐으면 장착
			if (_Cards_SelectedCharacterCard && _Cards_SelectedInventoryCard)
			{
				List<CardName> userCharCards = UserData.GetCharacterInfo(_Card_SelectedCardSlot.charName).EquippedCards;
				userCharCards[_Card_SelectedCardSlot.index] = _Cards_SelectedInventoryCard._CardName;
				ResetSelects();
				Cards_Refresh();
			}
			// 캐릭터 슬롯만 2개 선택 시 스왑
			else if (isCharSlotSelected && lastSelectedCharSlot.charName >= 0)
			{
				List<CardName> userCharCards1 = UserData.GetCharacterInfo(lastSelectedCharSlot.charName).EquippedCards;
				List<CardName> userCharCards2 = UserData.GetCharacterInfo(_Card_SelectedCardSlot.charName).EquippedCards;
				(userCharCards1[lastSelectedCharSlot.index], userCharCards2[_Card_SelectedCardSlot.index]) = (userCharCards2[_Card_SelectedCardSlot.index], userCharCards1[lastSelectedCharSlot.index]);
				ResetSelects();
				Cards_Refresh();
			}

			void ResetSelects()
			{
				if (_Cards_SelectedCharacterCard)
				{
					_Cards_SelectedCharacterCard.Select(false);
					_Cards_SelectedCharacterCard = null;
				}
				if (_Cards_SelectedInventoryCard)
				{
					_Cards_SelectedInventoryCard.Select(false);
					_Cards_SelectedInventoryCard = null;
				}
			}
		}
		// 아이템 UI
		else
		{
			_Cards_SelectedInventoryCard = card;
			Cards_Refresh();
		}
	}

	void Cards_EnterButton()
	{
		// 싱글에서는 전투 시작
		if (Game._IsSingleMode)
		{
			Game.LoadBattleScene(_Stage_SelectedElement._Stage._SceneName, _Stage_SelectedElement._Stage._StageNumber == 1);
		}
		// 멀티에서는 편성 완료
		else
		{
			Cards_Show(false, _Cards_EquipMode);
			Troop_Show(false);
		}
	}
}
