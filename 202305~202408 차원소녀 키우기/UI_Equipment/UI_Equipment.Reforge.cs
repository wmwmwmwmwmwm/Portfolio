using System;
using System.Collections;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EquipItem;

public partial class UI_Equipment
{
    [Header("돌파 팝업")]
    [SerializeField] PreviewSpace_ _reforgePreview;
    [SerializeField] GameObject _reforgePopup;
    //[SerializeField] RawImage _reforge_PortraitModel;
    [SerializeField] Image _reforge_Portrait;
    [SerializeField] TMP_Text _reforge_EquipName;
    [SerializeField] TMP_Text _reforge_StatNameText, _reforge_StatValueText;
    [SerializeField] TMP_Text _reforge_LevelText, _reforge_StepText;
    [SerializeField] GameObject _reforge_UnlockText;
    [SerializeField] GameObject _reforge_Chance, _reforge_CannotText;
    [SerializeField] TMP_Text _reforge_ChanceText;
    [SerializeField] UI_Button _reforge_ReforgeButton;
    [SerializeField] GameObject _reforge_FailEffect;
    [SerializeField] TMP_Text _reforge_ResultStatName, _reforge_ResultStatValue;
    [SerializeField] TMP_Text _reforge_ResultLevelText;
    [SerializeField] UI_LevelUpResult _reforge_ResultPopup;

    StatValueInfo _reforge_NextStat;
    int _reforge_NextLevel;

    void ShowReforgePopup(bool show)
    {
        if (!show)
        {
            _reforgePopup.SetActive(false);
            return;
        }

        if (_mode == EMode.Equipment)
        {
            _reforgePreview.gameObject.SetActive(true);
            _reforge_Portrait.gameObject.SetActive(false);
            RefreshPreview(_reforgePreview);
        }
        else
        {
            _reforgePreview.gameObject.SetActive(false);
            _reforge_Portrait.gameObject.SetActive(true);
            _reforge_Portrait.sprite = AtlasManager.GetItemIcon(_selectedEquip, EIconType.Icon);
        }
        _reforge_EquipName.text = GetEquipName();
        _reforge_NextStat = GetNextReforgeStat();
        _reforge_StatNameText.text = _reforge_NextStat.StatString;
        _reforge_StatValueText.text = $"+{_reforge_NextStat.ValueString}";
        _reforge_NextLevel = GetExpandLevel();
        _reforge_LevelText.text = $"+{_reforge_NextLevel}Lv";
        _reforge_StepText.text = "+1";
        _reforge_UnlockText.SetActive(_selectedEquip.NextReforgingData.Openhavestat > 0);
        bool reforgeActive = _selectedEquip.GetEnhanceType() == EquipEnhanceType.Reforging;
        _reforge_Chance.SetActive(reforgeActive);
        _reforge_CannotText.SetActive(!reforgeActive);
        _reforge_ChanceText.text = Formula.NumberToStringBy3_Percent((long)_selectedEquip.NextReforgingData.Rate);
        RefreshReforgePopup();
        _reforgePopup.SetActive(true);
    }

    void RefreshReforgePopup()
    {
        SettingLevelUpButton(_reforge_ReforgeButton, _selectedEquip.GetReforgeCost(), _selectedEquip.CheckReforgeLevel());
    }

    void OnClickReforge()
    {
        if (_selectedEquip == null) return;
        if (!_selectedEquip.CheckEnhanceCost())
        {
            if (_showShortageToast)
            {
                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                _showShortageToast = false;
            }
            return;
        }

        EnhanceUpResultInfo result = _selectedEquip.EnhanceUp();
        if (result.IsReforgingSuccess)
        {
            RefreshStatValue();
            UIRoot.Instance.PlayLevelUpEffect(transform, null, null);
            //Play2DEffect(id: "Fx_UI_Upgrade_Equipment_Refine_01_Success",
            //    position: _effectPosition_Reforge.localPosition,
            //    parent: _effectPosition_Reforge.parent);
            ShowReforgePopup(false);
            _reforge_ResultPopup.Show(LocalizeManager.GetText("UI_Equip_Break_Success_Title"),
                _reforge_NextStat.StatString,
                $"+{_reforge_NextStat.ValueString}",
                LocalizeManager.GetText("UI_Equip_MaxLv"),
                $"+{_reforge_NextLevel}Lv");
        }
        else
        {
            //Play2DEffect(id: "Fx_UI_Upgrade_Equipment_Refine_01_Fail",
            //    position: _effectPosition_Reforge.localPosition,
            //    parent: _effectPosition_Reforge.parent);
            _reforge_FailEffect.transform.GetComponentInChildren<ParticleSystem>().Play();
        }

        OnRefreshUI();
    }

    StatValueInfo GetNextReforgeStat()
    {
        StatValueInfo stat = _selectedEquip.GetStat(EquipStatType.Possesion1);
        BigInteger currentValue = _selectedEquip.CurrentReforgingData != null ? _selectedEquip.CurrentReforgingData.P_inc : 0;
        StatValueInfo statValue = new()
        {
            Stat = stat.Stat,
            Value = (BigInteger)_selectedEquip.NextReforgingData.P_inc - currentValue,
            CalcType = stat.CalcType,
        };
        return statValue;
    }

    int GetExpandLevel()
    {
        if (_selectedEquip.Next2ReforgingData != null)
        {
            return _selectedEquip.Next2ReforgingData.Needlevel - _selectedEquip.NextReforgingData.Needlevel;
        }
        else
        {
            return 0;
        }
    }
}
