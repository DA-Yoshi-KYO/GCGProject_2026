/*
+=====================================
 ファイル名 : CSE_EffectType.cs
 概要     : Effectの再生方式を定義するEnum
 作者     : ヨシモト リョウ
 履歴     : 2026/06/10 新規作成
=====================================+
*/

/// <summary>
/// Effectの再生方式を表す列挙型です。
/// </summary>
public enum CSE_EffectType
{
    None,

    /// <summary>
    /// C#スクリプトで制御するEffectです。
    /// </summary>
    CustomScript,

    /// <summary>
    /// UnityのParticleSystemを使用するEffectです。
    /// </summary>
    ParticleSystem,

    /// <summary>
    /// Vertex Animation Textureを使用するEffectです。
    /// </summary>
    VAT,

    /// <summary>
    /// スプライトシートを使用するEffectです。
    /// </summary>
    SpriteSheet,

    /// <summary>
    /// ShaderやMaterialだけで動くEffectです。
    /// </summary>
    ShaderOnly
}
