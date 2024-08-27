using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_ItemElement : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Image _selected;
    [SerializeField] ItemIcon _itemIcon;
    [SerializeField] Image _cooltime;
    [SerializeField] TMP_Text _cooltimeText;
    [SerializeField] RedDotComponent _redDot;
    int _itemId;

    public InventoryManager Inventory => InventoryManager.Instance;
    public int ItemId => _itemId;
    public BaseItem Item => InventoryManager.Instance.GetItem(_itemId);

    public void SetItemId(int itemId)
    {
        _itemId = itemId;
    }

    public void Setting()
    {
        _button?.SetID($"{UI_Inventory.EButtonId.Item}__{_itemId}");
        BigInteger count = _itemId > 0 ? Item.Count : 0;
        _itemIcon.Setting(_itemId, count);
        _cooltime.gameObject.SetActive(false);
    }

    public void Select(bool selected)
    {
        _selected.gameObject.SetActive(selected);
        if (selected) Item.New = false;
    }

    void Update()
    {
        if (Item == null) return;
        switch (Item.EItemType)
        {
            case EItemType.Use_Potion:
                float timePassed = Time.time - Inventory.LastPotionUseTime;
                float cooltime = (float)DataManager.Instance.GetSettingContents().Use_Potion_GlovalCoolTime / 10000f;
                _cooltime.gameObject.SetActive(timePassed < cooltime);
                _cooltime.fillAmount = 1f - (timePassed / cooltime);
                _cooltimeText.text = LocalizeManager.GetText("UI_Cooltime_Second", (cooltime - timePassed).ToString("0.0"));
                break;
            case EItemType.Use_Buff:
                (bool isCooltime, float pastTime, float cooltime1) = Inventory.GetBuffCooltime(Item);
                _cooltime.gameObject.SetActive(isCooltime);
                if (isCooltime)
                {
                    _cooltime.fillAmount = 1f - (pastTime / cooltime1);
                    _cooltimeText.text = LocalizeManager.GetText("UI_Cooltime_Second", (cooltime1 - pastTime + 0.9f).ToString("0"));
                }
                break;
        }
        _redDot?.Obj()?.SetActiveRedDot(Item.New);
    }
}
