# 📘 GOAP System Documentation - README

Welcome to the comprehensive documentation for your Unity GOAP (Goal-Oriented Action Planning) AI system!

---

## 📖 Documentation Overview

This documentation package contains **four detailed documents** designed to help you understand, use, and extend the GOAP system:

### 1. 📊 **GOAP_SYSTEM_ANALYSIS.md** (Main Document)
**Best for**: Deep understanding of how everything works

**Contents**:
- Executive summary of what GOAP is and why it's better than FSMs
- Detailed architecture overview with diagrams
- Complete breakdown of every class (Beliefs, Actions, Goals, Planner, etc.)
- How classes connect and communicate
- The planning algorithm explained step-by-step
- Practical scenarios with detailed walkthroughs
- Code flow diagrams
- Extension guides

**Read this if**: You want to truly understand how GOAP works under the hood, or you're new to GOAP systems.

---

### 2. 🎨 **GOAP_VISUAL_GUIDE.md**
**Best for**: Visual learners who prefer diagrams and flowcharts

**Contents**:
- Visual architecture diagrams
- Data flow illustrations
- Planning tree examples
- Action anatomy breakdowns
- Goal priority visualizations
- Belief system structure charts
- Strategy execution timelines
- Complete scenario walkthroughs with step-by-step visuals
- Performance analysis charts

**Read this if**: You learn better with visuals, or you want to see how data flows through the system.

---

### 3. 🚀 **GOAP_QUICK_REFERENCE.md** (Cheat Sheet)
**Best for**: Quick lookups and copy-paste templates

**Contents**:
- Setup checklists
- Code templates for every common task
- Common design patterns (combat, inventory, patrol, etc.)
- Debugging guide with troubleshooting table
- Performance optimization tips
- Testing scenarios
- Best practices (DOs and DON'Ts)
- Advanced techniques
- Common extensions

**Read this if**: You already understand GOAP and just need quick reference code snippets.

---

### 4. 📘 **README.md** (This File)
**Best for**: Getting started and finding what you need

**Contents**:
- Documentation overview (you are here!)
- Getting started guide
- Quick start tutorial
- Learning path recommendations
- FAQ

---

## 🚀 Getting Started

### For Complete Beginners to GOAP

**Recommended Reading Order**:

1. **Start here**: Read the "What is GOAP?" section in `GOAP_SYSTEM_ANALYSIS.md`
2. **Get visual**: Look at the architecture diagrams in `GOAP_VISUAL_GUIDE.md`
3. **Follow the example**: Read "Scenario 1: Simple Idle Behavior" in `GOAP_SYSTEM_ANALYSIS.md`
4. **Try it yourself**: Use templates from `GOAP_QUICK_REFERENCE.md` to add a new action
5. **Deep dive**: Read the planning algorithm explanation in `GOAP_SYSTEM_ANALYSIS.md`

**Time investment**: ~2-3 hours for full understanding

---

### For Developers Familiar with AI Systems

**Recommended Reading Order**:

1. **Architecture overview**: Skim `GOAP_SYSTEM_ANALYSIS.md` → "System Architecture Overview"
2. **Understand planning**: Read `GOAP_SYSTEM_ANALYSIS.md` → "The Planning Algorithm Explained"
3. **See it in action**: Review `GOAP_VISUAL_GUIDE.md` → "Complete Scenario: Combat Encounter"
4. **Get coding**: Use `GOAP_QUICK_REFERENCE.md` for templates and patterns

**Time investment**: ~30-45 minutes to get productive

---

### For Quick Implementation

**Recommended Path**:

1. **Checklist**: Follow `GOAP_QUICK_REFERENCE.md` → "Quick Setup Checklist"
2. **Templates**: Copy code from `GOAP_QUICK_REFERENCE.md` → "Code Templates"
3. **Patterns**: Use pre-built patterns from `GOAP_QUICK_REFERENCE.md` → "Common Patterns"
4. **Debug**: Reference `GOAP_QUICK_REFERENCE.md` → "Debugging Guide"

**Time investment**: ~15 minutes to add your first behavior

---

## 🎓 Quick Start Tutorial

### Tutorial 1: Making the NPC Go to a Location When Health is Low

**What you'll learn**: Basic Belief, Action, and Goal creation

**Steps**:

1. **Add a Transform in Unity**:
   - Create an empty GameObject in your scene called "Hospital"
   - Position it where you want the NPC to go
   - Assign it to a public field in `GoapAgent`:
     ```csharp
     [SerializeField] Transform hospital;
     ```

2. **Create Beliefs** (in `SetupBeliefs()`):
   ```csharp
   factory.AddBelief("HealthLow", () => health < 30);
   factory.AddLocationBelief("AtHospital", 3f, hospital);
   ```

3. **Create Actions** (in `SetupActions()`):
   ```csharp
   actions.Add(new AgentAction.Builder("Go To Hospital")
       .WithCost(2)
       .WithStrategy(new NavigateStrategy(navMeshAgent, hospital.position))
       .AddEffect(beliefs["AtHospital"])
       .Build());
   
   actions.Add(new AgentAction.Builder("Get Treatment")
       .WithCost(1)
       .WithStrategy(new IdleStrategy(5f))
       .AddPrecondition(beliefs["AtHospital"])
       .AddEffect(beliefs["HealthLow"])
       .Build());
   ```

4. **Create Goal** (in `SetupGoals()`):
   ```csharp
   goals.Add(new AgentGoal.Builder("Stay Healthy")
       .WithPriority(9)  // High priority!
       .WithDesiredEffects(beliefs["HealthLow"])
       .Build());
   ```

5. **Test**:
   - Run the game
   - In Inspector, manually set `health` to 20
   - Watch the NPC plan and execute: [Go To Hospital] → [Get Treatment]

**Expected behavior**: When health drops below 30, NPC stops whatever it's doing and goes to the hospital!

---

### Tutorial 2: Adding Enemy Detection and Combat

**What you'll learn**: Sensor integration and multi-action plans

**Steps**:

1. **Setup Sensor**:
   - Already exists in your project: `attackSensor`
   - Make sure it has a tag filter for "Enemy"

2. **Add Combat Beliefs** (in `SetupBeliefs()`):
   ```csharp
   factory.AddSensorBelief("EnemyNearby", attackSensor);
   factory.AddBelief("HasWeapon", () => currentWeapon != null);
   ```

3. **Add Combat Actions** (in `SetupActions()`):
   ```csharp
   actions.Add(new AgentAction.Builder("Equip Weapon")
       .WithCost(1)
       .WithStrategy(new IdleStrategy(1f))  // Simulate equipping
       .AddEffect(beliefs["HasWeapon"])
       .Build());
   
   actions.Add(new AgentAction.Builder("Attack Enemy")
       .WithCost(1)
       .WithStrategy(new AttackStrategy(attackSensor, animations))
       .AddPrecondition(beliefs["EnemyNearby"])
       .AddPrecondition(beliefs["HasWeapon"])
       .AddEffect(beliefs["EnemyNearby"])
       .Build());
   ```

4. **Add Combat Goal** (in `SetupGoals()`):
   ```csharp
   goals.Add(new AgentGoal.Builder("Defend Self")
       .WithPriority(8)
       .WithDesiredEffects(beliefs["EnemyNearby"])
       .Build());
   ```

5. **Test**:
   - Place an enemy GameObject with tag "Enemy" near the NPC
   - Watch the NPC: [Equip Weapon] → [Attack Enemy]

**Expected behavior**: When enemy enters sensor range, NPC automatically equips weapon and attacks!

---

## 🎯 Common Use Cases

### Use Case 1: NPC Daily Routine

**Scenario**: NPC should work during the day, sleep at night

**Solution**: Time-based beliefs + priority goals
- See `GOAP_QUICK_REFERENCE.md` → "Pattern 4: Patrol & Investigation"
- Add `TimeOfDayBelief`
- Create "Work" and "Sleep" goals with time preconditions

---

### Use Case 2: Resource Gathering AI

**Scenario**: NPC should gather materials to craft items

**Solution**: Inventory beliefs + crafting actions
- See `GOAP_QUICK_REFERENCE.md` → "Pattern 3: Inventory & Crafting"
- Complete example provided with wood, stone, and tools

---

### Use Case 3: Guard Behavior

**Scenario**: Guard patrols until hearing noise, then investigates

**Solution**: Sensor + patrol pattern
- See `GOAP_QUICK_REFERENCE.md` → "Pattern 4: Patrol & Investigation"
- Uses noise detection sensor
- Auto-returns to patrol after investigating

---

### Use Case 4: Survival AI

**Scenario**: NPC manages hunger, thirst, health, and threats

**Solution**: Multiple stat beliefs + priority system
- See `GOAP_SYSTEM_ANALYSIS.md` → "Scenario 2: Health/Stamina Management"
- See `GOAP_QUICK_REFERENCE.md` → "Pattern 1: State Management"
- Complete multi-stat example

---

## ❓ FAQ

### Q: Why isn't my NPC doing anything?

**A**: Common causes:
1. No beliefs are false (all goals satisfied, check their values)
2. No valid plan exists (preconditions can't be met)
3. NavMesh not baked (navigation fails)
4. Missing strategy assignment

**Debug**: Add this to your `Update()`:
```csharp
if (actionPlan == null) {
    Debug.LogWarning("No plan found!");
    DebugPrintBeliefs();
}
```

---

### Q: How do I make an NPC interrupt their current action?

**A**: Set the interrupting goal's priority HIGHER than the current goal's priority. The system automatically handles this.

Example:
```csharp
goals.Add(new AgentGoal.Builder("Casual Wandering")
    .WithPriority(1)  // Low priority
    .Build());

goals.Add(new AgentGoal.Builder("DANGER!")
    .WithPriority(10)  // High priority - will interrupt wandering
    .Build());
```

---

### Q: Can multiple NPCs share the same GOAP code?

**A**: Yes! Each NPC has its own `GoapAgent` component with its own beliefs/actions/goals. You can:
- Create different "profiles" (CombatNPC, CivilianNPC, GuardNPC)
- Override `SetupBeliefs/Actions/Goals()` in derived classes
- Share common actions via static methods

---

### Q: How do I debug why a plan isn't working?

**A**: Use the debug helper from `GOAP_QUICK_REFERENCE.md`:
```csharp
void DebugPrintState() {
    Debug.Log($"Current Goal: {currentGoal?.Name}");
    Debug.Log($"Current Action: {currentAction?.Name}");
    
    foreach (var belief in beliefs) {
        Debug.Log($"{belief.Key}: {belief.Value.Evaluate()}");
    }
}
```

Call it with `Input.GetKeyDown(KeyCode.D)` while playing.

---

### Q: How many actions/goals can I have before performance suffers?

**A**: Rule of thumb:
- **Safe**: < 20 actions, < 10 goals
- **Warning**: 20-30 actions, 10-15 goals (use profiler)
- **Danger**: > 30 actions, > 15 goals (likely lag)

Planning complexity is O(actions^depth × goals), so it grows fast!

---

### Q: Can I make actions that take a random amount of time?

**A**: Yes! Use timers in your strategy:
```csharp
public class RandomIdleStrategy : IActionStrategy {
    CountdownTimer timer;
    
    public RandomIdleStrategy(float minTime, float maxTime) {
        float randomTime = Random.Range(minTime, maxTime);
        timer = new CountdownTimer(randomTime);
    }
    
    // ... rest of implementation
}
```

---

### Q: How do I make an NPC "remember" something?

**A**: Use a persistent belief:
```csharp
private bool hasSeenPlayer = false;

factory.AddBelief("RemembersPlayer", () => hasSeenPlayer);

// In sensor callback:
void OnPlayerDetected() {
    hasSeenPlayer = true;  // Persists even after player leaves
}
```

---

### Q: Can actions fail?

**A**: Yes! Implement failure in your strategy:
```csharp
public class FallibleStrategy : IActionStrategy {
    public bool Complete { get; private set; }
    public bool Failed { get; private set; }
    
    public void Update(float dt) {
        if (SomeFailureCondition()) {
            Failed = true;
            Complete = true;  // Mark as "complete" to move on
        }
    }
}
```

Then in `GoapAgent.Update()`, check `Failed` and force replanning.

---

## 🔧 Troubleshooting

### Issue: NPC walks to location but doesn't perform action

**Cause**: Belief not updating when NPC arrives

**Fix**: Check your distance threshold:
```csharp
// Too strict (may never be exactly at position):
factory.AddLocationBelief("AtLocation", 0.1f, target);

// Better:
factory.AddLocationBelief("AtLocation", 2f, target);
```

---

### Issue: Planning is slow (frame drops)

**Cause**: Too many actions or deep recursion

**Fixes**:
1. Reduce action count
2. Increase action costs (limits depth)
3. Add early exit conditions
4. Cache belief evaluations

See `GOAP_QUICK_REFERENCE.md` → "Performance Optimization"

---

### Issue: NPC switches goals rapidly (flip-flopping)

**Cause**: Multiple goals with same priority or tight thresholds

**Fix**: 
1. Separate goal priorities more (use gaps: 1, 3, 5, 7, 10)
2. Add hysteresis to beliefs:
   ```csharp
   // Instead of exact threshold:
   factory.AddBelief("HealthLow", () => health < 30);
   
   // Use hysteresis:
   bool wasHealthLow = false;
   factory.AddBelief("HealthLow", () => {
       if (health < 25) wasHealthLow = true;
       else if (health > 40) wasHealthLow = false;
       return wasHealthLow;
   });
   ```

---

## 🌟 Next Steps

### Level 1: Beginner
- [ ] Read `GOAP_SYSTEM_ANALYSIS.md` "What is GOAP?"
- [ ] Complete Tutorial 1 (Health System)
- [ ] Add one custom action using templates
- [ ] Test and observe NPC behavior

### Level 2: Intermediate
- [ ] Complete Tutorial 2 (Combat)
- [ ] Implement a 3-stat system (health, stamina, hunger)
- [ ] Create a patrol+investigate behavior
- [ ] Add custom strategy class

### Level 3: Advanced
- [ ] Build a complete NPC daily routine (work, eat, sleep)
- [ ] Implement crafting system with multiple item types
- [ ] Create cooperative multi-agent behaviors
- [ ] Optimize performance for 10+ agents

---

## 📊 System Capabilities

### ✅ Currently Implemented
- ✅ Backward-chaining planner
- ✅ Belief system with conditions and locations
- ✅ Action builder pattern
- ✅ Goal priority system
- ✅ Sensor integration
- ✅ NavMesh integration
- ✅ Animation system hooks
- ✅ Timer utilities
- ✅ Event-driven replanning

### 🚧 Potential Extensions (Not Implemented)
- ⬜ A* optimization for planning
- ⬜ Plan caching (avoid replanning identical situations)
- ⬜ Multi-agent coordination
- ⬜ Learning system (adjust costs based on success)
- ⬜ Visual Debugger UI
- ⬜ Dialogue system integration
- ⬜ Hierarchical GOAP (goals can be actions in higher-level plans)

---

## 📚 Additional Resources

### In This Documentation Package
- **Main Analysis**: `GOAP_SYSTEM_ANALYSIS.md`
- **Visual Guide**: `GOAP_VISUAL_GUIDE.md`
- **Quick Reference**: `GOAP_QUICK_REFERENCE.md`
- **This File**: `README.md`

### External Resources
- [Jeff Orkin's GOAP Paper](http://alumni.media.mit.edu/~jorkin/goap.html) - Original F.E.A.R. implementation
- [Unity NavMesh Documentation](https://docs.unity3d.com/Manual/Navigation.html)
- [Red Blob Games: A* Pathfinding](https://www.redblobgames.com/pathfinding/a-star/introduction.html)
- [Game AI Pro Book Series](http://www.gameaipro.com/)

### Video Tutorials
- Search YouTube for "Unity GOAP Tutorial"
- Example: "Goal Oriented Action Planning in Unity"

---

## 🎉 You're Ready!

You now have everything you need to:
- ✅ **Understand** how GOAP works
- ✅ **Implement** new behaviors quickly
- ✅ **Debug** issues when they arise
- ✅ **Optimize** performance
- ✅ **Extend** the system for your game

**Start with the tutorials above and experiment!** The best way to learn GOAP is to see it in action.

---

## 💡 Remember

> **"GOAP is about emergent behavior. The magic happens when simple actions combine in unexpected ways to solve complex problems!"**

The beauty of GOAP is that you don't program every possible scenario - you define building blocks (beliefs, actions, goals) and let the planner figure out the best path. This creates AI that feels intelligent and adaptive.

**Happy coding! 🚀**

---

*For questions, issues, or contributions, refer to the main project documentation or contact the development team.*

*Last updated: December 2025*
