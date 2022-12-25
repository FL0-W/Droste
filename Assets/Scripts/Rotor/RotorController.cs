using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Rotor;


public class RotorController : MonoBehaviour, IRotor
{

    private float maxPower = 9.9f;
    private float throttle = 1;

    public void UpdateRotor(Rigidbody rb)
    {
        Vector3 upVector = transform.up;
        upVector.x = 0;
        upVector.z = 0;
        float diff = 1 - upVector.magnitude;
        float finalDiff = diff * Physics.gravity.magnitude;
        
        if(rb.transform.position.y < 9){
            IncreasePower();
        }
        if(rb.transform.position.y > 10){
            DecreasePower();
        }
        Vector3 engineForce = Vector3.zero;
        engineForce = transform.up * ((rb.mass*Physics.gravity.magnitude + finalDiff ) + (throttle * maxPower))/4;
        rb.AddForce(engineForce,ForceMode.Force);
    }

    public void DecreasePower()
    {
        maxPower = 9f;
    }

    public void IncreasePower()
    {
        maxPower = 10f;
    }
}
