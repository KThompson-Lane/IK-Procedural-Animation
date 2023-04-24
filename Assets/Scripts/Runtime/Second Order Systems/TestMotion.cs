using UnityEngine;

namespace Second_Order_Systems
{
    public class TestMotion : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Header("Movement coefficients")] [SerializeField]
        private float fx, zx, rx;

        [Header("Orientation coefficients")] [SerializeField]
        private float fy, zy, ry;

        private SecondOrderMotion<Vector3> _movement, _orientation;

        private void Awake()
        {
            _movement = new SecondOrderVector(fx, zx, rx, transform.position);
            _orientation = new SecondOrderVector(fy, zy, ry, transform.up);
        }

        private void FixedUpdate()
        {
            var currentPosition = transform.position;
            var newPosition = _movement.Update(Time.deltaTime, target.position);
            transform.position = newPosition;

            //  Calculate tilt
            var delta = newPosition - currentPosition;
            var test = Vector3.RotateTowards(transform.up, delta, delta.sqrMagnitude, 0.0f);
            var tilt = _orientation.Update(Time.deltaTime, test);
            Debug.DrawRay(transform.position, tilt.normalized * 5, Color.green);
        }

        private void OnValidate()
        {
            if (_movement == null || _orientation == null)
                return;
            _movement.CalculateKValues(fx, zx, rx);
            _orientation.CalculateKValues(fy, zy, ry);
        }
    }
}