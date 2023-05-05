using System;
using UnityEngine;

namespace IK
{
    [Serializable]
    public class Leg : MonoBehaviour
    {
        public Stepper Stepper;
        public ChainSolver Solver;
        public int Group;
        public float StepDistance;
        public float overshootAmount;
    }
}