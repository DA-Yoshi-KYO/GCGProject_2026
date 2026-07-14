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
        if (volume == null) return;
        CSV_GrayScaleVolume gray;
        if (!volume.profile.TryGet(out gray) || gray == null) return;
        gray.isEnabled.value = true;
        isOnce = true;
    }
}
