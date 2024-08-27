using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;
using System.Collections;
using static EquipItem;
using static UI_ItemMasteryPopup;

public partial class UI_Equipment : UIMenuBarBase, IMasteryPopup
{
    enum EMode
    {
        Equipment,
        Accessory
    }

    enum EPanel
    {
        Desc,
        Craft
    }

    enum EButtonType
    {
        None,
        Close,
        Compound,
        Equip,
        AutoEquip,
        Enhance,
        //Quality,
        CompoundAll,
        OpenCraft,
        Craft_Close,
        Craft_Min, Craft_Minus, Craft_Plus, Craft_Max,
        Craft_Craft,
        OpenReforge,
        Reforge_Close,
        Reforge_Reforge,
        OpenMastery,
    }

    [Header("공통")]
    [SerializeField] GridViewAdapter _gridView;
    [SerializeField] RectTransform _gridItemPrefab;
    [SerializeField] UI_TweeningTab _tweeningTab;
    [SerializeField] Transform _effectPosition_Enhance/*, _effectPosition_Quality, _effectPosition_Reforge*/;
    [SerializeField] GameObject _descPanel, _craftPanel;
    [SerializeField] UIOpenButtonStat _openMasteryOpenState;
    [SerializeField] RedDotComponent _openMasteryRedDot;
    [SerializeField] UI_ItemMasteryPopup _masteryPopup;

    [Header("우측 프로필")]
    [SerializeField] PreviewSpace_ _preview;
    [SerializeField] Image _bg;
    //[SerializeField] GameObject _reforgeInfo;
    //[SerializeField] TMP_Text _reforgeInfo_RateText;
    [SerializeField] Image _portrait;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] Image _gradeImage;
    //[SerializeField] TMP_Text _qualityText, _qualityChanceText;
    [SerializeField] GameObject[] _stars;
    //[SerializeField] Slider _qualityBar;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] Transform _mainStat, _possess1Stat;
    [SerializeField] Transform _possess2Stat, _possess3Stat;
    [SerializeField] UI_Button _openReforgeButton;
    [SerializeField] Transform[] _rt_WeaponTransforms = new Transform[(int)EUseWeapon.MAXCOUNT];
    //[SerializeField] Transform _levelUpEffectCenter;

    [Header("우측 하단 버튼들")]
    [SerializeField] UI_Button _equipButton;
    [SerializeField] TMP_Text _equipButtonText;
    [SerializeField] UI_Button _qualityButton, _enhanceButton, _openReforgeButtonBottom;
    [SerializeField] RedDotComponent _openReforgeBottomRedDot;
    [SerializeField] UI_Button _compoundButton, _compoundAllButton;
    [SerializeField] UI_Button _craftButton, _conversionButton;
    //[SerializeField] TMP_Text _enhanceButtonTitleText;

    EMode _mode;
    EPanel _currentPanel;
    EquipItem __selectedEquip;
    EquipItem _selectedEquip
    {
        get => __selectedEquip;
        set
        {
            SetSelectValue(value?.Id.ToString());
            __selectedEquip = value;
        }
    }
    EEquipedSlot _currentSlot;
    List<EquipItem> _currentEquipList;    
    int _currentTab;
    bool _scrollFlag;
    bool _showShortageToast;

    PlayerStatusInfo Player => PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF);
    //int QualityCostId => DataManager.Instance.GetSettingContents().Equip_Quality_Consume_Item;
    int CompoundNeedCount => DataManager.Instance.GetSettingContents().Equip_Compound_Need_Count;
    ItemEquipManager Manager => ItemEquipManager.Instance;

    public override void InitUI()
    {
        _reforgePopup.SetActive(false);
        _reforge_ResultPopup.gameObject.SetActive(false);
        _mode = UIName == EUIName.UI_Equipment ? EMode.Equipment : EMode.Accessory;
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Currency, gameObject, UpdateStates);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Equip, gameObject, UpdateStates);
        _masteryPopup.Init(this);

        // 무기 종류 탭 설정
        foreach (BaseMenuBar_Menu baseMenu in MenuBar.MenuButtons)
        {
            MenuBar_Menu menu = (MenuBar_Menu)baseMenu;
            menu.Tweener.to = 305;
        }
        OnClickTab(0, false);

        // OSA 세팅
        _currentEquipList = Manager.Items.FindAll(equip => equip.EquipData.Equipedslot == _currentSlot);
        _gridView.Setting(_gridItemPrefab, CreateCell, UpdateCell, UpdateComplete);
        //_reforgeInfo.SetActive(false);
        _scrollFlag = true;

        // 마스터리버튼 레드닷 초기화
        _openMasteryOpenState.Setting(new() { EContent.EquipMastery });
        _openMasteryRedDot.SetAuto(new() { new RedDotComponent.RedDotInfo() { Content = EContent.EquipMastery } });

        base.InitUI();
    }

    public override void OnClosed()
    {
        StopAllCoroutines();
        Manager.RefreshEquipDataAll();
        PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).CalcUseWeaponState();
        base.OnClosed();
    }

    public override bool OnBackKey()
    {
        if (_reforgePopup.activeSelf)
        {
            ShowReforgePopup(false);
            return false;
        }
        else if (_masteryPopup.gameObject.activeSelf)
        {
            _masteryPopup.Show(false);
            return false;
        }
        return base.OnBackKey();
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback?.Invoke(true);

    protected override void RefreshUI(LanguageType langCode) { }

    void Refresh()
    {
        switch (_currentPanel)
        {
            case EPanel.Desc:
                RefreshEquipDesc();
                break;
            case EPanel.Craft:
                RefreshCraftPanel();
                break;
        }
        UpdateStates();
    }

    public override void OutOnlyMyself(bool isDirect = false)
    {
        _currentEquipList.ForEach(p => p.New = false);
        base.OutOnlyMyself(isDirect);
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State == UI_ButtonState.HoldEnd)
        {
            _showShortageToast = true;
        }
        EButtonType buttonType = EnumHelper.Parse(buttonInfo.ID, EButtonType.None);
        switch (buttonInfo.State)
        {
            case UI_ButtonState.Click:
                if (buttonInfo.ButtonType == UI_ButtonType.TAB)
                {
                    int tabIndex = int.Parse(buttonInfo.ID);
                    if (_currentTab == tabIndex) return;
                    _currentEquipList.ForEach(p => p.New = false);
                    _currentTab = tabIndex;
                    OnClickTab(tabIndex, true);
                    return;
                }
                switch (buttonType)
                {
                    case EButtonType.Compound:
                        OnClickCompound();
                        break;
                    case EButtonType.Equip:
                        OnClickEquip();
                        break;
                    case EButtonType.AutoEquip:
                        OnClickAutoEquip();
                        break;
                    case EButtonType.Enhance:
                        OnClickEnhance();
                        break;
                    //case EButtonType.Quality:
                    //    OnClickQuality();
                    //    break;
                    case EButtonType.CompoundAll:
                        OnClickCompoundAll();
                        break;
                    case EButtonType.Close:
                        OutUI();
                        break;
                    case EButtonType.OpenCraft:
                        SetCraftCount(1);
                        SetPanel(EPanel.Craft);
                        break;
                    case EButtonType.Craft_Close:
                        SetPanel(EPanel.Desc);
                        break;
                    case EButtonType.Craft_Min:
                        SetCraftCount(1);
                        break;
                    case EButtonType.Craft_Minus:
                        SetCraftCount(_craftCount - 1);
                        break;
                    case EButtonType.Craft_Plus:
                        SetCraftCount(_craftCount + 1);
                        break;
                    case EButtonType.Craft_Max:
                        SetCraftCount(_selectedEquip.GetMaxCraftCount());
                        break;
                    case EButtonType.Craft_Craft:
                        OnClickCraft();
                        break;
                    case EButtonType.OpenReforge:
                        ShowReforgePopup(true);
                        break;
                    case EButtonType.Reforge_Close: 
                        ShowReforgePopup(false);
                        break;
                    case EButtonType.Reforge_Reforge:
                        OnClickReforge();
                        break;
                    case EButtonType.OpenMastery:
                        _preview.ClearAll();
                        _reforgePreview.ClearAll();
                        _masteryPopup.Show(true, _currentTab);
                        break;
                }
                break;
            case UI_ButtonState.HoldEnd:
                switch (buttonType)
                {
                    case EButtonType.Compound:
                    case EButtonType.Equip:
                    case EButtonType.Enhance:
                    //case EButtonType.Quality:
                    case EButtonType.CompoundAll:                                        
                    case EButtonType.Craft_Craft:
                        UserDataManager.Save_LocalData();
                        break;
                }
                break;
        }
    }

    void UpdateStates()
    {
        _gridView.UpdateVisibleItems();
        switch (_currentPanel)
        {
            case EPanel.Desc:
                UpdateEquipDesc();
                break;
            case EPanel.Craft:
                UpdateCraftPanel();
                break;
        }
        if (_reforgePopup.activeSelf)
        {
            RefreshReforgePopup();
        }
    }


    void SetPanel(EPanel panel)
    {
        _currentPanel = panel;
        _descPanel.SetActive(panel == EPanel.Desc);
        _craftPanel.SetActive(panel == EPanel.Craft);
        Refresh();
    }

    public void SelectEquipment(EquipItem equip)
    {
        if (_selectedEquip == equip) return;
        _selectedEquip = equip;
        SetPanel(EPanel.Desc);
        _gridView.UpdateGrid();
        Refresh();

        _reforgePreview.ClearAll();
        if (_mode == EMode.Equipment)
        {
            _preview.gameObject.SetActive(true);
            _reforgePreview.gameObject.SetActive(true);
            _portrait.gameObject.SetActive(false);
            RefreshPreview(_preview);
        }
        else
        {
            _preview.gameObject.SetActive(false);
            _reforgePreview.gameObject.SetActive(false);
            _portrait.sprite = AtlasManager.GetItemIcon(equip, EIconType.Icon);
        }
    }

    void RefreshEquipDesc()
    {
        if (_selectedEquip == null) return;

        // 우측 장비정보
        FEquip data = _selectedEquip.EquipData;
        string spritePath = DataManager.Instance.GetSettingContents().ItemGrade_BgFrame1[(int)data.Grade - 1];
        _bg.sprite = AtlasManager.GetSprite(EATLAS_TYPE.UI_Parts, spritePath);
        _nameText.text = GetEquipName();
        //_nameText.color = InGameUtil.GetGradeColor(_currentSelectedEquip.EquipData.Grade);
        _gradeImage.sprite = InGameUtil.GetGradeTextSprite(data.Grade);
        FEquipReforging reforgeData = _selectedEquip.ReforgingInfo.GetNextStepData();
        if (reforgeData != null)
        {
            _levelText.text = $"<#63CCB8>{_selectedEquip.Enhance}</color> / {reforgeData.Needlevel}";
        }
        else
        {
            _levelText.text = $"{_selectedEquip.Enhance}";
        }
        SetStatString(_mainStat, EquipStatType.Main);
        SetStatString(_possess1Stat, EquipStatType.Possesion1);
        SetStatString2(_possess2Stat, EquipStatType.Possesion2);
        SetStatString2(_possess3Stat, EquipStatType.Possesion3);
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].SetActive(i < data.Star);
        }
        //_qualityBar.value = (float)_currentSelectedEquip.Quality / _currentSelectedEquip.MaxQuality;
        //_qualityText.text = _currentSelectedEquip.Quality.ToString();
        _selectedEquip.New = false;

        // 우측 하단 버튼들
        bool equipped = Player.GetEquipItem(_currentSlot) == _selectedEquip;
        _equipButton.interactable = !equipped && _selectedEquip.Have;
        _equipButtonText.text = equipped ? LocalizeManager.GetText("UI_Equip_Equiped") : LocalizeManager.GetText("UI_Btn_Equip_01");

        void SetStatString(Transform statObject, EquipStatType type)
        {
            TMP_Text statName = statObject.Find("StatName").GetComponent<TMP_Text>();
            TMP_Text statValue = statObject.Find("StatValue").GetComponent<TMP_Text>();
            StatValueInfo stat = _selectedEquip.GetStat(type);
            statName.text = stat.StatString;
            statValue.text = $"+{stat.ValueString}";
        }


        void SetStatString2(Transform statObject, EquipStatType type)
        {
            StatValueInfo stat = _selectedEquip.GetStat(type);
            bool unlock = stat.Value > 0;
            statObject.Find("Lock").gameObject.SetActive(!unlock);
            statObject.Find("Unlock").gameObject.SetActive(unlock);
            if (unlock)
            {
                TMP_Text statName = statObject.Find("Unlock/StatName").GetComponent<TMP_Text>();
                TMP_Text statValue = statObject.Find("Unlock/StatValue").GetComponent<TMP_Text>();
                statName.text = stat.StatString;
                statValue.text = $"+{stat.ValueString}";
            }
            else
            {
                TMP_Text txt = statObject.Find("Lock/Text").GetComponent<TMP_Text>();
                FEquipReforging reforgeData = type switch
                {
                    EquipStatType.Possesion2 => _selectedEquip.ReforgingInfo.Possesion2Data,
                    EquipStatType.Possesion3 => _selectedEquip.ReforgingInfo.Possesion3Data,
                    _ => null
                };
                txt.text = LocalizeManager.GetText("UI_Equip_AdvUnlock_Desc", $"+{reforgeData.Step}");
            }
        }
    }

    void UpdateEquipDesc()
    {
        if (_selectedEquip == null) return;

        //// 품질 버튼
        //EquipGrowthCostInfo cost = new()
        //{
        //    ItemID = QualityCostId,
        //    ItemCount = _currentSelectedEquip.GetQualityCost()
        //};
        //bool maxQualityReached = _currentSelectedEquip.Quality >= _currentSelectedEquip.MaxQuality;
        //RefreshButton(_qualityButton, cost, _currentSelectedEquip.CheckQualityLevelAndHave(), maxQualityReached, !_currentSelectedEquip.CheckQualityCost());
        //_qualityChanceText.text = $"{Formula.NumberToStringBy3_Percent(_currentSelectedEquip.GetQualityUpChance())}";

        // 강화, 돌파 버튼
        SettingLevelUpButton(_enhanceButton, _selectedEquip.GetEnhanceCost(), _selectedEquip.CheckMaxLevelAndHave());
        _openReforgeButton.interactable = _selectedEquip.Next2ReforgingData != null;
        bool reforgeActive = _selectedEquip.GetEnhanceType() == EquipEnhanceType.Reforging;
        _enhanceButton.gameObject.SetActive(!reforgeActive);
        _openReforgeButtonBottom.gameObject.SetActive(reforgeActive);
        _openReforgeBottomRedDot.SetActiveRedDot(_selectedEquip.CheckEnhanceCost());
        //_enhanceButton.SetHoldable(!reforgeActive);
        //_enhanceButtonTitleText.text = reforgeActive ? LocalizeManager.GetText("UI_Btn_Equip_Reforging") : LocalizeManager.GetText("UI_Btn_Equip_Enhance");
        //_reforgeInfo.SetActive(reforgeActive);
        //if (reforgeActive)
        //{
        //    _reforgeInfo_RateText.text = Formula.NumberToStringBy3_Percent((long)_currentSelectedEquip.NextReforgingData.Rate);
        //}

        // 일괄합성, 합성, 제작, 변환 버튼
        List<EquipItem> items = _mode switch
        {
            EMode.Equipment => Manager.Weapons,
            EMode.Accessory => Manager.Accessorys,
            _ => default
        };
        _compoundAllButton.interactable = items.Any(x => x.CanCompound());
        EEquipCompoundType type = _selectedEquip.GetCompoundType();
        _compoundButton.gameObject.SetActive(type == EEquipCompoundType.Compound);
        _compoundButton.interactable = _selectedEquip.CanCompound();
        _craftButton.gameObject.SetActive(type == EEquipCompoundType.Craft);
        _conversionButton.gameObject.SetActive(type == EEquipCompoundType.Conversion);
    }

    (ScrollItemInfo itemInfo, RectTransform prefab) CreateCell(int idx)
    {
        RectTransform itemObj = _gridItemPrefab.transform as RectTransform;
        ScrollItemInfo itemInfo = new()
        {
            obj = _currentEquipList[idx]
        };
        return (itemInfo, itemObj);
    }

    ScrollItemInfo UpdateCell(int idx, ScrollItemInfo itemInfo, RectTransform prefab)
    {
        EquipItem equipItem = itemInfo.obj as EquipItem;
        bool equipped = equipItem == Player.GetEquipItem(equipItem.Slot);
        bool selected = equipItem == _selectedEquip;        
        var obj = prefab.GetComponent<UI_Equipment_Element>();        
        obj.Setting(this, equipItem, equipped, selected);
        itemInfo.tag = obj.name;
        return itemInfo;
    }

    void UpdateComplete(int value0, int value1)
    {
        if (_scrollFlag)
        {
            _scrollFlag = false;
            EquipItem equipment = Player.GetEquipItem(_currentSlot);
            equipment ??= _currentEquipList[0];
            if (equipment != null)
            {
                int index = _currentEquipList.IndexOf(equipment);
                if (!GuideTutorialManager.Instance.IsIng())
                    _gridView.MoveTo(index, false);
                SelectEquipment(equipment);
            }
        }
    }

    bool _initRedDotTabs = false;

    void OnClickTab(int idx, bool tween)
    {
        _currentTab = idx;
        _tweeningTab.Select(idx, tween);
        EEquipedSlot[] list = GetSlotTypes();
        if(_initRedDotTabs == false)
        {
            for (int i = 0; i < list.Length; i++)
            {
                var lst = Manager.Items.FindAll(equip => equip.EquipData.Equipedslot == list[i]);
                //var b = _tabGroup.Buttons.Find(p => p.ID == i + "");
                //b.GetComponentInChildren<RedDot_EquipTab>().Setting(lst);
                _tweeningTab.Buttons[i].GetComponentInChildren<RedDot_EquipTab>().Setting(lst);
            }
            _initRedDotTabs = true;
        }
        _currentSlot = list[idx];
        _currentEquipList = Manager.Items.FindAll(equip => equip.EquipData.Equipedslot == _currentSlot);
        _gridView.SetItems(_currentEquipList.Count);
        _gridView.MoveTo(0);
        _gridView.GetComponent<UIPlayTween>().Play();
        _scrollFlag = true;
        Refresh();
    }

    void OnClickEquip()
    {
        EquipItem equip = Player.GetEquipItem(_currentSlot) == _selectedEquip ? null : _selectedEquip;
        Equip(_currentSlot, equip);
    }

    void OnClickAutoEquip()
    {
        EEquipedSlot[] slots = GetSlotTypes();
        foreach (EEquipedSlot slot in slots)
        {
            EquipItem topEquipment = Manager.Items.Where(x => x.Slot == slot && x.Have)
                .OrderByDescending(x => x.GetStat(EquipStatType.Main).Value)
                .ThenByDescending(x => x.Id).FirstOrDefault();
            Equip(slot, topEquipment);
        }
        EquipItem equipped = Player.GetEquipItem(_currentSlot);
        if (equipped != null)
        {
            _gridView.ScrollTo(_currentEquipList.IndexOf(equipped), 0.5f);
            SelectEquipment(equipped);
        }
    }

    void OnClickCompound()
    {
        CompoundResultInfo result = Manager.Compound(_selectedEquip);
        StartCoroutine(ShowCompoundPopup(LocalizeManager.GetText("UI_Compose_Result_Title_01"), result)); 
        RefreshStatValue();
        Refresh();
    }

    void OnClickEnhance()
    {
        if (_selectedEquip == null) return;
        if (!_selectedEquip.CheckEnhanceCost())
        {
            if (_showShortageToast)
            {
                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                _showShortageToast = false;
            }
            return;
        }

        _selectedEquip.EnhanceUp();
        RefreshStatValue();
        UIRoot.Instance.PlayLevelUpEffect(transform, null, null);
        Play2DEffect(id: "Fx_UI_Upgrade_Equipment_Gauge_Line_01",
            position: _effectPosition_Enhance.localPosition,
            parent: _effectPosition_Enhance.parent,
            reappearDelay: 0.2f);
        Refresh();
    }

    //void OnClickQuality()
    //{
    //    if (_currentSelectedEquip == null) return;
    //    if (!_currentSelectedEquip.CheckQualityCost())
    //    {
    //        if (_showShortageToast)
    //        {
    //            UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
    //            _showShortageToast = false;
    //        }
    //        return;
    //    }

    //    bool success = _currentSelectedEquip.QualityUp();
    //    if (success)
    //    {
    //        RefreshStatValue();
    //        UIRoot.Instance.PlayLevelUpEffect(transform, null, null);
    //        Play2DEffect(id: "Fx_UI_Upgrade_Equipment_Gauge_01_Success",
    //            position: _effectPosition_Quality.localPosition,
    //            parent: _effectPosition_Quality.parent,
    //            reappearDelay: 0.2f);
    //    }
    //    else
    //    {
    //        Play2DEffect(id: "Fx_UI_Upgrade_Equipment_Gauge_01_Fail",
    //            position: _effectPosition_Quality.localPosition,
    //            parent: _effectPosition_Quality.parent,
    //            reappearDelay: 0.2f);
    //    }        
    //    Refresh();
    //}

    void OnClickCompoundAll()
    {
        CompoundResultInfo result = null;
        switch (_mode)
        {
            case EMode.Equipment:
                result = Manager.CompoundAll_Equip();
                break;
            case EMode.Accessory:
                result = Manager.CompoundAll_Accessory();
                break;
        }
        StartCoroutine(ShowCompoundPopup(LocalizeManager.GetText("UI_Compose_Result_Title_02"), result));
        RefreshStatValue();
        Refresh();
    }

    IEnumerator ShowCompoundPopup(string titleText, CompoundResultInfo result)
    {
        List<int> itemIds = new();
        List<BigInteger> itemCounts = new();
        for (int i = 0; i < (int)EEquipedSlot.MAXCOUNT; i++)
        {
            EquipItem item = result.LastCompoundItems[i];
            if (item == null) continue;
            itemIds.Add(result.LastCompoundItems[i].Id);
            itemCounts.Add(result.LastCompoundItemCounts[i]);
        }
        BigInteger compoundedCount = result.CompoundCount * CompoundNeedCount;
        _preview.ClearAll();
        yield return StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
        {
            TitleText = titleText,
            DescText = LocalizeManager.GetText("UI_Compose_Result_Desc", compoundedCount.ToString("N0")),
            ItemIds = itemIds,
            ItemCounts = itemCounts,
        }));
        RefreshPreview(_preview);
    }

    void Equip(EEquipedSlot slot, EquipItem equip)
    {
        Player.UnregisterEquipItem(slot);
        Player.RegisterEquipItem(equip);
        RefreshStatValue();
        Player.RefreshValues(true);

        if (equip == null) return;
        var player = BattleSceneManager.Instance.GetBattleScene()?.PlayerUnit;
        CharacterOutfitManager.Instance.SetOutfit(GameLoop.Instance.HeroID, equip.ModelID);
        var type = player.UnitStateType == EUnitState.Skill && player.NowSkill != null ? (EUseWeapon)player.NowSkill.skillItemInfo.SkillData.Useweapon : player.CurrentWeapon;
        player.RefreshSkin();
        player.ActiveWeaponModelOnly(type);
        Refresh();
    }

    void RefreshStatValue()
    {
        if (_selectedEquip != Player.GetEquipItem(_currentSlot)) return;
        Player.CalcUseWeaponState(_currentSlot.ToUseWeapon());
        Player.RefreshValues(true);
    }

    string GetEquipName()
    {
        return $"+{_selectedEquip.ReforgingInfo.Level} {LocalizeManager.GetText(_selectedEquip.EquipData.Nameid)}";
    }

    void SettingLevelUpButton(UI_Button button, EquipGrowthCostInfo cost, bool interactable)
    {
        Image image = button.transform.Find("Image").GetComponent<Image>();
        TMP_Text tmp = button.transform.Find("CostText").GetComponent<TMP_Text>();
        button.interactable = interactable;
        image.gameObject.SetActive(cost.ItemID != 0);
        image.sprite = AtlasManager.GetItemIcon(cost.ItemID, EIconType.MiniIcon);
        if (!_selectedEquip.IsMaxLevel)
        {
            tmp.text = Formula.NumberToStringBy3(cost.ItemCount);
            UIRoot.Instance.SetButtonItemShortage(button, !_selectedEquip.CheckEnhanceCost());
        }
        else
        {
            tmp.text = "MAX";
            UIRoot.Instance.SetButtonItemShortage(button, false);
        }
    }

    EEquipedSlot[] GetSlotTypes()
    {
        EEquipedSlot[] list = null;
        switch (_mode)
        {
            case EMode.Equipment:
                list = new EEquipedSlot[] { EEquipedSlot.Weapon1, EEquipedSlot.Weapon2, EEquipedSlot.Weapon3 };
                break;
            case EMode.Accessory:
                list = new EEquipedSlot[] { EEquipedSlot.Accessory1, EEquipedSlot.Accessory2, EEquipedSlot.Accessory3 };
                break;
        }
        return list;
    }

    void RefreshPreview(PreviewSpace_ preview)
    {
        if (_mode != EMode.Equipment) return;
        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return null;
            AddModel(preview);
        }

        void AddModel(PreviewSpace_ preview)
        {
            preview.ClearAll();
            FEquip data = _selectedEquip.EquipData;
            PreviewModelInfo model = preview.AddModel("npc1001", new string[1] { data.Weaponid }, model =>
            {
                var tr = preview.ModelContainer.Find(((EEquipedSlot)data.Equipedslot).ToUseWeapon().ToString());
                model.SetScale(tr.localScale.x);
                model.GetOwnerModel().transform.localPosition = tr.localPosition;
                for (int i = 0; i < (int)EModelPartType.MAXCOUNT; i++)
                {
                    var part = (EModelPartType)i;
                    model.SetActivePartOfType(((EEquipedSlot)data.Equipedslot).ToUseWeapon().ToModelPartType() == part, part);
                }
            });
            int aniGroup = ((EEquipedSlot)data.Equipedslot).ToUseWeapon().ToAnimationGroup();
            model.AnimationInfo = new()
            {
                Start = EAniType.Ani_WM_Idle,
                Idle = EAniType.Ani_WM_Idle,
                AnimationGroup = aniGroup,
                ActionAnimations = new(),
            };
        }
    }
}
