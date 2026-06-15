using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CS_CutSceneVideoTrigger : MonoBehaviour
{
    [Header("再生する場面")][SerializeField] public string situation;
    private void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag("Player"))
        //    return;

        ////再生するデータの場面の設定
        //for (int i = 0 ; i < cutScenedata.Length ; ++i)
        //{
        //    if (situation == cutScenedata[i].situation)
        //    {
        //        setNumber = i;
        //    }
        //}

        //if (timelinedata.timeLineDatas[number].start)
        //    return;

        //videoPlayer.clip = timelinedata.timeLineDatas[number].videoClip;
        //rawImage.enabled = true;
    }
}
