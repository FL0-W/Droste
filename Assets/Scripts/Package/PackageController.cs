using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageController : MonoBehaviour
{
    [Header("Package Target position")]
    public float xTarget = -100;
    public float yTarget = -100;
    public float zTarget = -100;
    public float height = 0.5f;
    [Header("Zone attributed")] 
    public string zoneName;
    public bool processing = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
