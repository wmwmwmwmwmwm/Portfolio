using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MobAlbum_CategoryElement : MonoBehaviour
{
    [SerializeField] MenuBar_Menu _menu;
    //[SerializeField] UI_Button _button;
    //[SerializeField] UIPlayTween _activeTween, _activeTween2;
    [SerializeField] Image _icon;
    //[SerializeField] Image _icon, _iconOn;
    //[SerializeField] GameObject _backInactive, _backActive;
    [SerializeField] RedDot_MobAlbmumTab _redDot;

    int _category;
    int _index;

    public int Category => _category;
    public MonsterAlbumTotalInfo TotalInfo => MonsterAlbumManager.Instance.TotalInfos[_category];
    public MenuBar_Menu Menu => _menu;

    public void Setting(int category, int index)
    {
        _category = category;
        _index = index;
        _menu.Button.SetID($"{UI_MobAlbum.EButtonId.Category}_{index}");
        //string iconName = DataManager.Instance.GetSettingContents().MobAlbum_Category_Icon[index].ToString();
        //_icon.sprite = AtlasManager.GetSprite(EATLAS_TYPE.Icon, iconName);
        //string iconOnName = $"{iconName}_on";
        //_iconOn.sprite = AtlasManager.GetSprite(EATLAS_TYPE.Icon, iconOnName);
        _redDot?.Obj()?.Setting(TotalInfo);
    }

    //public void Select(bool selected)
    //{
    //    _backInactive.SetActive(!selected);
    //    _backActive.SetActive(selected);
    //    if (selected)
    //    {
    //        _activeTween.Play();
    //        _activeTween2.Play();
    //    }
    //}

    public void Select(bool selected)
    {
        Menu.Button.IsCheck = selected;
        Menu.Animate();
        string iconName = DataManager.Instance.GetSettingContents().MobAlbum_Category_Icon[_index].ToString();
        string iconOnName = $"{iconName}_on";
        _icon.sprite = AtlasManager.GetSprite(EATLAS_TYPE.Icon, selected ? iconOnName : iconName);
    }
}
