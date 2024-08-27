using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class Block_Copy : SpecialBlock
{
	[HideInInspector] public int activatedCount;

	public override async UniTask OnDropComplete()
	{
		List<Block> neighbors = controller.GetNeighborBlocks(block);
		foreach (Block neighbor in neighbors)
		{
			if (neighbor.blockTypeInGame != BlockType.Normal) continue;
			if (block.data.numberType == neighbor.data.numberType)
			{
				controller.MissionCountEffect(block).Forget();
				controller.CopyBlockStartEffect(block);
				await UniTask.Delay(300);

				block.overlayImageBeforeText.gameObject.SetActive(false);
				block.textImageParent.gameObject.SetActive(false);
				Block copiedNeighborBlock = controller.CreateNewBlockUI(neighbor.data, transform);
				copiedNeighborBlock.data.coord = block.data.coord;
				copiedNeighborBlock.transform.localScale = controller.boardScaleMultiplier * Vector3.one;
				await controller.MergeBlock(new()
				{
					allBlocks = new() { neighbor, copiedNeighborBlock },
					destinationBlock = neighbor,
					otherBlocks = new() { copiedNeighborBlock },
				});
				activatedCount++;

				block.thisAnimator.Play("Copy", 0, 0f);
				block.overlayImageBeforeText.gameObject.SetActive(true);
				block.textImageParent.gameObject.SetActive(true);
				controller.CopyBlockEndEffect(neighbor);
				controller.uiBlocks.Remove(copiedNeighborBlock);
				controller.checkBoardNeed = true;
			}
		}
	}
}
