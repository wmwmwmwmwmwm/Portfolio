using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GameInterface;

public class Mission_TargetCell : Mission
{
    public NumberType targetNumber;

	int clearCount
    {
        get
        {
            int clearCount = 0;
            foreach (Cell boardCell in GetMissionCells())
            {
                if (boardCell.accupiedBlock && boardCell.accupiedBlock.canMerge && boardCell.accupiedBlock.data.numberType == targetNumber)
                {
                    clearCount++;
                }
            }
            return clearCount;
        }
	}

	public override void UpdateUI(Image image1, Image image2, List<Block> blockList)
    {
		image1.color = Color.white;
		image1.sprite = CommonResource.targetCellSprite1;
		Block uiBlock = BlockFactoryInst.CreateBlock(BlockType.Normal, targetNumber, image1.transform);
        float parentScale = image1.GetComponent<RectTransform>().sizeDelta.x / uiBlock.GetComponent<RectTransform>().sizeDelta.x;
		uiBlock.transform.localScale = Vector3.one * parentScale;
		uiBlock.transform.SetParent(image1.transform);
		blockList.Add(uiBlock);
		image2.color = Color.white;
		image2.sprite = CommonResource.targetCellSprite2;
		TMP_Text numberText = image1.transform.parent.Find("NumberText").GetComponent<TMP_Text>();
        numberText.text = TypeHelper.NumberTypeToString(targetNumber);
		numberText.gameObject.SetActive(true);
	}

	public override void UpdateInGameUI(TMP_Text txt)
    {
        txt.text = string.Format("X {0}", GetRemainedCount());
	}

	public override int GetRemainedCount() => Mathf.Max(controller.stageData.stageCells.Count - clearCount, 0);

	public override bool CheckMissionClear() => clearCount >= GetMissionCells().Count;

    public List<Cell> GetMissionCells()
    {
        List<Vector2Int> dataCoords = controller.stageData.stageCells.Select(x => x.coord).ToList();
        return controller.boardCellsLinear.FindAll(x => dataCoords.Contains(x.data.coord));
    }

    public override Mission GetCopy()
    {
        Mission_TargetCell copied = (Mission_TargetCell)base.GetCopy();
        copied.targetNumber = targetNumber;
        return copied;
    }
}