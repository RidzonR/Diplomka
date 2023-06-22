using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualControl : MonoBehaviour
{
    private CarModel carModel;

    public float CameraDistance;
    protected float InputRotationX;
    protected float InputRotationY;
    protected Vector3 LookDirection;
    protected Vector3 CameraVelocity;

    void Start()
    {
        carModel = GetComponent<CarModel>();
    }

    // Update is called once per frame
    void Update()
    {
        carModel.Input.Forward = Input.GetAxis("Vertical");
        carModel.Input.Steer = Input.GetAxis("Horizontal");

        var target = carModel.transform.position + (carModel.transform.forward * -1.5f + carModel.transform.up) * CameraDistance;
        Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, target, ref CameraVelocity, 0.3f);
        Camera.main.transform.LookAt(carModel.transform.position + carModel.transform.forward * 1.5f * CameraDistance, Vector3.up);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, .1f);
        //Gizmos.DrawWireSphere(transform.position, .11f);
    }
}
