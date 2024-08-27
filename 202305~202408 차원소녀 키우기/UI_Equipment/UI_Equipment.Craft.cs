using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_Equipment
{
    [Header("제작 패널")]
    [SerializeField] Image _craft_WeaponIcon;
    [SerializeField] TMP_Text _craft_RankText, _craft_DescText;
    [SerializeField] TMP_Text _craft_CountText;
    [SerializeField] List<UI_Equipment_CraftMaterial> _craft_Materials;
    [SerializeField] List<Sprite> _craft_IconSprites;
    [SerializeField] TMP_Text _craft_CraftButtonText;

    int _craftCount;
    FCraft _craftData => DataManager.Instance.DicEquipCraft[_selectedEquip.Id];

    void RefreshCraftPanel()
    {
        _craft_WeaponIcon.sprite = _craft_IconSprites[(int)_selectedEquip.Slot];
        string grade = "";
        switch (_selectedEquip.GetCompoundType())
        {
            case EEquipCompoundType.Craft:
                EItemGrade nextGrade = (EItemGrade)(_selectedEquip.itemData.Grade + 1);
                grade = nextGrade.ToString();
                break;
            case EEquipCompoundType.Conversion:
                grade = _selectedEquip.itemData.Grade.ToString();
                break;
        }
        _craft_RankText.text = grade;
        string weaponKindNameKey = (EEquipedSlot)_selectedEquip.Slot switch
        {
            EEquipedSlot.Weapon1 => "UI_Btn_Skill_Category_01",
            EEquipedSlot.Weapon2 => "UI_Btn_Skill_Category_02",
            EEquipedSlot.Weapon3 => "UI_Btn_Skill_Category_03",
            EEquipedSlot.Accessory1 => "UI_Btn_Skill_Category_04",
            EEquipedSlot.Accessory2 => "UI_Btn_Skill_Category_05",
            EEquipedSlot.Accessory3 => "UI_Btn_Skill_Category_06",
            _ => throw new NotImplementedException()
        };
        string weaponKindName = LocalizeManager.GetText(weaponKindNameKey);
        switch (_selectedEquip.GetCompoundType())
        {
            case EEquipCompoundType.Craft:
                _craft_DescText.text = LocalizeManager.GetText("UI_Craft_Desc_01", grade, weaponKindName);
                _craft_CraftButtonText.text = LocalizeManager.GetText("UI_Btn_Craft");
                break;
            case EEquipCompoundType.Conversion:
                _craft_DescText.text = LocalizeManager.GetText("UI_Craft_Desc_02", grade, weaponKindName, weaponKindName);
                _craft_CraftButtonText.text = LocalizeManager.GetText("UI_Btn_Conversion");
                break;
        }
        UpdateCraftPanel();
    }

    void UpdateCraftPanel()
    {
        // 재료 아이템
        for (int i = 0; i < _craftData.Itemid.Count; i++)
        {
            UI_Equipment_CraftMaterial element = _craft_Materials[i];
            switch (_selectedEquip.GetCompoundType())
            {
                case EEquipCompoundType.Craft:
                    int itemId = _craftData.Itemid[i];
                    if (itemId > 0)
                    {
                        element.gameObject.SetActive(true);
                        BigInteger needCount = (BigInteger)(int)_craftData.Itemcount[i];
                        element.Setting(itemId, needCount);
                    }
                    else
                    {
                        element.gameObject.SetActive(false);
                    }
                    break;
                case EEquipCompoundType.Conversion:
                    if (i == 0)
                    {
                        element.gameObject.SetActive(true);
                        element.Setting(_selectedEquip.Id, 1);
                    }
                    else
                    {
                        element.gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }

    void SetCraftCount(int count)
    {
        _craftCount = Mathf.Clamp(count, 1, _selectedEquip.GetMaxCraftCount());
        _craft_CountText.text = _craftCount.ToString();
        UpdateCraftPanel();
    }

    void OnClickCraft()
    {
        // 재료 부족시
        switch (_selectedEquip.GetCompoundType())
        {
            case EEquipCompoundType.Craft:
                if (!_selectedEquip.CanCraft(_craftCount))
                {
                    UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                    return;
                }
                break;
            case EEquipCompoundType.Conversion:
                if (!_selectedEquip.CanConversion(_craftCount))
                {
                    UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                    return;
                }
                break;
        }

        // 제작, 변환
        List<ItemCostInfo> costs = null;
        switch (_selectedEquip.GetCompoundType())
        {
            case EEquipCompoundType.Craft:
                (BaseItem item, int count) = _selectedEquip.Craft(_craftCount, out costs);
                StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
                {
                    TitleText = LocalizeManager.GetText("UI_Craft_Result_01"),
                    ItemIds = new() { item.Id },
                    ItemCounts = new() { count },
                }));
                break;
            case EEquipCompoundType.Conversion:
                List<(BaseItem item, int count)> results = _selectedEquip.Conversion(_craftCount, out costs);
                StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
                {
                    TitleText = LocalizeManager.GetText("UI_Craft_Result_02"),
                    ItemIds = results.Select(x => x.item.Id).ToList(),
                    ItemCounts = results.Select(x => (BigInteger)x.count).ToList(),
                }));
                break;
        }
        SetCraftCount(1);
    }
}
