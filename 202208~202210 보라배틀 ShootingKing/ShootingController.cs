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

namespace BoraBattle.Game.ShootingKing
{
	public partial class ShootingController : MonoBehaviour
	{
		public static ShootingController instance;

		public enum GameMode { HumanTarget, SurvivalTarget, AnimalTarget, Tutorial };
		public enum GameState { Start, Playing, End };

		[Header("Game Data")]
		public List<MapIdData> mapIdDatas;
		public List<GameModeData> gameModeDatas;

		[Header("Scene World References")]
		public GameObject playerObject;
		public Camera playerCamera, bulletCamera, sightCamera;
		public Transform bulletCameraStartPoint, bulletCameraTargetDeltaPoint;
		public Animator playerAnimator, manAnimator;
		public Transform scopePoint, riflePoint;
		public ShootEffect shootEffect;
		public Canvas zoomTimeCanvas;
		public Image zoomTimeFillGauge;
		public GameObject tutorialAimSphere, tutorialAimCenter;

		[Header("Scene Canvas References")]
		public GameObject inputBlocker;
		public GameObject readyLabel, goLabel;
		public TMP_Text remainedTimeText, scoreText, bulletCountText;
		public Animator missPopup, scorePopup;
		public TMP_Text scorePopupScoreText, accuracyPopupText1, accuracyPopupText2;
		public Animator windIndicator;
		public Transform windIndicatorArrow;
		public TMP_Text windIndicatorText;
		public Animator timerAlert;
		public TMP_Text timerAlertText;
		public GameObject pausePanel;
		public Image pauseBGMButton, pauseSFXButton;
		public Sprite pauseBGMButtonOffSprite, pauseSFXButtonOffSprite;
		public GameObject exitConfirmPanel;
		public GameObject tutorialPanel;
		public List<Image> tutorialPanelImages;
		public List<CanvasGroup> tutorialPanelOverlays;
		public ShootingResultPanel resultPanel;
		public GameObject topArea;
		public List<CanvasGroup> tutorialOverlays;
		public GameObject tutorialOverlaySkipButton;

		[Header("Asset References")]
		public GameObject bulletPrefab;
		public List<GameObject> bulletHolePrefabs;
		public GameObject bulletHitEffect;
		public GameObject readyEffect;
		public AudioClip sfxGunShoot, sfxGunShoulder, sfxGunHeartbeat, sfxGameOver, sfxGameStart, sfxGameFinishConfirm, sfxTargetMiss, sfxTargetWood, sfxTargetSteel, sfxButton, sfxResultScoring;

		[Header("Variables")]
		public AnimationCurve aimDragCurve;

		GameState gameState;
		bool isPaused;
		Well512Random randomProvider;
		List<FixedRandomData> fixedRandomDatas;
		ObscuredInt randomWindIndex, randomSurvivalTargetIndex;
		ObscuredUInt mapId;
		MapIdData mapData;
		GameModeData gameModeData;
		ShootingStage currentStage;
		ObscuredFloat remainedTime;
		ObscuredInt remainedBulletCount;
		Score score;
		int scoreInView;
		Vector2 pressTouchPosition, touchPosition;
		bool aimingStartTrigger, aimCancelTrigger, fireRifleTrigger, resetWindTrigger, resetTargetTrigger;
		[NonSerialized] public ShootingTargetPiece bulletCollidedObject;
		[NonSerialized] public Vector3 bulletCollidedPoint;
		[NonSerialized] public Vector3 windVector;
		List<Animator> survivalTargets;
		Animator survivalActiveTarget;
		bool alert60Seconds, alert30Seconds, alert10Seconds;
		bool resultPanelNext;
		bool userExitDemand;
		bool isTutorial, tutorialAimDone, tutorialSkip;
		int tutorialPanelIndex;
		Animator rifleAnimator => gameModeData.rifle.GetComponentInChildren<Animator>();
		bool tweenAnimating => gameState == GameState.Playing && flyBulletCoroutine == null;

		const string noneAnimation = "None";
		const string singleAnimation = "Animation";
		const float animationTime = 0.7f;
		const int bulletCount = 10;

		Coroutine gameCoroutine;
		Coroutine playerAimStartCoroutine;
		Coroutine playerAimingCoroutine;
		Coroutine zoomTimeCoroutine;
		Coroutine flyBulletCoroutine;
		Coroutine highlightTargetCoroutine;
		Coroutine resultPanelCoroutine;

#if MobirixTest
		public static uint mapIdTest;
#endif

		void Awake()
		{
			instance = this;
			fixedRandomDatas = new List<FixedRandomData>();
		}

		public IEnumerator StartGame(uint _mapId)
		{
			// 게임 세팅
			mapId = _mapId;
#if MobirixTest
			mapId = mapIdTest;
#endif

			randomProvider = new Well512Random(mapId);
			isTutorial = !PlayerPrefs.HasKey("ShootingFirstPlay");
			InitializeGame();

			// 튜토리얼
			if (isTutorial)
			{
				gameState = GameState.Playing;

				PlayerPrefs.SetInt("ShootingFirstPlay", 1);
				gameCoroutine = StartCoroutine(GameCoroutine());
				yield return new WaitUntil(() => tutorialSkip || userExitDemand);
				CheckAndStopCoroutine(ref gameCoroutine);
				if (userExitDemand)
				{
					yield break;
				}
				isTutorial = false;
				InitializeGame();
			}

			int windMinInt = (int)(mapData.windMin * 10f);
			int windMaxInt = (int)(mapData.windMax * 10f);
			for (int i = 0; i < bulletCount; i++)
			{
				fixedRandomDatas.Add(new()
				{
					windDirection = randomProvider.Next(0, 360),
					windSpeed = randomProvider.Next(windMinInt, windMaxInt) / 10f,
					survivalTargetIndex = (int)randomProvider.Next(0, survivalTargets.Count),
				});
			}

			gameState = GameState.Start;

			// 게임 시작 연출
			PlaySFX(sfxGameStart);
			playerAnimator.speed = 1f;
			playerAnimator.Play("CameraStartMove");
			yield return new WaitForSeconds(0.4f);
			readyLabel.SetActive(true);
			Animator readyLabelAnimator = readyLabel.GetComponent<Animator>();
			readyLabelAnimator.Play(singleAnimation);
			yield return new WaitUntil(() => AnimationDone(readyLabelAnimator));
			readyLabel.SetActive(false);
			goLabel.SetActive(true);
			Animator goLabelAnimator = goLabel.GetComponent<Animator>();
			goLabelAnimator.Play(singleAnimation);
			yield return new WaitUntil(() => AnimationDone(goLabelAnimator));
			goLabel.SetActive(false);
			playerAnimator.speed = 2f;

			gameState = GameState.Playing;

			if (gameModeData.gameMode == GameMode.SurvivalTarget)
			{
				resetTargetTrigger = true;
				StartCoroutine(SurvivalStageCoroutine());
			}

			// 게임 진행
			gameCoroutine = StartCoroutine(GameCoroutine());
			yield return new WaitUntil(() => gameCoroutine == null || userExitDemand);
			CheckAndStopCoroutine(ref gameCoroutine);
			PlayBGM(null);
			if (remainedTime <= 0f)
			{
				yield return StartCoroutine(TimerAlert("ShootingKing.InGame.TimesUp"));
				yield return new WaitForSeconds(2f);
			}
			score.timeBonus = remainedTime > 0f && remainedBulletCount == 0 ? (int)(remainedTime * 130f) : 0;

			gameState = GameState.End;
		}

		IEnumerator GameCoroutine()
		{
			resetWindTrigger = true;
			while (remainedTime > 0f && remainedBulletCount > 0)
			{
				if (mapData.gameMode == GameMode.HumanTarget && mapData.stageIndex == 0 && remainedBulletCount == 5)
				{
					currentStage.GetComponentInChildren<DOTweenAnimation>().DOPlay();
				}

				// 바람 방향 설정
				if (resetWindTrigger && !isTutorial)
				{
					resetWindTrigger = false;
					float windDirection = fixedRandomDatas[randomWindIndex].windDirection;
					float windDirectionRadian = windDirection * Mathf.Deg2Rad;
					float windSpeed = fixedRandomDatas[randomWindIndex].windSpeed;
					randomWindIndex++;
					windVector = new Vector3(Mathf.Cos(windDirectionRadian), Mathf.Sin(windDirectionRadian)) * windSpeed * 0.15f;
					windIndicatorArrow.eulerAngles = new Vector3(0f, 0f, windDirection);
					windIndicatorText.text = windSpeed.ToString("0.0");
					windIndicator.Play(singleAnimation, 0, 0f);
				}

				// 사격 영역 터치시 조준 시작
				if (isTutorial) SetTutorialOverlay(0);
				inputBlocker.SetActive(false);
				aimingStartTrigger = false;
				yield return new WaitUntil(() => aimingStartTrigger);
				aimingStartTrigger = false;

				if (isTutorial) SetTutorialOverlay(1);
				playerAimStartCoroutine = StartCoroutine(PlayerAimStart());
				playerAimingCoroutine = StartCoroutine(PlayerAiming());
				zoomTimeCoroutine = StartCoroutine(ZoomTimeProcess());

				aimCancelTrigger = false;
				fireRifleTrigger = false;
				AudioSource heartbeatSound = PlaySFX(sfxGunHeartbeat);
				if (isTutorial)
				{
					yield return new WaitUntil(() => tutorialAimDone);
					aimCancelTrigger = false;
					fireRifleTrigger = false;
					SetTutorialOverlay(2);
					yield return new WaitUntil(() => fireRifleTrigger);
				}
				else
				{
					yield return new WaitUntil(() => aimCancelTrigger || fireRifleTrigger || zoomTimeCoroutine == null);
				}
				inputBlocker.SetActive(true);
				CheckAndStopCoroutine(ref playerAimingCoroutine);
				PlayerAimStop();
				CheckAndStopCoroutine(ref zoomTimeCoroutine);
				ZoomCancel();
				if (heartbeatSound) heartbeatSound.Stop();

				// 조준 돌입 중 터치 뗄 시 조준 취소
				if (aimCancelTrigger)
				{
					aimCancelTrigger = false;
					continue;
				}

				// 조준 중 터치 뗄 시 발사
				fireRifleTrigger = false;
				SetRemainedBulletCount(remainedBulletCount - 1);
				StartCoroutine(shootEffect.ShowRifleShootEffect());
				flyBulletCoroutine = StartCoroutine(FlyBullet());
				yield return flyBulletCoroutine;
			}
			if (isTutorial) SetTutorialOverlay(3);
			gameCoroutine = null;
		}

		public IEnumerator ResultPanelCoroutine()
		{
			PlaySFX(sfxGameOver);
			SetPanel(resultPanel.gameObject);
			resultPanelCoroutine = StartCoroutine(ResultPanelAnimation());
			yield return new WaitUntil(() => resultPanelNext || resultPanelCoroutine == null);
			resultPanelNext = false;
			CheckAndStopCoroutine(ref resultPanelCoroutine);
			ResultPanelAnimationComplete();
			resultPanel.submitScoreButtonGauge.DOFillAmount(1f, 3f).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() => resultPanelNext = true);
			yield return new WaitUntil(() => resultPanelNext);
		}

		void InitializeGame()
		{
			inputBlocker.SetActive(true);
			readyLabel.SetActive(false);
			goLabel.SetActive(false);
			zoomTimeCanvas.gameObject.SetActive(false);
			missPopup.gameObject.SetActive(false);
			scorePopup.gameObject.SetActive(false);
			resultPanel.submitScoreButtonGauge.fillAmount = 0f;

			foreach (GameModeData gameData in gameModeDatas)
			{
				gameData.map.SetActive(false);
				gameData.stages.ForEach(x => x.gameObject.SetActive(false));
				gameData.rifle.SetActive(false);
				gameData.scope.SetActive(false);
			}
			if (isTutorial)
			{
				mapData = mapIdDatas.Find(x => x.gameMode == GameMode.Tutorial);
				gameModeData = gameModeDatas.Find(x => x.gameMode == GameMode.Tutorial);
			}
			else
			{
				mapData = mapIdDatas.Find(x => mapId >= x.from && mapId <= x.to);
				mapData ??= mapIdDatas[0];
				gameModeData = gameModeDatas.Find(x => mapData.gameMode == x.gameMode);
			}
			currentStage = gameModeData.stages[mapData.stageIndex];
			gameModeData.map.SetActive(true);
			currentStage.gameObject.SetActive(true);
			survivalTargets = currentStage.targets.Select(x => x.GetComponent<Animator>()).ToList();
			gameModeData.rifle.SetActive(true);
			gameModeData.scope.SetActive(true);
			RenderSettings.skybox = gameModeData.skybox;
			DynamicGI.UpdateEnvironment();
			PlayBGM(gameModeData.bgm);
			manAnimator.speed = 2f;
			rifleAnimator.speed = 2f;
			playerAnimator.speed = 2f;
			shootEffect.transform.SetParent(gameModeData.rifle.transform);
			SetRemainedTime(isTutorial ? float.MaxValue : mapData.gameTime);
			SetRemainedBulletCount(isTutorial ? 1 : bulletCount);
			topArea.SetActive(!isTutorial);
			score = new Score();
			UpdateScoreText(false);
			SetPanel(null);
			SetTutorialOverlay(-1);
			CheckAndStopCoroutine(ref playerAimStartCoroutine);
			CheckAndStopCoroutine(ref playerAimingCoroutine);
			CheckAndStopCoroutine(ref zoomTimeCoroutine);
			CheckAndStopCoroutine(ref flyBulletCoroutine);
		}

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

		void Update()
		{
			if (tweenAnimating)
			{
				DOTween.ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);
			}
			switch (gameState)
			{
				case GameState.Playing:
					SetRemainedTime(remainedTime - Time.deltaTime);
					if (remainedTime < 0f)
					{
						aimingStartTrigger = true;
						PlayerFireDemand();
					}
					break;
			}
        }

        void OnApplicationFocus(bool focus)
        {
            if (!focus && gameState == GameState.Playing) PauseButton();
        }

		void PlayBGM(AudioClip bgm) => GameInterface.Interface.Sound.PlayBGM(bgm);
		public AudioSource PlaySFX(AudioClip sfx) => GameInterface.Interface.Sound.PlaySFX(sfx);
		bool BGMMuted => GameInterface.Interface.Sound.BgmVolume == 0f;
		bool SFXMuted => GameInterface.Interface.Sound.SfxVolume == 0f;
		string GetLocalizedText(string textKey) => GameInterface.Interface.Data.GetText(textKey);

		bool AnimationDone(Animator animator) => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;
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
	}
}
