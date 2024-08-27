using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BoraBattle.Game.BingoMasterKing
{
	// DOTween 애니메이션 모음
	public partial class BingoController
	{
		IEnumerator DestroyBingoBall(BingoBall bingoBall)
		{
			appearedBingoBalls.Remove(bingoBall);
			yield return bingoBall.transform.DOScale(0f, animationTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Manual).WaitForCompletion();
			KillTweenDestroy(bingoBall.gameObject);
		}

		IEnumerator DeltaTextAnimation(TMP_Text textComponent)
		{
			float positionX = textComponent.transform.localPosition.x;
			textComponent.transform.DOLocalMoveX(positionX + 20f, animationTime / 2f).SetEase(Ease.OutCubic);
			yield return textComponent.DOFade(1f, animationTime / 2f).SetEase(Ease.OutCubic).WaitForCompletion();
			textComponent.transform.DOLocalMoveX(positionX, animationTime / 2f).SetEase(Ease.InCubic);
			yield return textComponent.DOFade(0f, animationTime / 2f).SetEase(Ease.InCubic).WaitForCompletion();
			KillTweenDestroy(textComponent.gameObject, textComponent.transform);
		}

		IEnumerator BingoEffectAnimation(bool showText)
		{
			List<BingoData> availableBingosCopy = new List<BingoData>(CurrentBoard.availableBingos);
			if (showText)
			{
				string bingoText = string.Format(GetLocalizedText("Bingo.InGame.Bingo.Extra"), availableBingosCopy.Count);
				switch (availableBingosCopy.Count)
				{
					case 1: bingoText = GetLocalizedText("Bingo.InGame.Bingo1"); break;
					case 2: bingoText = GetLocalizedText("Bingo.InGame.Bingo2"); break;
					case 3: bingoText = GetLocalizedText("Bingo.InGame.Bingo3"); break;
				}
				StartCoroutine(TopAlertAnimation(positiveTopAlert, bingoText));
			}
            BingoBoard animatedBoard = CurrentBoard;
			foreach (BingoData bingoData in availableBingosCopy)
			{
				yield return StartCoroutine(NumberBoxStateChangeAnimation(bingoData.associatedNumberBoxes, NumberBoxStateChangeType.ProceedState, animatedBoard));
			}
			bingoAnimationCoroutines.RemoveAt(0);
		}

		IEnumerator BlackoutAnimation()
		{
			StartCoroutine(TopAlertAnimation(positiveTopAlert, GetLocalizedText("Bingo.InGame.Bingo.Blackout")));
			yield return new WaitUntil(() => bingoAnimationCoroutines.Count == 0);
			List<BingoData> bingosToAnimate = CurrentBoard.achievedBingos.FindAll(x => x.bingoType == BingoData.BingoType.Vertical);
			bingosToAnimate.Sort(bingoDataComparer);
            BingoBoard animatedBoard = CurrentBoard;
            foreach (BingoData bingoData in bingosToAnimate)
			{
				yield return StartCoroutine(NumberBoxStateChangeAnimation(bingoData.associatedNumberBoxes, NumberBoxStateChangeType.None, animatedBoard));
			}
			blackoutAnimationCoroutines.RemoveAt(0);
		}

		public enum NumberBoxStateChangeType { ProceedState, MarkOnly, None }
		IEnumerator NumberBoxStateChangeAnimation(List<BingoNumberBox> numberBoxes, NumberBoxStateChangeType stateChangeType, BingoBoard animatedBoard)
		{
			foreach (BingoNumberBox numberBox in numberBoxes)
			{
				switch (stateChangeType)
				{
					case NumberBoxStateChangeType.ProceedState:
						numberBox.SetState(numberBox.state + 1);
						break;
					case NumberBoxStateChangeType.MarkOnly:
						numberBox.SetState(BingoNumberBox.State.NumberMarked);
						break;
                }
			}
			foreach (BingoNumberBox numberBox in numberBoxes)
			{
				PlaySFX(sfxBingoTile);
				numberBox.UpdateSprite(stateChangeType == NumberBoxStateChangeType.MarkOnly);
                StartCoroutine(numberBox.ProceedAnimation(animatedBoard.root.transform));
                yield return new WaitForSeconds(0.08f);
			}
		}

		IEnumerator TopAlertAnimation(CanvasGroup topAlertPrefab, string labelText)
		{
			GameObject newTopLabel = Instantiate(topAlertPrefab.gameObject, instanceParent);
			newTopLabel.SetActive(true);
			newTopLabel.GetComponentInChildren<TMP_Text>().text = labelText;
            Animator anim = newTopLabel.GetComponent<Animator>();
			anim.Play(singleAnimation);
			yield return new WaitUntil(() => anim.Done());
			Destroy(newTopLabel);
		}

		IEnumerator CountdownAnimation(TMP_Text animatedText)
		{
			animatedText.gameObject.SetActive(true);
			yield return animatedText.transform.DOScale(2.5f, 0.3f).WaitForCompletion();
			yield return animatedText.DOFade(0f, 0.7f).SetEase(Ease.InCubic).WaitForCompletion();
			animatedText.transform.DOScale(1.5f, 0.4f);
			yield return new WaitForSeconds(0.3f);
			KillTweenDestroy(animatedText.gameObject, animatedText.transform);
		}

		IEnumerator ScoreTextAnimation(float prevScore, float deltaScore)
		{
			if (deltaScore == 0f) yield break;
			GameObject newDeltaScoreText = Instantiate(deltaScoreText.gameObject, instanceParent, true);
			newDeltaScoreText.SetActive(true);
			TMP_Text textComponent = newDeltaScoreText.GetComponent<TMP_Text>();
			textComponent.text = ((int)deltaScore).GetSignedText();
			StartCoroutine(DeltaTextAnimation(textComponent));
			yield return scoreText.DOCounter((int)prevScore, (int)(prevScore + deltaScore), animationTime, true).WaitForCompletion();
		}

		IEnumerator ResultPanelAnimation()
		{
			PlaySFX(sfxFinishGame);
			resultPanel.markRecord.SetActive(false);
			resultPanel.markScoreSubject.text = string.Format(GetLocalizedText("Bingo.Result.CorrectDaub"), score.markCount);
			resultPanel.timeBonusRecord.SetActive(false);
			resultPanel.timeBonusSubject.text = GetLocalizedText("Bingo.Result.SpeedBonus");
			resultPanel.bingoScoreRecord.SetActive(false);
			resultPanel.bingoScoreSubject.text = string.Format(GetLocalizedText("Bingo.Result.Bingos"), score.bingoCount);
			resultPanel.boosterBonusRecord.SetActive(false);
			resultPanel.boosterBonusSubject.text = GetLocalizedText("Bingo.Result.BoosterBonus");
			resultPanel.blackoutScoreRecord.SetActive(false);
			resultPanel.blackoutScoreSubject.text = string.Format(GetLocalizedText("Bingo.Result.BlackoutBonus"), score.blackoutCount);
			resultPanel.blackoutBonusRecord.SetActive(false);
			resultPanel.blackoutBonusSubject.text = string.Format(GetLocalizedText("Bingo.Result.TimeBonus"), score.remainedTime.ToString("0"));
			resultPanel.penaltyRecord.SetActive(false);
			resultPanel.penaltySubject.text = GetLocalizedText("Bingo.Result.Penalty");
			resultPanel.totalScoreRecord.SetActive(false);
			resultPanel.totalScoreSubject.text = GetLocalizedText("Bingo.Result.TotalScore");
			yield return StartCoroutine(RecordAnimation(resultPanel.markRecord, resultPanel.markScoreText, score.markScore));
			yield return StartCoroutine(RecordAnimation(resultPanel.timeBonusRecord, resultPanel.timeBonusText, score.timeBonus));
			yield return StartCoroutine(RecordAnimation(resultPanel.bingoScoreRecord, resultPanel.bingoScoreText, score.bingoScore));
			yield return StartCoroutine(RecordAnimation(resultPanel.boosterBonusRecord, resultPanel.boosterBonusText, score.boosterBonus));
			yield return StartCoroutine(RecordAnimation(resultPanel.blackoutScoreRecord, resultPanel.blackoutScoreText, score.blackoutScore));
			yield return StartCoroutine(RecordAnimation(resultPanel.blackoutBonusRecord, resultPanel.blackoutBonusText, score.blackoutBonus));
			yield return StartCoroutine(RecordAnimation(resultPanel.penaltyRecord, resultPanel.penaltyText, score.penalty));
            yield return new WaitForSeconds(0.6f);
			yield return StartCoroutine(RecordAnimation(resultPanel.totalScoreRecord, resultPanel.totalScoreText, score.TotalScore));
            yield return new WaitForSeconds(1.2f);
            resultPanelAnimationCoroutine = null;

			IEnumerator RecordAnimation(GameObject record, TMP_Text textComponent, int scoreInt)
			{
				record.SetActive(true);
				textComponent.DOCounter(0, scoreInt, 1.2f);
				yield return record.transform.DOScale(1.05f, 0.1f).WaitForCompletion();
				record.transform.DOScale(1f, 0.2f);
				yield return new WaitForSeconds(0.2f);
            }
        }

		void ResultPanelAnimationComplete()
		{
			RecordComplete(resultPanel.markRecord, resultPanel.markScoreText, score.markScore);
			RecordComplete(resultPanel.timeBonusRecord, resultPanel.timeBonusText, score.timeBonus);
			RecordComplete(resultPanel.bingoScoreRecord, resultPanel.bingoScoreText, score.bingoScore);
			RecordComplete(resultPanel.boosterBonusRecord, resultPanel.boosterBonusText, score.boosterBonus);
			RecordComplete(resultPanel.blackoutScoreRecord, resultPanel.blackoutScoreText, score.blackoutScore);
			RecordComplete(resultPanel.blackoutBonusRecord, resultPanel.blackoutBonusText, score.blackoutBonus);
			RecordComplete(resultPanel.penaltyRecord, resultPanel.penaltyText, score.penalty);
			RecordComplete(resultPanel.totalScoreRecord, resultPanel.totalScoreText, score.TotalScore);

			void RecordComplete(GameObject record, TMP_Text textComponent, int scoreInt)
			{
				record.SetActive(true);
				record.transform.DOKill();
				record.transform.localScale = Vector3.one;
				textComponent.DOKill();
				textComponent.text = scoreInt.ToString("N0");
			}
		}

        //IEnumerator BingoBallsAnimation()
        //{
        //	foreach (Animator ball in resultPanelBingoBalls)
        //	{
        //		ball.Play(singleAnimation);
        //		yield return new WaitForSeconds(0.1f);
        //	}
        //}

        IEnumerator PlayParticle(ParticleSystem prefab, Vector3 position)
        {
            ParticleSystem effect = Instantiate(prefab, instanceParent);
			effect.transform.position = position;
            yield return new WaitForSeconds(effect.main.duration);
            Destroy(effect.gameObject);
        }
    }
}
