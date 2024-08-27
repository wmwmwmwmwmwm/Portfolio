using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;
using CodeStage.AntiCheat.ObscuredTypes;

public class UI_Equipment_Element : UICell
{
    [SerializeField] Image _bg;
    [SerializeField] Image _itemIcon;
    [SerializeField] Image[] _stars;
    [SerializeField] TextMeshProUGUI _lvText;
    [SerializeField] TMP_Text _reforgeCountText;
    [SerializeField] GameObject _equipped;
    [SerializeField] RedDotComponent _redDot;
    [SerializeField] GameObject _new, _lock;
    [SerializeField] Image _selected;
    [SerializeField] Slider _countSlider;
    [SerializeField] TMP_Text _countText;

    UI_Equipment _ui_equipment;
    EquipItem _equipItem;
    //Transform _currentIcon;

    //int _starCount;
    //public int StarCount
    //{
    //    get => _starCount;
    //    set
    //    {
    //        _starCount = value;
    //        for (int i = 0; i < _stars.Length; i++)
    //            _stars[i].gameObject.SetActive(i < _starCount);
    //    }
    //}

    //int _lv;
    //public int Lv
    //{
    //    get => _lv;
    //    set
    //    {
    //        _lv = value;
    //        _lvText.text = $"Lv.{_lv}";
    //    }
    //}

    //bool _isActive;
    //public bool IsActive
    //{
    //    get => _isActive;
    //    set
    //    {
    //        _isActive = value;
    //        _canvasGroups.ForEach(x => x.alpha = _isActive ? 1f : 0.66f);
    //        _reforgeCountText.gameObject.SetActive(_isActive);
    //        _lvText.gameObject.SetActive(_isActive);
    //        _itemSlotOn.SetActive(_isActive);
    //        _itemSlotOff.SetActive(!IsActive);
    //        _currentIcon = _isActive ? _itemSlotOn.transform : _itemSlotOff.transform;
    //    }
    //}

    //BigInteger _count;
    //public BigInteger Count
    //{
    //    get => _count;
    //    set
    //    {
    //        _count = value;
    //        ObscuredInt compoundMax = DataManager.Instance.GetSettingContents().Equip_Compound_Need_Count;
    //        _countText.text = $"{_count}/{compoundMax}";
    //        _countSlider.value = (float)_count / compoundMax;
    //    }
    //}

    //int _reforgeCount;
    //public int ReforgeCount
    //{
    //    get => _reforgeCount;
    //    set
    //    {
    //        _reforgeCount = value;
    //        _reforgeCountText.text = $"+ {_reforgeCount}";
    //    }
    //}

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;
        _ui_equipment.SelectEquipment(_equipItem);
    }

    public void Setting(UI_Equipment equipmentUI, EquipItem equip, bool equipped, bool selected)
    {
        _ui_equipment = equipmentUI;
        _equipItem = equip;

        if (null == equip) return;
        //IsActive = equip.Have;
        //Sprite gradeSprite = InGameUtil.GetGradeBg(equip.itemData.Grade);
        //Image bgFront = _currentIcon.Find("Item/BgFront").GetComponent<Image>();
        //bgFront.sprite = gradeSprite;
        //bgFront.gameObject.SetActive(gradeSprite != null);
        _bg.sprite = InGameUtil.GetGradeItemBg(equip.itemData.Grade);
        //Image iconOn = _itemSlotOn.transform.Find("Item/ItemIcon").GetComponent<Image>();
        //Image iconOff = _itemSlotOff.transform.Find("Item/ItemIcon").GetComponent<Image>();
        //_ui_equipment.SetEquipmentIcon(iconOn, iconOff, equip);
        _itemIcon.sprite = AtlasManager.GetItemIcon(equip, EIconType.Icon);
        //Lv = equip.Enhance;
        _lvText.text = $"Lv.{equip.Enhance}";
        //StarCount = equip.EquipData.Star;
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].gameObject.SetActive(i < equip.EquipData.Star);
        }
        //ReforgeCount = equip.ReforgingInfo.Level;
        _reforgeCountText.text = $"+{equip.ReforgingInfo.Level}";
        //Count = equip.Count;
        ObscuredInt compoundMax = DataManager.Instance.GetSettingContents().Equip_Compound_Need_Count;
        _countText.text = $"{equip.Count}/{compoundMax}";
        _countSlider.value = (float)equip.Count / compoundMax;
        //_gradeImage.sprite = InGameUtil.GetGradeTextSprite(equip.itemData.Grade);
        //_gradeImage.SetNativeSize();
        _equipped.SetActive(equipped);
        _selected.gameObject.SetActive(selected);
        _new.SetActive(equip.New);
        _redDot.SetActiveRedDot(equip.CheckTotalUpgrade());
        _lock.SetActive(!equip.Have);

        ///Tutorial를 위해서라도 버튼 셋팅 필요
        var btn = GetComponentInChildren<UI_Button>();
        btn.gameObject.name = "EquipItem_" + equip.Id;
        btn.SetID(equip.Id.ToString());
    }

    //public void Setting_GetItemPopup(UI_Equipment equipmentUI, EquipItem equip)
    //{
    //    Setting(equipmentUI, equip, false, false);
    //    _countParent.SetActive(false);
    //    _lvText.gameObject.SetActive(false);
    //    _reforgeCountText.gameObject.SetActive(false);
    //}
}
