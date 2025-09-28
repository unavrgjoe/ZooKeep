using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Base Item")]
public class ItemSO : ScriptableObject
{
    [Header("Base Properties")]
    public string itemName;
    public Sprite sprite;
    public int stackSize = 1;
    public float weight = 1f;

    [Header("Type")]
    public ItemType itemType;

    public virtual void OnPickup(Entity entity) { }
    public virtual void OnUse(Entity entity) { }
}

public enum ItemType //tobe used later hehehe jajaja
{
    Food,
    Material,
    Tool,
    Weapon
}

//Specific food items containging methods for healing and hunger reduction

[CreateAssetMenu(fileName = "FoodItem", menuName = "Items/Food")]
public class FoodItemSO : ItemSO
{
    [Header("Food Properties")]
    public float hungerReduction = 20f;
    public int healAmount = 0;
    public float eatTime = 1f; // How long it takes to eat

    [Header("Diet Restrictions")]
    public FoodType foodType = FoodType.None;
    public int minTierToEat = 0; // Minimum tier required to eat this
    public int maxTierToEat = 10; // Maximum tier that will eat this

    public override void OnUse(Entity entity)
    {
        // Food consumption handled by AIController/PlayerController
        var ai = entity.GetComponent<AIController>();
        if (ai != null)
        {
            ai.hunger = Mathf.Max(0, ai.hunger - hungerReduction);
        }

        if (healAmount > 0)
        {
            entity.Heal(healAmount);
        }
    }

    public bool CanEat(Entity entity)
    {
        if (entity.tier >= minTierToEat & entity.tier <= maxTierToEat) { return entity.diet.Contains(foodType); }
        else { return false; }
    }
}

public enum FoodType
{
    Meat,
    Plant,
    Processed,
    None
}