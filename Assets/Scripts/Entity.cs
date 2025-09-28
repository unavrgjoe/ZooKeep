using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class Entity : MonoBehaviour
{
    [SerializeField] int maxHP;
    [SerializeField] int currentHP;
    [SerializeField] public Creature creature;
    [SerializeField] Movement2D movement;
    [SerializeField] AIController controller;
    [SerializeField] public int tier;
    [SerializeField] public List<FoodType> diet;
    public List<ItemSO> drops;


    void Awake()
    {
        // Auto-wire if left empty
        movement ??= GetComponentInChildren<Movement2D>();
        controller ??= GetComponentInChildren<AIController>();
        //attacks ??= GetComponentInChildren<AttackRunner>();

        // Apply blueprint (stats/attacks/behaviors) once
        if (creature != null)
        {
            diet = creature.diet;
            tier = creature.tier;
            maxHP = creature.HP;
            currentHP = maxHP;
            if (creature.drops != null & creature.drops.Count != 0) drops = creature.drops;
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
            Debug.Log(creature + " is dead");
            Debug.Log("d e a d");
            if (drops == null) Debug.Log("Drops is null");
            //instantiate item drops(or pull from pool)
            // Spawn all drops with position variance
            if (drops != null && drops.Count > 0)
            {
                Vector2 deathPosition = transform.position;

                foreach (ItemSO item in drops)
                {
                    if (item == null) continue;

                    // Add random offset for item spread
                    Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 1.5f; // 1.5 unit radius spread
                    Vector2 spawnPos = deathPosition + randomOffset;

                    // Spawn the item
                    if (ItemManager.Instance == null) Debug.Log("Dont have ItemManager");
                    ItemManager.Instance.SpawnItem(item, spawnPos, 1);
                }
            }
            //death animation
            Destroy(gameObject, 0.1f); //0.5 second destuction delay
        }
    }

    public void Heal(int Heal)
    {
        currentHP += Math.Min(Heal, maxHP);
    }

    #endregion
}
