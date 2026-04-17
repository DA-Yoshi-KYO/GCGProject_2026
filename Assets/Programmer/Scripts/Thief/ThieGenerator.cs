/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒を生成するシステム
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-17 | 初回作成
 * 
 */
using UnityEditor;
using UnityEngine;

// 泥棒を生成するシステム
public class ThieGenerator
{
    [Tooltip("泥棒のデータベース")]
    private ThiefDataSO thiefDB;

    [Tooltip("使用するデータベースのパス")]
    private string thiefDBPath = "Assets/Programmer/ScriptableObject/ThiefDB.asset";

    private void Start()
    {
        // 泥棒のデータベースをロード
        thiefDB = AssetDatabase.LoadAssetAtPath<ThiefDataSO>(thiefDBPath);
    }

}
