using UnityEngine;

namespace DefaultNamespace
{
    public class SecondOrderTest : MonoBehaviour
    {
        private Vector3 previousInput;
        private Vector3 yd;
        private float k1, k2, k3;

        //  F corresponds to the frequency of the motion system
        //  Z is the damping coefficient
        //  R initial response of the system.
        public float f, z, r;
        public Transform target;
        private void Awake()
        {
            CalculateKValues();
        }

        private void OnValidate()
        {
            CalculateKValues();
        }

        private void CalculateKValues()
        {
            k1 = z / (Mathf.PI * f);
            k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
            k3 = r * z / (2 * Mathf.PI * f);

            previousInput = target.position;
            yd = Vector3.zero;
        }
        
        private void FixedUpdate()
        {
            Vector3 x = target.position;
            float T = Time.deltaTime;
            Vector3 xd = (x - previousInput) / T;
            previousInput = x;

            Vector3 y = transform.position;
            y = y + T * yd;
            yd = yd + T * (x + k3 * xd - y - k1 * yd) / k2;
            transform.position = y;
        }
    }
}