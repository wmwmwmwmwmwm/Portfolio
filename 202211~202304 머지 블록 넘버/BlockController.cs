using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if !MS && !AWS
using Firebase.Analytics;
#endif

using static Column;
using static GameInterface;
using static SoundManager;
using static CommonDefines;

using Random = UnityEngine.Random;

public partial class BlockController : MonoBehaviour, IBackButton
{
	public static BlockController instance;

	void Awake()
	{
		instance = this;
		boardCellsLinear = new();
		boardBlocks = new();
		connectors = new();
		uiBlocks = new();
		iceBlocksToDestroyPair = new();
		escapeBlocksToDestroy = new();
		currentBlockRaycastHits = new RaycastHit2D[1];
	}

	void Start()
	{
		int bgmTypeRandom = Random.Range((int)BgmType.Game0, (int)BgmType.Game3 + 1);
		SoundMgr.PlayBgm((BgmType)bgmTypeRandom);
		mainCanvas.worldCamera = mainCamera = Camera.main;
		if (isMapToolMode) return;
#if DEV_TEST
		stageStartTime = Time.time;
#endif

		Image bgOverlay = GameObject.Find("BgOverlay").GetComponent<Image>();
		bgOverlay.sprite = UserMgr.GetUserSkinBG();
		Image bgOverlayPattern = GameObject.Find("BgOverlayPattern").GetComponent<Image>();
		bgOverlayPattern.sprite = UserMgr.GetUserSkinBgPattern();

#if UNITY_ANDROID
		CommonCanvas.onFoldCallback += ProcessAdjustGameAreaSize;
#endif

		PlayGame(true).Forget();
	}

	void OnDestroy()
	{
#if UNITY_ANDROID
		CommonCanvas.onFoldCallback -= ProcessAdjustGameAreaSize;
#endif
		DOTween.KillAll();
	}

	void OnApplicationFocus(bool bIsFocused)
	{
		if (!bIsFocused)
		{
			if (isMapToolMode || gameEnd != GameEndType.Playing || gameState == GameState.None) return;
			ShowPausePanel();
			if (gameMode == GameMode.Endless)
			{
				AutoSaveStageToDisk();
			}
		}
	}

	public async UniTask PlayGame(bool resetGame)
	{
		await UniTask.WaitUntil(() => mainGameTask.Status != UniTaskStatus.Pending);
		if (resetGame)
		{
			await ResetGame();
        }

        if (!isMapToolMode)
        {
            await CommonCanvas.screenTransition.FadeIn();

			// 터치하여 시작 표시
			switch (gameMode)
			{
				case GameMode.Endless:
					if (resetGame && !isAutoSavedStage)
					{
						await EndlessModeStartPanel();
					}
					else
					{
						await TouchToStart();
					}
					break;
				case GameMode.Stage:
					await TouchToStart();
					break;
			}

			// 버프 적용
			(bool slowDrop, bool increaseSkill) = await UserMgr.AreBuffsOn();
			slowDropSpeedEnabled = slowDrop;
			increaseSkillGaugeEnabled = increaseSkill;
			buffIconSlowDrop.color = slowDrop ? Color.white : Color.white.WithAlpha(0.4f);
			buffIconSkillGauge.color = increaseSkill ? Color.white : Color.white.WithAlpha(0.4f);

			// 512블록 아이템 구매 적용
			if (startBlock512PurchaseDemand)
			{
				bool startBlock512Purchased = await UserMgr.UseCoin(startBlock512Price, false, "Buy StartBlock512");
				if (startBlock512Purchased)
				{
					ConsumeStartBlock512Demand();
#if !MS && !AWS
                    FirebaseAnalytics.LogEvent("Number_GOLD");
#endif
				}
			}
		}

		// 메인 게임 루프 시작
		AdMgr.ShowBannerAd();
		gameCancelToken = new();
		mainGameTask = GameStart().BP(gameCancelToken);
		await mainGameTask;
		AdMgr.HideBannerAd();

		if (isMapToolMode) return;
		if (gameEnd == GameEndType.Playing) return;

		// 게임 결과
		switch (gameEnd)
		{
			case GameEndType.StageFail:
				await StageFailAnimation();
				break;
		}
		switch (gameMode)
		{
			case GameMode.Endless:
				DeleteSavedStage();
				EndlessFail1_Show().Forget();
				break;
			case GameMode.Stage:
				switch (gameEnd)
				{
					case GameEndType.StageClear:
						StageClearProcess().Forget();
						break;
					case GameEndType.StageFail:
						StageFail1_Show().Forget();
						break;
				}
				break;
		}
	}

	public void StopGame()
	{
		DOTween.KillAll();
		gameCancelToken?.Cancel();
	}

	async UniTask GameStart()
	{
		SetNextBlockData();

		while (true)
		{
			// 자동 저장
			Debug.Log("Auto Saving & Check Game State");
			AutoSaveStage();
			await OnBeforeCreateBlock();

			blockRecreateTrigger = true;
			while (blockRecreateTrigger)
			{
				// 기존 블록 삭제
				Debug.Log("Recreate Block");
				if (currentDropBlock)
				{
					RemoveBlock(currentDropBlock, false, false);
				}

				// 블록 생성
				combo = 0;
				CreateNewBlock();
				SetNextBlockData();
                blockRecreateTrigger = false;

				// 터치로 블록 이동
				Debug.Log("Wait user input");
				boardInputArea.gameObject.SetActive(true);
				await BlockUserMove();
				boardInputArea.gameObject.SetActive(false);
			}

			// 낙하형 아이템을 사용중이라면 개수가 차감될때까지 기다린다.
			if(usingItemType == ItemType.Divide || usingItemType == ItemType.Add)
				await DecreaseItemCount(usingItemType.Value);

			// 블록 떨어트림
			if (currentDropBlock)
			{
				Cell closestCell = GetClosestCell(currentDropBlock.thisRigidbody.position);
				Cell emptyCell = null;
				for (int y = closestCell.data.coord.y; y >= 0; y--)
				{
					Cell cell = GetBoardCell(new(closestCell.data.coord.x, y));
					if (!cell.accupiedBlock || cell.accupiedBlock == currentDropBlock)
					{
						emptyCell = cell;
					}
					else
					{
						break;
					}
				}
				SetBlockToCell(emptyCell, currentDropBlock);
				ColumnAnimation(currentDropBlock.data.coord.x, ColumnState.Drop);
				await currentDropBlock.transform.DOMoveY(emptyCell.transform.position.y, blockAnimationSpeed * 2f).SetSpeedBased().SetEase(blockMoveEase).OnComplete(() => {
					BlockDropEffect(currentDropBlock);
					SoundMgr.PlaySfx(SfxType.BlockFloor);
				}).WithCancellation(gameCancelToken.Token);
			}
			currentDropBlock = null;

			// 병합
			await BoardProcess();

			if (gameCancelToken.IsCancellationRequested) return;
		}
	}

	async UniTask BoardProcess()
	{
		gameState = GameState.Merge;
		foreach (Block block in boardBlocks)
		{
			if (block.mergePriority && block != currentDropBlock)
			{
				block.mergePriority = false;
				block.mergedTurn = 1;
			}
			else
			{
				block.mergedTurn = 0;
			}
			block.movedTurn = 0;
		}
		blockTurn = 0;
		checkBoardNeed = true;
		while (checkBoardNeed)
		{
			while (checkBoardNeed)
			{
				checkBoardNeed = false;
				blockTurn++;
				await MergeBlocks();
				await DropBoardBlocks();
				UpdateMissions();
			}
			await DropComplete();
			await DropBoardBlocks();
			UpdateMissions();
		}
		gameState = GameState.None;
		usingItemType = null;

		CheckTargetCellMissionEffect();
		if (skillGauge >= 1f && UserMgr.bIsOnAutoSkill)
		{
			await ActivateSkill();
		}
		if (CheckGameEnd())
		{
			await UniTask.WaitUntil(() => missionEffectCount == 0);
			StopGame();
		}
	}

	async UniTask BlockUserMove()
	{
		float stickFloorTimer = blockDropStickTime;
		pointerXCoord = newBlockXCoord;
		fireBlockTrigger = false;
		while (true)
		{
			if (!currentDropBlock) return;
			if (userMovePause)
			{
				gameState = GameState.Pause;
				await UniTask.NextFrame();
				fireBlockTrigger = false;
				continue;
			}

			gameState = GameState.Drop;

			// 블록 X축 이동
			int blockDeltaX = pointerXCoord - newBlockXCoord;
			if (blockDeltaX != 0)
			{
				Vector2 castDirection = blockDeltaX > 0 ? Vector2.right : Vector2.left;
				float castDistance = (Mathf.Abs(blockDeltaX) - 1) * gridCellSize.x + blockPadding * 5f;
				int hit = currentDropBlock.thisCollider.Cast(castDirection, currentBlockRaycastHits, castDistance);
				if (hit == 0)
				{
					newBlockXCoord = pointerXCoord;
				}
			}

            // 낙하 속도 결정.
            float blockSpeed = Mathf.Min(BASE_FALL_SPEED + (spawnBlockCount / time), 90f);
			if (warningImage.gameObject.activeSelf || gameMode == GameMode.Stage || boardSize.x != 4)
			{
                blockSpeed = 20f;	// 위험 상황 or 스테이지 모드 or 6x7모드에선 낙하 속도를 20으로 고정한다.
            }
			blockSpeed *= slowDropSpeedEnabled ? 0.5f : 1f;
			if (currentDropBlock.blockTypeInGame is BlockType.Divide or BlockType.Joker)
			{
				blockSpeed = 0f;
            }

            // 블록 Y축 이동.
            Vector2 blockPosition = currentDropBlock.thisRigidbody.position;
			Cell bottomCell = GetBoardCell(new(newBlockXCoord, 0));
			int downHit = currentDropBlock.thisCollider.Cast(Vector2.down, currentBlockRaycastHits, blockSpeed * fixedDeltaTime + 5f);
			if (downHit > 0)
			{
				stickFloorTimer -= fixedDeltaTime;
				Block bottomBlock = currentBlockRaycastHits[0].collider.GetComponent<Block>();
				blockPosition.y = GetBoardCell(new(newBlockXCoord, bottomBlock.data.coord.y + 1)).transform.position.y;
			}
			else if (currentDropBlock.transform.position.y < bottomCell.transform.position.y + 5f)
			{
				stickFloorTimer -= fixedDeltaTime;
				blockPosition.y = bottomCell.transform.position.y;
			}
			else
			{
				stickFloorTimer = blockDropStickTime;
				if (AnimationDone(currentDropBlock.thisAnimator, 0.99f))
				{
					blockPosition += blockSpeed * deltaTime * Vector2.down;
				}
			}
			blockPosition.x = GetBoardCell(new(newBlockXCoord, 0)).transform.position.x;
			currentDropBlock.thisRigidbody.position = blockPosition;

			ColumnAnimation(pointerXCoord, isDragging ? ColumnState.Touch : ColumnState.None);

			if (fireBlockTrigger || (stickFloorTimer < 0f && blockSpeed > 0f) || blockRecreateTrigger) break;
			if (gameCancelToken.IsCancellationRequested) return;

			await UniTask.WaitForFixedUpdate();
		}
		fireBlockTrigger = false;
		pointerDownThisBlock = false;
		gameState = GameState.None;
	}

	/// <summary>
	/// 블록간에 병합을 진행한다. 피병합 대상이 된 블록들을 목표 블록 위치로 이동시키고, 목표 블록은 병합된 블록 개수만큼 숫자가 증가한다.
	/// 병합 도중에 관련 스테이지 및 퀘스트 진행도를 증가시키고, 블록에 따라 적합한 병합 이펙트도 스폰시킨다.
	/// </summary>
	/// <returns></returns>
	async UniTask MergeBlocks()
	{
		List<MergeInfo> mergeInfos = new();
		foreach (Block block in boardBlocks)
		{
			List<Block> newMergeBlocks = new();
			foreach (Block neighborBlock in GetNeighborBlocks(block))
			{
				if (MergeAvailable(block, neighborBlock))
				{
					newMergeBlocks.Add(neighborBlock);
				}
			}
			if (newMergeBlocks.Count > 0)
			{
				List<Block> newAllBlocks = new(newMergeBlocks) { block };
                mergeInfos.Add(new()
				{
					allBlocks = newAllBlocks,
					otherBlocks = newMergeBlocks,
					destinationBlock = block,
				});
			}
		}
		if (mergeInfos.Count == 0) return;

		// 합쳐지는 위치 우선순위 : 여러개가 합쳐지는 쪽 > 합쳐진 블록 > 이동한 블록 > 좌표상 위쪽 > 좌표상 왼쪽
		mergeInfos = mergeInfos.FindAll(x => x.otherBlocks.Count == mergeInfos.Max(x => x.otherBlocks.Count));
		mergeInfos = mergeInfos.FindAll(x => x.destinationBlock.mergedTurn == mergeInfos.Max(x => x.destinationBlock.mergedTurn));
		mergeInfos = mergeInfos.FindAll(x => x.destinationBlock.movedTurn == mergeInfos.Max(x => x.destinationBlock.movedTurn));
		mergeInfos = mergeInfos.FindAll(x => x.destinationBlock.data.coord.y == mergeInfos.Max(x => x.destinationBlock.data.coord.y));
		MergeInfo mergeInfo = mergeInfos.Find(x => x.destinationBlock.data.coord.x == mergeInfos.Min(x => x.destinationBlock.data.coord.x));

		if (mergeInfo != null)
		{
			Debug.Log("Merge start");
			await MergeBlock(mergeInfo);
		}
		await UniTask.NextFrame();
	}

	public async UniTask MergeBlock(MergeInfo mergeInfo, bool increase = true, GameObject specialMergeEffect = null)
	{
		// 특수블록 업데이트
		foreach (Block block in mergeInfo.allBlocks)
		{
			if (block && block.TryGetComponent(out SpecialBlock specialBlock))
			{
				specialBlock.OnBeforeMerge();
			}
		}

		// 병합되는 블록의 연결블록 파괴
		foreach (Block mergingBlock in mergeInfo.allBlocks)
		{
			DestroyConnectors(mergingBlock);
		}

		// 이동 애니메이션
		List<UniTask> moveTasks = new();
		foreach (Block block in mergeInfo.otherBlocks)
		{
			moveTasks.Add(block.transform.DOMove(mergeInfo.destinationBlock.transform.position, blockAnimationSpeed).SetSpeedBased().SetEase(Ease.OutCubic).WithCancellation(gameCancelToken.Token));
		}
		await moveTasks;

		// 숫자 증가 또는 감소
		bool bOnDestroySelf = false;
		bool bIsOver1024T = false;
		if (increase)
		{
			int mergedNumberType = (int)mergeInfo.destinationBlock.data.numberType + mergeInfo.otherBlocks.Count;
			if (mergedNumberType <= (int)NumberType.N1024T)
            {
				mergeInfo.destinationBlock.SetNumberType((NumberType)mergedNumberType);
			}
			else
            {
				bOnDestroySelf = true;
				bIsOver1024T = true;
			}
		}
		else
		{
			if (mergeInfo.destinationBlock.data.numberType == NumberType.N2)
			{
				bOnDestroySelf = true;
			}
			else
			{
				mergeInfo.destinationBlock.SetNumberType(mergeInfo.destinationBlock.data.numberType - 1);
			}
		}
		mergeInfo.destinationBlock.mergedTurn = blockTurn;
		foreach (Block block in mergeInfo.otherBlocks)
		{
			RemoveBlock(block, true, true);
		}

		// 콤보
		combo++;
		if (combo > 1)
		{
			ComboEffect(mergeInfo.destinationBlock.transform.position);
        }
		else
		{
            SoundMgr.PlaySfx(SfxType.NumberMerge);
        }

		// 스킬 게이지 상승
		float gaugeAmount = mergeInfo.allBlocks.Count switch
		{
			1 => 0.1f,
			2 => 0.1f,
			3 => 0.15f,
			4 => 0.2f,
			_ => 0.2f
		};
		gaugeAmount *= combo switch
		{
			1 => 1f,
			2 => 1.5f,
			3 => 2f,
			_ => 2f
		};
		gaugeAmount *= increaseSkillGaugeEnabled ? 1.2f : 1f;
		SetSkillGauge(skillGauge + gaugeAmount);

		// 특수블록, 미션 업데이트
		foreach (Block block in mergeInfo.allBlocks)
		{
			if (block && block.TryGetComponent(out SpecialBlock specialBlock))
			{
				specialBlock.OnAfterMerge();
			}
		}
		foreach (Block block in mergeInfo.allBlocks)
		{
			foreach (Block neighborBlock in GetNeighborBlocks(block))
			{
				if (neighborBlock && neighborBlock.TryGetComponent(out SpecialBlock specialBlock))
				{
					specialBlock.OnMergeNeighbor();
				}
			}
		}
		if(bIsOver1024T)	OnNumberChangeOverMaxNumberType(mergeInfo.destinationBlock, mergeInfo.otherBlocks.Count);
		else				OnNumberChange(mergeInfo.destinationBlock);

		// 퀘스트 카운팅
		UserMgr.UpdateQuestCount(QuestType.MergeNumber, 1);
		if (mergeInfo.otherBlocks.Count >= 2)
		{
			UserMgr.UpdateQuestCount(QuestType.MultiMerge, 1);
		}

		// 병합 애니메이션
		if (specialMergeEffect)
		{
			GameObject effect = InitializeEffect(mergeInfo.destinationBlock.transform.position, specialMergeEffect, true);
			ApplyEffectColor(effect, mergeInfo.destinationBlock);
		}
		else
		{
			BlockMergeEffect(mergeInfo.destinationBlock);
		}

		// 블록 파괴 여부에 따라 파괴시켜줌.
		if (bOnDestroySelf) RemoveBlock(mergeInfo.destinationBlock, true, true);

		await UniTask.Delay(blockMergeDelay.ToMilliseconds());
		checkBoardNeed = true;
	}

	async UniTask DropBoardBlocks()
	{
		bool moveCheckNeed = true;
		List<Block> movedBlocks = new();
		while (moveCheckNeed)
		{
			moveCheckNeed = false;
			HashSet<Block> boardBlocksCopy = new(boardBlocks);
			boardBlocksCopy.Remove(currentDropBlock);
			while (boardBlocksCopy.Count > 0) 
			{
				Block block1 = boardBlocksCopy.First();
				List<Block> blockGroup = new();
				Queue<Block> blockQueue = new();
				blockQueue.Enqueue(block1);
				while (blockQueue.Count > 0)
				{
					Block iterBlock = blockQueue.Dequeue();
					if (!blockGroup.Contains(iterBlock))
					{
						blockGroup.Add(iterBlock);
					}
					boardBlocksCopy.Remove(iterBlock);
					IEnumerable<Block> otherBlocks = iterBlock.attachedConnectors.Select(x => x.GetOtherOne(iterBlock)).Where(x => boardBlocksCopy.Contains(x));
					foreach (Block otherBlock in otherBlocks)
					{
						blockQueue.Enqueue(otherBlock);
					}
				}

				int minSpaceCount = int.MaxValue;
				foreach (Block block2 in blockGroup)
				{
					int spaceCount = 0;
					switch (block2.blockTypeInGame)
					{
						case BlockType.Brick: break;
						default:
							for (int y = block2.data.coord.y - 1; y >= 0; y--)
							{
								Cell cell = GetBoardCell(new(block2.data.coord.x, y));
								if (blockGroup.Contains(cell.accupiedBlock)) continue;

								if (!cell.accupiedBlock)
								{
									spaceCount++;
								}
								else
								{
									break;
								}
							}
							break;
					}
					minSpaceCount = Mathf.Min(spaceCount, minSpaceCount);
					if (minSpaceCount == 0) break;
				}

				if (minSpaceCount > 0)
				{
					foreach (Block block in blockGroup)
					{
						SetBlockToCell(GetBoardCell(new(block.data.coord.x, block.data.coord.y - minSpaceCount)), block);
						movedBlocks.Add(block);
					}
					moveCheckNeed = true;
				}
			}
		}

		movedBlocks.Distinct();
		if (movedBlocks.Count > 0)
		{
			checkBoardNeed = true;
		}

		// 이동 애니메이션
		List<UniTask> moveTasks = new();
		foreach (Block movedBlock in movedBlocks)
		{
			Cell destinationCell = GetBoardCell(new(movedBlock.data.coord.x, movedBlock.data.coord.y));
			moveTasks.Add(movedBlock.transform.DOMove(destinationCell.transform.position, blockAnimationSpeed).SetEase(blockMoveEase).SetSpeedBased().OnComplete(() =>
			{
				SoundMgr.PlaySfx(SfxType.BlockFloor);
			}).WithCancellation(gameCancelToken.Token));
			movedBlock.movedTurn = blockTurn;
		}
		await moveTasks;
	}

	public bool AnimationDone(Animator animator, float time)
	{
		if (!animator) return true;
		return animator.GetCurrentAnimatorStateInfo(0).normalizedTime > time;
	}
}
