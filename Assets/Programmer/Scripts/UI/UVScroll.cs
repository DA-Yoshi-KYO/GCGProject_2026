using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UVScroller : MonoBehaviour
{
    [Header("スクロール設定")]
    [SerializeField] private float scrollSpeedX = 0.5f;
    [SerializeField] private float scrollSpeedY = 0f;

    private RawImage _rawImage;
    private Vector2 _uvOffset;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _uvOffset = _rawImage.uvRect.position;
    }

    private void Update()
    {
        _uvOffset += new Vector2(scrollSpeedX, scrollSpeedY) * Time.deltaTime;

        // 負数でも正しく0〜1にループする
        _uvOffset.x = Mathf.Repeat(_uvOffset.x, 1f);
        _uvOffset.y = Mathf.Repeat(_uvOffset.y, 1f);

        Rect rect = _rawImage.uvRect;
        rect.position = _uvOffset;
        _rawImage.uvRect = rect;
    }
}
