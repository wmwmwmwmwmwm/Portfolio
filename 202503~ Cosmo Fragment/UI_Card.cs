using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SingletonManager;

public class UI_Card : MonoBehaviour 
{
	public UI_Button _Button;
	public GameObject _Empty, _Content;
	public GameObject _Red, _Blue, _Gray;
	public GameObject _Selected, _Disabled;

	[ReadOnly] public CardName _CardName;

	public void InitEmpty() => Init((CardName)(-1));
	public void Init(CardName cardName)
	{
		_CardName = cardName;
		_Selected.SetActive(false);
		_Disabled.SetActive(false);
		bool active = _CardName >= 0;
		_Empty.SetActive(!active);
		_Content.SetActive(active);
		if (!active) return;

		DataManager.Card cardInfo = Data.GetCard(cardName);
		_Red.SetActive(cardInfo._Type == CardType.Fixed);
		_Blue.SetActive(cardInfo._Type == CardType.Common);
		_Gray.SetActive(false);
		GameObject content = cardInfo._Type switch
		{
			CardType.Fixed => _Red,
			_ => _Blue
		};
		//content.transform.Find("Icon").GetComponent<Image>().sprite =;
		content.transform.Find("Name").GetComponent<TMP_Text>().text = cardInfo._DisplayName;
		content.transform.Find("Desc").GetComponent<TMP_Text>().text = cardInfo._Desc;
	}

	public void Select(bool select)
	{
		_Selected.SetActive(select);
	}

	//public void Disable(bool disable)
	//{
	//	_Button.interactable = !disable;
	//	_Disabled.SetActive(disable);
	//}
}
