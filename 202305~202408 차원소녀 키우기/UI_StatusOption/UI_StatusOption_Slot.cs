using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;

public class UI_StatusOption_Slot : MonoBehaviour
{
    public GameObject OpenedSlotPanel;
    public GameObject NotOpendSlotPanel;
    public GameObject OptionLockPanel;
    public GameObject OptionDescPanel;
    public GameObject OptionNotAssignedText;

    public Image NoneCover;
    public Image GradeCover, Icon2;

    public TextMeshProUGUI OpenConditionText;
    public TextMeshProUGUI StatDesc;
    public TextMeshProUGUI ValueDesc;

    public UI_Button Button;
    public Animation AnimationComponent;
    public GameObject UnlockAlarm, UnlockNotice;
    public Transform EffectPosition;

    [HideInInspector] public bool UnlockTrigger;
    int _category, _slot;
    OptionSlotInfoItem _data;
    Coroutine _noticeCoroutine;
    UI_StatusOption _parent;

    public string ButtonID
    {
        get => Button.ID;
        set => Button.SetID(value);
    }
    public bool UnlockWaiting => _data.Open && !_data.Notice;

    StatusOptionManager OptionManager => StatusOptionManager.Instance;

    public void RefreshSlot(UI_StatusOption parent, int category, int slot)
    {
        _parent = parent;
        _category = category;
        _slot = slot;
        _data = OptionManager.GetSlot(_category, _slot);
        UnlockAlarm.SetActive(UnlockWaiting);
        UnlockNotice.SetActive(false);
        if (_noticeCoroutine != null)
        {
            StopCoroutine(_noticeCoroutine);
            _noticeCoroutine = null;
        }
        if (UnlockWaiting)
        {
            _noticeCoroutine = StartCoroutine(NoticeCoroutine());
        }
        SetOpen(_data.Open);
        SetLock(_data.Lock);
        SetGrade(_data.Option);
        SetDesc(_data);
        SetOpenConditionText(OptionManager.GetSlotData(_category, _slot));
    }

    public IEnumerator NoticeCoroutine()
    {
        // 해금 애니메이션
        yield return new WaitUntil(() => UnlockTrigger);
        UnlockTrigger = false;
        StatusOptionManager.Instance.SetNotice(_data);
        UnlockAlarm.SetActive(false);
        UnlockNotice.SetActive(true);
        AnimationComponent.Play();
        yield return new WaitForSecondsRealtime(AnimationComponent.GetFirst().length);
        UnlockNotice.SetActive(false);
        _parent.RefreshGachaCost();
        _noticeCoroutine = null;
    }

    void SetOpen(bool isOpen)
    {
        OpenedSlotPanel.SetActive(isOpen);
        NotOpendSlotPanel.SetActive(!isOpen);
    }

    void SetLock(bool isLock)
    {
        OptionLockPanel.SetActive(isLock);
    }

    void SetGrade(OptionInfo option)
    {
        EItemGrade grade = option == null ? EItemGrade.NONE : option.Grade;
        bool active = grade > EItemGrade.NONE;
        GradeCover.gameObject.SetActive(active);
        NoneCover.gameObject.SetActive(!active);
        if (active)
        {
            GradeCover.sprite = _parent.GetCoverSprites(grade);
            Icon2.sprite = _parent.GetIconSprites(grade);
        }
    }

    void SetDesc(OptionSlotInfoItem option)
    {
        OptionNotAssignedText.SetActive(!option.HasOption);
        OptionDescPanel.SetActive(option.HasOption);
        if (!option.HasOption)
        {
            StatDesc.text = ValueDesc.text = "";
            return;
        }
        StatValueInfo statValue = new(option.Option.Stat, option.Option.Value, option.Option.OptionData.Calculation);
        StatDesc.text = statValue.StatString;
        ValueDesc.text = '+' + statValue.ValueString;
    }

    void SetOpenConditionText(FCharacterOptionSlot data)
    {
        OpenConditionText.text = LocalizeManager.GetText("SLOT_OPEN_CONDITION_" + data.Slotopencondition.ToString(), data.Slotopenconditionvalue);
    }
}
