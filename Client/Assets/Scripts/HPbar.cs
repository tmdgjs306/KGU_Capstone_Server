using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPbar : MonoBehaviour
{
    public Vector3 offset;
    public Transform targetTr;
    
    private Canvas canvas;

    private Camera hpCamera;

    private RectTransform rectParent;

    private RectTransform rectHp;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        hpCamera = canvas.worldCamera;
        rectParent = canvas.GetComponent<RectTransform>();
        rectHp = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        var screenPos = Camera.main.WorldToScreenPoint(targetTr.position + offset); // 몬스터의 월드 3d좌표를 스크린좌표로 변환
       
        if (screenPos.z < 0.0f)
        {
            screenPos *= -1.0f;
        }

        var localPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectParent, screenPos, hpCamera, out localPos); // 스크린 좌표를 다시 체력바 UI 캔버스 좌표로 변환

        rectHp.localPosition = localPos; // 체력바 위치조정
    }
}
