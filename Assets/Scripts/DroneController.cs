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
            Action action5 = new Action(this, ActionType.MOVINGTOLOCATION, -50, 10, 0, 0.5f);
            AddAction(action5);
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
            Debug.Log("Obstacle detected nearby");
            obstacleList.Add(other.gameObject);
        }
        if(other.gameObject.CompareTag("Drone")){
            Debug.Log("Drone detected nearby");
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
                //Debug.Log(distance);
                //FIND PATH



                float factor = obstacleFactor / ((distance - obstacleSafeDistance + 1) * (distance - obstacleSafeDistance + 1));
                accelerationSum += Vector3.Normalize(transform.position - collisionPoint) * factor;
            }
            Debug.Log("===========> "+accelerationSum);
            rb_core.AddForce(accelerationSum);
        }
    }

#endregion

#region Actions

    private void PerformActions(){
        if(_actions.Count > 0 && (currentAction == null || currentAction.IsFinished()))
        {
            currentAction = _actions.FirstOrDefault(action => action.IsFinished() != true);

            if(currentAction == null){
                Debug.Log("ACTIONS FINISHED");
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
        Vector3 stabilization = new Vector3(0f, rb_core.velocity.y, 0f);
        rb_core.velocity = stabilization;
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
        if(core.GetComponent<CollisionBehavior>().IsCharged()){
            GoToTargetedHeight(packageHeight);
        }else{
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
