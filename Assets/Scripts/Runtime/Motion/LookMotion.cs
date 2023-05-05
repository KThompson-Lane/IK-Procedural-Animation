using UnityEngine;

namespace Motion
{
    /// <summary>
    ///     <para>Applies smoothing and interpolation to produce a pleasing look motion</para>
    /// </summary>
    public class LookMotion : MonoBehaviour
    {
        /// <value>joint to transform</value>
        [SerializeField] private Transform lookJoint;
        /// <value>target to track</value>
        [SerializeField] private Transform target;
        public void SetTarget(Transform newTarget) => target = newTarget;
        /// <value>speed that it tracks the <c>target</c></value>
        [SerializeField] private float lookSpeed;

        /// <value>angular constraint of the <c>lookJoint</c></value>
        [SerializeField] private float lookConstraint;

        /// <summary>
        ///     <para>Updates the joint to track the target, interpolating between the current and target rotation</para>
        /// </summary>
        public void UpdateLookMotion()
        {
            //  Store current local rotation
            var currentRotation = lookJoint.localRotation;
            //  Reset local rotation to zero for transforming from world to local space
            lookJoint.localRotation = Quaternion.identity;

            //  Get direction to target in local space
            var toTarget = target.position - lookJoint.position;
            var toTargetLocal = lookJoint.parent.InverseTransformDirection(toTarget);


            // Create rotation vector and constrain it
            toTargetLocal = Vector3.RotateTowards(
                Vector3.forward,
                toTargetLocal,
                Mathf.Deg2Rad * lookConstraint, // Convert degrees to radians
                0 // Ignore magnitude as it's a directional vector
            );

            //  Create rotation in local space
            var targetRotation = Quaternion.LookRotation(toTargetLocal, Vector3.up);

            //  Set the joint local rotation to the smoothed quaternion
            lookJoint.localRotation = Quaternion.Slerp(currentRotation,
                targetRotation,
                1 - Mathf.Exp(-lookSpeed * Time.deltaTime)
            );
        }
    }
}