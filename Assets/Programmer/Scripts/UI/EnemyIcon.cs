using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIcon : MonoBehaviour
{
    [Header("アイコンのImage")]
    [SerializeField] private Image iconImage;

    [Header("アイコンのサイズ")]
    [SerializeField] private Vector3 iconSize = new Vector3(1f, 1f, 1f);

    [SerializeField] private NumberView numberView;

    [Header("追加表示アニメーションの再生時間（秒）")]
    [SerializeField] private float appearAnimationDuration = 0.4f;

    [Header("追加表示アニメーション開始時の回転角度（Z軸）")]
    [SerializeField] private float appearStartAngle = 90f;


    private CS_ThiefAI thiefAI;
    private CS_RoomPlayerPosition roomPlayerPosition;
    private bool isPlayingAppearAnimation = false;
    private void Start()
    {
        GameObject roomManager = GameObject.Find("RoomManager");
        if (roomManager != null)
        {
            roomPlayerPosition = roomManager.GetComponent<CS_RoomPlayerPosition>();
        }
        numberView.SetTensView(false);

    }
    void Update()
    {
        if (thiefAI == null)
        {
            return;
        }

        if (roomPlayerPosition == null)
        {
            return;
        }

        int currentHP = thiefAI.read_Durability;
        numberView.SetNumber(currentHP);

        // 追加表示アニメーション中はスケール制御をアニメーション側に譲る
        if (isPlayingAppearAnimation) return;

        // 同じ部屋にいる場合サイズを大きくする
        if (thiefAI.read_MemorySystem != null && roomPlayerPosition.PlayerRoomData != null &&
            thiefAI.read_MemorySystem.read_CurrentRoomPoint == roomPlayerPosition.PlayerRoomData.transform)
        {



            transform.localScale = Vector3.Lerp(transform.localScale, iconSize * 1.2f, Time.deltaTime * 5f);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, iconSize * 1.0f, Time.deltaTime * 5f);
        }

    }

    public void SetScript(CS_ThiefAI thiefAI)
    {
        this.thiefAI = thiefAI;
        iconImage.sprite = thiefAI.read_IconSprite;
    }

    /// <summary>
    /// 敵がプレイヤーと同じ部屋に来て、アイコンが新たに追加表示される際の演出アニメーション。
    /// 90度傾いた状態から回転しながら拡大して通常表示になる。
    /// </summary>
    public void PlayAppearAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(AppearAnimationRoutine());
    }

    private IEnumerator AppearAnimationRoutine()
    {
        isPlayingAppearAnimation = true;

        Vector3 targetScale = iconSize * 1.2f;
        Quaternion targetRotation = transform.localRotation;
        Quaternion startRotation = targetRotation * Quaternion.Euler(0, 0, appearStartAngle);

        transform.localScale = Vector3.zero;
        transform.localRotation = startRotation;

        float elapsed = 0f;
        while (elapsed < appearAnimationDuration)
        {
            // 部屋移動演出などでTimeScaleが0になっていてもアニメーションを進行させる
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / appearAnimationDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // イーズアウト

            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, eased);
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, eased);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.localRotation = targetRotation;
        isPlayingAppearAnimation = false;
    }


    public CS_ThiefAI GetScript() => thiefAI;
}
