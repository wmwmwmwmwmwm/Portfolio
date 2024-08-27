using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillMain_Slot : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Image /*_empty,*/ _icon;
    [SerializeField] GameObject _selected, _unequipMark, _arrow;
    [SerializeField] UIOpenButtonStat _openButtonStat;

    SkillItem _skill;
    int _index;

    public SkillItem Skill => _skill;
    public int Index => _index;
    public UIOpenButtonStat OpenButtonStat => _openButtonStat;

    public void Setting(SkillItem skill, int index, bool selected, bool isEquipMode, bool isDrag)
    {
        _skill = skill;
        _index = index;
        _button.SetID($"{UI_SkillMain.EButtonId.Slot}__{_index}");
        bool active = _skill != null;
        //_empty.gameObject.SetActive(!active);
        _icon.gameObject.SetActive(active);
        if (active) 
        {
            _icon.sprite = AtlasManager.GetItemIcon(_skill, EIconType.Icon);
        }
        _selected.SetActive(selected);
        _unequipMark.SetActive(isEquipMode && selected && !isDrag);
        _arrow.SetActive(isEquipMode && !selected);
    }
}
