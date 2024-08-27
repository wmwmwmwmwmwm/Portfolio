using BoraBattle.Game.Interface;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	// 버튼으로부터 호출되는 기능들
	public partial class BingoController
	{
		/// <summary>
		/// 화면 상단부분 터치 시 뷰 전환
		/// </summary>
		public void ChangeTopViewButton()
		{
			numberSummaryOpened = !numberSummaryOpened;
			numberSummary.SetActive(numberSummaryOpened);
			bingoBallParent.GetComponent<CanvasGroup>().alpha = numberSummaryOpened ? 0f : 1f;
		}

		/// <summary>
		/// 숫자 터치 시 마킹 / 페널티
		/// </summary>
		void NumberBoxButton(BingoNumberBox selectedNumberBox)
		{
			BingoItem blockingMainItem = usingItems.Find(x => x.IsBlockingMain());
			// 숫자 터치 : 아이템 사용 중일 때
			if (blockingMainItem)
			{
				List<BingoNumberBox> boxesToMark = new List<BingoNumberBox>();
				switch (blockingMainItem.itemType)
				{
					case BingoItem.ItemType.Select1:
					case BingoItem.ItemType.Select2:
						MarkNumberBox(selectedNumberBox, DaubScoreType.None, true);
						blockingMainItem.selectedNumberBoxes.Add(selectedNumberBox);
						break;
					case BingoItem.ItemType.Horizontal:
						for (int i = 0; i < 5; i++)
						{
							BingoNumberBox numberBox = CurrentBoard.board[i, selectedNumberBox.boardCoord.y];
							if (numberBox.IsChecked)
							{
								continue;
							}
							boxesToMark.Add(numberBox);
						}
						blockingMainItem.selectedNumberBoxes.Add(selectedNumberBox);
						break;
					case BingoItem.ItemType.Vertical:
						for (int i = 0; i < 5; i++)
						{
							BingoNumberBox numberBox = CurrentBoard.board[selectedNumberBox.boardCoord.x, i];
							if (numberBox.IsChecked)
							{
								continue;
							}
							boxesToMark.Add(numberBox);
						}
						blockingMainItem.selectedNumberBoxes.Add(selectedNumberBox);
						break;
					case BingoItem.ItemType.RandomBall:
						if (blockingMainItem.selectedBingoBall.imageNumber.number == selectedNumberBox.number)
						{
							MarkNumberBox(selectedNumberBox, DaubScoreType.RandomBallPerfect, false);
							blockingMainItem.selectedNumberBoxes.Add(selectedNumberBox);
						}
						else
						{
							MarkNumberBox(selectedNumberBox, DaubScoreType.Oops);
                        }
                        break;
				}
				switch (blockingMainItem.itemType)
				{
					case BingoItem.ItemType.Horizontal:
					case BingoItem.ItemType.Vertical:
						ApplyDoubleScoreAndAnimation(scorePerItem, true);
						score.boosterBonus += scorePerItem;
						StartCoroutine(NumberBoxStateChangeAnimation(boxesToMark, NumberBoxStateChangeType.MarkOnly, CurrentBoard));
						break;
				}
				CheckAvailableBingos();
			}
			// 숫자 터치 : 일반 진행 상황
			else if (appearedNumbers.Contains(selectedNumberBox.number))
			{
				if (selectedNumberBox.number == CurrentBingoBall.imageNumber.number)
				{
					if (BingoBallDuration - CurrentBingoBall.remainedTime < 1.5f)
					{
						MarkNumberBox(selectedNumberBox, DaubScoreType.Perfect);
					}
					else if (BingoBallDuration - CurrentBingoBall.remainedTime < 2.5f)
					{
						MarkNumberBox(selectedNumberBox, DaubScoreType.Great);
					}
					else if (BingoBallDuration - CurrentBingoBall.remainedTime < 3.5f)
					{
						MarkNumberBox(selectedNumberBox, DaubScoreType.Nice);
					}
					else
					{
						MarkNumberBox(selectedNumberBox, DaubScoreType.Good);
					}
				}
				else
				{
					MarkNumberBox(selectedNumberBox, DaubScoreType.Good);
				}
				CheckAvailableBingos();
			}
			// 잘못 마킹 시 페널티
			else
			{
				MarkNumberBox(selectedNumberBox, DaubScoreType.Oops);
			}
		}

		/// <summary>
		/// 빙고버튼 터치 시 빙고 / 페널티
		/// </summary>
		public void BingoButton()
		{
			if (CurrentBoard.availableBingos.Count > 0)
			{
				CurrentBoard.achievedBingos.AddRange(CurrentBoard.availableBingos);
				if (CurrentBoard.achievedBingos.Count == maxBingoCount)
				{
					ApplyDoubleScoreAndAnimation(scorePerBlackout, false);
					score.blackoutCount++;
					CurrentBoard.blackout = true;
				}
				ApplyDoubleScoreAndAnimation(CurrentBoard.availableBingos.Count * scorePerBingo, true);
				score.bingoCount += CurrentBoard.availableBingos.Count;
				bingoAnimationCoroutines.Add(StartCoroutine(BingoEffectAnimation(!CurrentBoard.blackout)));
				CurrentBoard.availableBingos.Clear();
				CurrentBoard.lastAvailableBingoCount = 0;
				UpdateBingoButtonAnimation(BingoButtonAnimationType.None);
				if (CurrentBoard.blackout)
				{
					blackoutAnimationCoroutines.Add(StartCoroutine(BlackoutAnimation()));
				}
			}
			else
			{
				AddPenalty(penaltyWrongBingo);
				StartCoroutine(TopAlertAnimation(negativeTopAlert, GetLocalizedText("Bingo.InGame.WrongMsg")));
				bingoButton.animator.Play("Release", false);
				bingoButton.transform.DOShakePosition(0.2f, strength: 6f, vibrato: 100, snapping: true, fadeOut: false);
			}
		}

		/// <summary>
		/// 아이템 터치 시 아이템 사용
		/// </summary>
		void UseItemButton()
		{
			PlaySFX(sfxButton);
			BingoItem item = obtainedItems[^1];
			switch (item.itemType)
			{
				case BingoItem.ItemType.Select1:
				case BingoItem.ItemType.Select2:
				case BingoItem.ItemType.Horizontal:
				case BingoItem.ItemType.Vertical:
					StartCoroutine(ItemSelectBox(item));
					break;
				case BingoItem.ItemType.RandomBall:
					StartCoroutine(ItemSelectBall(item));
					break;
				case BingoItem.ItemType.AddTime:
					StartCoroutine(TopAlertAnimation(itemTopAlert, GetLocalizedText("Bingo.InGame.ItemTitle5")));
					SetRemainedTime(remainedTime + 10f);
					GameObject newDeltaTimerText = Instantiate(deltaTimerText.gameObject, instanceParent, true);
					newDeltaTimerText.SetActive(true);
					TMP_Text textComponent = newDeltaTimerText.GetComponent<TMP_Text>();
					textComponent.text = "+ 10";
					StartCoroutine(DeltaTextAnimation(textComponent));
					KillTweenDestroy(item.gameObject, item.transform);
					break;
				case BingoItem.ItemType.DoubleScore:
					StartCoroutine(TopAlertAnimation(itemTopAlert, GetLocalizedText("Bingo.InGame.ItemTitle6")));
					if (doubleScoreCoroutine != null)
					{
						StopCoroutine(doubleScoreCoroutine);
						doubleScoreCoroutine = null;
					}
					doubleScoreCoroutine = StartCoroutine(DoubleScoreItem(item));
					KillTweenDestroy(item.gameObject, item.transform);
					break;
			}
			usingItems.Add(item);
			obtainedItems.Remove(item);
			itemFillGaugeFullText.SetActive(ItemFull);
		}

		/// <summary>
		/// 하단 카드 터치 시 카드 전환
		/// </summary>
		public void CardSelectButton(int index)
		{
			PlaySFX(sfxChangeCard);
			CardChange(index);
			UpdateBingoButtonAnimation(BingoButtonAnimationType.None);
		}

		/// <summary>
		/// 랜덤볼 아이템 사용 중 BingoBall 터치 시 
		/// </summary>
		public void BingoBallButton(BingoBall selectedBall)
		{
			PlaySFX(sfxButton);
			usingItems.Find(x => x.itemType == BingoItem.ItemType.RandomBall).selectedBingoBall = selectedBall;
		}

		/// <summary>
		/// 결과창 점수제출 버튼 터치 시
		/// </summary>
		public void ResultNextButton()
		{
			resultPanelNext = true;
		}

		/// <summary>
		/// 일시정지 버튼 터치 시
		/// </summary>
		public void PauseButton()
		{
			isPaused = true;
            pausePanel.SetActive(true);
            pause_BgmButton.Play(BGMMuted ? "Off" : "On", false);
            pause_SfxButton.Play(SFXMuted ? "Off" : "On", false);
		}

		/// <summary>
		/// 일시정지창 닫기 버튼 터치 시
		/// </summary>
		public void PauseCloseButton()
		{
			PlaySFX(sfxButton);
			isPaused = false;
			pausePanel.SetActive(false);
		}

		/// <summary>
		/// 일시정지창 배경음 버튼 터치 시
		/// </summary>
		public void PauseBGMButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.BgmVolume = BGMMuted ? 1f : 0f;
			pause_BgmButton.Play(BGMMuted ? "Off" : "On", true);
        }

		/// <summary>
		/// 일시정지창 효과음 버튼 터치 시
		/// </summary>
		public void PauseSFXButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.SfxVolume = SFXMuted ? 1f : 0f;
            pause_SfxButton.Play(SFXMuted ? "Off" : "On", true);
        }


		/// <summary>
		/// 일시정지창 게임방법 버튼 터치 시
		/// </summary>
		public void PauseTutorialButton()
		{
			PlaySFX(sfxButton);
			ShowTutorial(true);
		}

		/// <summary>
		/// 일시정지창 게임종료 버튼 터치 시
		/// </summary>
		public void PauseExitButton()
		{
			exitConfirmPanel.SetActive(true);
		}

		/// <summary>
		/// 게임종료 확인창 게임종료 버튼 터치 시
		/// </summary>
		public void ExitConfirmExitButton()
		{
			userExitDemand = true;
		}

		/// <summary>
		/// 게임종료 확인창 이어하기 버튼 터치 시
		/// </summary>
		public void ExitConfirmResumeButton()
		{
			PlaySFX(sfxButton);
			exitConfirmPanel.SetActive(false);
		}

		/// <summary>
		/// 튜토리얼창 닫기 버튼 터치 시
		/// </summary>
		public void TutorialCloseButton()
		{
			PlaySFX(sfxButton);
			ShowTutorial(false);
		}

		/// <summary>
		/// 튜토리얼창 이전 버튼 터치 시
		/// </summary>
		public void TutorialPrevButton()
		{
			PlaySFX(sfxButton);
			tutorialPanel.SetTutorialPage(tutorialPanel.pageIndex - 1);
		}

		/// <summary>
		/// 튜토리얼창 다음 버튼 터치 시
		/// </summary>
		public void TutorialNextButton()
		{
			PlaySFX(sfxButton);
			if (tutorialPanel.pageIndex != tutorialPanel.tutorialPages.Count - 1)
			{
				tutorialPanel.SetTutorialPage(tutorialPanel.pageIndex + 1);
			}
			else
			{
				ShowTutorial(false);
			}
		}
	}
}
