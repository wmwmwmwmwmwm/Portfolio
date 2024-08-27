using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoraBattle.Game.ShootingKing
{
	public class ShootingStage : MonoBehaviour
	{
		public List<ShootingTarget> targets;

		void Start()
		{
			DOTweenAnimation[] targetTweens = GetComponentsInChildren<DOTweenAnimation>();
			foreach (DOTweenAnimation targetTween in targetTweens)
			{
				targetTween.tween.SetUpdate(UpdateType.Manual);
			}
		}

		public float GetAccuracy(Vector3 point)
		{
			float highestAccuracy = float.MinValue;
			foreach (ShootingTarget target in targets)
			{
				foreach (SphereCollider accuracyTarget in target.accuracyTargets)
				{
					float globalScale = (Mathf.Abs(accuracyTarget.transform.lossyScale.x) + Mathf.Abs(accuracyTarget.transform.lossyScale.y)) / 2f;
					float worldScaleRadius = accuracyTarget.radius * globalScale;
					float accuracy = 1f - Vector2.Distance(point, accuracyTarget.transform.position) / worldScaleRadius;
					if (accuracy > highestAccuracy)
					{
						highestAccuracy = accuracy;
					}
				}
			}
			return Mathf.Max(0f, highestAccuracy);
		}

		public IEnumerator HighlightTarget(ShootingTargetPiece targetPiece)
		{
			ShootingTarget target = targets.Find(x => x.targetPieces.Contains(targetPiece));
			if (!target || !targetPiece.highlightMask)
			{
				yield break;
			}

			target.highlightMaterial.SetTexture("_MainTex", targetPiece.highlightMask);
			while (true)
			{
				target.highlightMaterial.SetColor("_TintColor", Color.red);
				yield return new WaitForSeconds(0.2f);
				target.highlightMaterial.SetColor("_TintColor", Color.clear);
				yield return new WaitForSeconds(0.2f);
			}
		}

		public void ResetHighlight()
		{
			foreach (ShootingTarget target in targets)
			{
				target.highlightMaterial.SetColor("_TintColor", Color.clear);
			}
		}
	}
}
