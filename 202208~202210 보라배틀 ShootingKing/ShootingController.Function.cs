using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

namespace BoraBattle.Game.ShootingKing
{
	public partial class ShootingController
	{
		IEnumerator PlayerAimStart()
		{
			PlaySFX(sfxGunShoulder);
			manAnimator.Play(singleAnimation);
			rifleAnimator.Play(singleAnimation);
			playerAnimator.Play("Aiming");
			yield return null;
			yield return new WaitUntil(() => AnimationDone(playerAnimator));
			playerAimStartCoroutine = null;
		}

		IEnumerator PlayerAiming()
		{
			while (true)
			{
				// 조준점 이동
				Vector2 aimDelta = touchPosition - pressTouchPosition;
				aimDelta *= aimDragCurve.Evaluate(aimDelta.magnitude) * Time.deltaTime;
				playerObject.transform.Rotate(-aimDelta.y, aimDelta.x, 0f);

				// 조준 영역 제한
				Vector3 playerRotation = playerObject.transform.eulerAngles;
				playerRotation.x = ClampAngle(playerRotation.x, 25f);
				playerRotation.y = ClampAngle(playerRotation.y, 20f);
				playerRotation.z = 0f;
				playerObject.transform.eulerAngles = playerRotation;

				if (isTutorial)
				{
					if (!tutorialAimDone)
					{
						tutorialAimDone = Physics.Raycast(sightCamera.transform.position, sightCamera.transform.forward);
					}
					if (tutorialAimDone)
					{
						Vector3 playerLookPosition = tutorialAimCenter.transform.position - sightCamera.transform.localPosition;
						playerObject.transform.LookAt(playerLookPosition);
						CheckAndStopCoroutine(ref playerAimStartCoroutine);
					}
				}
				yield return null;
			}

			float ClampAngle(float f, float max)
			{
				if (f < 360f - max && f > 180f) f = 360f - max;
				else if (f > max && f < 180f) f = max;
				return f;
			}
		}

		IEnumerator ZoomTimeProcess()
		{
			yield return playerAimStartCoroutine;

			GameObject newReadyEffect = Instantiate(readyEffect, playerCamera.transform);
			newReadyEffect.transform.localPosition = new Vector3(0f, -1.5f, 1f);
			newReadyEffect.GetComponent<ParticleSystem>().Play(true);
			zoomTimeCanvas.gameObject.SetActive(true);
			float currentZoomTime = mapData.zoomTime;
			float zoomTime = mapData.zoomTime;
			while (currentZoomTime > 0f)
			{
				currentZoomTime -= Time.deltaTime;
				zoomTimeFillGauge.fillAmount = currentZoomTime / zoomTime;
				yield return null;
			}
			ZoomCancel();
		}

		void PlayerAimStop()
		{
			manAnimator.Play(noneAnimation);
			rifleAnimator.Play(noneAnimation);
			playerAnimator.Play(noneAnimation);
		}

		void ZoomCancel()
		{
			zoomTimeCanvas.gameObject.SetActive(false);
			zoomTimeCoroutine = null;
		}

		IEnumerator FlyBullet()
		{
			playerCamera.gameObject.SetActive(false);
			bulletCamera.gameObject.SetActive(true);
			GameObject bullet = Instantiate(bulletPrefab);
			bullet.transform.position = sightCamera.transform.position + sightCamera.transform.forward;
			bullet.transform.forward = sightCamera.transform.forward;
			Vector3 cameraDistanceVectorOriginal = bullet.transform.position - bulletCamera.transform.position;
			float elapsedTime = 0f;
			while (!bulletCollidedObject && elapsedTime < 8f)
			{
				elapsedTime += Time.deltaTime;
				Vector3 cameraDistanceVector = bullet.transform.position - bulletCamera.transform.position;
				bulletCamera.transform.position = Vector3.SmoothDamp(bulletCamera.transform.position, bullet.transform.position - cameraDistanceVector, ref cameraDistanceVector, 1.2f);
				bulletCamera.transform.LookAt(bullet.transform);
				yield return null;
			}

			// 카메라 줌인, 총알 히트 이펙트
			if (bulletCollidedObject && !bulletCollidedObject.noBulletHole)
			{
				highlightTargetCoroutine = StartCoroutine(currentStage.HighlightTarget(bulletCollidedObject));
				float cameraShakeTimer = 0.1f;
				bulletCamera.transform.DOMove(bulletCollidedPoint + bulletCameraTargetDeltaPoint.position, animationTime * 0.7f).SetEase(Ease.OutCubic).OnUpdate(() =>
				{
					if (cameraShakeTimer > 0f)
					{
						cameraShakeTimer -= Time.deltaTime;
						bulletCamera.transform.localPosition += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
					}
				});
				bulletCamera.transform.DORotate(bulletCameraTargetDeltaPoint.eulerAngles, animationTime).SetEase(Ease.OutCubic);
				GameObject newBulletHole = Instantiate(bulletHolePrefabs.PickOne(), bulletCollidedObject.transform, true);
				newBulletHole.transform.rotation = playerCamera.transform.rotation;
				newBulletHole.transform.position = bulletCollidedPoint - newBulletHole.transform.forward * 0.01f;
				GameObject newBulletHitEffect = Instantiate(bulletHitEffect, bulletCollidedPoint - bullet.transform.forward * 0.3f, Quaternion.Euler(0f, 180f, 0f));
			}
			Destroy(bullet);

			// 점수 책정
			if (bulletCollidedObject && bulletCollidedObject.score > 0)
			{
				PlaySFX(gameModeData.gameMode switch
				{
					GameMode.HumanTarget => sfxTargetWood,
					GameMode.SurvivalTarget => sfxTargetSteel,
					GameMode.AnimalTarget => sfxTargetWood,
					_ => sfxTargetWood,
				});
				int pointScore = bulletCollidedObject.score * 100;
				score.pointScore += pointScore;
				scorePopup.gameObject.SetActive(true);
				scorePopupScoreText.text = pointScore.ToString();
				scorePopup.Play(singleAnimation);
				float accuracy = currentStage.GetAccuracy(bulletCollidedPoint);
				int accuracyBonus = (int)(pointScore * Mathf.Pow(accuracy, 2f));
				if (bulletCollidedObject.applyAccuracyBonus)
				{
					string accuracyText1 = "";
					if (accuracy < 0.33f)
					{
						accuracyText1 = GetLocalizedText("ShootingKing.InGame.AccuracyDesc2");
						score.goodCount++;
						score.goodScore += accuracyBonus;
					}
					else if (accuracy < 0.66f)
					{
						accuracyText1 = GetLocalizedText("ShootingKing.InGame.AccuracyDesc1");
						score.niceCount++;
						score.niceScore += accuracyBonus;
					}
					else
					{
						accuracyText1 = GetLocalizedText("ShootingKing.InGame.AccuracyDesc0");
						score.perfectCount++;
						score.perfectScore += accuracyBonus;
					}
					accuracyPopupText1.text = accuracyText1;
					accuracyPopupText2.text = string.Format(GetLocalizedText("ShootingKing.InGame.Accuracy"), accuracy.ToPercentString());
				}
				accuracyPopupText1.gameObject.SetActive(bulletCollidedObject.applyAccuracyBonus);
				accuracyPopupText2.gameObject.SetActive(bulletCollidedObject.applyAccuracyBonus);
				yield return new WaitUntil(() => AnimationDone(scorePopup));
				scorePopup.gameObject.SetActive(false);
				yield return new WaitForSeconds(0.2f);
			}
			else
			{
				PlaySFX(sfxTargetMiss);
				missPopup.gameObject.SetActive(true);
				missPopup.Play(singleAnimation);
				yield return new WaitUntil(() => AnimationDone(missPopup));
				missPopup.gameObject.SetActive(false);
			}

			if (CheckAndStopCoroutine(ref highlightTargetCoroutine))
			{
				currentStage.ResetHighlight();
			}

			UpdateScoreText(true);
			bulletCollidedObject = null;
			playerCamera.gameObject.SetActive(true);
			bulletCamera.gameObject.SetActive(false);
			bulletCamera.transform.SetPositionAndRotation(bulletCameraStartPoint.position, bulletCameraStartPoint.rotation);

			resetWindTrigger = true;
			if (gameModeData.gameMode == GameMode.SurvivalTarget)
			{
				resetTargetTrigger = true;
			}
			flyBulletCoroutine = null;
		}

		void SetRemainedTime(float newRemainedTime)
		{
			remainedTime = newRemainedTime;
			int minute = Mathf.Max(0, (int)(remainedTime / 60f));
			float second = Mathf.Max(0f, remainedTime % 60f);
			remainedTimeText.text = string.Format("{0}:{1}", minute, second.ToString("00.00"));

			if (!alert60Seconds && remainedTime < 60f)
			{
				alert60Seconds = true;
				StartCoroutine(TimerAlert("ShootingKing.InGame.TimeNotice60"));
			}
			else if (!alert30Seconds && remainedTime < 30f)
			{
				alert30Seconds = true;
				StartCoroutine(TimerAlert("ShootingKing.InGame.TimeNotice30"));
			}
			else if (!alert10Seconds && remainedTime < 10f)
			{
				alert10Seconds = true;
				StartCoroutine(TimerAlert("ShootingKing.InGame.TimeNotice10"));
			}
		}

		IEnumerator TimerAlert(string textKey)
		{
			timerAlertText.text = GetLocalizedText(textKey);
			timerAlert.Play(singleAnimation, 0, 0f);
			yield return new WaitUntil(() => AnimationDone(timerAlert));
		}

		void UpdateScoreText(bool animation)
		{
			int totalScore = score.TotalScore;
			scoreText.DOKill();
			if (animation)
			{
				scoreText.DOCounter(scoreInView, totalScore, animationTime);
			}
			else
			{
				scoreText.text = totalScore.ToString("N0");
			}
			scoreInView = totalScore;
		}

		void SetRemainedBulletCount(int newCount)
		{
			remainedBulletCount = newCount;
			bulletCountText.text = remainedBulletCount.ToString();
		}

		void PlayerFireDemand()
		{
			if (playerAimStartCoroutine != null)
			{
				aimCancelTrigger = true;
			}
			else if (playerAimingCoroutine != null)
			{
				fireRifleTrigger = true;
			}
		}

		IEnumerator SurvivalStageCoroutine()
		{
			while (true)
			{
				yield return new WaitUntil(() => resetTargetTrigger);
				resetTargetTrigger = false;
				if (survivalActiveTarget)
				{
					survivalActiveTarget.Play("Close");
				}
				if (randomSurvivalTargetIndex == fixedRandomDatas.Count)
				{
					yield break;
				}
				int targetIndex = fixedRandomDatas[randomSurvivalTargetIndex].survivalTargetIndex;
				randomSurvivalTargetIndex++;
				yield return null;
				Animator newTarget = survivalTargets[targetIndex];
				newTarget.Play("Open");
				survivalActiveTarget = newTarget;
			}
		}

		IEnumerator ResultPanelAnimation()
		{
			InitializeRecord(resultPanel.pointScoreRecord, resultPanel.pointScoreText, resultPanel.pointScoreSubject, "ShootingKing.Result.Score");
			InitializeRecord(resultPanel.perfectShotRecord, resultPanel.perfectShotText, resultPanel.perfectShotSubject, "ShootingKing.Result.Perfect", score.perfectCount.ToString());
			InitializeRecord(resultPanel.niceShotRecord, resultPanel.niceShotText, resultPanel.niceShotSubject, "ShootingKing.Result.Nice", score.niceCount.ToString());
			InitializeRecord(resultPanel.goodShotRecord, resultPanel.goodShotText, resultPanel.goodShotSubject, "ShootingKing.Result.Good", score.goodCount.ToString());
			InitializeRecord(resultPanel.timeBonusRecord, resultPanel.timeBonusText, resultPanel.timeBonusSubject, "ShootingKing.Result.TimeBonus");
			InitializeRecord(resultPanel.totalScoreRecord, resultPanel.totalScoreText, resultPanel.totalScoreSubject, "ShootingKing.Result.TotalScore");
			yield return StartCoroutine(RecordAnimation(resultPanel.pointScoreRecord, resultPanel.pointScoreText, score.pointScore));
			yield return StartCoroutine(RecordAnimation(resultPanel.perfectShotRecord, resultPanel.perfectShotText, score.perfectScore));
			yield return StartCoroutine(RecordAnimation(resultPanel.niceShotRecord, resultPanel.niceShotText, score.niceScore));
			yield return StartCoroutine(RecordAnimation(resultPanel.goodShotRecord, resultPanel.goodShotText, score.goodScore));
			yield return StartCoroutine(RecordAnimation(resultPanel.timeBonusRecord, resultPanel.timeBonusText, score.timeBonus));
			yield return new WaitForSecondsRealtime(0.5f);
			yield return StartCoroutine(RecordAnimation(resultPanel.totalScoreRecord, resultPanel.totalScoreText, score.TotalScore));
			resultPanelCoroutine = null;

			void InitializeRecord(CanvasGroup record, TMP_Text recordText, TMP_Text subjectText, string textKey, string parameter = "")
			{
				record.alpha = 0f;
				recordText.text = "0";
				subjectText.text = string.Format(GetLocalizedText(textKey), parameter);
			}
			IEnumerator RecordAnimation(CanvasGroup record, TMP_Text recordText, int score)
			{
				PlaySFX(sfxResultScoring);
				recordText.DOCounter(0, score, animationTime).SetUpdate(true);
				record.transform.localScale = Vector3.one * 1.05f;
				record.DOFade(1f, animationTime).SetUpdate(true);
				yield return record.transform.DOScale(1f, animationTime).SetEase(Ease.InCubic).SetUpdate(true).WaitForCompletion();
			}
		}

		void ResultPanelAnimationComplete()
		{
			RecordComplete(resultPanel.pointScoreRecord, resultPanel.pointScoreText, score.pointScore);
			RecordComplete(resultPanel.perfectShotRecord, resultPanel.perfectShotText, score.perfectScore);
			RecordComplete(resultPanel.niceShotRecord, resultPanel.niceShotText, score.niceScore);
			RecordComplete(resultPanel.goodShotRecord, resultPanel.goodShotText, score.goodScore);
			RecordComplete(resultPanel.timeBonusRecord, resultPanel.timeBonusText, score.timeBonus);
			RecordComplete(resultPanel.totalScoreRecord, resultPanel.totalScoreText, score.TotalScore);

			void RecordComplete(CanvasGroup record, TMP_Text textComponent, int scoreInt)
			{
				record.alpha = 1f;
				record.transform.DOKill();
				record.transform.localScale = Vector3.one;
				textComponent.DOKill();
				textComponent.text = scoreInt.ToString("N0");
			}
		}

		void SetTutorialOverlay(int index)
		{
			tutorialOverlays.ForEach(x => x.gameObject.SetActive(false));
			tutorialAimSphere.SetActive(index == 1);
			tutorialOverlaySkipButton.SetActive(index >= 0 && index <= 2);
			if (index >= 0)
			{
				CanvasGroup tutorialOverlay = tutorialOverlays[index];
				tutorialOverlay.gameObject.SetActive(true);
				tutorialOverlay.alpha = 0f;
				Vector3 initialPosition = tutorialOverlay.transform.localPosition;
				tutorialOverlay.transform.localPosition += Vector3.up * 70f;
				tutorialOverlay.DOFade(1f, animationTime);
				tutorialOverlay.transform.DOLocalMove(initialPosition, animationTime);
			}
		}
	}
}
