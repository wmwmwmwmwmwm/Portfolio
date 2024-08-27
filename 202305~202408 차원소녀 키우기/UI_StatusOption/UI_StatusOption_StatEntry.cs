using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatusOption_StatEntry : MonoBehaviour
{
    [SerializeField] Image _bgNotChecked, _bgChecked;
    [SerializeField] TMP_Text _statText;
    [SerializeField] Image _checkOn;

    [HideInInspector] public EStat Stat;
    [HideInInspector] public bool IsChecked;

    public void Setting(FCharacterOption data)
    {
        Stat = data.Stat;
        _statText.text = LocalizeManager.GetText(Stat.ToString());
        Select(false);
    }

    public void Select(bool check)
    {
        IsChecked = check;
        _bgNotChecked.gameObject.SetActive(!check);
        _bgChecked.gameObject.SetActive(check);
        _checkOn.gameObject.SetActive(check);
    }
}
