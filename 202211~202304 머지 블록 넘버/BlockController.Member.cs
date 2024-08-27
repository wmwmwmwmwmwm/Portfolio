using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using BInteger = System.Numerics.BigInteger;

public partial class BlockController
{
	public enum GameMode { Endless, Stage };
	public enum GameState { None, Drop, Merge, Pause };
	public enum GameEndType { Playing, StageClear, StageFail };

	public static bool startBlock512PurchaseDemand;

	[Header("Scene References")]
	public Canvas mainCanvas;
	public Transform topArea;
	public RectTransform adArea;
	public RectTransform safeArea;
	public RectTransform bottomArea;
	public GameObject endlessModeUI, stageModeUI;
	public GameObject nextBlockPoint;
	public TMP_Text stageText;
	public Transform blockParent;
	public Transform connectorParent;
	public Transform columnParent;
	public Transform boardParticleBackLayer, boardParticleFrontLayer, escapeBlockDisappearLayer;
	public CellVerticalGroup cellGroup, cellGroupFrontLayer;
	public Image boardInputArea;
	public List<MissionUI> missionUIs;
	public Image skillGaugeImage, skillGaugeGrayImage;
	public Transform skillButtonPoint;
	public Button skillAutoButton;
	public TMP_Text skillAuto_Text;
	public ItemButton itemButtonDivide, itemButtonJoker, itemButtonHammer;
	public Image warningImage;
	public List<RectTransform> boardRects;
	public GameObject inputBlocker;
	public GameObject itemCancelBar;
	public TMP_Text itemCancel_Text;

	public GameObject stageClearPanel, stageFail1Panel, stageFail2Panel, pausePanel;
	public GameObject endlessFail1Panel, endlessFail2Panel;
	public GameObject touchToStartPanel;
	public GameObject endlessModeStartPanel;
	public GameObject stageAllClearPanel;
	public TMP_Text endlessModeText, endlessModeScoreText, endlessModeHighscoreText;
	public TMP_Text endlessFail1_Text, endlessFail1_ContinueCoin, endlessFail2_HighScoreText;
	public Transform endlessFail2_BlockPosition, endlessFail2_ScoreTextStartPosition, endlessFail2_ScoreTextEndPosition;
	public GameObject endlessFail2_NewScoreEffect1, endlessFail2_NewScoreEffect2, endlessFail2_NewScoreEffect3;
	public ElementUI_Ranking endlessFail2_MyRanking;
	public RectTransform endlessFail2_RetryButton;
	public Button pause_SkillAutoToggle, pause_BgmToggle, pause_SfxToggle;
	public Image pause_SkillAutoCheckmark, pause_BgmImage, pause_SfxImage;
	public List<MissionCheckIcon> stageClear_MissionIcons;
	public GameObject stageClear_Coin;
	public TMP_Text stageClear_CoinText;
	public List<MissionCheckIcon> stageFail1PanelMissionIcons, stageFail2PanelMissionIcons;
	public TMP_Text stageFail1ContinueCoin;
	public List<MissionUI> stageInfo_MissionUIs;
	public GameObject itemPurchasePanel;
	public Image itemPurchasePanelIcon;
	public TMP_Text itemPurchasePanelItemTitle, itemPurchasePanelCoinText, itemPurchasePanelDescription, itemPurchasePanelPriceText;
	public StartBoosterItemUI startBlock512ItemUI;
	public Image endlessStart_SlowBlockIcon, endlessStart_SkillBoostIcon;
	public GameObject highestNumberPopup;
	public CanvasGroup highestNumber_Parent;
	public Transform highestNumber_BlockPosition;
	public TMP_Text highestNumber_CoinText;
	public GameObject stageInfoPanel;
	public TMP_Text stageInfo_StageNumberText;
	public CanvasGroup stageInfo_BuffButton;
	public Image stageInfo_SlowBlockIcon, stageInfo_SkillBoostIcon;

	[Header("Effect Prefabs")]
	public GameObject comboEffectPrefab;
	public GameObject trumpetEffectPrefab;
	public GameObject dropEffectPrefab, mergeEffectPrefab, destroyBlockEffectPrefab;
	public GameObject destroyIceEffectPrefab, destroyIceBlockEffect1Prefab, destroyIceBlockEffect2Prefab, destroyDummyEffectPrefab, destroyConnectorEffectPrefab;
	public GameObject missionCountEffectPrefab, missionCountDestroyEffectPrefab;
	public GameObject dummySetHpEffectPrefab;
	public GameObject skillGaugeEffectPrefab, skillGaugeFullEffectPrefab;
	public GameObject copyStartEffectPrefab, copyEndEffectPrefab, allMergeEffectPrefab, targetBlockEffectPrefab;
	public GameObject escapeBlockEffectPrefab;
	public GameObject skillEffectPrefab, skillEndEffectPrefab;
	public GameObject itemDivideEffectPrefab, itemJokerEffectPrefab, itemHammerEffectPrefab;
	public GameObject coinEffectPrefab;

	[Header("Asset References")]
	public Column columnPrefab;
	public List<Cell> cellPrefabs;
	public GameObject cellFrontLayerPrefab;
	public Connector connectorPrefab;
	public Sprite cellArrowSprite;
	public Sprite pause_SoundMuteSprite;
	public Image buffIconSlowDrop, buffIconSkillGauge;
	public Material comboMat1, comboMat2, comboMat3;

	[Header("Variables")]
	public bool isMapToolMode;
	public Color endlessStartPanelButtonCheckColor, endlessStartPanelButtonUncheckColor;
	public List<EndlessSpawnData> endlessSpawnDatas4x7, endlessSpawnDatas6x7;
	public float blockAnimationSpeed;
	public Ease blockMoveEase;
	public AnimationCurve skillEffectEase;
	public float blockMergeDelay;

	public class MergeInfo
	{
		public List<Block> allBlocks;
		public List<Block> otherBlocks;
		public Block destinationBlock;
	}

	[Serializable]
	public class EndlessSpawnData
	{
		public NumberType maxNumberType;
		public int minDifference, maxDifference;

		public NumberType GetMinNumberType(NumberType numberType) => (NumberType)((int)numberType - minDifference);
		public NumberType GetMaxNumberType(NumberType numberType) => (NumberType)((int)numberType - maxDifference);
	}

	Camera mainCamera;
	[HideInInspector] public Stage stageData;
	[HideInInspector] public Vector2 gridCellSize;
	[HideInInspector] public List<Column> columns;
	[HideInInspector] public List<Cell> boardCellsLinear;
	[HideInInspector] public List<GameObject> boardCellsFrontLayerLinear;	// 특수 셀 표시(ex: 지정 위치 블록 완성, 탈출 블록 존재시 하단 탈출 위치 표시)를 위한 앞부분 오브젝트 리스트.
	[HideInInspector] public Cell[,] boardCells;
	[HideInInspector] public List<Block> boardBlocks;
	[HideInInspector] public List<Connector> connectors;
	[HideInInspector] public List<Block> uiBlocks;
	[HideInInspector] public BInteger score;
	Block currentDropBlock;
	RaycastHit2D[] currentBlockRaycastHits;
	bool fireBlockTrigger;
	bool pointerDownThisBlock;
	int newBlockXCoord, pointerXCoord;
	bool isDragging;
	[HideInInspector] public int blockTurn;
	[HideInInspector] public bool checkBoardNeed;
	[HideInInspector] public List<(Block block, Block iceBlock)> iceBlocksToDestroyPair;
	[HideInInspector] public List<Block> escapeBlocksToDestroy;
	int newBlockId;
	List<(BlockData blockData, float chance)> blockSpawnPool;
	BlockData nextBlockData;
	Block nextBlockUI;
	CancellationTokenSource gameCancelToken;
	GameState gameState;
	UniTask mainGameTask;
	GameEndType gameEnd;
	ItemType? usingItemType;
	bool touchToStartTrigger;
	bool blockRecreateTrigger;
	float skillGauge;
	int combo;
	bool userMovePause;
	Stage autoSavedStage;
	bool isAutoSavedStage;
	int spawnBlockCount;
	CancellationTokenSource stageClearToken;
	bool stageClearExit, stageClearNext;
	ItemType itemPurchasePanelItemType;
	CancellationTokenSource endlessFail2Token;
	CancellationTokenSource endlessFail2LoadRankingToken;
	bool endlessFail2PanelAnimating;
	JToken endlessFail2MyRank;
	NumberType highestNumberType;
	bool highestNumberPopupNext;
	bool isNewStage;
	int endlessCoinAccumulated;
	bool nextBlockEnabled, slowDropSpeedEnabled, increaseSkillGaugeEnabled;
	GameObject skillGaugeFullEffect;
	int missionEffectCount;
	bool itemCancelTrigger;
	[HideInInspector] public Block selectedBlock;
	bool bIsCompleteSaveLeaderboard;
	bool isNewHighScore;
	bool bOnCreateItemProcess;
	GameObject adObj;
	Vector2 originBoardSize;
    int nContinue;
    int width, height, time;
	bool isInRanking;

    readonly Vector2 blockSpriteSize = new(114f, 114f);
	readonly float blockPadding = 1f;
	readonly float blockDropStickTime = 1f;
	readonly float topCellSpace = 15f;
	readonly List<Vector2Int> fourDirections = new() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
	readonly int startBlock512Price = 100, nextBlockPrice = 100, continuePrice = 100;
	readonly int newHighestNumberCoinAmount = 20;

	GameMode gameMode => stageData.stageNumber == 0 ? GameMode.Endless : GameMode.Stage;
	public Vector2Int boardSize => stageData.boardSize;
	public float boardScaleMultiplier => gridCellSize.x / blockSpriteSize.x;
	public Vector2 blockSize => gridCellSize - Vector2.one * blockPadding * 2f;
	int stageNumber => stageData.stageNumber;
	List<Mission> missions => stageData.missions;
	float deltaTime => !userMovePause ? Time.deltaTime : 0f;
	float fixedDeltaTime => !userMovePause ? Time.fixedDeltaTime : 0f;
	int stageClearCoinAmount => isNewStage ? 10 : 0;

#if DEV_TEST
	float stageStartTime;
#endif
}
