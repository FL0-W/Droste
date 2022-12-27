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
    private Rigidbody rb_core;
    private float minMaxPitch = 5;
    private float minMaxRoll = 5;
    private float pitchComputed;
    private float rollComputed;
    private float yawComputed;
    private float yaw;
    private float yawPower = 4;
    private float lerpSpeed = 1;
    Vector3 moving = new Vector3(0f, 0f, 0f);

    //Other variables
    private bool liftCompleted;
    private float xDistanceFromStart;
    private float zDistanceFromStart;


    //Debug
    public bool debug = false;

    // Start is called before the first frame update
    void Start()
    {
        liftCompleted = false;
        rb_core = core.GetComponent<Rigidbody>();
        _rotors = GetComponentsInChildren<IRotor>().ToList();
    }

    void Update()
    {
        if(!liftCompleted && rb_core.position.y >= 10){
            liftCompleted = true;
        }
    }

    void FixedUpdate()
    {
        if (!rb_core)
        {
            return;
        }
        HandleRotors();

        //Action aftre setup
        if(liftCompleted){
            //SideMotions(0, 1, 0);
            AssignObjective(0,0);
            GoToAndStabilize(0, 0);
        }
    }

    public void HandleRotors()
    {
        foreach (IRotor rotor in _rotors)
        {
            rotor.UpdateRotor(rb_core);
        }
    }

    public void SideMotions(float xAxis = 0f, float zAxis = 0f, float rotationA = 0f)
    {
            float pitchValue = zAxis * minMaxPitch;
            float rollValue = -xAxis * minMaxRoll;
            yaw += rotationA * yawPower;

            pitchComputed = Mathf.Lerp(pitchComputed, pitchValue, lerpSpeed * Time.deltaTime);
            rollComputed = Mathf.Lerp(rollComputed, rollValue, lerpSpeed * Time.deltaTime);
            yawComputed = Mathf.Lerp(yawComputed, yaw, lerpSpeed * Time.deltaTime);

            Quaternion rot = Quaternion.Euler(pitchComputed,yawComputed,rollComputed);
            rb_core.MoveRotation(rot);
    }

    public void GoToAndStabilize(float xAxis, float zAxis)
    {
        float x = 0f;
        float z = 0f;
        float r = 0f;

        if(xAxis < rb_core.position.x){
            if(rb_core.position.x - xAxis < 1){
                x = StabilizeX();
            }else{
                x = -1 * ((rb_core.position.x - xAxis ) / xDistanceFromStart);
            }

            if(!debug){
                print("xDistanceFromStart = "+xDistanceFromStart+" ; xAxis = "+xAxis+" ; rb_core.position.x = "+rb_core.position.x+ "; calcul = "+x);
            }

        }
        else if(xAxis > rb_core.position.x){
            if(xAxis - rb_core.position.x < 1){
                x = StabilizeX();
            }else{
                x = 1 * ((xAxis - rb_core.position.x) / xDistanceFromStart);
            }
        }
        


        if(zAxis < rb_core.position.z){
            if(rb_core.position.z - zAxis < 1){
                z = StabilizeZ();
            }else{
                z = -1 * ((rb_core.position.z - zAxis ) / zDistanceFromStart);
            }
        }
        else if(zAxis > rb_core.position.z){
            if(zAxis - rb_core.position.z < 1){
                z = StabilizeZ();
            }else{
                z = 1 * ((zAxis - rb_core.position.z) / zDistanceFromStart);
            }
        }
        

        if(!debug){
            print("X Z R"+x+" "+z+" "+r);
            debug =true;
        }
        SideMotions(x, z, r);
    }

    public float StabilizeX()
    {
        return -rb_core.velocity.x * 3;
    }

    public float StabilizeZ()
    {
        return -rb_core.velocity.z * 3;
    }

    public void AssignObjective(float x, float z)
    {
        //Assign distance on xAxis
        if(rb_core.position.x > x){
            xDistanceFromStart = rb_core.position.x - x;
        }else{
            xDistanceFromStart = x - rb_core.position.x;
        }
        //Assign distance on zAxis
        if(rb_core.position.z > z){
            zDistanceFromStart = rb_core.position.z - z;
        }else{
            zDistanceFromStart = z - rb_core.position.z;
        }
    }

}
