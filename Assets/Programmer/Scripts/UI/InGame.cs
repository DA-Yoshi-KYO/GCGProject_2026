using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame : MonoBehaviour
{
    private CS_EndManager endManager;

    private void Start()
    {
        endManager = GameObject.FindObjectOfType<CS_EndManager>();
    }

    private void Update()
    {
        if(endManager == null)
        {
            Debug.LogWarning("CS_EndManagerが見つかりません。");
            endManager = GameObject.FindObjectOfType<CS_EndManager>();
            return;
        }

        if (endManager.read_IsEnd)
        {
            gameObject.SetActive(false);
        }
    }
}
