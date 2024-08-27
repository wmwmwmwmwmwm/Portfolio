using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;

public class UI_Equipment_CraftMaterial : MonoBehaviour
{
    [SerializeField] ItemIcon _itemIcon;
    [SerializeField] TMP_Text _countText;

    BaseItem _item;

    public void Setting(int itemId, BigInteger needCount)
    {
        _itemIcon.Setting(itemId, 0);
        _item = InventoryManager.Instance.GetItem(itemId);
        string currentCountStr = Formula.NumberToStringBy3(_item.Count);
        string needCountStr = Formula.NumberToStringBy3(needCount);
        _countText.text = $"{currentCountStr}/{needCountStr}";
    }
}
