using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;
using System.Collections;

public class UI_Attendance_ItemElement : MonoBehaviour
{
    [SerializeField] Animation _anim;
    [SerializeField] ItemIcon _itemIcon;
    [SerializeField] TMP_Text _label;
    [SerializeField] Image _gray, _checkmark, _current;

    FAttendance_Item _data;
    bool _itemSecond;
    bool _isCurrent;

    public Animation Anim => _anim;
    public bool IsCurrent => _isCurrent;

    public void Setting(FAttendance_Item data, bool itemSecond, bool already, bool current)
    {
        _data = data;
        _itemSecond = itemSecond;
        _isCurrent = current;
        int itemId = _itemSecond ? data.Item1id : data.Item0id;
        int itemCount = _itemSecond ? data.Item1count : data.Item0count;
        _itemIcon.Setting(itemId, itemCount);
        _label.text = LocalizeManager.GetText("UI_Attendance_Order", _data.Value);
        bool received = already || current;
        _gray.gameObject.SetActive(received);
        _checkmark.gameObject.SetActive(received);
        _current.gameObject.SetActive(current);
    }
}
