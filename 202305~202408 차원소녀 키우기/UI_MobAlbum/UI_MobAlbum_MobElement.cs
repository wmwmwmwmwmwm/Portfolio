using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MobAlbum_MobElement : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Image _portrait;
    [SerializeField] Image _selected;
    [SerializeField] GameObject _level;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] RedDotComponent _redDot;

    MobAlbumInfo _info;
    int _index;

    public MobAlbumInfo Album => _info;
    public FMobAlbum AlbumData => _info.MobAlbum;
    public List<FMobAlbumSetting> AlbumSettingData => _info.MobAlbumSetting;
    public FCharacter MonsterData => DataManager.Instance.GetCharacterData(AlbumData.Id);
    public string Name => LocalizeManager.GetText(AlbumData.Name);

    public void Setting() => Setting(_info, _index);
    public void Setting(MobAlbumInfo info, int index)
    {
        _info = info;
        _index = index;
        _button.SetID($"{UI_MobAlbum.EButtonId.Mob}_{_index}");
        _portrait.sprite = AtlasManager.GetItemIcon(DataManager.Instance.GetCharacterData(info.MobAlbum.Id), EIconType.Icon);
        _level.SetActive(_info.Level != 0);
        _levelText.text = $"Lv. {_info.Level}";
        Refresh();
    }

    public void Refresh()
    {
        _redDot?.Obj()?.SetActiveRedDot(_info.CanLevelUp());
    }

    public void Select(bool selected)
    {
        _selected.gameObject.SetActive(selected);
    }
}
