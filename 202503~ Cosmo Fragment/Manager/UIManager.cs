using DG.Tweening;
using GameCreator.Core;
using GameCreator.Variables;
using Naninovel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SingletonManager;

public class UIManager : Singleton<UIManager>, IBackButton
{
	public CanvasGroup _Fader;
	public CanvasGroup _NetworkLoading;
	//public UI_Config _Config;
	public GameObject _Config;
	public BattleMessage _BattleMessage;
	public GameObject _CinematicOverlay;
	public Image _HotKey;

	[Header("로딩")]
	public CanvasGroup _Loading;
	public Image _Loading_Image1, _Loading_Image2;

	[Header("원버튼 팝업")]
	public GameObject _OneButtonPopup;
	public TMP_Text _OneButton_Text;
	public Button _OneButton_OKButton;

	[Header("투버튼 팝업")]
	public GameObject _TwoButtonPopup;
	public TMP_Text _TwoButton_TitleText, _TwoButton_OKText, _TwoButton_CancelText;
	public Button _TwoButton_OKButton, _TwoButton_CancelButton;

	[Header("ESC 메뉴")]
	public GameObject _EscMenu;
	public UI_Button _Esc_BackButton, _Esc_OptionButton, _Esc_TitleButton, _Esc_ExitButton;

	bool _OneButton_Trigger;
	bool _TwoButton_Trigger;
	[HideInInspector] public bool _TwoButton_Result;

	protected override void Init()
	{
		_Loading.gameObject.SetActive(false);
		_NetworkLoading.gameObject.SetActive(false);
		_OneButtonPopup.SetActive(false);
		_TwoButtonPopup.SetActive(false);
		_Config.gameObject.SetActive(false);
		_BattleMessage.gameObject.SetActive(false);
		_CinematicOverlay.SetActive(false);
		_EscMenu.SetActive(false);
		_HotKey.gameObject.SetActive(false);
		_OneButton_OKButton.onClick.AddListener(OneButton_OKButton);
		_TwoButton_OKButton.onClick.AddListener(TwoButton_OKButton);
		_TwoButton_CancelButton.onClick.AddListener(TwoButton_CancelButton);
		_Esc_BackButton.onClick.AddListener(Esc_BackButton);
		_Esc_OptionButton.onClick.AddListener(Esc_OptionButton);
		_Esc_TitleButton.onClick.AddListener(Esc_TitleButton);
		_Esc_ExitButton.onClick.AddListener(Esc_ExitButton);

		_Fader.gameObject.SetActive(true);
		_Fader.alpha = 0f;
	}

	void Update()
	{
		UpdateHotKey();
	}

	void UpdateHotKey()
	{
		if (Game.CurrentSceneName != SceneName.Battle1 && Game.CurrentSceneName != SceneName.Battle2) return;
		_HotKey.gameObject.SetActive(Input.GetKey(KeyCode.K));
	}

	public bool OnBackButton()
	{
		if (_TwoButtonPopup.activeSelf)
		{
			_TwoButton_Trigger = true;
			return true;
		}
		else if (_TwoButtonPopup.activeSelf)
		{
			_TwoButton_Trigger = true;
			return true;
		}
		else if (_Config.gameObject.activeSelf)
		{
			//_Config.Show(false);
			if (GlobalVariablesManager.Instance)
			{
				SaveLoadManager.Instance.SingleSave(GlobalVariablesManager.Instance, 0);
			}
			_Config.gameObject.SetActive(false);

			return true;
		}
		else if (_EscMenu.activeSelf)
		{
			ShowEscMenu(false);
			return true;
		}
		return false;
	}

	public void FadeIn(float time = 0.6f)
	{
		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			_Fader.DOComplete();
			_Fader.alpha = 1f;
			yield return _Fader.DOFade(0f, time).SetEase(Ease.InQuad).WaitForCompletion();
		}
	}

	public void FadeOut(float time = 1.2f)
	{
		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			_Fader.DOComplete();
			_Fader.alpha = 0f;
			yield return _Fader.DOFade(1f, time).SetEase(Ease.OutQuad).WaitForCompletion();
		}
	}

	public void ShowLoading(bool show, float fadeTime = 1.2f, bool image2 = false)
	{
		_Loading.gameObject.SetActive(show);
		_Loading_Image1.gameObject.SetActive(!image2);
		_Loading_Image2.gameObject.SetActive(image2);
		if (!show)
		{
			FadeIn(fadeTime);
		}
	}

	public void ShowNetworkLoading(bool show)
	{
		float fadeTime = 0.3f;
		if (show)
		{
			_NetworkLoading.gameObject.SetActive(true);
			_NetworkLoading.alpha = 0f;
			_NetworkLoading.DOFade(1f, fadeTime);
		}
		else
		{
			_NetworkLoading.DOFade(0f, fadeTime).OnComplete(() => _NetworkLoading.gameObject.SetActive(false));
		}
	}

	public IEnumerator ShowOneButtonPopup(string text)
	{
		_OneButton_Text.text = text;

		_OneButtonPopup.SetActive(true);
		_OneButton_Trigger = false;
		yield return new WaitUntil(() => _OneButton_Trigger);
		_OneButtonPopup.SetActive(false);
	}

	void OneButton_OKButton()
	{
		_OneButton_Trigger = true;
	}

	public IEnumerator ShowTwoButtonPopup(string title, string okText, string cancelText)
	{
		_TwoButton_TitleText.text = title;
		_TwoButton_OKText.text = okText;
		_TwoButton_CancelText.text = cancelText;

		_TwoButtonPopup.SetActive(true);
		_TwoButton_Trigger = false;
		yield return new WaitUntil(() => _TwoButton_Trigger);
		_TwoButtonPopup.SetActive(false);
	}

	void TwoButton_OKButton()
	{
		_TwoButton_Trigger = true;
		_TwoButton_Result = true;
	}

	void TwoButton_CancelButton()
	{
		_TwoButton_Trigger = true;
		_TwoButton_Result = false;
	}

	public void ShowEscMenu(bool show)
	{
		if (!show)
		{
			Time.timeScale = 1f;
			_EscMenu.SetActive(false);
			return;
		}

		Time.timeScale = 0f;
		_EscMenu.SetActive(true);
	}

	void Esc_BackButton()
	{
		ShowEscMenu(false);
	}

	void Esc_OptionButton()
	{
		UI._Config.SetActive(true);
	}

	void Esc_TitleButton()
	{
		ShowEscMenu(false);
		Game.LoadTitleScene();
	}

	void Esc_ExitButton()
	{
		ShowEscMenu(false);
		Game.LoadLobbyScene();
	}
}
