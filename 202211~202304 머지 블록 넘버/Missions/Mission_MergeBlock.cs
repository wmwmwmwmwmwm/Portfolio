using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInterface;

public class Mission_MergeBlock : Mission
{
	public override void UpdateUI(Image image1, Image image2, List<Block> blockList)
	{
		image1.color = Color.white;
		image1.sprite = CommonResource.nextBlockSprite;
		image2.color = Color.white;
		image2.sprite = CommonResource.mergeBlockSprite;
		TMP_Text numberText = image1.transform.parent.Find("NumberText").GetComponent<TMP_Text>();
		numberText.gameObject.SetActive(false);
	}

	public override void UpdateInGameUI(TMP_Text txt)
	{
        txt.text = string.Format("X {0}", GetRemainedCount());
	}

	public override int GetRemainedCount() => controller.boardBlocks.FindAll(x => x.blockTypeInGame == BlockType.Target).Count;

	public override bool CheckMissionClear() => GetRemainedCount() == 0;

	public override Mission GetCopy()
	{
		Mission_MergeBlock copied = (Mission_MergeBlock)base.GetCopy();
		return copied;
	}
}

