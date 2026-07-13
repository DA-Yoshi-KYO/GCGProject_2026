/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のアニメーションシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-07-13 | 初回作成
 * 
 */
using UnityEngine;

/// <summary>
/// 泥棒のアニメーションシステムを管理するクラス。
/// </summary>
public class CS_ThiefAnimation
{
    [Tooltip("ThiefAIスクリプトへの参照")]
    private CS_ThiefAI thiefAI;

    [Tooltip("Animatorコンポーネントへの参照")]
    private Animator animator;
    public Animator read_Animator => animator;

    public enum ThiefAnimationState
    {
        Walk,
        Run,
        Damage,
        Hunting,
        Stunned,
        RunAway,
    }

    public CS_ThiefAnimation(CS_ThiefAI thiefAI, Animator animator)
    {
        this.thiefAI = thiefAI;
        this.animator = animator;
    }

    public void SetAnimationState(ThiefAnimationState state)
    {
        switch (state)
        {
            case ThiefAnimationState.Walk:
                animator.speed = 1.5f;
                break;
            case ThiefAnimationState.Run:
                animator.speed = 1.5f * (thiefAI.read_MoveSystem.read_RunSpeed / thiefAI.read_MoveSystem.read_WalkSpeed);
                break;
            case ThiefAnimationState.Damage:
                animator.SetBool("IsDamage", true);
                animator.speed = 1.0f;
                break;
            case ThiefAnimationState.Hunting:
                animator.SetBool("IsHunting", true);
                animator.speed = 1.0f;
                break;
            case ThiefAnimationState.Stunned:
                animator.SetBool("IsStun", true);
                animator.speed = 1.0f;
                break;
            case ThiefAnimationState.RunAway:
                animator.SetTrigger("DeathTrigger");
                animator.speed = 1.0f;
                break;
        }
    }

    public void ResetAnimationState()
    {
        animator.SetBool("IsStun", false);
        animator.SetBool("IsDamage", false);
        animator.SetBool("IsHunting", false);
        animator.speed = 1.0f;

        SetAnimationState(ThiefAnimationState.Walk);
    }
}
