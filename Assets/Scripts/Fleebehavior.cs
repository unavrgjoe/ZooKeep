using UnityEngine;

[CreateAssetMenu(menuName = "Creatures/FleeBehavior", fileName = "FleeBehaviorSO")]
public class FleeBehavior : BehaviorSO
{
    //public int skittish = 0; //use for extra fearless or scared creatures
    //for entity in visible layers
    public override int Score(AIController ctrl)
    {
        int my = ctrl.tier;
        Entity threat = null;
        var pos = (Vector2)ctrl.transform.position;
        float d2 = 100000.0f;
        foreach (var e in ctrl.visibleEntities)
        {
            if (!e || e == ctrl.entity) continue;
            if (e.tier <= my - skittish) continue; // only higher-tier = threat

            float dd = ((Vector2)e.transform.position - pos).sqrMagnitude;
            if (dd < d2) { d2 = dd; threat = e; }
        }
        if (threat != null)
        {
            ctrl.predator = threat;
            return (threat.tier - my) * 10 + basePriority + (int)((Vector2)threat.transform.position - pos).sqrMagnitude;
        }
        else { return 0; }
    }

    public override void OnPriority(AIController ctrl)
    {
        // Direction AWAY from threat (unit vector):
        Vector2 awayDir = ((Vector2)ctrl.transform.position - (Vector2)ctrl.predator.transform.position).normalized; /*self - threat for oppsoite of threat*/
        ctrl.movement.SetMove(awayDir);
    }
}