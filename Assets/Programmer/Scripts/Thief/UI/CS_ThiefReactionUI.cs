/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のリアクションを管理するクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-30 | 初回作成
 * 2026-05-22 | ファイル名を変更（CS_ThiefReaction.cs → CS_ThiefReaction.cs）
 *            | クラス名を変更（CS_ThiefReaction → CS_ThiefReaction）
 * 
 */
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 泥棒のリアクションを管理するクラス
/// </summary>
public class CS_ThiefReactionUI : MonoBehaviour
{
    [SerializeField,Header("リアクションに使用するスプライトリスト"), Tooltip("リアクションに使用するスプライトリスト")]
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

    /// <summary>
    /// 初期化処理(リアクションのUIの親オブジェクトをCanvas内に作成する処理)
    /// </summary>
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
            parentThiefReaction.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-850, 0, 0);
        }
        else parentThiefReaction = parent.gameObject;
    }

    /// <summary>
    /// 毎フレーム、リアクションオブジェクトの表示時間を減算し、0以下になったらオブジェクトを破棄する処理
    /// </summary>
    private void Update()
    {
        if (reactionObjects.Count == 0) return;

        var keys = reactionObjects.Keys.ToList(); // スナップショット
        foreach (var key in keys)
        {
            if (!reactionObjects.ContainsKey(key)) continue; // 途中で消された保険

            reactionObjects[key] -= Time.deltaTime;

            // 最初の0.5秒で左から右にスライドインアニメーション
            float slideTime = 0.5f;
            if (reactionObjects[key] > reactionDisplayTime - slideTime)
            {
                float slideProgress = (reactionDisplayTime - reactionObjects[key]) / slideTime;
                Vector3 startPos = key.transform.localPosition;
                key.transform.localPosition = new Vector3(Mathf.Lerp(startPos.x, 100.0f, slideProgress), startPos.y, startPos.z);
            }

            // 最後の0.5秒でその位置から上にスライドアウトアニメーション
            if (reactionObjects[key] < slideTime)
            {
                float slideProgress = (slideTime - reactionObjects[key]) / slideTime;
                Vector3 startPos = key.transform.localPosition;
                key.transform.localPosition = new Vector3(startPos.x, startPos.y + 1.0f, startPos.z);

                // フェードアウトアニメーション
                Image image = key.GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    color.a = Mathf.Lerp(1.0f, 0.0f, slideProgress);
                    image.color = color;
                }
            }

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
        reactionUI.transform.localScale = new Vector3(4.0f, 3.0f, 1.0f);
        reactionUI.transform.localPosition = new Vector3(-100.0f, 0.0f + (parentThiefReaction.transform.childCount * 50.0f), 0.0f);

        // UIにImageコンポーネントを追加してスプライトを設定
        Image imageUI = reactionUI.AddComponent<Image>();
        imageUI.sprite = reactionSprites[(int)type];

        // リアクションオブジェクトと表示タイマーを辞書に追加
        reactionObjects.Add(reactionUI, reactionDisplayTime);
    }

}
