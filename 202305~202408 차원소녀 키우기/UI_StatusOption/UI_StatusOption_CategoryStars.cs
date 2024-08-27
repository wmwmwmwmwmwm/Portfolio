using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UI_StatusOption_CategoryStars : MonoBehaviour
{
    [Serializable]
    public class LineInfo
    {
        public GameObject Line;
        public UI_StatusOption_CategoryStars_Star Star1, Star2;
    }

    public List<UI_StatusOption_CategoryStars_Star> Stars;
    public List<GameObject> Lines;
    public List<LineInfo> LineInfos;
}
