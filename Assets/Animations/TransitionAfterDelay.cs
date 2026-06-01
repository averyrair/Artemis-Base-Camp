using UnityEngine;

public class TransitionAfterDelay : StateMachineBehaviour
{
    [Tooltip("How long to wait after the animation finishes.")]
    public float delayInSeconds = 2f;
    
    [Tooltip("The trigger parameter to call after the wait.")]
    public string triggerParameter = "NextState";

    private float timer = 0f;
    private bool isWaiting = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        isWaiting = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check if the animation has finished playing (normalizedTime >= 1)
        if (!isWaiting && stateInfo.normalizedTime >= 1f && !animator.IsInTransition(layerIndex))
        {
            isWaiting = true;
        }

        // If finished, start the countdown
        if (isWaiting)
        {
            timer += Time.deltaTime;
            if (timer >= delayInSeconds)
            {
                animator.SetTrigger(triggerParameter);
                isWaiting = false; // Reset to prevent multiple triggers
            }
        }
    }
}