using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Numerics;

public class UI_PetElement : MonoBehaviour
{
    [SerializeField] UI_Button _uiButton;
    [SerializeField] Image _bg, _portrait, _leaderMark;
    [SerializeField] Image _minusImage;
    [SerializeField] GameObject _accompanyFront;
    [SerializeField] TMP_Text _levelText, _advanceText;
    [SerializeField] List<GameObject> _stars;
    [SerializeField] Image _outline_Selected;
    [SerializeField] GameObject _lock;
    [SerializeField] GameObject _newIcon;
    [SerializeField] Slider _countSlider;
    [SerializeField] TMP_Text _countText;
    [SerializeField] RedDotComponent _redDot;

    PetInfoItem _info;
    public PetInfoItem Info => _info;

    public void Setting(PetInfoItem petInfo, bool selected, bool accompany)
    {
        _info = petInfo;
        if (_info != null)
        {
            string id = _info != null ? $"{UI_Pet.EButtonId.Pet}__{_info.Id}" : "";
            _uiButton.SetID(id);
            _bg.sprite = InGameUtil.GetGradeItemBg(_info.itemData.Grade);
            _portrait.color = Color.white;
            _portrait.sprite = AtlasManager.GetItemIcon(_info.PetData, EIconType.Icon);
            for (int i = 0; i < _stars.Count; i++)
            {
                _stars[i].SetActive(i < _info.itemData.Star);
            }
            _levelText.text = $"Lv.{_info.Level}";
            _advanceText.text = $"+{_info.AdvanceStar}";
            _outline_Selected.gameObject.SetActive(selected);
            _accompanyFront.SetActive(accompany);
            _leaderMark.gameObject.SetActive(accompany);
            _lock.SetActive(!_info.Have);
            _newIcon.SetActive(_info.New);
            int count = (int)_info.ConsumableCount;
            _countSlider.value = count / 5f;
            _countText.text = $"{count}/5";
            _redDot.SetActiveRedDot(_info.CanLevelUp());
        }
        else
        {
            _uiButton.SetID("");
            _bg.sprite = InGameUtil.GetGradeItemBg(EItemGrade.E);
            _portrait.color = Color.clear;
            for (int i = 0; i < _stars.Count; i++)
            {
                _stars[i].SetActive(false);
            }
            _levelText.text = "";
            _advanceText.text = "";
            _outline_Selected.gameObject.SetActive(false);
            _accompanyFront.SetActive(false);
            _leaderMark.gameObject.SetActive(false);
            _lock.SetActive(false);
            _newIcon.SetActive(false);
            _countSlider.value = 0f;
            _countText.text = $"0/5";
            _redDot.SetActiveRedDot(false);
        }
    }
}
