using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using static SingletonManager;

public partial class IntroController : SingleInstance<IntroController>, IBackButton
{
	public VideoPlayer _IntroVideo;
	public Button _IntroSkipButton;

	bool _IntroSkip;

	void Start()
	{
		_IntroSkipButton.onClick.AddListener(IntroSkipButton);

		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			_IntroVideo.Play();
			yield return new WaitUntil(() => _IntroVideo.isPlaying);
			yield return new WaitUntil(() => !_IntroVideo.isPlaying || _IntroSkip);
			UI.FadeOut(2f);
			yield return new WaitForSeconds(2f);
			SceneManager.LoadScene(SceneName.Title);
		}
	}

	public bool OnBackButton()
	{
		IntroSkipButton();
		return true;
	}

	void IntroSkipButton()
	{
		_IntroSkip = true;
	}
}
