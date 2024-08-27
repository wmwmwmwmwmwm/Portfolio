using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tower_FloorElement : MonoBehaviour
{
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] UI_Button _button;
    [SerializeField] GameObject _lineClear, _lineLock;
    [SerializeField] Image _bg;
    [SerializeField] TMP_Text _floorText, _descText;
    [SerializeField] Image _reward;
    [SerializeField] TMP_Text _rewardCount;
    [SerializeField] GameObject _clearMark, _lock, _dim, _selected;
    //[SerializeField] GameObject _selected, _current, _lock;
    [SerializeField] Sprite _bgClear, _bgCurrent, _bgLock;
    [SerializeField] Color _floorTextClearColor, _floorTextCurrentColor, _floorTextLockedColor;
    [SerializeField] Color _descTextClearColor, _descTextCurrentColor, _descTextLockedColor;

    public enum State
    {
        Clear, Current, Locked
    }

    public void Setting(StageInfo stage, int floor, bool selected, bool isLast, State state)
    {
        _canvasGroup.alpha = floor > 0 ? 1f : 0f;
        if (floor == 0) return;

        _button.SetID($"{UI_Tower.EButtonId.FloorElement}_{floor}");
        _lineClear.SetActive(!isLast && state == State.Clear);
        _lineLock.SetActive(!isLast && state != State.Clear);
        _bg.sprite = state switch
        {
            State.Clear => _bgClear,
            State.Current => _bgCurrent,
            _ => _bgLock,
        };
        _floorText.text = LocalizeManager.GetText("UI_Tower_Floor", floor);
        _floorText.color = state switch
        {
            State.Clear => _floorTextClearColor,
            State.Current => _floorTextCurrentColor,
            _ => _floorTextLockedColor,
        };
        _descText.text = LocalizeManager.GetText(state switch
        {
            State.Clear => "UI_Tower_Clear",
            State.Current => "UI_Tower_CanBeEnter",
            _ => "UI_Tower_CanNotEnter",
        });
        _descText.color = state switch
        {
            State.Clear => _descTextClearColor,
            State.Current => _descTextCurrentColor,
            _ => _descTextLockedColor,
        };
        _reward.gameObject.SetActive(state != State.Clear);
        (int rewardItemId, BigInteger count) = stage.GetRewardList().First();
        _reward.sprite = AtlasManager.GetItemIcon(rewardItemId, EIconType.Icon);
        _reward.GetComponent<UI_Button>().SetID($"{UI_Tower.EButtonId.RewardTooltip}_{floor}");
        _rewardCount.text = Formula.NumberToStringBy3(count);
        _clearMark.SetActive(state == State.Clear);
        _lock.SetActive(state == State.Locked);
        _dim.SetActive(state == State.Locked);
        _selected.SetActive(selected);
        //_selected.SetActive(selected);
        //_current.SetActive(isCurrent); 
        //_lock.SetActive(locked);
    }
}
