using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTimeSetting : MonoBehaviour
{
    [Header("加速倍率")]
    [SerializeField] private float timeScale = 1.0f;

    [Header("有効化")]
    [SerializeField] private bool isEnabled = true;

    private void Update()
    {
        if (isEnabled)
        {
            if (Time.timeScale <= 0.0f) return;

            Time.timeScale = timeScale;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }
}
