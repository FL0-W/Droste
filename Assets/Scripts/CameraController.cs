using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float horizontalFoV = 90.0f;
    public float speedH = 2f;
    public float speedV = 2f;
    public bool isSet = true;

    private float yaw = 0f;
    private float pitch = 0f;

    // Start is called before the first frame update
    void Start()
    {
        SetCamera();
    }

    void Update()
    {
        if(isSet && Input.GetMouseButton(0)){
            yaw+= speedH * Input.GetAxis("Mouse X");
            pitch+= speedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }

    private void SetCamera()
    {
        gameObject.SetActive(isSet);
        float halfWidth = Mathf.Tan(0.5f * horizontalFoV * Mathf.Deg2Rad);

        float halfHeight = halfWidth * Screen.height / Screen.width;

        float verticalFoV = 2.0f * Mathf.Atan(halfHeight) * Mathf.Rad2Deg;

        GetComponent<Camera>().fieldOfView = verticalFoV;
    }
}
