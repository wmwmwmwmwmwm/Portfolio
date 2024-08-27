using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static GameInterface;
using static BigIntegerUtil;

public class Mission_Score : Mission
{
	public long targetScore;

	public override void UpdateUI(Image image1, Image image2, List<Block> blockList)
	{
		image1.color = Color.white;
		image1.sprite = CommonResource.scoreSprite;
		image2.color = Color.clear;
		TMP_Text numberText = image1.transform.parent.Find("NumberText").GetComponent<TMP_Text>();
		numberText.gameObject.SetActive(false);
	}

	public override void UpdateInGameUI(TMP_Text txt)
	{
        txt.text = string.Format("{0}", GetRemainedCount().ToString("N0"));
	}

	public override int GetRemainedCount() => (int)Mathf.Max(targetScore - BigIntToFloat(controller.score), 0);

	public override bool CheckMissionClear() => controller.score >= targetScore;

    public override Mission GetCopy()
    {
        Mission_Score copied = (Mission_Score)base.GetCopy();
        copied.targetScore = targetScore;
        return copied;
    }
}