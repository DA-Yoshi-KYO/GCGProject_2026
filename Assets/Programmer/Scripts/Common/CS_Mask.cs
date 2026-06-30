/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    マスクシェーダの変数変更処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-19 | 初回作成
 */
using System;
using UnityEngine;
using UnityEngine.UI;

public class CS_Mask : MonoBehaviour
{
    [Header("マスク画像のIn")] public Image maskImageIn;
    [Header("マスク画像のOut")] public Image maskImageOut;
    [Header("時間の間隔")][SerializeField] public float durationTime = 1.0f;
    [Header("Startの大きさ")][SerializeField] public float startScale = 0.0f;
    [Header("Endの大きさ")][SerializeField] public float endScale = 0.0f;
    [Header("Startのアルファ値")][SerializeField] public float startAlpha= 0.0f;
    [Header("Endのアルファ値")][SerializeField] public float endAlpha = 0.0f;


    //処理用の変数
    private float startScaleValue;
    private float endScaleValue;
    private float startAlphaValue;
    private float endAlphaValue;
    [HideInInspector] public bool scaleIn = false;
    [HideInInspector] public bool scaleOut = false;

    float time = 0.0f;

    private Action betweenEvent;

    // Start is called before the first frame update
    void Start()
    {
        maskImageIn.material.SetFloat("_CurrentScaleFloat", 0.0f);
        maskImageIn.material.SetFloat("_AlphaScaleFloat", 1.0f);

        maskImageOut.material.SetFloat("_CurrentScaleFloat", 0.0f);
        maskImageOut.material.SetFloat("_AlphaScaleFloat", 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!scaleIn && !scaleOut)
            return;

        Time.timeScale = 0.0f;

        time += Time.unscaledDeltaTime;

        ScaleChage();
        AlphaChage();

        if (time >= durationTime)
        {
            time = 0.0f;

            if (scaleOut)
            {
                scaleOut = false;

                Time.timeScale = 1.0f;
            }

            if (scaleIn)
            {
                scaleIn = false;
                betweenEvent?.Invoke();
                StartOutMask();
            }
        }

    }

    public void StartInMask(Action between)
    {
        SetIn();
        scaleIn = true;
        betweenEvent = between;
    }

    public void StartOutMask()
    {
        SetOut();
        scaleOut = true;
    }

    //In用の値設定
    private void SetIn()
    {
        startScaleValue = startScale;
        endScaleValue = endScale;
        startAlphaValue = startAlpha;
        endAlphaValue = endAlpha;
    }

    //Out用の値設定（Inとは逆の値が入る）
    private void SetOut()
    {
        startScaleValue = endScale;
        endScaleValue = startScale;
        startAlphaValue = endAlpha;
        endAlphaValue = startAlpha;
    }

    //大きさ変化の処理
    private void ScaleChage()
    {
        float scale = Mathf.Lerp(startScaleValue, endScaleValue, time / durationTime);

        maskImageIn.material.SetFloat("_CurrentScaleFloat", scale);
        maskImageOut.material.SetFloat("_CurrentScaleFloat", scale);
    }

    //アルファ値変化の処理
    private void AlphaChage()
    {
        float alpha = Mathf.Lerp(startAlphaValue, endAlphaValue, time / durationTime);

        maskImageOut.material.SetFloat("_AlphaScaleFloat", alpha);
        maskImageIn.material.SetFloat("_AlphaScaleFloat", alpha);
    }
}
