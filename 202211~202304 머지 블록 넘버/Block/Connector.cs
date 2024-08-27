using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Connector : MonoBehaviour
{
	public ParentConstraint parentConstraint;
	public GameObject pivot;

	public (Block x1, Block x2) blocks;

	BlockController controller => BlockController.instance;

	public void SetBlocks(List<Block> _blocks)
	{
		blocks = (_blocks[0], _blocks[1]);
		parentConstraint.AddSource(new ConstraintSource()
		{
			sourceTransform = blocks.x1.transform,
			weight = 1f
		});
		transform.position = blocks.x1.transform.position;
		GetComponent<RectTransform>().sizeDelta = controller.gridCellSize;
		pivot.GetComponent<RectTransform>().sizeDelta = controller.gridCellSize;
		Vector2Int connectionDirection = blocks.x2.data.coord - blocks.x1.data.coord;
		pivot.transform.eulerAngles = new(0f, 0f, controller.DirectionToRotation(connectionDirection));
	}

	public Block GetOtherOne(Block one)
	{
		if (one == blocks.x1) return blocks.x2;
		else return blocks.x1;
	}
}
