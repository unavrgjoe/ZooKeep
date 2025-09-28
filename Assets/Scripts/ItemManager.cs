// ItemManager.cs - Manages item spawning and pooling
using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    private static ItemManager _instance;

    [Header("Prefab")]
    public GameObject worldItemPrefab; // Simple prefab with just SpriteRenderer

    [Header("Pooling")]
    private Queue<WorldItem> itemPool = new Queue<WorldItem>();
    private int poolSize = 100;
    private Transform poolContainer;

    [Header("Active Items Tracking")]
    private HashSet<WorldItem> activeItems = new HashSet<WorldItem>();
    private List<WorldItem> itemsInRange = new List<WorldItem>(); // Reusable list for queries

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("[ItemManager] Duplicate detected, destroying this one.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        if (_instance != null)
        {

            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        //_instance.Test();
        //Instance.Test();
    }

    public static ItemManager Instance
    {
        get
        {
            if (_instance is null)
                Debug.LogError("ItemManager is NULL");
            return _instance;
        }
    }

    void InitializePool()
    {
        poolContainer = new GameObject("ItemPool").transform;
        poolContainer.parent = transform;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(worldItemPrefab, poolContainer);
            WorldItem item = obj.GetComponent<WorldItem>();
            obj.SetActive(false);
            itemPool.Enqueue(item);
        }
    }

    public void RegisterActiveItem(WorldItem item)
    {
        activeItems.Add(item);
    }

    public void UnregisterActiveItem(WorldItem item)
    {
        activeItems.Remove(item);
    }

    // Transform-based item detection - much faster than Physics2D.OverlapCircle
    public List<WorldItem> GetItemsInRange(Vector2 position, float range)
    {
        itemsInRange.Clear();
        float rangeSqr = range * range; // Avoid sqrt by comparing squared distances

        foreach (var item in activeItems)
        {
            if (item == null || !item.gameObject.activeInHierarchy) continue;

            float distSqr = (item.transform.position - (Vector3)position).sqrMagnitude;
            if (distSqr <= rangeSqr)
            {
                itemsInRange.Add(item);
            }
        }

        return itemsInRange;
    }

    public WorldItem GetClosestEditableFood(Entity entity, float maxRange)
    {
        WorldItem closest = null;
        float closestDistSqr = maxRange * maxRange;
        Vector3 entityPos = entity.transform.position;

        foreach (var item in activeItems)
        {
            if (item == null || !item.gameObject.activeInHierarchy) continue;

            // Check if it's food
            if (!(item.itemData is FoodItemSO food)) continue;

            // Check if entity can eat this food
            if (!food.CanEat(entity)) continue;

            float distSqr = (item.transform.position - entityPos).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = item;
            }
        }

        return closest;
    }

    public WorldItem SpawnItem(ItemSO itemData, Vector2 position, int quantity = 1)
    {
        Debug.Log("[IM] Spawning Item: " + itemData);
        WorldItem item = GetFromPool();
        item.transform.position = position;
        item.SetItemData(itemData);
        item.quantity = quantity;
        item.gameObject.SetActive(true);
        return item;
    }

    public void SpawnRandomFood(Vector2 position)  //Not something we need... YET
    {
        // Load all food items from Resources/Items/Food folder
        FoodItemSO[] foods = Resources.LoadAll<FoodItemSO>("Items/Food");
        if (foods.Length > 0)
        {
            FoodItemSO randomFood = foods[Random.Range(0, foods.Length)];
            SpawnItem(randomFood, position);
        }
    }

    WorldItem GetFromPool()
    {
        if (itemPool.Count > 0)
        {
            Debug.Log("Retrieved item from pool");
            return itemPool.Dequeue();
        }
        else
        {
            Debug.Log("Pool empty, creating new item");
            GameObject obj = Instantiate(worldItemPrefab, poolContainer);
            return obj.GetComponent<WorldItem>();
        }
    }

    public void ReturnToPool(WorldItem item)
    {
        item.gameObject.SetActive(false);
        item.transform.parent = poolContainer;
        itemPool.Enqueue(item);
    }
}


// ================ HOW ITEMMANAGER WORKS ================
/*
The ItemManager is a centralized system that:

1. OBJECT POOLING:
   - Pre-creates 100+ WorldItem objects at startup
   - Stores inactive items in a Queue for O(1) retrieval
   - When you "destroy" an item, it just goes back to the pool
   - No garbage collection, no Instantiate/Destroy overhead

2. ACTIVE ITEM TRACKING:
   - Maintains a HashSet of all active items in the world
   - Items auto-register when enabled, unregister when disabled
   - Provides fast lookups without needing physics or tags

3. SPATIAL QUERIES:
   - GetItemsInRange(): Returns all items within a radius
   - GetClosestFood(): Finds nearest food item
   - Uses squared distances (no sqrt) for performance
   - Reuses the same List to avoid allocations

4. SPAWNING SYSTEM:
   - SpawnItem(): Places an item at a position
   - Pulls from pool, configures it, activates it
   - Can spawn specific items or random food

SETUP INSTRUCTIONS:
1. Create empty GameObject, add ItemManager script
2. Create item prefab: GameObject with SpriteRenderer + WorldItem script
3. Assign prefab to ItemManager's worldItemPrefab field
4. Create food ScriptableObjects in Assets (Create > Items > Food)
5. That's it! No layers, no physics setup needed
*/