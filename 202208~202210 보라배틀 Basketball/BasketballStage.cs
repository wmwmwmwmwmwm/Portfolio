using System;
using UnityEngine;
using static BoraBattle.Game.WorldBasketballKing.BasketballController;

namespace BoraBattle.Game.WorldBasketballKing
{
	public class BasketballStage : MonoBehaviour
	{
		public eMoveKind moveKind;

		[NonSerialized] public Transform rim;
		[NonSerialized] public Transform rimCenter;
		[NonSerialized] public Animator netAnimator;
		[NonSerialized] public Transform startBlockPad;
		[NonSerialized] public Material wallFrameMaterial;
		[NonSerialized] public float initialBrightness;

		public void Initialize()
		{
			rim = transform.FindRecursive("Rim");
			rimCenter = transform.FindRecursive("RimCenter");
			netAnimator = transform.FindRecursive("net_ani3").GetComponent<Animator>();
			startBlockPad = transform.FindRecursive("StartBlockPad");
			wallFrameMaterial = transform.FindRecursive("WallFrame").GetComponent<MeshRenderer>().material;
			Color.RGBToHSV(wallFrameMaterial.GetColor("_Color"), out _, out _, out initialBrightness);
		}
	}
}