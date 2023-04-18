using UnityEngine;

public class StepTarget : MonoBehaviour
{
    [SerializeField] private Transform initialTransform;
    //  Set our layer mask to only use layer 5 (ground)
    [SerializeField] private LayerMask mask;

    private void Awake()
    {
        transform.position = initialTransform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position + (Vector3.up * 5), Vector3.down, out var hit, 10, mask))
        {
            transform.position = hit.point;
        }
    }
}
