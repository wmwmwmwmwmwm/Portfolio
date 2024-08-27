using Cysharp.Threading.Tasks;
using UnityEngine;

public class Block_Ice : SpecialBlock
{
	public async UniTask OnDropComplete_Ice()
	{
		foreach (Block neighbor in controller.GetNeighborBlocks(block))
		{
			if (neighbor.blockTypeInGame == BlockType.Normal && block.data.numberType == neighbor.data.numberType)
			{
				controller.iceBlocksToDestroyPair.Add((neighbor, block));
            }
		}
		await UniTask.NextFrame();
		if(controller.iceBlocksToDestroyPair.Count > 0) { Debug.Log("OnDropComplete_Ice"); }
    }
}
