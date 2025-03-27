using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLineController : MonoBehaviour
{
    public float radius = 1.0f; // 圆的半径
    public int segments = 36;   // 圆的分段数（越多越平滑）
    public Vector3 center = Vector3.zero; // 圆心位置

    private void OnDrawGizmos()
    {
        DrawCircleRange(center, radius, segments);
    }

    void DrawCircleRange(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments; // 每一段的角度
        for (int i = 0; i < segments; i++)
        {
            // 计算当前线段的起点和终点
            float angle1 = i * angleStep * Mathf.Deg2Rad; // 起点角度
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad; // 终点角度

            Vector3 start = new Vector3(
                center.x + Mathf.Cos(angle1) * radius,
                center.y ,
                center.z + Mathf.Sin(angle1) * radius);

            Vector3 end = new Vector3(
                center.x + Mathf.Cos(angle2) * radius,
                center.y ,
                center.z + Mathf.Sin(angle2) * radius);

            // 绘制线段
            Debug.DrawLine(start, end, Color.green);
        }
    }
}
