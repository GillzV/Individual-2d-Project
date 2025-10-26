using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          

    [Header("Behavior")]
    public float smoothTime = 0.15f;  // This is how smooth the camera follow my character
    public Vector2 offset = new Vector2(1.5f, 1f);
    public bool lockY = true;         // Locks the Y position of camera

    [Header("Optional Bounds")]
    public Vector2 minBounds = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
    public Vector2 maxBounds = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

    private Vector3 _velocity;

    void LateUpdate()       //after the character moves, this updates to follow the character
    {
        if (!target) return;

        float targetX = target.position.x + offset.x;
        float targetY = lockY ? transform.position.y : target.position.y + offset.y;

        targetX = Mathf.Clamp(targetX, minBounds.x, maxBounds.x);
        targetY = Mathf.Clamp(targetY, minBounds.y, maxBounds.y);

        var desired = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
    }
}
