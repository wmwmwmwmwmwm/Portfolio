using Sirenix.OdinInspector;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Multi_MemberElement : MonoBehaviour 
{
	public TMP_Text _Name, _Troop, _State;

	[ReadOnly] public Friend _Friend;
}
