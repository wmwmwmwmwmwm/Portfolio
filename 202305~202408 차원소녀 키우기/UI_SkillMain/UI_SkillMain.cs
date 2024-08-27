using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_ItemMasteryPopup;
using static UI_SkillMain_SkillElement;

public partial class UI_SkillMain : UIMenuBarBase, IMasteryPopup
{
    public enum ETabId
    {
        None,
        Weapon,
        Style,
    }

    public enum EButtonId
    {
        None,
        Skill,
        Slot,
        AutoEquip,
        LevelUpAll,
        OverlayClose,
        OpenMastery,
    }

    [Header("좌측")]
    [SerializeField] UI_TabGroup _weaponsTab;
    [SerializeField] UIPlayTween _infoTweener;
    [SerializeField] TMP_Text _levelText, _nameText, _descText, _damageText, _cooltimeText;
    [SerializeField] UIOpenButtonStat _openMasteryOpenState;
    [SerializeField] RedDotComponent _openMasteryRedDot;
    [SerializeField] UI_ItemMasteryPopup _masteryPopup;

    [Header("우측 슬롯, 스킬 리스트, 버튼")]
    [SerializeField] UI_TweeningTab _styleTab;
    [SerializeField] UI_TabGroup _styleTabGroup;
    [SerializeField] ScrollRect _listScroll;
    [SerializeField] UI_SkillMain_SkillElement _skillElementPrefab;
    [SerializeField] Transform _skillElementParent;
    [SerializeField] GameObject _normalSlotParent;
    [SerializeField] List<UI_SkillMain_Slot> _normalSlots;
    [SerializeField] UI_SkillMain_Slot _costumeSlot;
    [SerializeField] UI_Button _autoEquipButton, _levelUpAllButton;
    [SerializeField] UI_Button _overlayCloseButton;
    [SerializeField] GameObject _equipOverlay_Normal, _equipOverlay_Costume;

    [Header("Preview")]
    [SerializeField] PreviewSpace_ _preview;

    SkillSlotSet _playerSkillInfo;
    List<SkillItem> _allSkills;
    List<UI_SkillMain_Slot> _slots;
    EUseWeapon _selectedWeapon;
    ESkillStyle _selectedStyle;
    List<UI_SkillMain_SkillElement> _skillElements;
    UI_SkillMain_SkillElement _selectedSkill;
    bool _showShortageToast;
    bool _isChangeData;

    bool IsEquipMode => _equipOverlay_Normal.activeSelf || _equipOverlay_Costume.activeSelf;

    public override void InitUI()
    {
        _skillElementPrefab.gameObject.SetActive(false);
        _equipOverlay_Normal.SetActive(false);
        _equipOverlay_Costume.SetActive(false);
        _overlayCloseButton.gameObject.SetActive(false);
        _skillElements = new();
        _playerSkillInfo = PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).SkillSlots;
        _allSkills = InventoryManager.Instance.SkillItems.Where(x => x.SkillData.Levelupgroupid > 0).ToList();
        _slots = new();
        _slots.AddRange(_normalSlots);
        _slots.Add(_costumeSlot);
        _masteryPopup.Init(this);

        // 탭 초기화
        _weaponsTab.Init();
        _weaponsTab.SetSelectButton(0);
        _styleTabGroup.Init();
        _styleTabGroup.SetSelectButton(0);

        // 레드닷 초기화
        for (int i = 1; i < (int)EUseWeapon.MAXCOUNT; i++)
        {
            List<SkillItem> skills = _allSkills.FindAll(x => x.SkillData.Useweapon == (EUseWeapon)i);
            RedDot_SkillTab redDot = _weaponsTab.Buttons[i - 1].GetComponentInChildren<RedDot_SkillTab>();
            redDot.Setting(skills);
        }

        // 마스터리버튼 레드닷 초기화
        _openMasteryOpenState.Setting(new() { EContent.SkillMastery });
        _openMasteryRedDot.SetAuto(new() { new RedDotComponent.RedDotInfo() { Content = EContent.SkillMastery } });

        SelectWeaponTab(EUseWeapon.Handgun);
        SelectStyleTab(ESkillStyle.Normal);
        CreateSkillList();

        base.InitUI();

        StartCoroutine(PreviewInitWaitCoroutine());
    }

    IEnumerator PreviewInitWaitCoroutine()
    {
        yield return new WaitUntil(() => _preview.GetModel("npc1001") != null && _preview.GetModel("npc1001").IsLoaded);
        SettingPreviewModel();
        if (_animationUpdateCoroutine == null)
        {
            PlaySkill();
        }
    }

    public override bool OnBackKey()
    {
        if (_masteryPopup.gameObject.activeSelf)
        {
            _masteryPopup.Show(false);
            return false;
        }
        else if (IsEquipMode)
        {
            SetEquipMode(false);
            return false;
        }
        return base.OnBackKey();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State == UI_ButtonState.HoldEnd)
        {
            _showShortageToast = true;
            if (_isChangeData)
            {
                UserDataManager.Save_LocalData();
                _isChangeData = false;
            }
        }

        // 드래그 조작
        switch (buttonInfo.State)
        {
            case UI_ButtonState.Press:
                OnBeginDrag(buttonInfo.PointerData);
                break;
            case UI_ButtonState.Move:
                OnDrag(buttonInfo);
                break;
            case UI_ButtonState.Up:
                OnEndDrag(buttonInfo.PointerData);
                break;
        }

        if (buttonInfo.State != UI_ButtonState.Click) return;
        string[] tokens = buttonInfo.ID.Split("__");

        // 탭 변경
        if (buttonInfo.ButtonType == UI_ButtonType.TAB)
        {
            List<SkillItem> group = _allSkills.FindAll(x => x.SkillData.Useweapon == _selectedWeapon);
            ETabId tabId = EnumHelper.Parse(buttonInfo.TabID, ETabId.None);
            switch (tabId)
            {
                case ETabId.Weapon:
                    foreach (SkillItem skill in _allSkills)
                    {
                        if (skill.SkillData.Useweapon == _selectedWeapon)
                        {
                            skill.New = false;
                        }
                    }
                    EUseWeapon weapon = EnumHelper.Parse(buttonInfo.ID, EUseWeapon.Handgun);
                    if (_selectedWeapon == weapon) return;
                    SelectWeaponTab(weapon);
                    CreateSkillList();
                    break;
                case ETabId.Style:
                    ESkillStyle style = EnumHelper.Parse(buttonInfo.ID, ESkillStyle.NONE);
                    foreach (SkillItem skill in group)
                    {
                        if (skill.SkillData.Skillstyle == style)
                        {
                            skill.New = false;
                        }
                    }
                    if (_selectedStyle == style) return;
                    SelectStyleTab(style);
                    CreateSkillList();
                    break;
            }
            return;
        }

        // 버튼 선택
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.Skill:
                EElementButtonId buttonId2 = EnumHelper.Parse(tokens[1], EElementButtonId.None);
                switch (buttonId2)
                {
                    case EElementButtonId.Select:
                        UI_SkillMain_SkillElement skill = buttonInfo.Trans.GetComponent<UI_SkillMain_SkillElement>();
                        if (_selectedSkill == skill) return;
                        SelectSkill(skill);
                        break;
                    case EElementButtonId.Equip:
                        SetEquipMode(true);
                        break;
                    case EElementButtonId.LevelUp:
                        LevelUpButton();
                        break;
                }
                break;
            case EButtonId.Slot:
                if (!int.TryParse(tokens[1], out int index)) return;
                SelectSlot(index);
                break;
            case EButtonId.AutoEquip:
                AutoEquipButton();
                break;
            case EButtonId.LevelUpAll:
                LevelUpAllButton();
                break;
            case EButtonId.OverlayClose:
                SetEquipMode(false);
                break;
            case EButtonId.OpenMastery:
                _masteryPopup.Show(true, (int)_selectedWeapon - 1);
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode)
    {
        RefreshSkillDesc();
        RefreshSkillList();
        if (_masteryPopup.gameObject.activeSelf)
        {
            _masteryPopup.RefreshMastery();
        }
    }

    public override void OutOnlyMyself(bool isDirect = false)
    {
        base.OutOnlyMyself(isDirect);
        foreach (SkillItem skill in _allSkills)
        {
            skill.New = false;
        }
        RefreshPlayer();
    }

    void SelectWeaponTab(EUseWeapon weapon)
    {
        _selectedWeapon = weapon;
        _weaponsTab.Buttons.ForEach(x =>
        {
            MenuBar_Menu menu = x.GetComponent<MenuBar_Menu>();
            menu.Animate();
            EUseWeapon weapon = EnumHelper.Parse(menu.Button.ID, EUseWeapon.Handgun);
            menu.Text.text = LocalizeManager.GetText(weapon switch
            {
                EUseWeapon.Handgun => "UI_Btn_Skill_Category_01",
                EUseWeapon.Sword => "UI_Btn_Skill_Category_02",
                EUseWeapon.Hammer => "UI_Btn_Skill_Category_03",
                _ => ""
            });
        });

        // 레드닷 초기화
        List<SkillItem> skills = _allSkills.FindAll(x => x.SkillData.Useweapon == _selectedWeapon);
        List<(ESkillStyle style, int index)> pairs = new()
        {
            (ESkillStyle.Normal, 0),
            (ESkillStyle.Costume, 1),
        };
        foreach ((ESkillStyle style, int index) in pairs)
        {
            List<SkillItem> skills2 = skills.FindAll(x => x.SkillData.Skillstyle == style);
            RedDot_SkillTab redDot = _styleTabGroup.Buttons[index].GetComponentInChildren<RedDot_SkillTab>();
            redDot.Setting(skills2);
        }

        SettingPreviewModel();
    }

    void SettingPreviewModel()
    {
        var model = _preview.GetModel("npc1001");
        if (null != model)
        {
            model.AnimationInfo.AnimationGroup = _selectedWeapon.ToAnimationGroup();
            var selection = _selectedWeapon.ToModelPartType();
            for (int i = 1; i < (int)EUseWeapon.MAXCOUNT; i++)
            {
                var target = ((EUseWeapon)(i)).ToModelPartType();
                model.Draw.SetActivePartOfType(selection == target, target);
            }
        }
    }

    void SelectStyleTab(ESkillStyle style)
    {
        _selectedStyle = style;
        _styleTab.Select((int)style - 1, true);
        _normalSlotParent.SetActive(_selectedStyle == ESkillStyle.Normal);
        _costumeSlot.gameObject.SetActive(_selectedStyle == ESkillStyle.Costume);
    }

    void SelectSkill(UI_SkillMain_SkillElement skill)
    {
        _selectedSkill = skill;
        _infoTweener.Play();
        OnRefreshUI();
        PlaySkill();
    }

    void SelectSlot(int index)
    {
        UI_SkillMain_Slot slot = _slots[index];

        // 장착모드일 때
        if (IsEquipMode)
        {
            // 선택한 스킬의 슬롯이면 해제
            if (_selectedSkill.Skill == slot.Skill)
            {
                SetSlot(_selectedWeapon, slot.Index, 0);
            }
            // 다른 스킬이면 장착
            else
            {
                EquipSkill(slot, _selectedWeapon, _selectedSkill.Skill);
            }
            SetEquipMode(false);
        }
        // 장착모드가 아닐 때
        else
        {
            // 비어있다면 장착
            if (slot.Skill == null)
            {
                EquipSkill(slot, _selectedWeapon, _selectedSkill.Skill);
            }
            // 이미 장착중인 슬롯은 선택
            else
            {
                UI_SkillMain_SkillElement element = _skillElements.Find(x => x.Skill == slot.Skill);
                SelectSkill(element);
                _listScroll.FocusOnItem(element.GetComponent<RectTransform>());
            }
        }

        OnRefreshUI();
    }

    void SetEquipMode(bool on)
    {
        if (on)
        {
            _overlayCloseButton.gameObject.SetActive(true);
            if (_selectedStyle == ESkillStyle.Normal)
            {
                _equipOverlay_Normal.SetActive(true);
            }
            else
            {
                _equipOverlay_Costume.SetActive(true);
            }
        }
        else
        {
            _overlayCloseButton.gameObject.SetActive(false);
            _equipOverlay_Normal.SetActive(false);
            _equipOverlay_Costume.SetActive(false);
        }
        OnRefreshUI();
    }

    void LevelUpButton()
    {
        bool success = _selectedSkill.Skill.LevelUp();
        if (!success)
        {
            if (_showShortageToast)
            {
                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                _showShortageToast = false;
            }
            return;
        }

        LevelUpEffect(_selectedSkill, true);
        OnRefreshUI();
    }

    void AutoEquipButton()
    {
        for (int i = 1; i < (int)EUseWeapon.MAXCOUNT; i++)
        {
            EUseWeapon weapon = (EUseWeapon)i;
            List<SkillItem> group = _allSkills.FindAll(x => x.Have && x.SkillData.Useweapon == weapon);

            // 일반 스킬 자동장착
            List<SkillItem> normalSkills = group.FindAll(x => x.SkillData.Skillstyle == ESkillStyle.Normal);
            normalSkills = Sort(normalSkills);
            for (int a = 0; a < _normalSlots.Count; a++)
            {
                UI_SkillMain_Slot slot = _normalSlots[a];
                if (a < normalSkills.Count)
                {
                    EquipSkill(slot, weapon, normalSkills[a]);
                }
            }

            // 코스튬 스킬 자동장착
            List<SkillItem> costumeSkills = group.FindAll(x => x.SkillData.Skillstyle == ESkillStyle.Costume);
            costumeSkills = Sort(costumeSkills);
            if (costumeSkills.Count > 0)
            {
                EquipSkill(_costumeSlot, weapon, costumeSkills[0]);
            }
        }
        OnRefreshUI();

        List<SkillItem> Sort(List<SkillItem> items)
        {
            return items.OrderByDescending(x => x.itemData.Grade)
            .ThenByDescending(x => x.Level)
            .ThenByDescending(x => x.Id).ToList();
        }
    }

    void LevelUpAllButton()
    {
        List<UI_SkillMain_SkillElement> results = new();
        foreach (SkillItem skill in _allSkills)
        {
            bool anySuccess = false;
            while (true)
            {
                bool success = skill.LevelUp();
                anySuccess |= success;
                if (!success) break;
            }
            if (anySuccess)
            {
                UI_SkillMain_SkillElement element = _skillElements.Find(x => x.Skill == skill);
                if (element != null)
                {
                    results.Add(element);
                }
            }
            skill.New = false;
        }

        foreach (UI_SkillMain_SkillElement element in results) 
        {
            LevelUpEffect(element, false);
        }

        OnRefreshUI();
    }

    void CreateSkillList()
    {
        // 스킬리스트 초기화
        _skillElements.ForEach(x => Destroy(x.gameObject));
        _skillElements.Clear();
        List<SkillItem> skillList = _allSkills.FindAll(x => x.SkillData.Useweapon == _selectedWeapon);
        skillList = skillList.FindAll(x => x.SkillData.Skillstyle == _selectedStyle);
        foreach (SkillItem skill in skillList)
        {
            UI_SkillMain_SkillElement element = Instantiate(_skillElementPrefab, _skillElementParent);
            element.gameObject.SetActive(true);
            element.gameObject.name = $"SkillInfoButton_{skill.Id}";
            element.SetSkill(skill);
            _skillElements.Add(element);
        }
        RefreshSkillList();
        SelectSkill(_skillElements[0]);
        _listScroll.verticalNormalizedPosition = 1f;
    }

    void RefreshSkillList()
    {
        // 정렬 : 1. 장착 또는 보유 여부 - 2. 아이템 등급 - 3. ID
        _skillElements = _skillElements.OrderBy(x =>
        {
            int priority = 0;
            if (CheckEquip(x))
            {
                priority = -2;
            }
            else if (x.Skill.Have)
            {
                priority = -1;
            }
            return priority;
        })
        .ThenByDescending(x => x.Skill.itemData.Grade)
        .ThenBy(x => x.Skill.Id)
        .ToList();
        foreach (UI_SkillMain_SkillElement element in _skillElements)
        {
            element.Setting(_selectedSkill == element, CheckEquip(element));
            element.transform.SetAsLastSibling();
        }

        // 장착 스킬
        SkillSlots slotData = GetSlots(_selectedWeapon);
        for (int i = 0; i < _normalSlots.Count; i++)
        {
            SkillItem skill = slotData[i].ItemInfo;
            bool selected = _selectedSkill && skill == _selectedSkill.Skill;
            _normalSlots[i].Setting(skill, i, selected, IsEquipMode, _dragCoroutine != null);
        }
        SkillItem costumeSkill = slotData[SkillSlots.CostumeSlotIndex].ItemInfo;
        bool selected2 = _selectedSkill && costumeSkill == _selectedSkill.Skill;
        _costumeSlot.Setting(costumeSkill, SkillSlots.CostumeSlotIndex, selected2, IsEquipMode, _dragCoroutine != null);

        // 자동장착, 일괄강화 버튼
        bool canLevelUp = _skillElements.Any(x => x.Skill.CheckLevelUp(out _));
        _levelUpAllButton.interactable = canLevelUp || GuideTutorialManager.Instance.IsIng();
    }

    void RefreshSkillDesc()
    {
        // 스킬설명
        _levelText.text = $"Lv.{_selectedSkill.Skill.Level}";
        FSkill data = _selectedSkill.Skill.SkillData;
        _nameText.text = LocalizeManager.GetText(data.Nameid);
        _descText.text = LocalizeManager.GetText(data.Descid);
        string levelUpPower = Formula.NumberToStringBy3_Percent(_selectedSkill.Skill.GetMaxPower(_selectedSkill.Skill.Level), false, true);
        _damageText.text = LocalizeManager.GetText("UI_Skill_Desc_Damage", levelUpPower);
        string cooltime = Formula.NumberToString_Cooltime((int)data.Cooltime);
        _cooltimeText.text = LocalizeManager.GetText("UI_Skill_Desc_CoolTime", cooltime);
    }

    void EquipSkill(UI_SkillMain_Slot slot, EUseWeapon weapon, SkillItem skill)
    {
        if (slot.OpenButtonStat && !slot.OpenButtonStat.IsUnLock()) return;
        if (!skill.Have) return;
        SkillSlots slotData = GetSlots(weapon);
        int sameSkillIdx = slotData.FindSkillIdx(skill.Id);
        if (sameSkillIdx >= 0)
        {
            SetSlot(weapon, sameSkillIdx, slotData[slot.Index].SkillId);
        }
        SetSlot(weapon, slot.Index, skill.Id);
    }

    void SetSlot(EUseWeapon weapon, int index, int id)
    {
        _playerSkillInfo.SetSlot(ESkillSlot.SET_1, weapon, index, id);
    }

    void LevelUpEffect(UI_SkillMain_SkillElement element, bool reappearDelay)
    {
        // 강화 이펙트 출력
        float delay = reappearDelay ? 0.2f : 0f;
        Play2DEffect(id: "Fx_UI_Upgrade_Icon_Skill_01",
            position: element.EffectPosition_Icon.localPosition,
            parent: element.EffectPosition_Icon.parent,
            reappearDelay: delay);
        Play2DEffect(id: "Fx_UI_Upgrade_Skill_Line_01",
            position: element.EffectPosition_Desc.localPosition,
            parent: element.EffectPosition_Desc.parent,
            reappearDelay: delay);
        UIRoot.Instance.PlayLevelUpEffect(transform, null, null);
    }

    SkillSlots GetSlots(EUseWeapon weapon) => _playerSkillInfo[ESkillSlot.SET_1][(int)weapon];

    bool CheckEquip(UI_SkillMain_SkillElement element)
    {
        return GetSlots(element.Skill.SkillData.Useweapon).FindSkill(element.Skill.Id) != null;
    }

    void RefreshPlayer()
    {
        _isChangeData = true;
        BattleSceneManager.Instance.GetBattleScene().Obj()?.PlayerUnit?.RefreshSkillList();
    }

    #region Preview setting

    float _afterSkillDelay = 0.5f;
    Coroutine _animationUpdateCoroutine;

    void PlaySkill()
    {
        StopSkill();

        var model = _preview.GetModel("npc1001");
        if (null == model || model.IsLoaded == false) return;

        _animationUpdateCoroutine = StartCoroutine(AnimationUpdateCoroutine(model));

    }

    void StopSkill()
    {
        _preview.Camera.StopAnimationCamera();

        if (null != _animationUpdateCoroutine)
        {
            StopCoroutine(_animationUpdateCoroutine);
            _animationUpdateCoroutine = null;
        }

        var model = _preview.GetModel("npc1001");
        if (null == model) return;
        model.SetNowSkill(null);
        model.SetTransformToOrigin();
        model.Draw.PlayAnimAction(model.AnimationInfo.Idle);
    }

    IEnumerator AnimationUpdateCoroutine(PreviewModelInfo model)
    {
        if (_selectedStyle == ESkillStyle.Normal)
            yield return new WaitForSecondsRealtime(0.2f);
        model.SetNowSkill(_selectedSkill.Skill.SkillData);
        model.PlayAnimation(EAniType.Ani_Skill1 + _selectedSkill.Skill.SkillData.Skillanimid - 1);
        yield return new WaitUntil(() => model.IsPlayIdleAnimation());
        yield return new WaitForSecondsRealtime(_afterSkillDelay);
        PlaySkill();
    }
    #endregion

    ////public enum EUISkillMode
    ////{
    ////    Main,
    ////    //Order,
    ////}

    //[SerializeField] UI_SkillDetail _ui_skillDetail;
    //[SerializeField] UI_MainSkillUI _uI_MainSkillUI;

    ////GameObject _selectUI;

    ////EUISkillMode _uiSkillMode;

    //public override void InitUI()
    //{
    //    _uI_MainSkillUI.Init(this);
    //    //SkillAllSlotInfos _skillAllSlotInfos = PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).SkillSlots;
    //    //Show(/*EUISkillMode.Main, */_skillAllSlotInfos.GetSlotSetIdx(BattleSceneManager.Instance.GameMode));
    //    _uI_MainSkillUI.Create_HaveSkills();
    //    base.InitUI();
    //}

    //public void Refresh_GuideTutorialSelectValue()
    //{
    //    _uI_MainSkillUI?.Refresh_GuideTutorialSelectValue();
    //}

    //public void Show(/*EUISkillMode mode, */int setIdx)
    //{
    //    //_uiSkillMode = mode;
    //    //if (_selectUI)
    //    //{
    //    //    _selectUI.SetActive(false);
    //    //}
    //    //switch (_uiSkillMode)
    //    //{
    //    //    case EUISkillMode.Main:
    //    //        _uI_MainSkillUI.Show(setIdx);
    //    //        _selectUI = _uI_MainSkillUI.gameObject;
    //    //        break;
    //    //}
    //    _uI_MainSkillUI.Show(setIdx);
    //    _ui_skillDetail.OutUi();
    //}

    //public override bool OnBackKey()
    //{
    //    if (_ui_skillDetail.IsShow)
    //    {
    //        _ui_skillDetail.OutUi();
    //        return false;
    //    }
    //    //switch (_uiSkillMode)
    //    //{
    //    //    case EUISkillMode.Main:
    //    //        return _uI_MainSkillUI.OnBackKey();
    //    //}
    //    //return true;
    //    return _uI_MainSkillUI.OnBackKey();
    //}

    //public override void EventButton(UIButtonInfo buttonInfo)
    //{
    //    if (buttonInfo.State != UI_ButtonState.Click)
    //        return;
    //}

    //protected override void OnPrvOpenWork(Action<bool> resultCallback)
    //{
    //    resultCallback(true);
    //}

    //protected override void RefreshUI(LanguageType langCode)
    //{
    //    //switch (_uiSkillMode)
    //    //{
    //    //    case EUISkillMode.Main:
    //    //        _uI_MainSkillUI.Refresh_Slots();
    //    //        _uI_MainSkillUI.Refresh_HaveSkills();
    //    //        break;
    //    //}
    //    _uI_MainSkillUI.Refresh_Slots();
    //    _uI_MainSkillUI.Refresh_HaveSkills();
    //}

    //public override void OutUI(bool isDirect = false)
    //{
    //    _uI_MainSkillUI?.OutUI();
    //    _ui_skillDetail?.OutUi();
    //    base.OutUI(isDirect);
    //}
}
