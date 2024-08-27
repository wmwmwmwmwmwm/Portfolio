using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using static UI_StatusOption_CategoryStars;
using System.Collections;

public partial class UI_StatusOption : UIMenuBarBase
{
    enum EButtonId
    {
        None,
        Category,
        Slot,
        Gacha,
        OpenAutoGacha,
        Auto_Close,
        Auto_GradeIcon,
        Auto_StatEntry,
        Auto_AutoGacha,
        Auto_LoopCancel,
    }

    [Header("메인")]
    [SerializeField] List<UI_StatusOption_CategoryStars> _categoryStarPrefabs;
    [SerializeField] Transform _categoryStarParent;
    [SerializeField] List<UI_StatusOption_Slot> _slots;
    [SerializeField] UI_Button _gachaButton, _openAutoPopupButton;
    [SerializeField] GameObject _gachaButtonOpen, _gachaButtonNotOpen;
    [SerializeField] Image _gachaCostIcon;
    [SerializeField] TextMeshProUGUI _gachaCostText;
    [SerializeField] List<UI_Button> _categoryButtons;
    [SerializeField] Sprite[] _coverSprites;
    [SerializeField] Sprite[] _iconSprites;

    UI_StatusOption_CategoryStars[] _categoryStars;
    int _selectedCategory;
    bool _gachaConfirmYes;

    StatusOptionManager Option => StatusOptionManager.Instance;
    bool AnySSR => Option.GetCategorySlots(_selectedCategory).Any(x => !x.Lock && x.Option.Grade >= EItemGrade.SSR);
    bool AnyNoticeWaiting => _slots.Any(x => x.UnlockWaiting);

    public override void InitUI()
    {
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Currency, gameObject, UpdateStates);
        
        _selectedCategory = 0;
        _categoryStars = new UI_StatusOption_CategoryStars[5];
        for (int i = 0; i < _categoryButtons.Count; i++)
        {
            _categoryButtons[i].SetID($"{EButtonId.Category}__{i}");
        }
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].ButtonID = $"{EButtonId.Slot}__{i}";
        }
        Option.RefreshSlotOpen();
        InitAutoPopup();
        base.InitUI();
    }

    protected override void RefreshUI(LanguageType langCode)
    {
        RefreshCategoryStars();
        RefreshSlots();
        RefreshCategoryButtons();
        UpdateStates();
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback)
    {
        resultCallback?.Invoke(true);
    }

    public override bool OnBackKey()
    {
        if (_autoGachaPopup.activeSelf)
        {
            OnClickAutoClose();
            return false;
        }
        else if (_autoGachaOverlay.activeSelf)
        {
            OnClickAutoCancel();
            return false;
        }
        return base.OnBackKey();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        // 버튼 선택
        string[] idTokens = buttonInfo.ID.Split("__");
        EButtonId buttonType = EnumHelper.Parse(idTokens[0], EButtonId.None);
        int index = idTokens.Length > 1 ? int.Parse(idTokens[1]) : -1;
        switch (buttonType)
        {
            case EButtonId.Gacha:
                StartCoroutine(OnClickGacha(false));
                break;
            case EButtonId.Category:
                // 카테고리 선택
                int category = int.Parse(idTokens[1]);
                if (!Option.GetCategoryOpen(category))
                {
                    UIManager.ShowToastMessage(LocalizeManager.GetText("UI_Option_TouchLockCategory", Option.GetCategoryData(category).Slotopenconditionvalue));
                }
                if (_selectedCategory == category) return;
                _selectedCategory = category;
                OnRefreshUI();
                break;
            case EButtonId.Slot:
                // 슬롯 선택
                if (_slots[index].UnlockWaiting)
                {
                    _slots.Where(x => x.UnlockWaiting).ToList().ForEach(x => x.UnlockTrigger = true);
                }
                else
                {
                    LockSlotAt(index);
                }
                RefreshSlots();
                RefreshGachaCost();
                break;
            case EButtonId.OpenAutoGacha:
                // 자동 부여 버튼
                if (AnyNoticeWaiting)
                {
                    UIManager.ShowToastMessage(LocalizeManager.GetText("Toast_Option_LockedSlot"));
                    return;
                }
                _autoGachaPopup.SetActive(true);
                break;
            case EButtonId.Auto_Close:
                // 자동 부여 닫기
                OnClickAutoClose();
                break;
            case EButtonId.Auto_GradeIcon:
                // 목표 등급 선택
                OnClickGradeIcon((EItemGrade)index);
                break;
            case EButtonId.Auto_StatEntry:
                // 목표 효과 선택
                OnClickSlotEntry(_auto_StatEntrys[index]);
                break;
            case EButtonId.Auto_AutoGacha:
                // 자동 부여 시작
                StartCoroutine(OnClickAutoStart());
                break;
            case EButtonId.Auto_LoopCancel:
                // 자동 부여 취소
                OnClickAutoCancel();
                break;
        }
    }

    void UpdateStates()
    {
        RefreshGachaCost();
    }

    void RefreshSlots()
    {
        for (int i = 0; i < StatusOptionManager.optionPerCategory; i++)
        {
            _slots[i].RefreshSlot(this, _selectedCategory, i);
        }
    }

    public void RefreshGachaCost()
    {
        UIRoot.Instance.SetButtonItemShortage(_gachaButton, AnyNoticeWaiting, true);
        UIRoot.Instance.SetButtonItemShortage(_openAutoPopupButton, AnyNoticeWaiting, true);
        ItemCostInfo cost = Option.GetOptionRollingCost(_selectedCategory);
        bool allLocked = cost.Item == null;
        bool affordable = Option.CanRerollOption(_selectedCategory);
        bool interactable = !allLocked && affordable/* && !noticeWaiting*/;
        if (_gachaButton.interactable != interactable)
        {
            _gachaButton.interactable = interactable;
        }
        _openAutoPopupButton.interactable = interactable;
        bool anyOpened = Option.GetOpenCount(_selectedCategory) > 0;
        _gachaButtonOpen.SetActive(anyOpened);
        _gachaButtonNotOpen.SetActive(!anyOpened);
        if (anyOpened)
        {
            _gachaCostIcon.gameObject.SetActive(!allLocked);
            _gachaCostText.gameObject.SetActive(!allLocked);
            if (!allLocked)
            {
                _gachaCostIcon.sprite = AtlasManager.GetItemIcon(cost.Item, EIconType.MiniIcon);
                _gachaCostText.text = Formula.NumberToStringBy3(cost.Count);
                _auto_CostIcon.sprite = AtlasManager.GetItemIcon(cost.Item, EIconType.MiniIcon);
                _auto_CostText.text = $"{Formula.NumberToStringBy3(cost.Item.Count)}/{Formula.NumberToStringBy3(cost.Count)}";
            }
        }
    }

    void RefreshCategoryStars()
    {
        // 카테고리 오브젝트 없으면 생성
        if (!_categoryStars[_selectedCategory])
        {
            _categoryStars[_selectedCategory] = Instantiate(_categoryStarPrefabs[_selectedCategory], _categoryStarParent);
        }

        for (int i = 0; i < _categoryStars.Length; i++)
        {
            if (!_categoryStars[i]) continue;
            _categoryStars[i].gameObject.SetActive(i == _selectedCategory);
        }

        // 옵션부여 가능한 슬롯에 해당하는 별에 반짝임 활성화
        UI_StatusOption_CategoryStars stars = _categoryStars[_selectedCategory];
        for (int i = 0; i < stars.Stars.Count; i++)
        {
            OptionSlotInfoItem slot = Option.GetSlot(_selectedCategory, i);
            UI_StatusOption_CategoryStars_Star star = stars.Stars[i];
            star.Setting(slot.Open);
        }

        // 선 양쪽의 슬롯이 열린 상태면 선 활성화
        for (int i = 0; i < stars.Lines.Count; i++)
        {
            GameObject line = stars.Lines[i];
            bool lineActive = i < stars.LineInfos.Count;
            if (lineActive)
            {
                LineInfo lineInfo = stars.LineInfos.Find(x => x.Line == line);
                bool star1Open = GetActiveFromStar(lineInfo, lineInfo.Star1);
                bool star2Open = GetActiveFromStar(lineInfo, lineInfo.Star2);
                lineActive = star1Open && star2Open;
            }
            line.SetActive(lineActive);
        }

        bool GetActiveFromStar(LineInfo info, UI_StatusOption_CategoryStars_Star star)
        {
            int index = stars.Stars.IndexOf(star);
            OptionSlotInfoItem slot = Option.GetSlot(_selectedCategory, index);
            return slot.HasOption;
        }
    }

    void RefreshCategoryButtons()
    {
        for (int i = 0; i < _categoryButtons.Count; i++)
        {
            UI_Button button = _categoryButtons[i];
            bool selected = i == _selectedCategory;
            button.transform.Find("Selected").gameObject.SetActive(selected);
            bool notOpened = !Option.Info.CategoryOpen.Open[i];
            button.transform.Find("NotOpened").gameObject.SetActive(notOpened);
        }
    }

    void LockSlotAt(int slotindex)
    {
        var info = Option.GetCategorySlots(_selectedCategory)[slotindex];
        if (!info.HasOption) return;
        info.Lock = !info.Lock;
        UserDataManager.Save_Content("StatusOptionRefresh");
    }

    IEnumerator OnClickGacha(bool isAuto)
    {
        if (AnyNoticeWaiting)
        {
            UIManager.ShowToastMessage(LocalizeManager.GetText("Toast_Option_LockedSlot"));
            yield break;
        }
        if (AnySSR && !isAuto)
        {
            yield return StartCoroutine(GachaConfirm());
            if (!_gachaConfirmYes) yield break;
            _gachaConfirmYes = false;
        }

        //List<OptionSlot> currentCategory = Option.GetCategorySlots(_selectedCategory);
        //int highRankCountBefore = currentCategory.Count(x => x.Option.Grade >= EItemGrade.S);
        List<int> changedOptions = Option.SetRandomOptionAll(_selectedCategory, !isAuto);
        //int highRankCountAfter = currentCategory.Count(x => x.Option.Grade >= EItemGrade.S);
        //bool isHighestGrade = highRankCountAfter > highRankCountBefore;
        UIRoot.Instance.PlayLevelUpEffect(transform, null, null);

        Dictionary<ObscuredInt, FCharacterOptionLock>.ValueCollection map = DataManager.Instance.GetCharacterOptionLockMap().Values;
        int lockCount = Option.GetLockCount(_selectedCategory);
        int cost = map.FirstOrDefault(e => e.Slotlocknumber == lockCount).Requireitemcount;        

        foreach (int index in changedOptions)
        {
            Play2DEffect(id: "Fx_UI_Upgrade_Mastery_UnLock_01",
                position: _slots[index].EffectPosition.localPosition,
                parent: _slots[index].EffectPosition.parent);
            _categoryStars[_selectedCategory].Stars[index].PlayUpgradeEffect();
        }

        UserDataManager.Save_Content("StatusOptionRefresh");
        OnRefreshUI();
    }

    IEnumerator GachaConfirm()
    {
        // SSR 등급 이상 변경 시도시 확인 팝업
        bool closed = false;
        _gachaConfirmYes = false;
        UIManager.Instance.OpenUI(EUIName.Popup_NormalMessage, new()
        {
            openedCallback = (ui) =>
            {
                var popup = ui as Popup_NormalMessage;
                popup.SettingInfo(new ShowMessageBoxInfo()
                {
                    Title = LocalizeManager.GetText("Alarm"),
                    ConfirmButtonName = LocalizeManager.GetText("UI_Btn_OK"),
                    CancelButtonName = LocalizeManager.GetText("UI_Btn_Cancel"),
                    ConfirmCallback = () => _gachaConfirmYes = true,
                    Content = LocalizeManager.GetText("UI_Option_Warring_Desc_01")
                });
            },
            closedCallback = (_, _) =>
            {
                closed = true;
            }
        });
        yield return new WaitUntil(() => closed);
    }

    public Sprite GetCoverSprites(EItemGrade grade) => _coverSprites[(int)grade - 1];
    public Sprite GetIconSprites(EItemGrade grade) => _iconSprites[(int)grade - 1];
}
