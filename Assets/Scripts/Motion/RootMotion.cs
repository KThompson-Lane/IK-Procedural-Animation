using Second_Order_Systems;
using UnityEngine;
using UnityEngine.Serialization;

namespace Motion
{
    /// <summary>
    ///     <para>Applies root motion to an object to move it towards a target</para>
    /// </summary>
    public class RootMotion : MonoBehaviour
    {
        /// <value>joint to transform</value>
        [SerializeField] private Transform root;
        /// <value>target to track</value>
        [SerializeField] private Transform target;
        
        /// <value>maximum allowed angle to the target in degrees before rotating to realign</value>
        [Header("Motion parameters")]
        [SerializeField] private float maxAngleToTarget;
        /// <value>minimum distance before moving to approach the target</value>
        [SerializeField] private float approachDistance;
        /// <value>minimum distance before retreating from target</value>
        [SerializeField] private float retreatDistance;
        /// <value>max movement speed</value>
        [SerializeField] private float moveSpeed;
        /// <value>max turn speed</value>
        [SerializeField] private float turnSpeed;
        /// <value>turn acceleration</value>
        [SerializeField] private float turnAcceleration;
        
        /// <value>frequency of the system</value>
        [Header("Movement parameters")]
        [SerializeField] private float acceleration;
        /// <value>damping coefficient of the system</value>
        [SerializeField] private float dampening;
        /// <value>initial response of the system</value>
        [SerializeField] private float response;
        
        /// <summary>
        /// Gets the root motion move speed value
        /// </summary>
        /// <returns>max movement speed</returns>
        public float MoveSpeed() => moveSpeed;
        
        
        private float _currentAngularVelocity;
        private Vector3 _currentVelocity = Vector3.zero;
        private SecondOrderMotion _movement;

        private void Start()
        {
            _movement = new SecondOrderMotion(acceleration, dampening, response, Vector3.zero);
        }

        /// <summary>
        ///     <para>Updates the root orientation and translation based on the target</para>
        /// </summary>
        public void UpdateRootMotion()
        {
            //  Reset velocities 
            var targetAngularVelocity = 0f;
            var targetVelocity = Vector3.zero;
            
            //  Get the direction toward our target
            var toTarget = target.position - root.position;

            //  Project our direction vector on the local XZ plane
            var toTargetProjected = Vector3.ProjectOnPlane(toTarget, root.up);

            //  Calculate the angle from our forward direction to our projected target direction
            var targetAngle = Vector3.SignedAngle(root.forward, toTargetProjected, root.up);

            // Check if we need to rotate to face our target
            if (Mathf.Abs(targetAngle) > maxAngleToTarget)
                //  Convert our angle to positive or negative turn speed (Positive angle is clockwise movement)
                targetAngularVelocity = targetAngle > 0 ? turnSpeed : -turnSpeed;

            
            //  TODO: Replace this with another second order motion system
            //  Linearly interpolate between our current angular velocity and our target velocity
            _currentAngularVelocity = Mathf.Lerp(
                _currentAngularVelocity,
                targetAngularVelocity,
                1 - Mathf.Exp(-turnAcceleration * Time.deltaTime)
            );

            //  Rotate around the global Y axis to face our target
            root.Rotate(0, Time.deltaTime * _currentAngularVelocity, 0, Space.World);
            
            //  Ensure we're facing the target before moving
            if (Mathf.Abs(targetAngle) < 45)
            {
                var targetDistance = Vector3.Distance(root.position, Vector3.ProjectOnPlane(target.position, root.up));

                //  Use our approach and retreat distances to set our target velocity
                targetVelocity = moveSpeed * (targetDistance > approachDistance ? toTargetProjected :
                    targetDistance <= retreatDistance ? -toTargetProjected : Vector3.zero).normalized;
            }
            
            //  Update our velocity using our second order system
            _currentVelocity = _movement.Update(Time.deltaTime, targetVelocity);
            //  Apply the velocity
            root.position += _currentVelocity;
        }
    }
}