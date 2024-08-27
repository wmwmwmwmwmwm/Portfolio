using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillMain_SkillElement : MonoBehaviour
{
    public enum EElementButtonId
    {
        None,
        Select,
        Equip,
        LevelUp,
    }

    [SerializeField] UI_Button _button;
    [SerializeField] Image _icon, _grade;
    [SerializeField] Slider _itemCountSlider;
    [SerializeField] TMP_Text _itemCountText;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] UI_Button _equipButton, _levelUpButton;
    [SerializeField] TMP_Text _equipButtonText;
    [SerializeField] GameObject _equipMark, _notHaveBg, _notHaveMark;
    [SerializeField] GameObject _selected;
    [SerializeField] Transform _effectPosition_Icon, _effectPosition_Desc;
    [SerializeField] RedDotComponent _redDot;
    [SerializeField] Image _newIcon;

    SkillItem _skill;

    public SkillItem Skill => _skill;
    public Transform EffectPosition_Icon => _effectPosition_Icon;
    public Transform EffectPosition_Desc => _effectPosition_Desc;

    public void SetSkill(SkillItem skill)
    {
        _skill = skill;
    }

    public void Setting(bool selected, bool equip)
    {
        _button.SetID($"{UI_SkillMain.EButtonId.Skill}__{EElementButtonId.Select}");
        _equipButton.SetID($"{UI_SkillMain.EButtonId.Skill}__{EElementButtonId.Equip}");
        _levelUpButton.SetID($"{UI_SkillMain.EButtonId.Skill}__{EElementButtonId.LevelUp}");
        _levelUpButton.gameObject.name = $"LevelUpButton_{_skill.Id}";
        (int needCount, int itemId, int itemCount) = _skill.GetSpendCost();
        _icon.sprite = AtlasManager.GetItemIcon(_skill, EIconType.Icon);
        _grade.sprite = InGameUtil.GetGradeTextSprite(_skill.itemData.Grade);
        _itemCountSlider.value = (float)((double)_skill.ConsumableCount / needCount);
        _itemCountText.text = $"{_skill.ConsumableCount}/{needCount}";
        _nameText.text = $"Lv.{_skill.Level} {LocalizeManager.GetText(_skill.SkillData.Nameid)}";
        bool buttonActive = selected && _skill.Have;
        _equipButton.gameObject.SetActive(buttonActive);
        _equipButtonText.text = LocalizeManager.GetText(equip ? "UI_Btn_Skill_Release" : "UI_Btn_Skill_Equip");
        _levelUpButton.gameObject.SetActive(buttonActive);
        _equipMark.SetActive(!buttonActive && equip);
        bool notHave = !buttonActive && !_skill.Have;
        _notHaveBg.SetActive(notHave);
        _notHaveMark.SetActive(notHave);
        _selected.SetActive(selected);
        bool canLevelUp = _skill.CheckLevelUp(out bool notEnoughCost);
        _levelUpButton.interactable = canLevelUp || notEnoughCost;
        UIRoot.Instance.SetButtonItemShortage(_levelUpButton, notEnoughCost);
        _redDot.SetActiveRedDot(_skill.CheckLevelUp(out _));
        _newIcon.gameObject.SetActive(_skill.New);
    }
}
