using UnityEngine;

namespace Second_Order_Systems
{
    public class SecondOrderVector : SecondOrderMotion<Vector3>
    {
        /// <summary>
        ///     <para>Creates a new second order motion providing an initial input</para>
        /// </summary>
        /// <param name="f">frequency of the system</param>
        /// <param name="z">damping coefficient of the system</param>
        /// <param name="r">initial response of the system</param>
        /// <param name="initialInput">initial input to the system</param>
        public SecondOrderVector(float f, float z, float r, Vector3 initialInput) : base(f, z, r, initialInput){}

        /// <summary>
        ///     <para>
        ///         Updates the second order system producing a new output based on the time-step <paramref name="T" /> and the
        ///         input <paramref name="input" />
        ///     </para>
        /// </summary>
        /// <param name="T">Time-step of the system</param>
        /// <param name="input">Input vector</param>
        /// <param name="inputDelta">Optional input delta or velocity</param>
        /// <returns>A vector representing the input after undergoing motion</returns>
        /// <remarks>
        ///     If <paramref name="inputDelta" /> is not provided, it will be estimated using the previous input and the
        ///     time-step <paramref name="T" />
        /// </remarks>
        public override Vector3 Update(float T, Vector3 input, Vector3 inputDelta = default)
        {
            if (inputDelta == default)
            {
                inputDelta = (input - PreviousInput) / T;
                PreviousInput = input;
            }

            Output = Output + T * OutputDelta;
            OutputDelta = OutputDelta + T * (input + K3 * inputDelta - Output - K1 * OutputDelta) / K2;
            return Output;
        }
    }
}