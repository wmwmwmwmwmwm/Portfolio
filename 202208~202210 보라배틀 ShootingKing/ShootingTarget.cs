using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoraBattle.Game.ShootingKing
{
	public class ShootingTarget : MonoBehaviour
	{
		public List<SphereCollider> accuracyTargets;
		public List<ShootingTargetPiece> targetPieces;
		public MeshRenderer targetRenderer;
		[NonSerialized] public Material highlightMaterial;

		void Start()
		{
			highlightMaterial = targetRenderer.materials[1];
		}
	}
}
