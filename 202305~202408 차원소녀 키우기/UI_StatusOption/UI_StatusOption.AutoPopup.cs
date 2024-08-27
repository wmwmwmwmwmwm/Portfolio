using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_StatusOption
{
    [Header("자동 부여 팝업")]
    [SerializeField] GameObject _autoGachaPopup;
    [SerializeField] Image _auto_GradeIconPrefab;
    [SerializeField] Transform _auto_GradeIconParent;
    [SerializeField] UI_StatusOption_StatEntry _auto_StatEntryPrefab;
    [SerializeField] Transform _auto_StatEntryParent;
    //[SerializeField] UI_Button _auto_StartButton;
    [SerializeField] Image _auto_CostIcon;
    [SerializeField] TMP_Text _auto_CostText;

    [Header("자동 부여 오버레이")]
    [SerializeField] GameObject _autoGachaOverlay;
    //[SerializeField] RectTransform _auto_Black, _auto_BlackPosition, _auto_BlackParent;
    [SerializeField] Image _auto_InfoIcon;
    [SerializeField] GameObject _auto_OverlayStatPrefab;
    [SerializeField] Transform _auto_OverlayStatParent;

    List<(Image icon, EItemGrade grade)> _auto_GradeIcons;
    List<UI_StatusOption_StatEntry> _auto_StatEntrys;
    List<UI_StatusOption_StatEntry> _auto_CheckedStats;
    List<GameObject> _auto_OverlayStats;
    EItemGrade _auto_SelectedGrade;
    Coroutine _auto_GachaLoopCoroutine;
    bool _auto_LoopCanceled;

    void InitAutoPopup()
    {
        _autoGachaOverlay.SetActive(false);
        //_autoGachaOverlayBlack.SetActive(false);
        _autoGachaPopup.SetActive(false);
        _auto_OverlayStatPrefab.SetActive(false);
        _auto_OverlayStats = new();
        //_auto_StartButton.interactable = false;

        // 목표 등급
        _auto_GradeIconPrefab.gameObject.SetActive(false);
        _auto_GradeIcons = new();
        EItemGrade gradeStart = EItemGrade.A;
        for (EItemGrade grade = gradeStart; grade <= EItemGrade.UR; grade++)
        {
            Image icon = Instantiate(_auto_GradeIconPrefab, _auto_GradeIconParent);
            icon.gameObject.SetActive(true);
            icon.material = new(icon.material);
            icon.sprite = GetIconSprites(grade);
            icon.GetComponent<UI_Button>().SetID($"{EButtonId.Auto_GradeIcon}__{(int)grade}");
            _auto_GradeIcons.Add((icon, grade));
        }

        // 목표 효과
        _auto_CheckedStats = new();
        _auto_StatEntryPrefab.gameObject.SetActive(false);
        _auto_StatEntrys = new();
        List<FCharacterOption> statDatas = DataManager.Instance.GetCharacterOptionMap().Values.ToList();
        for (int i = 0; i < statDatas.Count; i++)
        {
            UI_StatusOption_StatEntry entry = Instantiate(_auto_StatEntryPrefab, _auto_StatEntryParent);
            entry.gameObject.SetActive(true);
            entry.Setting(statDatas[i]);
            entry.GetComponent<UI_Button>().SetID($"{EButtonId.Auto_StatEntry}__{i}");
            _auto_StatEntrys.Add(entry);
        }

        OnClickGradeIcon(gradeStart);
    }

    void OnClickAutoClose()
    {
        _autoGachaPopup.gameObject.SetActive(false);
    }

    void OnClickGradeIcon(EItemGrade _selectedGrade)
    {
        _auto_SelectedGrade = _selectedGrade;
        (Image selectedIcon, EItemGrade selectedGrade) = _auto_GradeIcons.Find(x => x.grade == _selectedGrade);
        foreach ((Image icon, EItemGrade grade) in _auto_GradeIcons)
        {
            icon.material.SetFloat("_Grayscale", grade == selectedGrade ? 0f : 1f);
        }
    }

    void OnClickSlotEntry(UI_StatusOption_StatEntry selected)
    {
        if (!selected.IsChecked)
        {
            selected.IsChecked = true;
            _auto_CheckedStats.Add(selected);
        }
        else
        {
            selected.IsChecked = false;
            _auto_CheckedStats.Remove(selected);
        }
        if (_auto_CheckedStats.Count > 3)
        {
            UI_StatusOption_StatEntry first = _auto_CheckedStats[0];
            first.IsChecked = false;
            _auto_CheckedStats.Remove(first);
        }
        foreach (UI_StatusOption_StatEntry entry in _auto_StatEntrys)
        {
            entry.Select(entry.IsChecked);
        }
        //_auto_StartButton.interactable = _auto_CheckedStats.Count > 0;
    }

    IEnumerator OnClickAutoStart()
    {
        if (AnySSR)
        {
            yield return StartCoroutine(GachaConfirm());
            if (!_gachaConfirmYes) yield break;
            _gachaConfirmYes = false;
        }

        _autoGachaPopup.SetActive(false);
        StartCoroutine(AutoGachaCoroutine());
    }

    void OnClickAutoCancel()
    {
        _auto_LoopCanceled = true;
    }

    IEnumerator AutoGachaCoroutine()
    {
        // 우측 하단 정보패널 초기화
        _auto_InfoIcon.sprite = GetIconSprites(_auto_SelectedGrade);
        _auto_OverlayStats.ForEach(x => Destroy(x.gameObject));
        _auto_OverlayStats.Clear();
        foreach (UI_StatusOption_StatEntry entry in _auto_CheckedStats)
        {
            AddEntry(LocalizeManager.GetText(entry.Stat.ToString()));
        }
        if (_auto_OverlayStats.Count == 0)
        {
            AddEntry("-");
        }

        void AddEntry(string text)
        {
            GameObject statEntry = Instantiate(_auto_OverlayStatPrefab, _auto_OverlayStatParent);
            statEntry.SetActive(true);
            statEntry.GetComponentInChildren<TMP_Text>().text = text;
            _auto_OverlayStats.Add(statEntry);
        }

        // 자동부여 시작
        UIPlayTween tweener = _autoGachaOverlay.GetComponent<UIPlayTween>();
        _autoGachaOverlay.SetActive(true);
        //_auto_Black.SetParent(_auto_BlackPosition, false);
        //_auto_Black.SetParent(_auto_BlackParent, true);
        //_autoGachaOverlayBlack.SetActive(true);
        //UIPlayTween tweener2 = _autoGachaOverlay.GetComponent<UIPlayTween>();
        tweener.Play(true);
        //tweener2.Play(true);
        _auto_GachaLoopCoroutine = StartCoroutine(AutoGachaCoroutineLoop());
        yield return new WaitUntil(() => _auto_GachaLoopCoroutine == null);
        tweener.Play(false);
        //tweener2.Play(false);
        yield return new WaitUntil(() => !tweener.IsPlaying);
        _autoGachaOverlay.SetActive(false);
        //_autoGachaOverlayBlack.SetActive(false);

        PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).RefreshValues(true);
    }

    IEnumerator AutoGachaCoroutineLoop()
    {
        bool loop = true;
        List<OptionSlotInfoItem> slots = Option.GetCategorySlots(_selectedCategory).FindAll(x => !x.Lock && x.Open);
        while (loop && !_auto_LoopCanceled)
        {
            StartCoroutine(OnClickGacha(true));

            // 조건 달성 또는 재화 모자랄시 종료
            bool success = false;
            foreach (OptionSlotInfoItem slot in slots)
            {
                bool statMatched = _auto_CheckedStats.Count == 0 
                    || _auto_CheckedStats.Any(x => x.Stat == slot.Option.OptionData.Stat);
                if (slot.Option.Grade >= _auto_SelectedGrade && statMatched) 
                {
                    success = true;
                    break;
                }
            }
            if (success || !Option.CanRerollOption(_selectedCategory))
            {
                loop = false;
            }

            float waitTime = 0.7f;
            while (waitTime > 0f && !_auto_LoopCanceled)
            {
                waitTime -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
        _auto_LoopCanceled = false;
        _auto_GachaLoopCoroutine = null;
    }
}
