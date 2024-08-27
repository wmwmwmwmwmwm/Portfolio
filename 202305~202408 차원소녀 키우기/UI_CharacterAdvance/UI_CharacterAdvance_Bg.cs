using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterAdvance_Bg : MonoBehaviour
{
    [SerializeField] AnimationCurve _tweenerSpeed;

    [HideInInspector] public float _tweenerActivateTime;
    List<SeonTweener> _tweeners;

    void Start()
    {
        _tweeners = GetComponentsInChildren<SeonTweener>().ToList();
    }

    void Update()
    {
        _tweenerActivateTime += Time.unscaledDeltaTime;
        foreach (SeonTweener tweener in _tweeners)
        {
            tweener.timeScale = _tweenerSpeed.Evaluate(_tweenerActivateTime);
        }
    }
}
