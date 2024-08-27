using BoraBattle.Game.Interface;
using CodeStage.AntiCheat.ObscuredTypes;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoraBattle.Game.WorldBasketballKing
{
	public partial class BasketballController : MonoBehaviour
	{
		public static BasketballController instance;

		[Header("Game Data")]
		public List<SeedData> seedDatas;

		[Header("Scene World References")]
		public Camera mainCamera;
		public Transform inputBottomPoint;
		public List<BasketballBall> allBalls;
		public Collider middleCollider, groundCollider;

		[Header("Scene Canvas References")]
		public Animator startTexts;
		public TMP_Text remainedTimeText, scoreText, roundText;
		public Animator niceText, cleanText, boosterText, comboText;
		public GameObject niceScoreText, cleanScoreText;
		public Image boosterFillGauge;
		public Image timeoutImage;
		public GameObject pausePanel, exitConfirmPanel, tutorialPanel;
		public GameObject resultPanel;
		public BasketballResultRecord resultBasicScore, resultComboScore, resultTotalScore;
		public Image resultPanelFillImage;
		public GameObject tutorialPrevButton, tutorialNextButton;
		public Image tutorialImage;
		public List<GameObject> tutorialPages;
		public Transform effectParent;
		public Image whiteOverlay;
		public GameObject inputBlocker;
		public CanvasGroup timerAlert;
		public TMP_Text timerAlertText;

		[Header("Asset References")]
		public Sprite timeout2Sprite;
		public AudioClip sfxButton, sfxNice, sfxClean1, sfxClean2, sfxRim, sfxBounce, sfxGo, sfxTimeout, sfxTimerAlert, sfxResult, sfxShoot;
		public List<Sprite> tutorialSprites;

		[Header("Variables")]
		public Color boosterFillColor;

		Well512Random randomProvider;
		BasketballBall draggingBall;
		SeedData currentSeedData;
		BasketballStage currentStage => currentSeedData.stage;
		int round;
		[NonSerialized] public bool isPlaying;
		Score score;
		ObscuredFloat remainedTime;
		ObscuredFloat boosterGauge;
		Vector3[] lastPointerPositions;
		float lastPointerUpdateTime;
		ObscuredFloat boosterTime;
		bool booster => boosterTime > 0f;
		bool userExitDemand, resultPanelNext;
		int tutorialPanelIndex;
		int lastTimerSeconds;
		bool timerAlert30Seconds, timerAlert10Seconds;
		[NonSerialized] public int combo;

		Coroutine boosterCoroutine;
		Coroutine resultPanelCoroutine;

		const int lastPointerPositionsSize = 10;
		const string noneAnimation = "None";
		const string singleAnimation = "Animation";

#if MobirixTest
		public static uint seedTest;
#endif

		void Awake()
		{
			instance = this;
			lastPointerPositions = new Vector3[lastPointerPositionsSize];
			float relativeScreenRatio = ((float)Screen.height / Screen.width) / (16f / 9f);
			relativeScreenRatio = Mathf.Max(1f, relativeScreenRatio);
			mainCamera.fieldOfView *= relativeScreenRatio;

			seedDatas.ForEach(x => x.stage.gameObject.SetActive(false));
			startTexts.gameObject.SetActive(false);
			pausePanel.SetActive(false);
			exitConfirmPanel.SetActive(false);
			tutorialPanel.SetActive(false);
			resultPanel.SetActive(false);
			inputBlocker.SetActive(true);
			niceText.gameObject.SetActive(false);
			cleanText.gameObject.SetActive(false);
			boosterText.gameObject.SetActive(false);
			comboText.gameObject.SetActive(false);
			niceScoreText.SetActive(false);
			cleanScoreText.SetActive(false);
			timerAlert.gameObject.SetActive(false);
			resultPanelFillImage.fillAmount = 0f;
		}

		public IEnumerator StartGame(uint seed)
		{
#if MobirixTest
			seed = seedTest;
#endif

			// 게임 세팅
			randomProvider = new Well512Random(seed);
			currentSeedData = seedDatas.Find(x => seed >= x.from && seed <= x.to);
			currentSeedData ??= seedDatas[0];
			PlayBGM(currentSeedData.bgm);
			currentStage.gameObject.SetActive(true);
			currentStage.Initialize();
			_rim = currentStage.rim;
			_rimCenter = currentStage.rimCenter;
			_moveKind = currentStage.moveKind;
			SetRemainedTime(60f);
			score = new Score();
			AddScore(0);
			SetBoosterGauge(0f);

			// 첫 실행 튜토리얼
			int firstPlay = PlayerPrefs.GetInt("BasketballFirstPlay", 0);
			if (firstPlay == 0)
			{
				PlayerPrefs.SetInt("BasketballFirstPlay", 1);
				tutorialPanel.SetActive(true);
				SetTutorialPage(0);
				yield return new WaitUntil(() => !tutorialPanel.activeSelf);
			}

			// 게임 시작 연출
			StartCoroutine(PlayAnimator(startTexts));
			yield return new WaitForSeconds(2f);
			PlaySFX(sfxGo);
			currentStage.startBlockPad.DOBlendableRotateBy(new Vector3(-90f, 0f, 0f), 1.5f).OnComplete(() => currentStage.startBlockPad.gameObject.SetActive(false));
			isPlaying = true;
			inputBlocker.SetActive(false);
			while (remainedTime > 0f && !userExitDemand)
			{
				SetRemainedTime(remainedTime - Time.deltaTime);
				yield return null;
			}
			OnBallPointerUp(new PointerEventData(null));
			isPlaying = false;
			inputBlocker.SetActive(true);
			if (!userExitDemand)
			{
				yield return StartCoroutine(TimeoutImageCoroutine());
			}
		}

		void Update()
		{
			RimUpdate();
		}

		public (GameResult.ResultType resultType, float resultScore) GetResult()
		{
			if (userExitDemand)
			{
				return (GameResult.ResultType.UserExit, 0);
			}
			else
			{
				return (GameResult.ResultType.Success, score.totalScore);
			}
		}

        void OnApplicationFocus(bool focus)
        {
			if (!focus) PauseButton();
        }

		void PlayBGM(AudioClip bgm) => GameInterface.Interface.Sound.PlayBGM(bgm);
		public AudioSource PlaySFX(AudioClip sfx) => GameInterface.Interface.Sound.PlaySFX(sfx);
		string GetLocalizedText(string textKey) => GameInterface.Interface.Data.GetText(textKey);
		bool CheckAndStopCoroutine(ref Coroutine coroutine)
		{
			if (coroutine != null)
			{
				StopCoroutine(coroutine);
				coroutine = null;
				return true;
			}
			return false;
		}
		bool AnimationDone(Animator animator)
		{
			if (!animator.isActiveAndEnabled) return true;
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			return stateInfo.IsName(noneAnimation) || stateInfo.normalizedTime > 0.99f;
		}
		IEnumerator PlayAnimator(Animator animator)
		{
			animator.gameObject.SetActive(true);
			animator.Play(singleAnimation, 0, 0f);
			yield return null;
			yield return new WaitUntil(() => AnimationDone(animator));
			animator.gameObject.SetActive(false);
		}
	}
}