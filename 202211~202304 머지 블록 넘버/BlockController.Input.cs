using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if !MS && !AWS
using Firebase.Analytics;
#endif

using static GameInterface;
using static ServerApi;
using static SoundManager;
using static StartBoosterItemUI;
using static CommonDefines;

public partial class BlockController
{
	/// <summary>
	/// BoardInputArea의 EventTrigger에서 호출
	/// </summary>
	public void OnPointerDown(BaseEventData _eventData)
	{
		PointerEventData eventData = (PointerEventData)_eventData;
		UpdatePointerXCoord(eventData.position);
		isDragging = true;
		pointerDownThisBlock = true;
	}

	/// <summary>
	/// BoardInputArea의 EventTrigger에서 호출
	/// </summary>
	public void OnDrag(BaseEventData _eventData)
	{
		PointerEventData eventData = (PointerEventData)_eventData;
		UpdatePointerXCoord(eventData.position);
	}

	void UpdatePointerXCoord(Vector2 screenPosition)
	{
		Vector3 touchPositionWorld = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCanvas.planeDistance));
		Cell closestCell = GetClosestCell(touchPositionWorld);
		pointerXCoord = closestCell.data.coord.x;
	}

	/// <summary>
	/// BoardInputArea의 EventTrigger에서 호출.
	/// </summary>
	public void OnPointerUp(BaseEventData eventData)
	{
		isDragging = false;
		if (pointerDownThisBlock && pointerXCoord == newBlockXCoord)
		{
			SoundMgr.PlaySfx(SfxType.BlockDrop);
			fireBlockTrigger = true;
		}
	}

	public async void ItemDivideButton()
	{
		Debug.Log("bOnItemCreateProcess: " + bOnCreateItemProcess);
		await UniTask.WaitUntil(() => !bOnCreateItemProcess);
		bOnCreateItemProcess = true;

		if (gameState != GameState.Drop && gameState != GameState.Pause)
		{
			bOnCreateItemProcess = false;
            return;
        }
        await ShowTutorial(TutorialPopup.tutorialItem);
        if (usingItemType != null)
        {
            if (usingItemType == ItemType.Divide)
            {
                bOnCreateItemProcess = false;
                return;
            }

            itemCancelTrigger = true;
            await UniTask.WaitUntil(() => !itemCancelTrigger);
        }

        bool bCanUseFreeItem = FreeItemAvailable(ItemType.Divide);
        if (!bCanUseFreeItem && UserMgr.ItemDivideCount <= 0)
		{
			userMovePause = true;
			await OpenItemPurchasePanel(ItemType.Divide);
			userMovePause = false;
		}
		if (!bCanUseFreeItem && UserMgr.ItemDivideCount <= 0)
		{
			bOnCreateItemProcess = false;
            return;
        }
		UseItemBlock(ItemType.Divide);
    }

    public async void ItemAddButton()
	{
        Debug.Log("bOnItemCreateProcess: " + bOnCreateItemProcess);
        await UniTask.WaitUntil(() => !bOnCreateItemProcess);
        bOnCreateItemProcess = true;

        if (gameState != GameState.Drop && gameState != GameState.Pause)
		{
			Debug.Log("gameState: " + gameState);
			bOnCreateItemProcess = false;
            return;
        }
        await ShowTutorial(TutorialPopup.tutorialItem);
        if (usingItemType != null)
        {
            if (usingItemType == ItemType.Add)
            {
                bOnCreateItemProcess = false;
                return;
            }

            Debug.Log("Wait for itemCancelTrigger");
            itemCancelTrigger = true;
            await UniTask.WaitUntil(() => !itemCancelTrigger);
            Debug.Log("End wait for itemCancelTrigger");
        }

        bool bCanUseFreeItem = FreeItemAvailable(ItemType.Add);
        if (!bCanUseFreeItem && UserMgr.ItemAddCount <= 0)
		{
			userMovePause = true;
			await OpenItemPurchasePanel(ItemType.Add);
			userMovePause = false;
		}
		if (!bCanUseFreeItem && UserMgr.ItemAddCount <= 0)
		{
			bOnCreateItemProcess = false;
            return;
        }
		UseItemBlock(ItemType.Add);
	}

	public async void ItemHammerButton()
	{
        Debug.Log("bOnItemCreateProcess: " + bOnCreateItemProcess);
        await UniTask.WaitUntil(() => !bOnCreateItemProcess);
        bOnCreateItemProcess = true;

        if (gameState != GameState.Drop && gameState != GameState.Pause)
		{
			bOnCreateItemProcess = false;
            return;
        }
        await ShowTutorial(TutorialPopup.tutorialItem);
		if (usingItemType != null)
		{
            if (usingItemType == ItemType.Hammer)
            {
                bOnCreateItemProcess = false;
                return;
            }

            userMovePause = false;
            itemCancelTrigger = true;
            await UniTask.WaitUntil(() => !itemCancelTrigger);
        }
		bOnCreateItemProcess = false;

        bool bCanUseFreeItem = FreeItemAvailable(ItemType.Hammer);
        if (!bCanUseFreeItem && UserMgr.ItemHammerCount <= 0)
		{
			userMovePause = true;
			await OpenItemPurchasePanel(ItemType.Hammer);
			userMovePause = false;
		}
		if (!bCanUseFreeItem && UserMgr.ItemHammerCount <= 0) return;
        UseItemHammer();
	}

	public void ItemCancel_CancelButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		itemCancelTrigger = true;
	}

	public void ItemPurchase_CloseButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		itemPurchasePanel.SetActive(false);
	}

	public async void ItemPurchase_PurchaseButton()
	{
		int price = itemPurchasePanelItemType switch
		{
			ItemType.Divide => ITEM_DIVIDE_PRICE,
			ItemType.Add => ITEM_ADD_PRICE,
			ItemType.Hammer => ITEM_HAMMER_PRICE,
			_ => int.MaxValue
		};
		string productMemo = itemPurchasePanelItemType switch
		{
			ItemType.Divide => "Buy DivideBlock",
			ItemType.Add => "Buy JokerBlock",
			ItemType.Hammer => "Buy HammerBlock",
			_ => throw new NotImplementedException()
		};
		bool purchased = await UserMgr.UseCoin(price, true, productMemo);
		if (purchased)
		{
			int itemGot = 3;
			int itemCount = UserMgr.GetItemCount(itemPurchasePanelItemType);
			UserMgr.SetItemCount(itemPurchasePanelItemType, itemCount + itemGot);
			await UserMgr.UploadUserInfo_Coins(true);
			UpdateItemButtons();
			CommonCanvas.getItemPanel.EnableGetItemPopup(0, itemPurchasePanelItemType == ItemType.Divide ? itemGot : 0, itemPurchasePanelItemType == ItemType.Add ? itemGot : 0, itemPurchasePanelItemType == ItemType.Hammer ? itemGot : 0, false);
			itemPurchasePanel.GetComponent<CanvasGroup>().alpha = 0f;
			await UniTask.WaitUntil(() => !CommonCanvas.getItemPanel.gameObject.activeSelf);
			itemPurchasePanel.SetActive(false);
#if !MS && !AWS
            switch (itemPurchasePanelItemType)
            {
                case ItemType.Divide:
                    FirebaseAnalytics.LogEvent("Division_GOLD");
                    break;
                case ItemType.Add:
                    FirebaseAnalytics.LogEvent("Plus_GOLD");
                    break;
                case ItemType.Hammer:
                    FirebaseAnalytics.LogEvent("Hammer_GOLD");
                    break;
            }
#endif
        }
        else
		{
			UpdateItemPurchasePanel();
		}
    }

    public void PauseButton()
	{
		if (isMapToolMode) return;
		ShowPausePanel();
	}

	public async void ExitButton()
	{
		bool? yes = null;
		ModalMgr.ShowModal(GetText("common_notice_title"), GetText("common_notice_giveup_game").Replace("\\n", "\n"), () => yes = true, () => yes = false);
		await UniTask.WaitUntil(() => yes != null);
		if (yes == true)
		{
			switch (gameMode)
			{
				case GameMode.Endless:
					AutoSaveStageToDisk();
					break;
			}
#if !MS && !AWS
			FirebaseAnalytics.LogEvent("STAGE_Out");
#endif
			Exit().Forget();
		}
	}

	public async void RetryButton()
	{
		bool? yes = null;
		ModalMgr.ShowModal(GetText("common_notice_title"), GetText("game_notice_retry_confirm"), () => yes = true, () => yes = false);
		await UniTask.WaitUntil(() => yes != null);
		if (yes == true)
		{
			Retry();
		}
	}

	public void Pause_SkillAutoToggle()
	{
		SoundMgr.PlaySfx(SfxType.ButtonInput);
		UserMgr.bIsOnAutoSkill = !UserMgr.bIsOnAutoSkill;
        Pause_UpdateButtons();
        UpdateSkillButton();

#if !MS && !AWS
        if (UserMgr.bIsOnAutoSkill) FirebaseAnalytics.LogEvent("SKILL_On");
        else						FirebaseAnalytics.LogEvent("SKILL_Off");
#endif
    }

	public void Pause_BgmButton()
	{
		SoundMgr.ToggleBgmSound();
		Pause_UpdateButtons();
	}

	public void Pause_SfxButton()
	{
		SoundMgr.ToggleSfxSound();
		Pause_UpdateButtons();
	}

	public void Pause_CloseButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		HidePausePanel(true);
	}

	public async void EndlessStart_StartBlock512Button()
	{
		if (UserMgr.CoinTotal < startBlock512Price)
		{
			await CommonCanvas.shop.Show();
			return;
		}
		startBlock512PurchaseDemand = !startBlock512PurchaseDemand;
		UpdateStartPanelItemUI(startBlock512ItemUI, startBlock512PurchaseDemand);
	}

	void UpdateStartPanelItemUI(StartBoosterItemUI ui, bool isChecked)
	{
		UpdateStartPanelItemUI(ui, isChecked ? ButtonState.Checked : ButtonState.Unchecked);
	}

	void UpdateStartPanelItemUI(StartBoosterItemUI ui, ButtonState state)
	{
		bool isChecked = state != ButtonState.Unchecked;
		ui.buttonImage.GetComponent<Button>().interactable = state != ButtonState.Freeze;
		ui.buttonImage.color = isChecked ? endlessStartPanelButtonCheckColor : endlessStartPanelButtonUncheckColor;
		ui.checkIcon.gameObject.SetActive(isChecked);
		ui.priceText.gameObject.SetActive(!isChecked);
	}

	public void EndlessStart_StartButton()
	{
		touchToStartTrigger = true;
	}

	public void TouchToStartButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonInput);
		touchToStartTrigger = true;
	}

	public void StageClear_ExitButton()
	{
		StageClearAnimationComplete();
		stageClearExit = true;
    }

	public void StageClear_NextStageButton()
	{
		StageClearAnimationComplete();
		stageClearNext = true;
	}

	public void StageFail1PanelCloseButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		UserMgr.UpdateQuestCount(QuestType.PlayStage, 1);
		stageFail1Panel.SetActive(false);
		ShowPanel(stageFail2Panel);
		UpdateMissionCheckIcons(stageFail2PanelMissionIcons, missions);
		for (int i = 0; i < missions.Count; i++)
		{
			Mission mission = missions[i];
			MissionCheckIcon missionIcon = stageFail2PanelMissionIcons[i];
			missionIcon.countText.text = string.Format("X{0}", mission.GetRemainedCount());
		}
	}

	public async void StageFail1PanelContinueButton()
	{
		int price = continuePrice * (int)Math.Pow(2, nContinue);
        bool continuePurchased = await UserMgr.UseCoin(price, true, "Buy StageContinue");
		if (!continuePurchased) return;
		stageFail1Panel.SetActive(false);
#if !MS && !AWS
		Util.LogFirebaseEventWithUnityLog($"Continue_GOLD{nContinue + 1}");
#endif
        FailPanelContinue();
	}

	public void StageFail2_RetryButton()
	{
		Retry();
	}

	public void StageFail2_ExitButton()
	{
		Exit().Forget();
	}

	public async void EndlessFail1_ContinueButton()
	{
		int price = continuePrice * (int)Math.Pow(2, nContinue);
		bool continuePurchased = await UserMgr.UseCoin(price, true, "Buy StageContinue");
		if (!continuePurchased) return;
		endlessFail1Panel.SetActive(false);
#if !MS && !AWS
		Util.LogFirebaseEventWithUnityLog($"Continue_GOLD{nContinue + 1}");
#endif
        FailPanelContinue();
	}

	public void EndlessFail1_CloseButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		UserMgr.UpdateQuestCount(QuestType.PlayUnlimit, 1);
		UserMgr.SaveUserInfo();
		if (UserMgr.IsLoginUser() && score > 0)
		{
			LeaderboardType leaderboardType = boardSize.x == 4 ? LeaderboardType.INFINITE47 : LeaderboardType.INFINITE67;
			SaveToLeaderboard(leaderboardType, score, highestNumberType).Forget();
		}
		else
		{
			bIsCompleteSaveLeaderboard = true;
		}

		endlessFail1Panel.SetActive(false);
		ShowPanel(endlessFail2Panel);
		endlessFail2Token = new();
		EndlessFail2_PanelAnimation().BP(endlessFail2Token).Forget();
	}

	public void EndlessFail2_ExitButton()
	{
		if (endlessFail2PanelAnimating)
		{
			EndlessFail2PanelAnimationComplete();
			return;
		}
		else
		{
#if !SGS && !AWS && !MS
            endlessFail2LoadRankingToken.Cancel();
#endif
            Exit().Forget();
		}
	}

	public void EndlessFail2_RetryButton()
	{
		if (endlessFail2PanelAnimating)
		{
			EndlessFail2PanelAnimationComplete();
			return;
		}
		else
		{
#if !SGS && !AWS && !MS
            endlessFail2LoadRankingToken.Cancel();
#endif
            Retry();
		}
	}

	public void SkillAutoButton()
	{
        SoundMgr.PlaySfx(SfxType.ButtonInput);

        bool bIsOnAutoSkill = !UserMgr.bIsOnAutoSkill;
		UserMgr.bIsOnAutoSkill = bIsOnAutoSkill;

        skillAuto_Text.color = (bIsOnAutoSkill) ? Color.white : new Color32(255, 255, 255, 128);
        skillButtonPoint.GetChild(0).gameObject.SetActive(bIsOnAutoSkill);
        skillAutoButton.transform.GetChild(0).gameObject.SetActive(bIsOnAutoSkill);
        skillAutoButton.transform.GetChild(1).gameObject.SetActive(!bIsOnAutoSkill);

#if !MS && !AWS
        if (UserMgr.bIsOnAutoSkill) FirebaseAnalytics.LogEvent("SKILL_On");
        else						FirebaseAnalytics.LogEvent("SKILL_Off");
#endif
    }

	public void SkillButton()
	{
		if (usingItemType != null) return;
		if (gameState != GameState.Drop) return;
		if (skillGauge >= 1f)
		{
			ActivateSkill().Forget();
			return;
		}
#if CHEAT
		if (CheatPanel.unlimitSkill)
		{
			ActivateSkill().Forget();
		}
#endif
	}

	public void HighestNumber_NextButton()
	{
		highestNumberPopupNext = true;
	}

	public async void ShowBuffPanelButton()
	{
		CommonCanvas.gameBuffPanel.gameObject.SetActive(true);
		await UniTask.WaitUntil(() => !CommonCanvas.gameBuffPanel.gameObject.activeSelf);
		UpdateBuffIcons(endlessStart_SlowBlockIcon, endlessStart_SkillBoostIcon).Forget();
		UpdateBuffIcons(stageInfo_SlowBlockIcon, stageInfo_SkillBoostIcon).Forget();
	}

	public void StageInfo_NextButton()
	{
		StageMgr.StartStage(stageNumber + 1);
	}

	public void StageInfo_ExitButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		Exit().Forget();
	}

	public void StageAllClear_CheckUpdateButton()
	{
		Application.OpenURL(CommonDefines.STORE_LINK);
	}

	public void StageAllClear_ExitButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonClose);
		UserMgr.PrevPlayMode = UserInfoManager.PlayMode.INFINITE47;
		Exit().Forget();
	}

	public async void HelpButton()
	{
		SoundMgr.PlaySfx(SfxType.ButtonInput);
		Time.timeScale = 0f;
		AdMgr.HideBannerAd();
		await CommonCanvas.missionInfo.Show();
		AdMgr.ShowBannerAd();
		await TouchToStart();
		Time.timeScale = 1f;
	}

	public void OnBackButton()
	{
        switch (gameEnd)
        {
            case GameEndType.Playing:
                if (endlessModeStartPanel.activeSelf)
                {
                    EndlessStart_StartButton();
                }
                else if (pausePanel.activeSelf)
                {
					Pause_CloseButton();
				}
                else
                {
                    PauseButton();
                }
                break;
            case GameEndType.StageClear:
				if(stageClearPanel.activeSelf)
				{
					StageClear_ExitButton();
				}
                break;
            case GameEndType.StageFail:
				if(gameMode == GameMode.Endless)
				{
                    if (endlessFail1Panel.activeSelf)
                    {
                        EndlessFail1_CloseButton();
                    }
                    else if (endlessFail2Panel.activeSelf)
                    {
                        EndlessFail2_ExitButton();
                    }
                    else if (usingItemType != null)
                    {
                        ItemCancel_CancelButton();
                    }
                }
				else
				{
					if(stageFail1Panel.activeSelf)
					{
						StageFail1PanelCloseButton();
					}
					else if(stageFail2Panel.activeSelf)
					{
						StageFail2_ExitButton();
					}
				}
                break;
        }
    }
}
