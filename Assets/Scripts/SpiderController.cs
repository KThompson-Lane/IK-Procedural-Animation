using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    // Target transform
    [SerializeField] Transform target;
    // Head bone transform
    [SerializeField] Transform headBone;

    [SerializeField] public float Speed = 1.0f;
    [SerializeField] public float HeadTurnConstraint = 45.0f;
    // We do all animation code in LateUpdate
    // This ensures it has the latest object data prior to frame drawing
    void LateUpdate()
    {
        //Current rotation
        Quaternion currentLocalRotation = headBone.localRotation;
        headBone.localRotation = Quaternion.identity;

        Vector3 vectorToTarget = target.position - headBone.position;
        Vector3 localVectorToTarget = headBone.parent.InverseTransformDirection(vectorToTarget);
        
        // Apply angle limit
        localVectorToTarget = Vector3.RotateTowards(
            Vector3.forward,
            localVectorToTarget,
            Mathf.Deg2Rad * HeadTurnConstraint, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
            0 // We don't care about the length here, so we leave it at zero
        );
        Quaternion targetLocalRotation = Quaternion.LookRotation(localVectorToTarget, Vector3.up);

        headBone.localRotation = Quaternion.Slerp(currentLocalRotation,
            targetLocalRotation,
            1 - Mathf.Exp((-Speed * Time.deltaTime))
        );
    }
}
