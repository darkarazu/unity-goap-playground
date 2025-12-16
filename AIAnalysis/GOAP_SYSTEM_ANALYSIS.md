# 🎯 GOAP System - Complete Analysis & Documentation

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [What is GOAP?](#what-is-goap)
3. [System Architecture Overview](#system-architecture-overview)
4. [Core Components Deep Dive](#core-components-deep-dive)
5. [How Classes Connect](#how-classes-connect)
6. [The Planning Algorithm Explained](#the-planning-algorithm-explained)
7. [Practical Examples & Scenarios](#practical-examples--scenarios)
8. [Code Flow Diagrams](#code-flow-diagrams)
9. [Extending the System](#extending-the-system)

---

## Executive Summary

This is a **Goal-Oriented Action Planning (GOAP)** AI system for Unity that enables NPCs to make intelligent, dynamic decisions. Unlike traditional Finite State Machines (FSMs) or Behavior Trees, GOAP agents:

- ✅ **Think for themselves** - They create their own plans to achieve goals
- ✅ **Adapt dynamically** - They replan when situations change
- ✅ **Scale easily** - New actions/goals can be added without breaking existing code
- ✅ **Create emergent behavior** - Complex behaviors emerge from simple action combinations

**Current Implementation Status**: Basic framework with wandering/idling behaviors. The system is designed to be extended with combat, resource gathering, social interactions, etc.

---

## What is GOAP?

### The Core Philosophy

**Traditional AI (FSMs/Behavior Trees)**:

- You explicitly tell the NPC: "If enemy is near AND you have weapon THEN attack"
- Every combination must be manually programmed
- Gets exponentially complex as you add more states

**GOAP AI**:

- You give the NPC: Goals ("kill enemy"), Actions ("shoot", "find weapon"), and the current World State
- The NPC figures out: "I want to kill enemy → I need weapon → I should find weapon first"
- Plans are generated automatically using graph search algorithms

### Key Terminology

| Term             | Definition                                       | Example                              |
| ---------------- | ------------------------------------------------ | ------------------------------------ |
| **Belief**       | A fact the agent knows about the world           | "Player is nearby" = true            |
| **Precondition** | What must be true to perform an action           | "Has weapon" required to "Shoot"     |
| **Effect**       | What becomes true after an action completes      | "Shoot" makes "Enemy is dead" = true |
| **Goal**         | A desired world state the agent wants to achieve | "Enemy is dead" = true               |
| **Action**       | Something the agent can do to change the world   | "Shoot", "Reload", "Find Cover"      |
| **Plan**         | An ordered sequence of actions to achieve a goal | [Find Weapon → Load Weapon → Shoot]  |

---

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        GOAP AGENT                           │
│  (GoapAgent.cs - The "Brain" of the NPC)                    │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   BELIEFS    │  │   ACTIONS    │  │    GOALS     │       │
│  │ (World State)│  │ (What I can  │  │ (What I want)│       │
│  │ What I know  │  │     do)      │  │              │       │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
│         │                  │                  │             │
│         └──────────────────┼──────────────────┘             │
│                            ▼                                │
│                   ┌────────────────┐                        │
│                   │  GOAP PLANNER  │                        │
│                   │ (GoapPlanner)  │                        │
│                   └────────┬───────┘                        │
│                            │                                │
│                            ▼                                │
│                   ┌────────────────┐                        │
│                   │  ACTION PLAN   │                        │
│                   │  (Stack of     │                        │
│                   │   Actions)     │                        │
│                   └────────┬───────┘                        │
│                            │                                │
│                            ▼                                │
│                   ┌────────────────┐                        │
│                   │   STRATEGIES   │                        │
│                   │ (How actions   │                        │
│                   │  are executed) │                        │
│                   └────────────────┘                        │
└─────────────────────────────────────────────────────────────┘

         ┌──────────────┐          ┌──────────────┐
         │   SENSORS    │          │  UTILITIES   │
         │ (Perception) │          │ (Helpers)    │
         └──────────────┘          └──────────────┘
```

---

## Core Components Deep Dive

### 1. 🧠 **AgentBelief** (`Belief.cs`)

**Purpose**: Represents a single fact about the world that the agent believes to be true or false.

**Structure**:

```csharp
AgentBelief
├── Name: string (identifier like "PlayerNearby")
├── Condition: Func<bool> (lambda that checks if belief is true)
└── Location: Func<Vector3> (optional - where the belief is located)
```

**How it works**:

- Each belief is essentially a boolean flag with a name
- The `condition` function is called to check if the belief is currently true
- Optionally stores a location (useful for "Food is at X position")

**Example**:

```csharp
// A belief that's true when the agent is idle (not moving)
factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath);

// A belief with a location (player's position when in sensor range)
factory.AddSensorBelief("PlayerNearby", playerSensor);
```

**Builder Pattern**: Uses a fluent builder pattern for clean construction:

```csharp
new AgentBelief.Builder("PlayerNearby")
    .WithCondition(() => sensor.IsTargetInRange)
    .WithLocation(() => sensor.TargetPosition)
    .Build()
```

---

### 2. 🎬 **AgentAction** (`Actions.cs`)

**Purpose**: Represents something the agent can DO. Actions are the building blocks of plans.

**Structure**:

```csharp
AgentAction
├── Name: string (e.g., "Wander Around")
├── Cost: float (default 1, lower = more preferred)
├── Preconditions: HashSet<AgentBelief> (what must be true to start)
├── Effects: HashSet<AgentBelief> (what becomes true after completing)
└── Strategy: IActionStrategy (HOW the action is executed)
```

**The Action Lifecycle**:

1. **Can Start?** → Check if all preconditions are met
2. **Start()** → Initialize the strategy (e.g., calculate wander position)
3. **Update()** → Execute strategy each frame until complete
4. **Complete?** → Strategy determines when action is done
5. **Stop()** → Clean up

**Example Action**:

```csharp
new AgentAction.Builder("Wander Around")
    .WithCost(1)  // Neutral cost
    .WithStrategy(new WanderStrategy(navMeshAgent, 5))  // HOW to wander
    .AddPrecondition(beliefs["AgentIdle"])  // Must be idle to start wandering
    .AddEffect(beliefs["AgentMoving"])  // After wandering, agent is moving
    .Build()
```

**Key Insight**: Actions are **modular** and **reusable**. An action doesn't care about goals - it just knows:

- "I need X to start" (preconditions)
- "I will make Y true" (effects)
- "Here's how I execute" (strategy)

---

### 3. 🎯 **AgentGoal** (`Goals.cs`)

**Purpose**: Represents a desired world state the agent wants to achieve.

**Structure**:

```csharp
AgentGoal
├── Name: string (e.g., "Chill Out")
├── Priority: float (higher = more important)
└── DesiredEffects: HashSet<AgentBelief> (beliefs that should be true)
```

**How Priority Works**:

- Goals are evaluated in descending priority order
- If a higher-priority goal becomes achievable, the current plan is abandoned
- The `mostRecentGoal` gets a slight penalty (priority - 0.01) to avoid flip-flopping

**Example**:

```csharp
new AgentGoal.Builder("Survive")
    .WithPriority(10)  // VERY important
    .WithDesiredEffects(beliefs["HealthFull"])
    .Build()

new AgentGoal.Builder("Chill Out")
    .WithPriority(1)  // Low priority fallback
    .WithDesiredEffects(beliefs["Nothing"])  // Always false = always achievable
    .Build()
```

---

### 4. 🧮 **GoapPlanner** (`GoapPlanner.cs`)

**Purpose**: The "genius" of the system. Uses backward-chaining graph search to find action sequences.

**The Planning Algorithm** (Simplified):

```
1. Sort goals by priority (highest first)
2. For each goal:
   a. Create a goal node with desired effects
   b. Find actions whose effects contribute to the goal
   c. If action's preconditions aren't met, make them new subgoals
   d. Recursively solve subgoals
   e. Build a tree of possible action sequences
   f. Return the cheapest path (lowest total cost)
3. If no plan found for any goal, return null
```

**Backward Chaining Example**:

```
Goal: "Has Food" = true
Current State: "Has Food" = false, "At Kitchen" = false

Planner thinks backward:
1. What makes "Has Food" true? → "Cook Meal" action
2. Can I "Cook Meal"? → Precondition: "At Kitchen" = true
3. How do I get to kitchen? → "Go To Kitchen" action
4. Can I go to kitchen? → No preconditions! ✅

Final Plan: [Go To Kitchen] → [Cook Meal]
```

**Graph Search Details**:

- Uses a **recursive depth-first** approach
- Builds a tree of `Node` objects (each represents a world state)
- Each node tracks:
  - Parent node
  - Action that led here
  - Required effects still needed
  - Cumulative cost
  - Child nodes (leaves)

---

### 5. 🤖 **GoapAgent** (`GoapAgent.cs`)

**Purpose**: The main MonoBehaviour that brings everything together. This is the "brain" attached to NPCs.

**Initialization Flow** (in `Start()`):

```
1. SetupTimers()    → Create stat update timers
2. SetupBeliefs()   → Define what the agent knows
3. SetupActions()   → Define what the agent can do
4. SetupGoals()     → Define what the agent wants
```

**Update Loop Logic**:

```csharp
void Update() {
    // 1. Update stats (health, stamina)
    statsTimer.Tick(Time.deltaTime);

    // 2. Update animations based on movement
    animations.SetSpeed(navMeshAgent.velocity.magnitude);

    // 3. Do we need a new plan?
    if (currentAction == null) {
        CalculatePlan();  // Ask planner for a new plan

        if (plan found) {
            currentGoal = plan.Goal;
            currentAction = plan.Actions.Pop();  // Get first action
            currentAction.Start();
        }
    }

    // 4. Execute current action
    if (currentAction != null) {
        currentAction.Update(Time.deltaTime);

        if (currentAction.Complete) {
            currentAction.Stop();

            if (no more actions in plan) {
                // Plan complete! Reset for next frame
                currentGoal = null;
                currentAction = null;
            }
        }
    }
}
```

**Planning Strategy**:

- If no goal → check ALL goals
- If current goal exists → only check **higher priority** goals
- This allows interrupting low-priority tasks for emergencies

---

### 6. 👁️ **Sensor** (`Sensor.cs`)

**Purpose**: Gives the agent perception of the world using Unity's physics system.

**How it works**:

```
1. SphereCollider (trigger) defines detection radius
2. OnTriggerEnter → Player walks into range → Update beliefs
3. OnTriggerExit → Player leaves → Update beliefs
4. Timer-based position updates (prevents excessive updates)
5. OnTargetChanged event → Notifies agent when target changes
```

**Integration with Beliefs**:

```csharp
// In GoapAgent:
factory.AddSensorBelief("PlayerNearby", chaseSensor);

// This creates a belief that:
// - Is TRUE when chaseSensor.IsTargetInRange
// - Has LOCATION at chaseSensor.TargetPosition
```

**Event System**:

```csharp
// When sensor detects change:
OnTargetChanged?.Invoke();

// GoapAgent listens:
chaseSensor.OnTargetChanged += HandleTargetChanged;

// Handler forces replanning:
void HandleTargetChanged() {
    currentAction = null;  // Force recalculation
    currentGoal = null;
}
```

---

### 7. ⚙️ **Strategies** (`Strategies.cs`)

**Purpose**: Defines HOW actions are executed. Separates planning from execution.

**IActionStrategy Interface**:

```csharp
interface IActionStrategy {
    bool CanPerform { get; }  // Is action executable right now?
    bool Complete { get; }     // Has action finished?
    void Start();              // Initialize
    void Update(float dt);     // Execute each frame
    void Stop();               // Cleanup
}
```

**Built-in Strategies**:

#### IdleStrategy

```csharp
public class IdleStrategy : IActionStrategy {
    CountdownTimer timer;

    // Wait for X seconds, then mark complete
    public void Start() => timer.Start();
    public void Update(float dt) => timer.Tick(dt);
    public bool Complete => timer.IsFinished;
}
```

#### WanderStrategy

```csharp
public class WanderStrategy : IActionStrategy {
    NavMeshAgent agent;
    float wanderRadius;

    public void Start() {
        // Find random point on NavMesh within radius
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.position + randomDir, out hit, radius, 1)) {
            agent.SetDestination(hit.position);
        }
    }

    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;
}
```

---

### 8. 🎬 **AnimationController** (`AnimationController.cs`)

**Purpose**: Abstract base class for handling Unity Animator interactions.

**Design Pattern**: Template Method Pattern

- Base class handles common logic (timers, crossfading)
- Derived classes define specific animation clip hashes

**How it works**:

```csharp
// Set locomotion blend tree speed based on velocity
SetSpeed(navMeshAgent.velocity.magnitude);

// Play attack animation for its duration, then return to locomotion
Attack() →
    1. Get animation length
    2. Start timer
    3. Crossfade to attack clip
    4. When timer ends → Crossfade back to locomotion
```

---

### 9. ⏱️ **Timer** (`Timer.cs`)

**Purpose**: Reusable timer system with event callbacks.

**CountdownTimer**:

```csharp
CountdownTimer timer = new CountdownTimer(5f);
timer.OnTimerStart += () => Debug.Log("Started!");
timer.OnTimerStop += () => Debug.Log("Finished!");
timer.Start();

void Update() {
    timer.Tick(Time.deltaTime);  // Counts down to 0
}
```

**Common Use Cases**:

- Stat regeneration (`statsTimer` in GoapAgent)
- Sensor update intervals (`timer` in Sensor)
- Animation duration (`timer` in AnimationController)
- Idle action duration (`timer` in IdleStrategy)

---

## How Classes Connect

### 🔗 Dependency Graph

```
GoapAgent (Main Controller)
    ├── Requires: NavMeshAgent, AnimationController, Rigidbody
    ├── Uses: GoapPlanner (to create plans)
    ├── Contains: BeliefFactory (to build beliefs)
    ├── Manages:
    │   ├── Dictionary<string, AgentBelief> beliefs
    │   ├── HashSet<AgentAction> actions
    │   ├── HashSet<AgentGoal> goals
    │   └── ActionPlan currentPlan
    └── Subscribes: Sensor.OnTargetChanged

GoapPlanner
    ├── Input: GoapAgent, HashSet<AgentGoal>, (optional) mostRecentGoal
    ├── Output: ActionPlan (or null)
    └── Internal: Node tree for graph search

AgentAction
    ├── References: AgentBelief (preconditions & effects)
    └── Contains: IActionStrategy (execution logic)

AgentGoal
    └── References: AgentBelief (desired effects)

AgentBelief
    ├── Contains: Func<bool> condition
    └── Optional: Func<Vector3> location

BeliefFactory
    ├── Input: GoapAgent, Dictionary<string, AgentBelief>
    └── Creates: AgentBelief instances

Sensor
    ├── Requires: SphereCollider
    ├── Uses: CountdownTimer
    └── Provides: OnTargetChanged event, TargetPosition, IsTargetInRange

Strategies (IdleStrategy, WanderStrategy)
    ├── Uses: CountdownTimer (Idle)
    └── Uses: NavMeshAgent (Wander)

AnimationController
    ├── Uses: Animator, CountdownTimer
    └── Abstract: Must be inherited with specific clip definitions
```

### 🔄 Data Flow

```
FRAME N:
1. GoapAgent.Update() executes
2. statsTimer.Tick() → Updates health/stamina
3. No currentAction → CalculatePlan()
   ├── GoapPlanner.Plan(agent, goals, lastGoal)
   │   ├── Reads agent.beliefs (current world state)
   │   ├── Reads agent.actions (available actions)
   │   ├── Reads goals (sorted by priority)
   │   └── Returns ActionPlan with Stack<AgentAction>
   └── Pop first action → currentAction.Start()
       └── strategy.Start() initializes

FRAME N+1 to N+X:
4. currentAction.Update(deltaTime)
   ├── strategy.Update(deltaTime) executes
   └── Checks strategy.Complete
       ├── If complete → Apply action.Effects
       └── Pop next action or mark plan complete

FRAME N+X+1:
5. Plan complete → currentAction = null → Loop to step 3

INTERRUPT (Player enters sensor range):
* Sensor.OnTriggerEnter() → UpdateTargetPosition()
* OnTargetChanged.Invoke()
* GoapAgent.HandleTargetChanged() → currentAction = null, currentGoal = null
* Next frame → Replanning occurs with new world state
```

---

## The Planning Algorithm Explained

### Step-by-Step Walkthrough

Let's trace through a realistic scenario:

**Setup**:

```
Agent Stats:
- Health: 20 (low!)
- Stamina: 100

Beliefs:
- "AgentIdle": true (not moving)
- "AgentMoving": false
- "AtFoodShack": false (10 units away)
- "HealthLow": true (< 30)

Actions:
1. "Relax" (cost: 1)
   - Preconditions: None
   - Effects: "Nothing" (always false)
   - Strategy: Idle for 5 seconds

2. "Wander" (cost: 1)
   - Preconditions: "AgentIdle"
   - Effects: "AgentMoving"
   - Strategy: Walk to random point

3. "Go To Food Shack" (cost: 2)
   - Preconditions: "AgentIdle"
   - Effects: "AtFoodShack", "AgentMoving"
   - Strategy: Navigate to food shack

4. "Eat Food" (cost: 1)
   - Preconditions: "AtFoodShack"
   - Effects: "HealthLow" becomes false
   - Strategy: Idle for 3 seconds

Goals:
- "Stay Healthy" (priority: 10)
  - Desired: "HealthLow" = false
- "Wander" (priority: 1)
  - Desired: "AgentMoving" = true
```

### Planning Execution

**Phase 1: Goal Selection**

```
1. Sort goals by priority:
   [Stay Healthy (10), Wander (1)]

2. Filter goals where desired effects aren't met:
   - "Stay Healthy": "HealthLow" = true (not achieved) ✅ Include
   - "Wander": "AgentMoving" = false (not achieved) ✅ Include

3. Process highest priority: "Stay Healthy"
```

**Phase 2: Backward Chaining (Graph Search)**

```
Node 0 (Root): "Stay Healthy" Goal
├── Required Effects: {"HealthLow" = false}
├── Current World State: {"HealthLow" = true}
└── Search for actions...

    Found: "Eat Food" → Effects include "HealthLow" = false ✅

    Node 1: After "Eat Food"
    ├── Parent: Node 0
    ├── Action: "Eat Food"
    ├── Cost: 1
    ├── Required Effects: {"HealthLow" = false} - {"HealthLow" = false} + Preconditions
    │   = {} + {"AtFoodShack"}
    │   = {"AtFoodShack"}  ← New subgoal!
    └── Search for actions that make "AtFoodShack" true...

        Found: "Go To Food Shack" → Effects include "AtFoodShack" ✅

        Node 2: After "Go To Food Shack"
        ├── Parent: Node 1
        ├── Action: "Go To Food Shack"
        ├── Cost: 1 + 2 = 3
        ├── Required Effects: {"AtFoodShack"} - {"AtFoodShack"} + Preconditions
        │   = {} + {"AgentIdle"}
        │   = {"AgentIdle"}  ← New subgoal!
        └── Check current world state...
            "AgentIdle" = true ✅ Already satisfied!

        Node 2 returns TRUE (path found!)
        Add Node 2 to Node 1's leaves

    Node 1 returns TRUE
    Add Node 1 to Node 0's leaves

Node 0 returns TRUE
✅ Path found! Total cost: 3
```

**Phase 3: Extracting the Plan**

```
1. Select cheapest leaf from root:
   Node 0.Leaves = [Node 1 (cost: 3)]
   Cheapest = Node 1

2. Traverse from leaf to root, pushing actions to stack:
   Stack: []

   Navigate to Node 1: Push "Eat Food"
   Stack: [Eat Food]

   Navigate to Node 2: Push "Go To Food Shack"
   Stack: [Eat Food, Go To Food Shack]

   Node 2 has no leaves (base case)

3. Stack is now in execution order (LIFO):
   Pop → "Go To Food Shack" (executes first)
   Pop → "Eat Food" (executes second)
```

**Phase 4: Execution**

```
Frame 100:
├── currentAction = null
├── CalculatePlan() → ActionPlan created
├── Pop "Go To Food Shack" → currentAction
└── currentAction.Start()
    └── NavigateStrategy.Start() → agent.SetDestination(foodShack)

Frames 101-250:
├── currentAction.Update(deltaTime)
│   └── NavigateStrategy.Update(deltaTime)
│       └── Check agent.remainingDistance
├── animations.SetSpeed(agent.velocity.magnitude)  [Shows walking]
└── Complete = false (still traveling)

Frame 251:
├── currentAction.Update(deltaTime)
│   └── strategy.Complete = true (arrived!)
├── currentAction.Stop()
├── Apply Effects: "AtFoodShack" = true, "AgentMoving" = true
├── actionPlan.Actions.Count = 1 (still has "Eat Food")
├── currentAction = null (ready for next action)

Frame 252:
├── currentAction = null
├── No CalculatePlan() because actionPlan exists
├── Pop "Eat Food" → currentAction
└── currentAction.Start()
    └── IdleStrategy.Start() → timer.Start(3f)

Frames 253-433:
├── currentAction.Update(deltaTime)
│   └── IdleStrategy.Update(deltaTime)
│       └── timer.Tick(deltaTime)
├── animations.SetSpeed(0)  [Shows idle]
└── Complete = false (timer running)

Frame 434:
├── currentAction.Update(deltaTime)
│   └── strategy.Complete = true (timer finished!)
├── currentAction.Stop()
├── Apply Effects: "HealthLow" = false ✅ GOAL ACHIEVED!
├── actionPlan.Actions.Count = 0 (plan complete!)
├── lastGoal = "Stay Healthy"
├── currentGoal = null
└── currentAction = null

Frame 435:
├── currentAction = null
├── CalculatePlan() → Check all goals
│   ├── "Stay Healthy": "HealthLow" = false ✅ Already satisfied (skipped)
│   └── "Wander": "AgentMoving" = false ❌ Not satisfied
├── Plan for "Wander": [Wander Around]
└── Cycle continues...
```

---

## Practical Examples & Scenarios

### 📚 Scenario 1: Simple Idle Behavior

**Setup**:

```csharp
// In GoapAgent SetupBeliefs():
factory.AddBelief("Nothing", () => false);  // Always false

// In SetupActions():
actions.Add(new AgentAction.Builder("Relax")
    .WithStrategy(new IdleStrategy(5))
    .AddEffect(beliefs["Nothing"])
    .Build());

// In SetupGoals():
goals.Add(new AgentGoal.Builder("Chill Out")
    .WithPriority(1)
    .WithDesiredEffects(beliefs["Nothing"])
    .Build());
```

**What Happens**:

1. Goal "Chill Out" wants "Nothing" = true
2. "Nothing" is ALWAYS false (so goal is never truly achieved)
3. Planner finds "Relax" action (effect is "Nothing")
4. Agent executes "Relax" → Idles for 5 seconds
5. After 5 seconds, "Nothing" is still false
6. Planner creates new plan → "Relax" again
7. **Result**: Agent perpetually idles (fallback behavior)

**Why This Design?**

- Ensures agent ALWAYS has something to do
- Prevents null plans when no meaningful goals are achievable
- Acts as a "default" state

---

### 📚 Scenario 2: Health/Stamina Management

Let's extend the system to handle survival:

```csharp
// BELIEFS
factory.AddBelief("HealthLow", () => health < 30);
factory.AddBelief("StaminaLow", () => stamina < 30);
factory.AddLocationBelief("AtFoodShack", 3f, foodShack);
factory.AddLocationBelief("AtRest", 3f, restingPosition);

// ACTIONS
actions.Add(new AgentAction.Builder("Go To Food")
    .WithCost(2)
    .WithStrategy(new NavigateStrategy(navMeshAgent, foodShack.position))
    .AddPrecondition(beliefs["AgentIdle"])
    .AddEffect(beliefs["AtFoodShack"])
    .Build());

actions.Add(new AgentAction.Builder("Eat")
    .WithCost(1)
    .WithStrategy(new IdleStrategy(3))
    .AddPrecondition(beliefs["AtFoodShack"])
    .AddEffect(beliefs["HealthLow"])  // Makes it FALSE when evaluated
    .Build());

actions.Add(new AgentAction.Builder("Go To Bed")
    .WithCost(2)
    .WithStrategy(new NavigateStrategy(navMeshAgent, restingPosition.position))
    .AddPrecondition(beliefs["AgentIdle"])
    .AddEffect(beliefs["AtRest"])
    .Build());

actions.Add(new AgentAction.Builder("Sleep")
    .WithCost(1)
    .WithStrategy(new IdleStrategy(5))
    .AddPrecondition(beliefs["AtRest"])
    .AddEffect(beliefs["StaminaLow"])
    .Build());

// GOALS
goals.Add(new AgentGoal.Builder("Stay Healthy")
    .WithPriority(10)
    .WithDesiredEffects(beliefs["HealthLow"])  // Want this to be FALSE
    .Build());

goals.Add(new AgentGoal.Builder("Stay Energized")
    .WithPriority(8)
    .WithDesiredEffects(beliefs["StaminaLow"])
    .Build());
```

**Emergent Behavior**:

```
CASE A: Health = 20, Stamina = 100, At random location
→ Goal: "Stay Healthy" (priority 10)
→ Plan: [Go To Food, Eat]
→ Agent walks to food shack and eats

CASE B: Health = 100, Stamina = 20, At random location
→ Goal: "Stay Energized" (priority 8)
→ Plan: [Go To Bed, Sleep]
→ Agent walks to bed and sleeps

CASE C: Health = 20, Stamina = 20, At random location
→ Goal: "Stay Healthy" (priority 10 > 8)
→ Plan: [Go To Food, Eat]
→ After eating, health is restored
→ Next frame: Goal "Stay Energized" becomes highest priority
→ Plan: [Go To Bed, Sleep]
→ Agent then goes to sleep

CASE D: Health = 25, standing AT food shack
→ Goal: "Stay Healthy"
→ Belief "AtFoodShack" = true (already there!)
→ Plan: [Eat]  ← Skips navigation!
→ More efficient planning
```

---

### 📚 Scenario 3: Combat with Tactical Retreat

```csharp
// BELIEFS
factory.AddSensorBelief("EnemyNearby", attackSensor);  // Adds location
factory.AddBelief("HasWeapon", () => currentWeapon != null);
factory.AddBelief("HealthLow", () => health < 30);
factory.AddLocationBelief("AtCover", 2f, coverPosition);

// ACTIONS
actions.Add(new AgentAction.Builder("Pick Up Weapon")
    .WithCost(2)
    .WithStrategy(new NavigateToItemStrategy(navMeshAgent, "Weapon"))
    .AddEffect(beliefs["HasWeapon"])
    .Build());

actions.Add(new AgentAction.Builder("Shoot Enemy")
    .WithCost(1)
    .WithStrategy(new AttackStrategy(attackSensor, animations))
    .AddPrecondition(beliefs["EnemyNearby"])
    .AddPrecondition(beliefs["HasWeapon"])
    .AddEffect(beliefs["EnemyNearby"])  // Makes FALSE (enemy dead)
    .Build());

actions.Add(new AgentAction.Builder("Take Cover")
    .WithCost(3)
    .WithStrategy(new NavigateStrategy(navMeshAgent, coverPosition.position))
    .AddPrecondition(beliefs["HealthLow"])
    .AddEffect(beliefs["AtCover"])
    .Build());

actions.Add(new AgentAction.Builder("Heal")
    .WithCost(1)
    .WithStrategy(new IdleStrategy(5))
    .AddPrecondition(beliefs["AtCover"])
    .AddEffect(beliefs["HealthLow"])
    .Build());

// GOALS
goals.Add(new AgentGoal.Builder("Survive")
    .WithPriority(10)
    .WithDesiredEffects(beliefs["HealthLow"])
    .Build());

goals.Add(new AgentGoal.Builder("Eliminate Threat")
    .WithPriority(8)
    .WithDesiredEffects(beliefs["EnemyNearby"])
    .Build());
```

**Intelligent Behavior**:

```
SCENARIO A: Healthy + Enemy Nearby + Has Weapon
→ Goal: "Eliminate Threat" (priority 8)
→ Plan: [Shoot Enemy]
→ Agent immediately attacks

SCENARIO B: Healthy + Enemy Nearby + NO Weapon
→ Goal: "Eliminate Threat"
→ Plan: [Pick Up Weapon, Shoot Enemy]
→ Agent fetches weapon first, then attacks

SCENARIO C: Low Health + Enemy Nearby + Has Weapon
→ Goal: "Survive" (priority 10 > 8)
→ Plan: [Take Cover, Heal]
→ Agent RETREATS instead of fighting!
→ After healing, health restored
→ Next frame: Goal "Eliminate Threat" becomes top priority
→ Plan: [Shoot Enemy]
→ Agent re-engages

SCENARIO D: Low Health + Enemy Nearby + NO Weapon
→ Goal: "Survive" (priority 10)
→ Plan: [Take Cover, Heal]
→ Agent retreats first (doesn't try to get weapon while hurt)
→ After healing: Plan: [Pick Up Weapon, Shoot Enemy]
```

**This demonstrates**:

- ✅ **Self-preservation** (retreats when hurt)
- ✅ **Tactical thinking** (gets weapon before fighting)
- ✅ **Priority-based decision making** (survival > combat)
- ✅ **Emergent behavior** (you didn't code "retreat when hurt without weapon" - it emerged!)

---

### 📚 Scenario 4: Sensor-Driven Replanning

**What Happens When Player Enters/Exits Range?**

```csharp
// Initial situation:
Health: 100, No enemy nearby, At random location
Current Goal: "Wander" (low priority)
Current Plan: [Wander Around]
Current Action: Executing WanderStrategy (walking to random point)

// ⏱️ 2 seconds later...
Player walks into attackSensor range!

Sensor.OnTriggerEnter(player):
├── UpdateTargetPosition(player.gameObject)
├── target = player
├── lastKnownPosition = player.position
└── OnTargetChanged.Invoke()  ← EVENT FIRED

GoapAgent.HandleTargetChanged():
├── Debug.Log("Target changed, clearing action and goal")
├── currentAction = null  ← INTERRUPTS wandering
└── currentGoal = null    ← FORCES replanning

// Next frame:
GoapAgent.Update():
├── currentAction == null ✅
├── CalculatePlan()
│   ├── beliefs["EnemyNearby"].Evaluate() = true (sensor has target)
│   ├── Goal "Eliminate Threat" now has unmet desired effect
│   └── Plan: [Pick Up Weapon, Shoot Enemy]
├── currentAction = "Pick Up Weapon"
└── navMeshAgent.SetDestination(weaponLocation)

Result: Agent IMMEDIATELY stops wandering and goes into combat mode!
```

**Exit Behavior**:

```csharp
Player runs away, exits sensor range

Sensor.OnTriggerExit(player):
├── UpdateTargetPosition(null)  ← No target
├── target = null
└── OnTargetChanged.Invoke()  ← EVENT FIRED

GoapAgent.HandleTargetChanged():
├── currentAction = null  ← Stops shooting
└── currentGoal = null

// Next frame:
beliefs["EnemyNearby"].Evaluate() = false  ← Sensor has no target
Goal "Eliminate Threat" now satisfied (desired: not nearby)
New plan for "Wander" goal
Agent resumes patrol
```

---

## Code Flow Diagrams

### 🔄 **Initialization Flow**

```
Unity Awake():
    GoapAgent.Awake()
    ├── navMeshAgent = GetComponent<NavMeshAgent>()
    ├── animations = GetComponent<AnimationController>()
    ├── rb = GetComponent<Rigidbody>()
    │   └── rb.freezeRotation = true
    └── gPlanner = new GoapPlanner()

Unity Start():
    GoapAgent.Start()
    ├── SetupTimers()
    │   ├── statsTimer = new CountdownTimer(2f)
    │   ├── statsTimer.OnTimerStop += UpdateStats + Restart
    │   └── statsTimer.Start()
    │
    ├── SetupBeliefs()
    │   ├── beliefs = new Dictionary<string, AgentBelief>()
    │   ├── factory = new BeliefFactory(this, beliefs)
    │   ├── factory.AddBelief("Nothing", () => false)
    │   ├── factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath)
    │   └── factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath)
    │
    ├── SetupActions()
    │   ├── actions = new HashSet<AgentAction>()
    │   ├── actions.Add("Relax" action)
    │   └── actions.Add("Wander Around" action)
    │
    └── SetupGoals()
        ├── goals = new HashSet<AgentGoal>()
        ├── goals.Add("Chill Out" goal)
        └── goals.Add("Wander" goal)

Unity OnEnable():
    chaseSensor.OnTargetChanged += HandleTargetChanged
```

---

### 🔄 **Planning Flow (Detailed)**

```
CalculatePlan()
│
├─ Determine priority level
│  └─ priorityLevel = currentGoal?.Priority ?? 0
│
├─ Determine which goals to check
│  ├─ IF currentGoal exists:
│  │  └─ goalsToCheck = goals where (g.Priority > priorityLevel)
│  └─ ELSE:
│     └─ goalsToCheck = all goals
│
└─ Call GoapPlanner.Plan(this, goalsToCheck, lastGoal)
   │
   ├─ GoapPlanner.Plan()
   │  │
   │  ├─ Filter goals
   │  │  └─ orderedGoals = goals where (any DesiredEffect.Evaluate() == false)
   │  │     .OrderByDescending(priority with mostRecentGoal penalty)
   │  │
   │  ├─ FOR EACH goal in orderedGoals:
   │  │  │
   │  │  ├─ Create root node
   │  │  │  └─ goalNode = new Node(null, null, goal.DesiredEffects, 0)
   │  │  │
   │  │  ├─ FindPath(goalNode, agent.actions)
   │  │  │  │
   │  │  │  ├─ FOR EACH action in actions:
   │  │  │  │  │
   │  │  │  │  ├─ Copy required effects
   │  │  │  │  │  └─ requiredEffects = parent.RequiredEffects.Clone()
   │  │  │  │  │
   │  │  │  │  ├─ Remove satisfied effects
   │  │  │  │  │  └─ requiredEffects.RemoveWhere(b => b.Evaluate())
   │  │  │  │  │
   │  │  │  │  ├─ IF requiredEffects.Count == 0:
   │  │  │  │  │  └─ return true  ← BASE CASE (all satisfied)
   │  │  │  │  │
   │  │  │  │  ├─ IF action.Effects overlap with requiredEffects:
   │  │  │  │  │  │
   │  │  │  │  │  ├─ Calculate new required effects
   │  │  │  │  │  │  ├─ newRequired = requiredEffects - action.Effects
   │  │  │  │  │  │  └─ newRequired += action.Preconditions
   │  │  │  │  │  │
   │  │  │  │  │  ├─ Create child node
   │  │  │  │  │  │  └─ newNode = new Node(parent, action, newRequired, parent.Cost + action.Cost)
   │  │  │  │  │  │
   │  │  │  │  │  ├─ Recurse
   │  │  │  │  │  │  └─ IF FindPath(newNode, actions - currentAction):
   │  │  │  │  │  │     └─ parent.Leaves.Add(newNode)
   │  │  │  │  │  │
   │  │  │  │  │  └─ IF newRequired.Count == 0:
   │  │  │  │  │     └─ return true
   │  │  │  │  │
   │  │  │  │  └─ (continue to next action)
   │  │  │  │
   │  │  │  └─ return false  ← No path found
   │  │  │
   │  │  ├─ IF FindPath succeeded AND goalNode.Leaves.Count > 0:
   │  │  │  │
   │  │  │  ├─ Build action stack by traversing tree
   │  │  │  │  │
   │  │  │  │  ├─ actionStack = new Stack<AgentAction>()
   │  │  │  │  ├─ currentNode = goalNode
   │  │  │  │  │
   │  │  │  │  └─ WHILE currentNode.Leaves.Count > 0:
   │  │  │  │     ├─ cheapestLeaf = currentNode.Leaves.OrderBy(cost).First()
   │  │  │  │     ├─ currentNode = cheapestLeaf
   │  │  │  │     └─ actionStack.Push(cheapestLeaf.Action)
   │  │  │  │
   │  │  │  └─ RETURN new ActionPlan(goal, actionStack, cost)
   │  │  │
   │  │  └─ (try next goal)
   │  │
   │  └─ RETURN null  ← No goals had valid plans
   │
   └─ IF potentialPlan != null:
      ├─ actionPlan = potentialPlan
      └─ (Update GoapAgent's actionPlan)
```

---

### 🔄 **Action Execution Flow**

```
Frame N (currentAction == null):
│
├─ CalculatePlan()  [See above]
│  └─ actionPlan created: Stack = [Action2, Action1]
│
├─ currentGoal = actionPlan.AgentGoal
├─ currentAction = actionPlan.Actions.Pop()  → Action1
├─ currentAction.Start()
│  └─ strategy.Start()
│     └─ (Initialize execution, e.g., set nav destination)
│
└─ Debug: "Goal: {goal} with {count} actions in plan"

Frame N+1 to N+M (action executing):
│
├─ currentAction != null ✅
├─ currentAction.Update(Time.deltaTime)
│  │
│  ├─ IF strategy.CanPerform:
│  │  └─ strategy.Update(deltaTime)
│  │     └─ (Execute action logic, e.g., navigate, wait timer)
│  │
│  ├─ IF !strategy.Complete:
│  │  └─ return  ← Still executing
│  │
│  └─ IF strategy.Complete:
│     └─ FOR EACH effect in Effects:
│        └─ effect.Evaluate()  ← Updates world state
│
└─ Complete = false (not done yet)

Frame N+M+1 (action completes):
│
├─ currentAction.Update(deltaTime)
│  └─ strategy.Complete = true ✅
│
├─ currentAction.Complete = true
├─ currentAction.Stop()
│  └─ strategy.Stop()
│     └─ (Cleanup)
│
├─ Debug: "{actionName} complete"
│
├─ IF actionPlan.Actions.Count == 0:
│  ├─ Debug: "Plan complete"
│  ├─ lastGoal = currentGoal
│  ├─ currentGoal = null
│  └─ currentAction = null
│
└─ ELSE:
   └─ currentAction = null  ← Will pop next action next frame
```

---

## Extending the System

### 🔧 Adding a New Action

**Example**: Add a "door opening" action

```csharp
// Step 1: Add belief for door state
factory.AddBelief("DoorOpen", () => door.isOpen);
factory.AddLocationBelief("AtDoor", 2f, doorPosition);

// Step 2: Create action
actions.Add(new AgentAction.Builder("Open Door")
    .WithCost(1)
    .WithStrategy(new InteractStrategy(door.gameObject, "Open"))
    .AddPrecondition(beliefs["AtDoor"])
    .AddEffect(beliefs["DoorOpen"])
    .Build());

// Step 3: Create strategy (if custom behavior needed)
public class InteractStrategy : IActionStrategy {
    GameObject target;
    string interactionName;

    public bool CanPerform => target != null;
    public bool Complete { get; private set; }

    public InteractStrategy(GameObject target, string interaction) {
        this.target = target;
        interactionName = interaction;
    }

    public void Start() {
        // Send message to target object
        target.SendMessage(interactionName);
        Complete = true;  // Instant interaction
    }
}
```

**That's it!** The planner automatically incorporates "Open Door" into plans:

- If goal requires being past door → Plans: [Go To Door, Open Door, ...]
- If door is already open → Skips opening action

---

### 🔧 Adding a New Goal

```csharp
// Step 1: Define belief for goal condition
factory.AddLocationBelief("AtDestination", 1f, targetLocation);

// Step 2: Create goal
goals.Add(new AgentGoal.Builder("Reach Destination")
    .WithPriority(5)
    .WithDesiredEffects(beliefs["AtDestination"])
    .Build());
```

**Automatic Integration**:

- Planner checks if "AtDestination" is false
- Looks for actions with "AtDestination" effect
- Builds plan using existing navigation actions

---

### 🔧 Adding Complex Multi-Step Actions

**Example**: Crafting system

```csharp
// Beliefs
factory.AddBelief("HasWood", () => inventory.HasItem("Wood"));
factory.AddBelief("HasStone", () => inventory.HasItem("Stone"));
factory.AddBelief("HasAxe", () => inventory.HasItem("Axe"));
factory.AddLocationBelief("AtWorkbench", 2f, workbench);

// Actions
actions.Add(new AgentAction.Builder("Gather Wood")
    .WithCost(3)
    .WithStrategy(new GatherResourceStrategy("Wood"))
    .AddEffect(beliefs["HasWood"])
    .Build());

actions.Add(new AgentAction.Builder("Gather Stone")
    .WithCost(3)
    .WithStrategy(new GatherResourceStrategy("Stone"))
    .AddEffect(beliefs["HasStone"])
    .Build());

actions.Add(new AgentAction.Builder("Craft Axe")
    .WithCost(2)
    .WithStrategy(new CraftingStrategy("Axe"))
    .AddPrecondition(beliefs["HasWood"])
    .AddPrecondition(beliefs["HasStone"])
    .AddPrecondition(beliefs["AtWorkbench"])
    .AddEffect(beliefs["HasAxe"])
    .Build());

// Goal
goals.Add(new AgentGoal.Builder("Get Axe")
    .WithPriority(7)
    .WithDesiredEffects(beliefs["HasAxe"])
    .Build());
```

**Emergent Plan**:

```
CASE: No wood, no stone, not at workbench
→ Plan: [Gather Wood, Gather Stone, Go To Workbench, Craft Axe]

CASE: Has wood, no stone, not at workbench
→ Plan: [Gather Stone, Go To Workbench, Craft Axe]

CASE: Has wood, has stone, not at workbench
→ Plan: [Go To Workbench, Craft Axe]

CASE: Has wood, has stone, AT workbench
→ Plan: [Craft Axe]
```

The planner automatically optimizes based on current state!

---

### 🔧 Advanced: Adding Action Costs for Smart Behavior

```csharp
// Expensive but safe route
actions.Add(new AgentAction.Builder("Take Safe Path")
    .WithCost(10)  // High cost
    .WithStrategy(new NavigateStrategy(navMeshAgent, safePath))
    .AddEffect(beliefs["AtDestination"])
    .Build());

// Cheap but dangerous route
actions.Add(new AgentAction.Builder("Take Shortcut")
    .WithCost(2)  // Low cost
    .WithStrategy(new NavigateStrategy(navMeshAgent, dangerousPath))
    .AddPrecondition(beliefs["HealthHigh"])  // Only if healthy
    .AddEffect(beliefs["AtDestination"])
    .Build());
```

**Behavior**:

- High health → Takes shortcut (cheaper)
- Low health → Takes safe path (even though more expensive)
- Beliefs gate risky actions!

---

## Summary & Key Takeaways

### 🎯 Core Concepts

1. **Beliefs** = What the agent knows (world state)
2. **Actions** = What the agent can do (with preconditions & effects)
3. **Goals** = What the agent wants (desired world state)
4. **Planner** = How the agent thinks (backward-chaining graph search)
5. **Strategies** = How actions are executed (navigation, timing, etc.)

### 🧠 How GOAP Differs from FSMs

| Aspect                | Finite State Machine   | GOAP                     |
| --------------------- | ---------------------- | ------------------------ |
| **Transitions**       | Manually defined       | Automatically discovered |
| **Scalability**       | Exponential complexity | Linear addition          |
| **Flexibility**       | Rigid                  | Dynamic replanning       |
| **Design**            | State-centric          | Goal-centric             |
| **Emergent Behavior** | Rare                   | Common                   |

### ✅ Best Practices

1. **Keep actions atomic** - One action = one clear purpose
2. **Use meaningful costs** - Reflect actual effort/danger/time
3. **Define clear preconditions** - Prevents impossible plans
4. **Test beliefs frequently** - Stale data = bad plans
5. **Use priority wisely** - High priority = interrupts current plans
6. **Leverage sensors** - Dynamic world state updates
7. **Log everything (debug builds)** - GOAP can be hard to debug without logs

### ⚠️ Common Pitfalls

1. **Circular dependencies** - Action A needs B, B needs A → No plan found
2. **Always-true preconditions** - Makes actions too permissive
3. **Always-false effects** - Creates unsolvable goals
4. **Too many actions** - Planning becomes slow (use profiler!)
5. **Forgetting to update beliefs** - Agent acts on old information

---

## Resources & References

- **Original GOAP Paper**: Jeff Orkin (F.E.A.R. AI) - "Three States and a Plan: The A.I. of F.E.A.R."
- **Unity NavMesh Documentation**: [docs.unity3d.com/Manual/Navigation.html](https://docs.unity3d.com/Manual/Navigation.html)
- **A\* Algorithm**: [wikipedia.org/wiki/A\*\_search_algorithm](https://en.wikipedia.org/wiki/A*_search_algorithm)
- **Builder Pattern**: [refactoring.guru/design-patterns/builder](https://refactoring.guru/design-patterns/builder)

---

## Conclusion

This GOAP system provides a robust foundation for intelligent AI agents. The beauty of GOAP is that **complex behaviors emerge from simple rules**. As you add more beliefs, actions, and goals, the agents become more "alive" without requiring exponentially more code.

**Next Steps**:

1. Run the current implementation and observe wandering behavior
2. Add health/stamina actions as shown in examples
3. Implement combat with sensors
4. Create NPC-specific goals (guard posts, patrols, etc.)
5. Profile performance and optimize planning if needed

The system is designed to grow with your game!
