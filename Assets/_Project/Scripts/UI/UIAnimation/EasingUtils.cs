using UnityEngine;

[System.Serializable]
public struct EaseParam
{
    public EasingUtils.EaseType type;
    public AnimationCurve curve;

    public float Evaluate(float t) => EasingUtils.GetValue(type, t, curve);
    public float Lerp(float a, float b, float t) => Mathf.LerpUnclamped(a, b, Evaluate(t));
    public Vector2 Lerp(Vector2 a, Vector2 b, float t) => Vector2.LerpUnclamped(a, b, Evaluate(t));
    public Vector3 Lerp(Vector3 a, Vector3 b, float t) => Vector3.LerpUnclamped(a, b, Evaluate(t));
}

public static class EasingUtils
{
    public enum EaseType
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InSine, OutSine, InOutSine,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic, InOutElastic,
        InBounce, OutBounce, InOutBounce,
        SmoothStep,
        SmootherStep,
        Step5,
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
            case EaseType.OutCubic: return 1 - Mathf.Pow(1 - t, 3);
            case EaseType.InOutCubic: return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) * 0.5f;

            case EaseType.InQuart: return t * t * t * t;
            case EaseType.OutQuart: return 1 - Mathf.Pow(1 - t, 4);
            case EaseType.InOutQuart: return t < 0.5f ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) * 0.5f;

            case EaseType.InQuint: return t * t * t * t * t;
            case EaseType.OutQuint: return 1 - Mathf.Pow(1 - t, 5);
            case EaseType.InOutQuint: return t < 0.5f ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) * 0.5f;

            case EaseType.InSine: return 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
            case EaseType.OutSine: return Mathf.Sin(t * Mathf.PI * 0.5f);
            case EaseType.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1) * 0.5f;

            case EaseType.InExpo: return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
            case EaseType.OutExpo: return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
            case EaseType.InOutExpo: return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? Mathf.Pow(2, 20 * t - 10) * 0.5f : (2 - Mathf.Pow(2, -20 * t + 10)) * 0.5f;

            case EaseType.InCirc: return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
            case EaseType.OutCirc: return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
            case EaseType.InOutCirc: return t < 0.5f ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) * 0.5f : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) * 0.5f;

            case EaseType.InBack: return 2.70158f * t * t * t - 1.70158f * t * t;
            case EaseType.OutBack: return 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
            case EaseType.InOutBack:
                const float s3 = 1.70158f * 1.525f;
                return t < 0.5f ? (Mathf.Pow(2 * t, 2) * ((s3 + 1) * 2 * t - s3)) * 0.5f : (Mathf.Pow(2 * t - 2, 2) * ((s3 + 1) * (t * 2 - 2) + s3) + 2) * 0.5f;

            case EaseType.InElastic: return t == 0 ? 0 : t == 1 ? 1 : -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * (2 * Mathf.PI / 3f));
            case EaseType.OutElastic: return t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * (2 * Mathf.PI / 3f)) + 1;
            case EaseType.InOutElastic:
                const float c5 = (2 * Mathf.PI) / 4.5f;
                return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * c5)) * 0.5f : (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * c5)) * 0.5f + 1;

            case EaseType.InBounce: return 1 - OutBounceInternal(1 - t);
            case EaseType.OutBounce: return OutBounceInternal(t);
            case EaseType.InOutBounce: return t < 0.5f ? (1 - OutBounceInternal(1 - 2 * t)) * 0.5f : (1 + OutBounceInternal(2 * t - 1)) * 0.5f;

            case EaseType.SmoothStep: return t * t * (3 - 2 * t);
            case EaseType.SmootherStep: return t * t * t * (t * (t * 6 - 15) + 10);
            case EaseType.Step5: return Mathf.Floor(t * 5) / 5f;

            default: return t;
        }
    }

    private static float OutBounceInternal(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        if (t < 1 / d1) return n1 * t * t;
        else if (t < 2 / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
        else if (t < 2.5 / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        else return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }
}

public static class EasingExtensions
{
    public static float ToEase(this float t, EasingUtils.EaseType type, AnimationCurve curve = null)
        => EasingUtils.GetValue(type, t, curve);
}