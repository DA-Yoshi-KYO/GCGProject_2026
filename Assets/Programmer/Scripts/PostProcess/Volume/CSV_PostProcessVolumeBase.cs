/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    ポストプロセスのプロパティを記入するベースクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田京志郎
 * ----------------------------------------------------------
 * 2026-05-14 | 吉田京志郎
 * 
 */
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public abstract class CSV_PostProcessVolumeBase : VolumeComponent, IPostProcessComponent
{
    [Header("有効化")] public BoolParameter isEnabled = new BoolParameter(false);

    /// <summary>
    /// ポストプロセスのプロパティをマテリアルに適用
    /// </summary>
    /// <param name="material">material</param>
    public abstract void Apply(MaterialPropertyBlock materialBlock);

    /// <summary>
    /// ポストプロセスの有効/無効を返す
    /// </summary>
    /// <returns>true:有効 false:無効</returns>
    public bool IsActive() { return isEnabled.value; }
    /// <summary>
    /// タイルマップに対応しているかどうかを返す
    /// </summary>
    /// <returns>true:対応 false:非対応</returns>
    public bool IsTileCompatible() => false;
}
