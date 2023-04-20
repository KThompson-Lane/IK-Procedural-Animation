using UnityEngine;

namespace DefaultNamespace
{
    /// <summary>
    ///     <para>Applies root motion to an object to move it towards a target</para>
    /// </summary>
    public class RootMotion
    {
        //  transforms for the root and target
        private readonly Transform _root, _target;
        
        //  parameters for movement and turning 
        private float _moveSpeed, _turnSpeed, _moveAcceleration, _turnAcceleration;
        
        //  parameters for constraining movement
        private float _maxAngleToTarget, _approachDistance, _retreatDistance;
        
        private float _currentAngularVelocity = 0f;
        private Vector3 _currentVelocity = Vector3.zero;
        
        private float _targetAngle;
        private Vector3 _toTargetProjected;
        
        private float fx,zx,rx;
        private float fy,zy,ry;
        private SecondOrderMotion Movement, Orientation;
        
        /// <summary>
        ///     <para>Constructs a new root motion</para>
        /// </summary>
        /// <param name="root">root transform to move</param>
        /// <param name="target">target transform</param>
        /// <param name="moveSpeed">max movement speed</param>
        /// <param name="turnSpeed">max turn speed</param>
        /// <param name="maxAngleToTarget">maximum allowed angle to the target in degrees before rotating to realign</param>
        /// <param name="moveAcceleration">movement acceleration</param>
        /// <param name="turnAcceleration">turn acceleration</param>
        /// <param name="approachDistance">minimum distance before moving to approach the target</param>
        /// <param name="retreatDistance">minimum distance before retreating from target</param>
        public RootMotion(Transform root, Transform target, float moveSpeed, float turnSpeed, float maxAngleToTarget, float moveAcceleration = 1.0f,float turnAcceleration = 1.0f, float approachDistance = 1.0f, float retreatDistance = 1.0f)
        {
            _root = root;
            _target = target;
            _moveSpeed = moveSpeed * 0.01f;
            _turnSpeed = turnSpeed;
            _moveAcceleration = moveAcceleration;
            _turnAcceleration = turnAcceleration;
            _maxAngleToTarget = maxAngleToTarget;
            _approachDistance = approachDistance;
            _retreatDistance = retreatDistance;
        }
        /// <summary>
        ///     <para>Updates root motion parameter values</para>
        /// </summary>
        /// <param name="moveSpeed">max movement speed</param>
        /// <param name="turnSpeed">max turn speed</param>
        /// <param name="maxAngleToTarget">maximum allowed angle to the target in degrees before rotating to realign</param>
        /// <param name="moveAcceleration">movement acceleration</param>
        /// <param name="turnAcceleration">turn acceleration</param>
        /// <param name="approachDistance">minimum distance before moving to approach the target</param>
        /// <param name="retreatDistance">minimum distance before retreating from target</param>
        public void ChangeMotionParameters(float moveSpeed, float turnSpeed, float maxAngleToTarget,
            float moveAcceleration = 1.0f, float turnAcceleration = 1.0f, float approachDistance = 1.0f,
            float retreatDistance = 1.0f)
        {
            _moveSpeed = moveSpeed * 0.01f;
            _turnSpeed = turnSpeed;
            _moveAcceleration = moveAcceleration;
            _turnAcceleration = turnAcceleration;
            _maxAngleToTarget = maxAngleToTarget;
            _approachDistance = approachDistance;
            _retreatDistance = retreatDistance;
        }
        
        public void SetMovementCoefficients(float f, float z, float r)
        {
            fx = f;
            zx = z;
            rx = r;
            Movement = new SecondOrderMotion(fx, zx, rx, Vector3.zero);
        }

        /// <summary>
        ///     <para>Updates the root orientation and translation based on the target</para>
        /// </summary>
        public void UpdateRootMotion()
        {
            if(Movement == null)
                return;

            UpdateOrientation();
            UpdateTranslation();
        }
        /// <summary>
        ///     <para>Updates the root transform orientation to face the target</para>
        /// </summary>
        private void UpdateOrientation()
        {
            //  Get the direction toward our target
            var toTarget = _target.position - _root.position;
            
            //  Project our direction vector on the local XZ plane
            _toTargetProjected = Vector3.ProjectOnPlane(toTarget, _root.up);
            
            //  Calculate the angle from our forward direction to our projected target direction
            _targetAngle = Vector3.SignedAngle(_root.forward, _toTargetProjected, _root.up);

            //  Reset our target angular velocity
            var targetAngularVelocity = 0f;


            // Check if we need to rotate to face our target
            if (Mathf.Abs(_targetAngle) > _maxAngleToTarget)
            {
                //  Convert our angle to positive or negative turn speed (Positive angle is clockwise movement)
                targetAngularVelocity = (_targetAngle > 0) ? _turnSpeed : -_turnSpeed;
            }

            // Linearly interpolate between our current angular velocity and our target velocity
            _currentAngularVelocity = Mathf.Lerp(
                _currentAngularVelocity,
                targetAngularVelocity,
                1 - Mathf.Exp(-_turnAcceleration * Time.deltaTime)
            );
            
            // Rotate around the world Y axis to face our target
            _root.Rotate(0, Time.deltaTime * _currentAngularVelocity, 0, Space.World);
        }
        
        /// <summary>
        ///     <para>Updates the root transform position to move towards the target</para>
        /// </summary>
        private void UpdateTranslation()
        {
            //  Reset our target velocity
            var targetVelocity = Vector3.zero;
            
            //  Get the direction toward our target
            var toTarget = _target.position - _root.position;
            //  Project our direction vector on the local XZ plane
            var toTargetProjected = Vector3.ProjectOnPlane(toTarget, _root.up);

            // Ensure we're facing the target prior to moving
            if (Mathf.Abs(_targetAngle) < 45)
            {
                var targetDistance = Vector3.Distance(_root.position, _target.position);

                // Use our approach and retreat distances to set our target velocity
                targetVelocity = _moveSpeed * ((targetDistance > _approachDistance) ? _toTargetProjected :
                    (targetDistance <= _retreatDistance) ? -_toTargetProjected : Vector3.zero).normalized;
            }
            //  Update our velocity using our second order system
            _currentVelocity = Movement.Update(_moveAcceleration * Time.deltaTime, targetVelocity);
            // Apply the velocity
            _root.position += _currentVelocity;
        }
    }
}