using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BoraBattle.Game.WorldBasketballKing
{
	public partial class BasketballController
	{
		readonly List<string> netAnimationNames = new() { "Motion1", "Motion2", "Motion3" };
		public void NetAnimation()
		{
			currentStage.netAnimator.Play(netAnimationNames.PickOne(), 0, 0f);
		}

		public void Goal(BasketballBall ball)
		{
			if (!isPlaying) return;

			int goalScore = ball.isCleanShot ? 3 : 2;
			goalScore *= booster ? 2 : 1;
			combo++;
			if (combo >= 2)
			{
				score.comboCount++;
				comboText.gameObject.SetActive(true);
				comboText.Play(singleAnimation, 0, 0f);
			}

			bool boosterThisFrame = false;
			if (ball.isCleanShot)
			{
				AddScore(3);
				GameObject newCleanScoreText = Instantiate(cleanScoreText, effectParent);
				newCleanScoreText.GetComponent<TMP_Text>().text = goalScore.ToString();
				StartCoroutine(ScoreTextAnimation(newCleanScoreText));
				PlaySFX(sfxClean1);
				PlaySFX(sfxClean2);
				if (!booster)
				{
					boosterThisFrame = SetBoosterGauge(boosterGauge + 0.25f);
				}
				if (!boosterThisFrame && AnimationDone(boosterText))
				{
					cleanText.gameObject.SetActive(true);
					cleanText.GetComponent<TMP_Text>().ForceMeshUpdate(true);
					cleanText.Play(singleAnimation, 0, 0f);
				}
			}
			else
			{
				AddScore(2);
				GameObject newNiceScoreText = Instantiate(niceScoreText, effectParent);
				newNiceScoreText.GetComponent<TMP_Text>().text = goalScore.ToString();
				StartCoroutine(ScoreTextAnimation(newNiceScoreText));
				PlaySFX(sfxNice);
				if (!booster)
				{
					boosterThisFrame = SetBoosterGauge(boosterGauge + 0.1f);
				}
				if (!boosterThisFrame && AnimationDone(boosterText))
				{
					niceText.gameObject.SetActive(true);
					niceText.GetComponent<TMP_Text>().ForceMeshUpdate(true);
					niceText.Play(singleAnimation, 0, 0f);
				}
			}

			IEnumerator ScoreTextAnimation(GameObject textObject)
			{
				textObject.SetActive(true);
				textObject.GetComponent<TMP_Text>().ForceMeshUpdate(true);
				CanvasGroup canvasGroup = textObject.GetComponent<CanvasGroup>();
				canvasGroup.alpha = 0f;
				textObject.transform.localPosition = effectParent.InverseTransformPoint(_rim.position).WithZ(0f) * 0.5f;
				Vector3 scaleOriginal = textObject.transform.localScale;
				if (booster)
				{
					canvasGroup.alpha = 1f;
					yield return textObject.transform.DOScale(scaleOriginal * 1.8f, 0.2f).WaitForCompletion();
					textObject.transform.DOScale(scaleOriginal, 0.2f);
				}
				else
				{
					textObject.transform.localScale = scaleOriginal * 1.5f;
					textObject.transform.DOScale(scaleOriginal, 0.2f);
					yield return canvasGroup.DOFade(1f, 0.2f).WaitForCompletion();
				}
				textObject.transform.DOBlendableLocalMoveBy(Vector3.up * 70f, 0.4f);
				yield return canvasGroup.DOFade(0f, 0.4f).WaitForCompletion();
				Destroy(textObject);
			}
		}

		void SetRemainedTime(float newRemainedTime)
		{
			remainedTime = newRemainedTime;
			remainedTimeText.text = remainedTime.ToString("0");
			int timerSeconds = (int)remainedTime;
			if (remainedTime < 10f && timerSeconds != lastTimerSeconds)
			{
				PlaySFX(sfxTimerAlert);
				lastTimerSeconds = timerSeconds;
			}
			if (!timerAlert30Seconds && remainedTime < 30f)
			{
				timerAlert30Seconds = true;
				StartCoroutine(TimerAlertAnimation("BasketBall.InGame.Timenotice.30Sec"));
			}
			else if (!timerAlert10Seconds && remainedTime < 10f)
			{
				timerAlert10Seconds = true;
				StartCoroutine(TimerAlertAnimation("BasketBall.InGame.Timenotice.10sec"));
			}

			IEnumerator TimerAlertAnimation(string textKey)
			{
				timerAlert.gameObject.SetActive(true);
				timerAlert.alpha = 0f;
				timerAlertText.text = GetLocalizedText(textKey);
				yield return timerAlert.DOFade(1f, 0.7f).WaitForCompletion();
				yield return new WaitForSeconds(1.2f);
				yield return timerAlert.DOFade(0f, 0.7f).WaitForCompletion();
			}
		}

		void AddScore(int addScore)
		{
			score.scoreCount += addScore;
			if (booster)
			{
				score.scoreCount += addScore;
			}
			scoreText.text = score.scoreCount.ToString();
			if (round < currentSeedData.targetScores.Count && score.scoreCount >= currentSeedData.targetScores[round])
			{
				_nextActionReady = true;
				round++;
				roundText.text = (round + 1).ToString();
				StartCoroutine(RoundTextAnimation());
			}

			IEnumerator RoundTextAnimation()
			{
				roundText.transform.localScale = Vector3.one * 1.2f;
				yield return new WaitForSeconds(1.7f);
				yield return roundText.transform.DOScale(1f, 0.7f).WaitForCompletion();
			}
		}

		bool SetBoosterGauge(float newGauge)
		{
			boosterGauge = newGauge;
			boosterFillGauge.fillAmount = boosterGauge;
			if (boosterCoroutine == null && boosterGauge > 0.99f)
			{
				boosterCoroutine = StartCoroutine(BoosterCoroutine());
				return true;
			}
			return false;

			IEnumerator BoosterCoroutine()
			{
				StartCoroutine(PlayAnimator(boosterText));
				Sequence overlaySequence = DOTween.Sequence();
				overlaySequence.Append(whiteOverlay.DOFade(0.22f, 0.1f));
				overlaySequence.Append(whiteOverlay.DOFade(0f, 0.5f));

				foreach (BasketballBall ball in allBalls)
				{
					ball.SetParticleState(true);
				}
				float stageBrightness = currentStage.initialBrightness;
				currentStage.wallFrameMaterial.DOColor(Color.HSVToRGB(0f, 0f, stageBrightness * 0.7f), 0.7f);
				boosterFillGauge.color = boosterFillColor;

				boosterTime = 10f;
				while (booster)
				{
					yield return null;
					SetBoosterGauge(boosterTime / 10f);
					boosterTime -= Time.deltaTime;
				}

				foreach (BasketballBall ball in allBalls)
				{
					ball.SetParticleState(false);
				}
				currentStage.wallFrameMaterial.DOColor(Color.HSVToRGB(0f, 0f, stageBrightness), 0.7f);
				boosterFillGauge.color = Color.white;

				boosterCoroutine = null;
			}
		}

		IEnumerator TimeoutImageCoroutine()
		{
			PlaySFX(sfxTimeout);
			timeoutImage.gameObject.SetActive(true);
			for (int i = 0; i < 4; i++)
			{
				yield return new WaitForSeconds(0.3f);
				timeoutImage.overrideSprite = timeout2Sprite;
				yield return new WaitForSeconds(0.3f);
				timeoutImage.overrideSprite = null;
			}
			timeoutImage.gameObject.SetActive(false);
		}

		public IEnumerator ResultPanelCoroutine()
		{
			PlaySFX(sfxResult);
			resultPanel.SetActive(true);
			resultPanelCoroutine = StartCoroutine(ResultPanelAnimation());
			yield return new WaitUntil(() => resultPanelNext || resultPanelCoroutine == null);
			resultPanelNext = false;
			CheckAndStopCoroutine(ref resultPanelCoroutine);
			ResultPanelAnimationComplete();
			resultPanelFillImage.DOFillAmount(1f, 3f).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() => resultPanelNext = true);
			yield return new WaitUntil(() => resultPanelNext);
		}

		IEnumerator ResultPanelAnimation()
		{
			InitializeRecord(resultBasicScore, $"{score.scoreCount} X {scorePerScoreCount}");
			InitializeRecord(resultComboScore, $"{score.comboCount} X {scorePerComboCount}");
			InitializeRecord(resultTotalScore, "");
			yield return StartCoroutine(RecordAnimation(resultBasicScore, score.scoreCountScore));
			yield return StartCoroutine(RecordAnimation(resultComboScore, score.comboCountScore));
			yield return new WaitForSecondsRealtime(0.5f);
			yield return StartCoroutine(RecordAnimation(resultTotalScore, score.totalScore));
			resultPanelCoroutine = null;

			void InitializeRecord(BasketballResultRecord record, string countText)
			{
				record.canvasGroup.alpha = 0f;
				record.count.text = countText;
				record.score.text = "0";
			}
			IEnumerator RecordAnimation(BasketballResultRecord record, int score)
			{
				record.score.DOCounter(0, score, 0.7f).SetUpdate(true);
				record.canvasGroup.DOFade(1f, 0.7f).SetUpdate(true);
				record.transform.localScale = Vector3.one * 1.05f;
				yield return record.transform.DOScale(1f, 0.7f).SetEase(Ease.InCubic).SetUpdate(true).WaitForCompletion();
			}
		}

		void ResultPanelAnimationComplete()
		{
			RecordComplete(resultBasicScore, score.scoreCountScore);
			RecordComplete(resultComboScore, score.comboCountScore);
			RecordComplete(resultTotalScore, score.totalScore);

			void RecordComplete(BasketballResultRecord record, int scoreInt)
			{
				record.canvasGroup.alpha = 1f;
				record.transform.DOKill();
				record.transform.localScale = Vector3.one;
				record.score.DOKill();
				record.score.text = scoreInt.ToString("N0");
			}
		}
	}
}