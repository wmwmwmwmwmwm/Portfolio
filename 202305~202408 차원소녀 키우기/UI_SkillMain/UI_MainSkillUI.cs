using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public partial class UI_MainSkillUI : UICell
{
    enum ETabId
    {
        None,
        Weapon,
        Set,
    }

    public enum EButtonId
    {
        None,
        Close,
        SkillIconElement,
        SkillIconMinus,
        SkillInfoElement,
        AutoEquip,
    }

    [SerializeField] UI_TabGroup _weaponButtons;
    [SerializeField] UI_TabGroup _slotSetButtons;
    [SerializeField] List<SkillIconElement> _slotSkillItems = null;
    [SerializeField] List<GameObject> _slotSkillArrows = new List<GameObject>();

    [SerializeField] ScrollRect _skillInfoScroll;
    [SerializeField] Transform _skillInfoParent;
    [SerializeField] SkillInfoElement _SkillInfoElementPrefab;

    [SerializeField] UI_Button _completeButton;
    [SerializeField] TMP_Text _completeButton_text;
    [SerializeField] UI_Button _useOrderButton;
    [SerializeField] TMP_Text _useOrderButton_text;

    [SerializeField] UI_SkillDetail _ui_skillDetail;    

    [SerializeField, ReadOnly] EUseWeapon _selectWeapon = EUseWeapon.Handgun;
    [SerializeField, ReadOnly] ObscuredInt _selectSet = 0;
    [SerializeField, ReadOnly] SkillItem __selectSkill;
    SkillItem _selectSkill
    {
        get => __selectSkill;
        set
        {
            __selectSkill = value;
            Refresh_GuideTutorialSelectValue();
        }
    }

    List<SkillItem>[] _allSkills = new List<SkillItem>[(int)EUseWeapon.MAXCOUNT];
    List<SkillInfoElement> _skillInfoElements;
    //SkillAllSlotInfos _skillAllSlotInfos = null;
    SkillSlotSet _skillAllSlotInfos = null;
    UI_SkillMain _parentUI;

    //SkillSlotWeapons CurrentSkillSet => _skillAllSlotInfos.PlayerSkillSlots[_selectSet];
    SkillSlots CurrentSlots => _skillAllSlotInfos[0][(int)_selectWeapon];
    public int SetIndex => _selectSet;

    public void Init(UI_SkillMain parentUI)
    {
        _skillInfoElements = new();
        _parentUI = parentUI;
        _skillAllSlotInfos = PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).SkillSlots;
        //_selectSet = _skillAllSlotInfos.GetSlotSetIdx(BattleSceneManager.Instance.GameMode);
        _selectWeapon = EUseWeapon.Handgun;

        _weaponButtons.Init();
        _weaponButtons.SetSelectButton((int)_selectWeapon - 1);
        _slotSetButtons.Init();
        _slotSetButtons.SetSelectButton(_selectSet);

        List<SkillItem> skills = InventoryManager.Instance.SkillItems.Where(p => /*p.Have &&*/ p.SkillData.Levelupgroupid > 0).ToList();
        for (int i = 1; i < _allSkills.Length; i++)
        {
            _allSkills[i] = new List<SkillItem>();
            _allSkills[i] = skills.Where(p => p.SkillData.Useweapon == (EUseWeapon)i).ToList();
            var redDot = _weaponButtons.Buttons.Find(p => p.ID.Contains(i + "")).GetComponentInChildren<RedDot_SkillTab>();
            redDot.Setting(_allSkills[i]);
        }

        _draggingFront.SetActive(false);
        _draggingHandle.gameObject.SetActive(false);
    }

    public void OutUI()
    {
        _allSkills[(int)_selectWeapon].ForEach(p => p.New = false);
        UIManager.Instance.FindOpenUI(EUIName.UI_Hud)?.OnRefreshUI();
    }

    public void Show(int setIndex)
    {
        if (gameObject.activeSelf) return;
        gameObject.SetActive(true);
        _selectSkill = null;
        _parentUI.OnRefreshUI();
    }

    public void Refresh_GuideTutorialSelectValue()
    {
        if (!GuideTutorialManager.Instance.IsIng())
            return;
        _parentUI.SetSelectValue("");
        if (_ui_skillDetail.IsShow)
        {
            _parentUI.SetSelectValue(_ui_skillDetail.Skillitem.Id.ToString());
            if (!_parentUI.GetSelectValue().Contains("_Info"))
                _parentUI.SetSelectValue(_parentUI.GetSelectValue() + "_Info");
        }
        if(_selectSkill != null)
        {
            if (string.IsNullOrEmpty(_parentUI.GetSelectValue()))
                _parentUI.SetSelectValue(_selectSkill.Id.ToString());
            else if (!_parentUI.GetSelectValue().Contains(_selectSkill.Id.ToString()))
                _parentUI.SetSelectValue(_parentUI.GetSelectValue() + "_" + _selectSkill.Id.ToString());
            if (!_parentUI.GetSelectValue().Contains("_Select"))
                _parentUI.SetSelectValue(_parentUI.GetSelectValue() + "_Select");
        }
        if(_draggingSkill != null)
        {
            if (string.IsNullOrEmpty(_parentUI.GetSelectValue()))
                _parentUI.SetSelectValue(_draggingSkill.Id.ToString());
            else if (!_parentUI.GetSelectValue().Contains(_draggingSkill.Id.ToString()))
                _parentUI.SetSelectValue(_parentUI.GetSelectValue() + "_" + _draggingSkill.Id.ToString());
            if (!_parentUI.GetSelectValue().Contains("_Select"))
                _parentUI.SetSelectValue(_parentUI.GetSelectValue() + "_Select");
        }
    }

    public bool OnBackKey()
    {
        return true;
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        switch (buttonInfo.ButtonType)
        {
            case UI_ButtonType.TAB:
                if (buttonInfo.State != UI_ButtonState.Click) return;
                TabButton(buttonInfo);
                break;
            default:
                NormalButton(buttonInfo);
                break;
        }

        string[] temp = buttonInfo.ID.Split('_');
        EButtonId buttonId = EnumHelper.Parse(temp[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.SkillIconElement:
            case EButtonId.SkillInfoElement:
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
                break;
        }

        _parentUI.EventButton(buttonInfo);
    }

    void TabButton(UIButtonInfo buttonInfo)
    {
        var temp = buttonInfo.ID.Split('_');
        int idx = 0;
        if (temp.Length > 1)
            idx = int.Parse(temp[1]);
        switch (EnumHelper.Parse(temp[0], ETabId.None))
        {
            case ETabId.Weapon:
                if (_selectWeapon != (EUseWeapon)idx)
                    _allSkills[(int)_selectWeapon].ForEach(p => p.New = false);
                _selectWeapon = (EUseWeapon)idx;
                Create_HaveSkills();
                _parentUI.OnRefreshUI();
                break;
            case ETabId.Set:
                _selectSet = idx - 1;
                Refresh_Slots();
                break;
        }
    }

    void NormalButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;
        string[] temp = buttonInfo.ID.Split('_');
        EButtonId buttonId = EnumHelper.Parse(temp[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.Close:
                _parentUI.OutUI();
                break;
            case EButtonId.SkillIconElement:
                SkillIconElement skillIcon1 = buttonInfo.Trans.GetComponent<SkillIconElement>();
                int idx = int.Parse(temp[1]) - 1;
                if (_selectSkill == null)
                {
                    if (skillIcon1.Skill != null)
                    {
                        SelectSkill(skillIcon1.Skill);
                        SkillInfoElement focusItem = _skillInfoElements.Find(x => x.Skill?.Id == _selectSkill?.Id);
                        if (focusItem != null)
                        {
                            _skillInfoScroll.FocusOnItem(focusItem.GetComponent<RectTransform>());
                        }
                    }
                }
                else
                {
                    EquipSkill(idx, _selectSkill);
                }
                break;
            case EButtonId.SkillIconMinus:
                SkillIconElement skillIcon2 = buttonInfo.Trans.GetComponentInParent<SkillIconElement>();
                UnequipSkill(skillIcon2);
                break;
            case EButtonId.SkillInfoElement:
                SkillInfoElement skillInfo = buttonInfo.Trans.GetComponent<SkillInfoElement>();
                SelectSkill(skillInfo.Skill);
                break;
            case EButtonId.AutoEquip:
                OnClickAutoEquip();
                break;
        }
    }

    public void Refresh_Slots()
    {
        bool selectSkillIsNormalSkill = _selectSkill?.SkillData.Skillstyle == ESkillStyle.Normal;
        for (int i = 0; i < _slotSkillItems.Count; i++)
        {
            bool isNormalSlot = i < SkillSlots.CostumeSlotIndex;
            SkillItem skillitem = CurrentSlots[i].ItemInfo;
            _slotSkillItems[i].Setting(skillitem);
            _slotSkillItems[i].SetActiveMinusButton(true);

            var openButtonstat = _slotSkillItems[i].GetComponentInChildren<UIOpenButtonStat>();
            if (openButtonstat == null || openButtonstat.IsUnLock())
            {
                _slotSkillArrows[i].SetActive(_selectSkill != null && _selectSkill.Have && selectSkillIsNormalSkill == isNormalSlot);
            }
            else
            {
                _slotSkillArrows[i].SetActive(false);
            }
        }
        BattleSceneManager.Instance.GetBattleScene()?.PlayerUnit?.RefreshSkillList();
        //_skillAllSlotInfos.RefreshOrderSkill(SetIndex);
    }

    [SerializeField] TMP_Text _haveNoSkillText;
    public void Create_HaveSkills()
    {
        _skillInfoElements.ForEach(x => Destroy(x.gameObject));
        _skillInfoElements.Clear();
        List<SkillItem> skillList = _allSkills[(int)_selectWeapon];
        if (skillList.Count > 0)
        {
            _haveNoSkillText.gameObject.SetActive(false);
            foreach (SkillItem skill in skillList)
            {
                SkillInfoElement element = Instantiate(_SkillInfoElementPrefab, _skillInfoParent);
                element.gameObject.name = "SkillInfoButton_" + skill.Id;
                element.Setting(skill);
                element.SetSelect(_selectSkill == element.Skill);
                _skillInfoElements.Add(element);
            }
            _skillInfoElements = _skillInfoElements.OrderByDescending(x => x.Skill.Have)
                .ThenBy(x => x.Skill.Id)
                .ToList();
            Refresh_HaveSkills();
        }
        else
        {
            _haveNoSkillText.gameObject.SetActive(true);
            string key = _selectWeapon switch
            {
                EUseWeapon.Handgun => "UI_Skill_Empty_HG",
                EUseWeapon.Sword => "UI_Skill_Empty_Sword",
                EUseWeapon.Hammer => "UI_Skill_Empty_HM",
                _ => ""
            };
            _haveNoSkillText.text = LocalizeManager.GetText(key);
        }
    }

    public void Refresh_HaveSkills()
    {
        foreach (SkillInfoElement element in _skillInfoElements)
        {
            element.Setting(element.Skill);
            element.SetSelect(_selectSkill == element.Skill);
            element.transform.SetAsLastSibling();
        }
    }

    void SetSkillSlot(int index, SkillItem skill)
    {
        if (_selectWeapon != skill.SkillData.Useweapon) return;
        bool isCostumeSlot = index == SkillSlots.CostumeSlotIndex;
        if (skill.IsCostumeSkill != isCostumeSlot) return;
        
        int sameSkillIdx = CurrentSlots.FindSkillIdx(skill.Id);
        if (sameSkillIdx >= 0)
        {
            CurrentSlots[sameSkillIdx].SkillId = CurrentSlots[index].SkillId;
        }
        CurrentSlots[index].SkillId = skill.Id;
    }

    public UI_Button GetSlotButton(int index)
    {
        return _slotSkillItems[index].GetComponentInChildren<UI_Button>(true);        
    }

    void SelectSkill(SkillItem skill)
    {
        _selectSkill = skill;
        _ui_skillDetail.Show(_parentUI, _selectSkill, UI_SkillDetail.ETabId.Grow);
        Refresh_Slots();
        Refresh_HaveSkills();
    }

    void EquipSkill(int idx, SkillItem skill)
    {
        UIOpenButtonStat openButtonstat = _slotSkillItems[idx].GetComponentInChildren<UIOpenButtonStat>();
        if (openButtonstat && !openButtonstat.IsUnLock()) return;
        if (!skill.Have) return;
        SetSkillSlot(idx, skill);
        _selectSkill = null;
        Refresh_Slots();
        Refresh_HaveSkills();
    }

    void UnequipSkill(SkillIconElement skillIcon)
    {
        int slotIdx = CurrentSlots.FindSkillIdx(skillIcon.Skill.Id);
        CurrentSlots[slotIdx].SkillId = 0;
        Refresh_Slots();
    }

    void OnClickAutoEquip()
    {
        // 일반 스킬 자동장착
        List<SkillInfoElement> normalSkills = _skillInfoElements.FindAll(x => x.Skill.Have && x.Skill.SkillData.Skillstyle == ESkillStyle.Normal);
        normalSkills = Sort(normalSkills);
        for (int i = 0; i < _slotSkillItems.Count - 1; i++)
        {
            if (i < normalSkills.Count)
            {
                EquipSkill(i, normalSkills[i].Skill);
            }
        }

        // 코스튬 스킬 자동장착
        List<SkillInfoElement> costumeSkills = _skillInfoElements.FindAll(x => x.Skill.Have && x.Skill.SkillData.Skillstyle == ESkillStyle.Costume);
        costumeSkills = Sort(costumeSkills);
        if (costumeSkills.Count > 0)
        {
            EquipSkill(_slotSkillItems.Count - 1, costumeSkills[0].Skill);
        }

        List<SkillInfoElement> Sort(List<SkillInfoElement> items)
        {
            return items.OrderByDescending(x => x.Skill.itemData.Grade)
            .ThenByDescending(x => x.Skill.Level)
            .ThenByDescending(x => x.Skill.Id).ToList();
        }
    }
}
