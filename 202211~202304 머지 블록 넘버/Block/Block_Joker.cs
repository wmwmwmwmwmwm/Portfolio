using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameInterface;

public class Block_Joker : SpecialBlock
{
	public override async UniTask OnDropComplete()
	{
		List<Block> neighbors = controller.GetNeighborBlocks(block);
		List<Block> mergeNeighbors = new();
		foreach (Block neighbor in neighbors)
		{
			if (neighbor.canMerge)
			{
				mergeNeighbors.Add(neighbor);
			}
		}
		if (mergeNeighbors.Count > 0)
		{
			controller.RemoveBlock(block, false, true);
			NumberType maxNumberType = mergeNeighbors.Max(x => x.data.numberType);
			BlockData newBlockData = BlockFactoryInst.CreateBlockData(BlockType.Normal, maxNumberType);
			newBlockData.coord = block.data.coord;
			Cell cell = controller.GetBoardCell(block.data.coord);
			Block newBlock = controller.CreateNewBlockToBoard(cell, newBlockData);
			await controller.MergeBlock(new BlockController.MergeInfo()
			{
				allBlocks = new(mergeNeighbors) { newBlock },
				destinationBlock = newBlock,
				otherBlocks = new(mergeNeighbors)
			}, true, controller.itemJokerEffectPrefab);
		}
	}
}
