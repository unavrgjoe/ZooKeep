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
    //public BehaviorSO[] behaviors;   // plug BehaviorSO assets 
    //public TraitSO[] traits;
    public List<string> states = new List<string>();
    public List<BehaviorSO> behaviors;


    //for perceiving surroundings
    Rigidbody2D rb;
    Transform tf;
    public int tier; //primarily held on Entity

    public float vision; //radius of view to identify targets

    public LayerMask targetLayers;
    public Entity predator; //current
    public Entity prey = null; //current
    void Awake()
    {
        targetLayers = LayerMask.GetMask("Entities");
        tf = transform;
        rb = GetComponent<Rigidbody2D>();
        movement ??= GetComponent<Movement2D>();
        //attacks  ??= GetComponent<AttackController>();
        entity ??= GetComponent<Entity>();
        if (entity != null) tier = entity.tier; vision = entity.creature.vision; behaviors = entity.creature.behaviors;
    }
    public List<Entity> visibleEntities = new List<Entity>();

    void OnEnable() { GameTimer.Tick2s += Perceive; }
    void OnDisable() { GameTimer.Tick2s -= Perceive; }
    void Perceive()
    {
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
                Debug.Log("Adding to Visisble Entities");
                visibleEntities.Add(ent);
            }
        }
        Debug.Log("Visisble Entities: " + visibleEntities);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, vision);
    }
    BehaviorSO best = null;
    void Update()
    {

        int bestScore = -2;
        foreach (var b in behaviors)
        {
            if (!b) continue;
            int s = b.Score(this); //calls a method on each of the behaviors to determine their priority (cant just have a var for this cuz variables cant be changed in-game on SO)
            if (s > bestScore) { bestScore = s; best = b; }
        }


    }

    void FixedUpdate()
    {
        best?.OnPriority(this);
    }

}