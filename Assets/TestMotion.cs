using System;
using DefaultNamespace;
using UnityEngine;

public class TestMotion : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    [Header("Movement coefficients")]
    [SerializeField] 
    private float fx,zx,rx;
    
    [Header("Orientation coefficients")]
    [SerializeField]
    private float fy,zy,ry;

    private SecondOrderMotion Movement, Orientation;

    private void Awake()
    {
        Movement = new SecondOrderMotion(fx, zx, rx, transform.position);
        Orientation = new SecondOrderMotion(fy, zy, ry, transform.up);
    }

    private void FixedUpdate()
    {
        var currentPosition = transform.position;
        var newPosition = Movement.Update(Time.deltaTime, target.position);
        transform.position = newPosition;
        
        //  Calculate tilt
        var delta = newPosition - currentPosition;
        var test = Vector3.RotateTowards(transform.up, delta, delta.sqrMagnitude, 0.0f);
        var tilt = Orientation.Update(Time.deltaTime, test);
        Debug.DrawRay(transform.position, tilt.normalized * 5, Color.green);
    }

    private void OnValidate()
    {
        if(Movement == null || Orientation == null)
            return;
        Movement.CalculateKValues(fx,zx,rx);
        Orientation.CalculateKValues(fy, zy, ry);
    }
}
