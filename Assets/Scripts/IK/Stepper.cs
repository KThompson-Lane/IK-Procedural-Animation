using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// A stepper class which will be placed on the target game object for each leg
/// </summary>
public class Stepper : MonoBehaviour
{
    //  The home transform for this leg
    [SerializeField] private Transform home;
    //  If we surpass this distance, step towards home
    [SerializeField] float _stepDistance;
    //  Time it takes to complete a step 
    [SerializeField] float _stepDuration;
    //  Amount to overshoot the home position by
    [SerializeField] float _stepOvershoot;
    
    private bool _moving;
    public bool Grounded => !_moving;

    public float DistanceToHome => Vector3.Distance(transform.position, home.position);

    public void TryStep()
    {
        //  Do nothing if we're already taking a step
        if (_moving) return;

        if (Vector3.Distance(transform.position, home.position) > _stepDistance)
            StartCoroutine(TakeStep());
    }

    public void ChangeLegParameters(float stepDistance, float stepDuration, float stepOvershoot)
    {
        _stepDistance = stepDistance;
        _stepDuration = stepDuration;
        _stepOvershoot = stepOvershoot;
    }
    
    /// <summary>
    /// Coroutine for taking a step towards home taking stepDuration seconds
    /// </summary>
    /// <returns> IEnumerator for the coroutine </returns>
    IEnumerator TakeStep()
    {
        //  Indicate this leg is taking a step
        _moving = true;

        //  Keep track of our initial transform
        var startRotation = transform.rotation;
        var startPosition = transform.position;

        //  Keep track of our target transform
        var endRotation = home.rotation;
        var endPosition = home.position;
        //  Directional vector to the home position
        Vector3 stepDirection = (endPosition - startPosition);
        //  Distance to overshoot by
        float overshootDistance = _stepDistance * _stepOvershoot;
        Vector3 movementVector = stepDirection * overshootDistance;
        //  Project it onto the ground plane
        movementVector = Vector3.ProjectOnPlane(movementVector, Vector3.up);
        endPosition += movementVector;
        
        //  Pass through the centre point
        Vector3 centre = (startPosition + endPosition) / 2;
        
        //  Raise centre point slightly to give the step some lift
        centre += home.up * Vector3.Distance(startPosition, endPosition) / 2f;
        // Time since step started
        var timeElapsed = 0f;

        // Here we use a do-while loop so the normalized time goes past 1.0 on the last iteration,
        // placing us at the end position before ending.
        do
        {
            // Increment time elapsed with deltaTime
            timeElapsed += Time.deltaTime;

            //  Normalise elapsed time in terms of total step duration
            float normalizedTime = timeElapsed / _stepDuration;
            // Interpolate transform quadratically using nested Lerps
            transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(startPosition, centre, normalizedTime),
                    Vector3.Lerp(centre, endPosition, normalizedTime),
                    normalizedTime
                );

            transform.rotation = Quaternion.Slerp(startRotation, endRotation, normalizedTime);

            // Wait for one frame
            yield return null;
        }
        while (timeElapsed < _stepDuration);

        // Finished taking a step
        _moving = false;
    }

    //  Debug function for drawing each legs current position and home-position
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(home.position, _stepDistance);
    }
}
