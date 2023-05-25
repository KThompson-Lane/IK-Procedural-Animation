using System;
using UnityEngine;

namespace Runtime.IK
{
    /// <summary>
    ///     <para>Container class for all components related to creature legs</para>
    /// </summary>
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