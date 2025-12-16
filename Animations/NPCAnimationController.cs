using UnityEngine;

public class NPCAnimationController : AnimationController {
    protected override void SetLocomotionClip() {
        locomotionClip = Animator.StringToHash("Locomotion");
    }
    
    protected override void SetAttackClip() {
        attackClip = Animator.StringToHash("NPC_Attack01");
    }
    
    protected override void SetSpeedHash() {
        speedHash = Animator.StringToHash("Speed");
    }
}