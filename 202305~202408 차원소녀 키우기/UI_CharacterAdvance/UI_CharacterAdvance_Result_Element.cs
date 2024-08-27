using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterAdvance_Result_Element : MonoBehaviour
{
    [SerializeField] Image _statIcon;
    [SerializeField] TMP_Text _statName;
    [SerializeField] TMP_Text _valueText;

    public void Setting(Sprite statIcon, string statName, string statValue)
    {
        _statIcon.sprite = statIcon;
        _statName.text = statName;
        _valueText.text = statValue;
    }
}
