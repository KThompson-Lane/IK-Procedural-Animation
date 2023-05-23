using System;
using UnityEngine;

namespace IK
{
    [Serializable]
    public class Leg : MonoBehaviour
    {
        public Stepper Stepper;
        public int Group;
        public float StepDistance;
        public float overshootAmount;
        public bool Weakened;

        private void OnValidate()
        {
            Stepper.Weakened = Weakened;
        }
    }
}