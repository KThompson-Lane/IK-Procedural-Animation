using UnityEngine;

namespace DefaultNamespace
{
    /// <summary>
    ///     <para>Uses second order time derivatives to create dampening behaviours</para>.
    /// </summary>
    public class SecondOrderMotion
    {
        private Vector3 previousInput;
        private Vector3 output, outputDelta;
        
        //  Second order coefficients
        private float k1, k2, k3;
        
        
        /// <summary>
        ///     <para>Creates a new second order motion providing an initial input</para>
        /// </summary>
        /// <param name="f">frequency of the system</param>
        /// <param name="z">damping coefficient of the system</param>
        /// <param name="r">initial response of the system</param>
        /// <param name="initialInput">initial input to the system</param>
        public SecondOrderMotion(float f, float z, float r, Vector3 initialInput)
        {
            previousInput = initialInput;
            output = initialInput;
            outputDelta = initialInput;
            CalculateKValues(f,z,r);
        }
        
        /// <summary>
        ///     <para>Updates the second order system to use new values for
        ///         <paramref name="f"/>,
        ///         <paramref name="z"/>,
        ///         <paramref name="r"/>
        ///     </para>
        /// </summary>
        /// <param name="f">frequency of the system</param>
        /// <param name="z">damping coefficient of the system</param>
        /// <param name="r">initial response of the system</param>
        public void CalculateKValues(float f, float z, float r)
        {
            k1 = z / (Mathf.PI * f);
            k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
            k3 = r * z / (2 * Mathf.PI * f);
        }

        
        /// <summary>
        ///     <para>Updates the second order system producing a new output based on the time-step <paramref name="T"/> and the input <paramref name="input"/></para>
        /// </summary>
        /// <param name="T">Time-step of the system</param>
        /// <param name="input">Input vector</param>
        /// <param name="inputDelta">Optional input delta or velocity</param>
        /// <returns>A vector representing the input after undergoing motion</returns>
        /// <remarks>If <paramref name="inputDelta"/> is not provided, it will be estimated using the previous input and the time-step <paramref name="T"/></remarks>
        public Vector3 Update(float T, Vector3 input, Vector3 inputDelta = default)
        {
            if (inputDelta == default)
            {
                inputDelta = (input - previousInput) / T;
                previousInput = input;
            }

            output = output + T * outputDelta;
            outputDelta = outputDelta + T * (input + k3 * inputDelta - output - k1 * outputDelta) / k2;
            return output;
        }
    }
}