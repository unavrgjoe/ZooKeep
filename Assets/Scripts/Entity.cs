using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Entity : MonoBehaviour
{
    [SerializeField] int maxHP;
    [SerializeField] int currentHP;
    [SerializeField] public Creature creature;
    [SerializeField] Movement2D movement;
    [SerializeField] AIController controller;
    [SerializeField] public int tier;

    void Awake()
    {
        // Auto-wire if left empty
        movement ??= GetComponentInChildren<Movement2D>();
        controller ??= GetComponentInChildren<AIController>();
        //attacks ??= GetComponentInChildren<AttackRunner>();

        // Apply blueprint (stats/attacks/behaviors) once
        if (creature != null)
        {
            tier = creature.tier;
            maxHP = creature.HP;
            currentHP = maxHP;
            if (movement != null) movement.speed = creature.speed;
            if (creature.acceleration == -1 || creature.acceleration == 0) movement.acceleration = 200;
            if (controller != null) { controller.tier = creature.tier; controller.vision = creature.vision; }
            //if (attacks != null) attacks.LoadSlots(definition.attacks);
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool invincible;
    #region Health and Death
    public void TakeDamage(int damage)
    {
        if (invincible)
        {
            return;
        }
        currentHP -= damage;
        if (currentHP <= 0)
        {
            HandleDeath();
        }

    }

    public void HandleDeath()
    {
        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("DEAD");
        }
        else
        {
            //instantiate item drops(or pull from pool)
            //death animation
            Destroy(gameObject);
        }
    }

    public void Heal(int Heal)
    {
        currentHP += Math.Min(Heal, maxHP);
    }

    #endregion
}
