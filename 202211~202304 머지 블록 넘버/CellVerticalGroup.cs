using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellVerticalGroup : MonoBehaviour
{
	public List<HorizontalLayoutGroup> horizontalGroups;

	BlockController controller => BlockController.instance;

	public Transform GetCellParentByCoord(int yCoord)
	{
		if (yCoord == controller.boardSize.y - 1) yCoord++;
		return horizontalGroups[yCoord].transform;
	}
}
