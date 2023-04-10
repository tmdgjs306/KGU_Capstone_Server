using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static Transform targetTransform;
    public Vector3 cameraOffset;
    // 타겟의 시점 고정
    void Update()
    {
        transform.position = targetTransform.position + cameraOffset;
    }
}
