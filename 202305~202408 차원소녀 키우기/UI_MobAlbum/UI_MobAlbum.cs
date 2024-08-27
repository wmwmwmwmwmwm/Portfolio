using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using Vector3 = UnityEngine.Vector3;
using static Popup_LevelUpAllResult;

public class UI_MobAlbum : UIMenuBarBase
{
    public enum EButtonId
    {
        None,
        Category,
        GoalReward,
        Mob,
        GoStage,
        GoStageOK,
        GoStageCancel,
        LevelUp,
        LevelUpAll,
    }

    // 몬스터 모델크기 수동보정 리스트
    [Serializable]
    public class ModelSize
    {
        public int Id;
        public float Size;
    }
    [SerializeField] List<ModelSize> _monsterSizeList;
    [SerializeField, ReadOnly] int _monsterId;
    [SerializeField] PreviewSpace_ _preview;

    [Header("카테고리")]
    [SerializeField] UI_MobAlbum_CategoryElement _categoryElementPrefab;
    [SerializeField] Transform _categoryElementParent;

    [Header("몬스터 리스트")]
    [SerializeField] UI_MobAlbum_MobElement _mobElementPrefab;
    [SerializeField] Transform _mobElementParent;
    [SerializeField] UI_Button _levelUpAllButton;

    [Header("탐구 정보")]
    //[SerializeField] GameObject _info_StatArrow;
    [SerializeField] TMP_Text _info_TitleText;
    [SerializeField] TMP_Text _info_LevelText, _info_DescText;
    [SerializeField] TMP_Text _info_StatNameText, _info_SmallStatText/*, _info_BigStatText*/;
    [SerializeField] UI_Button _info_LevelUpButton;
    [SerializeField] GameObject _info_LevelMax;
    [SerializeField] Image _info_CostIcon;
    [SerializeField] TMP_Text _info_CostText;
    [SerializeField] Transform _info_EffectPosition;

    [Header("합산 레벨")]
    [SerializeField] TMP_Text _total_LevelText;
    [SerializeField] Slider _total_Gauge;
    [SerializeField] UI_MobAlbum_GoalElement _total_GoalPrefab;
    [SerializeField] Transform _total_GoalParent;
    [SerializeField] Transform _total_LeftPoint, _total_RightPoint;

    [Header("스테이지 이동 팝업")]
    [SerializeField] GameObject _goStagePopup;
    [SerializeField] TMP_Text _goStage_Desc;
    [SerializeField] TMP_Text _goStage_StageText;

    [Header("일괄탐구 결과 팝업")]
    [SerializeField] Popup_LevelUpAllResult _levelUpAllResultPopup;

    List<UI_MobAlbum_CategoryElement> _categoryElements;
    List<UI_MobAlbum_MobElement> _mobElements;
    List<UI_MobAlbum_GoalElement> _goalElements;
    UI_MobAlbum_CategoryElement _selectedCategory;
    UI_MobAlbum_MobElement _selectedMob;
    StageInfo _lastMonsterStage;
    List<StageInfo> _normalStages;
    bool _showShortageToast;
    bool _isChange;

    public override void InitUI()
    {
        _categoryElements = new();
        _mobElements = new();
        _goalElements = new();
        _goStagePopup.SetActive(false);
        _levelUpAllResultPopup.Init();
        UIManager.Instance.RegisterEvent_GetItem(EItemType.MobAlbumTicket, gameObject, RefreshButtons);        
        NormalStageInfo normalStageInfo = StageInfoManager.Instance.GetNormalStage();
        _normalStages = normalStageInfo.StageMap.Values.ToList();

        // 좌측 카테고리
        List<ObscuredInt> allCategory = MonsterAlbumManager.Instance.MonsterAlbumInfos.Keys.OrderBy(x => (int)x).ToList();
        for (int i = 0; i < allCategory.Count; i++)
        {
            int category = allCategory[i];
            UI_MobAlbum_CategoryElement newCategory = Instantiate(_categoryElementPrefab, _categoryElementParent);
            newCategory.Setting(category, i);
            _categoryElements.Add(newCategory);
        }
        SelectCategory(_categoryElements[0]);

        //// 캔버스 설정
        //Canvas.worldCamera = UIRoot.Instance.PreviewSpace.PreviewCamera;
        //Canvas.planeDistance = 1000f;
        base.InitUI();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State == UI_ButtonState.HoldEnd)
        {
            if (_isChange)
            {
                UserDataManager.Save_LocalData();
                _isChange = false;
            }
            _showShortageToast = true;
        }
        if (buttonInfo.State != UI_ButtonState.Click) return;

        string[] tokens = buttonInfo.ID.Split('_');
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.Category:
                // 카테고리 선택
                int categoryIndex = int.Parse(tokens[1]);
                SelectCategory(_categoryElements[categoryIndex]);
                break;
            case EButtonId.Mob:
                // 몬스터 선택
                int mobIndex = int.Parse(tokens[1]);
                SelectMob(_mobElements[mobIndex]);
                break;
            case EButtonId.GoStage:
                ShowGoStagePopup(true);
                break;
            case EButtonId.GoStageOK:
                // 출현하는 맵으로 이동
                CloseAndGoStage();
                break;
            case EButtonId.GoStageCancel:
                ShowGoStagePopup(false);
                break;
            case EButtonId.GoalReward:
                // 합산레벨 보상버튼
                int goalIndex = int.Parse(tokens[1]);
                SelectGoalReward(_goalElements[goalIndex]);
                break;
            case EButtonId.LevelUp:
                // 탐구 버튼
                int costId = _selectedMob.AlbumData.Itemid;
                BigInteger costCount = _selectedMob.Album.NextLevelCount;
                bool success = _selectedMob.Album.SetLevelUp();
                if (!success)
                {
                    if (_showShortageToast)
                    {
                        UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                        _showShortageToast = false;
                    }
                    return;
                }
                _isChange = true;
                RefreshPlayer();
                MobEffect();
                RefreshMobList();
                RefreshButtons();
                break;
            case EButtonId.LevelUpAll:
                // 일괄탐구 버튼
                LevelUpAll();
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode)
    {
        RefreshMobList();
        RefreshButtons();
        RefreshTitle();
    }

    public override bool OnBackKey()
    {
        if (_goStagePopup.activeSelf)
        {
            ShowGoStagePopup(false);
            return false;
        }
        else if (_levelUpAllResultPopup.gameObject.activeSelf)
        {
            _levelUpAllResultPopup.gameObject.SetActive(false);
            return false;
        }
        return base.OnBackKey();
    }

    void CreateMobList()
    {
        // 몬스터 리스트 생성
        _mobElements.ClearGameObjectList();
        List<MobAlbumInfo> mobs = MonsterAlbumManager.Instance.MonsterAlbumInfos[_selectedCategory.Category];
        for (int i = 0; i < mobs.Count; i++)
        {
            MobAlbumInfo mob = mobs[i];
            UI_MobAlbum_MobElement newMob = Instantiate(_mobElementPrefab, _mobElementParent);
            newMob.Setting(mob, i);
            _mobElements.Add(newMob);
        }
        SelectMob(_mobElements[0]);
        RefreshMobList();
        RefreshTitle();
    }

    void RefreshMobList()
    {
        _mobElements.ForEach(x => x.Setting());

        // 탐구 정보
        _info_TitleText.text = _selectedMob.Name;
        _info_LevelText.text = $"Lv. {_selectedMob.Album.Level} / {_selectedMob.AlbumSettingData.Last().Level}";
        _info_DescText.text = LocalizeManager.GetText(_selectedMob.AlbumData.Desc);
        MobAlbumInfo.RewardState rewardStat = _selectedMob.Album.GetNowRewardStateInfo();
        StatValueInfo statValue = new(rewardStat.Stat, rewardStat.Value, rewardStat.Calculation);
        _info_StatNameText.text = string.Format(LocalizeManager.GetText("UI_MobAlbum_Stat"), statValue.StatString);
        bool canLevelUp = !_selectedMob.Album.isMaxLevel;
        _info_LevelUpButton.gameObject.SetActive(canLevelUp);
        _info_LevelMax.SetActive(!canLevelUp);
        //_info_StatArrow.SetActive(canLevelUp);
        //_info_SmallStatText.gameObject.SetActive(canLevelUp);
        //if (canLevelUp)
        //{
        //    _info_SmallStatText.text = statValue.ValueString;
        //    MobAlbumInfo.RewardState nextRewardStat = _selectedMob.Album.GetNextRewardStateInfo();
        //    StatValueInfo nextStatValue = new(rewardStat.Stat, nextRewardStat.Value, rewardStat.Calculation);
        //    _info_BigStatText.text = nextStatValue.ValueString;
        //}
        //else
        //{
        //    _info_BigStatText.text = statValue.ValueString;
        //}
        _info_SmallStatText.text = '+' + statValue.ValueString;

        // 합산 레벨
        int currentLevel = _selectedCategory.TotalInfo.NowExp;
        int maxLevel = _selectedCategory.TotalInfo.MonsterTotalGroup.Last().Step;
        _total_LevelText.text = $"Lv. {currentLevel} / {maxLevel}";
        _total_Gauge.value = (float)currentLevel / maxLevel;
        _goalElements.ClearGameObjectList();
        MonsterAlbumTotalInfo goalsInfo = MonsterAlbumManager.Instance.TotalInfos[_selectedCategory.Category];
        for (int i = 0; i < goalsInfo.MonsterTotalGroup.Count; i++)
        {
            FMobAlbumTotal goal = goalsInfo.MonsterTotalGroup[i];
            UI_MobAlbum_GoalElement newGoal = Instantiate(_total_GoalPrefab, _total_GoalParent);
            newGoal.Setting(goalsInfo, goal, i, _total_LeftPoint, _total_RightPoint);
            _goalElements.Add(newGoal);
        }

        Canvas.ForceUpdateCanvases();
    }

    void RefreshButtons()
    {
        //_info_LevelUpButton.interactable = _selectedMob.Album.CanLevelUp();
        UIRoot.Instance.SetButtonItemShortage(_info_LevelUpButton, !_selectedMob.Album.CanLevelUp(), true);
        _info_CostIcon.sprite = AtlasManager.GetItemIcon(_selectedMob.AlbumData.Itemid, EIconType.MiniIcon);
        _info_CostText.text = $"{_selectedMob.Album.TicketCount} / {_selectedMob.Album.NextLevelCount}";
        _levelUpAllButton.interactable = _mobElements.Any(x => x.Album.CanLevelUp());
    }

    void SelectCategory(UI_MobAlbum_CategoryElement newSelected)
    {
        if (newSelected == _selectedCategory) return;
        _selectedCategory = newSelected;
        foreach (UI_MobAlbum_CategoryElement category in _categoryElements)
        {
            category.Select(category ==  _selectedCategory);
        }
        CreateMobList();
        OnRefreshUI();
    }

    void SelectMob(UI_MobAlbum_MobElement newSelected)
    {
        if (newSelected == _selectedMob) return;

        // 기존 몬스터 모델 끄기
        _preview.ClearAll();
        _selectedMob = newSelected;
        _mobElements.ForEach(x => x.Select(x == _selectedMob));

        // 몬스터 모델 출력
        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return null;
            _preview.ModelContainer.parent.localRotation = UnityEngine.Quaternion.Euler(0f, 180f, 0f);
            PreviewModelInfo previewModel = _preview.GetModel(_selectedMob.MonsterData.Resourceid);
            if (previewModel == null)
            {
                previewModel = _preview.AddCharacterModel(_selectedMob.MonsterData, model =>
                {
                    var tr = _preview.ModelContainer.Find(_selectedMob.MonsterData.Resourceid);
                    if(null != tr)
                    {
                        var target = model.GetOwnerModel().transform;
                        target.SetLocalPositionAndRotation(tr.localPosition, tr.localRotation);
                        target.localScale = tr.localScale;
                    }
                });
            }
        }
        OnRefreshUI();
    }

    void SelectGoalReward(UI_MobAlbum_GoalElement goal)
    {
        if (goal.CanGetReward)
        {
            // 보상 받기
            (List<MobAlbumInfo.RewardState> stats, List<RewardItemInfo> rewards, _) = goal.Info.GetRewardInfos();
            // TODOD 이영범 - 스탯 얻었을때 팝업
            StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
            {
                TitleText = LocalizeManager.GetText("UI_Inven_OpenResult_Title"),
                ItemIds = rewards.Select(x => (int)x.ItemID).ToList(),
                ItemCounts = rewards.Select(x => (BigInteger)x.ItemCount).ToList()
            }));
            goal.Info.ReciveReward();
            RefreshPlayer();
        }
        else
        {
            // 툴팁 출력
            ETooltipType type = goal.Goal.Stat != EStat.NONE ? ETooltipType.Stat : ETooltipType.Item;
            int rewardId;
            string descText = "";
            if (type == ETooltipType.Stat)
            {
                rewardId = goal.Goal.Stat;
                StatValueInfo info = new()
                {
                    Stat = goal.Goal.Stat,
                    Value = new BigInteger(goal.Goal.Stat_figure),
                    CalcType = goal.Goal.Calculation
                };
                descText = $"+ {info.ValueString}";
            }
            else
            {
                rewardId = goal.Goal.Itemid;
            }
            UI_ToolTip.Open(goal.GetComponent<RectTransform>(), type, rewardId, descText);
        }
    }

    void LevelUpAll()
    {
        // 레벨 업
        List<(UI_MobAlbum_MobElement mob, int before)> results = new();
        foreach (UI_MobAlbum_MobElement mob in _mobElements)
        {
            int levelBefore = mob.Album.Level;
            while (true)
            {
                int costId = mob.AlbumData.Itemid;
                BigInteger costCount = mob.Album.NextLevelCount;
                bool result = mob.Album.SetLevelUp();
                if (!result)                
                    break;
            }
            results.Add((mob, levelBefore));
        }
        UserDataManager.Save_LocalData();

        // 결과 창
        List<RecordInfo> resultInfos = new();
        foreach ((UI_MobAlbum_MobElement mob, int levelBefore) in results)
        {
            if (mob.Album.Level == levelBefore) continue;
            resultInfos.Add(new()
            {
                NameText = mob.Name,
                LevelBefore = levelBefore,
                LevelAfter = mob.Album.Level,
            });
        }
        _levelUpAllResultPopup.Show(LocalizeManager.GetText("UI_MobAlbum_Popup_Result"), resultInfos);

        MobEffect();
        OnRefreshUI();
    }

    void ShowGoStagePopup(bool show)
    {
        if (!show)
        {
            _goStagePopup.SetActive(false);
            OnRefreshUI();
            return;
        }

        // 선택된 몬스터가 나오는 스테이지 검색
        StageInfo maxStage = StageInfoManager.Instance.GetMode(EGameMode.Stage).MaxStage;
        FCharacter monster = DataManager.Instance.GetCharacterData(_selectedMob.AlbumData.Id);
        List<StageInfo> monsterStages = _normalStages.FindAll(stage => stage.MonsterInfos.Find(m => m.Characterid == monster.Id) != null);
        _lastMonsterStage = monsterStages.FindLast(stage => stage.ID <= maxStage.ID);

        // 텍스트 출력
        if (_lastMonsterStage != null)
        {
            //Preview.ClearAll();
            _goStagePopup.SetActive(true);
            _goStage_Desc.text = LocalizeManager.GetText("UI_MobAlbum_Popup_Shortcut_Desc", _selectedMob.Name);
            //string worldName = LocalizeManager.GetText(DataManager.Instance.StageWorlds[_lastMonsterStage.StageData.World].Nametextid);
            //string chapterName = LocalizeManager.GetText(_lastMonsterStage.StageData.Nametextid);
            //string stageName = LocalizeManager.GetText(_lastMonsterStage.StageData.Stagetextid);
            _goStage_StageText.text = $"[{_lastMonsterStage.GetFullName()}]";
        }
        else
        {
            UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_MobAlbum_CanNotMoveMap"));
        }
    }

    void CloseAndGoStage()
    {
        (BattleSceneManager.Instance.GetBattleScene() as StageBattleSceneController).JumpToStage(_lastMonsterStage.StageData.World, _lastMonsterStage.StageData.Chapter, _lastMonsterStage.StageData.Stage);
        OutUIAndMenuBar();
    }

    void MobEffect()
    {
        float bodySize = Mathf.Lerp(1f, _selectedMob.MonsterData.Bodysize / 10000f, 0.5f);
        Play2DEffect(id: "Fx_UI_AbilityUp_Mon_01",
            position: Vector3.zero,
            parent: _info_EffectPosition,
            scale: bodySize,
            reappearDelay: 0.2f);
    }

    void RefreshPlayer()
    {
        PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).RefreshValues(true);
    }

    void RefreshTitle()
    {
        FStageWorld worldData = DataManager.Instance.StageWorlds[_selectedCategory.Category];
        MenuBar.TitleText.text = $"{LocalizeManager.GetText(UIName.ToString())} - {LocalizeManager.GetText(worldData.Nametextid)}";
    }
}
