using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLineController : MonoBehaviour
{
    public float radius = 1.0f; // Բ�İ뾶
    public int segments = 36;   // Բ�ķֶ�����Խ��Խƽ����
    public Vector3 center = Vector3.zero; // Բ��λ��

    private void OnDrawGizmos()
    {
        DrawCircleRange(center, radius, segments);
    }

    void DrawCircleRange(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments; // ÿһ�εĽǶ�
        for (int i = 0; i < segments; i++)
        {
            // ���㵱ǰ�߶ε������յ�
            float angle1 = i * angleStep * Mathf.Deg2Rad; // ���Ƕ�
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad; // �յ�Ƕ�

            Vector3 start = new Vector3(
                center.x + Mathf.Cos(angle1) * radius,
                center.y ,
                center.z + Mathf.Sin(angle1) * radius);

            Vector3 end = new Vector3(
                center.x + Mathf.Cos(angle2) * radius,
                center.y ,
                center.z + Mathf.Sin(angle2) * radius);

            // �����߶�
            Debug.DrawLine(start, end, Color.green);
        }
    }
}
