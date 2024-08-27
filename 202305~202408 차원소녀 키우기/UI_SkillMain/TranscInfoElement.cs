using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public class TranscInfoElement : MonoBehaviour
{
    [SerializeField] Image _bgInactive, _bgActive;
    [SerializeField] Image _mark;
    [SerializeField] TMP_Text _levelText, _descText;

    [Header("색상")]
    [SerializeField] Color _inactiveColor, _activeColor;

    public void Setting(bool transcActive, int star, string desc)
    {
        _bgInactive.gameObject.SetActive(!transcActive);
        _bgActive.gameObject.SetActive(transcActive);
        _levelText.text = star.ToString();
        _descText.text = desc;

        Color color = transcActive ? _activeColor : _inactiveColor;
        _mark.color = color;
        _levelText.color = color;
        _descText.color = color;
    }
}
