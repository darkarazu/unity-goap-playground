# 🚀 GOAP Quick Reference & Cheat Sheet

Quick reference guide for implementing and extending the GOAP system.

---

## 📋 Quick Setup Checklist

### Setting Up a New GOAP Agent

- [ ] Create GameObject with NavMeshAgent component
- [ ] Add AnimationController component (or derived class)
- [ ] Add GoapAgent component
- [ ] Add Sensor components (if needed)
- [ ] Assign references in Inspector:
  - [ ] Chase Sensor
  - [ ] Attack Sensor
  - [ ] Resting Position
  - [ ] Food Shack
  - [ ] Door Position
- [ ] Verify NavMesh is baked in scene
- [ ] Test in Play mode

---

## 🔧 Code Templates

### Template 1: Adding a Simple Belief

```csharp
// In GoapAgent.SetupBeliefs()
factory.AddBelief("BeliefName", () => /* condition */);

// Examples:
factory.AddBelief("IsTired", () => stamina < 20);
factory.AddBelief("HasItem", () => inventory.Contains("Key"));
factory.AddBelief("IsNight", () => TimeOfDay.Hour > 18);
```

### Template 2: Adding a Location Belief

```csharp
// With Transform
factory.AddLocationBelief("AtLocation", distance, transform);

// With Vector3
factory.AddLocationBelief("AtLocation", distance, new Vector3(x, y, z));

// Examples:
factory.AddLocationBelief("AtHome", 3f, homePosition);
factory.AddLocationBelief("AtShop", 2f, shopTransform);
```

### Template 3: Adding a Sensor Belief

```csharp
// First, add Sensor component to your GameObject
[SerializeField] Sensor mySensor;

// In SetupBeliefs()
factory.AddSensorBelief("TargetDetected", mySensor);

// This automatically creates:
// - Condition: () => mySensor.IsTargetInRange
// - Location: () => mySensor.TargetPosition
```

### Template 4: Adding an Action (Simple)

```csharp
actions.Add(new AgentAction.Builder("ActionName")
    .WithCost(1)  // Optional, default is 1
    .WithStrategy(new IdleStrategy(duration))
    .AddPrecondition(beliefs["Precondition"])  // Optional
    .AddEffect(beliefs["Effect"])
    .Build()
);

// Example: Resting action
actions.Add(new AgentAction.Builder("Rest")
    .WithCost(1)
    .WithStrategy(new IdleStrategy(5f))
    .AddPrecondition(beliefs["AtHome"])
    .AddEffect(beliefs["IsTired"])  // Makes IsTired FALSE
    .Build()
);
```

### Template 5: Adding an Action (Navigation)

```csharp
actions.Add(new AgentAction.Builder("GoToLocation")
    .WithCost(2)  // Navigation typically costs more
    .WithStrategy(new NavigateStrategy(navMeshAgent, targetPosition))
    .AddPrecondition(beliefs["AgentIdle"])
    .AddEffect(beliefs["AtLocation"])
    .Build()
);
```

### Template 6: Adding an Action (Complex)

```csharp
actions.Add(new AgentAction.Builder("CraftItem")
    .WithCost(3)
    .WithStrategy(new CraftingStrategy(itemName, craftTime))
    .AddPrecondition(beliefs["HasMaterials"])
    .AddPrecondition(beliefs["AtWorkbench"])
    .AddPrecondition(beliefs["HasRecipe"])
    .AddEffect(beliefs["HasItem"])
    .Build()
);
```

### Template 7: Adding a Goal

```csharp
goals.Add(new AgentGoal.Builder("GoalName")
    .WithPriority(priority)  // 1-10, higher = more important
    .WithDesiredEffects(beliefs["Effect"])
    .Build()
);

// Example: Survival goal
goals.Add(new AgentGoal.Builder("Stay Alive")
    .WithPriority(10)  // CRITICAL
    .WithDesiredEffects(beliefs["HealthLow"])  // Want this FALSE
    .Build()
);

// Multiple desired effects:
var goal = new AgentGoal.Builder("Be Ready For Combat");
goal.WithPriority(7);
goal.WithDesiredEffects(beliefs["HasWeapon"]);
goal.WithDesiredEffects(beliefs["HasArmor"]);
goal.WithDesiredEffects(beliefs["HealthHigh"]);
goals.Add(goal.Build());
```

### Template 8: Creating a Custom Strategy

```csharp
using UnityEngine;

public class MyCustomStrategy : IActionStrategy {
    // Required properties
    public bool CanPerform { get; private set; }
    public bool Complete { get; private set; }
    
    // Your custom fields
    private float duration;
    private float elapsedTime;
    
    public MyCustomStrategy(/* parameters */) {
        // Initialize
        this.duration = duration;
        CanPerform = true;
        Complete = false;
    }
    
    public void Start() {
        // Called when action starts
        elapsedTime = 0f;
        Complete = false;
        
        // Your initialization code here
        Debug.Log("Strategy started");
    }
    
    public void Update(float deltaTime) {
        // Called every frame while action is active
        elapsedTime += deltaTime;
        
        // Your execution code here
        
        // Check completion condition
        if (elapsedTime >= duration) {
            Complete = true;
        }
    }
    
    public void Stop() {
        // Called when action completes or is interrupted
        // Cleanup code here
        Debug.Log("Strategy stopped");
    }
}
```

### Template 9: Creating a Timer Callback

```csharp
// Countdown Timer
CountdownTimer timer = new CountdownTimer(5f);
timer.OnTimerStart += () => {
    Debug.Log("Timer started!");
};
timer.OnTimerStop += () => {
    Debug.Log("Timer finished!");
};
timer.Start();

// In Update:
timer.Tick(Time.deltaTime);

// Stopwatch Timer
StopwatchTimer stopwatch = new StopwatchTimer();
stopwatch.Start();
// Later:
float elapsed = stopwatch.GetTime();
```

---

## 🎯 Common Patterns

### Pattern 1: State Management (Health/Stamina/Energy)

```csharp
// BELIEFS
factory.AddBelief("HealthLow", () => health < 30);
factory.AddBelief("StaminaLow", () => stamina < 30);
factory.AddLocationBelief("AtRestPoint", 3f, restPoint);
factory.AddLocationBelief("AtFoodPoint", 3f, foodPoint);

// ACTIONS
actions.Add(new AgentAction.Builder("Go Rest")
    .WithCost(2)
    .WithStrategy(new NavigateStrategy(navMeshAgent, restPoint.position))
    .AddEffect(beliefs["AtRestPoint"])
    .Build());

actions.Add(new AgentAction.Builder("Sleep")
    .WithCost(1)
    .WithStrategy(new IdleStrategy(5f))
    .AddPrecondition(beliefs["AtRestPoint"])
    .AddEffect(beliefs["StaminaLow"])
    .Build());

actions.Add(new AgentAction.Builder("Go Eat")
    .WithCost(2)
    .WithStrategy(new NavigateStrategy(navMeshAgent, foodPoint.position))
    .AddEffect(beliefs["AtFoodPoint"])
    .Build());

actions.Add(new AgentAction.Builder("Eat")
    .WithCost(1)
    .WithStrategy(new IdleStrategy(3f))
    .AddPrecondition(beliefs["AtFoodPoint"])
    .AddEffect(beliefs["HealthLow"])
    .Build());

// GOALS
goals.Add(new AgentGoal.Builder("Maintain Health")
    .WithPriority(9)
    .WithDesiredEffects(beliefs["HealthLow"])
    .Build());

goals.Add(new AgentGoal.Builder("Maintain Stamina")
    .WithPriority(8)
    .WithDesiredEffects(beliefs["StaminaLow"])
    .Build());
```

### Pattern 2: Combat System

```csharp
// BELIEFS
factory.AddSensorBelief("EnemyInAttackRange", attackSensor);
factory.AddSensorBelief("EnemyInChaseRange", chaseSensor);
factory.AddBelief("HasWeapon", () => currentWeapon != null);
factory.AddBelief("HasAmmo", () => currentAmmo > 0);

// ACTIONS
actions.Add(new AgentAction.Builder("Pick Up Weapon")
    .WithCost(3)
    .WithStrategy(new PickupStrategy(weaponPrefab))
    .AddEffect(beliefs["HasWeapon"])
    .Build());

actions.Add(new AgentAction.Builder("Reload")
    .WithCost(2)
    .WithStrategy(new IdleStrategy(2f))
    .AddPrecondition(beliefs["HasWeapon"])
    .AddEffect(beliefs["HasAmmo"])
    .Build());

actions.Add(new AgentAction.Builder("Shoot")
    .WithCost(1)
    .WithStrategy(new AttackStrategy(attackSensor))
    .AddPrecondition(beliefs["EnemyInAttackRange"])
    .AddPrecondition(beliefs["HasWeapon"])
    .AddPrecondition(beliefs["HasAmmo"])
    .AddEffect(beliefs["EnemyInAttackRange"])  // Enemy dies
    .Build());

// GOALS
goals.Add(new AgentGoal.Builder("Eliminate Threats")
    .WithPriority(8)
    .WithDesiredEffects(beliefs["EnemyInAttackRange"])
    .Build());
```

### Pattern 3: Inventory & Crafting

```csharp
// BELIEFS
factory.AddBelief("HasWood", () => inventory.Count("Wood") >= 1);
factory.AddBelief("HasStone", () => inventory.Count("Stone") >= 1);
factory.AddBelief("HasTool", () => inventory.Has("Axe"));
factory.AddLocationBelief("AtWorkbench", 2f, workbench);
factory.AddLocationBelief("AtTreeLocation", 5f, forest);

// ACTIONS
actions.Add(new AgentAction.Builder("Gather Wood")
    .WithCost(4)
    .WithStrategy(new GatheringStrategy("Wood", 5f))
    .AddPrecondition(beliefs["AtTreeLocation"])
    .AddEffect(beliefs["HasWood"])
    .Build());

actions.Add(new AgentAction.Builder("Go To Forest")
    .WithCost(3)
    .WithStrategy(new NavigateStrategy(navMeshAgent, forest.position))
    .AddEffect(beliefs["AtTreeLocation"])
    .Build());

actions.Add(new AgentAction.Builder("Craft Tool")
    .WithCost(2)
    .WithStrategy(new CraftStrategy("Axe", 3f))
    .AddPrecondition(beliefs["AtWorkbench"])
    .AddPrecondition(beliefs["HasWood"])
    .AddPrecondition(beliefs["HasStone"])
    .AddEffect(beliefs["HasTool"])
    .Build());

// GOALS
goals.Add(new AgentGoal.Builder("Be Equipped")
    .WithPriority(6)
    .WithDesiredEffects(beliefs["HasTool"])
    .Build());
```

### Pattern 4: Patrol & Investigation

```csharp
// BELIEFS
factory.AddBelief("HeardNoise", () => noiseDetector.NoiseDetected);
factory.AddBelief("IsPatrolling", () => navMeshAgent.hasPath);
factory.AddLocationBelief("AtNoiseSource", 2f, () => noiseDetector.NoisePosition);

// ACTIONS
actions.Add(new AgentAction.Builder("Patrol")
    .WithCost(1)
    .WithStrategy(new PatrolStrategy(navMeshAgent, patrolPoints))
    .AddEffect(beliefs["IsPatrolling"])
    .Build());

actions.Add(new AgentAction.Builder("Investigate Noise")
    .WithCost(2)
    .WithStrategy(new NavigateStrategy(navMeshAgent, () => noiseDetector.NoisePosition))
    .AddPrecondition(beliefs["HeardNoise"])
    .AddEffect(beliefs["AtNoiseSource"])
    .Build());

actions.Add(new AgentAction.Builder("Search Area")
    .WithCost(1)
    .WithStrategy(new SearchStrategy(3f))
    .AddPrecondition(beliefs["AtNoiseSource"])
    .AddEffect(beliefs["HeardNoise"])  // Clears the noise
    .Build());

// GOALS
goals.Add(new AgentGoal.Builder("Investigate")
    .WithPriority(7)
    .WithDesiredEffects(beliefs["HeardNoise"])
    .Build());

goals.Add(new AgentGoal.Builder("Patrol Area")
    .WithPriority(3)
    .WithDesiredEffects(beliefs["IsPatrolling"])
    .Build());
```

---

## 🐛 Debugging Guide

### Debug Logging Template

```csharp
// Add to GoapAgent.Update() for detailed logging:
void Update() {
    if (Input.GetKeyDown(KeyCode.D)) {
        DebugPrintState();
    }
    
    // ... rest of Update()
}

void DebugPrintState() {
    Debug.Log("=== GOAP STATE ===");
    Debug.Log($"Current Goal: {currentGoal?.Name ?? "None"}");
    Debug.Log($"Current Action: {currentAction?.Name ?? "None"}");
    Debug.Log($"Actions in Plan: {actionPlan?.Actions.Count ?? 0}");
    
    Debug.Log("\n=== BELIEFS ===");
    foreach (var belief in beliefs) {
        Debug.Log($"{belief.Key}: {belief.Value.Evaluate()}");
    }
    
    Debug.Log("\n=== GOALS ===");
    foreach (var goal in goals.OrderByDescending(g => g.Priority)) {
        bool satisfied = goal.DesiredEffects.All(e => e.Evaluate() == false);
        Debug.Log($"[{goal.Priority}] {goal.Name}: {(satisfied ? "SATISFIED" : "NEEDS PLAN")}");
    }
}
```

### Common Issues & Solutions

| Issue | Possible Cause | Solution |
|-------|---------------|----------|
| **Agent does nothing** | No valid plan found | Check preconditions are achievable |
| **Agent stuck in loop** | Effect not updating belief | Verify belief evaluation logic |
| **Plan never completes** | Strategy.Complete always false | Check strategy completion condition |
| **Unexpected behavior** | Goal priority wrong | Review priority values |
| **Performance issues** | Too many actions/goals | Reduce complexity, profile planner |
| **NavMesh errors** | NavMesh not baked | Bake NavMesh in Unity |
| **Sensor not detecting** | Wrong tag or layer | Check tag/layer settings |

### Profiling Tips

```csharp
// Add timing to CalculatePlan()
void CalculatePlan() {
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // ... planning code ...
    
    stopwatch.Stop();
    if (stopwatch.ElapsedMilliseconds > 5) {
        Debug.LogWarning($"Planning took {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

---

## ⚡ Performance Optimization

### Optimization Checklist

- [ ] **Limit Planner Calls**: Only replan when necessary (currentAction == null)
- [ ] **Cache Belief Evaluations**: Don't call expensive functions multiple times per frame
- [ ] **Reduce Action Count**: Keep under 20 actions per agent
- [ ] **Reduce Goal Count**: Keep under 10 goals per agent
- [ ] **Early Exit**: Return first valid plan (don't search for optimal)
- [ ] **Use Timers**: Don't update stats/sensors every frame
- [ ] **Disable Debug Logs**: In builds, use conditional compilation

### Caching Beliefs Example

```csharp
// BEFORE (calls expensive function multiple times):
factory.AddBelief("EnemyNearby", () => FindClosestEnemy() != null);

// AFTER (cache and update periodically):
private GameObject cachedEnemy;

void Start() {
    InvokeRepeating(nameof(UpdateCache), 0f, 0.5f);  // Update every 0.5s
}

void UpdateCache() {
    cachedEnemy = FindClosestEnemy();
}

// In SetupBeliefs():
factory.AddBelief("EnemyNearby", () => cachedEnemy != null);
```

### Conditional Debug Logging

```csharp
#if UNITY_EDITOR
    Debug.Log($"Planning for goal: {goal.Name}");
#endif
```

---

## 📊 Testing Scenarios

### Test Case Template

```csharp
// Scenario: Agent should eat when hungry
// 
// Given:
//   - health = 20 (< 30, so HealthLow = true)
//   - Agent is idle
//   - Food shack exists at (10, 0, 5)
// 
// Expected Plan:
//   [Go To Food Shack, Eat]
// 
// Expected Behavior:
//   1. Agent navigates to food shack
//   2. Agent idles for 3 seconds (eating)
//   3. Health increases above 30
//   4. Agent resumes wandering
```

### Manual Testing Checklist

- [ ] **Idle Behavior**: Does agent wander when nothing to do?
- [ ] **Priority Override**: Does high-priority goal interrupt low-priority action?
- [ ] **Sensor Reactivity**: Does agent react when player enters range?
- [ ] **Plan Completion**: Does agent complete multi-action plans?
- [ ] **State Changes**: Do stats update correctly (health, stamina)?
- [ ] **Edge Cases**: What happens at extreme values (health=0, stamina=0)?
- [ ] **Navigation**: Does agent path correctly to all locations?
- [ ] **Interruption**: Does agent replan when target disappears?

---

## 🔐 Best Practices

### DO ✅

1. **Start Simple**: Begin with 2-3 actions and 2 goals
2. **Name Clearly**: Use descriptive names for beliefs/actions/goals
3. **Log Early**: Add debug logs during development
4. **Test Incrementally**: Add one feature at a time
5. **Use Costs Wisely**: Reflect real effort/danger
6. **Update Beliefs**: Keep world state current
7. **Handle Nulls**: Check for null sensors/transforms
8. **Use Timers**: For periodic updates, not every frame

### DON'T ❌

1. **Don't Overcomplicate**: Start with simple behaviors
2. **Don't Circular Depend**: Action A needs B, B needs A
3. **Don't Hardcode**: Use serialized fields for tuneable values
4. **Don't Forget Cleanup**: Stop() should clean up resources
5. **Don't Replan Every Frame**: Only when action == null
6. **Don't Use Too Many Actions**: Performance degrades
7. **Don't Ignore Profiler**: Check frame time!
8. **Don't Skip Documentation**: Comment complex logic

---

## 🎓 Advanced Techniques

### Technique 1: Dynamic Costs

```csharp
// Action cost changes based on world state
actions.Add(new AgentAction.Builder("Take Risky Shortcut")
    .WithCost(CalculateRiskCost())  // Dynamic!
    .Build());

float CalculateRiskCost() {
    // Higher cost when hurt (agent will avoid)
    return health < 50 ? 10f : 2f;
}
```

### Technique 2: Conditional Effects

```csharp
public class ConditionalEffect : AgentBelief {
    private Func<bool> additionalCondition;
    
    public ConditionalEffect(string name, Func<bool> condition, Func<bool> additional) {
        this.Name = name;
        this.condition = condition;
        this.additionalCondition = additional;
    }
    
    public override bool Evaluate() {
        return condition() && additionalCondition();
    }
}
```

### Technique 3: Shared Beliefs (Multiple Agents)

```csharp
// Static belief system for shared world knowledge
public class WorldBeliefs : MonoBehaviour {
    public static WorldBeliefs Instance;
    
    public bool IsDay => TimeOfDay.Hour < 18;
    public bool IsDangerous => ThreatLevel > 5;
    
    void Awake() {
        Instance = this;
    }
}

// In agent beliefs:
factory.AddBelief("WorldIsDay", () => WorldBeliefs.Instance.IsDay);
```

### Technique 4: Action Cooldowns

```csharp
public class CooldownStrategy : IActionStrategy {
    private float cooldownTime;
    private float lastUsedTime = -999f;
    
    public bool CanPerform => Time.time - lastUsedTime > cooldownTime;
    public bool Complete { get; private set; }
    
    public void Start() {
        lastUsedTime = Time.time;
        Complete = true;
    }
}
```

---

## 📚 Common Extensions

### Extension 1: Save/Load System

```csharp
[System.Serializable]
public class GoapSaveData {
    public float health;
    public float stamina;
    public string currentGoalName;
    public string currentActionName;
}

public GoapSaveData Save() {
    return new GoapSaveData {
        health = this.health,
        stamina = this.stamina,
        currentGoalName = currentGoal?.Name,
        currentActionName = currentAction?.Name
    };
}

public void Load(GoapSaveData data) {
    health = data.health;
    stamina = data.stamina;
    // Restore goal/action by name lookup...
}
```

### Extension 2: Visual Debugger

```csharp
void OnGUI() {
    if (!Application.isEditor) return;
    
    GUILayout.BeginArea(new Rect(10, 10, 300, 400));
    GUILayout.Label($"<b>Agent: {gameObject.name}</b>");
    GUILayout.Label($"Goal: {currentGoal?.Name ?? "None"}");
    GUILayout.Label($"Action: {currentAction?.Name ?? "None"}");
    GUILayout.Label($"Health: {health:F0}");
    GUILayout.Label($"Stamina: {stamina:F0}");
    
    if (actionPlan != null) {
        GUILayout.Label($"\nPlan ({actionPlan.Actions.Count} remaining):");
        foreach (var action in actionPlan.Actions) {
            GUILayout.Label($"  • {action.Name}");
        }
    }
    
    GUILayout.EndArea();
}
```

### Extension 3: Event System

```csharp
// Add to GoapAgent
public event Action<AgentGoal> OnGoalChanged;
public event Action<AgentAction> OnActionStarted;
public event Action<AgentAction> OnActionCompleted;

// Fire events:
currentGoal = actionPlan.AgentGoal;
OnGoalChanged?.Invoke(currentGoal);

currentAction.Start();
OnActionStarted?.Invoke(currentAction);

// In other scripts:
agent.OnActionStarted += (action) => {
    Debug.Log($"Agent started: {action.Name}");
};
```

---

## 🎯 Quick Wins (Easy Improvements)

1. **Add Health Regeneration**
   ```csharp
   factory.AddLocationBelief("AtMedicStation", 3f, medicStation);
   // Action: "Heal" at medic station
   ```

2. **Add Day/Night Cycle Behavior**
   ```csharp
   factory.AddBelief("IsNight", () => TimeManager.IsNight);
   // Goal: "Go Home At Night" with high priority
   ```

3. **Add Weapon Variety**
   ```csharp
   factory.AddBelief("HasMelee", () => meleeWeapon != null);
   factory.AddBelief("HasRanged", () => rangedWeapon != null);
   // Different attack actions for each weapon type
   ```

4. **Add Social Interactions**
   ```csharp
   factory.AddSensorBelief("FriendNearby", friendSensor);
   // Action: "Greet Friend" when nearby
   ```

5. **Add Environmental Hazards**
   ```csharp
   factory.AddBelief("InDanger", () => hazardDetector.IsInDangerZone);
   // Goal: "Escape Danger" with very high priority
   ```

---

## 🌟 Success Criteria

Your GOAP system is working well when:

- ✅ Agents make **logical decisions** without explicit state machine code
- ✅ Agents **adapt** to changing situations (player appears, health drops, etc.)
- ✅ Agents **complete multi-step plans** successfully
- ✅ Adding new actions **doesn't break** existing behavior
- ✅ Agents show **emergent behavior** (actions combining in unexpected ways)
- ✅ Performance is **acceptable** (< 5ms planning time)
- ✅ Behavior is **predictable** when debugging

---

## 📖 Further Reading

- **F.E.A.R. AI Postmortem**: Original GOAP implementation [(link)](http://alumni.media.mit.edu/~jorkin/goap.html)
- **Unity NavMesh Guide**: [(docs.unity3d.com)](https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html)
- **A* Pathfinding**: [(redblobgames.com)](https://www.redblobgames.com/pathfinding/a-star/introduction.html)
- **Game AI Pro**: Book series with advanced GOAP techniques
- **AI Behavior Toolkit**: [(github.com/adammyhre/Unity-Utils)](https://github.com/adammyhre/Unity-Utils)

---

**Happy Planning! 🎯**

Remember: GOAP is about **emergent behavior**. The magic happens when simple actions combine in unexpected ways to solve complex problems!
