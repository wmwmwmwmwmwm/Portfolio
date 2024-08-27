using UnityEngine;

namespace BoraBattle.Game.ShootingKing
{
	[RequireComponent(typeof(Collider))]
	public class ShootingTargetPiece : MonoBehaviour
	{
		public int score;
		public bool noBulletHole;
		public bool applyAccuracyBonus;
		public Texture highlightMask;
	}
}
