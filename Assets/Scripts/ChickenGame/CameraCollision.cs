using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("Settings")]
    public Transform playerTransform;
    public Vector3 defaultLocalPosition = new Vector3(0f, 1.4f, -2.2f);
    public float minDistance = 0.5f;
    public float maxDistance = 2.5f;
    public float smoothSpeed = 12f;
    public float collisionRadius = 0.25f;
    public LayerMask collisionMask;

    private Vector3 currentVelocity;

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform.parent;
        }

        if (collisionMask == 0)
        {
            collisionMask = LayerMask.GetMask("Default");
        }

        transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 pivotPosition = playerTransform.position + Vector3.up * 0.2f;

        Vector3 desiredLocalPos = defaultLocalPosition;
        Vector3 desiredWorldPos = playerTransform.TransformPoint(desiredLocalPos);

        Vector3 dir = (desiredWorldPos - pivotPosition).normalized;
        float desiredDistance = Vector3.Distance(pivotPosition, desiredWorldPos);

        RaycastHit hit;
        float finalDistance = desiredDistance;

        if (Physics.SphereCast(pivotPosition, collisionRadius, dir, out hit, desiredDistance, collisionMask))
        {
            finalDistance = Mathf.Max(minDistance, hit.distance - 0.15f);
        }

        Vector3 targetLocalPos = new Vector3(defaultLocalPosition.x, defaultLocalPosition.y, -finalDistance);
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * smoothSpeed);
    }
}
