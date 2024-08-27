using Cysharp.Threading.Tasks;
using UnityEngine;

public class Block_Escape : SpecialBlock
{
	public Animator escapeAnimator;

	public override async UniTask OnDropComplete()
	{
		if (block.data.coord.y == 0)
		{
			controller.escapeBlocksToDestroy.Add(block);
		}
		await UniTask.NextFrame();
	}

	public void PlayOnAnimation() => escapeAnimator.Play("EscapeOn_Animation");
	public void PlayFlyAnimation() => escapeAnimator.Play("EscapeFly_Animation");
}
