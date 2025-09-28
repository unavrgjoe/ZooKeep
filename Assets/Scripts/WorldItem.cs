// WorldItem.cs - Lightweight component for items in the worldspace
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WorldItem : MonoBehaviour
{
    [Header("Item Data")]
    public ItemSO itemData; //what item is it? relevant for nabbing the sprite?
    public int quantity = 1;

    private SpriteRenderer spriteRenderer;

    [Header("Optimization")]
    private float lastDistanceCheck;
    private const float CULL_DISTANCE = 30f;
    private const float CHECK_INTERVAL = 0.5f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetItemData(itemData);
    }

    private void OnEnable() => ItemManager.Instance?.RegisterActiveItem(this);
    private void OnDisable() => ItemManager.Instance?.UnregisterActiveItem(this);

    public void SetItemData(ItemSO data)
    {
        itemData = data;
        if (data != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = data.sprite;
            gameObject.name = $"WorldItem_{data.itemName}";
        }
    }

    void Update()
    {
        // Simple distance culling for optimization
        if (Time.time - lastDistanceCheck > CHECK_INTERVAL)
        {
            lastDistanceCheck = Time.time;
            CheckDistanceCulling();
        }
    }

    void CheckDistanceCulling()
    {
        if (Camera.main == null) return;

        float dist = Vector2.Distance(transform.position, Camera.main.transform.position);
        bool shouldCull = dist > CULL_DISTANCE;

        spriteRenderer.enabled = !shouldCull;
    }

    public void PickUp(Entity entity)
    {
        if (itemData == null) return;

        itemData.OnPickup(entity);

        // For now, just destroy. Later integrate with inventory
        ItemManager.Instance.ReturnToPool(this);
    }

    public bool TryEat(Entity entity)
    {
        if (itemData is FoodItemSO food)
        {
            if (food.CanEat(entity))
            {
                food.OnUse(entity);
                ItemManager.Instance.ReturnToPool(this);
                return true;
            }
        }
        return false;
    }
}
