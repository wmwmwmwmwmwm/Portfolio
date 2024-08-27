using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public class UI_SkillDetail_Grow : UICell
{
    enum EButtonId
    {
        None,
        UpgradeButton,
    }

    [SerializeField] TMP_Text _infoText;
    [SerializeField] TMP_Text _infoAfterText;
    [SerializeField] GameObject _additionalInfo;
    [SerializeField] TMP_Text _additionalInfo_TitleText, _additionalInfo_InfoText;

    [SerializeField] GameObject _materialGroup;
    [SerializeField] Image _materialSkill;
    [SerializeField] TMP_Text _materialSkill_spendCount;
    [SerializeField] Image _materialItem;
    [SerializeField] TMP_Text _materialItem_spendCount;

    [SerializeField] UI_Button _upgradeButton;
    [SerializeField] TMP_Text _upgradeButtonText;

    [SerializeField] Transform _effectPosition_Icon;
    [SerializeField] Transform _effectPosition_Desc;

    UI_SkillDetail _parent;
    UI_EffectComponent _effectComponent;
    SkillItem _skillitem;
    //ESkillLevelUpType prvLevelUpType = ESkillLevelUpType.None;
    bool isChangeData = false;
    bool _showShortageToast;

    public void Show(SkillItem item, UI_SkillDetail parent)
    {
        _effectComponent = GetComponentInParent<UI_EffectComponent>();
        _parent = parent;
        _skillitem = item;
        isChangeData = false;
        Refresh();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State == UI_ButtonState.HoldEnd)
        {
            _showShortageToast = true;
        }

        EButtonId buttonId = EnumHelper.Parse(buttonInfo.ID, EButtonId.None);
        switch (buttonInfo.State)
        {
            case UI_ButtonState.Click:
                switch (buttonId)
                {
                    case EButtonId.UpgradeButton:
                        //if (buttonInfo.HoldingClickCount > 1 && prvLevelUpType != _skillitem.GetLevelUpType())
                        //    break;

                        //prvLevelUpType = _skillitem.GetLevelUpType();
                        bool success = _skillitem.LevelUp();
                        if (!success)
                        {
                            if (_showShortageToast)
                            {
                                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                                _showShortageToast = false;
                            }
                            return;
                        }

                        // 강화 이펙트 출력
                        //string effectId = "";
                        //Transform effectPosition = null;
                        //switch (prvLevelUpType)
                        //{
                        //    case ESkillLevelUpType.Level:
                        //        effectId = "Fx_UI_Upgrade_Icon_Skill_01";
                        //        effectPosition = _effectPosition_Icon;
                        //        break;
                        //    case ESkillLevelUpType.Advance:
                        //    case ESkillLevelUpType.Transcendence:
                        //        effectId = "Fx_UI_Upgrade_Skill_Line_01";
                        //        effectPosition = _effectPosition_Desc;
                        //        break;
                        //}
                        //_effectComponent.Play2DEffect(id: effectId,
                        //    position: effectPosition.localPosition,
                        //    parent: effectPosition.parent,
                        //    reappearDelay: 0.2f);
                        _effectComponent.Play2DEffect(id: "Fx_UI_Upgrade_Icon_Skill_01",
                            position: _effectPosition_Icon.localPosition,
                            parent: _effectPosition_Icon.parent,
                            reappearDelay: 0.2f);
                        isChangeData = true;
                        _parent.Refresh();
                        (UIManager.Instance.FindOpenUI(EUIName.UI_SkillMain) as UI_SkillMain)?.OnRefreshUI();
                        BattleSceneManager.Instance.GetBattleScene()?.PlayerUnit?.RefreshSkillList();
                        break;
                }
                break;
            case UI_ButtonState.HoldEnd:
                {
                    if (isChangeData)
                        UserDataManager.Save_LocalData();
                    isChangeData = false;
                }
                break;
        }
    }

    public void Refresh()
    {
        SetMaterialInfo();

        //if (_skillitem.GetLevelUpType() == ESkillLevelUpType.None)
        //{
        //    _upgradeButton.gameObject.SetActive(false);
        //}
        //else
        //{
        //    _upgradeButton.gameObject.SetActive(true);
        //    UIRoot.Instance.SetButtonItemShortage(_upgradeButton, !_skillitem.CheckLevelUp());
        //}
        SetBottomInfo_Level();
        _additionalInfo.SetActive(false);
        //switch (_skillitem.GetLevelUpType())
        //{
        //    case ESkillLevelUpType.Advance:
        //        _additionalInfo.SetActive(true);
        //        SetAdditionalInfo_Advance();
        //        break;
        //    case ESkillLevelUpType.Transcendence:
        //        _additionalInfo.SetActive(true);
        //        SetAdditionalInfo_Transcendence();
        //        break;
        //}

    }

    void SetMaterialInfo()
    {
        Color enableColor = new Color(0.6980392f, 1f, 1f);
        Color disableColor = new Color(0.5019608f, 0.5019608f, 0.5019608f);

        //if (_skillitem.GetLevelUpType() == ESkillLevelUpType.None)
        //{
        //    _materialGroup.SetActive(false);
        //}
        //else
        //{
        //    _materialGroup.SetActive(true);

        //    var costInfo = _skillitem.GetSpendCost();
        //    var itemInfo = InventoryManager.Instance.GetItem(costInfo.itemId);
        //    _materialSkill.sprite = AtlasManager.GetItemIcon(_skillitem, EIconType.Icon);
        //    var skillCountStr = Formula.NumberToStringBy3(_skillitem.Count);
        //    var skillCostCountStr = Formula.NumberToStringBy3(costInfo.skillCount);
        //    _materialSkill_spendCount.text = $"{skillCountStr}/{skillCostCountStr}";
        //    _materialSkill_spendCount.color = (_skillitem.Count >= costInfo.skillCount) ? enableColor : disableColor;

        //    if (itemInfo != null)
        //    {
        //        _materialItem.transform.parent.gameObject.SetActive(true);
        //        _materialItem.sprite = AtlasManager.GetItemIcon(itemInfo, EIconType.Icon);
        //        var itemCountStr = Formula.NumberToStringBy3(itemInfo.Count);
        //        var itemCostCountStr = Formula.NumberToStringBy3(costInfo.itemCount);
        //        _materialItem_spendCount.text = $"{itemCountStr}/{itemCostCountStr}";
        //        _materialItem_spendCount.color = (itemInfo.Count >= costInfo.itemCount) ? enableColor : disableColor;
        //    }
        //    else
        //    {
        //        _materialItem.transform.parent.gameObject.SetActive(false);
        //    }
        //}
    }

    void SetBottomInfo_Level()
    {

        //if (_skillitem.TranscendenceStar > 0)
        //{
        //    _upgradeButtonText.text = LocalizeManager.GetText("UI_Btn_TransLevelUp");
        //}
        //else
        //{
        //    _upgradeButtonText.text = LocalizeManager.GetText("UI_Btn_SkillLevelUp");
        //}

        //_infoText.text = GetSkillDesc(_skillitem.Level);

        //if (_skillitem.GetLevelUpType() != ESkillLevelUpType.None)
        //{
        //    _infoAfterText.text = GetSkillDesc(_skillitem.Level + 1);
        //}
        //else
        //{
        //    _infoAfterText.text = LocalizeManager.GetText("UI_Skill_LevelUp_Desc_Max");
        //}

        //string GetSkillDesc(int level)
        //{
        //    string levelUpPower = Formula.NumberToStringBy3_Percent(_skillitem.GetMaxPower(level), false);
        //    int transcAddPower = _skillitem.LinkSkills.Sum(x => DataManager.Instance.GetSkillData(x).Skillpowermax) / 100;
        //    if (transcAddPower > 0)
        //    {
        //        string powerText = $"{levelUpPower}<#FFE665>+{transcAddPower}%</color>";
        //        return LocalizeManager.GetText(_skillitem.SkillData.Descid, powerText);
        //    }
        //    else
        //    {
        //        return LocalizeManager.GetText(_skillitem.SkillData.Descid, levelUpPower);
        //    }
        //}
    }

    void SetAdditionalInfo_Advance()
    {
        //_upgradeButtonText.text = LocalizeManager.GetText("UI_Btn_Advance");

        //_additionalInfo_TitleText.text = LocalizeManager.GetText("UI_Skill_Advance_InfoTitle");
        //FSkillAdvance nextadv = _skillitem.SkillAdvance.NextAdvanceData;
        //FSkillAdvance next2adv = _skillitem.SkillAdvance.GetAdvanceData(_skillitem.AdvanceStar + 2);
        //if (nextadv.Star != next2adv.Star)
        //{
        //    _additionalInfo_InfoText.text = LocalizeManager.GetText(nextadv.Adddescid, next2adv.Needskilllevel);
        //}
        //else
        //{
        //    FSkillTranscendence tran = _skillitem.SkillTranscendence.GetTranscData(1);
        //    _additionalInfo_InfoText.text = LocalizeManager.GetText(nextadv.Adddescid, tran.Needskilllevel);
        //}
    }

    void SetAdditionalInfo_Transcendence()
    {
        //_upgradeButtonText.text = LocalizeManager.GetText("UI_Btn_Transcendence");
        //_additionalInfo_TitleText.text = LocalizeManager.GetText("UI_Skill_Transce_InfoTitle");
        //FSkillTranscendence next2Tran = _skillitem.SkillTranscendence.GetTranscData(_skillitem.TranscendenceStar + 2);
        //FSkillLinkTranscendence nextLinkSkill = _skillitem.SkillTranscendence.GetLinkTranscData(_skillitem.TranscendenceStar + 1);
        //_additionalInfo_InfoText.text = _parent.GetTranscDescText(next2Tran, nextLinkSkill);
    }
}