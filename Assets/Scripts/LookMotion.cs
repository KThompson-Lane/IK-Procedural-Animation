using UnityEngine;

public class LookMotion
{
    private readonly Transform _headBone, _target;
    private float _lookConstraint, _lookSpeed;

    public LookMotion(Transform headBone, Transform target, float lookConstraint, float lookSpeed)
    {
        _headBone = headBone;
        _target = target;
        _lookConstraint = lookConstraint;
        _lookSpeed = lookSpeed;
    }

    public void ChangeMotionParameters(float lookConstraint, float lookSpeed)
    {
        _lookConstraint = lookConstraint;
        _lookSpeed = lookSpeed;
    }
    
    public void UpdateHeadMotion()
    {
        
        //  Current rotation is stored as we are resetting it below
        var currentLocalRotation = _headBone.localRotation;
        //  Reset rotation to zero to correctly transform target vector into local space
        _headBone.localRotation = Quaternion.identity;
        
        var vectorToTarget = _target.position - _headBone.position;
        var localVectorToTarget = _headBone.parent.InverseTransformDirection(vectorToTarget);
        
        
        // Constrain rotation vector
        localVectorToTarget = Vector3.RotateTowards(
            Vector3.forward,
            localVectorToTarget,
            Mathf.Deg2Rad * _lookConstraint, // Convert degrees to radians
            0 // Directional vector magnitude is irrelevant
        );
        
        //  Creates rotation quaternion in local space
        var targetLocalRotation = Quaternion.LookRotation(localVectorToTarget, Vector3.up);

        //  Set the bone local rotation to the smoothed quaternion
        _headBone.localRotation = Quaternion.Slerp(currentLocalRotation,
            targetLocalRotation,
            1 - Mathf.Exp((-_lookSpeed * Time.deltaTime))
        );
    }
}
