using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Character_Slot : MonoBehaviour 
{
	public UI_Button _Button;
	public Image _Thumbnail;
	public Image _Highlight;
	public Image _Highlight2;
	public TMP_Text _NameText;
	public TMP_Text _LevelText;
	public GameObject _EquipMark;

	[HideInInspector] public CharacterName _Name;
}
