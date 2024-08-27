using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public class OrderSkillIconElement : MonoBehaviour
{
    [SerializeField] SkillIconElement _skillIconElement;
    [SerializeField] UI_Button _button;
    [SerializeField] TMP_Text _numberText;
    [SerializeField] Image _selectImage;
    [SerializeField] GameObject _arrowObject;

    [SerializeField, ReadOnly] SkillItem _skillItem;

    [SerializeField, ReadOnly] int _orderIndex;



    public void Setting(int order, SkillItem skillItem, bool isSelect = false)
    {
        _orderIndex = order;

        _skillItem = skillItem;
        _skillIconElement.Setting(_skillItem);
        //_skillIconElement.SetButtonActive(false);
        _button.SetID($"{this.name}");
        _numberText.text = (order + 1).ToString();
        Select(_skillItem != null && isSelect);
        ArrowActive(false);
    }

    public void Select(bool isSelect)
    {
        _selectImage.gameObject.SetActive(isSelect);
    }

    public void ArrowActive(bool isActive)
    {
        if (_skillItem != null)
            _arrowObject.SetActive(isActive);
    }

    public int GetIndex() => _orderIndex;

    public SkillItem GetSkillItemInfo() => _skillItem;

    public UI_Button GetButton() => _button;
}