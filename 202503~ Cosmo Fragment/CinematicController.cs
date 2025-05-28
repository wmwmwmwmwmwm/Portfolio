using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static SingletonManager;

public class CinematicController : SingleInstance<CinematicController>, IBackButton
{
	[HideInInspector] public bool _Skip;

	public bool OnBackButton()
	{
		if (!_Skip)
		{
			_Skip = true;
			return true;
		}
		return false;
	}
}
