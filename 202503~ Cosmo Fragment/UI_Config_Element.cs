using GameCreator.Variables;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SingletonManager;

public class UI_Config_Element : MonoBehaviour 
{
	public enum ElementType { List, Toggle, Slider }
	public ElementType _Type;

	public UI_Button _Button;
	public GameObject _Select;
	public GameObject _ListGroup;
	public ListVariables _List;
	public GameObject _ToggleGroup;
	public UI_Button _Toggle;
	public GameObject _SliderGroup;
	public Slider _Slider;

	void Start()
	{
		_Button.interactable = _Type == ElementType.List;
		_Button.transition = _Type == ElementType.List ? Selectable.Transition.ColorTint : Selectable.Transition.None;
		_Select.SetActive(_Type == ElementType.List);
		_ListGroup.SetActive(_Type == ElementType.List);
		_ToggleGroup.SetActive(_Type == ElementType.Toggle);
		_SliderGroup.SetActive(_Type == ElementType.Slider);
	}
}
