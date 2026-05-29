/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒の聴覚に関するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-05-27 | 初回作成
 * 
 */
using UnityEngine;

/// <summary>
/// 泥棒の聴覚に関するシステムを管理するクラス。
/// </summary>
public class CS_HearingSystem
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [Tooltip("泥棒が音に反応しているかどうかを判定するフラグ")]
    private bool isReactingToSound = false;
    public bool read_IsReactingToSound => isReactingToSound;

    [Tooltip("泥棒が反応している音の位置")]
    private Vector3 soundReactionPosition;
    public Vector3 read_SoundReactionPosition => soundReactionPosition;

    [Tooltip("現在の音を追跡し始めてからの経過時間")]
    private float soundReactionElapsedTime = 0.0f;
    [Tooltip("音を追跡する最大時間")]
    private const int maxSoundReactionTime = 5;

    [Tooltip("音の種類を定義する列挙型")]
    public enum AttractSoundType
    {
        [Tooltip("猫の鳴き声")]
        CatVoice,
        [Tooltip("ギミックの起動音")]
        GimmickActivate,
    }
    [Tooltip("泥棒が反応している音のタイプ")]
    private AttractSoundType soundReactionType;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="thiefAI">ThiefAIスクリプトへの参照</param>
    public CS_HearingSystem(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
    }

    /// <summary>
    /// 音のする方向に向かう処理
    /// </summary>
    /// <param name="soundPosition">音の鳴った座標</param>
    public void InvestigateSound(Vector3 soundPosition, AttractSoundType type)
    {
        if (isReactingToSound)// すでに音に反応している場合
        {
            if (type == AttractSoundType.CatVoice)
            {
                // すでに猫の鳴き声に反応している場合は、さらに近づくために音のする方向に向かう
                soundReactionPosition = soundPosition;
                soundReactionType = type;
                thiefAI.read_MoveSystem.MoveTo(soundReactionPosition);
            }
            else if (type == AttractSoundType.GimmickActivate)
            {
                switch (soundReactionType)
                {
                    case AttractSoundType.GimmickActivate:// すでにギミックの起動音に反応している場合は、より近いものを優先して音のする方向に向かう
                        {
                            // 音のする方向と現在の位置の距離を計算
                            float currentDistance = Vector3.Distance(thiefAI.transform.position, soundReactionPosition);
                            float newDistance = Vector3.Distance(thiefAI.transform.position, soundPosition);
                            // より近い方を優先して音のする方向に向かう
                            if (newDistance < currentDistance)
                            {
                                soundReactionPosition = soundPosition;
                                soundReactionType = type;
                                thiefAI.read_MoveSystem.MoveTo(soundReactionPosition);
                            }
                        }
                        break;
                    case AttractSoundType.CatVoice:  // 猫の鳴き声に反応している場合は、何も変更しない
                    default:
                        break;
                }
            }
        }
        else // 音に反応していない場合
        {
            // 現在の状態が探索状態ではない場合は何もしない
            if (thiefAI.CurrentState != CS_ThiefAI.ThiefState.Explore) return;

            // 音のする方向に向かう
            soundReactionPosition = soundPosition;
            soundReactionType = type;
            soundReactionElapsedTime = 0.0f;
            thiefAI.read_MoveSystem.MoveTo(soundReactionPosition);
        }

        // 音に反応している状態に切り替える
        isReactingToSound = true;
    }

    /// <summary>
    /// 音の位置に到達したかどうかを判定する処理
    /// </summary>
    /// <param name="exploredDistanceThreshold">到達とみなす距離の閾値</param>
    /// <returns>true: 音の位置に到達した, false: 音の位置に到達していない</returns>
    public bool IsAtSoundReactionPosition(float exploredDistanceThreshold)
    {
        // 音の位置に到達したかどうかを判定する
        float distanceToSound = Vector3.Distance(thiefAI.transform.position, soundReactionPosition);

        // 音の位置に到達したとみなす距離の閾値と比較して、到達しているかどうかを判定する
        bool isAtPosition = distanceToSound < exploredDistanceThreshold;

        // 現在の音を追跡し始めてからの経過時間を更新する
        soundReactionElapsedTime += Time.deltaTime;

        // 経過時間が最大時間に達しているかどうかを判定する
        if (soundReactionElapsedTime >= maxSoundReactionTime) isAtPosition = true;

        // 到達していない場合：音に反応している状態を維持する
        // 到達している場合：音に反応している状態を解除する
        isReactingToSound = !isAtPosition;

        // 音の位置に到達しているかどうかを返す
        return isAtPosition;
    }
}
