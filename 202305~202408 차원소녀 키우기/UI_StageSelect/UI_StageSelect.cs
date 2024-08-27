using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = System.Random;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using System.Collections;

public class UI_StageSelect : UIBase//, IGridViewAdapter
{
    enum EViewType { World, Stage }
    EViewType _currentView;

    public enum EButtonId
    {
        None,
        Close,
        World,
        Stage_Back,
        Stage_Go,
        Stage_ToCurrent,
        Stage_ToMax,
    }

    [Header("상단 메뉴")]
    [SerializeField] TMP_Text _titleText;
    [SerializeField] UI_Button /*_stageViewButton, */_worldViewButton, _goButton;
    [SerializeField] GameObject _worldView, _stageView;

    [Header("월드")]
    [SerializeField] ScrollRect _world_Scroll;
    [SerializeField] Transform _world_Content;
    [SerializeField] UI_WorldElement _world_Element;

    [Header("스테이지")]
    [SerializeField] TMP_Text _stage_TitleText;
    [SerializeField] ScrollRect _stage_ChapterScroll;
    [SerializeField] UI_ChapterElement _stage_ChapterElement;
    [SerializeField] Transform _stage_ChapterParent;
    [SerializeField] Image _stage_StageBg;
    [SerializeField] Transform _stage_BgLeftPoint, _stage_BgRightPoint;
    [SerializeField] ScrollRect _stage_StagePathScroll;
    [SerializeField] UI_StagePathElement _stage_StagePathElement;
    [SerializeField] Transform _stage_StagePathParent;
    [SerializeField] Image _stage_StagePathLine;
    [SerializeField] Transform _stage_StagePathLineParent;
    [SerializeField] Color _stage_LineActiveColor, _stage_LineInactiveColor;
    [SerializeField] ItemIcon _stage_RewardIconPrefab;
    [SerializeField] Transform _stage_RewardIconParent;

    List<FStageWorld> _allWorlds;
    int _selectedWorld, _selectedChapter;
    StageInfo _selectedStage;
    StageInfo _currentStage, _maxStage;
    List<UI_WorldElement> _worldElements;
    List<UI_ChapterElement> _chapterElements;
    List<UI_StagePathElement> _stagePathElements;
    List<GameObject> _stagePathLines;
    List<ItemIcon> _rewardIcons;
    bool _hideCharacterFlag;

    float _canvasScale => transform.localScale.x;
    StageData _selectedStageData => _selectedStage.StageData;
    StageData _currentStageData => _currentStage.StageData;
    StageData _maxStageData => _maxStage.StageData;

    public override void InitUI()
    {
        _worldElements = new();
        _chapterElements = new();
        _stagePathElements = new();
        _stagePathLines = new();
        _rewardIcons = new();
        _stage_StagePathLine.gameObject.SetActive(false);
        RefreshData();
        _selectedWorld = _currentStageData.World;
        _selectedChapter = _currentStageData.Chapter;
        _selectedStage = _currentStage;
        _allWorlds = DataManager.Instance.StageWorlds.Values.ToList();

        // 월드 스크롤뷰 초기화
        foreach (FStageWorld world in _allWorlds)
        {
            UI_WorldElement element = Instantiate(_world_Element, _world_Content);
            bool isCurrent = world.World == _currentStageData.World;
            bool complete = world.World < _maxStageData.World;
            bool canEnter = world.World <= _maxStageData.World;
            element.Setting(world.World, isCurrent, complete, canEnter);
            _worldElements.Add(element);
        }

        // 씬 변경 이벤트 등록
        UIManager.Instance.RegisterEvent_SceneChange(gameObject, () =>
        {
            RefreshData();
            switch (_currentView)
            {
                case EViewType.World:
                    WorldView(false);
                    break;
                case EViewType.Stage:
                    if (IsCurrentChapter(_selectedStageData))
                    {
                        StageView(false, false, null);
                    }
                    break;
            }
        });

        // 스테이지 선택창으로 이동
        StageView(true, true, null);
        base.InitUI();
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode) { }

    public override bool OnBackKey()
    {
        switch (_currentView)
        {
            case EViewType.World:
                StageView(true, true, _selectedStage);
                return false;
        }
        return base.OnBackKey();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        switch (_currentView)
        {
            case EViewType.Stage:
                // 스테이지 배경 스크롤
                RectTransform bgRect = _stage_StageBg.GetComponent<RectTransform>();
                float moveLength = bgRect.sizeDelta.x - (_stage_BgRightPoint.localPosition.x - _stage_BgLeftPoint.localPosition.x);
                float x = -_stage_StagePathScroll.horizontalNormalizedPosition * moveLength;
                bgRect.localPosition = new(x, bgRect.localPosition.y);
                break;
        }
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click)
            return;
        
        // 버튼 선택
        string[] tokens = buttonInfo.ID.Split("__");
        EButtonId buttonType = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonType)
        {
            case EButtonId.Close:
                if (_currentView == EViewType.Stage)
                {
                    OutUI();
                }
                else
                {
                    StageView(true, true, _selectedStage);
                }
                break;
            case EButtonId.World:
                if (_worldElements.Find(x => x.Tweener.IsPlaying)) return;
                int world = int.Parse(tokens[1]);
                if (world > _maxStageData.World) return;
                _selectedWorld = world;
                UIPlayTween tweener = _worldElements.Find(x => x.WorldNumber == _selectedWorld).Tweener;
                tweener.onFinished.Clear();
                tweener.Play();
                tweener.onFinished.Add(new EventDelegate(() =>
                {
                    StageView(true, true, GetWorldFirstStage());
                }));
                break;
            case EButtonId.Stage_Back:
                if (_currentView == EViewType.Stage)
                {
                    WorldView(true);
                }
                break;
            case EButtonId.Stage_Go:
                (BattleSceneManager.Instance.GetBattleScene() as StageBattleSceneController).JumpToStage(_selectedStageData.World, _selectedStageData.Chapter, _selectedStageData.Stage);
                OutUI();
                break;
            case EButtonId.Stage_ToCurrent:
                StageView(false, true, null);
                break;
            case EButtonId.Stage_ToMax:
                StageView(false, true, _maxStage);
                break;
        }

        // 챕터, 스테이지 아이템 선택
        if (buttonInfo.Trans.TryGetComponent(out UI_ChapterElement chapterElement))
        {
            if (_selectedChapter == chapterElement.ChapterNumber || chapterElement.ChapterNumber == -1) return;
            _selectedChapter = chapterElement.ChapterNumber;
            _selectedStage = GetChapterFirstStage();
            if (!IsCurrentChapter(_selectedStageData))
            {
                _hideCharacterFlag = true;
            }
            CreateStageItems();
            RefreshStageScrollItems();
        }
        else if (buttonInfo.Trans.TryGetComponent(out UI_StagePathElement stagePathElement))
        {
            if (_selectedStage == stagePathElement.Info) return;
            _selectedStage = stagePathElement.Info;
            RefreshStageScrollItems();
            stagePathElement.TweenerOnSelected.PlayForward();
        }
    }

    void WorldView(bool animate)
    {
        _currentView = EViewType.World;
        _titleText.text = LocalizeManager.GetText("UI_Stage_World");
        _worldViewButton.gameObject.SetActive(false);
        _goButton.gameObject.SetActive(false);
        _worldView.SetActive(true);
        _stageView.SetActive(false);
        if (animate)
        {
            PlayTween(_worldView);
            Canvas.ForceUpdateCanvases();
            UI_WorldElement focusElement = _worldElements.Find(x => x.WorldNumber == _selectedWorld);
            _world_Scroll.FocusOnItem(focusElement.GetComponent<RectTransform>());
        }
    }

    void StageView(bool tween, bool scroll, StageInfo focusStage)
    {
        _currentView = EViewType.Stage;
        _worldViewButton.gameObject.SetActive(true);
        _goButton.gameObject.SetActive(true);
        _worldView.SetActive(false);
        _stageView.SetActive(true);

        // 선택된 챕터, 스테이지 초기화
        if (focusStage != null)
        {
            _selectedWorld = focusStage.StageData.World;
            _selectedChapter = focusStage.StageData.Chapter;
            _selectedStage = focusStage;
        }
        else
        {
            _selectedWorld = _currentStageData.World;
            _selectedChapter = _currentStageData.Chapter;
            _selectedStage = _currentStage;
        }

        // 챕터 스크롤뷰 아이템 생성
        FStageWorld worldData = DataManager.Instance.StageWorlds[_selectedStageData.World];
        _titleText.text = LocalizeManager.GetText(worldData.Nametextid);
        _stage_StageBg.sprite = AtlasManager.GetSprite(EATLAS_TYPE.Low_Illustration, worldData.Bg_thumbnail);
        foreach (UI_ChapterElement element in _chapterElements)
        {
            element.transform.SetParent(null);
            Destroy(element.gameObject);
        }
        _chapterElements.Clear();
        List<StageInfo> chapters = StageInfoManager.Instance.GetNormalStage().NormalStageMap[_selectedWorld].Select(x => x.Value.First().Value).ToList();
        foreach (StageInfo chapterData in chapters)
        {
            UI_ChapterElement chapterElement = Instantiate(_stage_ChapterElement, _stage_ChapterParent);
            _chapterElements.Add(chapterElement);
            chapterElement.Info = chapterData.StageData;
        }
        CreateStageItems();
        RefreshStageScrollItems();

        if (tween)
        {
            PlayTween(_stageView);
        }
        if (scroll)
        {
            ScrollStageView(true);
        }
    }

    void RefreshData()
    {
        _currentStage = StageInfoManager.Instance.GetNormalStage().CurrentStage;
        _maxStage = StageInfoManager.Instance.GetNormalStage().MaxStage;
    }

    void CreateStageItems()
    {
        // 스테이지 스크롤뷰 아이템 생성
        foreach (UI_StagePathElement element in _stagePathElements)
        {
            element.transform.SetParent(null);
            Destroy(element.gameObject);
        }
        _stagePathElements.Clear();
        var stages = StageInfoManager.Instance.GetNormalStage().NormalStageMap[_selectedWorld][_selectedChapter].Select(x => x.Value);
        foreach (StageInfo stage in stages)
        {
            UI_StagePathElement stageElement = Instantiate(_stage_StagePathElement, _stage_StagePathParent);
            _stagePathElements.Add(stageElement);
            stageElement.Info = stage;
        }

        // 스테이지 Y 포지션 랜덤 설정
        Random random = new(_selectedWorld * 1000 + _selectedChapter);
        bool upDirection = random.Next(2) == 1;
        foreach (UI_StagePathElement element in _stagePathElements)
        {
            int r = random.Next(-20, 20);
            Vector3 dir = upDirection ? Vector3.up : Vector3.down;
            element.MovePivot.GetComponent<RectTransform>().anchoredPosition = dir * r;
            upDirection = !upDirection;
        }

        // 스테이지간 잇는 점선 설정
        _stagePathLines.ForEach(x => Destroy(x));
        _stagePathLines.Clear();
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < _stagePathElements.Count - 1; i++)
        {
            UI_StagePathElement current = _stagePathElements[i];
            UI_StagePathElement next = _stagePathElements[i + 1];
            Image newLine = Instantiate(_stage_StagePathLine, _stage_StagePathLineParent);
            _stagePathLines.Add(newLine.gameObject);
            newLine.gameObject.SetActive(true);
            Vector3 vector = (next.MovePivot.position - current.MovePivot.position) / _canvasScale;
            RectTransform rt = newLine.GetComponent<RectTransform>();
            rt.position = current.MovePivot.position;
            rt.sizeDelta = new(vector.magnitude, rt.sizeDelta.y);
            float zRotation = Mathf.Atan2(vector.normalized.y, vector.normalized.x) * Mathf.Rad2Deg;
            rt.eulerAngles = Vector3.forward * zRotation;
            newLine.color = next.Info.IsSmallerThan(_maxStage, true) ? _stage_LineActiveColor : _stage_LineInactiveColor;
        }

        PlayTween(_stage_StagePathParent.gameObject);
        if (IsCurrentChapter(_selectedStageData))
        {
            _selectedStage = _currentStage;
        }
        ScrollStageView(false);
    }

    void RefreshStageScrollItems()
    {
        // 챕터 스크롤뷰 아이템 세팅
        foreach (UI_ChapterElement element in _chapterElements)
        {
            bool selected = element.Info?.Chapter == _selectedChapter;
            element.Setting(selected);
        }

        // 스테이지 스크롤뷰 아이템 세팅
        foreach (UI_StagePathElement element in _stagePathElements)
        {
            bool canEnter = element.Info.IsSmallerThan(_maxStage, true);
            bool isCurrent = element.Info.IsSameStage(_currentStage);
            bool isMax = element.Info.IsSameStage(_maxStage);
            bool selected = element.Info.IsSameStage(_selectedStage);
            element.Setting(canEnter, isCurrent, isMax, selected, _hideCharacterFlag);
        }
        _hideCharacterFlag = false;

        // 기대 보상
        _rewardIcons.ForEach(x => Destroy(x.gameObject));
        _rewardIcons.Clear();
        List<(int itemId, BigInteger count)> rewards = _selectedStage.GetRewardList();
        List<StageMonsterData> monsters = DataManager.Instance.GetStageMonsterInfoData(_selectedStage.StageData.Monstergroupid);
        List<int> monsterRewardBoxIds = monsters.Select(x => (int)x.Dropitemgroup).Distinct().ToList();
        foreach (int boxId in monsterRewardBoxIds)
        {
            BoxItem box = InventoryManager.Instance.GetItem(boxId) as BoxItem;
            box.GetRating().ForEach(p => rewards.Add((p.ItemID, (int)p.MaxCount)));
        }
        foreach ((int id, BigInteger count) in rewards)
        {
            ItemIcon icon = Instantiate(_stage_RewardIconPrefab, _stage_RewardIconParent);
            icon.Setting(id, count);
            _rewardIcons.Add(icon);
        }

        // 상단 텍스트, 이동 버튼
        UI_StagePathElement selectedStage = _stagePathElements.Find(x => x.Info == _selectedStage);
        _goButton.interactable = selectedStage.CanEnter && selectedStage.Info != _currentStage;
        StageInfo chapterData = GetChapterFirstStage();
        _stage_TitleText.text = $"{LocalizeManager.GetText(chapterData.StageData.Nametextid)} {chapterData.StageData.Chapter}";
    }

    void ScrollStageView(bool chapterScroll)
    {
        Canvas.ForceUpdateCanvases();
        if (chapterScroll)
        {
            int focusChapter = _chapterElements.FindIndex(x => x.ChapterNumber == _selectedChapter);
            focusChapter = Mathf.Clamp(focusChapter + 1, 0, _chapterElements.Count - 1);
            UI_ChapterElement focusChapterItem = _chapterElements[focusChapter];
            _stage_ChapterScroll.FocusOnItem(focusChapterItem.GetComponent<RectTransform>());
        }
        int focusStage = _stagePathElements.FindIndex(x => x.Info == _selectedStage);
        focusStage = Mathf.Clamp(focusStage + 1, 0, _stagePathElements.Count - 1);
        UI_StagePathElement focusItem = _stagePathElements[focusStage];
        _stage_StagePathScroll.FocusOnItem(focusItem.GetComponent<RectTransform>());
    }

    void PlayTween(GameObject viewPanel)
    {
        Canvas.ForceUpdateCanvases();
        viewPanel.GetComponent<CanvasGroup>().alpha = 0f;
        viewPanel.GetComponent<UIPlayTween>().Play();
    }

    StageInfo GetWorldFirstStage() => StageInfoManager.Instance.GetNormalStage().NormalStageMap[_selectedWorld].First().Value.First().Value;
    StageInfo GetChapterFirstStage() => StageInfoManager.Instance.GetNormalStage().NormalStageMap[_selectedWorld][_selectedChapter].First().Value;
    bool IsCurrentChapter(StageData stage) => stage.World == _currentStageData.World && stage.Chapter == _currentStageData.Chapter;
}
