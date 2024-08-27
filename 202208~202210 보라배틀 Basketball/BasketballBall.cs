using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoraBattle.Game.WorldBasketballKing
{
	public class BasketballBall : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		public ParticleSystem fireEffect, trailEffect;

		[NonSerialized] public bool isCleanShot;
		[NonSerialized] public bool topGoalPass, bottomGoalPass, goal;
		[NonSerialized] public bool middleColliderPass;

		[NonSerialized] public Rigidbody thisRigidbody;
		[NonSerialized] public Collider thisCollider;
		[NonSerialized] public ParticleSystem.EmissionModule fireEmission;
		[NonSerialized] public ParticleSystem.EmissionModule trailEmission;

		BasketballController controller => BasketballController.instance;
		bool canGrab => controller.isPlaying && transform.position.z < -8f;

		void Awake()
		{
			thisRigidbody = GetComponent<Rigidbody>();
			thisCollider = GetComponent<Collider>();
			SetParticleState(false);
		}

		void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.GetComponent<BasketballBall>()) return;
			else if (collision.gameObject.name.Contains("Frame"))
			{
				controller.PlaySFX(controller.sfxBounce);
			}
			else
			{
				isCleanShot = false;
				if (collision.transform.parent?.name.Contains("Rim") == true)
				{
					controller.PlaySFX(controller.sfxRim);
				}
			}
		}

		void OnTriggerEnter(Collider collider)
		{
			if (collider.gameObject.name == "TopGoalCheck" && !bottomGoalPass)
			{
				topGoalPass = true;
				controller.NetAnimation();
			}
			else if (collider.gameObject.name == "BottomGoalCheck")
			{
				bottomGoalPass = true;
				controller.NetAnimation();
			}

			if (topGoalPass && bottomGoalPass && !goal)
			{
				goal = true;
				controller.Goal(this);
			}

			if (collider == controller.middleCollider)
			{
				middleColliderPass = true;
			}
			else if (collider == controller.groundCollider)
			{
				if (middleColliderPass && !goal)
				{
					controller.combo = 0;
				}
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!canGrab) return;
			controller.OnBallPointerDown(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!canGrab) return;
			controller.OnBallDrag(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (!canGrab) return;
			controller.OnBallPointerUp(eventData);
		}

		public void SetParticleState(bool state)
		{
			fireEmission = fireEffect.emission;
			trailEmission = trailEffect.emission;
			fireEmission.enabled = state;
			trailEmission.enabled = state;
		}
	}
}