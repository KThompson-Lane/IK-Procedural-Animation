using System.Collections;
using UnityEngine;

/// <summary>
/// <para>A class which will attempt to take a step when it surpasses a set distance from it's home position</para>
/// </summary>
public class Stepper : MonoBehaviour
{
    //  The home transform
    [SerializeField] private Transform home;
    //  If we surpass this distance, step towards home
    private float _stepDistance;
    //  Time it takes to complete a step 
    private float _stepDuration;
    //  Amount to overshoot the home position by
    private float _stepOvershoot;
    
    //  Whether it is currently taking a step
    private bool _moving;
    
    //  Is it grounded
    public bool Grounded => !_moving;

    //  Current distance to home position
    public float DistanceToHome => Vector3.Distance(transform.position, home.position);

    /// <summary>
    /// <para>Attempt to take a step if not moving and far enough away from home position</para>
    /// </summary>
    public void TryStep()
    {
        //  Do nothing if we're already taking a step
        if (_moving) return;

        if (Vector3.Distance(transform.position, home.position) > _stepDistance)
            StartCoroutine(TakeStep());
    }
    /// <summary>
    ///     <para>Adjust stepper parameters including step distance, step duration and the amount the step overshoots by</para>
    /// </summary>
    /// <param name="stepDistance">Minimum distance from home before taking a step</param>
    /// <param name="stepDuration">Duration of a step in seconds</param>
    /// <param name="stepOvershoot">Fraction of distance to overshoot step by</param>
    public void ChangeLegParameters(float stepDistance, float stepDuration, float stepOvershoot)
    {
        _stepDistance = stepDistance;
        _stepDuration = stepDuration;
        _stepOvershoot = stepOvershoot;
    }
    
    /// <summary>
    ///     <para>Coroutine for taking a step towards home taking stepDuration seconds</para>
    /// </summary>
    /// <returns> IEnumerator for the coroutine </returns>
    IEnumerator TakeStep()
    {
        //  Indicate a step has started
        _moving = true;

        //  Keep track of initial transform
        var initialPosition = transform.position;
        var initialRotation = transform.rotation;

        //  Keep track of our target transform
        var targetPosition = home.position;
        var targetRotation = home.rotation;
        
        //  Directional vector to the home position
        var toHome = (targetPosition - initialPosition);
        
        //  Calculate overshot amount and project it onto the XZ plane
        var overshotAmount = _stepDistance * _stepOvershoot;
        var overshootVector = toHome * overshotAmount;
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);
        
        //  Apply our overshoot vector to our home position to calculate our step target
        var stepTarget = targetPosition + overshootVector;
        
        //  Calculate centre point
        var stepCentre = (initialPosition + stepTarget) / 2;
        
        //  Raise centre point slightly to give the step some lift
        stepCentre += home.up * Vector3.Distance(initialPosition, stepTarget) / 2f;
        
        // Time since step started
        var timeElapsed = 0f;
        
        //  Take a step using a do-while loop
        do
        {
            // Increment time elapsed with deltaTime
            timeElapsed += Time.deltaTime;
            
            //  Calculate time-step using the step duration and our total time elapsed
            var T = timeElapsed / _stepDuration;
            // Interpolate transform quadratically using nested Lerps
            transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(initialPosition, stepCentre, T),
                    Vector3.Lerp(stepCentre, stepTarget, T),
                    T
                );

            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, T);

            // Wait for one frame
            yield return null;
        }
        while (timeElapsed < _stepDuration);

        // Indicate the step has finished
        _moving = false;
    }

    /// <summary>
    /// <para>Debug function for drawing each steppers current position and their home position</para>
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(home.position, _stepDistance);
    }
}
