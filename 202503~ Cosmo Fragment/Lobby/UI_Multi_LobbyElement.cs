using Sirenix.OdinInspector;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Multi_LobbyElement : MonoBehaviour 
{
	public UI_Button _Button;
	public TMP_Text _Name, _PlayerCount;

	[ReadOnly] public Lobby _Lobby;
}
