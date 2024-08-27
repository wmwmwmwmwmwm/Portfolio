using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_BoxInfoEntry : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Image _selectedImage, _selectedImage2;
    [SerializeField] ItemIcon _itemIcon;
    [SerializeField] TMP_Text _itemDesc;

    BoxItem.ItemNode _itemNode;
    int _index;

    public int Index => _index;

    public void Setting() => Setting(_itemNode, _index);
    public void Setting(BoxItem.ItemNode item, int index)
    {
        _itemNode = item;
        _index = index;
        _button.SetID($"{UI_Inventory.EButtonId.Box_Entry}__{_index}");
        _itemIcon.Setting(_itemNode.ItemID, (BigInteger)(int)_itemNode.MaxCount);
        _itemDesc.text = item.Name;
    }

    public void Select(bool selected)
    {
        _selectedImage.gameObject.SetActive(selected);
        _selectedImage2.gameObject.SetActive(selected);
    }
}
