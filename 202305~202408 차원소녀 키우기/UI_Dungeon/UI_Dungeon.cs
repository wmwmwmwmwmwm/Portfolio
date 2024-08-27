using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public partial class UI_Dungeon : UIMenuBarBase
{
    public enum EButtonId
    {
        None,
        ListElement,
        StageMinus,
        StagePlus,
        Enter,
        AutoPopup,
        AutoMin,
        AutoMinus,
        AutoPlus,
        AutoMax,
        AutoCancel,
        AutoOK,
        ClearPopup,
        Clear_Close,
        Clear_Reward,
        Clear_RewardAll,
    }

    [Header("위쪽")]
    [SerializeField] TMP_Text _timeText;
    [SerializeField] Image _ticketIcon;
    [SerializeField] TMP_Text _ticketCountText;

    [Header("왼쪽")]
    [SerializeField] ScrollRect _listScroll;
    [SerializeField] UI_Dungeon_ListElement _listElement;
    [SerializeField] Transform _listParent;

    [Header("오른쪽")]
    [SerializeField] Image _titleBg;
    [SerializeField] Image _backgroundBg;
    [SerializeField] TMP_Text _midTitle;
    [SerializeField] TMP_Text _descText;
    [SerializeField] TMP_Text _monsterNameText;
    [SerializeField] GameObject _levelSelect;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] List<TMP_Text> _dungeonSkillTexts;
    [SerializeField] ItemIcon _rewardIconPrefab;
    [SerializeField] Transform _rewardParent;
    [SerializeField] UI_Button _goButton, _autoButton;
    [SerializeField] Image _goCostIcon, _autoCostIcon;
    [SerializeField] TMP_Text _goCostCount, _autoCostCount;

    [Header("즉시완료 팝업")]
    [SerializeField] GameObject _autoCompletePopup;
    [SerializeField] TMP_Text _auto_HighscoreText;
    [SerializeField] Image _auto_CostIcon;
    [SerializeField] TMP_Text _auto_CountText;
    [SerializeField] UI_Button _auto_OKButton;

    [Header("최초클리어 팝업")]
    [SerializeField] GridViewAdapter _clear_gridView;
    [SerializeField] ItemIcon _firstClearReward;
    [SerializeField] GameObject _firstClearAlready;
    [SerializeField] GameObject _firstClearPopup;
    [SerializeField] RedDotComponent _firstClearRedDot;
    [SerializeField] Image _clear_Bg;
    [SerializeField] RectTransform _clear_ElementPrefab;
    [SerializeField] UI_Button _clear_RewardAllButton;
    [SerializeField] AnimationCurve _clear_ElementPosCurve;

    [Space]
    // _modelPositions, _modelSizeWeights 던전별 인덱스 확인용
    [SerializeField] PreviewSpace_ _preview;
    [SerializeField] List<RectTransform> _rt_ModelTransforms;
    [SerializeField, ReadOnly] List<string> _gameModeIndexInfos;

    StageInfo _startStageInfo;
    List<FStageConfig> _stageConfigs;
    List<UI_Dungeon_ListElement> _list_ListElements;
    UI_Dungeon_ListElement __selectedListElement;
    UI_Dungeon_ListElement _selected
    {
        get => __selectedListElement;
        set
        {
            SetSelectValue(value?.Config?.Gamemode.ToString());
            __selectedListElement = value;
            if(value != null)
                _dungeonTicket = InventoryManager.Instance.GetItem(value.Config.Ticketitemid) as Time_Ticket;
        }
    }
    Time_Ticket _dungeonTicket;
    bool _showingClearResult;

    int _selectedLevel;
    int _autoItemCount;
    List<PreviewModelInfo> _modelInfos;
    List<ItemIcon> _rewardIcons;
    bool _clear_Init;

    GameModeInfo _modeInfo => _selected.ModeInfo;
    StageInfo _stageInfo => _modeInfo.GetStage(_selectedLevel);
    bool ShowPreview => !_firstClearPopup.activeSelf && !_showingClearResult;

    public override void InitUI()
    {
        _autoCompletePopup.SetActive(false);
        _firstClearPopup.SetActive(false);
        _rewardIcons = new();
        _modelInfos = new();

        // 던전 리스트 초기화
        _stageConfigs = DataManager.Instance.GetStageConfigMap().Values.Where(x => x.Isdungeonui).ToList();
        _list_ListElements = new();
        foreach (FStageConfig stageConfig in _stageConfigs)
        {
            UI_Dungeon_ListElement listElement = Instantiate(_listElement, _listParent);
            listElement.Setting(stageConfig);
            _list_ListElements.Add(listElement);
            _gameModeIndexInfos.Add($"{LocalizeManager.GetText(stageConfig.Nametextid)} index : {(int)stageConfig.Gamemode}");
        }

        // 시작 화면 설정
        if (_startStageInfo != null)
        {
            UI_Dungeon_ListElement selected = _list_ListElements.Find(x => x.ModeInfo.GameMode == _startStageInfo.GameMode);
            SelectListElement(selected);
            Canvas.ForceUpdateCanvases();
            _listScroll.FocusOnItem(selected.GetComponent<RectTransform>());
            SelectLevel(_startStageInfo.Level);
            _startStageInfo = null;
        }
        else
        {
            SelectListElement(_list_ListElements[0]);
        }

        base.InitUI();
    }

    public override void OnUpdate()
    {
        if (_dungeonTicket != null)
        {
            if (_dungeonTicket.IsChage)
            {
                var h = _dungeonTicket.GetRemindTimeSec() / 3600;
                var m = (_dungeonTicket.GetRemindTimeSec() % 3600) / 60;
                var s = (_dungeonTicket.GetRemindTimeSec() % 3600) % 60;
                _timeText.text = $"{h:D2}:{m:D2}:{s:D2}";
            }
            else
                _timeText.text = $"--:--:--";
        }

        base.OnUpdate();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        string[] tokens = buttonInfo.ID.Split("__");
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.ListElement:
                // 던전 리스트 항목 선택
                int stageId = int.Parse(tokens[1]);
                UI_Dungeon_ListElement selected = _list_ListElements.Find(x => x.Config.Id == stageId);
                if (selected == _selected) return;
                SelectListElement(selected);
                break;
            case EButtonId.StageMinus:
                SelectLevel(_selectedLevel - 1);
                break;
            case EButtonId.StagePlus:
                SelectLevel(_selectedLevel + 1);
                break;
            case EButtonId.Enter:
                // 던전 입장
                if (!_modeInfo.HaveTicket())
                {
                    UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_Shortage_Dungeon"));
                    return;
                }
                if (!_modeInfo.CanEnter(_stageInfo)) return;
                if (null == _stageInfo) return;
                if (_modeInfo.GameMode == EGameMode.DungeonDPS)
                {
                    _selectedLevel = 1;
                }
                BattleSceneManager.Instance.SetBattleSceneLoad(_stageInfo.GameMode, _stageInfo.ID, ESCENEMOVITYPE.IgnoreFullScreen, null);
                break;
            case EButtonId.AutoPopup:
                ShowAutoCompletePopup(true);
                break;
            case EButtonId.AutoMin:
                _autoItemCount = 1;
                RefreshAutoCompletePopup();
                break;
            case EButtonId.AutoMinus:
                if (_autoItemCount > 1) _autoItemCount--;
                RefreshAutoCompletePopup();
                break;
            case EButtonId.AutoPlus:
                if (_autoItemCount < _modeInfo.GetTicketItem().Count) _autoItemCount++;
                RefreshAutoCompletePopup();
                break;
            case EButtonId.AutoMax:
                _autoItemCount = (int)_modeInfo.GetTicketItem().Count;
                RefreshAutoCompletePopup();
                break;
            case EButtonId.AutoCancel:
                ShowAutoCompletePopup(false);
                break;
            case EButtonId.AutoOK:
                List<RewardItemInfo> rewards = GetMaxMinusOneStage.InstantClear(_autoItemCount);
                StartCoroutine(UpdateInstantClearResult(rewards));
                _autoItemCount = 1;
                _auto_CountText.text = _autoItemCount.ToString();
                ShowAutoCompletePopup(false);
                UserDataManager.Save_LocalData();

                var dungeonLeaderboardPrc = new DungeonLeaderboardPrc(_modeInfo.GameMode);
                dungeonLeaderboardPrc.Clear();
                break;
            case EButtonId.ClearPopup:
                ShowFirstClearPopup(true);
                break;
            case EButtonId.Clear_Close:
                ShowFirstClearPopup(false);
                break;
            case EButtonId.Clear_Reward:
                int level = int.Parse(tokens[1]);
                ReceiveFirstRewards(level);
                break;
            case EButtonId.Clear_RewardAll:
                ReceiveFirstRewards(GetMaxMinusOneStage.Level);
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode)
    {
        RefreshInfo();
    }

    public override bool OnBackKey()
    {
        if (_autoCompletePopup.activeSelf)
        {
            ShowAutoCompletePopup(false);
            return false;
        }
        else if (_firstClearPopup.activeSelf) 
        {
            ShowFirstClearPopup(false);
            return false;
        }
        return base.OnBackKey();
    }

    void SelectLevel(int level)
    {
        level = Mathf.Clamp(level, 1, GetMaxLevel());
        _selectedLevel = level;
        _levelText.text = level.ToString();
        RefreshRight();
    }

    void RefreshInfo()
    {
        // 던전 이름, 설명 텍스트 설정
        _titleBg.sprite = AtlasManager.GetSprite(EATLAS_TYPE.UI_Parts, _modeInfo.GameMode switch
        {
            EGameMode.DungeonGold => "ui_com_title_bg_main_01",
            EGameMode.DungeonEquip => "ui_com_title_bg_main_01",
            EGameMode.DungeonSkill => "ui_com_title_bg_main_01",
            EGameMode.DungeonDPS => "ui_com_title_bg_main_01",
            EGameMode.DungeonPet => "ui_com_title_bg_main_01",
            EGameMode.DungeonTower => "ui_com_title_bg_main_01",
            EGameMode.DungeonAdvance => "ui_com_title_bg_main_01",
            _=> "ui_com_title_bg_main_01"
        });
        _midTitle.text = GetDungeonName();

        //_selectedListElement.Config.Image의 string의 뒤에값 두자리를 02로 변경하기
        var backgroudImageName = _selected.Config.Image.Substring(0, _selected.Config.Image.Length - 2) + "02";
        var backgroudImageAtlas = AtlasManager.GetSprite(EATLAS_TYPE.Icon, backgroudImageName);
        if(backgroudImageAtlas != null)
        {
            _backgroundBg.gameObject.SetActive(true);
            _backgroundBg.sprite = backgroudImageAtlas;
        }
        else
            _backgroundBg.gameObject.SetActive(false);
            
        _descText.text = LocalizeManager.GetText(_selected.Config.Desctextid[1]);

        // 몬스터 모델 출력
        if (ShowPreview)
        {
            StageMonsterData monsterData = _stageInfo.MonsterInfos.Find(x => x.Isboss == 1);
            monsterData ??= _stageInfo.MonsterInfos.FirstOrDefault();
            FCharacter charData = DataManager.Instance.GetCharacterData(monsterData.Characterid);
            _preview.ClearAll();
            StartCoroutine(Wait());
            IEnumerator Wait()
            {
                yield return null;
                PreviewModelInfo previewModel = _preview.GetModel(charData.Resourceid);
                if (previewModel == null)
                {
                    previewModel = _preview.AddCharacterModel(charData, model =>
                    {
                        var rt = _rt_ModelTransforms.Find(p => p.name == _modeInfo.GameMode.ToString());
                        if (null != rt)
                        {
                            model.GetOwnerModel().transform.SetLocalPositionAndRotation(rt.localPosition, rt.localRotation);
                            model.SetScale(rt.localScale.x);
                        }
                    });
                    _modelInfos.Add(previewModel);
                    previewModel.AnimationInfo = new()
                    {
                        ActionAnimations = new()
                    {
                        EAniType.Ani_IdleW01,
                        EAniType.Ani_IdleW02,
                    }
                    };
                }
            }
            _monsterNameText.text = LocalizeManager.GetText(charData.Nameid);
        }

        // 던전별 세팅
        switch (_modeInfo.GameMode)
        {
            case EGameMode.DungeonSkill:
                _levelSelect.SetActive(true);
                break;
            case EGameMode.DungeonDPS:
                _levelSelect.SetActive(false);
                break;
            default:
                _levelSelect.SetActive(true);
                break;
        }
        RefreshRight();
    }

    void RefreshRight()
    {
        // 던전별 세팅
        switch (_modeInfo.GameMode)
        {
            case EGameMode.DungeonSkill:
                GameModeInfo skillinfo = StageInfoManager.Instance.GetSkillDungeon();
                break;
        }

        // 최초 클리어 보상
        (int itemId, BigInteger count) firstClearReward = _stageInfo.GetFirstRewardList().First();
        _firstClearReward.Setting(firstClearReward.itemId, firstClearReward.count);
        (_, bool canGetRewardSelected) = GetFirstClearRewardState(_stageInfo);
        _firstClearAlready.SetActive(canGetRewardSelected);
        bool anyFirstReward = DungeonManager.Instance.AnyFirstReward(_modeInfo);
        _firstClearRedDot.SetActiveRedDot(anyFirstReward);
        _firstClearReward.SetShiny(anyFirstReward);

        // 기대 보상
        _rewardIcons.ForEach(x => Destroy(x.gameObject));
        _rewardIcons.Clear();
        foreach ((int id, BigInteger count) in _stageInfo.GetRewardList())
        {
            ItemIcon icon = Instantiate(_rewardIconPrefab, _rewardParent);
            icon.Setting(id, count);
            _rewardIcons.Add(icon);
        }

        // 던전 패시브 스킬
        SkillItem mainSkill = _stageInfo.GetStageSkill();
        List<SkillItem> skills = new() { mainSkill };
        if (mainSkill != null) 
        {
            skills.AddRange(mainSkill.SkillData.Skills.Select(x => InventoryManager.Instance.GetItem(x) as SkillItem));
        }
        for (int i = 0; i < _dungeonSkillTexts.Count; i++)
        {
            TMP_Text skillText = _dungeonSkillTexts[i];             
            bool active = i < skills.Count;
            skillText.gameObject.SetActive(active);
            if (active)
            {
                SkillItem skill = skills[i];
                skillText.text = LocalizeManager.GetText(skill?.itemData.Descid);
            }
        }

        // 입장권 아이템 수량 설정
        Time_Ticket ticket = _modeInfo.GetTicketItem() as Time_Ticket;
        FStageConfig config = _modeInfo.StageConfigData;
        Sprite ticketIcon = AtlasManager.GetItemIcon(ticket, EIconType.MiniIcon);
        _ticketIcon.sprite = ticketIcon;
        _ticketCountText.text = $"{ticket.Count} / {ticket.MaxChageCount}";
        _goCostIcon.sprite = ticketIcon;
        _autoCostIcon.sprite = ticketIcon;
        _goCostCount.text = $"{config.Ticketcount}";
        _autoCostCount.text = $"{config.Ticketcount}";

        // 입장, 즉시완료 가능여부로 버튼 설정
        bool haveItem = ticket.Count >= config.Ticketcount;
        UIRoot.Instance.SetButtonItemShortage(_goButton, !_modeInfo.CanEnter(_stageInfo) || !haveItem);
        _autoButton.interactable = GetMaxMinusOneStage != null && haveItem;
    }

    void SelectListElement(UI_Dungeon_ListElement newSelectedElement)
    {
        _selected = newSelectedElement;
        SelectLevel(GetMaxLevel());
        foreach (UI_Dungeon_ListElement element in _list_ListElements)
        {
            element.OnSelect(element == _selected);
        }
        RefreshInfo();
    }

    IEnumerator UpdateInstantClearResult(List<RewardItemInfo> result)
    {
        _showingClearResult = true;
        yield return UIManager.Instance.ShowGetItemPopupCoroutine(new()  //아이콘 부분 만들어야함
        {
            TitleText = LocalizeManager.GetText("UI_Dungeon_Result_Imm_Title"),
            ItemIds = result.Select(p => (int)p.ItemID).ToList(),
            ItemCounts = result.Select(p => (BigInteger)p.ItemCount).ToList(),
        });
        _showingClearResult = false;
        RefreshInfo();
    }

    void ShowAutoCompletePopup(bool show)
    {
        _autoCompletePopup.SetActive(show);
        if (!show)
        {
            RefreshInfo();
            return;
        }
        _preview.ClearAll();
        var ticket = _modeInfo.GetTicketItem();
        _auto_CostIcon.sprite = AtlasManager.GetItemIcon(ticket, EIconType.MiniIcon);
        _auto_HighscoreText.text = GetMaxMinusOneStage.GetFullName();
        _autoItemCount = 1;
        RefreshAutoCompletePopup();
    }

    void ShowFirstClearPopup(bool show)
    {
        if (!show)
        {
            _firstClearPopup.SetActive(false);
            RefreshInfo();
            return;
        }
        if (!_clear_Init)
        {
            _firstClearPopup.SetActive(true);
            _clear_gridView.Setting(_clear_ElementPrefab, Clear_CreateCell, Clear_UpdateCell, null);
            _clear_Init = true;
        }

        _preview.ClearAll();
        _firstClearPopup.SetActive(true);
        _clear_gridView.SetItems(_modeInfo.LastStage.Level);
        _clear_gridView.MoveTo(LevelToIndex(GetMaxLevel()), false);
        _clear_Bg.sprite = AtlasManager.GetSprite(EATLAS_TYPE.Icon, _modeInfo.StageConfigData.Image);
        RefreshFirstClearPopup();
    }

    void RefreshFirstClearPopup()
    {
        _clear_gridView.UpdateGrid();
        _clear_RewardAllButton.interactable = DungeonManager.Instance.AnyFirstReward(_modeInfo);
    }

    (ScrollItemInfo itemInfo, RectTransform prefab) Clear_CreateCell(int index) => (null, _clear_ElementPrefab);
    ScrollItemInfo Clear_UpdateCell(int idx, ScrollItemInfo itemInfo, RectTransform prefab)
    {
        UI_Dungeon_RewardElement element = prefab.GetComponent<UI_Dungeon_RewardElement>();
        StageInfo stage = _modeInfo.GetStage(IndexToLevel(idx));
        (bool already, bool canGetReward) = GetFirstClearRewardState(stage);
        float curveX = (idx % 10f) / 10f; 
        float yPos = _clear_ElementPosCurve.Evaluate(curveX);
        element.Setting(stage, already, canGetReward, yPos);
        return itemInfo;
    }

    void ReceiveFirstRewards(int destLevel)
    {
        List<RewardItemInfo> rewards = new();
        for (int level = (int)_modeInfo.RewardStep + 1; level <= destLevel; level++)
        {
            StageInfo stage = _modeInfo.GetStage(level);
            (_, bool canGetReward) = GetFirstClearRewardState(stage);
            if (!canGetReward) continue;
            rewards.AddRange(RewardItemInfo.ReceiveRewardItems(new() { stage.FirstClearReward }));
        }
        _modeInfo.RewardStep = destLevel;
        UserDataManager.Save_LocalData();
        List<RewardItemInfo> results = RewardItemInfo.OrganizingDuplicateRewardItems(null, rewards);
        UIManager.Instance.ShowGetItemPopup(new()
        {
            TitleText = LocalizeManager.GetText("UI_Reward_Title"),
            rewardInfos = results,
        });
        RefreshFirstClearPopup();
    }

    void RefreshAutoCompletePopup()
    {
        _auto_CountText.text = _autoItemCount.ToString();
        _auto_OKButton.interactable = _autoItemCount > 0;
    }

    string GetDungeonName()
    {
        return LocalizeManager.GetText(_selected.Config.Nametextid);
    }

    (bool already, bool canGetReward) GetFirstClearRewardState(StageInfo stage)
    {
        bool already = stage.Level <= _modeInfo.RewardStep;
        bool canGetReward = !already && stage.IsClearedStage();
        return (already, canGetReward);
    }

    public void SetStartStageInfo(StageInfo stage) => _startStageInfo = stage;
    int LevelToIndex(int level) => level - 1;
    int IndexToLevel(int index) => index + 1;
    int GetMaxLevel() => _modeInfo.MaxStage.Level;
    StageInfo GetMaxMinusOneStage => _modeInfo.GetStage(GetMaxLevel() - 1);
}
