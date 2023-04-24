using System.Collections;
using System.Collections.Generic;
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
    
    /// <value>frequency of the system</value>
    [Header("Root motion parameters")]
    [SerializeField] private float frequency;
    /// <value>damping coefficient of the system</value>
    [SerializeField] private float damping;
    /// <value>initial response of the system</value>
    [SerializeField] private float initialResponse;
    
    //  Stepper parameters
    [Header("Steppers")]
    [SerializeField] private Transform rootBone;
    [SerializeField] private float bodyHeightOffset;
    public Leg[] Legs;

    //  Motion scripts for the head tracking and root motion
    private LookMotion _headTracker;
    private Locomotion _locomotion;
    
    //  Script for tracking body orientation
    private SecondOrderMotion<Vector3> _rootMotion;

    /// <summary>
    ///     <para>Initialise the head tracking, root motion and leg steppers</para>
    /// </summary>
    private void Start()
    {
        _locomotion = GetComponent<Locomotion>();
        _headTracker = GetComponent<LookMotion>();
        _locomotion.SetTarget(target);
        _headTracker.SetTarget(target);
        foreach (var leg in Legs)
        {
            //  Calculate leg parameters
            //  Step duration is a function of move speed and step distance
            var stepDuration = leg.StepDistance / (_locomotion.MoveSpeed()* 200);
            leg.Stepper.ChangeLegParameters(leg.StepDistance, stepDuration, leg.overshootAmount);
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
        UpdateRootMotion(); 
        if(_locomotion != null)
            _locomotion.Move();
        if(_headTracker != null)
            _headTracker.UpdateLookMotion();

        //  Finally, apply root motion using our leg positions
        
    }

    private void UpdateRootMotion()
    {
        //  Determine body position relative to legs
        //  Calculate the average position of all legs
        var legCount = Legs.Length;
        var averagePosition = Legs
            .Aggregate(Vector3.zero, (current, leg) => current + leg.transform.position) / legCount;
        
        if (Physics.Raycast(averagePosition + Vector3.up * 10, Vector3.down, out var hit, 20))
        {
            averagePosition = hit.point;
        }

        averagePosition += rootBone.up * bodyHeightOffset;
        //  Update our body position
        rootBone.position = _rootMotion.Update(Time.deltaTime, averagePosition);
    }
    
    /// <summary>
    ///     <para>Try and move each group of legs</para>
    /// </summary>
    private IEnumerator MoveLegs()
    {
        while (Legs.Length > 0)
        {
            foreach (var group in Legs.GroupBy(leg => leg.Group))
            {
                foreach (var leg in group)
                    leg.Stepper.TryStep();
                do
                {
                    yield return null;
                } while (group.Any(leg => !leg.Stepper.Grounded));
            }
        }
    }
    
    public void InitialiseSpider()
    {
        DestroyImmediate(GameObject.FindWithTag("Legs"));
        DestroyImmediate(GameObject.FindWithTag("Target"));

        //  Create Target object
        var newTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newTarget.name = "Target";
        newTarget.tag = "Target";

        target = newTarget.transform;
        
        //  Find the child object with the tag "root" to get the root object
        var root = GameObject.FindWithTag("Root");
        rootBone = root.transform;
        
        //  Find all child objects with the tag "feet" to get the feet components
        var feet = GameObject.FindGameObjectsWithTag("Foot");
        
        var legsObj = new GameObject("Legs")
        {
            tag = "Legs",
            transform =
            {
                parent = transform.parent
            }
        };
        var legs = new List<Leg>();
        //  Create legs for feet game objects
        foreach (var foot in feet)
        {
            var legObj = new GameObject("Leg", typeof(Leg))
            {
                transform =
                {
                    parent = legsObj.transform,
                    position = foot.transform.position,
                    rotation = foot.transform.rotation,
                }
            };
            var legComponent = legObj.GetComponent<Leg>();
            var solver = foot.GetComponent<ChainSolver>() ?? foot.AddComponent<ChainSolver>();
            solver.SetLength(3);
            solver.SetTarget(legObj.transform);
            legComponent.Solver = solver;
            var stepper = legObj.AddComponent<Stepper>();
            stepper.root = root.transform.parent;
            legComponent.Stepper = stepper;
            legs.Add(legComponent);
        }
        Legs = legs.ToArray();
        

        
    }
    
}