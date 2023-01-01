using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Action;

public class CollisionBehavior : MonoBehaviour
{

    private Rigidbody package = null; 
    private float initalDroneMass = 5;
    private float initalPackageMass = 0;
    private DroneController droneFunctions;

    // Start is called before the first frame update
    void Start()
    {
        droneFunctions = transform.parent.GetComponent<DroneController>();
    }


    void OnCollisionEnter(Collision collision)
    {
        print("Collision "+name);
        if(CompareTag("Core") && collision.gameObject.CompareTag("Package") &&
        droneFunctions.GetCurrentAction() != null && droneFunctions.GetCurrentAction().GetType() == ActionType.GETTINGAPACKAGE){
            package = collision.gameObject.GetComponent<Rigidbody>();
            /*collision.*/gameObject.AddComponent<FixedJoint>();
            /*collision.*/gameObject.GetComponent<FixedJoint>().connectedBody = package; //GetComponent<Rigidbody>();
            GetComponent<Rigidbody>().mass = initalDroneMass + collision.rigidbody.mass;
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
                GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }

            if(droneFunctions.GetCurrentAction() != null && droneFunctions.GetCurrentAction().GetType() == ActionType.DROPPINGPACKAGE
            && transform.position.y <= droneFunctions.GetCurrentAction().GetPackageHeight()+1){
                //Remove joint component
                package.mass = initalPackageMass;
                initalPackageMass = 0;
                GetComponent<Rigidbody>().mass = initalDroneMass - package.mass;
                Destroy(gameObject.GetComponent<FixedJoint>());
                droneFunctions.SetCharge(false);
                package = null;
            }
        }
    }
}
