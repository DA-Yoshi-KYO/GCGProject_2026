using UnityEngine;
using UnityEngine.Rendering;

public class CS_ActiveGrayScale : MonoBehaviour
{
    bool isOnce = false;

    void Update()
    {
        if (!gameObject.activeSelf) return;
        if (isOnce) return;

        Volume volume = FindFirstObjectByType<Volume>();
        CSV_GrayScaleVolume gray;
        volume.profile.TryGet(out gray);
        gray.isEnabled.value = true;
        isOnce = true;
    }
}
