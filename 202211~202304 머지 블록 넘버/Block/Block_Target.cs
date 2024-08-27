using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Block_Target : SpecialBlock
{
	bool activated;

	public override void OnBeforeMerge()
	{
		if (activated) return;
		activated = true;
		controller.MissionCountEffect(block).Forget();
		controller.TargetBlockEffect(block);
		block.blockTypeInGame = BlockType.Normal;
		Destroy(block.blockTargetImage.gameObject);
		//OverlayImageAnimation();
	}

	//async void OverlayImageAnimation()
	//{
	//	Image overlayImage = block.blockTargetImage;
	//	overlayImage.transform.SetParent(controller.boardParticleFrontLayer);
	//	List<UniTask> tasks = new()
	//	{
	//		overlayImage.transform.DOMoveY(transform.position.y - 600f, 0.4f).SetEase(Ease.InBack).ToUniTask(),
	//		overlayImage.transform.DOMoveX(transform.position.x + Random.Range(-80f, 80f), 0.4f).SetEase(Ease.Linear).ToUniTask(),
	//		overlayImage.transform.DOScale(0f, 0.4f).SetEase(Ease.InCubic).ToUniTask(),
	//		overlayImage.transform.DORotate(Vector3.back * 1080f, 0.4f).SetEase(Ease.Linear).ToUniTask()
	//	};
	//	await tasks;
	//	Destroy(overlayImage.gameObject);
	//}
}
