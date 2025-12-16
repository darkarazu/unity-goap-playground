using System;
using UnityEngine;
using UnityEngine.AI;

public interface IActionStrategy {
    bool CanPerform { get; }
    bool Complete { get; }
    
    void Start() {}
    
    void Update(float deltaTime) {}
    
    void Stop() {}
    
}

public class IdleStrategy : IActionStrategy {
    public bool CanPerform => true;
    public bool Complete { get; private set; }
    
    readonly CountdownTimer timer;

    public IdleStrategy(float duration) {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public class WanderStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    private readonly float wanderRadius;
    
    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius) {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
    }

    public void Start() {
        for (int i = 0; i < 5; i++) {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius).With(y: 0);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1)) {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
    
}
