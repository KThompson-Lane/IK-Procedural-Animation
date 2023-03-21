using System;
using System.Collections;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    // Target transform
    [SerializeField] Transform target;
    // Head bone transform
    [SerializeField] Transform headBone;

    [SerializeField] float Speed = 1.0f;
    [SerializeField] float LookConstraint = 45.0f;

    //  Array is indexed back-left to front-right, i.e. index 0 is rear left and index 8 is front right 
    [SerializeField] private Stepper[] LegSteppers = new Stepper[8];
    // We do all animation code in LateUpdate
    // This ensures it has the latest object data prior to frame drawing
    private void Awake()
    {
        StartCoroutine(MoveLegs());
    }

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
    IEnumerator MoveLegs()
    {
        while (true)
        {
            // Try moving one diagonal pair of legs
            do
            {
                LegSteppers[0].TryStep();
                LegSteppers[3].TryStep();
                LegSteppers[4].TryStep();
                LegSteppers[7].TryStep();
                // Wait a frame
                yield return null;
      
                // Stay in this loop while either leg is moving.
                // If only one leg in the pair is moving, the calls to TryMove() will let
                // the other leg move if it wants to.
            } while (LegSteppers[0].Moving || LegSteppers[3].Moving || LegSteppers[4].Moving || LegSteppers[7].Moving);

            // Do the same thing for the other diagonal pair
            do
            {
                LegSteppers[1].TryStep();
                LegSteppers[2].TryStep();
                LegSteppers[5].TryStep();
                LegSteppers[6].TryStep();
                yield return null;
            } while (LegSteppers[1].Moving || LegSteppers[2].Moving || LegSteppers[5].Moving || LegSteppers[6].Moving);

        }
    }
}
