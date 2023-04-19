using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;
    
    [Header("Head tracking")]
    [SerializeField] Transform headBone;
    [SerializeField] float lookSpeed = 1.0f;
    [SerializeField] [Range(0f, 90f)] float lookConstraint = 45.0f;
    
    //  TODO: 
    //  Add parameter for following distance
    //  Add parameters to tune second-order motion
    [Header("Root motion")]
    [SerializeField] float turnSpeed = 1.0f, turnAcceleration = 1.0f;
    [SerializeField] float moveSpeed = 1.0f, moveAcceleration = 1.0f;
    [SerializeField] float approachDistance = 1.0f, retreatDistance = 1.0f;
    [SerializeField] [Range(0f, 90f)] float maxTurnAngle = 45.0f;
    
    [Header("Movement coefficients")]
    [SerializeField] 
    private float fx,zx,rx;
    
    [Header("Orientation coefficients")]
    [SerializeField]
    private float fy,zy,ry;
    
    [Header("Leg Steppers")]
    //  Leg stepper parameters
    [SerializeField] private float bodyHeightOffset = 2.75f;
    [SerializeField] Transform rootBone;
    [SerializeField] [Range(0f, 7f)] float bodyAngleSmoothing = 1f;
    //  Leg groups are groups of individual legs which move together
    [Serializable]
    public struct LegGroup
    {
        public Stepper[] legs;
        public float stepDistance;
        public float overshootAmount;
    }
    
    //  Array of leg pairs

    [SerializeField] private LegGroup[] legGroups;
    
    private LookMotion _headTracker;
    private RootMotion _rootMotion;

    private Vector3 _lastBodyUp;
    [SerializeField] private Transform[] corners;
    
    // We do all animation code in LateUpdate
    // This ensures it has the latest object data prior to frame drawing
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

    private void OnValidate()
    {
        //  Update the motion parameters used by the head and root motion scripts
        if (_rootMotion == null || _headTracker == null)
            return;
        _rootMotion.ChangeMotionParameters(moveSpeed, turnSpeed, maxTurnAngle, moveAcceleration, turnAcceleration, approachDistance, retreatDistance);
        _headTracker.ChangeMotionParameters(lookConstraint, lookSpeed);
    }

    void LateUpdate()
    {
        _rootMotion.UpdateRootMotion();
        _headTracker.UpdateHeadMotion();
        
        //  Determine body position relative to legs.
        var startPosition = rootBone.position;
        var averagePosition = legGroups.SelectMany(group => group.legs).Aggregate(Vector3.zero, (current, leg) => current + leg.transform.position) / legGroups.SelectMany(group => group.legs).Count();
        
        var endPosition = averagePosition + new Vector3(0,bodyHeightOffset);
        var centre = (startPosition + endPosition) / 2;
        rootBone.position =
            Vector3.Lerp(
                Vector3.Lerp(startPosition, centre,  Time.deltaTime),
                Vector3.Lerp(centre, averagePosition + new Vector3(0,bodyHeightOffset),  Time.deltaTime),
                Time.deltaTime
            );
        
        
        //  Orient body based on corner leg positions
        Vector3 v1 = corners[0].position - corners[1].position;
        Vector3 v2 = corners[2].position - corners[3].position;
        //  Calculate the cross product based on the vectors between the diagonally opposed legs
        var normal = Vector3.Cross(v1, v2).normalized;
        
        //  Interpolate the vector to smooth it
        Vector3 newUp = Vector3.Lerp(_lastBodyUp, normal, 1f / (bodyAngleSmoothing + 1));
        rootBone.localRotation = Quaternion.FromToRotation(Vector3.up, newUp);
        _lastBodyUp = newUp;
    }
    private IEnumerator MoveLegs()
    {
        while (true)
        {
            // Try moving each group of legs
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

    private void OnDrawGizmos()
    {
        Debug.DrawRay(rootBone.position, rootBone.up * 20, Color.green);
    }
}
