using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Scripts.Rotor;

public class DroneController : MonoBehaviour
{
    //Drone components
    public GameObject rotorTL;
    public GameObject rotorTR;
    public GameObject rotorBL;
    public GameObject rotorBR;
    public GameObject core;

    private List<IRotor> _rotors = new List<IRotor>();
    private Rigidbody rb_motor_BR;
    private Rigidbody rb_motor_TL;
    private Rigidbody rb_motor_BL;
    private Rigidbody rb_motor_TR;
    private Rigidbody rb_core;


    //Drone physics
    private Rigidbody rb;
    // Constantes pour les différentes forces
    public const float GRAVITY = 9.81f; // Gravité
    public const float LIFT_COEFFICIENT = 1.0f; // Coefficient de portance
    public const float DRAG_COEFFICIENT = 0.01f; // Coefficient de traînée

    // Variables pour les différentes forces
    private Vector3 gravity; // Force de gravité
    private Vector3 lift; // Force de portance
    private Vector3 drag; // Force de traînée
    private float angular_force = 0f;

    // Autres variables
    private float mass; // Masse du quadricoptère
    public float surfaceArea = 1.0f; // Surface des rotors
    public float airDensity = 1.225f; // Masse volumique de l'air
    public Vector3 MAX_VELOCITY = new Vector3(2f, 2f, 2f);

    private float roll = 0;
    private float pitch = 0;
    private float yaw = 0; 
    private float throttle = 0;




    public float throttleValue = 6.5f;
    public float sensitivity = 0.1f;


    private bool liftCompleted;

    // Start is called before the first frame update
    void Start()
    {
        liftCompleted = false;
        rb = core.GetComponent<Rigidbody>();
        mass = rb.mass;
        _rotors = GetComponentsInChildren<IRotor>().ToList();
        rb_core = core.GetComponent<Rigidbody>();
        rb_motor_BR = rotorBR.GetComponent<Rigidbody>();
        rb_motor_TL = rotorTL.GetComponent<Rigidbody>();
        rb_motor_BL = rotorBL.GetComponent<Rigidbody>();
        rb_motor_TR = rotorTR.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(!liftCompleted && rb_core.position.y >= 10){
            liftCompleted = true;
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

    protected virtual void HandleRotors()
    {
        foreach (IRotor rotor in _rotors)
        {
            rotor.UpdateRotor(rb);
        }
    }
    

    void old_FixedUpdate()
    {
        // throttleValue = CalculateThrottle(throttleValue);


        // rb.AddForceAtPosition(transform.up * 7.5f, rotorTL.transform.position);
        // rb.AddForceAtPosition(transform.up * 7.5f, rotorTR.transform.position);
        // rb.AddForceAtPosition(transform.up * 7.5f, rotorBL.transform.position);
        // rb.AddForceAtPosition(transform.up * 7.5f, rotorBR.transform.position);

        // ApplyThrust(throttleValue, throttleValue, throttleValue, throttleValue);
        // Récupérer la vitesse du quadricoptère
        Vector3 velocity = rb_core.velocity;
        if(velocity.x > MAX_VELOCITY.x){
            velocity.x = MAX_VELOCITY.x;
        }
        if(velocity.y > MAX_VELOCITY.y){
            velocity.y = MAX_VELOCITY.y;
        }
        if(velocity.z > MAX_VELOCITY.z){
            velocity.z = MAX_VELOCITY.z;
        }

        // Calculer la force de portance en fonction de la vitesse et de la surface des rotors
        lift = Vector3.up * (0.5f * airDensity * velocity.magnitude * velocity.magnitude * surfaceArea * LIFT_COEFFICIENT);

        // Calculer la force de traînée en fonction de la vitesse et de la surface des rotors
        drag = -velocity.normalized * (0.5f * airDensity * velocity.magnitude * velocity.magnitude * surfaceArea * DRAG_COEFFICIENT);

        // Récupérer les contrôles de l'utilisateur
        roll = Input.GetAxis("Horizontal");
        pitch = Input.GetAxis("Vertical");
        // yaw = Input.GetAxis("Yaw");
        // throttle = Input.GetAxis("Throttle");

        gravity = Vector3.up * GRAVITY * mass;
        
        // Ajuster la force de portance en fonction des contrôles de l'utilisateur
        lift += pitch * Vector3.forward + roll * Vector3.right;

        // Ajuster la force de traînée en fonction des contrôles de l'utilisateur
        drag += pitch * Vector3.forward + roll * Vector3.right;

        // Ajuster la force de gravité en fonction des contrôles de l'utilisateur
        // gravity += throttle * Vector3.up * 10 + new Vector3(0, -lift.y - drag.y , 0);

        // Orientation actuel du core
        Quaternion orientation = rb_core.rotation;

        // Conversion en angle d'Euleur
        Vector3 eulerAngles = orientation.eulerAngles;

        // Angle en Y
        float yawAngle = eulerAngles.y;

        // Axe d'orientaton
        Vector3 axis = Vector3.up;

        // Calcule de la force appliquée à chaque moteur
        Vector3 force = StabilizedForce(gravity, lift, drag);

        // Rotation des forces appliquées dans le plan du drone
        Quaternion rotation = Quaternion.AngleAxis(yawAngle, axis);
        Vector3 rotatedVector = rotation * force;

        rb_motor_TL.AddForce(rotatedVector, ForceMode.Force);
        rb_motor_BL.AddForce(rotatedVector, ForceMode.Force);
        rb_motor_BR.AddForce(rotatedVector, ForceMode.Force);
        rb_motor_TR.AddForce(rotatedVector, ForceMode.Force);

        rb_core.freezeRotation = true;
        // angular_force = yaw * 3f; 

        // rb_core.AddTorque(new Vector3(0, angular_force, 0));
        
        // if(yaw == 0){
        //     rb_core.freezeRotation = true;
        // }

        // if (throttle == 0){
        //     cancelThrust(velocity);
        // }
       

    }

    public float CalculateThrottle(float throttleValue)
    {
        throttleValue += Input.GetAxis("Vertical");
        throttleValue = Mathf.Clamp(throttleValue, 2.5f, 10f);

        return throttleValue;
    }

    void cancelThrust(Vector3 velocity){
        if (velocity.y > 0){
            rb_core.AddForce(new Vector3(0,-GRAVITY * mass,0), ForceMode.Force);
        }else if (velocity.y < 0){
            rb_core.AddForce(new Vector3(0,GRAVITY * mass,0), ForceMode.Force);            
        }
    }

    void ApplyThrust(float tl, float tr, float bl, float br)
    {
        rb.AddForceAtPosition(transform.up * tl, rotorTL.transform.position);
        rb.AddForceAtPosition(transform.up * tr, rotorTR.transform.position);
        rb.AddForceAtPosition(transform.up * bl, rotorBL.transform.position);
        rb.AddForceAtPosition(transform.up * br, rotorBR.transform.position);
    }

    void ApplyVerticalThrust(float thrust)
    {
        rb.AddForceAtPosition(transform.up * thrust, rotorTL.transform.position);
        rb.AddForceAtPosition(transform.up * thrust, rotorTR.transform.position);
        rb.AddForceAtPosition(transform.up * thrust, rotorBL.transform.position);
        rb.AddForceAtPosition(transform.up * thrust, rotorBR.transform.position);
    }

    public Vector3 StabilizedForce(Vector3 gravity, Vector3 lift, Vector3 drag)
    {
        Vector3 force = (gravity + lift + drag) / 2f;
        if(rb_core.transform.position.y > 10){
        // if(rb.transform.position.y > 5){
            force = (gravity + lift + drag) / 7f;
        }
        if(liftCompleted && rb_core.transform.position.y < 9.8){
            force = (gravity + lift + drag) ;
        }
        return force;
    }
}
