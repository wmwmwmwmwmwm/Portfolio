using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInterface;

public class Mission_Number : Mission
{
	public NumberType missionNumberType;
	public int targetCount;

	[HideInInspector] public int clearCount;

	public override void UpdateUI(Image image1, Image image2, List<Block> blockList)
	{
		image1.color = Color.clear;
		Block uiBlock = BlockFactoryInst.CreateBlock(BlockType.Normal, missionNumberType, image1.transform);
		float parentScale = image1.GetComponent<RectTransform>().sizeDelta.x / uiBlock.GetComponent<RectTransform>().sizeDelta.x;
		uiBlock.transform.localScale = Vector3.one * parentScale;
		blockList.Add(uiBlock);
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
		Mission_Number copied = (Mission_Number)base.GetCopy();
		copied.missionNumberType = missionNumberType;
		copied.targetCount = targetCount;
		copied.clearCount = clearCount;
		return copied;
	}
}
