using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_Achievement;

[Obsolete]
public class UI_Achievement_TabButton : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Transform _on, _off;
    [SerializeField] TMP_Text _onText, _offText;

    public void Setting(ETabId tabType)
    {
        _button.SetID(tabType.ToString());
        _button.InteractableChangeCallbacks.Add(SetOnOff);

        string textKey = tabType switch
        {
            ETabId.Daily => "UI_Achieve_Tap_01",
            ETabId.Weekly => "UI_Achieve_Tap_02",
            ETabId.Monthly => "UI_Achieve_Tap_03",
            _ => "UI_Achieve_Tap_04"
        };
        string text = LocalizeManager.GetText(textKey);
        _onText.text = text;
        _offText.text = text;
    }

    void SetOnOff(UIButtonInfo info, bool on)
    {
        _on.gameObject.SetActive(on);
        _off.gameObject.SetActive(!on);
    }
}