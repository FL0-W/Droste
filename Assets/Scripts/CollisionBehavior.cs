using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBehavior : MonoBehaviour
{

    private bool isCharged = false;
    private Rigidbody package = null; 
    private float initalMass = 5;
    private float initalPackageMass = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    void OnCollisionEnter(Collision collision)
    {
        print("Collision "+name);
        if(CompareTag("Core") && collision.gameObject.CompareTag("Package")){
            package = collision.gameObject.GetComponent<Rigidbody>();
            collision.gameObject.AddComponent<FixedJoint>();
            collision.gameObject.GetComponent<FixedJoint>().connectedBody = GetComponent<Rigidbody>();
            GetComponent<Rigidbody>().mass = initalMass + collision.rigidbody.mass;
            print("MASSE CORE : "+GetComponent<Rigidbody>().mass+"  ;  MASSE PACKAGE AFTER LIFT : "+collision.rigidbody.mass);
        }
    }

    public bool IsCharged()
    {
        UpdatePackage();
        return CompareTag("Core") && package != null;;
    }

    public void UpdatePackage()
    {
        if(package != null){
            if(initalPackageMass == 0 && GetComponent<Rigidbody>().velocity.y > 0){
                initalPackageMass = package.mass;
                package.mass = 0;
                transform.rotation = Quaternion.identity;
                // GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                print("ROTATION HANDLED: "+transform.rotation);
            }

            print("ROTATION: "+transform.rotation);
        }
    }
}
