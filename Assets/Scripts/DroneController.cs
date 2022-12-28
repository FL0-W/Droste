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
    private Rigidbody rb_drone;
    private float minMaxPitch = 5;
    private float minMaxRoll = 5;
    private float maxVelocity = 4;
    private float stabilizeQuotient = 2;
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
        rb_drone = GetComponent<Rigidbody>();
        _rotors = GetComponentsInChildren<IRotor>().ToList();
    }

    void Update()
    {
        UpdatePositionDrone();
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

        //Action after setup
        if(liftCompleted){
            AssignObjective(0,0);
            GoToAndStabilize(0, 0);
        }
    }

    void OnTriggerStay(Collider other)
    {
        //Smart detection area
        if(other.gameObject.CompareTag("Obstacle")){
            print("Obstacle detected nearby");
        }
        if(other.gameObject.CompareTag("Drone")){
            print("Drone detected nearby");
        }
    }

    public void UpdatePositionDrone()
    {
        Vector3 pos = new Vector3(rb_core.position.x, rb_core.position.y-1, rb_core.position.z);
        rb_drone.position = pos;
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

        //AXE X
        x = StabilizeX(xAxis);

        //AXE Y
        z = StabilizeZ(zAxis);
        
        SideMotions(x, z, r);
    }

    public float StabilizeX(float xAxis)
    {
        //If velocity too fast
        if(rb_core.velocity.x <= (-maxVelocity) || rb_core.velocity.x >= (maxVelocity)){
            return -rb_core.velocity.x;
        }
        //Target in negative direction
        if(xAxis < rb_core.position.x){            
            if(rb_core.position.x - xAxis < 1){
                return -rb_core.velocity.x * maxVelocity;
            }else{
                return -1 * ((rb_core.position.x - xAxis) / xDistanceFromStart);
            }
        }
        //Target in positive direction
        if(xAxis - rb_core.position.x < 1){
            return -rb_core.velocity.x * 3;
        }
        return -1 * ((rb_core.position.x - xAxis) / xDistanceFromStart);
    }

    public float StabilizeZ(float zAxis)
    {
        //If velocity too fast
        if(rb_core.velocity.z <= (-maxVelocity) || rb_core.velocity.z >= (maxVelocity)){
            return -rb_core.velocity.z / 2.5f;
        }
        //Target in negative direction
        if(zAxis < rb_core.position.z){            
            if(rb_core.position.z - zAxis < 1){
                return -rb_core.velocity.z * maxVelocity;
            }else{
                return -1 * ((rb_core.position.z - zAxis) / zDistanceFromStart);
            }
        }
        //Target in positive direction
        if(zAxis - rb_core.position.z < 1){
            return -rb_core.velocity.z * 4;
        }
        return -1 * ((rb_core.position.z - zAxis) / zDistanceFromStart);
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
