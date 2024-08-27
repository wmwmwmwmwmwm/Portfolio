using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UI_SkillMain
{
    public void OnClose() { }

    public int GetTabCount()
    {
        return DataManager.Instance.DicSkillMastery.Keys.Count;
    }

    public BaseMasteryInfoItem GetItem(int tabIndex)
    {
        return InventoryManager.Instance.DicSkillMasterInfoItems[(EUseWeapon)(tabIndex + 1)];
    }

    public bool GetTabRedDotActive(int tabIndex)
    {
        EUseWeapon weapon = (EUseWeapon)(tabIndex + 1);
        SkillMasterInfoItem infos = InventoryManager.Instance.DicSkillMasterInfoItems[weapon];
        (List<RewardItemInfo> reward, _) = infos.GetRewardInfos();
        return reward.Count > 0;
    }

    public string GetTitleTextKey()
    {
        return "UI_SKill_Proficiency_Title";
    }

    public string GetTabName(int tabIndex)
    {
        EUseWeapon weapon = (EUseWeapon)(tabIndex + 1);
        return LocalizeManager.GetText(weapon switch
        {
            EUseWeapon.Handgun => "UI_Skill_Proficiency_Tap_Handgun",
            EUseWeapon.Sword => "UI_Skill_Proficiency_Tap_Sword",
            _ => "UI_Skill_Proficiency_Tap_Hammer",
        });
    }

    public string GetElementTitleTextKey()
    {
        return "UI_Skill_Proficiency_Lv";
    }

    public string GetElementDescTextKey()
    {
        return "UI_Skill_Proficiency_Raward";
    }

    public string GetLevelTextKey()
    {
        return "UI_Skill_Proficiency_Bonus_01";
    }

    public string GetStatText(int tabIndex, string statValue)
    {
        return LocalizeManager.GetText("UI_Skill_Proficiency_Bonus_02", "", statValue);
    }
}
