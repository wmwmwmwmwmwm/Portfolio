using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_CompassTraining : UIMenuBarBase
{
    public enum EButtonId
    {
        None,
        GetReward,
        Enter,
    }

    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _bestScore;
    [SerializeField] TMP_Text _nextScore;
    [SerializeField] ItemIcon _rewardIcon;
    [SerializeField] UI_Button _getRewardButton;

    DPSDungeonInfo _stageInfo;

    CompassChargeItemInfo Manager => InventoryManager.Instance.CompassChargeItemInfo;

    public override void InitUI()
    {
        _stageInfo = StageInfoManager.Instance.GetDPSDungeon();
        Refresh();
        base.InitUI();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        string[] tokens = buttonInfo.ID.Split("__");
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.GetReward:
                GetRewardButton();
                break;
            case EButtonId.Enter:
                BattleSceneManager.Instance.SetBattleSceneLoad(_stageInfo.GameMode, _stageInfo.FirstStage.ID, ESCENEMOVITYPE.IgnoreFullScreen, null);
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    public override bool OnBackKey()
    {
        return base.OnBackKey();
    }

    protected override void RefreshUI(LanguageType langCode) { }

    void Refresh()
    {
        _titleText.text = LocalizeManager.GetText("UI_CompassTraining_Step", _stageInfo.MaxStage.Level);
        _bestScore.text = Formula.NumberToStringBy3(_stageInfo.MaxValue);
        BigInteger nextScore = _stageInfo.DamageTable[_stageInfo.MaxLevelIndex];
        _nextScore.text = Formula.NumberToStringBy3(nextScore);
        _rewardIcon.Setting(_stageInfo.MaxStage.FirstClearReward);
        bool anyReward = Manager.HaveAnyReward();
        _getRewardButton.interactable = anyReward;
        _getRewardButton.GetComponentInChildren<RedDotComponent>().SetActiveRedDot(anyReward);
    }

    void GetRewardButton()
    {
        List<RewardItemInfo> rewards = new();
        for (int index = Manager.TrainingDungeonLevelReceived; index < _stageInfo.MaxLevelIndex; index++) 
        {
            int level = index + 1;
            StageInfo stage = _stageInfo.GetStage(level);
            rewards.Add(stage.FirstClearReward);
        }
        rewards = RewardItemInfo.OrganizingDuplicateRewardItems(null, rewards);
        RewardItemInfo.ReceiveRewardItems(rewards);
        UIManager.Instance.ShowGetItemPopup(new()
        {
            rewardInfos = rewards,
        });
        Manager.TrainingDungeonLevelReceived = _stageInfo.MaxLevelIndex;
        Refresh();
    }
}
