using UnityEngine;

[System.Serializable]
public struct EaseParam
{
    public EasingUtils.EaseType type;
    public AnimationCurve curve;

    public float Evaluate(float t) => EasingUtils.GetValue(type, t, curve);
    public float Lerp(float a, float b, float t) => Mathf.LerpUnclamped(a, b, Evaluate(t));
    public Vector3 Lerp(Vector3 a, Vector3 b, float t) => Vector3.LerpUnclamped(a, b, Evaluate(t));
}

public static class EasingUtils
{
    public enum EaseType
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InSine, OutSine, InOutSine,
        InExpo, OutExpo,
        InBack, OutBack, InOutBack,
        OutElastic,
        CustomCurve
    }

    public static float GetValue(EaseType type, float t, AnimationCurve curve = null)
    {
        t = Mathf.Clamp01(t);

        if (type == EaseType.CustomCurve && curve != null && curve.length > 0)
            return curve.Evaluate(t);

        switch (type)
        {
            case EaseType.InQuad: return t * t;
            case EaseType.OutQuad: return t * (2 - t);
            case EaseType.InOutQuad: return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) * 0.5f;
            case EaseType.InCubic: return t * t * t;
            case EaseType.OutCubic: { float f = t - 1; return f * f * f + 1; }
            case EaseType.InOutCubic: return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) * 0.5f;
            case EaseType.InSine: return 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
            case EaseType.OutSine: return Mathf.Sin(t * Mathf.PI * 0.5f);
            case EaseType.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1) * 0.5f;
            case EaseType.InExpo: return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
            case EaseType.OutExpo: return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
            case EaseType.InBack:
                const float s1 = 1.70158f;
                return t * t * ((s1 + 1) * t - s1);
            case EaseType.OutBack:
                const float s2 = 1.70158f;
                float f2 = t - 1;
                return f2 * f2 * ((s2 + 1) * f2 + s2) + 1;
            case EaseType.InOutBack:
                const float s3 = 1.70158f * 1.525f;
                float t2 = t * 2;
                if (t2 < 1) return 0.5f * (t2 * t2 * ((s3 + 1) * t2 - s3));
                t2 -= 2;
                return 0.5f * (t2 * t2 * ((s3 + 1) * t2 + s3) + 2);
            case EaseType.OutElastic:
                if (t == 0) return 0;
                if (t == 1) return 1;
                return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * (2 * Mathf.PI / 3f)) + 1;
            default: return t;
        }
    }
}

public static class EasingExtensions
{
    public static float ToEase(this float t, EasingUtils.EaseType type, AnimationCurve curve = null)
        => EasingUtils.GetValue(type, t, curve);
}