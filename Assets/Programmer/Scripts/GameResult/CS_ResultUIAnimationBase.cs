using System;
using UnityEngine;

public class CS_ResultUIAnimationBase : MonoBehaviour
{
    [Serializable]
    public struct Vector3Data
    {
        [Tooltip("初期値")] public Vector3 init;
        [Tooltip("目標値")] public Vector3 target;
    }

    [Serializable]
    public struct ColorData
    {
        [Tooltip("初期値")] public Color init;
        [Tooltip("目標値")] public Color target;
    }
    
    [Serializable]
    public struct TransitionData
    {
        [Header("遷移ステータス")]
        [Tooltip("Canvas上の座標")] public Vector3Data transitionPos;
        [Tooltip("Canvas上のスケール")] public Vector3Data transitionScale;
        [Tooltip("回転")] public Vector3Data transitionRotate;
        [Tooltip("色")] public ColorData transitionColor;
        
        [Header("遷移フラグ")]
        [Tooltip("座標を固定")] public bool useInitPos;
        [Tooltip("スケールを固定")] public bool useInitScale;
        [Tooltip("回転を固定")] public bool useInitRotate;
        [Tooltip("色を固定")] public bool useInitColor;

        [Header("遷移方法")]
        [Tooltip("座標移動に使用するイージング")] public Easing.EaseKind posEaseKind;
        [Tooltip("拡縮に使用するイージング")] public Easing.EaseKind scaleEaseKind;
        [Tooltip("回転に使用するイージング")] public Easing.EaseKind rotateEaseKind;
        [Tooltip("色遷移に使用するイージング")] public Easing.EaseKind colorEaseKind;

        [Header("オブジェクト")]
        [Tooltip("アニメーションに使用するオブジェクト")] public GameObject imageObject;  // アニメーションに使用するオブジェクト
    }

    [Serializable]
    public struct TransitionPhase
    {
        public int[] sameTimeAnimationIndex;
        public float animationDuration;
    }
    [SerializeField] protected TransitionPhase[] phase;
    protected int progressIndex = 0;
    // 経過時間計測
    protected float timer = 0.0f;

    [Header("外部から格納するUI情報"), SerializeField]
    protected TransitionData[] inputDatas =
    {
        new TransitionData()
        {
            transitionPos = new Vector3Data() { init = new Vector3(0f, 0f, 0f), target = new Vector3(0f, 0f, 0f) },
            transitionScale = new Vector3Data() { init = new Vector3(1f, 1f, 1f), target = new Vector3(1f, 1f, 1f) },
            transitionRotate = new Vector3Data() { init = new Vector3(0f, 0f, 0f), target = new Vector3(0f, 0f, 0f) },
            transitionColor = new ColorData() { init = new Color(1f, 1f, 1f, 1f), target = new Color(1f, 1f, 1f, 1f) },

            useInitPos = true,
            useInitScale = true,
            useInitRotate = true,
            useInitColor = true,

            posEaseKind = Easing.EaseKind.Linear,
            scaleEaseKind = Easing.EaseKind.Linear,
            rotateEaseKind = Easing.EaseKind.Linear,
            colorEaseKind = Easing.EaseKind.Linear,

            imageObject = null
        }
    };

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (phase.Length <= progressIndex)
            return;

        float duration = phase[progressIndex].animationDuration;
        timer += Time.deltaTime;
        timer = Mathf.Clamp(timer, 0f, duration);

        foreach (int index in phase[progressIndex].sameTimeAnimationIndex)
        {
            TransitionData data = inputDatas[index];
            GameObject imageObject = data.imageObject;
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            if (!data.useInitPos)
            {
                rectTransform.anchoredPosition = data.transitionPos.init +
                (data.transitionPos.target - data.transitionPos.init) *
                Easing.Ease(data.posEaseKind, timer, duration);
            }
            if (!data.useInitScale)
            {
                rectTransform.localScale = data.transitionScale.init +
                (data.transitionScale.target - data.transitionScale.init) *
                Easing.Ease(data.scaleEaseKind, timer, duration);
            }
            if (!data.useInitRotate)
            {
                Vector3 initRotateRadian = new Vector3(
                    data.transitionRotate.init.x * Mathf.Deg2Rad,
                    data.transitionRotate.init.y * Mathf.Deg2Rad,
                    data.transitionRotate.init.z * Mathf.Deg2Rad);
                Vector3 targetRotateRadian = new Vector3(
                    data.transitionRotate.target.x * Mathf.Deg2Rad,
                    data.transitionRotate.target.y * Mathf.Deg2Rad,
                    data.transitionRotate.target.z * Mathf.Deg2Rad);
                rectTransform.Rotate(initRotateRadian +
                (targetRotateRadian - initRotateRadian) *
                Easing.Ease(data.rotateEaseKind, timer, duration));
            }
            if (!data.useInitColor)
            {
                UnityEngine.UI.Image image = imageObject.GetComponent<UnityEngine.UI.Image>();
                image.color = data.transitionColor.init +
                (data.transitionColor.target - data.transitionColor.init) *
                Easing.Ease(data.colorEaseKind, timer, duration);
            }
        }

        if (timer >= duration)
        {
            progressIndex++;
            timer = 0f;
        }
    }
}
