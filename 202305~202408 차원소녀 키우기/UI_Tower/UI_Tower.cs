using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_Tower_FloorElement;

public class UI_Tower : UIMenuBarBase, IGridViewAdapter
{
    public static bool AutoProceed;

    public enum EButtonId
    {
        None,
        FloorElement,
        RewardTooltip,
        ReturnToCurrent,
        Enter,
        AutoProceedToggle,
    }

    [Header("왼쪽")]
    [SerializeField] GridViewAdapter _gridView;
    [SerializeField] RectTransform _floorElement;
    [SerializeField] GridViewAdapter _bgGridView;
    [SerializeField] UI_Tower_BgScroll _bgGridViewAdapter;

    [Header("오른쪽")]
    [SerializeField] TMP_Text _titleText;
    [SerializeField] TMP_Text _battleInfoText;
    [SerializeField] Transform _monsterIconPrefab;
    [SerializeField] Transform _monsterIconParent;
    [SerializeField] UI_OnOffToggle _autoProceedToggle;
    [SerializeField] GameObject _bottomCurrent, _bottomAlready;

    int _selectedFloor;
    List<Transform> _monsterIcons;

    public RectTransform _gridPrefab => _floorElement;
    GameModeInfo ModeInfo => StageInfoManager.Instance.GetMode(EGameMode.DungeonTower);
    StageInfo SelectedStageInfo => ModeInfo.GetStage(_selectedFloor);
    int MaxFloor => ModeInfo.MaxStage.Level;
    int AllFloorCount => ModeInfo.LastStage.Level;

    public override void InitUI()
    {
        _monsterIcons = new();
        _monsterIconPrefab.gameObject.SetActive(false);

        // 왼쪽
        _gridView.Setting(this);
        _gridView.SetItems(AllFloorCount + 1);
        _bgGridView.Setting(_bgGridViewAdapter);
        _bgGridView.SetItems(AllFloorCount + 1);
        FloorScrollMove(MaxFloor, false);

        // 오른쪽
        SelectFloor(MaxFloor);
        SetAutoProceed(AutoProceed);

        var canvas = UIRoot.Instance.TowerBackground;
        canvas.gameObject.SetActive(true);
        canvas.worldCamera = UIRoot.Instance.UICamera;
        canvas.planeDistance = UIRoot.Instance.UICamera.farClipPlane - 1;

        base.InitUI();
    }

    public override void OutOnlyMyself(bool isDirect = false)
    {
        base.OutOnlyMyself(isDirect);
        UIRoot.Instance.TowerBackground.gameObject.SetActive(false);
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        string[] tokens = buttonInfo.ID.Split('_');
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.FloorElement:
                SelectFloor(int.Parse(tokens[1]));
                break;
            case EButtonId.RewardTooltip:
                ShowTooltip(buttonInfo.Trans, int.Parse(tokens[1]));
                break;
            case EButtonId.ReturnToCurrent:
                SelectFloor(MaxFloor);
                FloorScrollMove(_selectedFloor, true);
                break;
            case EButtonId.Enter:
                EnterTower();
                break;
            case EButtonId.AutoProceedToggle:
                SetAutoProceed(!AutoProceed);
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode) { }

    public override void OnUpdate()
    {
        base.OnUpdate();
        _bgGridView.GetComponent<Canvas>().sortingOrder = 199;
        _bgGridView.SetNormalizedPosition(_gridView.GetNormalizedPosition() * 0.2f);
    }

    public (ScrollItemInfo itemInfo, RectTransform prefab) CreateScrollItem(int index) => (null, _floorElement);

    public ScrollItemInfo UpdateScrollItem(int idx, ScrollItemInfo itemInfo, RectTransform prefab)
    {
        int floor = IndexToFloor(idx);
        StageInfo stage = ModeInfo.GetStage(floor);
        State state = floor < MaxFloor ? State.Clear : floor == MaxFloor ? State.Current : State.Locked;
        bool isLast = floor == AllFloorCount;
        prefab.GetComponent<UI_Tower_FloorElement>().Setting(stage, floor, _selectedFloor == floor, isLast, state);
        return itemInfo;
    }

    public void UpdateComplete(int newCount, int prevCount) { }

    void SelectFloor(int floor)
    {
        // 왼쪽 새로고침
        _selectedFloor = floor;
        _gridView.UpdateGrid();

        // 오른쪽 새로고침
        _titleText.text = LocalizeManager.GetText("UI_Tower_Floor", floor);
        FCharacter firstMonster = DataManager.Instance.GetCharacterData(SelectedStageInfo.MonsterInfos.FirstOrDefault().Characterid);
        _battleInfoText.text = LocalizeManager.GetText((EWeak)firstMonster.Weak switch
        {
            EWeak.Pierce => "UI_Tower_BattleInfo_Pierce",
            EWeak.Slash => "UI_Tower_BattleInfo_Slash",
            _ => "UI_Tower_BattleInfo_Smash",
        });

        // 등장 몬스터
        _monsterIcons.ForEach(x => Destroy(x.gameObject));
        _monsterIcons.Clear();
        foreach (StageMonsterData monsterData in SelectedStageInfo.MonsterInfos)
        {
            FCharacter charData = DataManager.Instance.GetCharacterData(monsterData.Characterid);
            Sprite sprite = AtlasManager.GetItemIcon(charData, EIconType.Icon);
            Transform element = Instantiate(_monsterIconPrefab, _monsterIconParent);
            Image image = element.Find("Bg/Icon").GetComponent<Image>();
            image.sprite = sprite;
            element.gameObject.SetActive(true);
            _monsterIcons.Add(element);
        }

        // 하단
        bool locked = _selectedFloor > MaxFloor;
        if (locked)
        {
            _bottomCurrent.SetActive(false);
            _bottomAlready.SetActive(false);
        }
        else
        {
            bool isCurrent = _selectedFloor == MaxFloor;
            _bottomCurrent.SetActive(isCurrent);
            _bottomAlready.SetActive(!isCurrent);
        }
    }

    void ShowTooltip(RectTransform rt, int floor)
    {
        UI_ToolTip.Open(rt, ETooltipType.Item, ModeInfo.GetStage(floor).Reward.ItemID);
    }

    void EnterTower()
    {
        BattleSceneManager.Instance.SetBattleSceneLoad(SelectedStageInfo.GameMode, SelectedStageInfo.ID, ESCENEMOVITYPE.IgnoreFullScreen, null);
    }

    void SetAutoProceed(bool autoProceed)
    {
        AutoProceed = autoProceed;
        _autoProceedToggle.SetState(AutoProceed);
    }

    void FloorScrollMove(int floor, bool lerp)
    {
        _gridView.MoveTo(FloorToIndex(floor), lerp, normalizedOffset: 0.5f);
        _bgGridView.MoveTo(FloorToIndex(floor), lerp, normalizedOffset: 0.5f);
    }

    int IndexToFloor(int index) => AllFloorCount - index;
    int FloorToIndex(int floor) => AllFloorCount - floor;
}
