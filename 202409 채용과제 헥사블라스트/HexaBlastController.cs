using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public partial class HexaBlastController : SingleInstance<HexaBlastController>
{
	public Block _BlockPrefab;
	public Transform _BlockParent;
	public Tile _TilePrefab;
	public Transform _TileParent;
	public GameObject _TouchBlocker;
	public ParticleSystem _ParticlePrefab;

	[HorizontalLine(color: EColor.Orange)]
	[Header("UI")]
	public Canvas _Canvas;
	public TMP_Text _MissionCountText;
	public TMP_Text _MoveCountText;
	public Slider _ScoreFillGauge;
	public TMP_Text _ScoreText;
	public GameObject _EndPopup;
	public TMP_Text _End_TitleText, _End_ScoreText;

	[HorizontalLine(color: EColor.Orange)]
	[Header("애셋")]
	public List<Sprite> _BlockSprites;
	public Sprite _BlockSprite_SpinnerDamaged;

	List<Tile> _TilesLinear;
	HashSet<Block> _BlocksLinear;
	Dictionary<Vector2Int, Tile> _Tiles;
	Vector2Int _MapSize;
	List<DirectionType> _AllDirectionTypes;
	List<Vector2Int> _AllDirections;
	Vector2Int _StartTileCoord;
	float _LastCreateBlockTime;
	List<List<Vector2Int>> _BoomerangPatterns;
	List<Vector2Int> _TNTArea;
	int _MissionCount;
	int _MoveCount;
	int _Score;
	bool _EndPopupTrigger;

	class MergeInfo
	{
		public List<Block> _Blocks;
		public SpecialBlockType _SpecialType;
		public BlockType _SpecialBlockColor;
		public Vector2Int _SpecialBlockCoord;
	}

	void Start()
	{
		// 초기화
		_BlockPrefab.gameObject.SetActive(false);
		_TilePrefab.gameObject.SetActive(false);
		_EndPopup.SetActive(false);
		_TouchBlocker.SetActive(false);
		_ParticlePrefab.gameObject.SetActive(false);
		_TilesLinear = new();
		_BlocksLinear = new();
		_MapSize = new(7, 7);
		_StartTileCoord = new(0, 4);
		_Tiles = new();
		_AllDirectionTypes = new();
		_AllDirections = new();
		foreach (DirectionType direction in Enum.GetValues(typeof(DirectionType)))
		{
			_AllDirectionTypes.Add(direction);
			_AllDirections.Add(DirectionToCoord(direction));
		}
		_BoomerangPatterns = new();
		_BoomerangPatterns.Add(new() { new(1, 0), new(1, -1), new(2, -1) }); // 오른쪽
		_BoomerangPatterns.Add(new() { new(1, 0), new(1, 1), new(0, 1) }); // 오른쪽 위
		_BoomerangPatterns.Add(new() { new(0, 1), new(-1, 2), new(-1, 1) }); // 왼쪽 위
		_BoomerangPatterns.Add(new() { new(-1, 1), new(-2, 1), new(-1, 0) }); // 왼쪽
		_BoomerangPatterns.Add(new() { new(-1, 0), new(-1, -1), new(0, -1) }); // 왼쪽 아래
		_BoomerangPatterns.Add(new() { new(0, -1), new(1, -2), new(1, -1) }); // 오른쪽 아래
		_TNTArea = new() { new(1, 0), new(0, 1), new(-1, 1), new(-1, 0), new(0, -1), new(1, -1),
			new(2, -2), new(2, -1), new(2, 0), new(-2, 2), new(-2, 1), new(-2, 0), };

		GameSetting();
		GameRoutine().Forget();
	}

	void GameSetting()
	{
		// 타일 세팅
		int[,] tileMap =
		{
			{ 0, 0, 0, 1, 0, 0, 0 },
			{ 1, 1, 1, 1, 0, 0, 0 },
			{ 1, 1, 1, 1, 1, 0, 0 },
			{ 1, 1, 1, 1, 1, 1, 0 },
			{ 0, 1, 1, 1, 1, 1, 1 },
			{ 0, 0, 1, 1, 1, 1, 1 },
			{ 0, 0, 0, 1, 1, 1, 1 },
		};
		for (int y = 0; y < _MapSize.y; y++)
		{
			for (int x = 0; x < _MapSize.x; x++) 
			{
				if (tileMap[y, x] == 0) continue;
				Vector2Int coord = ArrayIndexToCoord(x, y);
				Tile newTile = Instantiate(_TilePrefab, _TileParent.transform);
				newTile.gameObject.SetActive(true);
				newTile._Coord = coord;
				newTile.transform.localPosition = CellToLocal(coord);
				_TilesLinear.Add(newTile);
				_Tiles[coord] = newTile;

				// 시작 타일 숨기기
				if (coord == _StartTileCoord)
				{
					newTile._Bg.gameObject.SetActive(false);
				}
			}
		}

		// 블록 세팅
		BlockType[,] blockMap =
		{
			{ BlockType.None, BlockType.None, BlockType.None, BlockType.None, BlockType.None, BlockType.None, BlockType.None },
			{ BlockType.Yellow, BlockType.Spinner, BlockType.Spinner, BlockType.Blue, BlockType.None, BlockType.None, BlockType.None },
			{ BlockType.Green, BlockType.Purple, BlockType.Green, BlockType.Green, BlockType.Spinner, BlockType.None, BlockType.None },
			{ BlockType.Blue, BlockType.Purple, BlockType.Red, BlockType.Red, BlockType.Blue, BlockType.Spinner, BlockType.None },
			{ BlockType.None, BlockType.Spinner, BlockType.Yellow, BlockType.Spinner, BlockType.Green, BlockType.Purple, BlockType.Yellow },
			{ BlockType.None, BlockType.None, BlockType.Spinner, BlockType.Purple, BlockType.Red, BlockType.Purple, BlockType.Green },
			{ BlockType.None, BlockType.None, BlockType.None, BlockType.Spinner, BlockType.Spinner, BlockType.Spinner, BlockType.Blue },
		};
		SpecialBlockType[,] specialBlockMap =
		{
			{ SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None },
			{ SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None },
			{ SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None },
			{ SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None },
			{ SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.Missile, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None },
			{ SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.Boomerang, SpecialBlockType.MirrorBall, SpecialBlockType.Turtle, SpecialBlockType.TNT },
			{ SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None, SpecialBlockType.None },
		};
		for (int y = 0; y < _MapSize.y; y++)
		{
			for (int x = 0; x < _MapSize.x; x++)
			{
				BlockType blockType = blockMap[y, x];
				if (blockType == BlockType.None) continue;
				SpecialBlockType specialBlockType = specialBlockMap[y, x];
				Vector2Int coord = ArrayIndexToCoord(x, y);
				CreateBlock(coord, blockType, specialBlockType);
			}
		}

		// 상단 UI
		SetMissionCount(_BlocksLinear.Count(x => x._BlockType == BlockType.Spinner));
		SetMoveCount(99);
		SetScore(0);

		Vector2Int ArrayIndexToCoord(int x, int y)
		{
			Vector2Int coord = new(x, y);
			coord.x -= _MapSize.x / 2;
			coord.y = _MapSize.y - coord.y;
			coord.y -= _MapSize.y / 2;
			return coord;
		}
	}

	async UniTask GameRoutine()
	{
		bool showEnd = true;
		while (true)
		{
			// 블록 이동
			if (_DragInputTrigger)
			{
				_DragInputTrigger = false;
				_TouchBlocker.SetActive(true);
				await DragMoveBlock();
				_TouchBlocker.SetActive(false);
			}

			// 클리어 체크
			bool end = false;
			bool clear = false;
			if (_MissionCount == 0)
			{
				end = true;
				clear = true;
			}
			else if (_MoveCount == 0)
			{
				end = true;
				clear = false;
			}

			// 게임 종료
			if (showEnd && end)
			{
				showEnd = false;
				await ShowEndPopup(clear);
			}

			await UniTask.NextFrame();
		}
	}

	async UniTask DragMoveBlock()
	{
		// 목적지 좌표 계산
		Vector2Int dragStartCoord = _DragStartBlock.Coord;
		Vector2Int dragNextCoord = dragStartCoord;
		dragNextCoord += DirectionToCoord(_DragDirection);

		// 목적지 타일 레이캐스트
		Tile destTile = GetTile(dragNextCoord);
		if (!destTile) return;
		RaycastHit2D cast = Physics2D.CircleCast(destTile.transform.position, 0.1f, Vector2.zero);
		if (!cast.transform) return;

		// 이동
		_DragDestBlock = cast.transform.GetComponent<Block>();
		if (_DragStartBlock == _DragDestBlock) return;
		await SwitchTwoBlocks(_DragStartBlock, _DragDestBlock);

		// 보드 진행
		bool moveSuccess = false;
		while (true)
		{
			bool loop = await BoardProcess();
			moveSuccess |= loop;
			if (!loop) break;
			await UniTask.WaitForSeconds(0.3f);
		}

		// 이동횟수 차감
		if (moveSuccess)
		{
			SetMoveCount(_MoveCount - 1);
		}
		// 원상복귀
		else
		{
			await SwitchTwoBlocks(_DragStartBlock, _DragDestBlock);
		}
	}

	async UniTask<bool> BoardProcess()
	{
		// 터트릴 블록 계산
		List<MergeInfo> mergeList = new();
		MergeInfo specialPop = GetMergeList_Special();
		if (specialPop != null)
		{
			mergeList.Add(specialPop);
		}
		else
		{
			mergeList.AddRange(GetMergeList_Color());
		}

		// 블록 터트리기
		List<Block> affectedSpinners = new();
		foreach (MergeInfo mergeInfo in mergeList)
		{
			Debug.Log($"Merge Info - Special Type : {mergeInfo._SpecialType}   Coord : {mergeInfo._SpecialBlockCoord}   Block Count : {mergeInfo._Blocks.Count}");

			// 팽이
			foreach (Block block in mergeInfo._Blocks)
			{
				foreach (Vector2Int direction in _AllDirections)
				{
					Block adjacent = GetBlock(block.Coord + direction);
					if (!adjacent) continue;
					if (adjacent._BlockType == BlockType.Spinner)
					{
						affectedSpinners.Add(adjacent);
					}
				}
			}
			affectedSpinners = affectedSpinners.Distinct().ToList();
			foreach (Block spinner in affectedSpinners)
			{
				spinner._SpinnerHP--;
				spinner._Image.sprite = _BlockSprite_SpinnerDamaged;
				if (spinner._SpinnerHP <= 0)
				{
					SetMissionCount(_MissionCount - 1);
					mergeInfo._Blocks.Add(spinner);
				}
			}

			// 특수블록 발동
			bool anyNewTarget = true;
			while (anyNewTarget)
			{
				anyNewTarget = GatherSpecialBlockTargets(ref mergeInfo._Blocks);
			}

			// 블록 파괴
			foreach (Block block in mergeInfo._Blocks)
			{
				DestroyBlock(block);
			}

			// 특수블록 생성
			if (mergeInfo._SpecialType != SpecialBlockType.None)
			{
				CreateBlock(mergeInfo._SpecialBlockCoord, mergeInfo._Blocks.First()._BlockType, mergeInfo._SpecialType);
			}
		}

		// 낙하, 빈 블록 채우기
		await DropBlocks();

		return mergeList.Count > 0;
	}

	List<MergeInfo> GetMergeList_Color()
	{
		// 이동 유효 판정 - 같은 색 모으기
		List<List<Block>> sameBlocks = new();
		HashSet<Block> remainedBlocks = new(_BlocksLinear);
		while (remainedBlocks.Count > 0)
		{
			Block block = remainedBlocks.First();
			remainedBlocks.Remove(block);
			if (!block.CanMerge()) continue;
			List<Block> sets = new() { block };
			Queue<Block> iterBlocks = new();
			iterBlocks.Enqueue(block);
			while (iterBlocks.Count > 0)
			{
				Block iterBlock = iterBlocks.Dequeue();
				foreach (Vector2Int coordDelta in _AllDirections)
				{
					Block adjacent = GetBlock(iterBlock.Coord + coordDelta);
					if (!adjacent) continue;
					if (sets.Contains(adjacent)) continue;
					if (adjacent._BlockType != iterBlock._BlockType) continue;
					sets.Add(adjacent);
					remainedBlocks.Remove(adjacent);
					iterBlocks.Enqueue(adjacent);
				}
			}
			if (sets.Count > 2)
			{
				sameBlocks.Add(sets);
			}
		}

		// 이동 블록 판정 - 특수블록 판정
		List<MergeInfo> mergeList = new();
		foreach (List<Block> blocks in sameBlocks)
		{
			bool determined = false;
			List<Block> maxRow;
			SpecialBlockType special = SpecialBlockType.None;
			List<Block> blocksToMerge = blocks;

			// 3블록 이상 그룹 수
			List<List<Block>> threeRows = new();
			List<List<Block>> xRows = GetRows(blocks, new() { DirectionType.BottomLeft, DirectionType.TopRight });
			threeRows.AddRange(xRows);
			List<List<Block>> yRows = GetRows(blocks, new() { DirectionType.Top, DirectionType.Bottom });
			threeRows.AddRange(yRows);
			List<List<Block>> diagonalRows = GetRows(blocks, new() { DirectionType.TopLeft, DirectionType.BottomRight });
			threeRows.AddRange(diagonalRows);
			maxRow = threeRows.MaxBy(x => x.Count);
			maxRow ??= new();

			if (blocks.Count >= 5 && threeRows.Count > 0)
			{
				// 직선 5블록 : 미러볼
				if (maxRow.Count >= 5)
				{
					special = SpecialBlockType.MirrorBall;
					determined = true;
				}
				// Y자 5블록 : 거북이
				else if (CheckTurtleAdjacent(blocks))
				{
					special = SpecialBlockType.Turtle;
					determined = true;

				}
				// 교차 5블록 : TNT
				else if (threeRows.Count > 1)
				{
					special = SpecialBlockType.TNT;
					determined = true;
				}
			}

			if (!determined)
			{
				// 직선 4블록 : 미사일
				if (maxRow.Count == 4)
				{
					special = SpecialBlockType.Missile;
					blocksToMerge = maxRow;
					determined = true;
				}
				// 사각 4블록 : 부메랑
				else if (CheckBoomerangPattern(blocks))
				{
					special = SpecialBlockType.Boomerang;
					determined = true;
				}
				// 직선 3블록
				else if (threeRows.Count == 1)
				{
					blocksToMerge = maxRow;
					determined = true;
				}
			}

			if (!determined) continue;
			BlockType blockType = blocksToMerge.First()._BlockType;

			// 특수블록 위치결정
			Vector2Int? specialBlockCoord = null;
			List<Block> dragBlocks = new() { _DragStartBlock, _DragDestBlock };
			foreach (Block block in dragBlocks)
			{
				if (blocksToMerge.Contains(block))
				{
					specialBlockCoord = block.Coord;
					break;
				}
			}
			specialBlockCoord ??= blocksToMerge.PickOne().Coord;

			MergeInfo mergeInfo = new()
			{
				_SpecialType = special,
				_SpecialBlockColor = blockType,
				_SpecialBlockCoord = specialBlockCoord.Value,
				_Blocks = blocksToMerge,
			};
			mergeList.Add(mergeInfo);
		}
		return mergeList;
	}

	MergeInfo GetMergeList_Special()
	{
		if (!_DragStartBlock || !_DragDestBlock) return null;
		if (_DragStartBlock._SpecialType != SpecialBlockType.None && _DragDestBlock._SpecialType != SpecialBlockType.None)
		{
			MergeInfo mergeInfo = new()
			{
				_Blocks = new() { _DragStartBlock, _DragDestBlock },
			};
			return mergeInfo;
		}
		return null;
	}

	bool GatherSpecialBlockTargets(ref List<Block> blocks)
	{
		bool anyNewTarget = false;
		List<Block> blockCopy = new(blocks);
		foreach (Block block in blocks)
		{
			if (block._SpecialType == SpecialBlockType.None) continue;
			if (block._SpecialActivated) continue;
			anyNewTarget = true;
			block._SpecialActivated = true;
			switch (block._SpecialType)
			{
				case SpecialBlockType.Boomerang:
					{
						Block target = _BlocksLinear.FirstOrDefault(x => x.IsMissionBlock());
						if (!target)
						{
							target = _BlocksLinear.ToList().PickOne();
						}
						blockCopy.Add(target);
					}
					break;
				case SpecialBlockType.Missile:
					{
						List<DirectionType> directions = new() { DirectionType.TopRight, DirectionType.Top, DirectionType.TopLeft };
						DirectionType direction = directions.PickOne();
						List<DirectionType> rowDirections = new() { direction, GetOppositeDirection(direction) };
						List<Block> rowBlocks = GetRow_SpecialBlock(block, rowDirections);
						blockCopy.AddRange(rowBlocks);
					}
					break;
				case SpecialBlockType.TNT:
					foreach (Vector2Int tntTargetCoord in _TNTArea)
					{
						Block target = GetBlock(block.Coord + tntTargetCoord);
						if (target)
						{
							blockCopy.Add(target);
						}
					}
					break;
				case SpecialBlockType.Turtle:
					List<Block> row = GetRow_SpecialBlock(block, _AllDirectionTypes);
					blockCopy.AddRange(row);
					break;
				case SpecialBlockType.MirrorBall:
					blockCopy.AddRange(_BlocksLinear.Where(x => x._BlockType == block._BlockType));
					break;
			}
		}
		blocks = blockCopy.Distinct().ToList();
		return anyNewTarget;
	}

	async UniTask DropBlocks()
	{
		_TilesLinear.Sort((a, b) => a._Coord.y - b._Coord.y);
		while (CheckIsDropping()) 
		{
			// 빈 블록 채우기
			Vector2Int emptyCheckCoord = new(0, 3);
			if (Time.time - _LastCreateBlockTime > 0.1f && !GetBlock(emptyCheckCoord))
			{
				CreateBlock(_StartTileCoord, GetRandomBlockColor(), SpecialBlockType.None);
				_LastCreateBlockTime = Time.time;
			}

			// 수직 낙하
			bool anyBottomDrop = false;
			foreach (Tile tile in _TilesLinear)
			{
				if (tile._AccupiedBlock) continue;
				Tile fromTile = null;
				Vector2Int topCoord = tile._Coord + DirectionToCoord(DirectionType.Top);
				Tile top = GetTile(topCoord);
				Block topBlock = GetBlock(topCoord);

				if (topBlock)
				{
					fromTile = top;
					anyBottomDrop = true;
				}

				// 이동
				if (fromTile)
				{
					MoveBlock(fromTile, tile);
				}
			}

			// 양쪽에서 낙하
			if (!anyBottomDrop)
			{
				foreach (Tile tile in _TilesLinear)
				{
					if (tile._AccupiedBlock) continue;
					Tile fromTile = null;

					Vector2Int topLeftCoord = tile._Coord + DirectionToCoord(DirectionType.TopLeft);
					Vector2Int topRightCoord = tile._Coord + DirectionToCoord(DirectionType.TopRight);
					Tile topLeft = GetTile(topLeftCoord);
					Tile topRight = GetTile(topRightCoord);
					Block topLeftBlock = GetBlock(topLeftCoord);
					Block topRightBlock = GetBlock(topRightCoord);

					// 번갈아 낙하
					if (topLeftBlock && topRightBlock)
					{
						if (!tile._FallFromRight)
						{
							tile._FallFromRight = true;
							fromTile = topLeft;
						}
						else
						{
							tile._FallFromRight = false;
							fromTile = topRight;
						}
					}
					// 왼쪽에서 낙하
					else if (topLeftBlock)
					{
						fromTile = topLeft;
					}
					// 오른쪽에서 낙하
					else if (topRightBlock)
					{
						fromTile = topRight;
					}

					// 이동
					if (fromTile)
					{
						MoveBlock(fromTile, tile);
					}
				}
			}

			await UniTask.WaitForSeconds(0.05f);
		}

		await UniTask.WaitUntil(() => _BlocksLinear.All(x => x._MoveQueue.Count == 0));

		void MoveBlock(Tile fromTile, Tile toTile)
		{
			Block fromBlock = fromTile._AccupiedBlock;
			SetBlockToTile(fromTile, null);
			SetBlockToTile(toTile, fromBlock);
			fromBlock._MoveQueue.Enqueue(toTile._Coord);
		}
	}

	async UniTask ShowEndPopup(bool clear)
	{
		_EndPopup.SetActive(true);
		_End_TitleText.text = clear ? "STAGE CLEAR" : "STAGE FAILED";
		_End_ScoreText.text = _Score.ToString("N0");
		await UniTask.WaitUntil(() => _EndPopupTrigger);
		_EndPopupTrigger = false;
		_EndPopup.SetActive(false);
	}
}
