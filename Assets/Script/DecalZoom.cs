using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalZoom : MonoBehaviour
{
    // �����ٶ�
    public float speed = 1.0f;
    // ���ŷ���
    public float magnitude = 0.5f;
    // ��ʼ���ű���
    private Vector3 originalScale;

    void Start()
    {
        // ��¼����ĳ�ʼ���ű���
        originalScale = transform.localScale;
    }

    void Update()
    {
        // ʹ�����Ҳ�����ƽ�������ű仯
        float scaleFactor = Mathf.Sin(Time.time * speed) * magnitude;

        // ������������ű���
        transform.localScale = originalScale + new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }
}
