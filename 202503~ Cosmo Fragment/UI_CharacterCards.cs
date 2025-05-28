using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SingletonManager;

public class UI_CharacterCards : MonoBehaviour 
{
	public Image _Portrait;
	public TMP_Text _NameText;
	public List<UI_Card> _Cards;

	[ReadOnly] public CharacterName _CharacterName;

	public void Init(CharacterName charName)
	{
		_CharacterName = charName;
		_Portrait.sprite = Data._Characters[(int)_CharacterName]._Thumbnail;
		_NameText.text = charName.ToString();
		UserInfoManager.SaveData.CharacterInfo charInfo = UserData.GetCharacterInfo(charName);
		for (int slotIndex = 0; slotIndex < _Cards.Count; slotIndex++)
		{
			UI_Card card = _Cards[slotIndex];
			if (slotIndex < charInfo.EquippedCards.Count)
			{
				card.Init(charInfo.EquippedCards[slotIndex]);
			}
			else
			{
				card.InitEmpty();
			}
		}
	}
}
