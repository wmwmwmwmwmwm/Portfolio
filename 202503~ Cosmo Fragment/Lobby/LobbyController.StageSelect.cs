using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static SingletonManager;

public partial class LobbyController
{
	[Header("스테이지 선택")]
	public GameObject _StageSelect;
	public List<UI_Stage_Element> _Stage_StageElements;
	public CanvasGroup _Stage_StageInfo;
	public UI_Button _Stage_EnterButton;
	public Sprite _Stage_ElementActiveSprite;

	UI_Stage_Element _Stage_SelectedElement;

	void Stage_Show(bool show)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
            Game._IsSingleMode = true;
            ViewSetting(ViewType.Main);
			Main_Refresh();
			_StageSelect.SetActive(false);
			return;
		}

        Game._IsSingleMode = true;
        ViewSetting(ViewType.Stage);
		_StageSelect.SetActive(true);
		_Stage_SelectedElement = null;
		UI.FadeIn(0.3f);
		Stage_Refresh();
	}

	void Stage_Refresh()
	{
		for (int i = 0; i < _Stage_StageElements.Count; i++)
		{
			UI_Stage_Element element = _Stage_StageElements[i];
			element._Button.onClick.RemoveAllListeners();
			element._Button.onClick.AddListener(() => Stage_ElementButton(element));
			element._Button.enabled = !element._Lock;
			element._Unlocked.SetActive(!element._Lock);
			element._Locked.SetActive(element._Lock);
			element._Normal.overrideSprite = element == _Stage_SelectedElement ? _Stage_ElementActiveSprite : null;
			int stageNumber = i + 1;
			DataManager.Stage stage = Data._Stages.Find(x => x._StageNumber == stageNumber);
			element._Stage = stage;
		}

		// 스테이지 정보패널
		Util.PanelTween(_Stage_StageInfo, Vector2.right);
		_Stage_StageInfo.gameObject.SetActive(_Stage_SelectedElement);
	}

	void Stage_ElementButton(UI_Stage_Element element)
	{
		if (_Stage_SelectedElement == element) return;

		_Stage_SelectedElement = element;
		RectTransform rt = _Stage_StageInfo.GetComponent<RectTransform>();
		rt.anchoredPosition = rt.anchoredPosition.WithX(1500f);
		rt.DOAnchorPosX(0f, 0.6f).SetEase(Ease.OutCubic);
		Sound.PlaySfx(SoundManager.SfxType.Slide);
		Stage_Refresh();
	}

	void Stage_EnterButton()
	{
		Troop_Show(true);
	}
}
