using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class AIController : MonoBehaviour
{
    [Header("Refs")]
    public Movement2D movement;
    //public AttackContoller attack;
    public Entity entity;

    [Header("Behavior Set (priority from data)")]
    public List<BehaviorSO> behaviors;

    //for perceiving surroundings
    Rigidbody2D rb;
    Transform tf;
    public int tier; //primarily held on Entity
    public float vision; //radius of view to identify targets and food

    [Header("FOOOOOOOOD")]
    public float hunger = 100;
    public float hungerRate = 2f;
    private float lastFoodScan = 0f;
    private WorldItem targetFood;
    private const float FOOD_SCAN_INTERVAL = 1f;

    [Header("Other")]
    public LayerMask targetLayers;
    public Entity predator; //current
    public Entity prey = null; //current
    public AttackController attackController;
    public AttackBrainSO attackBrain; //used to determine which attack to use... and hopefully other combat focused ai later
    void Awake()
    {
        targetLayers = LayerMask.GetMask("Entities");
        tf = transform;
        rb = GetComponent<Rigidbody2D>();
        movement ??= GetComponent<Movement2D>();
        //attacks  ??= GetComponent<AttackController>();
        entity ??= GetComponent<Entity>();
        attackController ??= GetComponent<AttackController>();
        if (entity != null) tier = entity.tier; vision = entity.creature.vision; behaviors = entity.creature.behaviors;
        hunger = 100; //was mostly for testing but it kinda makes sense animals spawn in hungry
    }
    public List<Entity> visibleEntities = new List<Entity>();

    void OnEnable() { GameTimer.Tick2s += Perceive; }
    void OnDisable() { GameTimer.Tick2s -= Perceive; }
    void Perceive()
    {
        hunger += Time.deltaTime * 10; //uptick hunger cuz why not do it here
        // 1) Grab everything in range on the target layers
        var hits = Physics2D.OverlapCircleAll(tf.position, vision, targetLayers);

        // 2) Sort colliders by squared distance (fast, no sqrt)
        System.Array.Sort(hits, (a, b) =>
        {
            // null checks just in case
            if (a == null) return 1;
            if (b == null) return -1;
            var pa = (Vector2)a.transform.position;
            var pb = (Vector2)b.transform.position;
            var p0 = (Vector2)tf.position;
            float da = (pa - p0).sqrMagnitude;
            float db = (pb - p0).sqrMagnitude;
            return da.CompareTo(db);
        });

        // 3) Fill nearest-first entities list (skip self, dedupe by reference)
        visibleEntities.Clear();
        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            // ignore self (same rigidbody or same root)
            if (rb != null && c.attachedRigidbody == rb) continue;

            var ent = c.GetComponentInParent<Entity>();
            if (ent == null) continue;

            if (!visibleEntities.Contains(ent))
            { // simple dedupe if a creature has multiple colliders
                //Debug.Log("Adding to Visisble Entities");
                visibleEntities.Add(ent);
            }
        }
        //Debug.Log("Visisble Entities: " + visibleEntities);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, vision);
    }
    BehaviorSO best = null; //the currently active behavior
    void Update()
    {

        int bestScore = -2;
        foreach (var b in behaviors)
        {

            if (!b) continue;
            int s = b.Score(this); //calls a method on each of the behaviors to determine their priority (cant just have a var for this cuz variables cant be changed in-game on SO)
            Debug.Log("Behavior: " + b + " Score: " + s);
            if (s > bestScore) { bestScore = s; best = b; }
        }
    }

    void FixedUpdate()
    {
        best?.OnPriority(this);
    }

    public void UpdateCombat(Entity attackTarget) //called from current behavior
    {
        if (attackTarget != null && attackBrain != null && attackController != null)
        {
            AttackSO selectedAttack = attackBrain.SelectAttack(attackController, attackTarget);
            if (selectedAttack != null)
            {
                attackController.TryAttack(selectedAttack, attackTarget.transform.position);
            }
        }
    }

    public WorldItem ScanForFood()
    {
        if (Time.time - lastFoodScan < FOOD_SCAN_INTERVAL) return targetFood;
        lastFoodScan = Time.time;

        // Use ItemManager's efficient transform-based detection
        List<WorldItem> nearbyItems = ItemManager.Instance.GetItemsInRange(transform.position, vision);

        WorldItem closestFood = null;
        float closestDistSqr = float.MaxValue;

        foreach (var item in nearbyItems)
        {
            //if (!(item.itemData is FoodItemSO food)) continue;

            // Check if we can eat this food
            if (item.itemData is FoodItemSO food)
            {
                if (food.CanEat(entity))
                {
                    float distSqr = (item.transform.position - transform.position).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                        closestFood = item;
                    }
                }
                else { continue; }
            }
            else { continue; }
        }
        targetFood = closestFood;
        return targetFood;
    }

    public void TryEatFood(WorldItem food)
    {
        if (food == null) return;

        float distSqr = (food.transform.position - transform.position).sqrMagnitude;
        if (distSqr < 2.25f) // 1.5f squared - eating range
        {
            food.TryEat(entity);
            targetFood = null;
        }
    }

}