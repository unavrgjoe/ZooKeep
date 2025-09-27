using UnityEngine;

[CreateAssetMenu(menuName = "Creatures/ChaseBehavior", fileName = "ChaseBehaviorSO")]
public class ChaseBehavior : BehaviorSO
{
    //for entity in visible layers
    //public int skittish = 0; //use for extra fearless or scared creatures now implemented in parent
    public override int Score(AIController ctrl)
    {
        int my = ctrl.tier; Entity prey = null; var pos = (Vector2)ctrl.transform.position; float d2 = 10000; //sorry for having a local prey separate from the one in AIController
        foreach (var e in ctrl.visibleEntities)
        {
            //Debug.Log("Entity tier: " + e.tier + "  My tier: " + my);
            if (!e || e == ctrl.entity) continue;
            if (e.tier >= my + skittish) continue; // only higher-tier = threat

            float dd = ((Vector2)e.transform.position - pos).sqrMagnitude;
            if (dd < d2) { d2 = dd; prey = e; }
        }
        float distanceWeight = 1;
        if (prey != null)
        {
            //Debug.Log("Total score is: " + prey.tier + " * 10 + " + basePriority + " + " + (int)(((Vector2)prey.transform.position - pos).sqrMagnitude * distanceWeight));
            ctrl.prey = prey;
            return prey.tier * 10 + basePriority + (int)(((Vector2)prey.transform.position - pos).sqrMagnitude * distanceWeight);
        }
        else { return 0; }

    }

    public override void OnPriority(AIController ctrl)
    {
        if (ctrl.prey != null)
        {
            //Debug.Log("Prey found");
            // Direction AWAY from threat (unit vector):
            Vector2 toPrey = ((Vector2)ctrl.prey.transform.position - (Vector2)ctrl.transform.position).normalized; /*self - threat for oppsoite of threat*/
            ctrl.movement.SetMove(toPrey);
            ctrl.UpdateCombat(ctrl.prey);
        }

    }


}