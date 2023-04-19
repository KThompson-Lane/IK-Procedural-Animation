using UnityEngine;

namespace DefaultNamespace
{
    public class SecondOrderMotion
    {
        private Vector3 previousInput;
        private Vector3 output, previousOutput;
        private float k1, k2, k3;
        
        public SecondOrderMotion(float f, float z, float r, Vector3 initialInput)
        {
            previousInput = initialInput;
            output = initialInput;
            previousOutput = initialInput;
            //  F corresponds to the frequency of the motion system
            //  Z is the damping coefficient
            //  R initial response of the system.
            CalculateKValues(f,z,r);
        }

        public void CalculateKValues(float f, float z, float r)
        {
            k1 = z / (Mathf.PI * f);
            k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
            k3 = r * z / (2 * Mathf.PI * f);
        }

        public Vector3 Update(float T, Vector3 x)
        {
            Vector3 xd = (x - previousInput) / T;
            previousInput = x;

            output = output + T * previousOutput;
            previousOutput = previousOutput + T * (x + k3 * xd - output - k1 * previousOutput) / k2;
            return output;
        }
    }
}