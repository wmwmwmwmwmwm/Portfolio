using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tower_BgScroll : MonoBehaviour, IGridViewAdapter
{
    [SerializeField] RectTransform _element;

    public RectTransform _gridPrefab => _element;

    public (ScrollItemInfo itemInfo, RectTransform prefab) CreateScrollItem(int index) => (null, _element);

    public ScrollItemInfo UpdateScrollItem(int idx, ScrollItemInfo itemInfo, RectTransform prefab)
    {
        return itemInfo;
    }

    public void UpdateComplete(int newCount, int prevCount) { }
}
