using System;
using System.Collections;
using UnityEngine;

namespace IK
{
    /// <summary>
    ///     <para>A class which will attempt to take a step when it surpasses a set distance from it's home position</para>
    /// </summary>
    public class Stepper : MonoBehaviour
    {
        public Transform root;
        private Vector3 _footOffset; 
        public LayerMask groundLayer;

        private Vector3 _homeLocation;
        
        //  Is this stepper weakened
        [SerializeField] private bool weakened;
        //  Whether it is currently taking a step
        private bool _moving;

        //  If we surpass this distance, step towards home
        private float _stepDistance;

        //  Time it takes to complete a step 
        private float _stepDuration;

        //  Amount to overshoot the home position by
        private float _stepOvershoot;

        //  Is it grounded
        /// <summary>
        ///     Boolean which represents if the leg is grounded
        /// </summary>
        public bool Grounded => !_moving;

        private void Awake()
        {
            _footOffset = transform.position - root.position;
            groundLayer = LayerMask.NameToLayer("Ground");
        }

        private void LateUpdate()
        {
            //  Update home position
            //  Calculate home position relative to root transform
            _homeLocation = root.position + (root.TransformDirection(_footOffset.normalized) * _footOffset.magnitude);
            //  Raycast home position onto ground
            if (Physics.Raycast(_homeLocation + Vector3.up * 10, Vector3.down, out var hit, 20, ~groundLayer))
            {
                _homeLocation = hit.point;
            }

        }

        /// <summary>
        ///     <para>Debug function for drawing each steppers current position and their home position</para>
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(_homeLocation, _stepDistance);
        }

        /// <summary>
        ///     <para>Attempt to take a step if not moving and far enough away from home position</para>
        /// </summary>
        public void TryStep()
        {
            //  Do nothing if we're already taking a step
            if (_moving) return;
            
            //  Check if we can take a step

            //  First check if we're outside of our home location
            if (Vector3.Distance(transform.position, _homeLocation) > (!weakened ? _stepDistance : _stepDistance / 2))
            {
                var toTarget = _homeLocation - transform.position;
                var overshotAmount = _stepDistance * _stepOvershoot;
                var overshootVector = toTarget * overshotAmount;
                overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);
                
                //  Apply our overshoot vector to our home position to calculate our step target
                var stepTarget = _homeLocation + overshootVector;
                
                StartCoroutine(TakeStep(transform.position, stepTarget));
                
            }
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
        private IEnumerator TakeStep(Vector3 initialPosition, Vector3 targetPosition)
        {
            //  Indicate a step has started
            _moving = true;

            //  Calculate centre point
            var stepCentre = (transform.position + targetPosition) / 2;

            //  Raise centre point slightly to give the step some lift
            stepCentre += Vector3.up * Vector3.Distance(initialPosition, targetPosition) / 2f;

            // Time since step started
            var timeElapsed = 0f;

            //  Take a step using a do-while loop
            do
            {
                // Increment time elapsed with deltaTime
                timeElapsed += Time.deltaTime;

                //  Calculate time-step using the step duration and our total time elapsed
                var T = timeElapsed / _stepDuration;
                // Interpolate transform bi-linearly using nested Lerps
                transform.position =
                    Vector3.Lerp(
                        Vector3.Lerp(initialPosition, stepCentre, T),
                        Vector3.Lerp(stepCentre, targetPosition, T),
                        T
                    );

                // Wait for one frame
                yield return null;
            } while (timeElapsed < _stepDuration);

            // Indicate the step has finished
            _moving = false;
        }
    }
}