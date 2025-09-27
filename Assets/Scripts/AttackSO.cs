using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
//this script also includes the Attack Pattern SOs and the AttackBrain SO and AttackBrain SO implementations
[CreateAssetMenu(fileName = "Attack", menuName = "Combat/Attack")]
public class AttackSO : ScriptableObject
{

    public string attackName;
    public int damage;
    public float range;
    public float cooldown;

    [Header("Attack Pattern")]
    public AttackPattern pattern; // Modular component


    public virtual bool IsInRange(Vector2 origin, Vector2 target)
    {
        float distance = Vector2.Distance(origin, target);
        return distance <= range;
    }


    // [Header("Visual/Audio")]
    // public GameObject effectPrefab;
    // public AudioClip attackSound;
    public virtual void Execute(Entity attacker, Vector2 targetPos) //executes the attack, cooldown logic on AttackController
    {
        // attern to determine actual damage dealing
        pattern?.Execute(attacker, targetPos, damage);

        // Play effects if any

        //if (effectPrefab != null)
        // {
        //     GameObject effect = Instantiate(effectPrefab, attacker.transform.position, Quaternion.identity);
        //     Destroy(effect, 2f); // Clean up after 2 seconds
        // }

        // if (attackSound != null)
        // {
        //     AudioSource.PlayClipAtPoint(attackSound, attacker.transform.position);
        // }
    }
}



public abstract class AttackPattern : ScriptableObject  //Serves as template for specific kinds of attacks like hitscans
{
    public abstract void Execute(Entity attacker, Vector2 targetPos, int damage);
    public virtual void DrawAttackGizmo() { }
}

[CreateAssetMenu(fileName = "ProjectilePattern", menuName = "Combat/Patterns/Projectile")]
public class ProjectilePattern : AttackPattern
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    public override void Execute(Entity attacker, Vector2 targetPos, int damage)
    {
        Vector2 origin = attacker.transform.position;
        Vector2 direction = (targetPos - origin).normalized;

        GameObject proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        //Projectile projectile = proj.GetComponent<Projectile>();
        //projectile?.Initialize(damage, direction * projectileSpeed, attacker);
    }
}

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

public class SlashVisualController : MonoBehaviour
{
    private float duration;
    private float timer;
    private SpriteRenderer spriteRenderer;

    public void Initialize(float duration)
    {
        this.duration = duration;
        this.timer = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SlashVisualController requires a SpriteRenderer!");
            Destroy(gameObject);
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Fade out
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = color;

        }

        // light scale increase for impact
        transform.localScale = transform.localScale * (1f + Time.deltaTime * 0.5f);
    }
}

// Example Pattern: Hitscan
[CreateAssetMenu(fileName = "HitscanPattern", menuName = "Combat/Patterns/Hitscan")]
public class HitscanPattern : AttackPattern
{
    public float maxRange = 20f;
    public bool piercing = false;
    public int maxTargets = 1;

    public override void Execute(Entity attacker, Vector2 targetPos, int damage)
    {
        Vector2 origin = attacker.transform.position;
        Vector2 direction = (targetPos - origin).normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxRange, LayerMask.GetMask("Entities"));

        int targetsHit = 0;
        foreach (var hit in hits)
        {
            if (hit.transform == attacker.transform) continue;

            Entity target = hit.collider.GetComponent<Entity>();
            if (target != null)
            {
                target.TakeDamage(damage);
                targetsHit++;

                if (!piercing || targetsHit >= maxTargets) break;
            }
        }
    }
}

// Example Pattern: Area of Effect
[CreateAssetMenu(fileName = "AOEPattern", menuName = "Combat/Patterns/AOE")]
public class AOEPattern : AttackPattern
{
    public float radius = 3f;
    public bool damageAttacker = false;

    public override void Execute(Entity attacker, Vector2 targetPos, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, radius, LayerMask.GetMask("Entities"));

        foreach (var hit in hits)
        {
            if (!damageAttacker && hit.transform == attacker.transform) continue;

            Entity target = hit.GetComponent<Entity>();
            target?.TakeDamage(damage);
        }
    }
}

public abstract class AttackBrainSO : ScriptableObject  //brains return a usable attack for the entity, using range, randomness and patterns to determine which
{
    public abstract AttackSO SelectAttack(AttackController controller, Entity target);
}


[CreateAssetMenu(fileName = "ShortestRangeBrain", menuName = "AI/AttackBrains/ShortestRange")]
public class ShortestRangeAttackBrain : AttackBrainSO
{
    public override AttackSO SelectAttack(AttackController controller, Entity target)
    {
        if (target == null) return null;

        Vector2 targetPos = target.transform.position;
        List<AttackSO> usableAttacks = controller.GetUsableAttacks(targetPos);

        if (usableAttacks.Count == 0) return null;

        // Sort by range and return shortest
        usableAttacks.Sort((a, b) => a.range.CompareTo(b.range));
        return usableAttacks[0];
    }
}

[CreateAssetMenu(fileName = "PatternBrain", menuName = "AI/AttackBrains/Pattern")]
public class PatternAttackBrain : AttackBrainSO
{
    [SerializeField] private AttackSO[] attackPattern;
    private int currentIndex = 0;

    public override AttackSO SelectAttack(AttackController controller, Entity target)
    {
        if (target == null || attackPattern.Length == 0) return null;

        Vector2 targetPos = target.transform.position;

        // Try attacks in pattern order
        for (int i = 0; i < attackPattern.Length; i++)
        {
            int index = (currentIndex + i) % attackPattern.Length;
            AttackSO attack = attackPattern[index];

            if (controller.CanUseAttack(attack, targetPos))
            {
                currentIndex = (index + 1) % attackPattern.Length;
                return attack;
            }
        }

        return null;
    }
}