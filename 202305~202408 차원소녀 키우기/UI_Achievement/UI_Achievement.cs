using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class UI_Achievement : UIBase
{
    public enum EAchievementState
    {
        CanGetReward,
        NotComplete,
        Complete
    }

    public enum ETabId
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Repeat
    }

    public enum EButtonId
    {
        None,
        Close,
        GetReward,
        GetRewardAll,
    }

    [SerializeField] UI_TabGroup _tabGroup;
    //[SerializeField] List<UI_Achievement_TabButton> _tabButtons;
    [SerializeField] List<MenuBar_Menu> _tabButtons;
    [SerializeField] UI_AchievementElement _elementPrefab;
    [SerializeField] Transform _listParent;
    [SerializeField] ScrollRect _listScrollRect;
    [SerializeField] UI_Button _getRewardAllButton;

    List<ETabId> _tabs = new() { ETabId.Daily, ETabId.Weekly, ETabId.Monthly, ETabId.Repeat };
    ETabId _currentTab;
    List<UI_AchievementElement> _elements;
    Dictionary<EResetTimeType, List<AchievementInfo>> _achievementDict;

    AchievementManager Manager => AchievementManager.Instance;

    public override void InitUI()
    {
        UIManager.Instance.RegisterEvent_Achievement(gameObject, UpdateStates);
        _elements = new();

        // 업적 데이터 초기화
        var maxStage = StageInfoManager.Instance.GetMode(EGameMode.Stage).MaxValue;
        _achievementDict = Manager.AchievementInfos.Values.Where(p=>p.Data.Openstageid >= 0 && p.Data.Openstageid <= maxStage).GroupBy(x => x.Data.Resettimetype).ToDictionary(k => (EResetTimeType)k.Key, v => v.ToList());
        foreach (EResetTimeType timeType in Enum.GetValues(typeof(EResetTimeType)))
        {
            if (!_achievementDict.ContainsKey(timeType))
            {
                _achievementDict.Add(timeType, new());
            }
        }

        // 탭 초기화
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            //_tabButtons[i].Setting(tabs[i]);
            _tabButtons[i].Button.SetID(_tabs[i].ToString());
        }
        _tabGroup.Init();
        _tabGroup.SetSelectButton(0);
        _currentTab = ETabId.Daily;
        OnClickTab(_currentTab);

        base.InitUI();
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode)
    {
        UpdateStates();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        // 업적 탭 선택
        if (buttonInfo.ButtonType == UI_ButtonType.TAB)
        {
            ETabId tabId = EnumHelper.Parse(buttonInfo.ID, ETabId.None);
            if (_currentTab == tabId) return;
            OnClickTab(tabId);
            return;
        }

        // 버튼 선택
        string[] idTokens = buttonInfo.ID.Split('_');
        EButtonId buttonType = EnumHelper.Parse(idTokens[0], EButtonId.None);
        switch (buttonType)
        {
            case EButtonId.Close:
                OutUI();
                break;
            case EButtonId.GetReward:
                int id = int.Parse(idTokens[1]);
                UI_AchievementElement element = _elements.Find(x => x.Data.Id == id);
                OnClickGetReward(element);
                break;
            case EButtonId.GetRewardAll:
                OnClickGetRewardAll();
                break;
        }
    }

    void OnClickTab(ETabId tabId)
    {
        _currentTab = tabId;
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            MenuBar_Menu tabButton = _tabButtons[i];
            tabButton.Animate();
            string textKey = _tabs[i] switch
            {
                ETabId.Daily => "UI_Achieve_Tap_01",
                ETabId.Weekly => "UI_Achieve_Tap_02",
                ETabId.Monthly => "UI_Achieve_Tap_03",
                _ => "UI_Achieve_Tap_04"
            };
            _tabButtons[i].Text.text = LocalizeManager.GetText(textKey);
            _tabButtons[i].Text.gameObject.SetActive(true);
        }
        EResetTimeType timeType = TabIdToResetTimeType(tabId);
        List<AchievementInfo> currentTabDatas = _achievementDict[timeType];

        // 업적 리스트 초기화
        _elements.ForEach(x => Destroy(x.gameObject));
        _elements.Clear();
        for (int i = 0; i < currentTabDatas.Count; i++)
        {
            AchievementInfo data = currentTabDatas[i];
            UI_AchievementElement newElement = Instantiate(_elementPrefab, _listParent);
            _elements.Add(newElement);
            newElement.Setting(data);
        }

        _listScrollRect.verticalNormalizedPosition = 1f;
        UpdateStates();
    }

    void UpdateStates()
    {
        // 업적 항목 업데이트
        foreach (UI_AchievementElement element in _elements)
        {
            element.UpdateState();
        }
        _getRewardAllButton.interactable = _elements.Any(x => x.Data.GetReceiveRewardInfosNow() != null);

        // 모두완료가 제일 위, (보상 받기, 진행중, 완료) 상태별로 정렬
        _elements.Sort((x, y) =>
        {
            if (IsCompleteMission(x.Data.Data.Mtype)) return -1;
            else if (IsCompleteMission(y.Data.Data.Mtype)) return 1;
            else return 1_0000_0000 * (x.State - y.State) + (x.Data.Id - y.Data.Id);

            bool IsCompleteMission(EMissionType missionType)
            {
                return missionType is EMissionType.Daily_Complete or EMissionType.Weekly_Complete or EMissionType.Month_Complete;
            }
        });
        foreach (UI_AchievementElement element in _elements)
        {
            element.transform.SetAsLastSibling();
        }
    }

    void OnClickGetReward(UI_AchievementElement element)
    {
        // 보상 받기
        int prevLevel = element.Data.Level;
        List<RewardItemInfo> rewards = element.Data.GetReceiveRewardInfosNow();
        element.Data.ReceiveAllReward();
        ShowRewardPopup(rewards);
        UserDataManager.Save_LocalData();
        OnRefreshUI();
    }

    void OnClickGetRewardAll()
    {
        // 모든 보상 받기
        List<RewardItemInfo> totalRewards = new();
        foreach (UI_AchievementElement item in _elements)
        {
            int prevLevel = item.Data.Level;
            ReceiveRewardAll(totalRewards, item);
        }
        UserDataManager.Save_LocalData();
        ShowRewardPopup(totalRewards);
        OnRefreshUI();
    }

    void ReceiveRewardAll(List<RewardItemInfo> totalRewards, UI_AchievementElement item)
    {
        List<RewardItemInfo> rewardInfos = item.Data.GetReceiveRewardInfosNow();
        if (rewardInfos == null) return;
        foreach (RewardItemInfo rewardInfo in rewardInfos)
        {
            RewardItemInfo reward = totalRewards.Find(x => x.ItemID == rewardInfo.ItemID);
            if (reward == null)
            {
                totalRewards.Add(rewardInfo);
            }
            else
            {
                reward.ItemCount += rewardInfo.ItemCount;
            }
        }
        item.Data.ReceiveAllReward();
    }

    void ShowRewardPopup(List<RewardItemInfo> rewards)
    {
        // 팝업 출력
        StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
        {
            TitleText = LocalizeManager.GetText("UI_Achieve_Reward_Title"),
            ItemIds = rewards.Select(x => (int)x.ItemID).ToList(),
            ItemCounts = rewards.Select(x => (BigInteger)x.ItemCount).ToList()
        }));
    }

    EResetTimeType TabIdToResetTimeType(ETabId tabId)
    {
        return tabId switch
        {
            ETabId.Daily => EResetTimeType.Day,
            ETabId.Weekly => EResetTimeType.Week,
            ETabId.Monthly => EResetTimeType.Month,
            ETabId.Repeat => EResetTimeType.NONE,
            _ => EResetTimeType.NONE,
        };
    }
}
