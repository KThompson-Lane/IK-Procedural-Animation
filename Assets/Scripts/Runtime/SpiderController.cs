using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.IK;
using Runtime.Motion;
using Runtime.Second_Order_Systems;
using UnityEngine;

namespace Runtime
{
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
        [SerializeField] private Vector3 rootOffset;
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

            if (Legs.Length < 1)
                throw new Exception("Error, no legs");


            var legGroups = Legs.GroupBy(l => l.Group);
            List<Leg>[] legs = new List<Leg>[legGroups.Count()];
        
            foreach (var group in legGroups)
            {
                legs[group.Key - 1] = group.ToList();
            }
        
            StartCoroutine(MoveLegs(legs));
            _rootMotion = new SecondOrderVector(frequency, damping, initialResponse, rootBone.position);
            //  Calculate body height offset
            rootOffset = rootBone.position;
        }


        /// <summary>
        ///     <para>Update the root motion and head tracking before adjusting the body position and orientation.</para>
        /// </summary>
        /// <remarks>We perform this in late update as it will ensure we have the most recent data</remarks>
        private void LateUpdate()
        {
            if (_locomotion != null)
                _locomotion.Move();
            if (_headTracker != null)
                _headTracker.UpdateLookMotion();
            //  Finally apply root motion
            UpdateRootMotion();
        }

        private void OnValidate()
        {
            if (_rootMotion != null)
                _rootMotion.CalculateKValues(frequency, damping, initialResponse);
        }

        private void UpdateRootMotion()
        {
            //  Determine root bone position relative to legs
        
            //  Calculate the average position of all legs
            Vector3 averagePosition = Vector3.zero;
            foreach (var leg in Legs)
            {
                averagePosition += leg.transform.position;
            }
            averagePosition /= Legs.Length;

            //  Raycast to account for terrain height
            if (Physics.Raycast(averagePosition + Vector3.up * 2, Vector3.down, out var hit, 5))
            {
                averagePosition = hit.point;
            }
            rootBone.position = _rootMotion.Update(Time.deltaTime, averagePosition + rootOffset);

        }
    
        /// <summary>
        ///     <para>Try and move each group of legs</para>
        /// </summary>
        private IEnumerator MoveLegs(List<Leg>[] legs)
        {
            bool moving;
            while (legs.Length > 0)
            {
                for (int i = 0; i < legs.Length; i++)
                {
                    var group = legs[i];
                    foreach (var leg in group)
                        leg.Stepper.TryStep();
                    moving = true;
                    do
                    {
                        yield return null;
                        moving = false;
                        foreach (var leg in group)
                        {
                            if (leg.Stepper.Grounded) continue;
                            moving = true;
                            break;
                        }
                    } while (moving);
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
                var stepper = legObj.AddComponent<Stepper>();
                stepper.root = root.transform.parent;
                legComponent.Stepper = stepper;
                legs.Add(legComponent);
            }
            Legs = legs.ToArray();
        

        
        }
    
    }
}