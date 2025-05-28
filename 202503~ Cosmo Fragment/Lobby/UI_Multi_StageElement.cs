using Sirenix.OdinInspector;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Multi_StageElement : MonoBehaviour 
{
	public UI_Button _Button;
	public GameObject _Selected;
	public TMP_Text _Name;

	[HideInInspector] public DataManager.Stage _Stage;
}
