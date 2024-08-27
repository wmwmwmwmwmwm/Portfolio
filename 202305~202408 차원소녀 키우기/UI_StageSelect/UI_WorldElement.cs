using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_WorldElement : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Image _thumbnail;
	[SerializeField] TMP_Text _titleText;
	[SerializeField] Image _crystal, _checkmark, _black;
    [SerializeField] Image _current, _current2;
    [SerializeField] UIPlayTween _tweener;
	[SerializeField] Color _checkmark_Enabled, _checkmark_Disabled;
	[SerializeField] Sprite _crystal_Current, _crystal_CanEnter, _crystal_NotComplete;

	[HideInInspector] public int WorldNumber;

    public UIPlayTween Tweener => _tweener;

    public void Setting(int world, bool isCurrent, bool complete, bool canEnter)
	{
        WorldNumber = world;
        _button.SetID($"{UI_StageSelect.EButtonId.World}__{WorldNumber}");
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        FStageWorld worldData = DataManager.Instance.StageWorlds[WorldNumber];
        _thumbnail.sprite = AtlasManager.GetSprite(EATLAS_TYPE.Icon, worldData.Thumbnail);
        _titleText.text = LocalizeManager.GetText(worldData.Nametextid);
        _current.gameObject.SetActive(isCurrent);
        _current2.gameObject.SetActive(isCurrent);
        _crystal.sprite = isCurrent ? _crystal_Current : canEnter ? _crystal_CanEnter : _crystal_NotComplete;
        _checkmark.color = complete ? _checkmark_Enabled : _checkmark_Disabled;
        _black.gameObject.SetActive(!canEnter);
    }
}