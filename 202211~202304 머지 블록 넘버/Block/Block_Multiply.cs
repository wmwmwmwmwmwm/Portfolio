using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class Block_Multiply : SpecialBlock
{
	public override async UniTask OnDropComplete()
	{
		List<Block> neighbors = controller.GetNeighborBlocks(block);
		List<Block> mergeNeighbors = new();
		foreach (Block neighbor in neighbors)
		{
			if (neighbor.canMerge && neighbor.data.numberType <= NumberType.N1024M)
			{
				mergeNeighbors.Add(neighbor);
			}
		}
		Block blockToMerge = null;
		Vector2Int[] directions = new[] { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };
		blockToMerge = mergeNeighbors.Find(x => x.data.coord - block.data.coord == Vector2Int.down);
		foreach (Vector2Int direction in directions)
		{
			blockToMerge = mergeNeighbors.Find(x => x.data.coord - block.data.coord == direction);
			if (blockToMerge != null)
			{
				await controller.MergeBlock(new BlockController.MergeInfo()
				{
					allBlocks = new() { block, blockToMerge },
					destinationBlock = blockToMerge,
					otherBlocks = new() { block }
				}, true);
				break;
			}
		}
	}
}
