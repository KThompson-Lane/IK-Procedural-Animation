using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ChainSolver : MonoBehaviour
{
    //  Default is two-bone IK
    public int ChainLength = 2;
    
    //  Target and pole transforms
    public Transform Target;
    //public Transform Pole;
    
    //  Max solver iterations
    public int MaxIterations = 10;
    
    //  Distance to stop solving
    public float Delta = 0.001f;

    
    //  Arrays for solving
    protected float[] BoneLengths; //  Target to origin
    protected float CompleteLength;
    protected Transform[] Bones;
    protected Vector3[] Positions;
    
    private void Awake()
    {
        Init();
    }

    void Init()
    {
        //  An N length IK chain will have N + 1 bone transforms.
        Bones = new Transform[ChainLength + 1];
        Positions = new Vector3[ChainLength + 1];
        BoneLengths = new float[ChainLength];

        CompleteLength = 0.0f;

        var currentBone = transform;
        
        //  Iterate over joint chain calculating the individual bone lengths and complete chain length.
        for (var i = ChainLength; i >= 0; i--)
        {
            Bones[i] = currentBone;

            if (i < ChainLength)
            {
                var boneLength = (Bones[i + 1].position - currentBone.position).magnitude;
                BoneLengths[i] = boneLength;
                CompleteLength += boneLength;
            }
            currentBone = currentBone.parent;
        }

    }

    private void LateUpdate()
    {
        SolveIK();
    }

    private void SolveIK()
    {
        if (Target is null)
            return;
        if (BoneLengths.Length != ChainLength)
        {
            Init();
        }

        Positions = Bones.Select(bone => bone.position).ToArray();
        
        //  Calculate chain positions
        
        //  First check if target position is further than total length of chain
        var rootToTarget = Target.position - Positions[0];
        if (rootToTarget.sqrMagnitude >= CompleteLength * CompleteLength)
        {
            //  Stretch chain out as far as possible
            var direction = rootToTarget.normalized;
            //  Calculate each bone position (except root)
            for (int i = 1; i < Positions.Length; i++)
            {
                //  Position of parent bone, plus direction times by length of parent to current transform.
                Positions[i] = Positions[i - 1] + direction * BoneLengths[i - 1];
            }
        }
        //  Otherwise start solving
        else
        {
            for (var iteration = 0; iteration < MaxIterations; iteration++)
            {
                //  Backward IK
                for (var i = ChainLength; i > 0; i--)
                {
                    if (i == ChainLength)
                        Positions[i] = Target.position;
                    else
                    {
                        //  Position of parent bone, plus direction from parent to current bone, times by length of current bone
                        Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BoneLengths[i];
                    }
                }
                
                //  Forward IK
                for (int i = 1; i < Positions.Length; i++)
                {
                    //  Position of child bone, plus direction from child to current bone, times by length of child bone
                    Positions[i] = Positions[i-1] + (Positions[i] - Positions[i - 1]).normalized * BoneLengths[i-1];
                }

                if ((Positions[^1] - Target.position).sqrMagnitude < Delta * Delta)
                    break;
            }
        }
        
        //  Set bone positions to new solved positions
        for (var i = 0; i < Positions.Length; i++)
        {
            Bones[i].position = Positions[i];
        }

    }

    private void OnDrawGizmos()
    {
        var currentTransform = transform;
        for (var i = 0; i < ChainLength && currentTransform.parent != null; i++)
        {
            var parentPosition = currentTransform.parent.position;
            var currentPosition = currentTransform.position;
            var scale = Vector3.Distance(currentPosition, parentPosition) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(currentPosition, Quaternion.FromToRotation(Vector3.up, parentPosition - currentPosition), new Vector3(scale, Vector3.Distance(parentPosition, currentPosition), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            currentTransform = currentTransform.parent;
        }
    }
}
