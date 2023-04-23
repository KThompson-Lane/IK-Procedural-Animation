using System;
using System.Collections;
using System.Linq;
using IK;
using Motion;
using Second_Order_Systems;
using UnityEngine;

/// <summary>
///     <para>This class is responsible for controlling the spider creature</para>
/// </summary>
public class SpiderController : MonoBehaviour
{
    //  Spider target transform
    [Header("Target")] [SerializeField] private Transform target;

    //  Stepper parameters
    [Header("Steppers")]
    //  height offset for the body relative to the feet joints
    [SerializeField]
    private float bodyHeightOffset = 2.75f;

    [SerializeField] private Transform rootBone;

    //  Array of leg groups
    [SerializeField] private LegGroup[] legGroups;

    //  Corner legs used for body orientation
    [SerializeField] private Transform[] corners;

    //  Motion scripts for the head tracking and root motion
    private LookMotion _headTracker;
    private Locomotion _locomotion;
    
    /// <value>frequency of the system</value>
    [Header("Root motion parameters")]
    [SerializeField] private float frequency;
    /// <value>damping coefficient of the system</value>
    [SerializeField] private float damping;
    /// <value>initial response of the system</value>
    [SerializeField] private float initialResponse;
    
    //  Script for tracking body orientation
    private SecondOrderMotion<Vector3> _rootMotion;
    /// <summary>
    ///     <para>Initialise the head tracking, root motion and leg steppers</para>
    /// </summary>
    private void Awake()
    {
        _locomotion = GetComponent<Locomotion>();
        _headTracker = GetComponent<LookMotion>();
        foreach (var group in legGroups)
        {
            //  Calculate leg parameters
            //  Step duration is a function of move speed and step distance
            var stepDuration = group.stepDistance / (_locomotion.MoveSpeed() * 200);
            group.legs.ToList().ForEach(leg =>
                leg.ChangeLegParameters(group.stepDistance, stepDuration, group.overshootAmount));
        }

        StartCoroutine(MoveLegs());
        _rootMotion = new SecondOrderVector(frequency, damping, initialResponse, rootBone.position);
    }


    /// <summary>
    ///     <para>Update the root motion and head tracking before adjusting the body position and orientation.</para>
    /// </summary>
    /// <remarks>We perform this in late update as it will ensure we have the most recent data</remarks>
    private void LateUpdate()
    {
        if(_locomotion != null)
            _locomotion.Move();
        if(_headTracker != null)
            _headTracker.UpdateLookMotion();

        //  Finally, apply root motion using our leg positions
        UpdateRootMotion();
    }

    private void UpdateRootMotion()
    {
        
        //  Determine body position relative to legs
        //  Calculate the average position of all legs
        var legCount = legGroups.SelectMany(group => group.legs).Count();
        var averagePosition = legGroups.SelectMany(group => group.legs)
            .Aggregate(Vector3.zero, (current, leg) => current + leg.transform.position) / legCount;
        
        //  Raise the end position by our body heightOffset
        var endPosition = averagePosition + new Vector3(0, bodyHeightOffset);
        
        //  Update our body position
        rootBone.position = _rootMotion.Update(Time.deltaTime, endPosition);
        
        //  Orient body based on corner leg positions
        var v1 = corners[0].position - corners[1].position;
        var v2 = corners[2].position - corners[3].position;
        //  Calculate the cross product of the vectors between diagonally opposed legs
        var normal = Vector3.Cross(v1, v2).normalized;
        
        //  Interpolate the vector to smooth it and apply it to our local rotation
        //var newUp = _bodyOrientation.Update(Time.deltaTime, normal);
        rootBone.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
    }

    /// <summary>
    ///     <para>Debug method for drawing the body orientation</para>
    /// </summary>
    private void OnDrawGizmos()
    {
        Debug.DrawRay(rootBone.position, rootBone.up * 20, Color.green);
    }

    /// <summary>
    ///     <para>Try and move each group of legs</para>
    /// </summary>
    private IEnumerator MoveLegs()
    {
        while (true)
            foreach (var group in legGroups)
            {
                foreach (var leg in group.legs) leg.TryStep();
                do
                {
                    yield return null;
                } while (group.legs.Any(leg => !leg.Grounded));
            }
    }


    /// <summary>
    ///     A struct representing groups of legs that move in parallel
    /// </summary>
    [Serializable]
    public struct LegGroup
    {
        /// <value>The legs which the group contains</value>
        public Stepper[] legs;

        /// <value>The group default step distance</value>
        public float stepDistance;

        /// <value>The group default step overshoot amount</value>
        [Range(0.0f, 1.0f)] public float overshootAmount;
    }
}