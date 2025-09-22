using UnityEngine;


//these are not the permananent values but the default values of a new creature, SO are read-only
[CreateAssetMenu(menuName = "Creatures/Creature", fileName = "NewCreature")]
public class Creature : ScriptableObject
{
    public int HP;
    public int speed;

    public float vision;

    public int tier; //used to determine predator/prey

    //public BehaviorSO[] behaviors;

    //public AttackSO[] attacks;



}