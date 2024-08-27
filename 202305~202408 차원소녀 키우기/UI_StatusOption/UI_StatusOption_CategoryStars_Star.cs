using Coffee.UIExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_StatusOption_CategoryStars_Star : MonoBehaviour
{
    [SerializeField] GameObject _bigEffect, _smallEffect;
    [SerializeField] UIParticle _upgradeEffect;
    [SerializeField] bool _isBig;

    //UI_EffectComponent _effectComponent;

    public void Setting(bool isOpen)
    {
        //_effectComponent = GetComponentInParent<UI_EffectComponent>();
        if (isOpen)
        {
            _bigEffect.SetActive(_isBig);
            _smallEffect.SetActive(!_isBig);
            _smallEffect.transform.localScale = Vector3.one;
        }
        else
        {
            _bigEffect.SetActive(false);
            _smallEffect.SetActive(true);
            _smallEffect.transform.localScale = Vector3.one * 0.4f;
        }
    }

    public void PlayUpgradeEffect()
    {
        //_effectComponent.Play2DEffect(id: "Fx_UI_Option_Icon_Star_Upgade_01",
        //    position: _upgradeEffect.localPosition,
        //    parent: _upgradeEffect.parent);
        _upgradeEffect.Play();
    }
}
