using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Scripts.Action;
using TMPro;

public class UnitControler : MonoBehaviour
{

    [Header("Drone camera")]
    public Camera camera;
    private int cameraOnDrone = 0;
    
    [Header("Monitoring")]
    public TextMeshProUGUI droneCount;

    [Header("Instruction")]
    public float heightToFly = 10;
    public bool activate = false;
    private List<GameObject> droneList;
    private List<GameObject> packageList;

    // Start is called before the first frame update
    void Start()
    {
        droneList = GameObject.FindGameObjectsWithTag("Drone").ToList();
        packageList = GameObject.FindGameObjectsWithTag("Package").ToList();

        SetCountText();
        UpdateCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if(activate){
            foreach (GameObject package in packageList)
            {
                AssignDroneToPackage(package);
            }
        }
        SetCountText();
        UpdateCamera();
    }

    void UpdateCamera()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)){
            cameraOnDrone--;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)){
            cameraOnDrone++;
        }

        if(cameraOnDrone >= droneList.Count){
            cameraOnDrone = 0;
        }else if(cameraOnDrone < 0){
            cameraOnDrone = droneList.Count -1;
        }

        Vector3 position = new Vector3(droneList[cameraOnDrone].transform.position.x,
                                    droneList[cameraOnDrone].transform.position.y,
                                    droneList[cameraOnDrone].transform.position.z - 10);

        camera.transform.position = position;
    }
    
    public void SetCountText()
    {
        float count = 0f;
        foreach (GameObject drone in droneList)
        {
            if(drone.GetComponent<DroneController>().IsActive()){
                count++;
            }
        }
        droneCount.text = "Drones in service : "+count+"/"+droneList.Count;
    }

    public void AssignDroneToPackage(GameObject package)
    {
        if(!package.GetComponent<PackageController>().processing){
            GameObject drone = droneList.FirstOrDefault(drone => 
                            drone.GetComponent<DroneController>().available == true &&
                            drone.GetComponent<DroneController>().zoneName == package.GetComponent<PackageController>().zoneName);
            if(drone){
                //Locking the objects
                float pX = package.GetComponent<Rigidbody>()./*transform.*/position.x;
                float pY = package.GetComponent<Rigidbody>()./*transform.*/position.y;
                float pZ = package.GetComponent<Rigidbody>()./*transform.*/position.z;
                float pH = package.GetComponent<PackageController>().height;
                float pXTarget = package.GetComponent<PackageController>().xTarget;
                float pYTarget = package.GetComponent<PackageController>().yTarget;
                float pZTarget = package.GetComponent<PackageController>().zTarget;
                //Powering up the drone if inactive
                if(!drone.GetComponent<DroneController>().IsActive()){
                    drone.GetComponent<DroneController>().PowerUp();
                }
                //Overriding default height
                Action goToHeight = new Action(drone.GetComponent<DroneController>(), 
                                ActionType.GOINGUPDOWN, pX, heightToFly, pZ, pH);
                drone.GetComponent<DroneController>().AddAction(goToHeight);
                //Going on package location
                Action goToPackage = new Action(drone.GetComponent<DroneController>(), 
                                ActionType.MOVINGTOLOCATION, pX, heightToFly, pZ, pH);
                drone.GetComponent<DroneController>().AddAction(goToPackage);
                //Getting the package
                Action getPackage = new Action(drone.GetComponent<DroneController>(), 
                                ActionType.GETTINGAPACKAGE, pX, heightToFly, pZ, pH);
                drone.GetComponent<DroneController>().AddAction(getPackage);
                //Going on package target location
                Action goToPackageTarget = new Action(drone.GetComponent<DroneController>(), 
                                ActionType.MOVINGTOLOCATION, pXTarget, heightToFly, pZTarget, pH);
                drone.GetComponent<DroneController>().AddAction(goToPackageTarget);
                //Dropping package
                Action droppingPackage = new Action(drone.GetComponent<DroneController>(), 
                                ActionType.DROPPINGPACKAGE, pXTarget, heightToFly, pZTarget, pH);
                drone.GetComponent<DroneController>().AddAction(droppingPackage);

                //Locking the objects
                drone.GetComponent<DroneController>().available = false;
                package.GetComponent<PackageController>().processing = true;
            }
        }
    }
}
