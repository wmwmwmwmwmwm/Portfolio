using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoNumberBox : MonoBehaviour
	{
		[Header("Reference")]
		public Button button;
		public BingoImageNumber blackImageNumber, whiteImageNumber;
		public Image baseImage, greyBackground, greyStar, redStar;
		public Transform images;

        [Header("Asset")]
        public Sprite bingo1;
        public Sprite bingo2;
        public Sprite bingo3;
        public Sprite bingo4;
        public Sprite bingo5;

        public enum State { NumberUnmarked, NumberMarked, Bingo1, Bingo2, Bingo3, Bingo4, Bingo5 };
        [NonSerialized] public State state;
		[NonSerialized] public int number;
		[NonSerialized] public int boardIndex;
		[NonSerialized] public Vector2Int boardCoord;

        BingoController Controller => BingoController.instance;
        Transform numberBoxProceed => Controller.numberBoxProceed;
        public bool IsChecked => state != State.NumberUnmarked;
		bool IsBingoState => state is State.Bingo1 or State.Bingo2 or State.Bingo3 or State.Bingo4 or State.Bingo5;

        public void SetState(State newState)
		{
			state = newState;
			switch (state)
			{
				case State.NumberUnmarked:
					button.enabled = true;
					break;
				case State.NumberMarked:
				case State.Bingo1:
				case State.Bingo2:
				case State.Bingo3:
				case State.Bingo4:
				case State.Bingo5:
					button.enabled = false;
					break;
			}
		}

		public void SetNumber(int newNumber)
		{
			number = newNumber;
			blackImageNumber.SetNumber(number);
			whiteImageNumber.SetNumber(number);
		}

		public void UpdateSprite(bool hideNumber = false)
		{
			baseImage.enabled = false;
            blackImageNumber.gameObject.SetActive(false);
			whiteImageNumber.gameObject.SetActive(false);
			greyBackground.gameObject.SetActive(false);
			greyStar.gameObject.SetActive(false);
			redStar.gameObject.SetActive(false);
            button.interactable = state == State.NumberUnmarked;
            switch (state)
			{
				case State.NumberUnmarked:
                    baseImage.enabled = true;
                    blackImageNumber.gameObject.SetActive(true);
					break;
				case State.NumberMarked:
                    if (!hideNumber)
                    {
                        greyBackground.gameObject.SetActive(true);
                        whiteImageNumber.gameObject.SetActive(true);
					}
					else
					{
						greyStar.gameObject.SetActive(true);
					}
					break;
				case State.Bingo1:
				case State.Bingo2:
				case State.Bingo3:
				case State.Bingo4:
				case State.Bingo5:
                    greyBackground.gameObject.SetActive(true);
                    greyStar.gameObject.SetActive(true);
					redStar.gameObject.SetActive(true);
                    redStar.sprite = GetBingoSprite();
                    break;
            }
        }

        public IEnumerator ProceedAnimation(Transform board)
        {
			if (IsBingoState)
			{
                images.gameObject.SetActive(false);
                Transform proceedEffect = Instantiate(numberBoxProceed, board);
                proceedEffect.gameObject.SetActive(true);
				proceedEffect.position = transform.position;
				Image proceedImage = proceedEffect.GetComponentInChildren<Image>();
				proceedImage.sprite = GetBingoSprite();
				Animator proceedAnimator = proceedEffect.GetComponentInChildren<Animator>();
				proceedAnimator.Play(BingoController.singleAnimation, true);
				yield return new WaitUntil(() => proceedAnimator.Done());
				images.gameObject.SetActive(true);
				Destroy(proceedEffect.gameObject);
            }
			ParticleSystem prefab = IsBingoState ? Controller.bingoEffectPrefab : Controller.markEffectPrefab;
            ParticleSystem effect = Instantiate(prefab, board);
			effect.transform.position = transform.position;
            Color color = number switch
            {
                <= 15 => Controller.colorB,
                <= 30 => Controller.colorI,
                <= 45 => Controller.colorN,
                <= 60 => Controller.colorG,
                _ => Controller.colorO
            };
            ParticleSystem[] childs = effect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem child in childs)
            {
                ParticleSystem.MainModule main = child.main;
				float alpha = main.startColor.color.a;
                main.startColor = color.WithAlpha(alpha);
            }
            yield return new WaitForSeconds(effect.main.duration);
            Destroy(effect.gameObject);
        }

        Sprite GetBingoSprite()
		{
			return state switch
			{
				State.Bingo1 => bingo1,
				State.Bingo2 => bingo2,
				State.Bingo3 => bingo3,
				State.Bingo4 => bingo4,
				State.Bingo5 => bingo5,
				_ => throw new NotImplementedException()
			};
        }
    }

    public class BingoNumberBoxComparer : IEqualityComparer<BingoNumberBox>
	{
		public bool Equals(BingoNumberBox x, BingoNumberBox y) => x.number == y.number && x.number == y.number;
		public int GetHashCode(BingoNumberBox obj) => obj.number.GetHashCode() ^ obj.number.GetHashCode();
	}
}
