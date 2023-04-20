using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IK
{
    /// <summary>
    ///     <para>An IK chain solver which implements the FABRIK method</para>
    /// </summary>
    public class ChainSolver : MonoBehaviour
    {
        //  Length of the kinematic chain
        [SerializeField] private int chainLength = 2;

        //  Target and pole transforms
        [SerializeField] private Transform target;
        [SerializeField] private Transform pole;

        //  Max solver iterations
        [SerializeField] private int maxIterations = 10;

        //  Distance to stop solving
        [SerializeField] private float delta = 0.001f;

        //  Lengths of each bone (i.e. distance from parent joint to child joint)
        private float[] _boneLengths;

        //  total length of the joint chain
        private float _completeLength;

        //  each joint transform
        private Transform[] _joints;

        //  joint positions
        private Vector3[] _positions;

        //  initial bone directions
        private Vector3[] _startDirections;

        //  initial joint rotations
        private Quaternion[] _startRotations;

        //  initial rotations for root and target
        private Quaternion _startRotationTarget, _startRotationRoot;

        /// <summary>
        ///     <para>Calls init on awake</para>
        /// </summary>
        private void Awake()
        {
            Init();
        }

        /// <summary>
        ///     <para>Solve the kinematic chain in late update after all other updates are completed</para>
        /// </summary>
        private void LateUpdate()
        {
            SolveIK();
        }

        /// <summary>
        ///     <para>Debug method to draw bone gizmos</para>
        /// </summary>
        private void OnDrawGizmos()
        {
            var currentTransform = transform;

            //  Traverse joint chain drawing bones as wire cubes
            for (var i = 0; i < chainLength && currentTransform.parent != null; i++)
            {
                var parentPosition = currentTransform.parent.position;
                var currentPosition = currentTransform.position;
                var scale = Vector3.Distance(currentPosition, parentPosition) * 0.1f;
                Handles.matrix = Matrix4x4.TRS(currentPosition,
                    Quaternion.FromToRotation(Vector3.up, parentPosition - currentPosition),
                    new Vector3(scale, Vector3.Distance(parentPosition, currentPosition), scale));
                Handles.color = Color.green;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                currentTransform = currentTransform.parent;
            }
        }

        /// <summary>
        ///     <para>Initialises the solver storing the joint positions, bone lengths and initial directions and rotations</para>
        /// </summary>
        private void Init()
        {
            //  An N length IK chain will have N + 1 joint transforms.
            _joints = new Transform[chainLength + 1];
            _boneLengths = new float[chainLength];
            _startDirections = new Vector3[chainLength + 1];
            _startRotations = new Quaternion[chainLength + 1];

            if (target is null)
            {
                Debug.LogError("IK Target game object is null!");
                return;
            }

            //  Initialise target rotation
            _startRotationTarget = target.rotation;

            _completeLength = 0.0f;

            //  The chain solver is attached to the end-effector or leaf-joint
            var currentJoint = transform;

            //  Traverse joint chain starting at end-effector calculating the individual bone lengths and complete chain length.
            for (var i = chainLength; i >= 0; i--)
            {
                _joints[i] = currentJoint;
                _startRotations[i] = currentJoint.rotation;
                //  Check if current joint is end effector
                if (i == chainLength)
                {
                    _startDirections[i] = target.position - currentJoint.position;
                }
                else
                {
                    //  Calculate the vector between the current joint and it's child
                    var boneVector = _joints[i + 1].position - currentJoint.position;
                    _startDirections[i] = boneVector.normalized;
                    //  Calculate bone length and add it to the total length
                    var boneLength = boneVector.magnitude;
                    _boneLengths[i] = boneLength;
                    _completeLength += boneLength;
                }

                //  Set current bone to parent bone
                currentJoint = currentJoint.parent;
            }
        }

        /// <summary>
        ///     <para>Solve each joint transform to move the end-effector to the target position using the FABRIK method</para>
        /// </summary>
        private void SolveIK()
        {
            if (target is null)
                return;
            //  Initialise if not already
            if (_boneLengths.Length != chainLength)
                Init();

            //Initialise positions array by mapping joint positions to array
            _positions = _joints.Select(joint => joint.position).ToArray();

            //  First check if target position is further than total length of chain
            var rootToTarget = target.position - _positions[0];
            //  Using the square magnitude is more performant
            if (rootToTarget.sqrMagnitude >= _completeLength * _completeLength)
            {
                //  Stretch chain out as far as possible
                var direction = rootToTarget.normalized;
                //  Traverse joint chain starting at the child of the root joint
                for (var i = 1; i < _positions.Length; i++)
                    //  new position is parent joint position + bone length in the direction of the target
                    _positions[i] = _positions[i - 1] + direction * _boneLengths[i - 1];
            }
            //  Otherwise begin solving
            else
            {
                for (var iteration = 0; iteration < maxIterations; iteration++)
                {
                    //  Backward step

                    //  First set end effector to target position
                    _positions[chainLength] = target.position;

                    //  Then traverse joint chain starting at the end-effector parent
                    for (var i = chainLength - 1; i > 0; i--)
                    {
                        //  Direction of child joint to current joint
                        var childToCurrent = (_positions[i] - _positions[i + 1]).normalized;
                        //  Set position to child joint plus bone length in the direction of the child to this joint.
                        _positions[i] = _positions[i + 1] + childToCurrent * _boneLengths[i];
                    }

                    //  Forward step
                    //  Traverse the joint chain starting at root joint child
                    for (var i = 1; i < _positions.Length; i++)
                    {
                        //  Direction of parent joint to current joint
                        var parentToCurrent = (_positions[i] - _positions[i - 1]).normalized;
                        //  Set position to parent joint plus bone length in direction of parent to current joint.
                        _positions[i] = _positions[i - 1] + parentToCurrent * _boneLengths[i - 1];
                    }

                    //  Check if end effector is within minimum distance to target
                    //      Again using square magnitude for performance
                    if ((_positions[^1] - target.position).sqrMagnitude < delta * delta)
                        break;
                }
            }

            //  Factor in pole
            if (pole is not null)
                //  Only modify the joints between the root and end effector
                for (var i = 1; i < _positions.Length - 1; i++)
                {
                    //  TODO: CHECK THE PLANE EQUATION IS CORRECT
                    //  Create a plane with the normal being the direction from the parent joint to the child joint
                    //  and the point being the position of the child joint
                    var plane = new Plane(_positions[i + 1] - _positions[i - 1], _positions[i - 1]);

                    //  project the pole and joint onto the plane
                    var projectedPole = plane.ClosestPointOnPlane(pole.position);
                    var projectedJoint = plane.ClosestPointOnPlane(_positions[i]);


                    // Get the angle between the projected joint and pole relative to the plane normal
                    var angle = Vector3.SignedAngle(projectedJoint - _positions[i - 1],
                        projectedPole - _positions[i - 1],
                        plane.normal);

                    //  Use that angle to rotate the joint position about the plane normal such that it is close to the pole
                    _positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (_positions[i] - _positions[i - 1]) +
                                    _positions[i - 1];
                }

            //  Update joint positions to new solved positions
            for (var i = 0; i < _positions.Length; i++)
            {
                if (i == _positions.Length - 1)
                    _joints[i].rotation =
                        target.rotation * Quaternion.Inverse(_startRotationTarget) * _startRotations[i];
                else
                    _joints[i].rotation =
                        Quaternion.FromToRotation(_startDirections[i], _positions[i + 1] - _positions[i]) *
                        _startRotations[i];
                _joints[i].position = _positions[i];
            }
        }
    }
}