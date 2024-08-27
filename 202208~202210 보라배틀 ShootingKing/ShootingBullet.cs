using DG.Tweening;
using UnityEngine;

namespace BoraBattle.Game.ShootingKing
{
	public class ShootingBullet : MonoBehaviour
	{
		public Transform windArrow;

		Rigidbody thisRigidbody;
		Vector3 velocity;
		RaycastHit[] hits;

		ShootingController controller => ShootingController.instance;

		void Start()
		{
			thisRigidbody = GetComponent<Rigidbody>();
			velocity = transform.forward * 30f;
			hits = new RaycastHit[20];
			windArrow.DOLocalRotate(Vector3.forward * 2160f, 1f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
		}

		void OnDestroy()
		{
			windArrow.DOKill();
		}

		void FixedUpdate()
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			velocity += controller.windVector * fixedDeltaTime;
			thisRigidbody.MovePosition(thisRigidbody.position + velocity * fixedDeltaTime);

			// 정면 콜라이더 체크
			if (!controller.bulletCollidedObject)
			{
				Ray ray = new(thisRigidbody.position - transform.forward * 1f, transform.forward);
				int hitCount = Physics.RaycastNonAlloc(ray, hits, 2f);
				ShootingTargetPiece highscoreTargetPiece = null;
				Vector3 highscoreTargetPoint = Vector3.zero;
				int highscore = int.MinValue;
				for (int i = 0; i < hitCount; i++)
				{
					if (hits[i].collider.TryGetComponent(out ShootingTargetPiece targetPiece))
					{
						if (targetPiece.score > highscore)
						{
							highscore = targetPiece.score;
							highscoreTargetPiece = targetPiece;
							highscoreTargetPoint = hits[i].point;
						}
					}
				}
				controller.bulletCollidedObject = highscoreTargetPiece;
				controller.bulletCollidedPoint = highscoreTargetPoint;
			}
		}
	}
}