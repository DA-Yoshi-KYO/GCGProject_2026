/*
+=====================================
 ファイル名 : CS_MagicCircleBlackCutReveal.cs
 概要     : 魔法陣全体のBlackCutを一括制御して出現演出を行う
 作者     : ヨシモト リョウ
 履歴     : 2026/06/01 新規作成
=====================================+
*/

using System.Collections;
using UnityEngine;

/// <summary>
/// 魔法陣全体のRendererに対して、_BlackCutを一括で制御するクラス。
/// </summary>
public class CS_MagicCircleBlackCutReveal : MonoBehaviour
{
    private static readonly int BlackCutId = Shader.PropertyToID("_BlackCut");

    [SerializeField]
    private Renderer[] targetRenderers;

    [SerializeField]
    private float revealSeconds = 0.45f;

    [SerializeField]
    private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private bool playOnEnable = true;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine revealCoroutine;

    /// <summary>
    /// 初期化処理を行う。
    /// </summary>
    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        CollectRenderersIfNeeded();
        SetBlackCut(1f);
    }

    /// <summary>
    /// 有効化時に出現演出を再生する。
    /// </summary>
    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlayReveal();
        }
    }

    /// <summary>
    /// 魔法陣の出現演出を再生する。
    /// </summary>
    [CS_EffectPlay]
    [ContextMenu("Play Reveal")]
    public void PlayReveal()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        revealCoroutine = StartCoroutine(PlayRevealCoroutine());
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
    /// _BlackCutを1から0へ変化させる。
    /// </summary>
    private IEnumerator PlayRevealCoroutine()
    {
        float elapsedTime = 0f;

        SetBlackCut(1f);

        while (elapsedTime < revealSeconds)
        {
            elapsedTime += Time.deltaTime;

            float rate = Mathf.Clamp01(elapsedTime / revealSeconds);
            float curveRate = revealCurve.Evaluate(rate);

            float blackCut = Mathf.Lerp(1f, 0f, curveRate);
            SetBlackCut(blackCut);

            yield return null;
        }

        SetBlackCut(0f);
        revealCoroutine = null;
    }

    /// <summary>
    /// 全Rendererの全Materialスロットに_ BlackCut値を設定する。
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
