using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class HexaBlastController
{
	Block _DragStartBlock;
	Block _DragDestBlock;
	bool _IsDragging;
	bool _DragInputTrigger;
	DirectionType _DragDirection;

	void OnBeginDrag(GameObject draggingBlock, PointerEventData data)
	{
		if (_IsDragging) return;
		_DragStartBlock = draggingBlock.GetComponent<Block>();
		_IsDragging = true;
	}

	void OnDrag(GameObject draggingBlock, PointerEventData data)
	{
		if (!_IsDragging) return;
		Vector2 dragVector = data.position - draggingBlock.transform.position.Vector2();
		if (dragVector.magnitude < 50f) return;

		// 블록 이동 방향 설정
		float zRot = Util.DirectionToRotationZ(dragVector);
		zRot = Util.Mod(zRot, 360f);
		_DragDirection = (DirectionType)(zRot / 360f * 6f);
		_IsDragging = false;
		_DragInputTrigger = true;
	}
}
