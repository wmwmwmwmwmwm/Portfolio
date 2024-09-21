using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
	public Image _Bg;

	[ReadOnly] public Vector2Int _Coord;
	[ReadOnly] public Block _AccupiedBlock;

	[HideInInspector] public bool _FallFromRight;
}
