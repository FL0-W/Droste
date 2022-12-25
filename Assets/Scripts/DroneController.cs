using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Scripts.Rotor;

public class DroneController : MonoBehaviour
{
    //Drone components
    public GameObject core;
    private List<IRotor> _rotors = new List<IRotor>();
    
    //Drone physics
    private Rigidbody rb;
    private float minMaxPitch = 30;
    private float minMaxRoll = 30;
    private float pitchComputed;
    private float rollComputed;
    private float yawComputed;
    private float yaw;
    private float yawPower = 4;
    private float lerpSpeed = 2;

    //Other variables
    private bool liftCompleted;

    // Start is called before the first frame update
    void Start()
    {
        liftCompleted = false;
        rb = core.GetComponent<Rigidbody>();
        _rotors = GetComponentsInChildren<IRotor>().ToList();
    }

    void Update()
    {
        if(!liftCompleted && rb.position.y >= 10){
            liftCompleted = true;
            // SideMotions(1, 0, 1);
        }
    }

    void FixedUpdate()
    {
        if (!rb)
        {
            return;
        }
        HandleRotors();
    }

    public void HandleRotors()
    {
        foreach (IRotor rotor in _rotors)
        {
            rotor.UpdateRotor(rb);
        }
    }
    
    public void SideMotions(float yAxis = 0f, float xAxis = 0, float rotationA = 0f)
    {
        float pitchValue = yAxis * minMaxPitch;
        float rollValue = -xAxis * minMaxRoll;
        yaw += rotationA * yawPower;

        pitchComputed = Mathf.Lerp(pitchComputed, pitchValue, lerpSpeed * Time.deltaTime);
        rollComputed = Mathf.Lerp(rollComputed, rollValue, lerpSpeed * Time.deltaTime);
        yawComputed = Mathf.Lerp(yawComputed, yaw, lerpSpeed * Time.deltaTime);
            
        Quaternion rot = Quaternion.Euler(pitchComputed,yawComputed,rollComputed);
        rb.MoveRotation(rot);
    }


}
