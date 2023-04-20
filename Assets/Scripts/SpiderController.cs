using System;
using System.Collections;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

/// <summary>
/// <para>This class is responsible for controlling the spider creature</para>
/// </summary>
public class SpiderController : MonoBehaviour
{
    //  Spider target transform
    [Header("Target")]
    [SerializeField] Transform target;
    
    
    //  Head tracking parameters
    [Header("Head tracking")]
    [SerializeField] Transform headBone;
    [SerializeField] float lookSpeed = 1.0f;
    [SerializeField] [Range(0f, 90f)] float lookConstraint = 45.0f;
    
    //  TODO: 
    //  Add parameters to tune second-order motion
    //  Root motion parameters
    [Header("Root motion")]
    [SerializeField] float turnSpeed = 1.0f, turnAcceleration = 1.0f;
    [SerializeField] float moveSpeed = 1.0f, moveAcceleration = 1.0f;
    [SerializeField] float approachDistance = 1.0f, retreatDistance = 1.0f;
    [SerializeField] [Range(0f, 90f)] float maxTurnAngle = 45.0f;
    
    //  Coefficients to tune the second order movement system
    [Header("Movement coefficients")]
    [SerializeField] 
    private float fx,zx,rx;

    //  Stepper parameters
    [Header("Steppers")]
    //  height offset for the body relative to the feet joints
    [SerializeField] private float bodyHeightOffset = 2.75f;
    [SerializeField] Transform rootBone;
    //  Smoothing factor to apply to body orientation matching
    [SerializeField] [Range(0f, 7f)] float bodyAngleSmoothing = 1f;
    
    
    /// <summary>
    /// A struct representing groups of legs that move in parallel
    /// </summary>
    [Serializable]
    public struct LegGroup
    {
        /// <value>The legs which the group contains</value>
        public Stepper[] legs;
        /// <value>The group default step distance</value>
        public float stepDistance;
        /// <value>The group default step overshoot amount</value>
        [Range(0.0f,1.0f)]
        public float overshootAmount;
    }
    
    //  Array of leg groups
    [SerializeField] private LegGroup[] legGroups;
    
    //  Motion scripts for the head tracking and root motion
    private LookMotion _headTracker;
    private RootMotion _rootMotion;
    
    private Vector3 _lastBodyUp;
    //  Corner legs used for body orientation
    [SerializeField] private Transform[] corners;


    /// <summary>
    ///     <para>Initialise the head tracking, root motion and leg steppers</para>
    /// </summary>
    private void Awake()
    {
        _headTracker = new(headBone, target, lookConstraint, lookSpeed);
        _rootMotion = new(transform, target, moveSpeed, turnSpeed, maxTurnAngle, moveAcceleration, turnAcceleration, approachDistance, retreatDistance);
        _rootMotion.SetMovementCoefficients(fx,zx, rx);

        foreach (var group in legGroups)
        {
            //  Calculate leg parameters
            //  Step duration is a function of move speed and step distance
            var stepDuration =  group.stepDistance / moveSpeed;
            group.legs.ToList().ForEach(leg => leg.ChangeLegParameters(group.stepDistance, stepDuration, group.overshootAmount));
        }
        StartCoroutine(MoveLegs());
        _lastBodyUp = rootBone.up;
    }

    /// <summary>
    ///     <para>Update the motion parameters for the head tracking and root motion</para> 
    /// </summary>
    private void OnValidate()
    {
        //  Update the motion parameters used by the head and root motion scripts
        if (_rootMotion == null || _headTracker == null)
            return;
        _rootMotion.ChangeMotionParameters(moveSpeed, turnSpeed, maxTurnAngle, moveAcceleration, turnAcceleration, approachDistance, retreatDistance);
        _headTracker.ChangeMotionParameters(lookConstraint, lookSpeed);
    }

    
    /// <summary>
    ///     <para>Update the root motion and head tracking before adjusting the body position and orientation.</para>
    /// </summary>
    /// <remarks>We perform this in late update as it will ensure we have the most recent data</remarks>
    void LateUpdate()
    {
        _rootMotion.UpdateRootMotion();
        _headTracker.UpdateLookMotion();
        
        //  Determine body position relative to legs
        var startPosition = rootBone.position;
        //  Calculate the average position of all legs
        var averagePosition = legGroups.SelectMany(group => group.legs)
            .Aggregate(Vector3.zero, (current, leg) => current + leg.transform.position) / legGroups.SelectMany(group => group.legs).Count();
        
        //  Calculate the end position by applying our body offset
        var endPosition = averagePosition + new Vector3(0,bodyHeightOffset);
        
        //  Calculate the centre of the body position before interpolating toward it
        var centre = (startPosition + endPosition) / 2;
        rootBone.position =
            Vector3.Lerp(
                Vector3.Lerp(startPosition, centre,  Time.deltaTime),
                Vector3.Lerp(centre, averagePosition + new Vector3(0,bodyHeightOffset),  Time.deltaTime),
                Time.deltaTime
            );

        //  Orient body based on corner leg positions
        var v1 = corners[0].position - corners[1].position;
        var v2 = corners[2].position - corners[3].position;
        //  Calculate the cross product of the vectors between diagonally opposed legs
        var normal = Vector3.Cross(v1, v2).normalized;
        
        //  Interpolate the vector to smooth it and apply it to our local rotation
        var newUp = Vector3.Lerp(_lastBodyUp, normal, 1f / (bodyAngleSmoothing + 1));
        rootBone.localRotation = Quaternion.FromToRotation(Vector3.up, newUp);
        _lastBodyUp = newUp;
    }
    /// <summary>
    ///     <para>Try and move each group of legs</para>
    /// </summary>
    private IEnumerator MoveLegs()
    {
        while (true)
        {
            foreach (var group in legGroups)
            {
                foreach (var leg in group.legs)
                {
                    leg.TryStep();
                }
                do
                {
                    yield return null;
                    
                } while (group.legs.Any(leg => !leg.Grounded));
            }
        }
    }

    /// <summary>
    ///     <para>Debug method for drawing the body orientation</para>
    /// </summary>
    private void OnDrawGizmos()
    {
        Debug.DrawRay(rootBone.position, rootBone.up * 20, Color.green);
    }
}
