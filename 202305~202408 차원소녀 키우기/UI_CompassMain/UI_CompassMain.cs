using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CompassMain : UIMenuBarBase
{
    [Header("나침반 레벨 업")]
    [SerializeField] TMP_Text _levelText;
    [SerializeField] Slider _expGauge;
    [SerializeField] TMP_Text _expText;
    [SerializeField] UI_Button _levelUpButton;
    [SerializeField] Transform _levelUpEffectPosition;

    [Header("오른쪽")]
    [SerializeField] TMP_Text _timeText;
    [SerializeField] Slider _timeGauge;
    [SerializeField] TMP_Text _timePercentText;
    [SerializeField] TMP_Text _maxStageText;
    [SerializeField] ItemIcon _itemIconPrefab;
    [SerializeField] Transform _estimateRewardsParent;
    [SerializeField] Transform _rewardsParent;
    [SerializeField] TMP_Text _noRewardText;
    [SerializeField] UI_Button _getRewardButton;
    [SerializeField] RedDotComponent _quickButtonDot, _getRewardButtonDot;

    [Header("즉시 충전")]
    [SerializeField] GameObject _quickPopup;
    [SerializeField] TMP_Text _quick_TimeText;
    [SerializeField] Transform _quick_RewardsParent;
    [SerializeField] TMP_Text _quick_CountText;
    [SerializeField] UI_Button _quick_FreeGetRewardButton;
    [SerializeField] UI_Button _quick_GetRewardButton;
    [SerializeField] Image _quick_GetRewardButton_Icon;
    [SerializeField] TMP_Text _quick_GetRewardButton_Text;
    [SerializeField] UI_Button _quick_NotAvailableButton;

    List<ItemIcon> _estimateRewardIcons;
    List<ItemIcon> _rewardIcons;
    List<ItemIcon> _quick_RewardIcons;

    CompassChargeItemInfo Manager => InventoryManager.Instance.CompassChargeItemInfo;
    StageInfo MaxStage => StageInfoManager.Instance.GetNormalStage().MaxStage;
    FSettingContents SettingContents => DataManager.Instance.GetSettingContents();
    int QuickReceiveBoxCount => (int)SettingContents.Compass_SubReward_Time * 60;

    public enum EButtonId
    {
        None,
        LevelUp,
        OpenQuick,
        GetReward,
        Quick_Close,
        Quick_GetReward,
    }

    public override void InitUI()
    {
        _estimateRewardIcons = new();
        _rewardIcons = new();
        _quick_RewardIcons = new();
        UIManager.Instance.RegisterEvent_StageClear(gameObject, RefreshRight);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Currency, gameObject, RefreshBottom);
        Timer.ADD_MinChangeCallbacks(RefreshRight);

        RefreshAll();
        ShowQuickPopup(false);
        base.InitUI();
    }

    public override void OutOnlyMyself(bool isDirect = false)
    {
        base.OutOnlyMyself(isDirect);
        Timer.Remove_MinChangeCallbacks(RefreshRight);
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        string[] tokens = buttonInfo.ID.Split("__");
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.LevelUp:
                LevelUpButton();
                break;
            case EButtonId.OpenQuick:
                ShowQuickPopup(true);
                break;
            case EButtonId.GetReward:
                GetRewardButton();
                break;
            case EButtonId.Quick_Close:
                ShowQuickPopup(false);
                break;
            case EButtonId.Quick_GetReward:
                Quick_GetRewardButton();
                break;
        }
    }

    public override void OnUpdate()
    {
        // 충전 시간
        TimeSpan span = TimeSpan.FromSeconds(Manager.ChargeTime);
        _timeText.text = GetTimeText(span);
        TimeSpan maxSpan = TimeSpan.FromDays(1);
        double percent = span / maxSpan;
        _timeGauge.value = (float)percent;
        _timePercentText.text = $"{percent * 100:0.00}%";
        _quickButtonDot.SetActiveRedDot(InventoryManager.Instance.CheckRedDot_CompassMain_ChargeCount());
        _getRewardButtonDot.SetActiveRedDot(InventoryManager.Instance.CheckRedDot_CompassMain_IsFull());
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    public override bool OnBackKey()
    {
        if (_quickPopup.activeSelf)
        {
            ShowQuickPopup(false);
            return false;
        }
        return base.OnBackKey();
    }

    protected override void RefreshUI(LanguageType langCode) { }

    void RefreshAll()
    {
        RefreshBottom();
        RefreshRight();
    }

    void RefreshBottom()
    {
        // 나침반 레벨 업
        _levelUpButton.interactable = !Manager.IsMaxLevel;
        UIRoot.Instance.SetButtonItemShortage(_levelUpButton, !Manager.CanLevelUp());
        _levelText.text = LocalizeManager.GetText("LEVEL_MARK", Manager.Level);
        BigInteger nowExp = Manager._compassExp.Count;
        BigInteger nextExp = Manager.GetNextLevelExp();
        double percent = Math.Min(1d, (double)nowExp / (double)nextExp);
        _expGauge.value = Manager.IsMaxLevel ? 1f : (float)percent;
        _expText.text = Manager.IsMaxLevel ? "" : $"{Formula.NumberToStringBy3(nowExp)}/{Formula.NumberToStringBy3(nextExp)}  {percent:P}";
    }

    void RefreshRight()
    {
        // 보상 기준
        _estimateRewardIcons.ClearGameObjectList();
        _maxStageText.text = LocalizeManager.GetText("UI_Compass_StageInfo", MaxStage.Level);
        List<BoxItem.ItemNode> rewardItems = GetRewardBox().GetRating();
        foreach (BoxItem.ItemNode itemNode in rewardItems)
        {
            ItemIcon estimateReward = Instantiate(_itemIconPrefab, _estimateRewardsParent);
            estimateReward.Setting(itemNode.ItemID, (int)itemNode.MaxCount);
            if (itemNode.RandValue > 0)
            {
                estimateReward.Text.text = $"{itemNode.PerValue:0.00}%";
            }
            _estimateRewardIcons.Add(estimateReward);
        }

        // 현재 획득 보상
        _rewardIcons.ClearGameObjectList();
        long minutes = Manager.ChargeTime / 60;
        bool canReceive = minutes >= SettingContents.Compass_Reward_MinTime;
        _noRewardText.gameObject.SetActive(!canReceive);
        if (canReceive)
        {
            foreach (BoxItem.ItemNode itemNode in rewardItems)
            {
                if (itemNode.RandValue > 0) continue;
                ItemIcon reward = Instantiate(_itemIconPrefab, _rewardsParent);
                BigInteger count = (BigInteger)(itemNode.MaxCount * minutes * GetRewardCountMultiplier());
                reward.Setting(itemNode.ItemID, count);
                _rewardIcons.Add(reward);
            }
        }

        // 버튼
        _getRewardButton.interactable = canReceive;
    }

    void LevelUpButton()
    {
        if (!Manager.CanLevelUp())
        {
            UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_Shortage_LevelUp_01"));
            return;
        }

        Manager.LevelUp();
        UIRoot.Instance.PlayLevelUpEffect(transform, null, null);
        RefreshAll();
    }

    void GetRewardButton()
    {
        int count = (int)Manager.ChargeTime / 60;
        count *= MaxStage.StageData.RewardCount_Compass;
        ReceiveReward(count);
        Manager.ResetCharge();
        RefreshRight();
    }

    void ShowQuickPopup(bool on)
    {
        _quickPopup.SetActive(on);
        if (!on) return;
        RefreshQuickPopup();
    }

    void RefreshQuickPopup()
    {
        _quick_TimeText.text = GetTimeText(TimeSpan.FromHours(SettingContents.Compass_SubReward_Time));
        _quick_RewardIcons.ClearGameObjectList();
        List<BoxItem.ItemNode> rewardItems = GetRewardBox().GetRating();
        foreach (BoxItem.ItemNode itemNode in rewardItems)
        {
            if (itemNode.RandValue > 0) continue;
            ItemIcon reward = Instantiate(_itemIconPrefab, _quick_RewardsParent);
            BigInteger count = (BigInteger)(itemNode.MaxCount * QuickReceiveBoxCount * GetRewardCountMultiplier());
            reward.Setting(itemNode.ItemID, count);
            _quick_RewardIcons.Add(reward);
        }
        int maxCount = SettingContents.Compass_SubReward_ItemCount.Count;
        _quick_CountText.text = $"{maxCount - Manager.ImmediateChageCount}/{maxCount}";
        bool available = Manager.HaveImmediateChargeCount();
        _quick_NotAvailableButton.gameObject.SetActive(!available);
        if (available)
        {
            ItemCostInfo quickCost = Manager.GetImmediateChargeCost();
            bool isFree = quickCost.Count == 0;
            _quick_FreeGetRewardButton.gameObject.SetActive(isFree);
            _quick_GetRewardButton.gameObject.SetActive(!isFree);
            _quick_GetRewardButton_Icon.sprite = AtlasManager.GetItemIcon(quickCost.Item, EIconType.Icon);
            _quick_GetRewardButton_Text.text = Formula.NumberToStringBy3(quickCost.Count);
            UIRoot.Instance.SetButtonItemShortage(_quick_GetRewardButton, !Manager.HaveImmediateChargeCost());
        }
    }

    void Quick_GetRewardButton()
    {
        // 비용 체크
        if (!Manager.HaveImmediateChargeCost())
        {
            UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
            return;
        }
        ItemCostInfo costItem = Manager.GetImmediateChargeCost();
        costItem.Item.Count -= costItem.Count;

        ReceiveReward(QuickReceiveBoxCount);
        Manager.ImmediateChageCount++;
        RefreshQuickPopup();
    }

    void ReceiveReward(int boxCount)
    {
        // 보상 지급
        BoxItem rewards = GetRewardBox();
        List<RewardItemInfo> items = rewards.GetResultItems(boxCount).Select(x => (RewardItemInfo)x).ToList();
        foreach (RewardItemInfo item in items) 
        {
            item.ItemCount *= GetRewardCountMultiplier();
            InventoryManager.Instance.GetItem(item.ItemID).Count += (BigInteger)item.ItemCount;
        }
        UIManager.Instance.ShowGetItemPopup(new()
        {
            rewardInfos = items,
        });
    }

    double GetRewardCountMultiplier()
    {
        int add = (Manager.Level - 1) * SettingContents.Compass_Reward_LevelAdd;
        return (SettingContents.Compass_Reward_Multiplier + add) / 10000d;
    }

    BoxItem GetRewardBox()
    {
        return InventoryManager.Instance.GetItem<BoxItem>(MaxStage.StageData.Reward_Compass);
    }

    string GetTimeText(TimeSpan span) => $"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
}
