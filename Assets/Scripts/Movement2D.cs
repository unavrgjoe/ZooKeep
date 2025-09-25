using UnityEngine;

public class Movement2D : MonoBehaviour
{ //lives on object with RB
    Rigidbody2D rb;
    public float speed;         // assigned from Entity or PlayerController
    private Vector2 direction;
    public float acceleration = 1;  // smoothing, can always be made 1 for enemies we want jagged movement from

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetMove(Vector2 dir)
    {
        direction = dir;
    }

    public void Stop()
    {
        direction = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {

        // Target velocity from last requested direction
        Vector2 targetVel = direction * speed;
        // Debug.Log("Target direction: " + direction + " Speed: " + speed);
        // Smooth toward target to avoid jitter / instant stops
        var maxDelta = acceleration * Time.fixedDeltaTime;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVel, maxDelta);
        //Debug.Log("maxDelta: " + maxDelta);
        // Debug.Log("Current Vel: " + rb.linearVelocity);
    }
    // FixedUpdate applies velocity based on the last SetMove
}