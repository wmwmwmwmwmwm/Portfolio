using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UI_MainSkillUI
{
    [SerializeField] GameObject _draggingFront;
    [SerializeField] Image _draggingHandle;

    SkillItem _draggingSkill;
    Coroutine _dragCoroutine;
    bool _dragIsHolding;
    bool _dragEndTrigger;
    GameObject _dragEndObject;
    float _dragHoldTime = 0.3f;

    IEnumerator DragCoroutine(GameObject obj, SkillItem skill, bool isInfo)
    {
        _draggingSkill = skill;
        _dragIsHolding = true;
        yield return new WaitForSecondsRealtime(_dragHoldTime);
        _dragIsHolding = false;
        Refresh_GuideTutorialSelectValue();
        GuideTutorialManager.Instance.OnEventButtonCleck(obj.GetComponentInChildren<UI_Button>(true), UI_ButtonState.Click);

        _skillInfoScroll.enabled = false;
        _draggingFront.SetActive(isInfo);
        _dragEndTrigger = false;
        yield return new WaitUntil(() => _dragEndTrigger);
        _dragEndTrigger = false;

        if (_dragEndObject)
        {
            // 장착 : Slot 또는 Info -> Slot
            SkillIconElement skillIcon = _dragEndObject.GetComponentInParent<SkillIconElement>();
            bool isMinusTouched = _dragEndObject.name == "MinusImage";
            int slotIdx = _slotSkillItems.FindIndex(x => x == skillIcon);

            if (slotIdx >= 0)
            {
                if (!isMinusTouched)
                {
                    EquipSkill(slotIdx, _draggingSkill);
                    var btn = GetSlotButton(slotIdx);
                    GuideTutorialManager.Instance.OnEventButtonCleck(btn, UI_ButtonState.Click);
                }
            }
            // 장착 해제 : Slot -> 아무 곳
            else if (!isInfo)
            {
                SkillIconElement draggingSlot = _slotSkillItems.Find(x => x.Skill?.Id == _draggingSkill.Id);
                UnequipSkill(draggingSlot);
            }
        }
        EndDrag();
    }

    void EndDrag()
    {
        _skillInfoScroll.enabled = true;
        _draggingSkill = null;
        _draggingFront.SetActive(false);
        _draggingHandle.gameObject.SetActive(false);
        _dragIsHolding = false;
        _dragCoroutine = null;
        Refresh_GuideTutorialSelectValue();
        GuideTutorialManager.Instance.OnEventButtonCleck(null, UI_ButtonState.Click);
    }

    void OnBeginDrag(PointerEventData data)
    {
        if (_dragCoroutine != null) return;

        GameObject dragObject = data.pointerCurrentRaycast.gameObject;
        SkillItem skill;
        bool isInfo;
        SkillInfoElement infoComponent = dragObject.GetComponentInParent<SkillInfoElement>();
        GameObject o = infoComponent?.gameObject;
        if (infoComponent != null)
        {
            skill = infoComponent.Skill;
            isInfo = true;
        }
        else
        {
            var c = dragObject.GetComponentInParent<SkillIconElement>();
            skill = c.Skill;
            o = c.gameObject;
            isInfo = false;
        }
        if (skill == null) return;
        if (!skill.Have) return;

        _dragCoroutine = StartCoroutine(DragCoroutine(o, skill, isInfo));
    }

    void OnDrag(UIButtonInfo info)
    {
        if (_dragCoroutine == null) return;

        // 홀드 중 아이콘 영역을 벗어나면 드래그 취소
        if (_dragIsHolding)
        {
            GameObject dragObject = info.PointerData.pointerCurrentRaycast.gameObject;
            if (dragObject?.Obj()?.GetComponentInParent<SkillInfoElement>()?.Obj()?.Skill != _draggingSkill)
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
