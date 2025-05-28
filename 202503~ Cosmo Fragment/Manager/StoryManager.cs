using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using System.Collections;
using static SingletonManager;
using System;
using GameCreator.Core.Hooks;
using Naninovel.Commands;

public class StoryManager : Singleton<StoryManager>
{
	public Canvas _Canvas;
	public RawImage _BlurBackground;
	public UI_Button _BgButton;
	public UI_Button _SkipButton;
	public RenderTexture _RenderTexture;

	Script _Script;
	bool _CanInput;
	bool _IsBattle;

	protected override void Init()
	{
		_Canvas.gameObject.SetActive(false);
		_BgButton.onClick.AddListener(Next);
		_SkipButton.onClick.AddListener(SkipButton);
		Game.RegisterEvent_ResolutionChange(gameObject, ResizeTexture);
		ResizeTexture();
	}

	void Update()
	{
		// 배경 블러
		if (_BlurBackground.gameObject.activeSelf && !_Canvas.worldCamera) 
		{
			GameObject cam = GameObject.Find("CustomCamera");
			if (cam)
			{
				_Canvas.worldCamera = cam.GetComponent<Camera>();
			}
		}

		UpdatePosition();
		UpdateInput();
	}

	void UpdateInput()
	{
		if (!_CanInput) return;

		if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
		{
			Next();
		}
	}

	void UpdatePosition()
	{
		Transform parent = Engine.RootObject.transform.Find("Character");
		if (!parent) return;

		parent.localPosition = _IsBattle ? Vector3.down * 1000f : Vector3.zero;
	}

	public IEnumerator Play(Script script, string label, bool isBattle)
	{
		// 이전 스토리 보이스 재생되는 이슈
		AudioSource[] audioSources = Engine.RootObject.GetComponentsInChildren<AudioSource>();
		foreach (AudioSource audioSource in audioSources)
		{
			audioSource.playOnAwake = false;
		}

		// 재생
		_Script = script;
		_IsBattle = isBattle;
		_BlurBackground.gameObject.SetActive(isBattle);
		yield return new WaitUntil(() => Engine.Initialized);
		Engine.RootObject.SetActive(true);
		_Canvas.gameObject.SetActive(true);
		_CanInput = true;
		IScriptPlayer scriptPlayer = Engine.GetService<IScriptPlayer>();
		if (!string.IsNullOrEmpty(label))
		{
			yield return scriptPlayer.PreloadAndPlayAsync(_Script, label).ToCoroutine();
		}
		else
		{
			yield return scriptPlayer.PreloadAndPlayAsync(_Script).ToCoroutine();
		}
		yield return new WaitUntil(() => !scriptPlayer.Playing);

		// 복귀 연출
		UI.FadeOut(2f);
		yield return new WaitForSeconds(2f);
		HidePrinter hidePrinter = new();
		yield return hidePrinter.ExecuteAsync().ToCoroutine();
		HideAllCharacters hideAllChars = new();
		yield return hideAllChars.ExecuteAsync().ToCoroutine();
		StopVoice stopVoice = new();
		yield return stopVoice.ExecuteAsync().ToCoroutine();
		Next();
		UI.ShowLoading(true);

		Engine.RootObject.SetActive(false);
		_Canvas.gameObject.SetActive(false);
		_CanInput = false;
	}

	void Next()
	{
		Engine.GetService<IInputManager>().GetContinue().Activate(1f);
	}

	public IEnumerator SetupBlurBackground()
	{
		// 배경 블러
		Camera cam = HookCamera.Instance.Get<Camera>();
		cam.targetTexture = _RenderTexture;
		yield return null;
		cam.targetTexture = null;
	}

	void SkipButton()
	{
		ScriptPlayer player = Engine.GetService<ScriptPlayer>();
		player.Resume(player.Playlist.Count - 1);
		player.Stop();
	}

	void ResizeTexture()
	{
		_RenderTexture.Release();
		_RenderTexture.width = Screen.width / 2;
		_RenderTexture.height = Screen.height / 2;
		_RenderTexture.Create();
	}
}
