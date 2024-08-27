using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIEffects;
using static UI_Achievement;

public class UI_AchievementElement : MonoBehaviour
{
    [SerializeField] TMP_Text _titleText, _descText, _gaugeText;
    [SerializeField] Slider _fillGauge;
    [SerializeField] List<ItemIcon> _rewardIcons;
    [SerializeField] UI_Button _button;
    [SerializeField] TMP_Text _buttonText;
    //[SerializeField] GameObject _getRewardButton, _notComplete, _complete;
    //[SerializeField] GameObject _completeBg;

    AchievementInfo _data;
    EAchievementState _state;

    public AchievementInfo Data => _data;
    public EAchievementState State => _state;

    public void Setting(AchievementInfo data)
    {
        _data = data;
        if (_data == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        _button.SetID($"{EButtonId.GetReward}_{data.Id}");
        _titleText.text = LocalizeManager.GetText(data.Data.Title);
        _descText.text = LocalizeManager.GetText(data.Data.Desc);
        List<RewardItemInfo> rewardInfos = data.GetRewardInfos();
        for (int i = 0; i < _rewardIcons.Count; i++) 
        {
            ItemIcon icon = _rewardIcons[i];
            bool active = i < rewardInfos.Count;
            icon.gameObject.SetActive(active);
            if (active)
            {
                icon.Setting(rewardInfos[i].ItemID, 0);
                icon.Text.text = $"X{rewardInfos[i].ItemCount:N0}";
            }
        }
    }

    public void UpdateState()
    {
        if (_data == null) return;
        var gaugeInfo = _data.GetNextGaugeValue();
        BigInteger current = (EResetTimeType)_data.Data.Resettimetype switch
        {
            EResetTimeType.NONE => _data.Count,
            _ => _data.Count > gaugeInfo.nxtLvExp ? gaugeInfo.nxtLvExp : _data.Count
        };
        _fillGauge.value = gaugeInfo.percent;
        _state = _data.GetReceiveRewardInfosNow() != null ? EAchievementState.CanGetReward 
            : _data.IsMaxLevel ? EAchievementState.Complete : EAchievementState.NotComplete;

        // 특정 스테이지 클리어, 접속유지시간 예외처리
        switch ((EMissionType)_data.Data.Mtype)
        {
            case EMissionType.FixStageClear:
            case EMissionType.CurrentTime:
                current = _state != EAchievementState.NotComplete ? 1 : 0;
                gaugeInfo.nxtLvExp = 1;
                break;
        }

        _gaugeText.text = $"{current}/{gaugeInfo.nxtLvExp}";
        _button.interactable = _state == EAchievementState.CanGetReward;
        _buttonText.text = LocalizeManager.GetText(_state switch
        {
            EAchievementState.CanGetReward => "UI_Btn_Rewarded",
            EAchievementState.NotComplete => "UI_Btn_Ing",
            _ => "UI_Btn_Complete",
        });
        _button.GetComponent<UIShiny>().enabled = _state == EAchievementState.CanGetReward;
        
        //_notComplete.SetActive(_state == EAchievementState.NotComplete);
        //_complete.SetActive(_state == EAchievementState.Complete);
        //_completeBg.SetActive(_state == EAchievementState.Complete);
    }
}