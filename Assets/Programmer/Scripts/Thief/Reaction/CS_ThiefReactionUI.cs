/* ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    泥棒のリアクションUIを管理するクラス
 * ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
 *    宇留野 陸斗
 * ----------------------------------------------------------
 * 2026-04-30 | 初回作成
 * 2026-05-22 | ファイル名を変更（CS_ThiefReaction.cs → CS_ThiefReaction.cs）
 *            | クラス名を変更（CS_ThiefReaction → CS_ThiefReaction）
 *            | クラス名を変更（CS_ThiefReaction → CS_ThiefReactionUI）
 * 
 */
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 泥棒のリアクションを管理するクラス
/// </summary>
public class CS_ThiefReactionUI : MonoBehaviour
{
    [SerializeField,Header("リアクションに使用するスプライトリスト"), Tooltip("リアクションに使用するスプライトリスト")]
    private List<Sprite> reactionSprites = new List<Sprite>();

    struct ReactionObject
    {
        [Tooltip("リアクションのUIオブジェクト")]
        public GameObject reactionUI;

        [Tooltip("表示するまでの時間")]
        public float timeToDisplay;

        [Tooltip("残存時間")]
        public float remainingTime;

        [Tooltip("振動時間")]
        public float vibrationTime;

        [Tooltip("スライドイン時間")]
        public float slideInTime;

        [Tooltip("フェード時間")]
        public float fadeOutTime;

        [Tooltip("初期位置")]
        public Vector3 initialPosition;

        [Tooltip("ターゲットポイント")]
        public Vector3 targetPosition;

        /// <summary>
        /// リアクションオブジェクトのコンストラクタ
        /// </summary>
        /// <param name="ui"> リアクションのUIオブジェクト</param>
        /// <param name="time"> リアクションのUIを表示するまでの時間</param>
        /// <param name="slideIn"> リアクションのUIをスライドインさせる時間</param>
        public ReactionObject(GameObject ui, float remainingTime, float time, float slideIn, float fadeTime)
        {
            reactionUI = ui;
            this.remainingTime = remainingTime;
            timeToDisplay = time;
            slideInTime = slideIn;
            vibrationTime = 0.0f;
            fadeOutTime = fadeTime;
            initialPosition = Vector3.zero;
            targetPosition = Vector3.zero;
        }

        public void SetVibrationTime(float time)
        {
            vibrationTime = time;
        }
    }

    [Tooltip("作成したリアクションオブジェクトを格納するリスト")]// リアクションオブジェクトと表示タイマーを格納する辞書
    private List<ReactionObject> reactionObjects = new List<ReactionObject>();

    [Tooltip("リアクションのUIの親オブジェクト")]
    private GameObject parentThiefReaction;

    [SerializeField,Header("残存時間"), Tooltip("リアクションを表示する時間")]
    private float reactionDisplayTime = 3.0f;

    [SerializeField, Header("表示するまでの時間"), Tooltip("リアクションUIの間隔")]
    private float reactionUIInterval = 0.3f;

    [SerializeField, Header("振動時間"),Tooltip("リアクションUIの振動時間")]
    private float reactionUIVibrationTime = 0.4f;

    [SerializeField, Header("振動の強さ"), Tooltip("リアクションUIの振動の強さ")]
    private float vibrationStrength = 10.0f;

    [SerializeField, Header("スライドインの時間"), Tooltip("リアクションUIのスライドイン時間")]
    private float reactionUISlideInTime = 0.5f;

    [SerializeField, Header("フェード時間"), Tooltip("リアクションUIのフェード時間")]
    private float fadeOutTime = 0.5f;

    [SerializeField, Header("振動の周波数"), Tooltip("リアクションUIの振動の周波数")]
    private float vibrationFrequency = 50.0f;

    private Vector3? cameraInitialPosition = null;


    /// <summary>
    /// 初期化処理(リアクションのUIの親オブジェクトをCanvas内に作成する処理)
    /// </summary>
    private void Start()
    {
        // ThiefReactionの親オブジェクトを取得
        Transform parent = this.transform;

        if (parent == null)
        {
            parentThiefReaction = new GameObject("ParentThiefReaction");
        }
        else parentThiefReaction = parent.gameObject;

        parentThiefReaction.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-550, -340, 0);
    }

    /// <summary>
    /// 毎フレーム、リアクションオブジェクトの表示時間を減算し、0以下になったらオブジェクトを破棄する処理
    /// </summary>
    private void Update()
    {
        if (reactionObjects.Count == 0) return;

        bool isSkip = false;
        bool isVibrating = false;

        for (int i = 0; i < reactionObjects.Count; i++)
        {
            var key = reactionObjects[i];

            if (isSkip) continue;

            if (key.timeToDisplay >= 0.0f)
            {
                // 時間が残っている場合は、表示時間を減算
                key.timeToDisplay -= Time.deltaTime;
                reactionObjects[i] = key;
                key.reactionUI.transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0);
                isSkip = true;
                continue;
            }

            key.reactionUI.transform.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1);

            key.remainingTime -= Time.deltaTime;
            reactionObjects[i] = key;

            // 最初の0.5秒で下から上にスライドインアニメーション
            if (key.slideInTime > 0.0f)
            {
                float slideInProgress = 1.0f - (key.slideInTime / reactionUISlideInTime);
                float slideInY = Mathf.Lerp(key.initialPosition.y, key.targetPosition.y, slideInProgress);
                key.reactionUI.transform.localPosition = new Vector3(key.targetPosition.x, slideInY, key.targetPosition.z);

                // スライドインの時間を減算
                key.slideInTime -= Time.deltaTime;
                reactionObjects[i] = key;
            }
            // 振動処理
            else if (key.vibrationTime > 0.0f)
            {
                isVibrating = true;
                Transform camera = Camera.main.transform;

                if (cameraInitialPosition == null)
                {
                    cameraInitialPosition = camera.localPosition;
                }

                // 進行度 (1.0 -> 0.0)
                float progress = key.vibrationTime / reactionUIVibrationTime;
                // 減衰する振幅
                float amplitude = vibrationStrength * progress;
                // 振動オフセット
                float offsetX = Mathf.Sin(Time.time * vibrationFrequency) * amplitude * Random.Range(-1f, 1f);
                float offsetY = Mathf.Sin(Time.time * vibrationFrequency) * amplitude * Random.Range(-1f, 1f);
                
                Vector3 offset = new Vector3(offsetX, offsetY, 0);

                // カメラ位置を更新
                camera.localPosition = cameraInitialPosition.Value + offset;

                // 振動時間を減算
                key.vibrationTime -= Time.deltaTime;
                reactionObjects[i] = key;
            }
        }

        // どのオブジェクトも振動していない場合、カメラ位置をリセット
        if (!isVibrating && cameraInitialPosition != null)
        {
            Camera.main.transform.localPosition = cameraInitialPosition.Value;
            cameraInitialPosition = null;
        }

        for (int i = reactionObjects.Count - 1; i >= 0; i--)
        {
            var key = reactionObjects[i];
            // フェードアウト
            if (key.remainingTime <= 0.0f)
            {
                // フェードアウトの進行度 (0.0 -> 1.0)
                float fadeOutProgress = 1.0f - (key.fadeOutTime / fadeOutTime);

                // フェードアウトのアルファ値を計算
                float alpha = Mathf.Lerp(1.0f, 0.0f, fadeOutProgress);

                // フェードアウトのアルファ値をUIに反映
                key.reactionUI.transform.GetComponentInChildren<Image>().color = new Color(1, 1 , 1, alpha);

                key.fadeOutTime -= Time.deltaTime;
                reactionObjects[i] = key;

                // フェードアウトが完了したらオブジェクトを破棄
                if (key.fadeOutTime <= 0.0f)
                {
                    Destroy(key.reactionUI);
                    reactionObjects.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// 感情を登録する処理(使用するリアクションスプライトを設定する処理)
    /// </summary>
    /// <param name="reactionData">設定するスプライトリスト</param>
    public void RegisterReaction(List<Sprite> reactionData)
    {
        if (reactionSprites.Count() != 0){
            return;
        }
        reactionSprites = reactionData;
    }

    /// <summary>
    /// リアクションのUIを設定する処理(スプライトリストから指定されたリアクションのスプライトをUIに反映させる処理)
    /// </summary>
    /// <param name="type"></param>
    public void SetReactionUI()
    {
        if (reactionSprites == null || reactionSprites.Count == 0)
        {
            Debug.LogWarning("リアクションスプライトが設定されていません。");
            return;
        }

        // 既存のリアクションオブジェクトの表示タイマーをリセット
        for (int i = 0; i < reactionObjects.Count; i++)
        {
            var key = reactionObjects[i];
            key.remainingTime = reactionDisplayTime;
            reactionObjects[i] = key;
        }

        if (parentThiefReaction.transform.childCount < 4)
        {
            // リアクションオブジェクトを作成
            GameObject reactionUI = new GameObject("ThiefReaction");
            reactionUI.transform.SetParent(parentThiefReaction.transform);

            // 新しいものを一番上に表示
            reactionUI.transform.SetAsLastSibling();

            // UIにImageコンポーネントを追加してスプライトを設定
            GameObject reactionUIObject = new GameObject("ReactionImage");
            reactionUIObject.transform.SetParent(reactionUI.transform);
            reactionUIObject.transform.localPosition = Vector3.zero;

            Image imageUI = reactionUIObject.AddComponent<Image>();

            imageUI.sprite = reactionSprites[parentThiefReaction.transform.childCount - 1];
            // リアクションオブジェクトの構造体を作成
            // リアクションオブジェクトと表示タイマーするまでの時間
            ReactionObject reactionObject = new ReactionObject(
                reactionUI,             // リアクションのUIオブジェクト
                reactionDisplayTime,    // 表示時間
                reactionUIInterval,     // 表示するまでの時間
                reactionUISlideInTime,  // スライドインさせる時間
                fadeOutTime             // フェードアウトさせる時間
                );

            // 大きさを調整
            imageUI.SetNativeSize();

            // 位置を調整
            int myChildIndex = reactionUI.transform.GetSiblingIndex();
            if (myChildIndex > 0)
            {
                Vector3 PreviousChildPoint = reactionObjects[reactionObjects.Count() - 1].targetPosition; // 直前のリアクションオブジェクトの位置を取得

                RectTransform rectTransform = imageUI.GetComponent<RectTransform>();
                reactionObject.targetPosition = new Vector3(
                    PreviousChildPoint.x - (rectTransform.sizeDelta.x * 0.1f),
                    PreviousChildPoint.y + (rectTransform.sizeDelta.y * 0.2f),
                    0.0f
                    );
            }

            reactionObject.initialPosition = reactionUI.transform.localPosition = new Vector3(
                reactionObject.targetPosition.x,
                reactionObject.targetPosition.y - 30.0f,
                reactionObject.targetPosition.z
                ); // 初期位置を設定

            // リアクションオブジェクトと表示タイマーを辞書に追加
            reactionObjects.Add(reactionObject);
        }
        else
        {
            // 一番上のリアクションオブジェクトの振動時間を設定
            var lastKey = reactionObjects.Last();
            lastKey.SetVibrationTime(reactionUIVibrationTime);
            reactionObjects[reactionObjects.Count - 1] = lastKey;
        }
    }

}
