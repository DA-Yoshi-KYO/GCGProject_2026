/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    イージング関数
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-05-11 | 初回作成
 */
using Unity.VisualScripting;
using UnityEngine;

public class Easing : MonoBehaviour
{
    public enum EaseKind
    {
        Linear,
        EaseInSine,
        EaseInOutSine,
        EaseInOutCubic,
        EaseInOutQuintic,
        EaseInBack,
        EaseOutBack,
        EaseInCubic,
        EaseOutBounce
    }

    public static float Ease(EaseKind kind, float time, float timeMax = 1.0f)
    {
        float t = time / timeMax;

        switch (kind)
        {
            case EaseKind.Linear: return Linear(t);
            case EaseKind.EaseInSine: return EaseInSine(t);
            case EaseKind.EaseInOutSine: return EaseInOutSine(t);
            case EaseKind.EaseInOutCubic: return EaseInOutCubic(t);
            case EaseKind.EaseInOutQuintic: return EaseInOutQuintic(t);
            case EaseKind.EaseInBack: return EaseInBack(t);
            case EaseKind.EaseOutBack: return EaseOutBack(t);
            case EaseKind.EaseInCubic: return EaseInCubic(t);
            case EaseKind.EaseOutBounce: return EaseOutBounce(t);
            default: return 0.0f;
        }
    }

    public static float Linear(float time, float timeMax = 1.0f)
    {
        if (timeMax <= 0.0f)
            return 0.0f;
        float t = time / timeMax;
        if (t >= 1.0f)
            t = 1.0f;

        return t;
    }

    ///<summary>
    ///緩やかに変化するイーズインを行います
    ///参考：https://easings.net/ja#easeInSine
    ///</summary>
    ///<param name="time">timeDeltaTimeの値</param>
    ///<param name="timeMax">イーズインにかける時間(秒)</param>
    ///<returns>イージングの進行度(0.0f ~ 1.0f)</returns>
    public static float EaseInSine(float time, float timeMax = 1.0f)
    {
        if (timeMax <= 0.0f)
            return 0.0f;
        float t = time / timeMax;
        if (t >= 1.0f)
            t = 1.0f;

        return 1.0f - Mathf.Cos(t * Mathf.PI / 2.0f);
    }

    ///<summary>
    ///緩やかに変化するイーズインアウトを行います
    ///参考：https://easings.net/ja#easeInOutSine
    ///</summary>
    ///<param name="time">timeDeltaTimeの値</param>
    ///<param name="timeMax">イーズインにかける時間(秒)</param>
    ///<returns>イージングの進行度(0.0f ~ 1.0f)</returns>
    public static float EaseInOutSine(float time, float timeMax = 1.0f)
    {
        if (timeMax <= 0.0f)
            return 0.0f;
        float t = time / timeMax;
        if (t >= 1.0f)
            t = 1.0f;

        return -(Mathf.Cos(Mathf.PI * t) - 1.0f) / 2.0f;
    }

    ///<summary>
    ///急激に変化するイーズインアウトを行います
    ///参考：https://easings.net/ja#easeInOutSine
    ///</summary>
    ///<param name="time">timeDeltaTimeの値</param>
    ///<param name="timeMax">イーズインにかける時間(秒)</param>
    ///<returns>イージングの進行度(0.0f ~ 1.0f)</returns>
    public static float EaseInOutCubic(float time, float timeMax = 1.0f)
    {
        if (timeMax <= 0.0f)
            return 0.0f;
        float t = time / timeMax;
        if (t >= 1.0f)
            t = 1.0f;

        if (t < 0.5f)
        {
            return 4.0f * t * t * t;
        }
        else
        {
            return 1.0f - Mathf.Pow(-2.0f * t + 2.0f, 3.0f) / 2.0f;
        }
    }

    public static float EaseInOutQuintic(float t)
    {
        if (t < 0.5f)
        {
            //イージング関数の前半
            return 16.0f * Mathf.Pow(t, 5.0f);
        }
        else
        {
            //イージング関数の後半
            return 1.0f - Mathf.Pow(-2.0f * t + 2.0f, 5.0f) / 2.0f;
        }
    }

    public static float EaseInBack(float t, float s = 1.70158f)
    {
        return t * t * ((s + 1) * t - s);
    }

    public static float EaseOutBack(float t, float s = 1.70158f)
    {
        t = t - 1;
        return (t * t * ((s + 1) * t + s) + 1);
    }

    public static float EaseInCubic(float t)
    {
        return Mathf.Pow(t, 3.0f);
    }

    public static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1) 
        {
            return n1 * t * t;
        } 
        else if (t < 2 / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        else
        {
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
    }
}
