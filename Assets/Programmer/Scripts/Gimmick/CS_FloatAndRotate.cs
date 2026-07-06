using UnityEngine;

public class CS_FloatAndRotate : MonoBehaviour
{
    [Header("上下移動")]
    [SerializeField]
    private float f_MoveHeight = 0.3f;

    [SerializeField]
    private float f_MoveSpeed = 1.0f;

    [Header("回転")]
    [SerializeField]
    private float f_RotateSpeed = 30.0f;

    private Vector3 v3_StartPosition;

    private void Start()
    {
        v3_StartPosition = transform.position;
    }

    private void Update()
    {
        MoveUpDown();
        RotateSlowly();
    }

    private void MoveUpDown()
    {
        Vector3 pos = v3_StartPosition;

        pos.y += Mathf.Sin(Time.time * f_MoveSpeed) * f_MoveHeight;

        transform.position = pos;
    }

    private void RotateSlowly()
    {
        transform.Rotate(
            0.0f,
            f_RotateSpeed * Time.deltaTime,
            0.0f,
            Space.World);
    }
}
