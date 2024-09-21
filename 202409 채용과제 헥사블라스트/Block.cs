using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
	public Image _Image;
	public GameObject _Special;
	public TMP_Text _SpecialText;

	[ReadOnly] public Tile _AccupiedTile;
	[ReadOnly] public int _SpinnerHP;

	[HideInInspector] public DraggableUI _DraggableUI;
	[HideInInspector] public BlockType _BlockType;
	[HideInInspector] public SpecialBlockType _SpecialType;
	public Queue<Vector2Int> _MoveQueue;
	public bool _SpecialActivated;

	Vector2Int? _MoveDest;
	float _MoveVelocity;
	float _MoveAccel;

	public HexaBlastController Controller => HexaBlastController.Instance;
	public Vector2Int Coord => _AccupiedTile._Coord;

	public void Init()
	{
		_DraggableUI = GetComponent<DraggableUI>();
		_MoveQueue = new();
		_SpinnerHP = 2;
		_MoveAccel = 4000f;
	}

	void Update()
	{
		if (_MoveDest == null)
		{
			if (_MoveQueue.Count == 0) return;
			_MoveDest = _MoveQueue.Dequeue();
		}
		Tile destTile = Controller.GetTile(_MoveDest.Value);
		Vector3 distanceVector = destTile.transform.position - transform.position;
		_MoveVelocity += _MoveAccel * Time.deltaTime;
		Vector3 moveAmount = _MoveVelocity * Time.deltaTime * distanceVector.normalized;
		if (moveAmount.sqrMagnitude < distanceVector.sqrMagnitude)
		{
			transform.position += moveAmount;
		}
		else
		{
			transform.position = destTile.transform.position;
			_MoveDest = null;
			if (_MoveQueue.Count == 0)
			{
				_MoveVelocity = 0f;
			}
		}
	}

	public void SetBlockType(BlockType blockType, SpecialBlockType specialType)
	{
		_BlockType = blockType;
		_Image.sprite = Controller._BlockSprites[(int)blockType];
		_SpecialType = specialType;
		_Special.SetActive(_SpecialType != SpecialBlockType.None);
		_SpecialText.text = specialType switch
		{
			SpecialBlockType.Boomerang => "BOOMERANG",
			SpecialBlockType.Missile => "MISSILE",
			SpecialBlockType.TNT => "TNT",
			SpecialBlockType.Turtle => "TURTLE",
			SpecialBlockType.MirrorBall => "MIRROWBALL",
			_ => "",
		};
	}

	public bool CanMerge()
	{
		return _BlockType switch
		{
			BlockType.None or BlockType.Spinner => false,
			_ => true
		};
	}

	public bool IsMissionBlock()
	{
		return _BlockType switch
		{
			BlockType.Spinner => true,
			_ => false
		};
	}
}
