using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Rotor;


public class RotorController : MonoBehaviour, IRotor
{

    private float maxPower = 4f;
    private float throttle = 1;
    private float minLimitUp = 9;
    private float maxLimitUp = 10;


    private Rigidbody rb;

    public void UpdateRotor(Rigidbody rbody)
    {
        rb = rbody;
        Vector3 upVector = transform.up;
        upVector.x = 0;
        upVector.z = 0;
        float diff = 1 - upVector.magnitude;
        float finalDiff = diff * Physics.gravity.magnitude;        

        UpdateThrottle();
        Vector3 engineForce = Vector3.zero;
        engineForce = transform.up * ((rb.mass*Physics.gravity.magnitude + finalDiff ) + (throttle * maxPower))/4;
        rb.AddForce(engineForce,ForceMode.Force);
    }

    public void DecreaseThrottle()
    {
        throttle = (-1f * ((rb.position.y - maxLimitUp )) / maxLimitUp) / maxPower;
    }

    public void IncreaseThrottle()
    {
        throttle = (1f * ((minLimitUp - rb.position.y)) / minLimitUp) / maxPower;
    }

    public void UpdateThrottle()
    {
        if(rb.position.y < minLimitUp){
            IncreaseThrottle();
        }
        else if(rb.position.y > maxLimitUp){
            DecreaseThrottle();
        }else{
            //Brievely reverse ongoing motion
            throttle = -rb.velocity.y;
        }

    }
}
