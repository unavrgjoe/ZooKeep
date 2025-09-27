using UnityEngine;

[CreateAssetMenu(fileName = "SwordAttack", menuName = "Combat/Attacks/Sword")]
public class SwordAttackSO : AttackSO
{
    [Header("Sword Specific")]
    public float parryWindow = 0.3f;

    // Override the virtual method if you want different behavior
    public override void Execute(Entity attacker, Vector2 targetPos)
    {
        // Custom sword logic
        Debug.Log("Sword swing!");
        pattern.DrawAttackGizmo(); //assuming MeleeAttackPattern which actually has this method implemented and not empty
        base.Execute(attacker, targetPos); // Call parent's version
        // Could add: Check for parry, play sword-specific effects, etc.
    }

    // Add new methods specific to swords
    public bool CanParry()
    {
        return parryWindow > 0;
    }


}
