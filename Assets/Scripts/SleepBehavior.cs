using UnityEngine;

[CreateAssetMenu(menuName = "Creatures/SleepBehavior", fileName = "SleepBehaviorSO")]
public class SleepBehavior : BehaviorSO
{
    public int sleepPriority = 80;
    public int sleepTime = 16;
    public override int Score(AIController ctrl)
    {
        if (Utilities.timeOfDay > sleepTime)
        {
            return sleepPriority;
        }
        else
        {
            return 0;
        }
    }

    public override void OnPriority(AIController ctrl)
    {
        ctrl.movement.Stop();
    }

}