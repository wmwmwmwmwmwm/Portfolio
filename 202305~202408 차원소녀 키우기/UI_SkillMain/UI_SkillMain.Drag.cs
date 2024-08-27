using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UI_SkillMain
{
    [Header("드래그")]
    [SerializeField] Image _draggingHandle;

    SkillItem _draggingSkill;
    Coroutine _dragCoroutine;
    bool _dragIsHolding;
    bool _dragEndTrigger;
    GameObject _dragEndObject;
    float _dragHoldTime = 0.3f;
    bool _isDragElement;

    IEnumerator DragCoroutine(SkillItem skill)
    {
        _draggingSkill = skill;
        if (_isDragElement)
        {
            _dragIsHolding = true;
            yield return new WaitForSecondsRealtime(_dragHoldTime);
            _dragIsHolding = false;
        }
        //Refresh_GuideTutorialSelectValue();
        //GuideTutorialManager.Instance.OnEventButtonCleck(element.GetComponentInChildren<UI_Button>(true), UI_ButtonState.Click);

        _listScroll.enabled = false;
        SetEquipMode(true);
        _dragEndTrigger = false;
        yield return new WaitUntil(() => _dragEndTrigger);
        _dragEndTrigger = false;

        UI_SkillMain_Slot slot = null;
        if (_dragEndObject)
        {
            slot = _dragEndObject.GetComponentInParent<UI_SkillMain_Slot>();
        }

        // 장착
        if (slot)
        {
            EquipSkill(slot, _selectedWeapon, _draggingSkill);
            SelectSkill(_skillElements.Find(x => x.Skill == _draggingSkill));
            //var btn = GetSlotButton(slotIdx);
            //GuideTutorialManager.Instance.OnEventButtonCleck(btn, UI_ButtonState.Click);
        }
        // 장착 해제
        else
        {
            if (!_isDragElement)
            {
                UI_SkillMain_Slot slotAlready = _slots.Find(x => x.Skill == _draggingSkill);
                if (slotAlready)
                {
                    SetSlot(_selectedWeapon, slotAlready.Index, 0);
                }
            }
        }

        EndDrag();
    }

    void EndDrag()
    {
        _listScroll.enabled = true;
        _draggingSkill = null;
        SetEquipMode(false);
        _draggingHandle.gameObject.SetActive(false);
        _dragIsHolding = false;
        _dragCoroutine = null;
        //Refresh_GuideTutorialSelectValue();
        GuideTutorialManager.Instance.OnEventButtonCleck(null, UI_ButtonState.Click);
    }

    void OnBeginDrag(PointerEventData data)
    {
        if (IsEquipMode) return;
        if (_dragCoroutine != null) return;

        GameObject dragObject = data.pointerCurrentRaycast.gameObject;
        SkillItem skill = null;
        UI_SkillMain_SkillElement element = dragObject.GetComponentInParent<UI_SkillMain_SkillElement>();
        UI_SkillMain_Slot slot = dragObject.GetComponentInParent<UI_SkillMain_Slot>();
        if (element)
        {
            skill = element.Skill;
            _isDragElement = true;
        }
        else if (slot)
        {
            skill = slot.Skill;
            _isDragElement = false;
        }
        if (skill == null || !skill.Have) return;

        _dragCoroutine = StartCoroutine(DragCoroutine(skill));
    }

    void OnDrag(UIButtonInfo info)
    {
        if (_dragCoroutine == null) return;

        // 홀드 중 아이콘 영역을 벗어나면 드래그 취소
        if (_dragIsHolding)
        {
            GameObject dragObject = info.PointerData.pointerCurrentRaycast.gameObject;
            if (dragObject.Obj()?.GetComponentInParent<UI_SkillMain_SkillElement>().Obj()?.Skill != _draggingSkill)
            {
                StopCoroutine(_dragCoroutine);
                EndDrag();
            }
        }
        // 드래그 중인 스킬 아이콘
        else
        {
            _draggingHandle.gameObject.SetActive(true);
            _draggingHandle.sprite = AtlasManager.GetItemIcon(_draggingSkill, EIconType.Icon);
            _draggingHandle.GetComponent<RectTransform>().anchoredPosition = info.TouchPoint;
        }
    }

    void OnEndDrag(PointerEventData data)
    {
        if (_dragCoroutine == null) return;

        if (_dragIsHolding)
        {
            StopCoroutine(_dragCoroutine);
            EndDrag();
        }
        else
        {
            _dragEndObject = data.pointerCurrentRaycast.gameObject;
            _dragEndTrigger = true;
        }
    }
}
