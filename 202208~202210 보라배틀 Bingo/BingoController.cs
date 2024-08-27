using BoraBattle.Game.Interface;
using CodeStage.AntiCheat.ObscuredTypes;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	// 변수 선언, 핵심 로직
	public partial class BingoController : MonoBehaviour
	{
		public static BingoController instance;
#if MobirixTest
		public static uint gameSeedDevTest;
#endif

		public enum GameMode { Card1 = 0, Card2, Card3 }
		public enum DaubScoreType { Perfect, Great, Nice, Good, Oops, RandomBallPerfect, None }

		[Header("Scene References")]
		public Transform instanceParent;
		public List<BingoBoard> bingoBoards;
		public Image timerAlertBoardBackground;
		public Image scoreIcon;
		public GameObject bingoBallParent, numberSummary;
		public TMP_Text remainedTimeText, scoreText;
		public List<Transform> bingoBallPositions;
		public TMP_Text deltaScoreText, deltaTimerText;
		public Button bingoButton;
		public Image bingoButtonActiveImage;
		public GameObject bingoButtonAnimation;
		public List<BingoNumberSummaryCell> numberSummaryCells;
		public Image itemFillGaugeImage, itemFillGaugeImage2;
		public GameObject itemFillGaugeFullText;
		public Transform itemCreatedPosition, itemParent;
		public List<Transform> itemPositions;
		public CanvasGroup itemTopPopup, itemTopLabel;
		public Image itemTopPopupIcon;
		public TMP_Text itemTopDesc, itemTopTimer;
		public GameObject itemFullScreenPopup, itemDoubleScoreEffect;
		public BingoBall itemTopBingoBall;
		public TMP_Text itemFullScreenTimer;
		public List<BingoBall> itemFullScreenBingoBalls;
		public TMP_Text countdownText;
		public CanvasGroup positiveTopAlert, negativeTopAlert, countdownTopAlert, itemTopAlert;
		public Transform card1Buttons, card2Buttons, card3Buttons;
		public GameObject inputBlockerMainArea, inputBlockerCardButtonArea;
		public BingoResultPanel resultPanel;
		//public List<Animator> resultPanelBingoBalls;
		public GameObject pausePanel;
		public Animator pause_BgmButton, pause_SfxButton;
		public GameObject exitConfirmPanel;
		public BingoTutorialPanel tutorialPanel;
		public List<SeedToGameModeData> seedDatas;
		public Transform bingoBoardOriginLayer, bingoBoardTopPopupLayer;
        public Transform cardButtonsArea;
		public Image backBg;
        public Transform numberBoxProceed;

        [Header("Asset References")]
		public BingoBall bingoBallPrefab;
		public List<BingoItem> itemPrefabs;
		public GameObject topLabelPrefab;
		public AudioClip bgm, sfxBingoTile, sfxChangeCard, sfxFinishGame, sfxMarking, sfxTimeNotice, sfxButton, sfxGetItem, sfxOops;
		public ParticleSystem markEffectPrefab, bingoEffectPrefab, getItemEffectPrefab;

		[Header("Variables")]
		public Color remainedTimeRedColor;
		public Color colorB, colorI, colorN, colorG, colorO;

		GameMode gameMode;
		ObscuredUInt gameSeed;
		ObscuredFloat remainedTime;
		List<BingoBall> appearedBingoBalls;
		List<ObscuredInt> remainedNumbers;
		HashSet<ObscuredInt> appearedNumbers;
		Score score;
		bool numberSummaryOpened;
		ObscuredFloat itemFillGauge;
		List<BingoItem> itemList;
		ObscuredInt itemListIndex;
		List<BingoItem> obtainedItems;
		List<BingoItem> usingItems;
		ObscuredFloat doubleScoreTimer;
		bool remainedTime10Seconds;
		int currentBoardIndex;
		bool alert30SecondsShown, alert10SecondsShown, alert5SecondsShown;
		bool resultPanelNext;
		Transform cardButtonsParent;
		List<BingoCardButton> cardButtons;
		bool isPaused;
		bool userExitDemand;
		Well512Random randomProvider;

		int BoardCount => gameMode switch
		{
			GameMode.Card1 => 1,
			GameMode.Card2 => 2,
			GameMode.Card3 => 3,
			_ => 1
		};
		public ObscuredFloat BingoBallDuration => gameMode switch
		{
			GameMode.Card1 => 4f - appearingDuration,
			GameMode.Card2 => 5f - appearingDuration,
			GameMode.Card3 => 5f - appearingDuration,
			_ => 5f - appearingDuration
		};
		BingoBall CurrentBingoBall => appearedBingoBalls[0];
		BingoBall FinalBingoBall => appearedBingoBalls[^1];
		public BingoBoard CurrentBoard => bingoBoards[currentBoardIndex];
		BingoCardButton CurrentCardButton => cardButtons[currentBoardIndex];
		bool ItemFull => obtainedItems.Count == 3;

		[NonSerialized] public ObscuredFloat appearingDuration = 0.7f;
		ObscuredFloat animationTime = 0.7f;
		ObscuredInt maxBingoCount = 13;
		public const string singleAnimation = "Animation";

		Coroutine bingoBallCoroutine;
		List<Coroutine> bingoAnimationCoroutines;
		List<Coroutine> blackoutAnimationCoroutines;
		Coroutine doubleScoreCoroutine;
		Coroutine resultPanelAnimationCoroutine;

		void Awake()
		{
			instance = this;
			bingoDataComparer = new BingoDataComparer();
			bingoNumberBoxComparer = new BingoNumberBoxComparer();
			appearedBingoBalls = new List<BingoBall>();
			remainedNumbers = new List<ObscuredInt>();
			appearedNumbers = new HashSet<ObscuredInt>();
			itemList = new List<BingoItem>();
			obtainedItems = new List<BingoItem>();
			usingItems = new List<BingoItem>();
			score = new Score();
			bingoAnimationCoroutines = new List<Coroutine>();
			blackoutAnimationCoroutines = new List<Coroutine>();
        }

		public IEnumerator StartGame(uint _gameSeed)
		{
			// 시드 데이터 세팅
			gameSeed = _gameSeed;
#if MobirixTest
			gameSeed = gameSeedDevTest;
#endif
			SeedToGameModeData seedData = seedDatas.Find(x => gameSeed >= x.from && gameSeed <= x.to);
			if (seedData == null)
			{
				seedData = new SeedToGameModeData
				{
					gameMode = GameMode.Card3,
					usingItemTypes = new List<BingoItem.ItemType>()
				};
				foreach (BingoItem.ItemType itemType in typeof(BingoItem.ItemType).GetEnumValues())
				{
					seedData.usingItemTypes.Add(itemType);
				}
			}
			gameMode = seedData.gameMode;

			// 오브젝트 초기화
            randomProvider = new Well512Random(gameSeed);
            scoreText.text = "0";
            SetRemainedTime(120f);
            positiveTopAlert.gameObject.SetActive(false);
            negativeTopAlert.gameObject.SetActive(false);
            itemTopAlert.gameObject.SetActive(false);
            countdownTopAlert.gameObject.SetActive(false);
            bingoBallParent.SetActive(true);
            numberSummary.SetActive(false);
            timerAlertBoardBackground.gameObject.SetActive(false);
			itemTopPopup.gameObject.SetActive(false);
            itemFullScreenPopup.SetActive(false);
            itemDoubleScoreEffect.SetActive(false);
            bingoButtonActiveImage.gameObject.SetActive(false);
            bingoButtonAnimation.SetActive(false);
            resultPanel.gameObject.SetActive(false);
            pausePanel.SetActive(false);
            exitConfirmPanel.SetActive(false);
            tutorialPanel.gameObject.SetActive(false);
            inputBlockerMainArea.SetActive(true);
            inputBlockerCardButtonArea.SetActive(false);
			itemFillGaugeImage.fillAmount = 0f;
            itemFillGaugeImage2.fillAmount = 0f;
            numberBoxProceed.gameObject.SetActive(false);

            // 카드버튼 세팅
            card1Buttons.gameObject.SetActive(false);
            card2Buttons.gameObject.SetActive(false);
            card3Buttons.gameObject.SetActive(false);
            switch (gameMode)
            {
                case GameMode.Card1:
                    cardButtonsParent = card1Buttons;
                    break;
                case GameMode.Card2:
                    cardButtonsParent = card2Buttons;
                    card2Buttons.gameObject.SetActive(true);
                    break;
                case GameMode.Card3:
                    cardButtonsParent = card3Buttons;
                    card3Buttons.gameObject.SetActive(true);
                    break;
            }
            cardButtons = new();
            foreach (Transform tr in cardButtonsParent)
            {
                cardButtons.Add(tr.GetComponent<BingoCardButton>());
            }
            ShowItemTopPopup(false, true);

			// 나올 숫자 셔플
            for (int i = 1; i <= 75; i++)
			{
				remainedNumbers.Add(i);
				numberSummaryCells[i - 1].SetNumber(i);
			}
			remainedNumbers.ShuffleList(randomProvider);

			// 빙고 보드 세팅
			for (int boardIndex = 0; boardIndex < BoardCount; boardIndex++)
			{
				BingoBoard bingoBoard = bingoBoards[boardIndex];
				bingoBoard.availableBingos = new List<BingoData>();
				bingoBoard.achievedBingos = new List<BingoData>();
				bingoBoard.board = new BingoNumberBox[5, 5];
				for (int x = 0; x < 5; x++)
				{
					List<ObscuredInt> numberBoxNumbers = new List<ObscuredInt>();
					for (int i = 1; i <= 15; i++)
					{
						numberBoxNumbers.Add(x * 15 + i);
					}
					numberBoxNumbers.ShuffleList(randomProvider);

					for (int y = 0; y < 5; y++)
					{
						BingoNumberBox numberBox = bingoBoard.boardLinear[y * 5 + x];
						bingoBoard.board[x, y] = numberBox;
						numberBox.boardIndex = boardIndex;
						numberBox.boardCoord = new Vector2Int(x, y);
						numberBox.GetComponent<Button>().onClick.AddListener(() => NumberBoxButton(numberBox));
						numberBox.SetState(BingoNumberBox.State.NumberUnmarked);
						numberBox.UpdateSprite();
						numberBox.SetNumber(numberBoxNumbers[y]);
					}
				}
				bingoBoard.board[2, 2].SetState(BingoNumberBox.State.Bingo1);
				bingoBoard.board[2, 2].UpdateSprite();
			}
            for (int i = 0; i < bingoBoards.Count; i++)
			{
				bingoBoards[i].bg.gameObject.SetActive(i < BoardCount);
				bingoBoards[i].bg.color = Color.white.WithAlpha(0f);
            }

			// 4연속 꽝인 경우 방지
			List<int> allBoardNumbersDistinct = new List<int>();
			foreach (BingoBoard board in GetAllBoard())
			{
				allBoardNumbersDistinct.AddRange(board.boardLinear.Select(x => x.number));
			}
			allBoardNumbersDistinct = allBoardNumbersDistinct.Distinct().ToList();
			List<ObscuredInt> remainedNumberInBoardReverse = new(remainedNumbers);
			remainedNumberInBoardReverse.Reverse();
			remainedNumberInBoardReverse = remainedNumberInBoardReverse.FindAll(x => CurrentBoard.boardLinear.Find((numberBox) => numberBox.number == x));
			int remainedNumberInBoardReverseIndex = 0;
			int notAppearedCount = 0;
			for (int i = 0; i < 30; i++)
			{
				ObscuredInt remainedNumber = remainedNumbers[i];
				if (!allBoardNumbersDistinct.Contains(remainedNumber))
				{
					notAppearedCount++;
					if (notAppearedCount >= 4)
					{
						ObscuredInt numberInBoard = remainedNumberInBoardReverse[remainedNumberInBoardReverseIndex];
						remainedNumberInBoardReverseIndex++;
						int numberInBoardIndex = remainedNumbers.IndexOf(numberInBoard);
						remainedNumbers[i] = numberInBoard;
						remainedNumbers[numberInBoardIndex] = remainedNumber;
						notAppearedCount = 0;
					}
				}
				else
				{
					notAppearedCount = 0;
				}
			}

			// 나올 아이템 세팅
			foreach (BingoItem.ItemType itemType in seedData.usingItemTypes)
			{
				itemList.Add(itemPrefabs.Find(x => x.itemType == itemType));
			}
			itemList.ShuffleList(randomProvider);

			CardChange(0);
			PlayBGM();

			// 첫 실행 튜토리얼
			int firstPlay = PlayerPrefs.GetInt("BingoFirstPlay", 0);
			if (firstPlay == 0)
			{
				PlayerPrefs.SetInt("BingoFirstPlay", 1);
				isPaused = true;
				ShowTutorial(true);
				yield return new WaitUntil(() => !tutorialPanel.gameObject.activeSelf);
				isPaused = false;
			}

			// 게임 시작
			yield return StartCoroutine(GameCoroutine());
		}

		IEnumerator GameCoroutine()
		{
			// 카운트다운
			for (int i = 3; i >= 1; i--)
			{
				TMP_Text newCountdownText = Instantiate(countdownText.gameObject, instanceParent, true).GetComponent<TMP_Text>();
				newCountdownText.text = i.ToString();
				yield return StartCoroutine(CountdownAnimation(newCountdownText));
			}

			// 위 BingoBall에 시간이 흐름 또는 새로운 BingoBall 생성
			inputBlockerMainArea.SetActive(false);
			inputBlockerCardButtonArea.SetActive(false);
			bingoBallCoroutine = StartCoroutine(BingoBallCoroutine());
			while (!IsGameEnd())
			{
				if (!usingItems.Find(x => x.IsBlockingMain()))
				{
					SetRemainedTime(remainedTime - GetDeltaTime());
					if (CurrentBingoBall.appearingTime > 0f)
					{
						CurrentBingoBall.appearingTime -= GetDeltaTime();
					}
					else if (CurrentBingoBall.remainedTime > 0f)
					{
						CurrentBingoBall.SetRemainedTime(CurrentBingoBall.remainedTime - GetDeltaTime());
					}
					DOTween.ManualUpdate(GetDeltaTime(), Time.unscaledDeltaTime);
				}
				if (userExitDemand)
				{
					yield break;
				}
				yield return null;
			}
			inputBlockerMainArea.SetActive(true);
			inputBlockerCardButtonArea.SetActive(true);
			timerAlertBoardBackground.gameObject.SetActive(false);
			CurrentBingoBall.timerFillImage.fillAmount = 0f;

			// 타임오버로 게임 끝
			if (remainedTime < 0f)
			{
				yield return StartCoroutine(TopAlertAnimation(countdownTopAlert, GetLocalizedText("Bingo.InGame.GameOver")));
			}
			// 블랙아웃으로 게임 끝
			else if (GetAllBoard().All(x => x.blackout))
			{
				score.remainedTime = (int)remainedTime;
				int blackoutBonus = (int)(remainedTime * remainedTimeScore);
				ApplyDoubleScoreAndAnimation(blackoutBonus, false);
				score.blackoutBonus += blackoutBonus;
			}

			if (bingoBallCoroutine != null) StopCoroutine(bingoBallCoroutine);
			yield return new WaitUntil(() => bingoAnimationCoroutines.Count == 0);
			yield return new WaitUntil(() => blackoutAnimationCoroutines.Count == 0);
			yield return new WaitForSeconds(1.2f);
		}

		bool IsGameEnd() => remainedTime < 0f || GetAllBoard().All(x => x.blackout);

		public IEnumerator ResultPanelCoroutine()
		{
			inputBlockerMainArea.SetActive(false);
			inputBlockerCardButtonArea.SetActive(false);
			resultPanel.gameObject.SetActive(true);
			resultPanel.resultButtonFillImage.fillAmount = 0f;
			resultPanelAnimationCoroutine = StartCoroutine(ResultPanelAnimation());
			yield return new WaitUntil(() => resultPanelAnimationCoroutine == null || resultPanelNext);
			if (resultPanelAnimationCoroutine != null) 
			{
				StopCoroutine(resultPanelAnimationCoroutine);
			}
			ResultPanelAnimationComplete();
			resultPanelNext = false;
			resultPanel.resultButtonFillImage.DOFillAmount(1f, 5f).SetEase(Ease.Linear).OnComplete(() => resultPanelNext = true);
			resultPanel.submitButtonText.text = GetLocalizedText("Bingo.Result.SubmitScoreBtn");
			//StartCoroutine(BingoBallsAnimation());
			yield return new WaitUntil(() => resultPanelNext);
		}

		IEnumerator BingoBallCoroutine()
		{
			CreateAndMoveBingoBalls();
			while (remainedNumbers.Count > 0)
			{
				if (CurrentBingoBall.Expired && remainedTime > 0.5f)
				{
					CreateAndMoveBingoBalls();
				}
				yield return null;
			}
			bingoBallCoroutine = null;
		}

		public string GetLocalizedText(string textKey) => GameInterface.Interface.Data.GetText(textKey);

		public (GameResult.ResultType resultType, float resultScore) GetResult()
		{
			if (userExitDemand)
			{
				return (GameResult.ResultType.UserExit, 0);
			}
			else
			{
				return (GameResult.ResultType.Success, score.TotalScore);
			}
		}

		void PlayBGM() => GameInterface.Interface.Sound.PlayBGM(bgm);
		void PlaySFX(AudioClip sfx) => GameInterface.Interface.Sound.PlaySFX(sfx);
		bool BGMMuted => GameInterface.Interface.Sound.BgmVolume == 0f;
		bool SFXMuted => GameInterface.Interface.Sound.SfxVolume == 0f;

		float GetDeltaTime() => !isPaused ? Time.deltaTime : 0f;

		void Update()
		{
#if MobirixTest
			UpdateDevTest();
#endif
		}

        void OnApplicationFocus(bool focus)
        {
			if (!focus) PauseButton();
        }
	}
}
