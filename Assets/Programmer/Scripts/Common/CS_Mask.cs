/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    マスクシェーダの変数変更処理
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    元浪梨緒
 * ----------------------------------------------------------
 * 2026-06-19 | 初回作成
 */
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CS_Mask : MonoBehaviour
{
    [Header("マスク画像")] public Image maskImage;
    [Header("時間の間隔")][SerializeField] public float durationTime = 1.0f;
    [Header("Startの大きさ")][SerializeField] public float startScale = 0.0f;
    [Header("Endの大きさ")][SerializeField] public float endScale = 0.0f;
    [Header("Startのアルファ値")][SerializeField] public float startAlpha= 0.0f;
    [Header("Endのアルファ値")][SerializeField] public float endAlpha = 0.0f;


    //処理用の変数
    [HideInInspector] public Image mainImage;
    [HideInInspector] public Material mainMaterial;
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
        mainImage = maskImage;
        mainMaterial = mainImage.material;

        mainMaterial.SetFloat("_ScaleFloat", 0.0f);
        mainMaterial.SetFloat("_AlphaScaleFloat", 1.0f);
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
        mainMaterial.SetFloat("_ScaleFloat", scale);
    }

    //アルファ値変化の処理
    private void AlphaChage()
    {
        float alpha = Mathf.Lerp(startAlphaValue, endAlphaValue, time / durationTime);
        mainMaterial.SetFloat("_AlphaScaleFloat", alpha);
    }
}
