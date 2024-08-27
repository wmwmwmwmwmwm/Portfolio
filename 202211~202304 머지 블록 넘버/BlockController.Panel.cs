using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

#if !MS && !AWS
using Firebase.Analytics;
#endif

using static GameInterface;
using static ServerApi;
using static SoundManager;
using static CommonDefines;
using static BigIntegerUtil;

using BInteger = System.Numerics.BigInteger;

public partial class BlockController
{
	void ShowPausePanel()
	{
		Debug.Log("ShowPausePanel");
		if (gameEnd != GameEndType.Playing) return;
		ShowPanel(pausePanel);
		Pause_UpdateButtons();
		Time.timeScale = 0f;
	}

	async void HidePausePanel(bool touchToStart)
	{
		Debug.Log("HidePausePanel");
		pausePanel.SetActive(false);
		if (touchToStart)
		{
			await TouchToStart();
		}
		Time.timeScale = 1f;
		if (skillGauge >= 1f && UserMgr.bIsOnAutoSkill)
		{
			await ActivateSkill();
		}
	}

	async UniTask TouchToStart()
	{
		touchToStartPanel.SetActive(true);
		Transform textAnimated = touchToStartPanel.transform.Find("TextAnimated");
		TMP_Text text = textAnimated.GetComponent<TMP_Text>();
		textAnimated.localScale = Vector3.one;
		text.color = Color.white;
		textAnimated.DOScale(1.1f, 0.8f).SetUpdate(true).SetLoops(-1).ToUniTask().Forget();
		text.DOFade(0f, 0.8f).SetUpdate(true).SetLoops(-1).ToUniTask().Forget();
		touchToStartTrigger = false;
		await UniTask.WaitUntil(() => touchToStartTrigger);
		touchToStartTrigger = false;
		textAnimated.DOKill(false);
		text.DOKill(false);
		touchToStartPanel.SetActive(false);
		Time.timeScale = 1f;
	}

	async UniTask EndlessModeStartPanel()
	{
		await UpdateBuffIcons(endlessStart_SlowBlockIcon, endlessStart_SkillBoostIcon);
		ShowPanel(endlessModeStartPanel);
		PanelPopEffect(endlessModeStartPanel);
		UpdateStartPanelItemUI(startBlock512ItemUI, startBlock512PurchaseDemand);
		startBlock512ItemUI.priceText.text = startBlock512Price.ToString("N0");
		touchToStartTrigger = false;
		await UniTask.WaitUntil(() => touchToStartTrigger);
		touchToStartTrigger = false;
		endlessModeStartPanel.SetActive(false);
	}

	async void FailPanelContinue()
	{
		// 계속하기 횟수 증가
		nContinue++;

		// 실패 애니메이션 원상복귀
		StageFailAnimationRevert();

		// 위에 3줄 삭제
		bool boardProcessNeed = false;
		List<Block> top3LineBlocks = boardBlocks.FindAll(x => x.data.coord.y >= boardSize.y - 4);
		foreach (Block block in top3LineBlocks)
		{
			switch (block.blockTypeInGame)
			{
				case BlockType.Escape:
				case BlockType.Brick:
					continue;
			}
			RemoveBlock(block, true, true);
			boardProcessNeed = true;
		}

		if (boardProcessNeed)
		{
			gameCancelToken = new();
            await BoardProcess();
		}

		// 게임 이어서 진행
		spawnBlockCount /= 2;
		PlayGame(false).Forget();
	}

	async UniTask OpenItemPurchasePanel(ItemType itemType)
	{
		itemPurchasePanelItemType = itemType;
		itemPurchasePanel.SetActive(true);
		itemPurchasePanel.GetComponent<CanvasGroup>().alpha = 1f;
		UpdateItemPurchasePanel();
		await UniTask.WaitUntil(() => !itemPurchasePanel.activeSelf);
		UpdateItemButtons();
	}

	void UpdateItemPurchasePanel()
	{
		itemPurchasePanelCoinText.text = UserMgr.CoinTotal.ToString("N0");
		switch (itemPurchasePanelItemType)
		{
			case ItemType.Divide:
				itemPurchasePanelIcon.sprite = CommonResource.itemDivideIcon;
				itemPurchasePanelItemTitle.text = GetText("ingame_item_purchase_divide_title");
				itemPurchasePanelDescription.text = GetText("ingame_item_purchase_divide_desc");
				itemPurchasePanelPriceText.text = ITEM_DIVIDE_PRICE.ToString("N0");
				break;
			case ItemType.Add:
				itemPurchasePanelIcon.sprite = CommonResource.itemJokerIcon;
				itemPurchasePanelItemTitle.text = GetText("ingame_item_purchase_add_title");
				itemPurchasePanelDescription.text = GetText("ingame_item_purchase_add_desc");
				itemPurchasePanelPriceText.text = ITEM_ADD_PRICE.ToString("N0");
				break;
			case ItemType.Hammer:
				itemPurchasePanelIcon.sprite = CommonResource.itemHammerIcon;
				itemPurchasePanelItemTitle.text = GetText("ingame_item_purchase_hammer_title");
				itemPurchasePanelDescription.text = GetText("ingame_item_purchase_hammer_desc");
				itemPurchasePanelPriceText.text = ITEM_HAMMER_PRICE.ToString("N0");
				break;
		}
	}

	async UniTask StageClearProcess()
	{
		AdMgr.HideBannerAd();

		bool bShouldLog = (stageNumber <= 50 || stageNumber % 10 == 0);
		if(bShouldLog)
		{
			int logStageNumber = stageNumber;
#if !MS && !AWS
            FirebaseAnalytics.LogEvent("Stage_Clear_999", "No_1", logStageNumber);
#endif
        }

		int coinBefore = UserMgr.CoinTotal;
		if (stageNumber > UserMgr.LastClearStageNumber)
			UserMgr.UpdateQuestCount(QuestType.StageLevel, stageNumber);
		UserMgr.SetLastClearedStageNumber(stageNumber);
		UserMgr.Coin += stageClearCoinAmount;
		UserMgr.PiggyBankCoin += stageClearCoinAmount;
		UserMgr.SaveUserInfo();
		UserMgr.UploadUserInfo().Forget();

#if !MS && !AWS
		if (stageClearCoinAmount > 0) Util.LogFirebaseEventWithUnityLog("Stageclear");
#endif

        if (UserMgr.IsLoginUser())
		{
			SaveLeaderboard(LeaderboardType.STAGE, UserMgr.LastClearStageNumber, highestNumberType).Forget();
		}
#if DEV_TEST
		int stageTimeSeconds = (int)(Time.time - stageStartTime);
		LogStageTime(stageNumber.ToString(), stageTimeSeconds.ToString()).Forget();
#endif

		GameObject trumpetEffect = Instantiate(trumpetEffectPrefab, stageClearPanel.transform);
		ShowPanel(stageClearPanel);
		PanelPopEffect(stageClearPanel);
		if (stageClearCoinAmount != 0)
		{
			stageClear_Coin.SetActive(true);
			stageClear_CoinText.text = "X0";
		}
		else
		{
			stageClear_Coin.SetActive(false);
		}
		UpdateMissionCheckIcons(stageClear_MissionIcons, missions);
		for (int i = 0; i < missions.Count; i++)
		{
			MissionCheckIcon missionIcon = stageClear_MissionIcons[i];
			missionIcon.countText.gameObject.SetActive(false);
		}
		stageClearToken = new();
		await StageClearAnimation(coinBefore).BP(stageClearToken);
		stageClearExit = false; stageClearNext = false;
		await UniTask.WaitUntil(() => stageClearExit || stageClearNext);
		stageClearPanel.SetActive(false);
		CommonCanvas.coin.Hide();
		Destroy(trumpetEffect);
		if (stageData.stageNumber == 8)		await AdMgr.ShowReview();
		if (stageData.stageNumber >= 10)	await AdMgr.ShowInterstitial();

        if (stageClearExit)
        {
            stageClearExit = false;
            stageClearNext = false;
			await Exit();
            return;
        }
        stageClearExit = false;
        stageClearNext = false;

        if (stageNumber != GameInfoMgr.LastStageNumber)
		{
			StageInfo_UpdateBuffIcons();
			async void StageInfo_UpdateBuffIcons()
			{
				stageInfo_BuffButton.alpha = 0.5f;
				stageInfo_BuffButton.interactable = false;
				await UpdateBuffIcons(stageInfo_SlowBlockIcon, stageInfo_SkillBoostIcon);
				stageInfo_BuffButton.alpha = 1f;
				stageInfo_BuffButton.interactable = true;
			}

			ShowPanel(stageInfoPanel);
			int nextStageNumber = stageNumber + 1;
			stageInfo_StageNumberText.text = string.Format(GetText("ingame_mode_stage"), nextStageNumber);
			Stage nextStageData = StageMgr.LoadStage(nextStageNumber);
			UpdateMissionUiIcons(stageInfo_MissionUIs, nextStageData.missions);
			for (int i = 0; i < nextStageData.missions.Count; i++)
			{
				Mission mission = nextStageData.missions[i];
				MissionUI missionUI = stageInfo_MissionUIs[i];
				switch (mission)
				{
					case Mission_Score missionScore:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_score"), missionScore.targetScore.ToString("N0"));
						break;
					case Mission_Number missionNumber:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_number"), TypeHelper.NumberTypeToString(missionNumber.missionNumberType), missionNumber.targetCount);
						break;
					case Mission_TargetCell missionTargetCell:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_target_cell"), TypeHelper.NumberTypeToString(missionTargetCell.targetNumber), nextStageData.stageCells.Count);
						break;
					case Mission_MergeBlock missionMerge:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_merge_block"), nextStageData.stageBlocks.FindAll(x => x.blockType == BlockType.Target).Count);
						break;
					case Mission_EscapeBlock missionEscape:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_escape_block"), missionEscape.targetCount);
						break;
					case Mission_DestroyDummy missionDummy:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_destroy_dummy"), missionDummy.targetCount);
						break;
					case Mission_DestroyIce missionIce:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_destroy_ice"), missionIce.targetCount);
						break;
					case Mission_CopyBlock missionCopy:
						missionUI.textMain.text = string.Format(GetText("main_stage_mission_copy_block"), missionCopy.targetCount);
						break;
				};
			}
		}
		else
		{
			ShowPanel(stageAllClearPanel);
			PanelPopEffect(stageAllClearPanel);
		}
	}

	async UniTask StageClearAnimation(int coinBefore)
	{
		await UniTask.Delay(700).BP(stageClearToken);
		SoundMgr.PlaySfx(SfxType.PopupClear);
		foreach (MissionCheckIcon missionIcon in stageClear_MissionIcons)
		{
			Image checkIcon = missionIcon.checkIcon;
			checkIcon.gameObject.SetActive(true);
			checkIcon.transform.localScale = Vector3.one * 1.5f;
			await checkIcon.transform.DOScale(1f, 0.5f).SetEase(Ease.InBack).WithCancellation(stageClearToken.Token);
		}
		if (stageClearCoinAmount != 0)
		{
			stageClear_CoinText.text = string.Format("X{0}", stageClearCoinAmount);
			CommonCanvas.coin.Show(false, coinBefore);
			await CommonCanvas.coin.StartCoinParticle(stageClear_CoinText.transform.position, coinBefore);
		}
	}

	async void StageClearAnimationComplete()
	{
		stageClearToken.Cancel();
		await UniTask.NextFrame();
		await UniTask.NextFrame();
		foreach (MissionCheckIcon missionIcon in stageClear_MissionIcons)
		{
			Image checkIcon = missionIcon.checkIcon;
			checkIcon.gameObject.SetActive(true);
			checkIcon.transform.localScale = Vector3.one;
		}
		int coinObtained = stageClearCoinAmount;
		stageClear_CoinText.text = string.Format("X{0}", coinObtained);
	}

	async UniTask StageFail1_Show()
	{
		CommonCanvas.coin.Show(true);
		SoundMgr.PlaySfx(SfxType.PopupFail);
		ShowPanel(stageFail1Panel);
		PanelPopEffect(stageFail1Panel);
		UpdateMissionCheckIcons(stageFail1PanelMissionIcons, missions);
        stageFail1ContinueCoin.SetText((continuePrice * (int)System.Math.Pow(2, nContinue)).ToString());
        for (int i = 0; i < missions.Count; i++)
		{
			Mission mission = missions[i];
			MissionCheckIcon missionIcon = stageFail1PanelMissionIcons[i];
			missionIcon.countText.text = string.Format("X{0}", mission.GetRemainedCount());
        }
		await UniTask.WaitUntil(() => !stageFail1Panel.activeSelf);
		CommonCanvas.coin.Hide();
	}

	async UniTask EndlessFail1_Show()
	{
		CommonCanvas.coin.Show(true);
		BInteger lastHighScore = GetLastHighScore();
		isNewHighScore = score >= lastHighScore;
		if (isNewHighScore)
		{
			UserMgr.SetHighScore(boardSize.x, score);
		}
		switch (gameEnd)
		{
			// 1024T에 도달해서 게임 끝날 때
			case GameEndType.StageClear:
				EndlessFail1_CloseButton();
				break;
			case GameEndType.StageFail:
				string panelText;
				if (isNewHighScore)
				{
					panelText = string.Format(GetText("ingame_unlimit_continue_desc_new"), score.ToString("N0"));
				}
				else
				{
					BInteger remainedScore = lastHighScore - score;
					panelText = string.Format(GetText("ingame_unlimit_continue_desc"), remainedScore.ToString("N0"));
				}
				endlessFail1_Text.text = panelText;
				endlessFail1_ContinueCoin.SetText((continuePrice * (int)System.Math.Pow(2, nContinue)).ToString());
				SoundMgr.PlaySfx(SfxType.PopupFail);
				ShowPanel(endlessFail1Panel);
				PanelPopEffect(endlessFail1Panel);
				break;
		}
		await UniTask.WaitUntil(() => !endlessFail1Panel.activeSelf);
		CommonCanvas.coin.Hide();
	}

	async UniTask EndlessFail2_PanelAnimation()
	{
#if SGS || AWS || MS
        endlessFail2_MyRanking.gameObject.SetActive(false);
		var btnPos = endlessFail2_RetryButton.anchoredPosition;
		btnPos.y += 100f;
        endlessFail2_RetryButton.anchoredPosition = btnPos;
#else
        var rankData = new RankData();
		rankData.nickname = (UserMgr.IsLoginUser()) ? UserMgr.Nickname : "-";
        endlessFail2_MyRanking.Init(rankData);
		endlessFail2_MyRanking.SetLoadingPanel(true);
		endlessFail2LoadRankingToken = new CancellationTokenSource();
		EndlessFail2LoadMyRank(endlessFail2LoadRankingToken.Token);
#endif

		endlessFail2PanelAnimating = true;
		endlessFail2_HighScoreText.text = "0";
		endlessFail2_NewScoreEffect1.SetActive(false);
		endlessFail2_NewScoreEffect2.SetActive(false);
		endlessFail2_NewScoreEffect3.SetActive(false);

		if (isNewHighScore)
		{
            SoundMgr.PlaySfx(SfxType.HighestNumber);
            Instantiate(trumpetEffectPrefab, endlessFail2Panel.transform);

			BInteger logScore = (score / 10000L + 1) * 10000;
			logScore = BInteger.Min(100000L, logScore);
#if !MS && !AWS
            switch (boardSize.x)
			{
                case 4: FirebaseAnalytics.LogEvent("Point_4X7_100000", "Po_4x7_1", logScore.ToString()); break;
				case 6: FirebaseAnalytics.LogEvent("Point_6X7_100000", "Po_6X7_1", logScore.ToString()); break;
			}
#endif
		}
		endlessFail2_HighScoreText.transform.position = endlessFail2_ScoreTextStartPosition.position;
		await UniTask.Delay(300).BP(endlessFail2Token);

        SoundMgr.PlaySfx(SfxType.IncreaseResultScore);
        float scoreAnimated = 0;
		float scoreEnd = BigIntToFloat(score);
		float animationTime = GetScoreAnimationTime(0);
		await DOTween.To(() => scoreAnimated, x => scoreAnimated = x, scoreEnd, animationTime * 5f).SetEase(Ease.Linear).OnUpdate(() =>
		{
            endlessFail2_HighScoreText.text = scoreAnimated.ToString("N0");
		}).WithCancellation(endlessFail2Token.Token);
		await UniTask.Delay(400).BP(endlessFail2Token);
		await endlessFail2_HighScoreText.transform.DOMove(endlessFail2_ScoreTextEndPosition.position, 0.7f).WithCancellation(endlessFail2Token.Token);
		await UniTask.Delay(400).BP(endlessFail2Token);
		endlessFail2_NewScoreEffect1.SetActive(isNewHighScore);
		endlessFail2_NewScoreEffect2.SetActive(isNewHighScore);
		endlessFail2_NewScoreEffect3.SetActive(isNewHighScore);
		if (isNewHighScore)
		{
			SoundMgr.PlaySfx(SfxType.PopupClear);
		}
		Block block = CreateNewBlockUI(GetHighestBlock().data, endlessFail2_BlockPosition);
		block.transform.localScale = Vector3.one * 1.11f;
		await block.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 0, 1f).WithCancellation(endlessFail2Token.Token);
		endlessFail2PanelAnimating = false;
    }

	async void EndlessFail2LoadMyRank(CancellationToken ct)
	{
		if (!UserMgr.IsLoginUser()) return;

		await UniTask.WaitUntil(() => bIsCompleteSaveLeaderboard, PlayerLoopTiming.Update, ct);
		bIsCompleteSaveLeaderboard = false;
		if(ct.IsCancellationRequested)
		{
            endlessFail2_MyRanking.SetLoadingPanel(false);
            return;
        }

		endlessFail2MyRank = null;

		bool bIsInf47 = (boardSize.x == 4);
        var type = (bIsInf47) ? LeaderboardType.INFINITE47 : LeaderboardType.INFINITE67;
		var round = (bIsInf47) ? GameInfoMgr.LeaderboardRound47 : GameInfoMgr.LeaderboardRound67;
		(int resultCode, JObject rankInfo) = await GetMyLeaderBoardInfo(type, round, ct);
		if (resultCode < 0 || rankInfo == null)
		{
            endlessFail2_MyRanking.SetLoadingPanel(false);
            return;
        }

        endlessFail2MyRank = rankInfo["rank_info"];
        if (endlessFail2MyRank != null && endlessFail2MyRank.HasValues)
        {
            string userDataStr = string.Format("{0}", endlessFail2MyRank["user_data"].ToString());
            JObject userDataObj = JObject.Parse(userDataStr);
            var data = new RankData
            {
                rank = int.Parse(endlessFail2MyRank["rank"].ToString()),
                nickname = userDataObj["nickname"].ToString(),
                score = long.Parse(endlessFail2MyRank["score"].ToString()),
                highestBlockNumber = (NumberType)int.Parse(userDataObj["number_type"].ToString())
            };
            endlessFail2_MyRanking.Init(data);
			endlessFail2_MyRanking.SetLoadingPanel(false);
        }
        endlessFail2_MyRanking.SetLoadingPanel(false);
    }

	async void EndlessFail2PanelAnimationComplete()
	{
        endlessFail2Token.Cancel();
		await UniTask.NextFrame();
		endlessFail2PanelAnimating = false;
		endlessFail2_HighScoreText.transform.position = endlessFail2_ScoreTextEndPosition.position;
		endlessFail2_HighScoreText.text = score.ToString("N0");
		if (!endlessFail2_BlockPosition.GetComponentInChildren<Block>())
		{
			Block block = CreateNewBlockUI(GetHighestBlock().data, endlessFail2_BlockPosition);
			block.transform.localScale = Vector3.one * 1.11f;
		}
		endlessFail2_NewScoreEffect1.SetActive(isNewHighScore);
		endlessFail2_NewScoreEffect2.SetActive(isNewHighScore);
		endlessFail2_NewScoreEffect3.SetActive(isNewHighScore);
		if (isNewHighScore)
		{
			SoundMgr.PlaySfx(SfxType.PopupClear);
		}
	}

	async UniTask ShowHighestNumberPopup(Block highestBlock, int coinAmount)
	{
		endlessCoinAccumulated += coinAmount;
		int coinBefore = UserMgr.CoinTotal;
		UserMgr.Coin += coinAmount;
		UserMgr.PiggyBankCoin += coinAmount;
		UserMgr.SaveUserInfo();
		UserMgr.UploadUserInfo_Coins().Forget();
#if !MS && !AWS
		Util.LogFirebaseEventWithUnityLog("first_number");
#endif

		SoundMgr.PlaySfx(SfxType.PopupHighestNumber);
		userMovePause = true;
		await UniTask.Delay(600);
		CommonCanvas.coin.Show(false, coinBefore);
		AdMgr.HideBannerAd();
		highestNumberPopup.SetActive(true);
		highestNumber_CoinText.text = coinAmount.ToString();
		Block uiBlock = CreateNewBlockUI(highestBlock.data, highestNumber_BlockPosition);
		highestNumber_Parent.alpha = 0f;
		highestNumber_Parent.transform.localScale = Vector3.one * 0.9f;
		highestNumber_Parent.DOFade(1f, 0.2f).SetEase(Ease.Linear).ToUniTask().Forget();
		await UniTask.Delay(700);
		var type = (boardSize.x == 4) ? LeaderboardType.INFINITE47 : LeaderboardType.INFINITE67;
		int rank = GameInfoMgr.GetRank(type, score);
		Debug.Log($"rank: {rank}");
		if(rank != 0)
		{
            if (rank > N_DETAILED_RANKING_LIMIT)
			{
				// 획득한 랭킹이 랭킹 패널에 등록 불가능한 점수라면 관련 플래그를 꺼준다..
                isInRanking = false;
            }

			// 랭킹을 보여준다.
        }
		await CommonCanvas.coin.StartCoinParticle(highestNumber_CoinText.transform.position, coinBefore);
		highestNumberPopupNext = false;
		await UniTask.WaitUntil(() => highestNumberPopupNext);
		highestNumberPopupNext = false;
		Destroy(uiBlock.gameObject);
		CommonCanvas.coin.Hide();
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		highestNumberPopup.SetActive(false);
		bool reviewed = PlayerPrefs.GetInt(PrefsKey.Reviewed, 0) == 1;
		if (highestBlock.data.numberType >= NumberType.N1024)
		{
			await AdMgr.ShowReview();
		}
		if (reviewed)
		{
			await AdMgr.ShowInterstitial();
		}
		AdMgr.ShowBannerAd();

		await TouchToStart();
		userMovePause = false;
	}

	async UniTask UpdateBuffIcons(Image slowBlockIcon, Image skillBoostIcon)
	{
		inputBlocker.SetActive(true);
		(bool slowBlockBuff, bool skillBoostBuff) = await UserMgr.AreBuffsOn();
		inputBlocker.SetActive(false);
		slowBlockIcon.color = Color.white.WithAlpha(slowBlockBuff ? 1f : 0.4f);
		skillBoostIcon.color = Color.white.WithAlpha(skillBoostBuff ? 1f : 0.4f);
	}

	void UpdateMissionCheckIcons(List<MissionCheckIcon> missionCheckIcons, List<Mission> missionDatas)
	{
		for (int i = 0; i < missionCheckIcons.Count; i++)
		{
			MissionCheckIcon missionIcon = missionCheckIcons[i];
			if (i < missionDatas.Count)
			{
				missionIcon.gameObject.SetActive(true);
				Mission mission = missionDatas[i];
				mission.UpdateUI(missionIcon.image1, missionIcon.image2, uiBlocks);
			}
			else
			{
				missionIcon.gameObject.SetActive(false);
			}
		}
	}

	void ShowPanel(GameObject panel)
	{
		panel.transform.Find("Background").GetComponent<Image>().sprite = UserMgr.GetUserSkinBG();
		panel.SetActive(true);
	}


}
