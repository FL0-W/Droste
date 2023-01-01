using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Scripts.Rotor;
using Scripts.Action;

public class DroneController : MonoBehaviour
{
    //Drone components
    [Header("Core of the drone")]
    public GameObject core;
    [Header("Availability")]
    public bool available = true;
    [Header("Zone attributed")]
    public string zoneName;
    private List<IRotor> _rotors = new List<IRotor>();
    private List<Action> _actions = new List<Action>();

    //Drone physics
    private Rigidbody rb_core;
    private Rigidbody rb_drone;
    private bool isActive = false;
    private float xStart;
    private float yStart;
    private float zStart;
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
    public bool debug = true;

    // Start is called before the first frame update
    void Start()
    {
        available = true;
        yTarget = -1;
        liftCompleted = false;
        hasTarget = name.Contains("Charged") ? true : false;
        rb_core = core.GetComponent<Rigidbody>();
        rb_drone = GetComponent<Rigidbody>();
        obstacleList = new List<GameObject>();
        _rotors = GetComponentsInChildren<IRotor>().ToList();
        xStart = rb_core.transform.position.x;
        yStart = rb_core.transform.position.y;
        zStart = rb_core.transform.position.z;
        obstacleFactor = 10;
        obstacleSafeDistance = 10;

        if(debug){
            PowerUp();
        }
    }

    void Update()
    {
        UpdatePositionDrone();
        if(!liftCompleted && rb_core.position.y >= yTarget){
            liftCompleted = true;
        }
    }

    void FixedUpdate()
    {
        if (!rb_core || !isActive)
        {
            return;
        }
        HandleRotors(yTarget, hasTarget);

        //Action after setup
        if(liftCompleted){
            //Action
            PerformActions();

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

    public bool IsActive()
    {
        return isActive;
    }

    public float GetInitialX()
    {
        return xStart;
    }

    public float GetInitialY()
    {
        return yStart;
    }

    public float GetInitialZ()
    {
        return zStart;
    }

    public void PowerUp()
    {
        Debug.Log(name+" powered up!");
        isActive = true;
    }

    public void ShutDown()
    {
        Debug.Log(name+" shut down.");
        isActive = false;
        _actions.Clear();
        rb_core.velocity = Vector3.zero;
        available = true;
    }

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

    public bool IsCharged()
    {
        return hasTarget;
    }

    public void SetCharge(bool isCharged)
    {
        hasTarget = isCharged;
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

    private void PerformActions(){

        // if(currentAction == null){
        //     Action action1 = new Action(this, ActionType.MOVINGTOLOCATION, -10, 0, 0);
        //     Action action2 = new Action(this, ActionType.MOVINGTOLOCATION, -10, 0, -10);
        //     Action action3 = new Action(this, ActionType.GOINGUPDOWN, 0, 20, 0);
        //     Action action4 = new Action(this, ActionType.GOINGUPDOWN, 0, 10, 0);
        //     Action action5 = new Action(this, ActionType.GETTINGAPACKAGE, -50, 15, -10, 0.5f);
        //     Action action6 = new Action(this, ActionType.GOBACKANDSHUTDOWN, /*should not count :*/ -10000, -100000, -100000);
        //     Action action7 = new Action(this, ActionType.DROPPINGPACKAGE, -50, 15, -10, 0.5f);

        //     //AddAction(action1);
        //     // AddAction(action6);

        //     AddAction(action5);
        //     AddAction(action7);
        //     // AddAction(action3);
        //     // AddAction(action2);
        //     // AddAction(action4);
        // }

        if(_actions.Count > 0 && (currentAction == null || currentAction.IsFinished()))
        {
            currentAction = _actions.FirstOrDefault(action => action.IsFinished() != true);

            if(currentAction == null){
                Debug.Log("ACTION FINISHED");
                available = true;
                GoBackAndShutDown();
            }
        }

        if(currentAction != null){
            currentAction.Execute();
        }
    }

    public Action GetCurrentAction()
    {
        return currentAction;
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


    public void GoDownAndGetPackage(float goBackUpTo, float packageHeight = 0.5f){
        if(!core.GetComponent<CollisionBehavior>().IsCharged()){
            GoToTargetedHeight(packageHeight);
        }else{
            hasTarget = true;
            GoToTargetedHeight(goBackUpTo);
        }
    }

    public void GoDownAndDropPackage(float goBackUpTo, float packageHeight = 0.5f){
        Debug.Log("========== GOING DOWN FOR DROP");
        if(core.GetComponent<CollisionBehavior>().IsCharged()){
            GoToTargetedHeight(packageHeight);
        Debug.Log("========== IS STILL CHARGED");

        }else{
        Debug.Log("========== UP NOW");

            GoToTargetedHeight(goBackUpTo);
        }
    }

    public void GoBackAndShutDown(bool landing = false)
    {
        Action getBack = new Action(this, ActionType.MOVINGTOLOCATION, xStart, yStart, zStart);
        Action land = new Action(this, ActionType.GOINGUPDOWN, xStart, yStart, zStart);
        
        if(!landing){
            currentAction = getBack;
        }else{
            currentAction = land;
        }

        if(transform.position.y <= yStart+0.1f){
            ShutDown();
        }
    }

#endregion

    

}
