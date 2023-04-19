using UnityEngine;

namespace DefaultNamespace
{
    public class RootMotion
    {
        private readonly Transform _root, _target;
        private float _moveSpeed, _turnSpeed, _moveAcceleration, _turnAcceleration;
        private float _maxAngleToTarget, _approachDistance, _retreatDistance;
        private float _currentAngularVelocity = 0f;
        private Vector3 _currentVelocity = Vector3.zero;
        private float _targetAngle;
        private Vector3 _projectedDirectionToTarget;
        
        private float fx,zx,rx;
        private float fy,zy,ry;
        private SecondOrderMotion Movement, Orientation;

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


        public void UpdateRootMotion()
        {
            if(Movement == null)
                return;

            UpdateOrientation();
            UpdateTranslation();
        }

        private void UpdateOrientation()
        {
            //  Get the direction toward our target
            var toTarget = _target.position - _root.position;
            
            //  Project our direction vector on the local XZ plane
            _projectedDirectionToTarget = Vector3.ProjectOnPlane(toTarget, _root.up);
            
            //  Calculate the signed angle from our forward direction to our projected target direction
            _targetAngle = Vector3.SignedAngle(_root.forward, _projectedDirectionToTarget, _root.up);

            var angularVelocityTarget = 0f;


            // Check if we're within the maximum allowed angle to our target
            if (Mathf.Abs(_targetAngle) > _maxAngleToTarget)
            {
                //  Convert our angle to positive or negative turn speed (Positive angle is clockwise movement)
                if (_targetAngle > 0)
                {
                    angularVelocityTarget = _turnSpeed;
                }
                // Invert angular speed if target is to our left
                else
                {
                    angularVelocityTarget = -_turnSpeed;
                }
            }

            // smooth the velocity
            _currentAngularVelocity = Mathf.Lerp(
                _currentAngularVelocity,
                angularVelocityTarget,
                1 - Mathf.Exp(-_turnAcceleration * Time.deltaTime)
            );

            // Rotate the transform around the Y axis in world space, 
            // making sure to multiply by delta time to get a consistent angular velocity
            _root.Rotate(0, Time.deltaTime * _currentAngularVelocity, 0, Space.World);
        }

        private void UpdateTranslation()
        {
            Vector3 targetVelocity = Vector3.zero;

            // Don't move if we're facing away from the target, just rotate in place
            if (Mathf.Abs(_targetAngle) < 45)
            {
                float distToTarget = Vector3.Distance(_root.position, _target.position);

                // If we're too far away, approach the target
                if (distToTarget > _approachDistance)
                {
                    targetVelocity = _moveSpeed * _projectedDirectionToTarget.normalized;
                }
                // If we're too close, reverse the direction and move away
                else if (distToTarget < _retreatDistance)
                {
                    targetVelocity = _moveSpeed * -_projectedDirectionToTarget.normalized;
                }
            }
            
            _currentVelocity = Movement.Update(_moveAcceleration * Time.deltaTime, targetVelocity);
            // Apply the velocity
            _root.position += _currentVelocity;
        }
    }
}