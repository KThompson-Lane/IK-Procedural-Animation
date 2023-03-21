using UnityEngine;

public class StepTarget : MonoBehaviour
{
    [SerializeField] private Transform initialTransform;
    //  Set our layer mask to only use layer 5 (ground)
    [SerializeField] private LayerMask mask;
    [SerializeField] private float heightOffset;

    private void Awake()
    {
        transform.position = initialTransform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 dwn = transform.TransformDirection(Vector3.down);

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * heightOffset, dwn, out hit, 10, mask))
        {
            Debug.DrawRay(transform.position, dwn * hit.distance, Color.yellow);
            print("Hit Ground!");
            transform.position = hit.point;
        }
    }
}
