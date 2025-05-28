using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stage_Element : MonoBehaviour 
{
	public UI_Button _Button;
	public Image _Normal;
	public GameObject _Unlocked, _Locked;
	public bool _Lock;

	[HideInInspector] public DataManager.Stage _Stage;
}
