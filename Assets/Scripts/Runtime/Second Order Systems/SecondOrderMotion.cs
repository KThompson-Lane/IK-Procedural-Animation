using UnityEngine;

namespace Runtime.Second_Order_Systems
{
    /// <summary>
    ///     <para>Uses second order time derivatives to create dampening behaviours</para>
    /// </summary>
    public abstract class SecondOrderMotion<T>
    {
        //  Second order coefficients
        protected float K1, K2, K3;
        protected T Output, OutputDelta;
        protected T PreviousInput;
        
        /// <summary>
        ///     <para>Creates a new second order motion providing an initial input</para>
        /// </summary>
        /// <param name="f">frequency of the system</param>
        /// <param name="z">damping coefficient of the system</param>
        /// <param name="r">initial response of the system</param>
        /// <param name="initialInput">initial input to the system</param>
        protected SecondOrderMotion(float f, float z, float r, T initialInput)
        {
            PreviousInput = initialInput;
            Output = initialInput;
            OutputDelta = initialInput;
            CalculateKValues(f, z, r);
        }

        /// <summary>
        ///     <para>
        ///         Updates the second order system to use new values for
        ///         <paramref name="f" />,
        ///         <paramref name="z" />,
        ///         <paramref name="r" />
        ///     </para>
        /// </summary>
        /// <param name="f">frequency of the system</param>
        /// <param name="z">damping coefficient of the system</param>
        /// <param name="r">initial response of the system</param>
        public void CalculateKValues(float f, float z, float r)
        {
            K1 = z / (Mathf.PI * f);
            K2 = 1 / (2 * Mathf.PI * f * (2 * Mathf.PI * f));
            K3 = r * z / (2 * Mathf.PI * f);
        }

        /// <summary>
        ///     <para>
        ///         Updates the second order system producing a new output based on the time-step <paramref name="T" /> and the
        ///         input <paramref name="input" />
        ///     </para>
        /// </summary>
        /// <param name="T">Time-step of the system</param>
        /// <param name="input">Input</param>
        /// <param name="inputDelta">Optional input delta or velocity</param>
        /// <returns>A value representing the input after undergoing motion</returns>
        /// <remarks>
        ///     If <paramref name="inputDelta" /> is not provided, it will be estimated using the previous input and the
        ///     time-step <paramref name="T" />
        /// </remarks>
        public abstract T Update(float T, T input, T inputDelta = default);
    }
}