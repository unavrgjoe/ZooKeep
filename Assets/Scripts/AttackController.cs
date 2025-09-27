using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class AttackController : MonoBehaviour
{
    [Header("Available Attacks")]
    public List<AttackSO> availableAttacks = new List<AttackSO>(); //includes different attacks from the same weapon (for the player)

    private Vector2 AttackDirection;

    // Cooldown tracking
    private Dictionary<AttackSO, float> cooldowns = new Dictionary<AttackSO, float>(); //for active cooldwons

    private Entity entity;
    void Awake()
    {
        entity = GetComponent<Entity>();
        InitializeCooldowns();
    }
    void InitializeCooldowns()
    {
        cooldowns.Clear();
        foreach (var attack in availableAttacks)
        {
            if (attack != null)
                cooldowns[attack] = 0f;
        }
    }

    [Header("Debug View")]
    [SerializeField] private List<AttackCooldownDebug> cooldownsDebug = new List<AttackCooldownDebug>();

    [System.Serializable]
    private class AttackCooldownDebug
    {
        public string attackName;
        public float remainingCooldown;

        public AttackCooldownDebug(string name, float cooldown)
        {
            attackName = name;
            remainingCooldown = cooldown;
        }
    }
    void UpdateDebugView()
    {
        cooldownsDebug.Clear();
        foreach (var kvp in cooldowns)
        {
            if (kvp.Value > 0)
                cooldownsDebug.Add(new AttackCooldownDebug(kvp.Key.attackName, kvp.Value));
        }
    }

    // Check if an attack can be used
    public bool CanUseAttack(AttackSO attack, Vector2 target)
    {
        if (attack == null) return false;
        if (!availableAttacks.Contains(attack)) return false;
        if (GetCooldown(attack) > 0) return false;
        // if (AIController) //first if statement checks for AIController or whatever else to see if its ai, if the player skip the second if
        //     if (!attack.IsInRange(transform.position, target)) return false;
        return true;
    }
    public float GetCooldown(AttackSO attack)
    {
        if (cooldowns.TryGetValue(attack, out float cd))
            return cd;
        return 0f;
    }

    public bool TryAttack(AttackSO attack, Vector2 target)
    {
        if (!CanUseAttack(attack, target)) return false;

        // Executes the attack
        attack.Execute(entity, target);

        // Set th cooldown
        cooldowns[attack] = attack.cooldown;

        return true;
    }

    public List<AttackSO> GetUsableAttacks(Vector2 target)
    {
        List<AttackSO> usable = new List<AttackSO>();
        foreach (var attack in availableAttacks)
        {
            if (CanUseAttack(attack, target))
                usable.Add(attack);
        }
        return usable;
    }

    void Update()
    {
        // Update all cooldowns
        List<AttackSO> attacks = new List<AttackSO>(cooldowns.Keys);
        foreach (var attack in attacks)
        {
            if (cooldowns[attack] > 0)
                cooldowns[attack] = Mathf.Max(0, cooldowns[attack] - Time.deltaTime);
        }

        // Update debug view
        UpdateDebugView();
    }


    // private AttackType[] AvailableAttacks; better set either on PlayerController or AIController because theyll have
    // distinguishing features between them.. ex player uses item system -- items use attack SO

    //Take input of ...
    //attack  SO (type) (a scriptable object for the attack itself?)
    //Attack target as Vector 2 (which thus has to be an input param of the attack SO along with the entity dmg)

    //Have PlayerController handle inputs from player that feed in a specific Attack from a list of the Attack SO as well 
    //as the target Vector2

    //Every Attack has a Range, Cooldown, AI is going to need an AttackBrain SO to determine which attack to use from the list available
    //that would be set on the creature, then set on AIController from there...
    //AtkBrain chooses randomly from attacks that are in range to target and not on cooldwon to target or alt. entirely random (set a range to positive.infinityd)
    //AIController set either a Vector2 target or no target depending on current behavior state,
    //^ set this in update()



    //other utility
    public void RemoveAttack(AttackSO attack)
    {
        if (availableAttacks.Contains(attack))
        {
            availableAttacks.Remove(attack);
            cooldowns.Remove(attack);
        }
    }
    // Adding attack at runtime (ex. from item pickup)
    public void AddAttack(AttackSO attack)
    {
        if (attack != null && !availableAttacks.Contains(attack))
        {
            availableAttacks.Add(attack);
            cooldowns[attack] = 0f;
        }
    }

}

