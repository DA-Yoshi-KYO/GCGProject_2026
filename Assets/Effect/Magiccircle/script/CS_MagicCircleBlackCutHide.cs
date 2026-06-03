/*
+=====================================
 ファイル名 : CS_MagicCircleBlackCutHide.cs
 概要     : 魔法陣全体のBlackCutを一括制御して消失演出を行う
 作者     : ヨシモト リョウ
 履歴     : 2026/06/01 新規作成
=====================================+
*/

using System.Collections;
using UnityEngine;

/// <summary>
/// 魔法陣全体のRendererに対して、_BlackCutを一括で制御して消失演出を行うクラス。
/// </summary>
public class CS_MagicCircleBlackCutHide : MonoBehaviour, CSI_EffectDurationProvider
{
    private static readonly int BlackCutId = Shader.PropertyToID("_BlackCut");

    [SerializeField]
    private Renderer[] targetRenderers;

    [SerializeField]
    private float hideSeconds = 0.45f;

    [SerializeField]
    private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private bool setInactiveAfterHide = false;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine hideCoroutine;

    /// <summary>
    /// 再生時間を持っているか取得します。
    /// </summary>
    public bool HasPlayDuration => false;

    /// <summary>
    /// 再生演出の秒数を取得します。
    /// </summary>
    public float PlayDuration => 0.0f;

    /// <summary>
    /// 停止時間を持っているか取得します。
    /// </summary>
    public bool HasStopDuration => true;

    /// <summary>
    /// 消失演出の秒数を取得します。
    /// </summary>
    public float StopDuration => hideSeconds;

    /// <summary>
    /// 初期化処理を行う。
    /// </summary>
    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        CollectRenderersIfNeeded();
    }

    /// <summary>
    /// 魔法陣の消失演出を再生する。
    /// </summary>
    [CS_EffectStop]
    [ContextMenu("Play Hide")]
    public void PlayHide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(PlayHideCoroutine());
    }

    /// <summary>
    /// 子オブジェクトからRendererを自動取得する。
    /// </summary>
    private void CollectRenderersIfNeeded()
    {
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            return;
        }

        targetRenderers = GetComponentsInChildren<Renderer>(true);
    }

    /// <summary>
    /// _BlackCutを0から1へ変化させる。
    /// </summary>
    private IEnumerator PlayHideCoroutine()
    {
        float elapsedTime = 0f;

        SetBlackCut(0f);

        while (elapsedTime < hideSeconds)
        {
            elapsedTime += Time.deltaTime;

            float rate = Mathf.Clamp01(elapsedTime / hideSeconds);
            float curveRate = hideCurve.Evaluate(rate);

            float blackCut = Mathf.Lerp(0f, 1f, curveRate);
            SetBlackCut(blackCut);

            yield return null;
        }

        SetBlackCut(1f);

        hideCoroutine = null;

        if (setInactiveAfterHide)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 魔法陣を即座に表示状態にする。
    /// </summary>
    [ContextMenu("Set Visible")]
    public void SetVisible()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        SetBlackCut(0f);
    }

    /// <summary>
    /// 魔法陣を即座に非表示状態にする。
    /// </summary>
    [ContextMenu("Set Hidden")]
    public void SetHidden()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        SetBlackCut(1f);
    }

    /// <summary>
    /// 全Rendererの全Materialスロットに_BlackCut値を設定する。
    /// </summary>
    /// <param name="blackCut">設定するBlackCut値。</param>
    private void SetBlackCut(float blackCut)
    {
        for (int rendererIndex = 0 ; rendererIndex < targetRenderers.Length ; rendererIndex++)
        {
            Renderer targetRenderer = targetRenderers[rendererIndex];

            if (targetRenderer == null)
            {
                continue;
            }

            int materialCount = targetRenderer.sharedMaterials.Length;

            for (int materialIndex = 0 ; materialIndex < materialCount ; materialIndex++)
            {
                targetRenderer.GetPropertyBlock(propertyBlock, materialIndex);
                propertyBlock.SetFloat(BlackCutId, blackCut);
                targetRenderer.SetPropertyBlock(propertyBlock, materialIndex);
            }
        }
    }
}
