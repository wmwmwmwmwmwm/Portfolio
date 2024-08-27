using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInterface;

public class Mission_DestroyIce : Mission
{
	public int targetCount;

	[HideInInspector] public int clearCount;

	public override void UpdateUI(Image image1, Image image2, List<Block> blockList)
	{
		image1.color = Color.white;
		image1.sprite = CommonResource.iceBlockSpriteIcon;
		image2.color = Color.clear;
		TMP_Text numberText = image1.transform.parent.Find("NumberText").GetComponent<TMP_Text>();
		numberText.gameObject.SetActive(false);
	}

	public override void UpdateInGameUI(TMP_Text txt)
	{
        txt.text = string.Format("X {0}", GetRemainedCount());
	}

	public override int GetRemainedCount() => Mathf.Max(targetCount - clearCount, 0);

	public override bool CheckMissionClear() => clearCount >= targetCount;

	public override Mission GetCopy()
	{
		Mission_DestroyIce copied = (Mission_DestroyIce)base.GetCopy();
		copied.targetCount = targetCount;
		copied.clearCount = clearCount;
		return copied;
	}
}