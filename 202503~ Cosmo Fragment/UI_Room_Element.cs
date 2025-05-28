using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Room_Element : MonoBehaviour 
{
	public UI_Button _Button;
	public Image _Icon;
	public TMP_Text _Name;
	public GameObject _Selected;
	public GameObject _RedDot;

	[HideInInspector] public CharacterName _CharacterName;
	[HideInInspector] public string _ClothName;
}
