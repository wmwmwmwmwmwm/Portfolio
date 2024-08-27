using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete]
public class UI_SkillDetail_Info : MonoBehaviour
{
    [SerializeField] TranscInfoElement _infoPrefab;
    [SerializeField] Transform _infoParent;

    SkillItem _skill;
    UI_SkillDetail _parent;
    List<TranscInfoElement> _infoElements = new();

    public void Show(SkillItem skillItem, UI_SkillDetail parent)
    {
        _skill = skillItem;
        _parent = parent;

        // 초월 정보 항목 초기화
        _infoPrefab.gameObject.SetActive(false);
        _infoElements.ForEach(x => Destroy(x.gameObject));
        _infoElements.Clear();
        //List<FSkillTranscendence> transcDatas = _skill.SkillTranscendence.TranscDatas;
        //for (int i = 0; i < transcDatas.Count - 1; i++)
        //{
        //    int star = i + 1;
        //    TranscInfoElement element = Instantiate(_infoPrefab, _infoParent);
        //    bool active = skillItem.SkillTranscendence.Star >= star;
        //    FSkillTranscendence nextTran = skillItem.SkillTranscendence.GetTranscData(star + 1);
        //    FSkillLinkTranscendence linkSkill = skillItem.SkillTranscendence.GetLinkTranscData(star);
        //    string descText = _parent.GetTranscDescText(nextTran, linkSkill);
        //    element.Setting(active, star, descText);
        //    element.gameObject.SetActive(true);
        //    _infoElements.Add(element);
        //}
    }
}
