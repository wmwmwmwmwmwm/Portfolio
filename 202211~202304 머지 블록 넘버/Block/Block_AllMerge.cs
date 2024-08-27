using Cysharp.Threading.Tasks;
using UnityEngine;

public class Block_AllMerge : SpecialBlock
{
	public Sprite upDirectionSprite, downDirectionSprite, leftDirectionSprite, rightDirectionSprite;
	BlockAllMergeData data;

	public override void Start()
	{
		data = (BlockAllMergeData)block.data;
		if (data.direction == Vector2Int.up) block.baseImage.sprite = upDirectionSprite;
		else if (data.direction == Vector2Int.down) block.baseImage.sprite = downDirectionSprite;
		else if (data.direction == Vector2Int.left) block.baseImage.sprite = leftDirectionSprite;
		else block.baseImage.sprite = rightDirectionSprite;
	}

	public override async UniTask OnDropComplete()
	{
		Block blockToMerge = controller.GetBoardBlock(data.coord + data.direction);
		if (!blockToMerge || !blockToMerge.canMerge) return;
		await controller.MergeBlock(new BlockController.MergeInfo()
		{
			allBlocks = new() { block, blockToMerge },
			destinationBlock = blockToMerge,
			otherBlocks = new() { block }
		});
		controller.AllMergeBlockEffect(blockToMerge.transform.position);
	}
}
