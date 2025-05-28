using UnityEngine;
using static SoundManager;

public class UI_ButtonExpansion : MonoBehaviour 
{
	public SfxType _HoverSound;
	public SfxType _ClickSound;

	void Reset()
	{
		_HoverSound = SfxType.MouseOver;
		_ClickSound = SfxType.Click;
	}
}
