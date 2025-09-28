using UnityEngine;

[CreateAssetMenu(menuName = "Creatures/FoodSeek", fileName = "FoodSeekBehaviorSO")]
public class FoodSeekBehavior : BehaviorSO
{
    [Header("Priority Settings")]
    public int lowHungerPriority = 0;
    public int mediumHungerPriority = 30;
    public int highHungerPriority = 50;
    public int starvingPriority = 200;

    public override int Score(AIController ai)
    {
        // No food visible? No priority
        if (ai.ScanForFood() == null) return -1;

        float hungerPercent = ai.hunger / 100f;

        if (hungerPercent < 0.3f) return lowHungerPriority;
        if (hungerPercent < 0.5f) return mediumHungerPriority;
        if (hungerPercent < 0.8f) return highHungerPriority;
        return starvingPriority;
    }

    public override void OnPriority(AIController ai)
    {
        WorldItem food = ai.ScanForFood();
        if (food == null)
        {
            // Wander to find food
            ai.movement.SetMove(Random.insideUnitCircle);
            return;
        }

        // Move towards food
        Vector2 direction = (food.transform.position - ai.transform.position).normalized;
        ai.movement.SetMove(direction);

        // Try to eat if close enough
        ai.TryEatFood(food);
    }
}