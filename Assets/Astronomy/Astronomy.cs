using NaughtyAttributes;
using System;
using Unity.Mathematics;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[Serializable]
public struct SkyBody 
{
    public Color color;
    public string name;
    public int a;
    public int d;
    public bool hide;
}

public class Astronomy : MonoBehaviour
{
    [Header("Parameters")]
    [Range(-math.PIHALF, math.PIHALF)] public float phi;
    [Range(-math.PI, math.PI)] public float lambda;
    [Range(0f, math.PI2)] public float S;
    [Range(-math.PIHALF, math.PIHALF)] public float d;
    [Range(0f, math.PI2)] public float a;
    [Range(0, 365)] public float day;
    public bool writeParametersConsole = false;
    public bool simulation = false;
    public bool constS = true;
    public bool constDay = true;
    public float dayInSecond = 36f;

    public SkyBody[] bodies;

    /*[ReadOnly] public float h;
    [ReadOnly] public float t;
    [ReadOnly] public float A;
    [ReadOnly] public float Tm;*/
    //[Range(0f, math.PI2)] public float Azimut;
    //[Range(0f, math.PIHALF)] public float height;

    [Header("Constants")]
    public Color sphereColor = Color.white;
    public Color bodyColor = Color.white;
    public Color lineColor = Color.red;
    public Color horizontalColor = Color.green;
    public float positionLength = 1.5f;
    public Color springColor = Color.green;
    public Color sunColor = Color.yellow;
    public float sphereRadius = 1f;
    public float bodyRadius = 0.05f;

    public int circleResolution = 16;

    private float s = 0f;

    public void Update() 
    {
        if (simulation)
        {
            if (constS)
            {
                s += math.frac(dayInSecond * Time.deltaTime) * math.PI2;
                if (s >= math.PI2)
                {
                    int dday = (int)math.trunc(s / math.PI2);
                    day += dday;
                    s -= math.PI2 * dday;
                }
            }
            else if (constDay)
            {
                S += math.frac(dayInSecond * Time.deltaTime) * math.PI2;
                S = math.frac(S / math.PI2) * math.PI2;
            }
            else 
            {
                S += math.frac(dayInSecond * Time.deltaTime) * math.PI2;
                if (S >= math.PI2)
                {
                    int dday = (int)math.trunc(S / math.PI2);
                    day += dday;
                    S -= math.PI2 * dday;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 1. Правильно присваиваем высоту (h) и азимут (A)

        // 2. Переводим высоту над горизонтом в полярный угол от зенита (оси Y)

        Gizmos.color = sphereColor;
        Gizmos.DrawSphere(transform.position, sphereRadius);
        Gizmos.DrawCube(transform.position, new Vector3(sphereRadius * 3f, sphereRadius * 3f, 0f)); 

        Gizmos.color = bodyColor;
        Gizmos.DrawSphere(transform.position, bodyRadius);

        Gizmos.color = sunColor;
        DrawBody(23.45f * math.PI / 180f * math.sin(math.PI2 / 365 * (day - 81)), math.PI2 * day / 365, S, "sun");

        foreach (SkyBody body in bodies)
        {
            if (body.hide) { continue; }
            Gizmos.color = body.color;
            float d = body.d / 10000 + body.d % 10000 / 100 / 60f + body.d % 100 / 3600f;
            int absa = math.abs(body.a);
            float a = absa / 10000 * 15 + absa % 10000 / 100 * 0.25f + absa % 100 * 0.25F / 60f;
            DrawBody(d, a * math.sign(body.a), S, body.name);
        }

        // 1. Горизонт (всегда строго в плоскости XZ, без наклона!)
        Gizmos.color = horizontalColor;
        DrawCircle(transform.position, Quaternion.Euler(90f, 0f, 0f), sphereRadius, circleResolution);

        // Линия в зенит (строго вверх по оси Y)
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * positionLength);

        // Полупрозрачная плоскость горизонта (лежит строго горизонтально)
        Gizmos.color = horizontalColor * sphereColor;
        Gizmos.DrawCube(transform.position, new Vector3(sphereRadius * 3f, 0.01f, sphereRadius * 3f));

        // 2. Небесный экватор (наклонён на угол (90° - phi) относительно горизонта)
        Gizmos.color = lineColor;
        float tiltDeg = (math.PIHALF - phi) * Mathf.Rad2Deg;
        // Поворачиваем круг в плоскость XZ и наклоняем вокруг оси Z в сторону юга/севера
        Quaternion equatorRot = Quaternion.Euler(0f, 0f, -tiltDeg) * Quaternion.Euler(90f, 0f, 0f);
        DrawCircle(transform.position, equatorRot, sphereRadius, circleResolution);

        /*Gizmos.color = horizontalColor;
        // Горизонт
        DrawCircle(transform.position, quaternion.EulerXYZ(math.PIHALF, 0f, 0f), sphereRadius, circleResolution);
        Gizmos.DrawLine(transform.position, transform.position + positionLength * SphereToDecarte(phi, lambda));

        Vector3 normal = SphereToDecarte(phi, lambda);

        // Вычисляем поворот плоскости так, чтобы её ось Y совпала с нормалью
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);

        // Альтернативный способ через углы Эйлера (если нужен именно Vector3 угол):
        // Vector3 eulerAngles = rotation.eulerAngles;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, rotation, Vector3.one);

        // Рисуем плоскость
        Gizmos.color = horizontalColor * sphereColor;
        Gizmos.DrawCube(Vector3.zero, new Vector3(sphereRadius * 3f, 0.01f, sphereRadius * 3f));

        Gizmos.matrix = oldMatrix;

        Gizmos.color = lineColor;
        // Небесный экватор (наклонён в сторону +X)
        DrawCircle(transform.position, quaternion.EulerXYZ(math.PIHALF, 0f, (math.PIHALF - phi)), sphereRadius, circleResolution);*/
    }

    private Vector3 SphereToDecarte(float h, float A) 
    {
        float zenithAngle = math.PIHALF - h;

        Vector3 planetPos = sphereRadius * new Vector3(
            math.cos(A) * math.sin(zenithAngle),
            math.cos(zenithAngle),
            math.sin(A) * math.sin(zenithAngle)
        );

        return planetPos;
    }

    private void DrawBody(float d, float a, float S, string name = "null") 
    {
        float t = S + lambda - a;
        float h = D2H(d, t);
        float A = a2A(d, t);

        if (writeParametersConsole) 
        {
            Debug.Log($"{name} h:{h}, A:{A}");
        }

        // 2. Переводим высоту над горизонтом в полярный угол от зенита (оси Y)

        Gizmos.DrawSphere(transform.position + SphereToDecarte(h, A), bodyRadius);
    }

    private float D2H(float d, float t)
    {
        // Добавлен math.cos(d) и убрано лишнее math.PI - t
        float sinH = math.sin(phi) * math.sin(d) + math.cos(phi) * math.cos(d) * math.cos(t);
        return math.asin(math.clamp(sinH, -1f, 1f)); // clamp защищает от NaN из-за погрешности float
    }

    private float a2A(float d, float t)
    {
        // Используем atan2(y, x). Знак минус перед sin(t) или sin(A) задаёт направление вращения (по/против часовой)
        return math.atan2(
            math.sin(t),
            math.cos(t) * math.sin(phi) - math.tan(d) * math.cos(phi)
        );
    }


    private void DrawCircle(Vector3 center, Quaternion rotation, float r, int segs)
    {
        float angleStep = 2f * Mathf.PI / segs;

        // Вычисляем локальную стартовую точку и поворачиваем её
        Vector3 previousPoint = center + rotation * new Vector3(Mathf.Cos(0) * r, Mathf.Sin(0) * r, 0f);

        for (int i = 1; i <= segs; i++)
        {
            float angle = i * angleStep;

            // Локальная позиция следующей точки
            Vector3 localPos = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);

            // Умножаем Quaternion на Vector3, чтобы повернуть точку, и прибавляем к центру
            Vector3 nextPoint = center + rotation * localPos;

            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}
