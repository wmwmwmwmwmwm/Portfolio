using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StageClear : UIBase
{
    enum EButtonId
    {
        None,
        Exit,
        Retry,
        NextLevel,
    }

    [SerializeField] protected Animation _titleAnimation;
    [SerializeField] protected GameObject _defaultTitle, _dpsFailTitle;
    [SerializeField] protected TMP_Text _stageText;
    [SerializeField] protected GameObject _defaultGroup, _dpsGroup;
    [SerializeField] protected GameObject _dpsCurrentLevel;
    [SerializeField] protected TMP_Text _dpsMaxLevelText, _dpsCurrentLevelText;
    [SerializeField] protected ItemIcon _itemPrefab;
    [SerializeField] protected Transform _itemParent;
    [SerializeField] protected GameObject _towerNextReward;
    [SerializeField] protected ItemIcon _towerNextRewardItem;
    [SerializeField] protected TMP_Text _descText;
    [SerializeField] protected TMP_Text _secText;
    [SerializeField] protected UI_Button _retryButton, _nextLevelButton;
    [SerializeField] protected Image _retryButtonIcon, _nextLevelButtonIcon;

    List<RewardItemInfo> _rewards;
    bool _receiveInput;
    protected float _remainTime = 5f;

    GameModeInfo ModeInfo
    {
        get
        {
            EGameMode mode = BattleSceneManager.Instance.GetBattleScene().GameMode;
            return StageInfoManager.Instance.GetMode(mode);
        }
    }
    bool HasNextStage => ModeInfo.CurrentStage != ModeInfo.LastStage;

    public override void InitUI()
    {
        _itemPrefab.gameObject.SetActive(false);
        UIManager.Instance.OutAdditionalUIs();

        StageInfo stageInfo = BattleSceneManager.Instance.GetBattleScene().StageInfo;
        _stageText.text = stageInfo.GetFullName();

        UIRoot.Instance.SetButtonItemShortage(_retryButton, !ModeInfo.HaveTicket());
        UIRoot.Instance.SetButtonItemShortage(_nextLevelButton, !ModeInfo.HaveTicket());
        Sprite ticketIcon = AtlasManager.GetItemIcon(ModeInfo.GetTicketItem(), EIconType.MiniIcon);
        _retryButtonIcon.sprite = _nextLevelButtonIcon.sprite = ticketIcon;
        _retryButtonIcon.gameObject.SetActive(ticketIcon);
        _nextLevelButtonIcon.gameObject.SetActive(ticketIcon);
        _nextLevelButton.gameObject.SetActive(HasNextStage);

        // 던전별 세팅
        _towerNextReward.Obj()?.SetActive(false);
        _defaultGroup.Obj()?.SetActive(false);
        _dpsGroup.Obj()?.SetActive(false);
        _defaultTitle.Obj()?.SetActive(false);
        _dpsFailTitle.Obj()?.SetActive(false);
        switch (ModeInfo.GameMode)
        {
            case EGameMode.DungeonDPS:
                _dpsGroup.SetActive(true);
                DungeonSceneController scene = BattleSceneManager.Instance.GetBattleScene() as DungeonSceneController;
                int startLevel = scene.StartDpsDungeonLevelIndex + 1;
                int currentLevel = scene.DpsDungeonLevelIndex + 1;
                bool clear = currentLevel > startLevel;
                _defaultTitle.SetActive(clear);
                _dpsFailTitle.SetActive(!clear);
                _dpsMaxLevelText.text = startLevel.ToString();
                _dpsCurrentLevel.SetActive(clear);
                _dpsCurrentLevelText.text = currentLevel.ToString();
                _nextLevelButton.gameObject.SetActive(false);
                break;
            case EGameMode.DungeonTower:
                _retryButton.gameObject.SetActive(false);
                _towerNextReward.SetActive(HasNextStage);
                if (HasNextStage)
                {
                    _towerNextRewardItem.Setting(ModeInfo.GetStage(ModeInfo.CurrentStage.Level + 1).Reward);
                }
                break;
            default:
                _defaultGroup.Obj()?.SetActive(true);
                break;
        }
        switch (ModeInfo.GameMode)
        {
            case EGameMode.DungeonDPS:
                break;
        }

        base.InitUI();
    }

    public override void OpeningAnimationFinish()
    {
        if(_titleAnimation != null)
            _titleAnimation.Play();
        StartCoroutine(TweenItems());
        _receiveInput = true;
        base.OpeningAnimationFinish();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;
        if (!_receiveInput) return;

        EButtonId buttonId = EnumHelper.Parse(buttonInfo.ID, EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.Exit:
                UI_Tower.AutoProceed = false;
                ReturnToStage();
                break;
            case EButtonId.Retry:
                GoDungeon(false);
                break;
            case EButtonId.NextLevel:
                GoDungeon(true);
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode) { }

    public override void OnUpdate()
    {
        _remainTime -= Time.deltaTime;
        if (_remainTime < 0f)
        {
            bool goNext = true;
            if (ModeInfo.GameMode == EGameMode.Raid 
                || ModeInfo.GameMode == EGameMode.DungeonDPS
                || (ModeInfo.GameMode == EGameMode.DungeonTower && !UI_Tower.AutoProceed))
            {
                goNext = false;
            }
            if (goNext && ModeInfo.HaveTicket() && HasNextStage)
            {
                GoDungeon(true);
            }
            else
            {
                ReturnToStage();
            }
        }

        string remainStr = (_remainTime + 0.9f).ToString("0");
        _descText.text = ModeInfo.GameMode switch
        {
            EGameMode.DungeonDPS or
            EGameMode.DungeonTower => LocalizeManager.GetText("UI_Dungeon_ClearFailed_Desc", remainStr),
            EGameMode.Raid => LocalizeManager.GetText("UI_Raid_Clear_Desc", remainStr),
            _ => LocalizeManager.GetText("UI_Dungeon_ClearFailed_Desc_02", remainStr),
        };
        _secText.setLocalizeText("UI_Cooltime", remainStr);
    }

    public override bool OnBackKey()
    {
        ReturnToStage();
        return false;
    }

    public void SetRewards(List<RewardItemInfo> rewards)
    {
        _rewards = rewards;
    }

    IEnumerator TweenItems()
    {
        foreach (RewardItemInfo reward in _rewards)
        {
            BaseItem item = InventoryManager.Instance.GetItem(reward.ItemID);
            if (item == null) continue;
            ItemIcon itemIcon = Instantiate(_itemPrefab, _itemParent);
            itemIcon.gameObject.SetActive(true);
            itemIcon.Setting(item.itemData.Id, reward.ItemCount);
            itemIcon.GetComponent<UIPlayTween>().Play();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    void GoDungeon(bool isNext)
    {
        if (!ModeInfo.HaveTicket())
        {
            UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_Shortage_Dungeon"));
            return;
        }
        int level = ModeInfo.GameMode != EGameMode.DungeonDPS ? ModeInfo.CurrentStage.Level : 1;
        level += isNext && ModeInfo.MaxStage != ModeInfo.CurrentStage ? 1 : 0;
        StageInfo stage = ModeInfo.GetStage(level);
        BattleSceneManager.Instance.SetBattleSceneLoad(stage.GameMode, stage.ID, ESCENEMOVITYPE.IgnoreFullScreen, null);
        OutUI();
    }

    void ReturnToStage()
    {
        BattleSceneManager.Instance.GetBattleScene().PrepareDungeonUI(ModeInfo.GameMode);
        var next = StageInfoManager.Instance.GetNormalStage().CurrentValue;
        BattleSceneManager.Instance.SetBattleSceneLoad(EGameMode.Stage, (int)next, ESCENEMOVITYPE.Normal, null);
        OutUI();
    }
}
