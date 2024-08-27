using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static GameInterface;
using static SoundManager;

using BInteger = System.Numerics.BigInteger;

public partial class BlockController
{
	async UniTask ResetGame()
	{
		// 멤버 초기화
		endlessModeUI.SetActive(false);
		stageModeUI.SetActive(false);
		stageClearPanel.SetActive(false);
		stageFail1Panel.SetActive(false);
		stageFail2Panel.SetActive(false);
		pausePanel.SetActive(false);
		endlessModeStartPanel.SetActive(false);
		endlessFail1Panel.SetActive(false);
		endlessFail2Panel.SetActive(false);
		boardInputArea.gameObject.SetActive(false);
		newBlockId = 1001;
		spawnBlockCount = 0;
		newBlockXCoord = 0;
		DestroyObjects(columns);
		DestroyObjects(boardCellsLinear);
		boardCellsFrontLayerLinear.ForEach(x => Destroy(x));
		boardCellsFrontLayerLinear.Clear();
		DestroyObjects(boardBlocks);
		DestroyObjects(connectors);
		DestroyObjects(uiBlocks);
		UpdateItemButtons();
        UpdateSkillButton();
        gameState = GameState.None;
		gameEnd = GameEndType.Playing;
		usingItemType = null;
		touchToStartPanel.SetActive(false);
		itemPurchasePanel.SetActive(false);
		highestNumberType = NumberType.N2;
		highestNumberPopup.SetActive(false);
		stageInfoPanel.SetActive(false);
		endlessCoinAccumulated = 0;
		stageAllClearPanel.SetActive(false);
		warningImage.gameObject.SetActive(false);
		if (skillGaugeFullEffect) Destroy(skillGaugeFullEffect);
		inputBlocker.SetActive(false);
		itemCancelBar.SetActive(false);
        nextBlockPoint.SetActive(true);

        // 스테이지 설정
        if (isMapToolMode)
		{
#if UNITY_EDITOR
			stageData = MapToolManager.Inst.StageMgr.CreateDefaultStage();
			MapToolManager.Inst.StageMgr.DeepCopyStage(stageData, MapToolManager.Inst.CurStage);
#endif
		}
		else
		{
			stageData = StageMgr.CurrentStage;

#if DEV_TEST
			width = stageData.width; 
			height = stageData.height; 
			time = stageData.time;
#else
			width = 4;
			height = 7;
			time = 10;
#endif
            Debug.LogFormat("width: {0}, height: {1}, time: {2}", width, height, time);
        }
		stageText.text = string.Format(GetText("ingame_mode_stage"), stageNumber);
		switch (gameMode)
		{
			case GameMode.Stage:
				List<BlockData> spawnChancesCopy = new();
				foreach (BlockData blockData in stageData.spawnBlocks)
				{
					spawnChancesCopy.Add(blockData.GetCopy());
				}
				blockSpawnPool = new();
				float chanceAccumulated = 0f;
				float chanceTotal = stageData.spawnBlocks.Sum(x => x.spawnChance);
				foreach (BlockData blockData in spawnChancesCopy)
				{
					chanceAccumulated += blockData.spawnChance;
					blockSpawnPool.Add((blockData, chanceAccumulated / chanceTotal));
				}
				isNewStage = stageNumber > UserMgr.LastClearStageNumber;
				break;
		}

		// 점수, 스킬 게이지, 다음 블록 여부 로드
		IngameSaveData saveData = null;
		if (gameMode == GameMode.Endless)
		{
			switch (boardSize.x)
			{
				case 4:
					saveData = StageMgr.SaveData_Infinite47;
					break;
				case 6:
					saveData = StageMgr.SaveData_Infinite67;
					break;
			}
		}
		if (saveData != null)
		{
			SetScore(BInteger.Parse(saveData.score), true);
			SetSkillGauge(saveData.skillGauge, true);

			slowDropSpeedEnabled = saveData.bIsDecreaseDropSpeedEnabled;
			increaseSkillGaugeEnabled = saveData.bIsIncreaseSkillGaugeEnabled;
			spawnBlockCount = saveData.spawnBlockCount;
			highestNumberType = saveData.highestNumberType;
			endlessCoinAccumulated = saveData.endlessCoinAccumulated;
			isAutoSavedStage = true;
			nContinue = saveData.nContinue;
			isInRanking = saveData.bIsInRanking;

            CheckBlockSpawnRangeOnInf47();
        }
		else
		{
			SetScore(0, true);
			SetSkillGauge(0f, true);
			slowDropSpeedEnabled = false;
			increaseSkillGaugeEnabled = false;
			isAutoSavedStage = false;
			nContinue = 0;
			isInRanking = false;
		}

		// 게임 영역을 조정한다.
		originBoardSize = cellGroup.GetComponent<RectTransform>().rect.size;
        await AdjustGameAreaSize();

		// 보드 생성
		bool anyEscapeMission = missions.Find(x => x.missionType == MissionType.EscapeBlock);
		Mission_TargetCell targetCellMission = missions.Find(x => x.missionType == MissionType.TargetCell) as Mission_TargetCell;
		NumberType anyTargetCellMissionNumber = targetCellMission ? targetCellMission.targetNumber : NumberType.None;
		boardCells = new Cell[boardSize.x, boardSize.y];
		for (int y = 0; y < boardSize.y; y++)
		{
			for (int x = 0; x < boardSize.x; x++)
			{
				Vector2Int cellCoord = new(x, y);
				Cell cellPrefab = GetCellPrefab(CellType.Blank);
				CellData specialCell = stageData.stageCells.Find(x => x.coord == cellCoord);
				if (specialCell != null)
				{
					cellPrefab = GetCellPrefab(specialCell.cellType);
				}
				Cell newCell = Instantiate(cellPrefab, cellGroup.GetCellParentByCoord(y));
				newCell.data.coord = cellCoord;
				newCell.GetComponent<RectTransform>().sizeDelta = gridCellSize;
				bool isEven = (x + y) % 2 == 0;
				newCell.cellImage1.gameObject.SetActive(!isEven);
				newCell.cellImage2.gameObject.SetActive(isEven);
				bool isTopCell = y == boardSize.y - 1;
				newCell.arrow.gameObject.SetActive(isTopCell);
				newCell.arrow.transform.localScale = Vector3.one * boardScaleMultiplier;
				newCell.warning.gameObject.SetActive(false);

				GameObject newCellFrontLayer = Instantiate(cellFrontLayerPrefab, cellGroupFrontLayer.GetCellParentByCoord(y));
				newCellFrontLayer.GetComponent<RectTransform>().sizeDelta = gridCellSize;
				newCell.frontImage = newCellFrontLayer.transform.GetChild(0).GetComponent<Image>();
				newCell.frontBottomImage = newCellFrontLayer.transform.GetChild(1).GetComponent<Image>();
				newCell.frontNumberText = newCellFrontLayer.transform.GetChild(2).GetComponent<TMP_Text>();

				// 지정위치 미션이라면 해당 셀의 앞쪽 이미지 활성화
				if (specialCell != null && specialCell.cellType == CellType.TargetNumber)
				{
					newCell.frontImage.gameObject.SetActive(true);
					newCell.frontNumberText.gameObject.SetActive(true);
					newCell.frontNumberText.text = TypeHelper.NumberTypeToString(anyTargetCellMissionNumber);
				}
				else
				{
					newCell.frontImage.gameObject.SetActive(false);
					newCell.frontNumberText.gameObject.SetActive(false);
				}

				// 탈출블록 미션이 있다면
				// 가장 아래 셀에 이미지 활성화
				// 레인에 화살표 이미지 애니메이션
				newCell.frontBottomImage.gameObject.SetActive(anyEscapeMission && y == 0);
				newCell.escapeArrow.gameObject.SetActive(anyEscapeMission);

				boardCellsFrontLayerLinear.Add(newCellFrontLayer);
				boardCellsLinear.Add(newCell);
				boardCells[x, y] = newCell;
			}
		}
		await UniTask.NextFrame();

		// Column 생성
		for (int x = 0; x < boardSize.x; x++)
		{
			Column newColumn = Instantiate(columnPrefab, columnParent);
			newColumn.GetComponent<RectTransform>().sizeDelta = new()
			{
				x = gridCellSize.x,
				y = gridCellSize.y * boardSize.y + topCellSpace
			};
			columns.Add(newColumn);
		}

		// 스테이지 블록 생성
		foreach (BlockData blockData in stageData.stageBlocks)
		{
			CreateNewBlockToBoard(GetBoardCell(blockData.coord), blockData);
		}

		// 연결블록 연결
		foreach (ListWrapper<int> connectorData in stageData.stageBlockIdGroups)
		{
			List<Block> blocks = new();
			foreach (int blockId in connectorData)
			{
				blocks.Add(boardBlocks.Find(x => x.data.Id == blockId));
			}
			Connector newConnector = Instantiate(connectorPrefab, connectorParent);
			newConnector.SetBlocks(blocks);
			connectors.Add(newConnector);
			foreach (Block block in blocks)
			{
				block.attachedConnectors.Add(newConnector);
			}
		}

		// 상단 UI 초기화
		switch (gameMode)
		{
			case GameMode.Endless:
				endlessModeUI.SetActive(true);
				endlessModeText.text = boardSize.x switch
				{
					4 => string.Format(GetText("ingame_mode_unlimit_4"), boardSize.x, boardSize.y),
					_ => string.Format(GetText("ingame_mode_unlimit_6"), boardSize.x, boardSize.y),
				};
				endlessModeHighscoreText.text = string.Format(GetText("ingame_unlimit_best_score"), GetLastHighScore().ToString("N0"));
				break;
			case GameMode.Stage:
				stageModeUI.SetActive(true);
				UpdateMissionUiIcons(missionUIs, missions);
				UpdateMissions();
				break;
		}
	}

	void ProcessAdjustGameAreaSize()
	{
		AdjustGameAreaSize().Forget();
	}

	/// <summary>
	/// 화면 비율에 따라 게임 화면을 알맞게 조정한다.
	/// </summary>
	/// <returns></returns>
    async UniTask AdjustGameAreaSize()
    {
		// 해상도 변경으로 인한 UI 수정으로 인해 발생하는 블록 위치를 바로잡기 위해 현재 블록들의 위치 비율값을 구한다.
        Vector2 prevBoardSize = cellGroup.GetComponent<RectTransform>().rect.size / 2f;
        List<Vector2> blockPosRatioList = new List<Vector2>();
        foreach (Transform block in blockParent.transform)
        {
            var anchoredPos = block.GetComponent<RectTransform>().anchoredPosition;
            blockPosRatioList.Add(new Vector2(
                Mathf.InverseLerp(-prevBoardSize.x, prevBoardSize.x, anchoredPos.x),
                Mathf.InverseLerp(-prevBoardSize.y, prevBoardSize.y, anchoredPos.y)
                ));
        }

        // 배너 광고 높이는 현재 스크린의 15% 높이를 넘지 않으므로 해당 크기 미만으로 광고 영역을 조절해준다.
        float ratio =  0.10f;
#if !MS
		float unsafeAreaHeight = Screen.height - Screen.safeArea.height;
        float adHeight = Screen.safeArea.height * ratio + unsafeAreaHeight;
		float canvasAdHeight = GetComponent<CanvasScaler>().referenceResolution.y * (adHeight / Screen.height);
        adArea.sizeDelta = adArea.sizeDelta.WithY(canvasAdHeight);
#else
		float refResY = GetComponent<CanvasScaler>().referenceResolution.y;
        float canvasAdHeight = refResY * ratio;
        adArea.sizeDelta = adArea.sizeDelta.WithY(canvasAdHeight);
#endif

        cellGroup.GetComponent<RectTransform>().sizeDelta = originBoardSize;
        cellGroupFrontLayer.GetComponent<RectTransform>().sizeDelta = originBoardSize;

#if !MS
		safeArea.anchorMax = new Vector2(1f, (Screen.height - adHeight) / Screen.height);
#else
        safeArea.anchorMax = new Vector2(1f, (refResY - canvasAdHeight) / refResY);
#endif

        await AdjustGridParentSize(cellGroup);
        await AdjustGridParentSize(cellGroupFrontLayer);

		boardRects.ForEach(x => x.sizeDelta = new(gridCellSize.x * boardSize.x, gridCellSize.y * boardSize.y + topCellSpace));

		// 해상도 변경으로 인해 보정된 UI 내에서 블록 위치를 다시 잡아준다
        Vector2 newBoardSize = cellGroup.GetComponent<RectTransform>().rect.size / 2f;
        for (int i = 0; i < blockParent.transform.childCount; i++)
		{
			var block = blockParent.transform.GetChild(i);
            block.localScale = blockSize.x / blockSpriteSize.x * Vector3.one;

			var newBlockPos = new Vector2(
				Mathf.Lerp(-newBoardSize.x, newBoardSize.x, blockPosRatioList[i].x),
				Mathf.Lerp(-newBoardSize.y, newBoardSize.y, blockPosRatioList[i].y)
				);
			block.GetComponent<RectTransform>().anchoredPosition = newBlockPos;
        }
    }

    async UniTask AdjustGridParentSize(CellVerticalGroup group)
    {
		await UniTask.NextFrame();

		const float OFFSET = 100f;
		float boardHeight = safeArea.rect.height;

		float topHeight = stageModeUI.GetComponent<RectTransform>().rect.height;
		float bottomHeight = bottomArea.rect.height;

		boardHeight -= (topHeight + bottomHeight + OFFSET);
        RectTransform gridParentRect = group.GetComponent<RectTransform>();
        gridParentRect.sizeDelta = gridParentRect.sizeDelta.WithY(boardHeight);

        Vector2 gridRectSize = gridParentRect.rect.size;
        Vector2 totalCellSize = gridRectSize.WithY(gridRectSize.y - topCellSpace);
        gridCellSize = totalCellSize / boardSize;
        float blockSpriteRatio = blockSpriteSize.x / blockSpriteSize.y;
        if (gridCellSize.x / gridCellSize.y > blockSpriteRatio)
        {
            gridCellSize.x = gridCellSize.y * blockSpriteRatio;
        }
        else
        {
            gridCellSize.y = gridCellSize.x / blockSpriteRatio;
        }

        for (int i = 0; i < group.horizontalGroups.Count; i++)
        {
            HorizontalLayoutGroup horizontalGroup = group.horizontalGroups[i];
            horizontalGroup.gameObject.SetActive(i < boardSize.y + 1);
            RectTransform rt = horizontalGroup.GetComponent<RectTransform>();
            if (i == boardSize.y - 1)
            {
                rt.sizeDelta = new(1f, topCellSpace);
            }
            else
            {
                rt.sizeDelta = new(gridCellSize.x * boardSize.x, gridCellSize.y);
            }
        }
    }

    void ConsumeStartBlock512Demand()
	{
		if (startBlock512PurchaseDemand)
		{
			BlockData block512 = BlockFactoryInst.CreateBlockData(BlockType.Normal, NumberType.N512);
			stageData.startBlocks.Insert(0, block512);
			startBlock512PurchaseDemand = false;
		}
	}

	void CreateNewBlock()
	{
        Cell startCell = GetBoardCell(new(newBlockXCoord, boardSize.y - 1));
        currentDropBlock = CreateNewBlockToBoard(startCell, nextBlockData);
        currentDropBlock.mergePriority = true;
        currentDropBlock.thisAnimator.Play("Created", 0, 0f);
    }

	void SetNextBlockData()
	{
		if (nextBlockUI)
		{
			Destroy(nextBlockUI.gameObject);
			uiBlocks.Remove(nextBlockUI);
		}
		nextBlockData = null;

		if (stageData.startBlocks.Count > 0)
		{
			nextBlockData = stageData.startBlocks[0];
			stageData.startBlocks.RemoveAt(0);
		}
		else
		{
			switch (gameMode)
			{
				case GameMode.Endless:
					// 최고 숫자에 따라 스폰
					List<EndlessSpawnData> spawnDatas = boardSize.x switch
					{
						4 => endlessSpawnDatas4x7,
						6 => endlessSpawnDatas6x7,
						_ => endlessSpawnDatas6x7,
					};
					EndlessSpawnData spawnData = spawnDatas.Find(x => highestNumberType >= x.maxNumberType);
					NumberType minNumberType = (NumberType)Mathf.Clamp((int)spawnData.GetMinNumberType(highestNumberType), (int)NumberType.N2, (int)NumberType.N64T);
					NumberType maxNumberType = (NumberType)Mathf.Clamp((int)spawnData.GetMaxNumberType(highestNumberType), (int)NumberType.N32, (int)NumberType.N1024T);
					NumberType numberType = (NumberType)Random.Range((int)minNumberType, (int)maxNumberType + 1);
					nextBlockData = BlockFactoryInst.CreateBlockData(BlockType.Normal, numberType);
					break;
				case GameMode.Stage:
					// BoardLimit, SpawnCount로 스폰
					if (!nextBlockData)
					{
						List<BlockData> boardLimitSpawnBlocks = stageData.blockBoardLimits.FindAll(boardLimitBlock => boardBlocks.FindAll(x => x.blockTypeInGame == boardLimitBlock.blockType).Count < boardLimitBlock.boardLimit);
						BlockData spawnCountBlock = null;
						foreach (BlockData boardLimitBlock in boardLimitSpawnBlocks)
						{
							List<BlockData> spawnCountBlocks = stageData.blockSpawnCounts.FindAll(x => x.blockType == boardLimitBlock.blockType);
							spawnCountBlock = spawnCountBlocks.FindAll(x => x.spawnCount > 0).PickOne();
							if (spawnCountBlock)
							{
								spawnCountBlock.spawnCount--;
								nextBlockData = spawnCountBlock;
								break;
							}
						}
					}

					// BlockSpawnPool로 스폰
					if (!nextBlockData)
					{
						float randomValue = Random.value;
						nextBlockData = blockSpawnPool[0].blockData;
						foreach ((BlockData blockData, float possibility) in blockSpawnPool)
						{
							if (randomValue < possibility)
							{
								nextBlockData = blockData;
								break;
							}
						}
					}
					break;
			}
		}

		nextBlockUI = CreateNewBlockUI(nextBlockData, nextBlockPoint.transform);
	}

	public Block CreateNewBlockToBoard(Cell cell, BlockData blockData)
	{
		Block newBlock = BlockFactoryInst.CreateBlock(blockData, blockParent);
		newBlock.data = blockData.GetCopy();
		SetBlockToCell(cell, newBlock);
		newBlock.transform.position = cell.transform.position;
		newBlock.transform.localScale = blockSize.x / blockSpriteSize.x * Vector3.one;
		if (newBlock.data.Id == 0)
		{
			newBlock.data.Id = newBlockId;
			newBlockId++;
			spawnBlockCount++;
		}
		boardBlocks.Add(newBlock);
		return newBlock;
	}

	public Block CreateNewBlockUI(BlockData blockData, Transform uiParent)
	{
		Block newBlock = BlockFactoryInst.CreateBlock(blockData, uiParent);
		RectTransform newBlockRect = newBlock.GetComponent<RectTransform>();
		newBlockRect.localPosition = Vector3.zero;
		newBlock.thisCollider.enabled = false;
		newBlock.data = blockData.GetCopy();
		uiBlocks.Add(newBlock);
		return newBlock;
	}

	bool CheckGameEnd()
	{
		gameEnd = GameEndType.Playing;
		for (int x = 0; x < boardSize.x; x++)
		{
			if (GetBoardCell(new(x, boardSize.y - 2)).accupiedBlock)
			{
				gameEnd = GameEndType.StageFail;
				break;
			}
		}
		switch (gameMode)
		{
			case GameMode.Stage:
				if (missions.All(x => x.CheckMissionClear()))
				{
					gameEnd = GameEndType.StageClear;
				}
				break;
		}
		return gameEnd != GameEndType.Playing;
	}

	/// <summary>
	/// 블록과 셀간의 관계 여부를 설정한다.
	/// </summary>
	/// <param name="cell">블록과 관계될 셀</param>
	/// <param name="block">설정할 블록</param>
	void SetBlockToCell(Cell cell, Block block)
	{
		if (block.accupiedCell)
		{
			block.accupiedCell.accupiedBlock = null;
		}
		if (cell.accupiedBlock)
		{
			cell.accupiedBlock.accupiedCell = null;
		}
		block.accupiedCell = cell;
		cell.accupiedBlock = block;
		block.data.coord = cell.data.coord;
	}

	bool MergeAvailable(Block block1, Block block2)
	{
		List<Block> twoBlock = new() { block1, block2 };
		if (twoBlock.All(x => x.canMerge) && twoBlock.Any(x => x.attachedConnectors.Count == 0))
		{
			return block1.data.numberType == block2.data.numberType;
		}
		else
		{
			return false;
		}
	}

	async UniTask DropComplete()
	{
		List<Block> boardBlocksCopy = new(boardBlocks);
		foreach (Block block in boardBlocksCopy)
		{
			if (block && block.TryGetComponent(out SpecialBlock specialBlock))
			{
				await specialBlock.OnDropComplete();
			}
		}

		// 얼음블록은 다른 특수블록들보다 나중에 처리
		foreach (Block block in boardBlocks)
		{
			if (block && block.TryGetComponent(out Block_Ice iceBlock))
			{
				await iceBlock.OnDropComplete_Ice();
			}
		}

		// 얼음블록 파괴
		if (iceBlocksToDestroyPair.Count > 0)
		{
			IEnumerable<Block> iceBlocks = iceBlocksToDestroyPair.Select(x => x.iceBlock).Distinct();
			IEnumerable<Block> blocks = iceBlocksToDestroyPair.Select(x => x.block).Distinct();
			foreach (Block iceBlock in iceBlocks)
			{
				SoundMgr.PlaySfx(SfxType.BlockTarget);
				RemoveBlock(iceBlock, true, true);
			}
			foreach (Block block in blocks)
			{
				block.blockBeakerImage.gameObject.SetActive(true);
				Color destColor = block.blockBeakerImage.color.WithAlpha(1f);
				block.blockBeakerImage.DOColor(destColor, 0.1f).ToUniTask().Forget();
				GameObject effect = InitializeEffect(block.transform.position, destroyIceBlockEffect1Prefab, true);
				ApplyEffectColor(effect, block);
			}
			await UniTask.Delay(400);
			foreach (Block block in blocks)
			{
				block.transform.DOShakePosition(0.6f, strength: 8f, vibrato: 30, snapping: true, fadeOut: true).ToUniTask().Forget();
				block.transform.DOScale(1.1f, 0.6f).ToUniTask().Forget();
			}
			await UniTask.Delay(700);
			foreach (Block block in blocks)
			{
				SoundMgr.PlaySfx(SfxType.NumberDestroy);
				GameObject effect = InitializeEffect(block.transform.position, destroyIceBlockEffect2Prefab, true);
				ApplyEffectColor(effect, block);
				RemoveBlock(block, true, true);
			}
			iceBlocksToDestroyPair.Clear();
			checkBoardNeed = true;
            Debug.Log("Destroy beaker");
        }

		// 탈출블록 탈출
		if (escapeBlocksToDestroy.Count > 0)
		{
			List<UniTask> moveTasks = new();
			foreach (Block block in escapeBlocksToDestroy)
			{
				SoundMgr.PlaySfx(SfxType.BlockEscape);
				Block_Escape escapeBlock = block.GetComponent<Block_Escape>();
				MissionCountEffect(block).Forget();
				DestroyConnectors(block);
				block.transform.SetParent(escapeBlockDisappearLayer, true);
				InitializeEffect(block.transform.position, escapeBlockEffectPrefab, true);
				escapeBlock.PlayOnAnimation();
				moveTasks.Add(EscapeAnimation());
				async UniTask EscapeAnimation()
				{
					await UniTask.Delay(1000);
					ScaleAnimation().Forget();
					escapeBlock.PlayFlyAnimation();
					await block.transform.DOMoveY(topArea.position.y, 1.1f).SetEase(Ease.InBack);
					async UniTask ScaleAnimation()
					{
						await block.transform.DOScale(1.1f, 0.3f);
						await UniTask.Delay(500);
						await block.transform.DOScale(0.8f, 0.3f);
					}
				}
			}
			await moveTasks;
			escapeBlocksToDestroy.ForEach(x => RemoveBlock(x, false, true));
			escapeBlocksToDestroy.Clear();
			checkBoardNeed = true;
		}
	}

	public void RemoveBlock(Block block, bool destroyEffect, bool missionCount)
	{
		var accupiedCell = block.accupiedCell;
		if (accupiedCell) accupiedCell.accupiedBlock = null;

		DestroyConnectors(block);
		boardBlocks.Remove(block);
		Destroy(block.gameObject);

		if (destroyEffect)
		{
			BlockDestroyEffect(block);
		}

		if (missionCount)
		{
			BlockType blockType = block.blockTypeInGame;
			foreach (Mission mission in missions)
			{
				switch (mission.missionType)
				{
					case MissionType.EscapeBlock:
						if (blockType == BlockType.Escape)
						{
							((Mission_EscapeBlock)mission).clearCount++;
						}
						break;
					case MissionType.DestroyDummy:
						if (blockType == BlockType.Dummy)
						{
							((Mission_DestroyDummy)mission).clearCount++;
							MissionCountEffect(mission, block).Forget();
						}
						break;
					case MissionType.DestroyIce:
						if (blockType == BlockType.Ice)
						{
							((Mission_DestroyIce)mission).clearCount++;
							MissionCountEffect(mission, block).Forget();
						}
						break;
				}
			}
		}
	}

	public List<Block> GetNeighborBlocks(Block block)
	{
		List<Block> neighbors = new();
		foreach (Vector2Int direction in fourDirections)
		{
			Block otherBlock = GetBoardBlock(block.data.coord + direction);
			if (otherBlock)
			{
				neighbors.Add(otherBlock);
			}
		}
		return neighbors;
	}
	Cell GetCellPrefab(CellType cellType) => cellPrefabs.Find(x => x.data.cellType == cellType);

	public Block GetBoardBlock(Vector2Int coord)
	{
		if (!IsValidCoord(coord)) return null;
		return GetBoardCell(new(coord.x, coord.y)).accupiedBlock;

		bool IsValidCoord(Vector2Int coord) => coord.x >= 0 && coord.y >= 0 && coord.x < boardSize.x && coord.y < boardSize.y;
	}

	public Cell GetBoardCell(Vector2Int coord)
	{
		return boardCells[coord.x, coord.y];
	}

	public Cell GetClosestCell(Vector3 position)
	{
		Cell closestCell = null;
		float closestDistance = float.MaxValue;
		foreach (Cell cell in boardCellsLinear)
		{
			float distance = Vector3.Distance(cell.transform.position, position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestCell = cell;
			}
		}
		return closestCell;
	}

	void DestroyConnectors(Block mergingBlock)
	{
		foreach (Connector connector in mergingBlock.attachedConnectors)
		{
			ConnectorDestroyEffect(connector);
			connector.GetOtherOne(mergingBlock).attachedConnectors.Remove(connector);
			Destroy(connector.gameObject);
			connectors.Remove(connector);
			(int id1, int id2) = (connector.blocks.x1.data.Id, connector.blocks.x2.data.Id);
			stageData.stageBlockIdGroups.RemoveAll(x => x.ToTuple() == (id1, id2) || x.ToTuple() == (id2, id1));
		}
		mergingBlock.attachedConnectors.Clear();
	}
}
