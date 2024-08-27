using Coffee.UIEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemMastery_Element : MonoBehaviour
{
    public UI_Button _getRewardButton;
    public Slider _upperGauge, _gauge;
    public Image _bgOff, _bgOn;
    public TMP_Text _textOff, _textOn;
    public TMP_Text _titleText, _descText;
    public ItemIcon _reward;
    public Image _check;
    public TMP_Text _getRewardButtonText;

    [HideInInspector] public BaseMasteryInfoItem _infos;
    [HideInInspector] public int _step;
}
