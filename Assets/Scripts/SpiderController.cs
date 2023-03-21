using UnityEngine;

public class SpiderController : MonoBehaviour
{
    // Target transform
    [SerializeField] Transform target;
    // Head bone transform
    [SerializeField] Transform headBone;

    [SerializeField] float Speed = 1.0f;
    [SerializeField] float LookConstraint = 45.0f;
    // We do all animation code in LateUpdate
    // This ensures it has the latest object data prior to frame drawing
    void LateUpdate()
    {
        //  Current rotation is stored as we are resetting it below
        var currentLocalRotation = headBone.localRotation;
        //  Reset rotation to zero to correctly transform target vector into local space
        headBone.localRotation = Quaternion.identity;
        
        var vectorToTarget = target.position - headBone.position;
        var localVectorToTarget = headBone.parent.InverseTransformDirection(vectorToTarget);
        
        
        // Constrain rotation vector
        localVectorToTarget = Vector3.RotateTowards(
            Vector3.forward,
            localVectorToTarget,
            Mathf.Deg2Rad * LookConstraint, // Convert degrees to radians
            0 // Directional vector magnitude is irrelevant
        );
        
        //  Creates rotation quaternion in local space
        var targetLocalRotation = Quaternion.LookRotation(localVectorToTarget, Vector3.up);

        //  Set the bone local rotation to the smoothed quaternion
        headBone.localRotation = Quaternion.Slerp(currentLocalRotation,
            targetLocalRotation,
            1 - Mathf.Exp((-Speed * Time.deltaTime))
        );
    }
}
