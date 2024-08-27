using System;
using System.Collections;
using UnityEngine;

namespace BoraBattle.Game.BingoMasterKing
{
    public class SafeArea_Scale : MonoBehaviour
    {
        void Start()
        {
            // 탑노치만큼 아래로 작게 스케일
            Vector2 anchorMax = Screen.safeArea.position + Screen.safeArea.size;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            transform.localScale = Vector3.one * anchorMax.y;
        }
    }
}

