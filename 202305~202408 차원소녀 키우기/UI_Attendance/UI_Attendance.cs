using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;
using System.Collections;

public class UI_Attendance : UIBase
{
    [SerializeField] TMP_Text _titleText, _descText;
    [SerializeField] TMP_Text _rightDescText;
    [SerializeField] List<UI_Attendance_ItemElement> _elements;
    [SerializeField] List<UI_Attendance_ItemElement> _elementRights;
    [SerializeField] Animation _bodyAnimation;
    [SerializeField] List<Animation> _views;

    List<UI_Attendance_ItemElement> _allElements;
    bool _pressed;
    List<AttendanceInfo> _infos;
    [NonSerialized] public bool OpenFromStart;
    Animation _currentView;

    public static void OpenUI(bool openFromStart)
    {
        if (openFromStart)
        {
            if (!GuideMissionManager.Instance.CanAttend) return;
            if (!AttendanceManager.Instance.ShowToday) return;
        }
        AttendanceManager.Instance.ShowToday = false;
        UserDataManager.Save_LocalData();
        UIManager.Instance.OpenUI(EUIName.UI_Attendance, new UICallbackInfo()
        {
            prvOpenUIWork = (uiBase, callback) =>
            {
                UI_Attendance ui = uiBase as UI_Attendance;
                ui.OpenFromStart = openFromStart;
                callback?.Invoke(true);
            },
        });
    }

    public override void InitUI()
    {
        _allElements = new();
        _allElements.AddRange(_elements);
        _allElements.AddRange(_elementRights);
        _infos = AttendanceManager.Instance.ViewAttendanceInfos;
        if (OpenFromStart)
        {
            _infos = _infos.FindAll(x => !x.IsFinish);
        }

        StartCoroutine(ShowCoroutine());
        base.InitUI();
    }

    public override bool OnBackKey()
    {
        _pressed = true;
        return false;
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback?.Invoke(true);

    protected override void RefreshUI(LanguageType langCode) { }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;
        _pressed = true;
    }

    IEnumerator ShowCoroutine()
    {
        foreach (AttendanceInfo info in _infos) 
        {
            _currentView = _views[info.Data.Viewtype - 1];
            foreach (Animation view in _views) 
            {
                view.gameObject.SetActive(view == _currentView);
            }
            Setting(info);
            yield return StartCoroutine(PopupCoroutine());
        }
        OutUI();
    }

    void Setting(AttendanceInfo info)
    {
        _titleText.text = LocalizeManager.GetText(info.Data.Titletext);
        _descText.text = LocalizeManager.GetText(info.Data.Desctext);
        _rightDescText.text = LocalizeManager.GetText("UI_Attendance_Bonus_Info", info.RewardPoint);

        // 출석 보상
        for (int i = 0; i < _elements.Count; i++)
        {
            UI_Attendance_ItemElement element = _elements[i];
            bool active = i < info.ItemInfos.Count;
            element.gameObject.SetActive(active);
            if (active)
            {
                FAttendance_Item itemData = info.ItemInfos[i];
                bool already = itemData.Value <= info.RewardPoint;
                bool current = itemData.Value == info.RewardPoint;
                element.Setting(itemData, false, already, current);
            }
        }

        // 누적 출석 보상
        List<FAttendance_Item> itemSecondDatas = info.ItemInfos.FindAll(x => x.Item1id > 0);
        for (int i = 0; i < _elementRights.Count; i++)
        {
            UI_Attendance_ItemElement element = _elementRights[i];
            bool active = i < itemSecondDatas.Count;
            element.gameObject.SetActive(active);
            if (active)
            {
                FAttendance_Item itemData = itemSecondDatas[i];
                bool already = itemData.Value <= info.RewardPoint;
                bool current = itemData.Value == info.RewardPoint;
                element.Setting(itemData, true, already, current);
            }
        }
    }

    IEnumerator PopupCoroutine()
    {
        StartCoroutine(BodyAnimation(OpenFromStart));
        if (OpenFromStart)
        {
            List<UI_Attendance_ItemElement> currents = _allElements.FindAll(x => x.IsCurrent);
            foreach (UI_Attendance_ItemElement element in currents) 
            {
                element.Anim.Play();
            }
            yield return new WaitForSecondsRealtime(_allElements.First().Anim.GetFirst().length);
        }
        _pressed = false;
        yield return new WaitUntil(() => _pressed);
    }

    IEnumerator BodyAnimation(bool playStart)
    {
        if (playStart)
        {
            _bodyAnimation.Play();
            _currentView.Play();
            yield return new WaitUntil(() => !_bodyAnimation.isPlaying);
            yield return new WaitUntil(() => !_currentView.isPlaying);
        }
        _currentView.clip = _currentView.GetSecond().clip;
        _currentView.Play();
    }
}
