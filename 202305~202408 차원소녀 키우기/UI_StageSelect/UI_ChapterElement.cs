using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChapterElement : MonoBehaviour
{
    [SerializeField] Transform _movePivot;
	[SerializeField] GameObject _on, _off;
    [SerializeField] UIPlayTween _sizeTweener;

    StageData _info;
	GameObject _activeIcon;
    bool _selected;

    public int ChapterNumber => Info != null ? Info.Chapter : -1;
	TMP_Text ChapterText => _activeIcon.transform.Find("Text").GetComponent<TMP_Text>();
    public StageData Info { get => _info; set => _info = value; }

    public void Setting(bool selected)
    {
        bool changed = selected != _selected;
        _selected = selected;
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (_info == null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            return;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
		_activeIcon = _selected ? _on : _off;
        _on.SetActive(_selected);
        _off.SetActive(!_selected);
        ChapterText.text = $"{LocalizeManager.GetText(_info.Nametextid)} {ChapterNumber}";
        if (changed)
        {
            _sizeTweener.Play(_selected);
        }
    }
}