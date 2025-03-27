using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalZoom : MonoBehaviour
{
    // 缩放速度
    public float speed = 1.0f;
    // 缩放幅度
    public float magnitude = 0.5f;
    // 初始缩放比例
    private Vector3 originalScale;

    void Start()
    {
        // 记录物体的初始缩放比例
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 使用正弦波生成平滑的缩放变化
        float scaleFactor = Mathf.Sin(Time.time * speed) * magnitude;

        // 更新物体的缩放比例
        transform.localScale = originalScale + new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }
}
