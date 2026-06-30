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
    public struct TransitionData
    {
        [Header("遷移ステータス")]
        [Tooltip("Canvas上の座標")] public Vector3Data transitionPos;
        [Tooltip("Canvas上のスケール")] public Vector3Data transitionScale;
        [Tooltip("回転")] public Vector3Data transitionRotate;
        
        [Header("遷移フラグ")]
        [Tooltip("座標を固定")] public bool useInitPos;
        [Tooltip("スケールを固定")] public bool useInitScale;
        [Tooltip("回転を固定")] public bool useInitRotate;

        [Header("遷移方法")]
        [Tooltip("座標移動に使用するイージング")] public Easing.EaseKind posEaseKind;
        [Tooltip("拡縮に使用するイージング")] public Easing.EaseKind scaleEaseKind;
        [Tooltip("回転に使用するイージング")] public Easing.EaseKind rotateEaseKind;

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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
