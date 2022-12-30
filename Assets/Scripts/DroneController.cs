using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Scripts.Rotor;
using Scripts.Action;

public class DroneController : MonoBehaviour
{
    //Drone components
    public GameObject core;
    private List<IRotor> _rotors = new List<IRotor>();
    private List<Action> _actions = new List<Action>();

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
    private float powerRotor = 4;
    private float lerpSpeed = 1;
    Vector3 moving = new Vector3(0f, 0f, 0f);

    //Other variables
    private Action currentAction;
    private bool liftCompleted;
    private float xDistanceFromStart;
    private float zDistanceFromStart;
    private float yTarget;
    private bool hasTarget;
    private List<GameObject> obstacleList;
    private float obstacleFactor;
    private float obstacleSafeDistance;


    //Debug
    public bool debug = false;

    // Start is called before the first frame update
    void Start()
    {
        yTarget = -1;
        liftCompleted = false;
        hasTarget = false;
        rb_core = core.GetComponent<Rigidbody>();
        rb_drone = GetComponent<Rigidbody>();
        obstacleList = new List<GameObject>();
        _rotors = GetComponentsInChildren<IRotor>().ToList();
        obstacleFactor = 10;
        obstacleSafeDistance = 10;
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
        HandleRotors(yTarget, hasTarget);

        //Action after setup
        if(liftCompleted){
            //Action
            SimulateActions();
            //GoToTargetedHeight(20);
            //GoDownAndGetPackage(0.5f);
            // GoToTargetedHeight(1);
            // AssignObjective(0,0);
            // currentAction.Execute();

            //Use smar area
            AvoidObstacles();

            //Clear smart area
            obstacleList.Clear();
        }
    }

    void OnTriggerStay(Collider other)
    {
        //Smart detection area
        if(other.gameObject.CompareTag("Obstacle")){
            print("Obstacle detected nearby");
            obstacleList.Add(other.gameObject);
        }
        if(other.gameObject.CompareTag("Drone")){
            print("Drone detected nearby");
        }
    }

    public Rigidbody GetDrone()
    {
        return rb_core;
    }

    public void UpdatePositionDrone()
    {
        Vector3 pos = new Vector3(rb_core.position.x, rb_core.position.y-1, rb_core.position.z);
        rb_drone.position = pos;
    }

#region Physics

    public void HandleRotors(float yTarget = -1, bool isCharged = false)
    {
        foreach (IRotor rotor in _rotors)
        {
            rotor.UpdateRotor(rb_core, yTarget, isCharged);
        }
    }

    public void SideMotions(float xAxis = 0f, float zAxis = 0f, float rotationA = 0f)
    {
        Calibrate();
        float pitchValue = zAxis * minMaxPitch;
        float rollValue = -xAxis * minMaxRoll;
        yaw += rotationA * yawPower;

        pitchComputed = Mathf.Lerp(pitchComputed, pitchValue, lerpSpeed * Time.deltaTime);
        rollComputed = Mathf.Lerp(rollComputed, rollValue, lerpSpeed * Time.deltaTime);
        yawComputed = Mathf.Lerp(yawComputed, yaw, lerpSpeed * Time.deltaTime);

        Quaternion rot = Quaternion.Euler(pitchComputed,yawComputed,rollComputed);
        rb_core.MoveRotation(rot);
    }


    public void Calibrate()
    {
        minMaxPitch = hasTarget ? 4 : 5;
        minMaxRoll = hasTarget ? 4 : 5;
        maxVelocity = hasTarget ? 2 : 4;
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
            return -rb_core.velocity.x * /*OLD : 3*/ maxVelocity;
        }
        return -1 * ((rb_core.position.x - xAxis) / xDistanceFromStart);
    }

    public float StabilizeZ(float zAxis)
    {
        //If velocity too fast
        if(rb_core.velocity.z <= (-maxVelocity) || rb_core.velocity.z >= (maxVelocity)){
            return -rb_core.velocity.z /*OLD :/ 2.5f*/;
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
            return -rb_core.velocity.z * /*OLD: 4*/ maxVelocity;
        }
        return -1 * ((rb_core.position.z - zAxis) / zDistanceFromStart);
    }

    public void AvoidObstacles()
    {
        if (obstacleList.Count > 0)
        {
            Vector3 accelerationSum = Vector3.zero;
            foreach (GameObject obstacle in obstacleList)
            {
                Vector3 collisionPoint = obstacle.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

/*
                float goingAbove = obstacle.GetComponent<Collider>().bounds.size.y / 2;
                
                if(collisionPoint.y > goingAbove // and goingAbove shorter than the other directions//){
                    //Pause and go upwards
                }else if(/*direction vers la droite et veux monter sur y// transform.position){

                }
*/
                float distance = Vector3.Distance(transform.position, collisionPoint);
                print(distance);
                //FIND PATH



                float factor = obstacleFactor / ((distance - obstacleSafeDistance + 1) * (distance - obstacleSafeDistance + 1));
                accelerationSum += Vector3.Normalize(transform.position - collisionPoint) * factor;
            }
            rb_core.AddForce(accelerationSum);
        }
    }

#endregion

#region Actions

    private void SimulateActions(){

        if(currentAction == null){

        Action action1 = new Action(this, ActionType.MOVINGTOLOCATION, 0, 0, 0);
        Action action2 = new Action(this, ActionType.MOVINGTOLOCATION, -10, 0, -10);
        Action action3 = new Action(this, ActionType.GOINGUPDOWN, 0, 20, 0);
        Action action4 = new Action(this, ActionType.GOINGUPDOWN, 0, 10, 0);

        AddAction(action3);
        AddAction(action1);
        AddAction(action2);
        AddAction(action4);
        }

        if(_actions.Count > 0 && (currentAction == null || currentAction.IsFinished()))
        {
            // if(currentAction != null && currentAction.IsFinished())
            // {
            //     Debug.Log("STATUS ACTION :1="+_actions[0].IsFinished()+" ; 2="+_actions[1].IsFinished()+" ; 3="+_actions[2].IsFinished()+" ; 4="+_actions[3].IsFinished());
            // }
            currentAction = _actions.FirstOrDefault(action => action.IsFinished() != true);

            if(currentAction != null){
                // Debug.Log("ACTION ======> Type: "+currentAction.GetType()+", finished: "+currentAction.IsFinished());
            }else{
                Debug.Log("ACTION FINISHED");
            }
            // foreach(Action action in _actions)
            // {
            //     currentAction = action;
            // }
        }

        if(currentAction != null){
            currentAction.Execute();
        }
    }

    public void AddAction(Action newAction){
        _actions.Add(newAction);
    }

    public void SetDistanceX(float xDistance){
        xDistanceFromStart = xDistance;
    }

    public void SetDistanceZ(float zDistance){
        zDistanceFromStart = zDistance;
    }

    // public void AssignObjective(float x, float z)
    // {
    //     //Assign distance on xAxis
    //     if(rb_core.position.x > x){
    //         xDistanceFromStart = rb_core.position.x - x;
    //     }else{
    //         xDistanceFromStart = x - rb_core.position.x;
    //     }
    //     //Assign distance on zAxis
    //     if(rb_core.position.z > z){
    //         zDistanceFromStart = rb_core.position.z - z;
    //     }else{
    //         zDistanceFromStart = z - rb_core.position.z;
    //     }

    //     currentAction = new Action(this, 0,0);
    // }
    
    
    public void GoToTargetedHeight(float target)
    {
        yTarget = target;
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


    public void GoDownAndGetPackage(float packageHeight = 0.5f){
        if(!core.GetComponent<CollisionBehavior>().IsCharged()){
            GoToTargetedHeight(packageHeight);
        }else{
            hasTarget = true;
            powerRotor = 1f;
            GoToTargetedHeight(10);
            // AssignObjective(rb_core.position.x, rb_core.position.z);
        }
    }

#endregion

    

}
