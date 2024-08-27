using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UI_Equipment
{
    public void OnClose()
    {
        RefreshPreview(_preview);
    }

    public int GetTabCount()
    {
        return DataManager.Instance.DicEquipMastery.Keys.Count;
    }

    public BaseMasteryInfoItem GetItem(int tabIndex)
    {
        return InventoryManager.Instance.DicEquipMasterInfoItems[(EEquipedSlot)(tabIndex + 1)];
    }

    public bool GetTabRedDotActive(int tabIndex)
    {
        EEquipedSlot slot = (EEquipedSlot)(tabIndex + 1);
        EquipMasterInfoItem infos = InventoryManager.Instance.DicEquipMasterInfoItems[slot];
        (List<RewardItemInfo> reward, _) = infos.GetRewardInfos();
        return reward.Count > 0;
    }

    public string GetTitleTextKey()
    {
        return "UI_Equip_Proficiency_Title";
    }

    public string GetTabName(int tabIndex)
    {
        EEquipedSlot slot = (EEquipedSlot)(tabIndex + 1);
        return LocalizeManager.GetText(slot switch
        {
            EEquipedSlot.Weapon1 => "UI_Btn_Skill_Category_01",
            EEquipedSlot.Weapon2 => "UI_Btn_Skill_Category_02",
            EEquipedSlot.Weapon3 => "UI_Btn_Skill_Category_03",
            EEquipedSlot.Accessory1 => "UI_Btn_Equip_Accl_Category_01",
            EEquipedSlot.Accessory2 => "UI_Btn_Equip_Accl_Category_02",
            _ => "UI_Btn_Equip_Accl_Category_03",
        });
    }

    public string GetElementTitleTextKey()
    {
        return "UI_Equip_Proficiency_Lv";
    }

    public string GetElementDescTextKey()
    {
        return "UI_Equip_Proficiency_Raward";
    }

    public string GetLevelTextKey()
    {
        return "UI_Equip_Proficiency_Bonus_01";
    }

    public string GetStatText(int tabIndex, string statValue)
    {
        EEquipedSlot slot = (EEquipedSlot)(tabIndex + 1);
        string textKey = slot switch
        {
            EEquipedSlot.Weapon1 => "UI_Equip_Proficiency_Bonus_02",
            EEquipedSlot.Weapon2 => "UI_Equip_Proficiency_Bonus_02",
            EEquipedSlot.Weapon3 => "UI_Equip_Proficiency_Bonus_02",
            EEquipedSlot.Accessory1 => "UI_Equip_Proficiency_Bonus_Acc_01",
            EEquipedSlot.Accessory2 => "UI_Equip_Proficiency_Bonus_Acc_02",
            _ => "UI_Equip_Proficiency_Bonus_Acc_03",
        };
        return LocalizeManager.GetText(textKey, GetTabName(tabIndex), statValue);
    }
}
