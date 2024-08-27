using BoraBattle.Game.Interface;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	// ��ư���κ��� ȣ��Ǵ� ��ɵ�
	public partial class BingoController
	{
		/// <summary>
		/// ȭ�� ��ܺκ� ��ġ �� �� ��ȯ
		/// </summary>
		public void ChangeTopViewButton()
		{
			numberSummaryOpened = !numberSummaryOpened;
			numberSummary.SetActive(numberSummaryOpened);
			bingoBallParent.GetComponent<CanvasGroup>().alpha = numberSummaryOpened ? 0f : 1f;
		}

		/// <summary>
		/// ���� ��ġ �� ��ŷ / ���Ƽ
		/// </summary>
		void NumberBoxButton(BingoNumberBox selectedNumberBox)
		{
			BingoItem blockingMainItem = usingItems.Find(x => x.IsBlockingMain());
			// ���� ��ġ : ������ ��� ���� ��
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
			// ���� ��ġ : �Ϲ� ���� ��Ȳ
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
			// �߸� ��ŷ �� ���Ƽ
			else
			{
				MarkNumberBox(selectedNumberBox, DaubScoreType.Oops);
			}
		}

		/// <summary>
		/// �����ư ��ġ �� ���� / ���Ƽ
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
		/// ������ ��ġ �� ������ ���
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
		/// �ϴ� ī�� ��ġ �� ī�� ��ȯ
		/// </summary>
		public void CardSelectButton(int index)
		{
			PlaySFX(sfxChangeCard);
			CardChange(index);
			UpdateBingoButtonAnimation(BingoButtonAnimationType.None);
		}

		/// <summary>
		/// ������ ������ ��� �� BingoBall ��ġ �� 
		/// </summary>
		public void BingoBallButton(BingoBall selectedBall)
		{
			PlaySFX(sfxButton);
			usingItems.Find(x => x.itemType == BingoItem.ItemType.RandomBall).selectedBingoBall = selectedBall;
		}

		/// <summary>
		/// ���â �������� ��ư ��ġ ��
		/// </summary>
		public void ResultNextButton()
		{
			resultPanelNext = true;
		}

		/// <summary>
		/// �Ͻ����� ��ư ��ġ ��
		/// </summary>
		public void PauseButton()
		{
			isPaused = true;
            pausePanel.SetActive(true);
            pause_BgmButton.Play(BGMMuted ? "Off" : "On", false);
            pause_SfxButton.Play(SFXMuted ? "Off" : "On", false);
		}

		/// <summary>
		/// �Ͻ�����â �ݱ� ��ư ��ġ ��
		/// </summary>
		public void PauseCloseButton()
		{
			PlaySFX(sfxButton);
			isPaused = false;
			pausePanel.SetActive(false);
		}

		/// <summary>
		/// �Ͻ�����â ����� ��ư ��ġ ��
		/// </summary>
		public void PauseBGMButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.BgmVolume = BGMMuted ? 1f : 0f;
			pause_BgmButton.Play(BGMMuted ? "Off" : "On", true);
        }

		/// <summary>
		/// �Ͻ�����â ȿ���� ��ư ��ġ ��
		/// </summary>
		public void PauseSFXButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.SfxVolume = SFXMuted ? 1f : 0f;
            pause_SfxButton.Play(SFXMuted ? "Off" : "On", true);
        }


		/// <summary>
		/// �Ͻ�����â ���ӹ�� ��ư ��ġ ��
		/// </summary>
		public void PauseTutorialButton()
		{
			PlaySFX(sfxButton);
			ShowTutorial(true);
		}

		/// <summary>
		/// �Ͻ�����â �������� ��ư ��ġ ��
		/// </summary>
		public void PauseExitButton()
		{
			exitConfirmPanel.SetActive(true);
		}

		/// <summary>
		/// �������� Ȯ��â �������� ��ư ��ġ ��
		/// </summary>
		public void ExitConfirmExitButton()
		{
			userExitDemand = true;
		}

		/// <summary>
		/// �������� Ȯ��â �̾��ϱ� ��ư ��ġ ��
		/// </summary>
		public void ExitConfirmResumeButton()
		{
			PlaySFX(sfxButton);
			exitConfirmPanel.SetActive(false);
		}

		/// <summary>
		/// Ʃ�丮��â �ݱ� ��ư ��ġ ��
		/// </summary>
		public void TutorialCloseButton()
		{
			PlaySFX(sfxButton);
			ShowTutorial(false);
		}

		/// <summary>
		/// Ʃ�丮��â ���� ��ư ��ġ ��
		/// </summary>
		public void TutorialPrevButton()
		{
			PlaySFX(sfxButton);
			tutorialPanel.SetTutorialPage(tutorialPanel.pageIndex - 1);
		}

		/// <summary>
		/// Ʃ�丮��â ���� ��ư ��ġ ��
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
