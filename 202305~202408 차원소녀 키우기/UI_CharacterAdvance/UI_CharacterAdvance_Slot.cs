using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterAdvance_Slot : MonoBehaviour
{
    //[SerializeField] Image _line;
    [SerializeField] UI_Button _button/*, _button2*/;
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] GameObject _cost;
    [SerializeField] Image _costIcon;
    [SerializeField] TMP_Text _costText, _maxText;
    [SerializeField] GameObject _effectFull;
    [SerializeField] float _projectileEffectYAmount;

    int _index;
    AdvanceStateInfoItem _info;

    public int Index => _index;
    public AdvanceStateInfoItem Info => _info;
    FCharacterAdvanceStat StatData => _info.CharacterAdvanceStatData;
    public Image Icon => _icon;
    public float ProjectileEffectYAmount => _projectileEffectYAmount * GetComponentInParent<Canvas>().transform.localScale.x;

    public void Setting(AdvanceStateInfoItem info, FCharacterAdvance previewAdvData, Sprite icon, int index, bool active, bool canLevelUp)
    {
        _index = index;
        _info = info;
        string buttonId = $"{UI_CharacterAdvance.EButtonId.LevelUp}_{_index}";
        _button.SetID(buttonId);
        _button.gameObject.SetActive(active);
        //_button2.SetID(buttonId);
        //_line.gameObject.SetActive(buttonActive);
        //_icon.sprite = InGameUtil.GetStatIcon(StatData.Characteradvancestat);
        _icon.sprite = icon;
        _title.text = LocalizeManager.GetText(StatData.Advancename);
        if (previewAdvData != null)
        {
            int nextLevel = previewAdvData.Conditionlevel[index + 1];
            int level = Mathf.Min(info.Level, nextLevel);
            _levelText.text = $"Lv. {level}/{nextLevel}";
        }
        else
        {
            _levelText.text = $"Lv. {info.Level}";
        }
        _cost.SetActive(active);
        _costText.gameObject.SetActive(!_info.IsMaxLevel);
        _maxText.gameObject.SetActive(_info.IsMaxLevel);
        BigInteger cost = _info.GetNextMaterialCost(1);
        if (!_info.IsMaxLevel)
        {
            _costIcon.sprite = AtlasManager.GetItemIcon(InventoryManager.Instance.GetItem(StatData.Statlevelupitem), EIconType.MiniIcon);
            _costText.text = Formula.NumberToStringBy3(cost);
        }
        else
        {
            _costText.text = LocalizeManager.GetText("MAX_Value");
        }
        bool isEnoughCost = InventoryManager.Instance.GetItem(StatData.Statlevelupitem).Count >= cost;
        UIRoot.Instance.SetButtonItemShortage(_button, _info.IsMaxLevel || !isEnoughCost, true);
        _button.interactable = !_info.IsMaxLevel;
        //_button2.interactable = buttonActive && !_info.IsMaxLevel;
        _effectFull.SetActive(canLevelUp);
    }
}
