using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StagePathElement : MonoBehaviour
{
	[SerializeField] GameObject _selectedMark;
	[SerializeField] Image _icon;
    [SerializeField] GameObject _on, _current, _off;
	[SerializeField] TMP_Text _stageText/*, _completeText*/;
    [SerializeField] Transform _movePivot;
    //[SerializeField] Sprite _onSprite, _currentSprite, _offSprite;
    [SerializeField] UIPlayTween _tweener;
    [SerializeField] TweenScale _tweenerOnSelected;
    //[SerializeField] Color _iconNotSelectedColor;

    StageInfo _info;
    GameObject _active;
    bool _canEnter;

    public Transform MovePivot => _movePivot;
    public StageInfo Info { get => _info; set => _info = value; }
    public bool CanEnter => _canEnter;
    public TweenScale TweenerOnSelected => _tweenerOnSelected;

    public void Setting(bool canEnter, bool isCurrent, bool isMaxStage, bool selected, bool hideCharacter)
    {
        _canEnter = canEnter;
        _selectedMark.SetActive(selected);
        _on.SetActive(false);
        _current.SetActive(false);
        _off.SetActive(false);
        _active = isCurrent ? _current : _canEnter ? _on : _off;
        _active.SetActive(true);
        //_icon.sprite = isCurrent ? _currentSprite : canEnter ? _onSprite : _offSprite;
        //_icon.color = selected ? Color.white : _iconNotSelectedColor;
        _icon.gameObject.SetActive(selected && _canEnter && !hideCharacter);
        _stageText.text = _info.ID.ToString();
        //string completeTextKey = isMaxStage ? "UI_Stage_OpenStage" : canEnter ? "UI_Stage_Cleared" :  "UI_Stage_CloseStage";
        //_completeText.text = LocalizeManager.GetText(completeTextKey);
        if (!_tweener.IsPlaying)
        {
            _tweener.Play();
        }
    }
}