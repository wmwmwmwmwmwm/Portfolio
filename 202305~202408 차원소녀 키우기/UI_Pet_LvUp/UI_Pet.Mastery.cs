using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UI_Pet
{
    public void OnClose()
    {
        RefreshPreview();
    }

    public int GetTabCount()
    {
        return DataManager.Instance.DicPetMastery.Keys.Count;
    }

    public BaseMasteryInfoItem GetItem(int tabIndex)
    {
        return InventoryManager.Instance.DicPetMasterInfoItems[(EItemGrade)(tabIndex + 1)];
    }

    public bool GetTabRedDotActive(int tabIndex)
    {
        EItemGrade grade = (EItemGrade)(tabIndex + 1);
        PetMasterInfoItem infos = InventoryManager.Instance.DicPetMasterInfoItems[grade];
        (List<RewardItemInfo> reward, _) = infos.GetRewardInfos();
        return reward.Count > 0;
    }

    public string GetTitleTextKey()
    {
        return "UI_Pet_Proficiency_Title";
    }

    public string GetTabName(int tabIndex)
    {
        EItemGrade grade = (EItemGrade)(tabIndex + 1);
        return LocalizeManager.GetText($"UI_Pet_Mastery_Rank{(int)grade}");
    }

    public string GetElementTitleTextKey()
    {
        return "UI_Pet_Proficiency_Lv";
    }

    public string GetElementDescTextKey()
    {
        return "UI_Pet_Proficiency_Raward";
    }

    public string GetLevelTextKey()
    {
        return "UI_Pet_Proficiency_Bonus_01";
    }

    public string GetStatText(int tabIndex, string statValue)
    {
        string statText = LocalizeManager.GetText(EStat.CriticalDamage.ToString());
        return LocalizeManager.GetText("UI_Skill_Proficiency_Bonus_02", statText, statValue);
    }
}
