using UnityEngine;

[RequireComponent(typeof(Movement2D))]
public class Pathfinding : MonoBehaviour
{
    public Vector2 intendedDirection;
    public Vector2 actualDirection;
    private Movement2D movement;
    private Transform tf;

    [Header("Pathfinding Settings")]
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private int rayCount = 12;
    [SerializeField] private LayerMask obstacleLayer;
    // Cached values to avoid allocations
    private float angleStep;
    private RaycastHit2D hit;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;


    void Awake()
    {
        movement = GetComponent<Movement2D>();
        tf = transform;
        obstacleLayer = LayerMask.GetMask("Default");
        angleStep = 360f / rayCount;
    }

    public void SetMove(Vector2 direction)
    {
        intendedDirection = direction.normalized;

        if (intendedDirection.sqrMagnitude > 0.01f)
        {
            actualDirection = CalculateActualDirection();
        }
        else
        {
            actualDirection = Vector2.zero;
        }
        movement.SetMove(actualDirection);
    }

    private Vector2 CalculateActualDirection()
    {
        // Get the angle of intended direction
        float baseAngle = Mathf.Atan2(intendedDirection.y, intendedDirection.x) * Mathf.Rad2Deg;

        // Check rays in priority order: center first, then alternating left/right
        for (int i = 0; i < rayCount; i++)
        {
            int offset = (i + 1) / 2; // 0, 1, 1, 2, 2, 3, 3...
            bool isLeft = (i % 2 == 0); // Alternate left/right

            float angleOffset = isLeft ? -offset * angleStep : offset * angleStep;
            float checkAngle = baseAngle + angleOffset;

            Vector2 rayDir = new Vector2(
                Mathf.Cos(checkAngle * Mathf.Deg2Rad),
                Mathf.Sin(checkAngle * Mathf.Deg2Rad)
            );

            hit = Physics2D.Raycast(tf.position, rayDir, rayDistance, obstacleLayer);

            if (showDebugRays)
            {
                Color rayColor = hit.collider != null ? Color.red : Color.green;
                Debug.DrawRay(tf.position, rayDir * rayDistance, rayColor);
            }

            // If this ray is clear, use it
            if (hit.collider == null)
            {
                return rayDir;
            }
        }

        // All rays blocked - return intended direction anyway (entity is stuck)
        return intendedDirection;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw intended direction in yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, intendedDirection * rayDistance);

        // Draw actual direction in cyan
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, actualDirection * rayDistance * 1.1f);
    }
}
