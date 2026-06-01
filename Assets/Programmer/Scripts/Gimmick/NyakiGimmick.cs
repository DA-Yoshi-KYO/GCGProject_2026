using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NyakiGimmick : GimmickBase
{
    //[SerializeField]
    private Volume volume;

    [Header("校歌時間")]
    [SerializeField]
    private float time;

    CSV_CatEye catEye;
    private bool isFirstActive = false;

    // Start is called before the first frame update
    void Start()
    {
        volume = FindFirstObjectByType<Volume>();

        if (volume != null)
        {
            volume.profile.TryGet(out catEye);
        }
        gimmickState = GimmickState.Active;
    }

    // Update is called once per frame
    protected override void ActiveUpdate()
    {
        if (!isFirstActive)
        {
            isFirstActive = true;
            if (volume.profile.TryGet(out catEye))
            {
                catEye.active = true;
                catEye.isEnabled.value = true;
            }
        }
        SetHitChecker(transform.position);
        //時間でエフェクト消去
        time -= Time.deltaTime;
        if(time <= 0)
        {
            gimmickState = GimmickState.Broken;
        }
    }

    protected override void BrokenUpdate()
    {
        if (catEye != null)
        {
            catEye.isEnabled.value = false;
        }
        DeleteHitChecker();
        Destroy(gameObject);
    }
}
