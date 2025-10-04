TODO:

Slightly randomize the speed of enemies by a percentage, especially for hordes makes their movement a little more natural, make collision boxes on enemies slightly smaller than their sprites






Movement:
                                        InputManager
                                                ↓
                                        PlayerController
                                                ↓
AIController → BehaviorSO → Pathfinding → Movement2D
   ↓              ↓            ↓              ↓
Scores         Decides      Modifies       Executes
Behavior       Direction    for Obstacles  Movement