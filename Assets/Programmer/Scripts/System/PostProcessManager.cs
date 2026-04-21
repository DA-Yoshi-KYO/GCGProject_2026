using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PostProcessManager : MonoBehaviour
{
    [Header("ポストプロセス用マテリアル")]
    [SerializeField] private Material material;   // ポストプロセス用のマテリアル

    // エフェクトのオンオフを切り替えるためのフラグ
    [Header("グレースケール")] [SerializeField] bool GrayScale = false;  // グレースケール
    [Header("色反転")] [SerializeField] bool InvertColor = false;  // 色反転

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
