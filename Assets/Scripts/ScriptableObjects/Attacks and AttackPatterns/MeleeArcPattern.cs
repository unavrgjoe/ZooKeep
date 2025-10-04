using UnityEngine;

// Example Pattern: Melee Arc
[CreateAssetMenu(fileName = "MeleeArcPattern", menuName = "Combat/Patterns/MeleeArc")]
public class MeleeArcPattern : AttackPattern
{
    [Header("Arc Settings")]
    public float arcAngle = 90f;
    public float arcRadius = 2f;

    [Header("Visual Settings")]
    public GameObject slashVisualPrefab; // Assign the slash sprite prefab here
    public float visualDuration = 0.2f;
    public bool scaleVisualToArc = true;

#if UNITY_EDITOR
    private static Vector2 lastAttackOrigin;
    private static Vector2 lastAttackDirection;
    private static float gizmoDisplayTime;
#endif

    public override void Execute(Entity attacker, Vector2 targetPos, int damage)
    {
        Vector2 origin = attacker.transform.position;
        Vector2 direction = (targetPos - origin).normalized;

        // Create visual effect
        if (slashVisualPrefab != null)
        {
            CreateSlashVisual(origin, direction);
        }

        // Hit all enemies in arc
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, arcRadius, LayerMask.GetMask("Entities"));

        foreach (var hit in hits)
        {
            if (hit.transform == attacker.transform) continue;

            Vector2 toTarget = ((Vector2)hit.transform.position - origin).normalized;
            float angle = Vector2.Angle(direction, toTarget);

            if (angle <= arcAngle / 2f)
            {
                Entity target = hit.GetComponent<Entity>();
                target?.TakeDamage(damage);
            }
        }
#if UNITY_EDITOR
        Debug.DrawRay(origin, direction * arcRadius, Color.red, 0.5f);
        Debug.DrawRay(origin, Quaternion.Euler(0, 0, arcAngle / 2) * direction * arcRadius, Color.red, 0.2f);
        Debug.DrawRay(origin, Quaternion.Euler(0, 0, -arcAngle / 2) * direction * arcRadius, Color.red, 0.2f);
#endif

    }
    void CreateSlashVisual(Vector2 origin, Vector2 direction)
    {
        GameObject slash = Instantiate(slashVisualPrefab, origin, Quaternion.identity);

        // Rotate to face the attack direction
        float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        slash.transform.rotation = Quaternion.Euler(0, 0, rotationZ - 90); // -90 if sprite points up by default

        if (scaleVisualToArc)
        {
            // Scale the visual based on arc parameters
            // X scale = arc radius, Y scale = arc angle (as proportion)
            float xScale = arcRadius;
            float yScale = arcRadius * (arcAngle / 90f); // Normalize to 90 degree base
            slash.transform.localScale = new Vector3(xScale, yScale, 1);
        }

        // Add component to fade and destroy
        SlashVisualController slashController = slash.AddComponent<SlashVisualController>();
        slashController.Initialize(visualDuration);
    }

}

