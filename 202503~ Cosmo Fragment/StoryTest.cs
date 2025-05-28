using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using System.Collections;
using static SingletonManager;

public class StoryTest : MonoBehaviour
{
	public Script _Script;
	public string _Label;

	void Start()
	{
		if (Game._StartSceneName != SceneName.Story) return;
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			yield return StartCoroutine(Story.Play(_Script, _Label, false));
		}
	}
}
