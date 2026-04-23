/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    グレースケール用のボリューム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    吉田 京志郎
 * ----------------------------------------------------------
 * 2026-04-22 | 初回作成
 * 
 */

using UnityEngine.Rendering;

[System.Serializable, VolumeComponentMenu("CustomPostEffect/GrayScale")]
public class GrayScaleVolume : VolumeComponent
{
    public BoolParameter enable = new BoolParameter(false);
    public ClampedFloatParameter intensity = new ClampedFloatParameter(1.0f, 0.0f, 1.0f);
}
