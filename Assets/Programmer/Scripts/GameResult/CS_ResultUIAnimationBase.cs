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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
