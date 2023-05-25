namespace Runtime.Second_Order_Systems
{
    public class SecondOrderFloat : SecondOrderMotion<float>
    {
        /// <summary>
        ///     <para>Creates a new second order motion providing an initial input</para>
        /// </summary>
        /// <param name="f">frequency of the system</param>
        /// <param name="z">damping coefficient of the system</param>
        /// <param name="r">initial response of the system</param>
        /// <param name="initialInput">initial input to the system</param>
        public SecondOrderFloat(float f, float z, float r, float initialInput) : base(f, z, r, initialInput){}

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
        public override float Update(float T, float input, float inputDelta = 0)
        {
            if (inputDelta == 0)
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