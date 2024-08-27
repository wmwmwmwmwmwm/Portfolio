using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInterface;

public class Mission_CopyBlock : Mission
{
	public int targetCount;

	int totalCopyCount
	{
		get
		{
			int totalCount = 0;
			List<Block> copyBlocks = controller.boardBlocks.FindAll(x => x.blockTypeInGame == BlockType.Copy);
			foreach (Block copyBlock in copyBlocks)
			{
				totalCount += copyBlock.GetComponent<Block_Copy>().activatedCount;
			}
			return totalCount;
		}
	}

	public override void UpdateUI(Image image1, Image image2, List<Block> blockList)
	{
		image1.color = Color.white;
		image1.sprite = CommonResource.copyBlockSprite1;
		image2.color = Color.white;
		image2.sprite = CommonResource.copyBlockSprite2;
		TMP_Text numberText = image1.transform.parent.Find("NumberText").GetComponent<TMP_Text>();
		numberText.gameObject.SetActive(false);
	}

	public override void UpdateInGameUI(TMP_Text txt)
	{
		txt.text = string.Format("X {0}", GetRemainedCount());
	}

	public override int GetRemainedCount() => Mathf.Max(targetCount - totalCopyCount, 0);

	public override bool CheckMissionClear() => totalCopyCount >= targetCount;

	public override Mission GetCopy()
	{
		Mission_CopyBlock copied = (Mission_CopyBlock)base.GetCopy();
		copied.targetCount = targetCount;
		return copied;
	}
}