using UnityEngine;

namespace Motion
{
    /// <summary>
    ///     <para>Applies smoothing and interpolation to produce a pleasing look motion</para>
    ///     .
    /// </summary>
    public class LookMotion
    {
        //  Transforms for the joint to move and the target of the look motion
        private readonly Transform _lookJoint, _target;

        //  Floats which adjust the angular constraint of the head bone and the speed at which it tracks a target.
        private float _lookConstraint, _lookSpeed;


        /// <summary>
        ///     <para>Creates a new look motion object which tracks a given target</para>
        /// </summary>
        /// <param name="lookJoint">The joint to be animated</param>
        /// <param name="target">The target to be tracked.</param>
        /// <param name="lookConstraint">The angular constraint of the joint in degrees</param>
        /// <param name="lookSpeed">The speed at which the joint moves to track the target</param>
        public LookMotion(Transform lookJoint, Transform target, float lookConstraint, float lookSpeed)
        {
            _lookJoint = lookJoint;
            _target = target;
            _lookConstraint = lookConstraint;
            _lookSpeed = lookSpeed;
        }

        /// <summary>
        ///     <para>Adjusts the motion parameters </para>
        /// </summary>
        /// <param name="lookConstraint">The angular constraint of the joint in degrees</param>
        /// <param name="lookSpeed">The speed at which the joint moves to track the target</param>
        public void ChangeMotionParameters(float lookConstraint, float lookSpeed)
        {
            _lookConstraint = lookConstraint;
            _lookSpeed = lookSpeed;
        }

        /// <summary>
        ///     <para>Updates the joint to track the target, interpolating between the current and target rotation</para>
        /// </summary>
        public void UpdateLookMotion()
        {
            //  Store current local rotation
            var currentRotation = _lookJoint.localRotation;
            //  Reset local rotation to zero for transforming from world to local space
            _lookJoint.localRotation = Quaternion.identity;

            //  Get direction to target in local space
            var toTarget = _target.position - _lookJoint.position;
            var toTargetLocal = _lookJoint.parent.InverseTransformDirection(toTarget);


            // Create rotation vector and constrain it
            toTargetLocal = Vector3.RotateTowards(
                Vector3.forward,
                toTargetLocal,
                Mathf.Deg2Rad * _lookConstraint, // Convert degrees to radians
                0 // Ignore magnitude as it's a directional vector
            );

            //  Create rotation in local space
            var targetRotation = Quaternion.LookRotation(toTargetLocal, Vector3.up);

            //  Set the joint local rotation to the smoothed quaternion
            _lookJoint.localRotation = Quaternion.Slerp(currentRotation,
                targetRotation,
                1 - Mathf.Exp(-_lookSpeed * Time.deltaTime)
            );
        }
    }
}