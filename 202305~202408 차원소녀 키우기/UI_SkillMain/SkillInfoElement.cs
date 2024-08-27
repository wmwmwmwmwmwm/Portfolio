using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
class SkillInfoElement : MonoBehaviour
{
    [SerializeField] Image _gradeBg;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] GameObject _lockIcon;
    [SerializeField] SkillIconElement _skillIcon;
    [SerializeField] Image _gaugeMask;
    [SerializeField] TMP_Text _itemCountText;
    [SerializeField] GameObject _selectCurserImage;

    [SerializeField] Sprite _gradeBg0, _gradeBg1, _gradeBg2, _gradeBg3;
    [SerializeField] RedDotComponent _redDot;

    SkillItem _skillItem;
    UI_Button button;
    float _maxGaugeValue = -1;

    public SkillItem Skill => _skillItem;

    public void Setting(SkillItem skillItem)
    {
        _skillItem = skillItem;
        _skillIcon.Setting(_skillItem);
        button = GetComponent<UI_Button>();
        if (_maxGaugeValue < 0)
            _maxGaugeValue = _gaugeMask.fillAmount;

        button.SetID($"{UI_MainSkillUI.EButtonId.SkillInfoElement}_{_skillItem.Id}");
        Refresh();
    }

    public void Refresh()
    {
        (int skillCount, int itemId, int itemCount) = _skillItem.GetSpendCost();
        _gradeBg.sprite = (EItemGrade)_skillItem.itemData.Grade switch
        {
            EItemGrade.E or
            EItemGrade.D or
            EItemGrade.C => _gradeBg0,
            EItemGrade.B or
            EItemGrade.A => _gradeBg1,
            EItemGrade.S or
            EItemGrade.SR => _gradeBg2,
            EItemGrade.SSR or
            EItemGrade.UR => _gradeBg3,
            _ => _gradeBg0
        };
        _levelText.text = LocalizeManager.GetText("LEVEL_MARK", _skillItem.Level);
        _itemCountText.text = $"{_skillItem.Count}/{skillCount}";
        _gaugeMask.fillAmount = (float)((double)_skillItem.Count * _maxGaugeValue / skillCount);
        _redDot.SetActiveRedDot(_skillItem.New | _skillItem.CheckLevelUp(out _));
        _skillIcon.Refresh();
        _lockIcon.SetActive(!_skillItem.Have);
    }

    public void SetSelect(bool isSel)
    {
        _selectCurserImage.SetActive(isSel);
    }
}
