/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のリアクションを管理するクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-30 | 初回作成
 * 
 */
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// 泥棒のリアクションを管理するクラス
public class ThiefReaction : MonoBehaviour
{
    [Tooltip("リアクションに使用するスプライトリスト")]
    private List<Sprite> reactionSprites = new List<Sprite>();

    [Tooltip("作成したリアクションオブジェクトを格納するリスト")]// リアクションオブジェクトと表示タイマーを格納する辞書
    private Dictionary<GameObject, float> reactionObjects = new Dictionary<GameObject, float>();

    [Tooltip("リアクションのUIの親オブジェクト")]
    private GameObject parentThiefReaction;

    [Tooltip("リアクションの種類")]
    public enum ThiefReactionType
    {
        Pot,
        IronBall,
    }

    [Tooltip("リアクションを表示する時間")]
    private const float reactionDisplayTime = 2.0f;


    private void Start()
    {
        // Canvasを取得
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        // 見つからなかった場合作成
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // ThiefReactionの親オブジェクトを取得
        Transform parent = canvas.transform.Find("ParentThiefReaction");

        if (parent == null)
        {
            parentThiefReaction = new GameObject("ParentThiefReaction");
            parentThiefReaction.transform.SetParent(canvas.transform);
            parentThiefReaction.transform.AddComponent<RectTransform>();
            parentThiefReaction.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-530, 250, 0);
        }
        else parentThiefReaction = parent.gameObject;
    }

    private void Update()
    {
        if (reactionObjects.Count == 0) return;

        var keys = reactionObjects.Keys.ToList(); // スナップショット
        foreach (var key in keys)
        {
            if (!reactionObjects.ContainsKey(key)) continue; // 途中で消された保険

            reactionObjects[key] -= Time.deltaTime;
            if (reactionObjects[key] <= 0f)
            {
                reactionObjects.Remove(key);
                Destroy(key);
            }
        }
    }

    /// <summary>
    /// 感情を登録する処理(使用するリアクションスプライトを設定する処理)
    /// </summary>
    /// <param name="reactionData">設定するスプライトリスト</param>
    public void RegisterReaction(List<Sprite> reactionData)
    {
        reactionSprites = reactionData;
    }

    /// <summary>
    /// リアクションのUIを設定する処理(スプライトリストから指定されたリアクションのスプライトをUIに反映させる処理)
    /// </summary>
    /// <param name="type"></param>
    public void SetReactionUI(ThiefReactionType type)
    {
        if (reactionSprites == null || reactionSprites.Count == 0)
        {
            Debug.LogWarning("リアクションスプライトが設定されていません。");
            return;
        }

        // リアクションオブジェクトを作成
        GameObject reactionUI = new GameObject("ThiefReaction");
        reactionUI.transform.SetParent(parentThiefReaction.transform);
        // 子オブジェクトの一番上に配置
        reactionUI.transform.localPosition = Vector3.zero;
        reactionUI.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);

        // UIにImageコンポーネントを追加してスプライトを設定
        Image imageUI = reactionUI.AddComponent<Image>();
        imageUI.sprite = reactionSprites[(int)type];

        // リアクションオブジェクトと表示タイマーを辞書に追加
        reactionObjects.Add(reactionUI, reactionDisplayTime);
    }

}
