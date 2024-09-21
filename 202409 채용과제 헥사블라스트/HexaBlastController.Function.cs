using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class HexaBlastController
{
	void CreateBlock(Vector2Int coord, BlockType blockType, SpecialBlockType specialType)
	{
		Block newBlock = Instantiate(_BlockPrefab, _BlockParent.transform);
		newBlock.gameObject.SetActive(true);
		newBlock.Init();
		newBlock._DraggableUI.BeginDragCallback = OnBeginDrag;
		newBlock._DraggableUI.DragCallback = OnDrag;
		SetBlockToTile(GetTile(coord), newBlock);
		newBlock.transform.localPosition = CellToLocal(coord);
		newBlock.SetBlockType(blockType, specialType);
		_BlocksLinear.Add(newBlock);
	}

	public Tile GetTile(Vector2Int coord)
	{
		_Tiles.TryGetValue(coord, out Tile tile);
		return tile;
	}

	Block GetBlock(Vector2Int coord)
	{
		Tile tile = GetTile(coord);
		if (!tile) return null;
		//if (!tile._AccupiedBlock || !tile._AccupiedBlock.isActiveAndEnabled) return null;
		return tile._AccupiedBlock;
	}

	Vector3 CellToLocal(Vector2Int cellCoord)
	{
		Vector2 coord;
		coord.x = cellCoord.x * 1.5f;
		coord.y = cellCoord.y * 1.732f + cellCoord.x * 1.732f / 2f;
		coord /= 2f;
		Vector2 blockSize = _TilePrefab.GetComponent<RectTransform>().sizeDelta;
		return coord * blockSize;
	}

	//Vector2Int LocalToCell(Vector2 blockPos)
	//{
	//	Vector2 blockSize = _TilePrefab.GetComponent<RectTransform>().sizeDelta;
	//	Vector2 coord = blockPos / blockSize;
	//	coord *= 2f;
	//	coord.x = coord.x / 1.5f;
	//	coord.y = coord.y / 1.732f + coord.x / 1.732f * 2f;
	//	return new(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y));
	//}

	Vector2Int DirectionToCoord(DirectionType direction)
	{
		return direction switch
		{
			DirectionType.TopRight => Vector2Int.right,
			DirectionType.Top => Vector2Int.up,
			DirectionType.TopLeft => new(-1, 1),
			DirectionType.BottomLeft => Vector2Int.left,
			DirectionType.Bottom => Vector2Int.down,
			DirectionType.BottomRight => new(1, -1),
			_ => Vector2Int.zero,
		};
	}

	//DirectionType CoordToDirection(Vector2Int coord)
	//{
	//	if (coord == Vector2Int.right) return DirectionType.TopRight;
	//	else if (coord == Vector2Int.up) return DirectionType.Top;
	//	else if (coord.x == -1 && coord.y == 1) return DirectionType.TopLeft;
	//	else if (coord == Vector2Int.left) return DirectionType.BottomLeft;
	//	else if (coord == Vector2Int.down) return DirectionType.Bottom;
	//	else return DirectionType.BottomRight;
	//}

	async UniTask SwitchTwoBlocks(Block block1, Block block2)
	{
		Tile block1Tile = block1._AccupiedTile;
		Tile block2Tile = block2._AccupiedTile;
		block1.transform.DOMove(block2Tile.transform.position, 0.3f);
		SetBlockToTile(block2Tile, _DragStartBlock);
		await block2.transform.DOMove(block1Tile.transform.position, 0.3f);
		SetBlockToTile(block1Tile, block2);
	}

	void SetBlockToTile(Tile tile, Block block)
	{
		tile._AccupiedBlock = block;
		if (block)
		{
			block._AccupiedTile = tile;
		}
	}

	List<List<Block>> GetRows(List<Block> blocks, List<DirectionType> directions)
	{
		List<List<Block>> results = new();
		HashSet<Block> remainedBlocks = new(blocks);
		while (remainedBlocks.Count > 0) 
		{
			Block block = remainedBlocks.First();
			remainedBlocks.Remove(block);
			List<Block> row = new() { block };
			bool loop = true;
			int distance = 1;
			while (loop)
			{
				loop = false;
				foreach (DirectionType direction in directions)
				{
					Block adjacent = GetBlock(block.Coord + DirectionToCoord(direction) * distance);
					if (!remainedBlocks.Contains(adjacent)) continue;

					row.Add(adjacent);
					remainedBlocks.Remove(adjacent);
					loop = true;
				}
				distance++;
			}
			if (row.Count >= 3)
			{
				results.Add(row);
			}
		}
		return results;
	}

	List<Block> GetRow_SpecialBlock(Block block, List<DirectionType> directions)
	{
		List<Block> row = new() { block };
		bool loop = true;
		int distance = 1;
		while (loop)
		{
			loop = false;
			foreach (DirectionType direction in directions)
			{
				Block adjacent = GetBlock(block.Coord + DirectionToCoord(direction) * distance);
				if (!adjacent) continue;
				row.Add(adjacent);
				loop = true;
			}
			distance++;
		}
		return row;
	}

	bool CheckTurtleAdjacent(List<Block> blocks)
	{
		List<DirectionType> list1 = new() { DirectionType.TopRight, DirectionType.TopLeft, DirectionType.Bottom };
		List<DirectionType> list2 = new() { DirectionType.Top, DirectionType.BottomLeft, DirectionType.BottomRight };
		foreach (Block block in blocks)
		{
			bool turtleAdjacent = CheckTriangleAdjacent(block, list1);
			turtleAdjacent |= CheckTriangleAdjacent(block, list2);
			if (turtleAdjacent) return true;
		}
		return false;

		bool CheckTriangleAdjacent(Block block, List<DirectionType> list)
		{
			foreach (DirectionType direction in list)
			{
				bool triangleAdjacent = blocks.Any(x => x.Coord == block.Coord + DirectionToCoord(direction));
				if (!triangleAdjacent) return false;
			}
			return true;
		}
	}

	bool CheckBoomerangPattern(List<Block> blocks)
	{
		foreach (Block block in blocks)
		{
			foreach (List<Vector2Int> pattern in _BoomerangPatterns)
			{
				bool success = true;
				foreach (Vector2Int coord in pattern)
				{
					Block adjacent = GetBlock(block.Coord + coord);
					if (!adjacent || !blocks.Contains(adjacent))
					{
						success = false;
						break;
					}
				}
				if (success) return true;
			}
		}
		return false;
	}

	void DestroyBlock(Block block)
	{
		SetScore(_Score + 30);
		SetBlockToTile(block._AccupiedTile, null);
		Util.DestroyElement(_BlocksLinear, block);
		PlayParticle(block.transform.position).Forget();
	}

	bool CheckIsDropping()
	{
		// 빈 타일이 있을 때
		bool isDropping = false;
		foreach (Tile tile in _TilesLinear)
		{
			if (tile._Coord == _StartTileCoord) continue;
			if (!tile._AccupiedBlock)
			{
				isDropping = true;
				break;
			}
		}
		return isDropping;
	}

	void SetMissionCount(int count)
	{
		_MissionCount = Mathf.Max(0, count);
		_MissionCountText.text = _MissionCount.ToString();
	}

	void SetMoveCount(int count)
	{
		_MoveCount = count;
		_MoveCountText.text = _MoveCount.ToString();
	}

	void SetScore(int score)
	{
		_Score = score;
		_ScoreText.text = _Score.ToString("N0");
		_ScoreFillGauge.value = score / 1000f;
	}

	async UniTask PlayParticle(Vector3 pos)
	{
		ParticleSystem particle = Instantiate(_ParticlePrefab, _Canvas.transform);
		particle.gameObject.SetActive(true);
		particle.transform.position = pos;
		await UniTask.WaitForSeconds(particle.main.duration);
		Destroy(particle.gameObject);
	}

	DirectionType GetOppositeDirection(DirectionType direction) => (DirectionType)(((int)direction + 3) % 6);

	BlockType GetRandomBlockColor() => (BlockType)Random.Range(1, 6);

	/// <summary>
	/// 셔플 버튼
	/// </summary>
	public void ShuffleButton()
	{
		List<Vector2Int> coords = _BlocksLinear.Select(x => x.Coord).ToList();
		coords.ShuffleList();
		List<Vector2Int>.Enumerator coordIter = coords.GetEnumerator();
		foreach (Block block in _BlocksLinear)
		{
			coordIter.MoveNext();
			Tile tile = GetTile(coordIter.Current);
			SetBlockToTile(tile, block);
			block.transform.DOMove(tile.transform.position, 0.6f);
		}
	}

	/// <summary>
	/// 게임종료 팝업 닫기
	/// </summary>
	public void End_CloseButton()
	{
		_EndPopupTrigger = true;
	}
}
