using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	// 세부 로직
	public partial class BingoController
	{
		void CreateAndMoveBingoBalls()
		{
			int newNumber = remainedNumbers[0];
			MarkNumber(newNumber);
			BingoBall newBingoBall = Instantiate(bingoBallPrefab.gameObject, bingoBallParent.transform).GetComponent<BingoBall>();

			// 볼 개수를 맞추기 위한 시간 보정
			float lastBingoBallCorrectionTime = 0f;
			if (appearedBingoBalls.Count > 0)
			{
				lastBingoBallCorrectionTime = CurrentBingoBall.remainedTime + CurrentBingoBall.appearingTime;
			}

			newBingoBall.Initialize(newNumber, BingoBallDuration + lastBingoBallCorrectionTime);
			appearedBingoBalls.Insert(0, newBingoBall);
			newBingoBall.transform.position = bingoBallPositions[0].position;
			newBingoBall.transform.localScale = Vector3.zero;

			for (int i = 0; i < appearedBingoBalls.Count; i++)
			{
				BingoBall bingoBall = appearedBingoBalls[i];
				if (i < 5)
				{
					bingoBall.transform.DOMove(bingoBallPositions[i].position, appearingDuration).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Manual);
					bingoBall.transform.DOScale(i == 0 ? 1.4f : 0.9f, appearingDuration).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Manual);
				}
				else
				{
					StartCoroutine(DestroyBingoBall(FinalBingoBall));
				}
			}
		}

		void SetRemainedTime(float newRemainedTime)
		{
			remainedTime = newRemainedTime;
			if (!alert30SecondsShown && remainedTime < 30f)
			{
				PlaySFX(sfxTimeNotice);
				alert30SecondsShown = true;
				StartCoroutine(TopAlertAnimation(countdownTopAlert, GetLocalizedText("Bingo.InGame.TimeNotice30")));
			}
			else if (!alert10SecondsShown && remainedTime < 10f)
			{
				PlaySFX(sfxTimeNotice);
				alert10SecondsShown = true;
				StartCoroutine(TopAlertAnimation(countdownTopAlert, GetLocalizedText("Bingo.InGame.TimeNotice10")));
			}
			else if (!alert5SecondsShown && remainedTime < 5f)
			{
				PlaySFX(sfxTimeNotice);
				alert5SecondsShown = true;
				StartCoroutine(TopAlertAnimation(countdownTopAlert, GetLocalizedText("Bingo.InGame.TimeNotice5")));
			}

			if (!remainedTime10Seconds && remainedTime < 10f)
			{
				remainedTime10Seconds = true;
				remainedTimeText.DOColor(remainedTimeRedColor, 0.3f);
				timerAlertBoardBackground.gameObject.SetActive(true);
				timerAlertBoardBackground.color = Color.white.WithAlpha(0f);
				timerAlertBoardBackground.DOFade(1f, 0.6f).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Yoyo);
			}
			else if (remainedTime10Seconds && remainedTime > 10f)
			{
				remainedTime10Seconds = false;
				remainedTimeText.DOKill();
				remainedTimeText.color = Color.white;
				timerAlertBoardBackground.gameObject.SetActive(false);
				timerAlertBoardBackground.DOKill();
			}
			int minute = (int)(remainedTime / 60f);
			float second = Mathf.Max(0f, remainedTime % 60f);
			if (remainedTime10Seconds)
			{
				remainedTimeText.text = string.Format("{0}:{1}", minute, second.ToString("00.00"));
			}
			else
			{
				remainedTimeText.text = string.Format("{0}:{1}", minute, ((int)second).ToString("00"));
			}
		}

		void ApplyDoubleScoreAndAnimation(int deltaScore, bool applyDoubleScore)
		{
			float prevScore = score.TotalScore;
			if (applyDoubleScore && doubleScoreTimer > 0f)
			{
				score.boosterBonus += deltaScore;
				deltaScore *= 2;
			}
			StartCoroutine(ScoreTextAnimation(prevScore, deltaScore));
		}

		void CheckAvailableBingos()
		{
			CurrentBoard.availableBingos.Clear();
			for (int x = 0; x < 5; x++)
			{
				bool allChecked = true;
				for (int y = 0; y < 5; y++)
				{
					if (!CurrentBoard.board[x, y].IsChecked)
					{
						allChecked = false;
						break;
					}
				}
				if (allChecked)
				{
					CurrentBoard.availableBingos.Add(new BingoData()
					{
						bingoType = BingoData.BingoType.Vertical,
						intData = x,
						associatedNumberBoxes = new List<BingoNumberBox>()
						{
							CurrentBoard.board[x, 0],
							CurrentBoard.board[x, 1],
							CurrentBoard.board[x, 2],
							CurrentBoard.board[x, 3],
							CurrentBoard.board[x, 4],
						}
					});
				}
			}
			for (int y = 0; y < 5; y++)
			{
				bool allChecked = true;
				for (int x = 0; x < 5; x++)
				{
					if (!CurrentBoard.board[x, y].IsChecked)
					{
						allChecked = false;
						break;
					}
				}
				if (allChecked)
				{
					CurrentBoard.availableBingos.Add(new BingoData()
					{
						bingoType = BingoData.BingoType.Horizontal,
						intData = y,
						associatedNumberBoxes = new List<BingoNumberBox>()
						{
							CurrentBoard.board[0, y],
							CurrentBoard.board[1, y],
							CurrentBoard.board[2, y],
							CurrentBoard.board[3, y],
							CurrentBoard.board[4, y],
						}
					});
				}
			}
			bool bingoRightUp = CurrentBoard.board[0, 0].IsChecked && CurrentBoard.board[1, 1].IsChecked && CurrentBoard.board[2, 2].IsChecked && CurrentBoard.board[3, 3].IsChecked && CurrentBoard.board[4, 4].IsChecked;
			if (bingoRightUp)
			{
				CurrentBoard.availableBingos.Add(new BingoData()
				{
					bingoType = BingoData.BingoType.Cross,
					intData = 0,
					associatedNumberBoxes = new List<BingoNumberBox>()
					{
						CurrentBoard.board[0, 0],
						CurrentBoard.board[1, 1],
						CurrentBoard.board[2, 2],
						CurrentBoard.board[3, 3],
						CurrentBoard.board[4, 4],
					}
				});
			}
			bool bingoRightDown = CurrentBoard.board[0, 4].IsChecked && CurrentBoard.board[1, 3].IsChecked && CurrentBoard.board[2, 2].IsChecked && CurrentBoard.board[3, 1].IsChecked && CurrentBoard.board[4, 0].IsChecked;
			if (bingoRightDown)
			{
				CurrentBoard.availableBingos.Add(new BingoData()
				{
					bingoType = BingoData.BingoType.Cross,
					intData = 1,
					associatedNumberBoxes = new List<BingoNumberBox>()
					{
						CurrentBoard.board[0, 4],
						CurrentBoard.board[1, 3],
						CurrentBoard.board[2, 2],
						CurrentBoard.board[3, 1],
						CurrentBoard.board[4, 0],
					}
				});
			}
			bool bingoEdge = CurrentBoard.board[0, 0].IsChecked && CurrentBoard.board[0, 4].IsChecked && CurrentBoard.board[4, 0].IsChecked && CurrentBoard.board[4, 4].IsChecked;
			if (bingoEdge)
			{
				CurrentBoard.availableBingos.Add(new BingoData()
				{
					bingoType = BingoData.BingoType.Edge,
					associatedNumberBoxes = new List<BingoNumberBox>()
					{
						CurrentBoard.board[0, 0],
						CurrentBoard.board[0, 4],
						CurrentBoard.board[4, 0],
						CurrentBoard.board[4, 4],
					}
				});
			}
			CurrentBoard.availableBingos = CurrentBoard.availableBingos.Except(CurrentBoard.achievedBingos, bingoDataComparer).ToList();

			// 새로운 빙고가 만들어졌을 때 애니메이션
			BingoButtonAnimationType buttonAnimationType = BingoButtonAnimationType.None;
			bool newBingoAvailable = CurrentBoard.availableBingos.Count > CurrentBoard.lastAvailableBingoCount;
			if (newBingoAvailable)
			{
				bool isFirstBingo = CurrentBoard.availableBingos.Count == 1 && CurrentBoard.lastAvailableBingoCount == 0 && CurrentBoard.achievedBingos.Count == 0;
				if (isFirstBingo)
				{
					buttonAnimationType = BingoButtonAnimationType.All;
				}
				else
				{
					buttonAnimationType = BingoButtonAnimationType.OnlyGlow;
				}
			}
			UpdateBingoButtonAnimation(buttonAnimationType);
			CurrentBoard.lastAvailableBingoCount = CurrentBoard.availableBingos.Count;
		}

		public enum BingoButtonAnimationType { None, OnlyGlow, All }
		void UpdateBingoButtonAnimation(BingoButtonAnimationType animationType)
		{
			bool bingoAvailable = CurrentBoard.availableBingos.Count > 0;
            CurrentCardButton.SetBingoAvailable(bingoAvailable);
            bingoButtonActiveImage.gameObject.SetActive(bingoAvailable);
			bingoButtonAnimation.SetActive(bingoAvailable);
        }

		void SetItemFillGauge(float newItemFillGauge)
		{
			if (ItemFull) return;
			itemFillGauge = Mathf.Clamp01(newItemFillGauge);
			itemFillGaugeImage2.DOFillAmount(itemFillGauge * 0.9f, animationTime).SetEase(Ease.OutCubic);
            itemFillGaugeImage.DOFillAmount(itemFillGauge * 0.9f, animationTime).SetEase(Ease.OutCubic).OnComplete(() =>
			{
				if (itemFillGauge > 0.99f)
				{
					SetItemFillGauge(0f);
					CreateNewItem(itemList[itemListIndex].itemType);
					itemListIndex = (itemListIndex + 1) % itemList.Count;
				}
			});
		}

		void CreateNewItem(BingoItem.ItemType itemType)
		{
			if (ItemFull) return;
			PlaySFX(sfxGetItem);
			BingoItem itemPrefab = itemPrefabs.Find(x => x.itemType == itemType);
			BingoItem newItem = Instantiate(itemPrefab, itemParent);
			newItem.transform.position = itemCreatedPosition.position;
			newItem.transform.DOMove(itemPositions[obtainedItems.Count].position, animationTime).OnComplete(()=> 
			{
				if (newItem.gameObject.activeSelf)
				{
                    StartCoroutine(PlayParticle(getItemEffectPrefab, newItem.transform.position));
				}
			});
			newItem.GetComponent<Button>().onClick.AddListener(() => UseItemButton());
			obtainedItems.Add(newItem);
			itemFillGaugeFullText.SetActive(ItemFull);
		}

        IEnumerator ItemSelectBox(BingoItem item)
		{
			item.gameObject.SetActive(false);
			ShowItemTopPopup(true, false, true);
			itemTopPopupIcon.sprite = item.GetComponent<Image>().sprite;
			string topText = item.itemType switch
			{
				BingoItem.ItemType.Select1 => GetLocalizedText("Bingo.InGame.ItemTitle0"),
				BingoItem.ItemType.Select2 => GetLocalizedText("Bingo.InGame.ItemTitle1"),
				BingoItem.ItemType.Horizontal => GetLocalizedText("Bingo.InGame.ItemTitle3"),
				BingoItem.ItemType.Vertical => GetLocalizedText("Bingo.InGame.ItemTitle2"),
				_ => ""
			};
			itemTopLabel.GetComponentInChildren<TMP_Text>().text = topText;
			itemTopLabel.transform.localScale = Vector3.one * 1.1f;
			itemTopLabel.transform.DOScale(1f, animationTime);
			itemTopDesc.text = GetLocalizedText("Bingo.InGame.ItemState.Sub");
			int itemCount = 1;
			switch (item.itemType)
			{
				case BingoItem.ItemType.Select2:
					itemCount = 2;
					break;
			}
			item.SetRemainedTime(10f, itemTopTimer);
			while (item.remainedTime > 0f && item.selectedNumberBoxes.Count < itemCount && GetAllUncheckedCell(true).Count > 0)
			{
				item.SetRemainedTime(item.remainedTime - GetDeltaTime(), itemTopTimer);
				yield return null;
			}
			ShowItemTopPopup(false, false);
			usingItems.Remove(item);
			KillTweenDestroy(item.gameObject, item.transform);
		}

		IEnumerator ItemSelectBall(BingoItem item)
		{
			item.gameObject.SetActive(false);

			// 나올 숫자 설정
			List<BingoNumberBox> randomUncheckedCells = GetAllUncheckedCell(false);
			randomUncheckedCells.RemoveAll(x => appearedNumbers.Contains(x.number));
			if (randomUncheckedCells.Count < 4)
			{
				int otherCellCount = 4 - randomUncheckedCells.Count;
				List<BingoNumberBox> randomUncheckedCellsOtherCard = GetAllUncheckedCell(true);
				randomUncheckedCellsOtherCard.RemoveAll(x => CurrentBoard.boardLinear.Find(currentBoardCell => currentBoardCell.number == x.number));
				randomUncheckedCellsOtherCard = randomUncheckedCellsOtherCard.Distinct(bingoNumberBoxComparer).ToList();
				randomUncheckedCellsOtherCard = randomUncheckedCellsOtherCard.SampleList(Mathf.Min(otherCellCount, randomUncheckedCellsOtherCard.Count), randomProvider);
				randomUncheckedCells.AddRange(randomUncheckedCellsOtherCard);
			}
			randomUncheckedCells = randomUncheckedCells.SampleList(Mathf.Min(4, randomUncheckedCells.Count), randomProvider);
			if (randomUncheckedCells.Count == 0)
			{
				yield break;
			}

			// 팝업 표시
			int i = 0;
			foreach (BingoBall bingoBall in itemFullScreenBingoBalls)
			{
				if (i < randomUncheckedCells.Count)
				{
					bingoBall.gameObject.SetActive(true);
					bingoBall.Initialize(randomUncheckedCells[i].number, 0f);
				}
				else
				{
					bingoBall.gameObject.SetActive(false);
				}
				i++;
			}
			itemFullScreenPopup.SetActive(true);

			// 볼 선택 대기
			item.SetRemainedTime(10f, itemFullScreenTimer);
			while (item.remainedTime > 0f && item.selectedBingoBall == null)
			{
				item.SetRemainedTime(item.remainedTime - GetDeltaTime(), itemFullScreenTimer);
				yield return null;
			}

			// 선택하지 않고 시간 초과시
			if (item.selectedBingoBall == null)
			{
				List<BingoBall> allActiveBingoBalls = itemFullScreenBingoBalls.FindAll(x => x.gameObject.activeSelf);
				item.selectedBingoBall = allActiveBingoBalls.PickOne(randomProvider);
			}

			// 선택한 볼의 숫자 마킹 대기
			int ballNumber = item.selectedBingoBall.imageNumber.number;
			MarkNumber(ballNumber);
			itemFullScreenPopup.SetActive(false);
			ShowItemTopPopup(true, true, false);
			itemTopBingoBall.Initialize(ballNumber, 0f);
			item.SetRemainedTime(10f, itemTopTimer);
			List<BingoNumberBox> boxToMark = GetAllUncheckedCell(true).FindAll(x => x.number == ballNumber);
			while (item.remainedTime > 0f && item.selectedNumberBoxes.Count != boxToMark.Count)
			{
				item.SetRemainedTime(item.remainedTime - GetDeltaTime(), itemTopTimer);
				yield return null;
			}

			// 아이템 삭제
			ShowItemTopPopup(false, false, false);
			usingItems.Remove(item);
			KillTweenDestroy(item.gameObject, item.transform);
		}

		void ShowItemTopPopup(bool active, bool immediate, bool showTexts = true)
        {
            itemTopPopup.gameObject.SetActive(active);
			if (immediate)
			{
				itemTopPopup.alpha = active ? 1f : 0f;
			}
			else
			{
				itemTopPopup.DOFade(active ? 1f : 0f, animationTime / 4f);
			}
			itemTopPopupIcon.gameObject.SetActive(showTexts);
			itemTopDesc.gameObject.SetActive(showTexts);
			itemTopLabel.gameObject.SetActive(showTexts);
			itemTopBingoBall.gameObject.SetActive(!showTexts);

            Transform newParent = active ? bingoBoardTopPopupLayer : bingoBoardOriginLayer;
			bingoBoards.ForEach(x => x.root.transform.SetParent(newParent));
			cardButtonsArea.transform.SetParent(newParent);
		}

		void MarkNumberBox(BingoNumberBox numberBox, DaubScoreType scoreType, bool hideNumber = false)
		{
			// 빨리 눌렀을 때 보너스 점수 계산
			int remainedTimeBonusScore = 0;
			switch (scoreType)
			{
				case DaubScoreType.Perfect:
				case DaubScoreType.Great:
				case DaubScoreType.Nice:
				case DaubScoreType.Good:
					if (numberBox.number == CurrentBingoBall.imageNumber.number)
					{
						remainedTimeBonusScore = (int)(CurrentBingoBall.RemainedTimePercent * scorePerMark);
					}
					break;
				case DaubScoreType.RandomBallPerfect:
					remainedTimeBonusScore = scorePerMark;
					break;
			}

			// 점수 알림 표시, 아이템 게이지 상승
			string topAlertText = scoreType switch
			{
				DaubScoreType.Perfect => GetLocalizedText("Bingo.InGame.Perfect"),
				DaubScoreType.Great => GetLocalizedText("Bingo.InGame.Great"),
				DaubScoreType.Nice => GetLocalizedText("Bingo.InGame.Nice"),
				DaubScoreType.Good => GetLocalizedText("Bingo.InGame.Good"),
				DaubScoreType.Oops => GetLocalizedText("Bingo.InGame.WrongMsg"),
				DaubScoreType.RandomBallPerfect => GetLocalizedText("Bingo.InGame.Perfect"),
				_ => ""
			};
			float itemFillGaugeToAdd = scoreType switch
			{
				DaubScoreType.Perfect => 0.75f,
				DaubScoreType.RandomBallPerfect => 0.75f,
				DaubScoreType.Great => 0.5f,
				DaubScoreType.Nice => 0.25f,
				_ => 0f
			};
			switch (scoreType)
			{
				case DaubScoreType.Perfect:
				case DaubScoreType.RandomBallPerfect:
				case DaubScoreType.Great:
				case DaubScoreType.Nice:
				case DaubScoreType.Good:
					StartCoroutine(TopAlertAnimation(positiveTopAlert, topAlertText));
					SetItemFillGauge(itemFillGauge + itemFillGaugeToAdd);
					ApplyDoubleScoreAndAnimation(scorePerMark + remainedTimeBonusScore, true);
					score.markCount++;
					score.timeBonus += remainedTimeBonusScore;
					break;
				case DaubScoreType.Oops:
					StartCoroutine(TopAlertAnimation(negativeTopAlert, topAlertText));
					AddPenalty(penaltyWrongNumber);
					numberBox.transform.DOComplete();
                    numberBox.transform.DOShakePosition(0.2f, strength: 4f, vibrato: 100, snapping: true, fadeOut: false);
                    break;
				case DaubScoreType.None:
					ApplyDoubleScoreAndAnimation(scorePerMark, true);
					score.markCount++;
					break;
			}

			// 빙고보드 숫자 업데이트
			switch (scoreType)
			{
				case DaubScoreType.Perfect:
				case DaubScoreType.RandomBallPerfect:
				case DaubScoreType.Great:
				case DaubScoreType.Nice:
				case DaubScoreType.Good:
				case DaubScoreType.None:
					numberBox.SetState(BingoNumberBox.State.NumberMarked);
					numberBox.UpdateSprite(hideNumber);
                    StartCoroutine(numberBox.ProceedAnimation(CurrentBoard.root.transform));
                    PlaySFX(sfxMarking);
					break;
			}
		}

		IEnumerator DoubleScoreItem(BingoItem item)
		{
			doubleScoreTimer = 10f;
			itemDoubleScoreEffect.SetActive(true);
			scoreIcon.gameObject.SetActive(false);
			while (doubleScoreTimer > 0f)
			{
				if (!usingItems.Find(x => x.IsBlockingMain()))
				{
					doubleScoreTimer -= GetDeltaTime();
				}
				yield return null;
			}
			itemDoubleScoreEffect.SetActive(false);
			scoreIcon.gameObject.SetActive(true);
		}

		List<BingoNumberBox> GetAllUncheckedCell(bool allBoard)
		{
			List<BingoNumberBox> result = new List<BingoNumberBox>();
			if (allBoard)
			{
				foreach (BingoBoard oneBoard in GetAllBoard())
				{
					result.AddRange(oneBoard.boardLinear.FindAll(x => x.state == BingoNumberBox.State.NumberUnmarked));
				}
			}
			else
			{
				result.AddRange(CurrentBoard.boardLinear.FindAll(x => x.state == BingoNumberBox.State.NumberUnmarked));
			}
			return result;
		}

		void AddPenalty(int penalty)
		{
			PlaySFX(sfxOops);
			if (score.TotalScore + penalty < 0f)
			{
				penalty = -score.TotalScore;
			}
			ApplyDoubleScoreAndAnimation(penalty, false);
			score.penalty += penalty;
		}

		void MarkNumber(int number)
		{
			appearedNumbers.Add(number);
			remainedNumbers.Remove(number);
			numberSummaryCells[number - 1].MarkCell();
		}

		void CardChange(int index)
		{
			currentBoardIndex = index;
			int i = 0;

            // 배경 애니메이션, 빙고보드 교체
            backBg.color = CurrentBoard.bgColor;
			CurrentBoard.bg.transform.SetAsFirstSibling();
            foreach (BingoBoard board in bingoBoards)
			{
				bool current = currentBoardIndex == i;
                //board.root.SetActive(current);
                CanvasGroup canvasGroup = board.root.GetComponent<CanvasGroup>();
				canvasGroup.alpha = current ? 1f : 0f;
				canvasGroup.interactable = current;
				canvasGroup.blocksRaycasts = current;
				board.bg.DOKill(true);
				board.bg.DOFade(current ? 1f : 0f, 0.1f);
                i++;
			}
			i = 0;

			// 카드 버튼 색변경
			foreach (BingoCardButton cardButton in cardButtons)
			{
                float alpha = currentBoardIndex == i ? 1f : 0f;
				cardButton.image.color = cardButton.image.color.WithAlpha(alpha);
				i++;
			}
		}

		void ShowTutorial(bool show)
		{
			tutorialPanel.gameObject.SetActive(show);
			tutorialPanel.SetTutorialPage(0);
		}

		List<BingoBoard> GetAllBoard() => bingoBoards.GetRange(0, BoardCount);

		void KillTweenDestroy(GameObject obj, params Component[] components)
		{
			foreach (Component component in components)
			{
				component.DOKill();
			}
			Destroy(obj);
		}
	}
}
