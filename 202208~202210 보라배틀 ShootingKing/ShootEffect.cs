using System.Collections;
using UnityEngine;

namespace BoraBattle.Game.ShootingKing
{
	public class ShootEffect : MonoBehaviour
	{
		[Header("Random muzzle effects")]
		public Sprite[] randomMuzzleSideSprite;
		public Sprite[] randomMuzzleSprite;

		[Header("Muzzle positions")]
		public GameObject muzzleFlash;
		public GameObject sideMuzzle;
		public GameObject topMuzzle;

		[Header("Casing prefab")]
		public Transform casing;
		public Transform casingSpawnPoint;

		void Start()
		{
			muzzleFlash.GetComponent<Renderer>().enabled = false;
			sideMuzzle.GetComponent<Renderer>().enabled = false;
			topMuzzle.GetComponent<Renderer>().enabled = false;
		}

		public IEnumerator ShowRifleShootEffect()
		{
			ShootingController.instance.PlaySFX(ShootingController.instance.sfxGunShoot);
			casing.transform.rotation = casingSpawnPoint.transform.rotation;
			Instantiate(casing, casingSpawnPoint.transform.position, casingSpawnPoint.transform.rotation);
			muzzleFlash.GetComponent<SpriteRenderer>().sprite = randomMuzzleSprite[Random.Range(0, randomMuzzleSprite.Length)];
			sideMuzzle.GetComponent<SpriteRenderer>().sprite = randomMuzzleSideSprite[Random.Range(0, randomMuzzleSideSprite.Length)];
			topMuzzle.GetComponent<SpriteRenderer>().sprite = randomMuzzleSideSprite[Random.Range(0, randomMuzzleSideSprite.Length)];
			muzzleFlash.transform.Rotate(new Vector3(0f, 0f, Random.Range(0f, 360f)));
			muzzleFlash.GetComponent<SpriteRenderer>().enabled = true;
			sideMuzzle.GetComponent<SpriteRenderer>().enabled = true;
			topMuzzle.GetComponent<SpriteRenderer>().enabled = true;
			yield return new WaitForSeconds(0.3f);
			muzzleFlash.GetComponent<SpriteRenderer>().enabled = false;
			sideMuzzle.GetComponent<SpriteRenderer>().enabled = false;
			topMuzzle.GetComponent<SpriteRenderer>().enabled = false;
		}
	}
}