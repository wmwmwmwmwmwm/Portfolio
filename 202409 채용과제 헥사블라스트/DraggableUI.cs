using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public Action<GameObject, PointerEventData> ClickCallback;
	public Action<GameObject, PointerEventData> BeginDragCallback;
	public Action<GameObject, PointerEventData> DragCallback;
	public Action<GameObject, GameObject, PointerEventData> EndDragCallback;

	GameObject _DraggingObject;

	public void OnPointerClick(PointerEventData eventData)
	{
		ClickCallback?.Invoke(eventData.pointerPress, eventData);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_DraggingObject = eventData.pointerPress;
		Graphic graphic = _DraggingObject.GetComponent<Graphic>();
		if (graphic) 
		{
			graphic.raycastTarget = false;
		}
		BeginDragCallback?.Invoke(_DraggingObject, eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		DragCallback?.Invoke(_DraggingObject, eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		Graphic graphic = _DraggingObject.GetComponent<Graphic>();
		if (graphic)
		{
			graphic.raycastTarget = true;
		}
		_DraggingObject = null;
		GameObject dropPlaceObject = eventData.pointerEnter;
		EndDragCallback?.Invoke(_DraggingObject, dropPlaceObject, eventData);
	}
}
