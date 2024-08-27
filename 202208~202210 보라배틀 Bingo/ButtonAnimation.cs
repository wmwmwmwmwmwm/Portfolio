using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Button))]
    public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        Animator thisAnimator;
        Button thisButton;

        void Awake()
        {
            thisAnimator = GetComponent<Animator>();
            thisButton = GetComponent<Button>();
        }

        void OnEnable()
        {
            thisAnimator.Play("Release", false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!thisButton.interactable) return;
            thisAnimator.Play("Press", true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            thisAnimator.Play("Release", true);
        }
    }
}

