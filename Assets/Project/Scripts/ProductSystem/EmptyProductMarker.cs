using UnityEngine;

public sealed class EmptyProductMarker : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobSpeed = 2.5f;

    private Vector3 _startLocalPosition;

    private void Awake()
    {
        _startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = _startLocalPosition + Vector3.up * offset;
    }
}