using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIcon : MonoBehaviour
{
    [Header("HPゲージのUI")]
    [SerializeField] private Image hp;
    private CS_ThiefAI thiefAI;

    void Start()
    {
        
    }
    void Update()
    {
        // HPゲージの更新
        int currentDurability = thiefAI.read_Durability;
        // 0.0f～1.0fの範囲でHPゲージの割合を計算
        int maxDurability = 0; // 今後実装
        float hpRatio = (float)currentDurability / (float)maxDurability;
        hp.fillAmount = hpRatio;
    }

    /// <summary>
    /// CS_ThiefAIのスクリプトを受け取る関数
    /// </summary>
    /// <param name="thiefAI">CS_ThiefAIのインスタンス</param>
    public void SetScript(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
    }

    /// <summary>
    ///  CS_ThiefAIのスクリプトを返す関数
    /// </summary>
    /// <returns></returns>
    public CS_ThiefAI GetScript()
    {
        return thiefAI;
    }
}
