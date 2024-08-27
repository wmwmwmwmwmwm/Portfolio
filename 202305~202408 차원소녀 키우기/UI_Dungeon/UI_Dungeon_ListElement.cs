using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dungeon_ListElement : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Image _bg;
    [SerializeField] TMP_Text _titleText, _descText;
    [SerializeField] Image _costIcon;
    [SerializeField] TMP_Text _costCount;
    [SerializeField] Image _outlineNormal, _outlineSelected;
    [SerializeField] UIOpenButtonStat _uiOpenButtonStat;
    [SerializeField] RedDotComponent _redDot;

    GameModeInfo _gameModeInfo;
    //StageInfo _stageInfo;
    FStageConfig _config;

    public GameModeInfo ModeInfo => _gameModeInfo;
    public FStageConfig Config => _config;

    public void Setting(FStageConfig data)
    {
        _gameModeInfo = StageInfoManager.Instance.GetMode(data.Gamemode);

        _config = data;
        _button.SetID($"{UI_Dungeon.EButtonId.ListElement}__{_config.Id}");

        _bg.sprite = AtlasManager.GetSprite(EATLAS_TYPE.Icon, _config.Image);
        _titleText.text = LocalizeManager.GetText(_config.Nametextid);
        _descText.text = LocalizeManager.GetText(_config.Desctextid[0]);
        _costIcon.sprite = AtlasManager.GetItemIcon(_config.Ticketitemid, EIconType.MiniIcon);
        _costCount.text = _config.Ticketcount.ToString("N0");

        var eConent = EnumHelper.Parse(((EGameMode)data.Gamemode).ToString(), EContent.NONE);

        if (_uiOpenButtonStat != null)
        {
            if (eConent != EContent.NONE)
                _uiOpenButtonStat.Setting(new() { eConent });
        }
        _redDot?.Obj()?.SetAuto(new() { new RedDotComponent.RedDotInfo() { Content = eConent } });
    }

    public void OnSelect(bool selected)
    {
        _outlineNormal.gameObject.SetActive(!selected);
        _outlineSelected.gameObject.SetActive(selected);
    }
}
