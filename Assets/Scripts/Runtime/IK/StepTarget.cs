using UnityEngine;

namespace Runtime.IK
{
    /// <summary>
    ///     <para>Projects the objects transform onto the <c>ground</c> layer</para>
    /// </summary>
    public class StepTarget : MonoBehaviour
    {
        //  Used to set the initial position of the object
        [SerializeField] private Transform startPosition;

        //  Layer mask used as the "ground" layer
        [SerializeField] private LayerMask ground;

        /// <summary>
        ///     <para>Sets the objects position to the specified start position</para>
        /// </summary>
        private void Awake()
        {
            transform.position = startPosition.position;
        }

        /// <summary>
        ///     <para>Uses a raycast to project the objects position onto the <c>ground</c> layer</para>
        /// </summary>
        private void FixedUpdate()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 5, Vector3.down, out var hit, 100, ground))
                transform.position = hit.point;
        }
    }
}