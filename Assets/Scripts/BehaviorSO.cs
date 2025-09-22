using UnityEngine;
using System.Collections.Generic;

public abstract class BehaviorSO : ScriptableObject
{
    public int basePriority = 50; //[Range(0, 100)] maybe 
    public virtual void OnPriority(AIController controller) { } //default of nothing, called when this behavior is highest priority and chose
    public virtual int Score(AIController controller) { return basePriority; }
    public int skittish = 0; //use for extra fearless or scared creatures



}


