﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Axle {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool powered;  // if the axle should have torque applied
    public bool steers;   // if the axle should have the steering angle applied
    public float antirollStiffness = 5000f;
}

public class CarController : MonoBehaviour
{
    public List<Axle> axles;
    public float maxTorque = 500f;
    public float maxRotationAngle = 45f;

    public Rigidbody car_rb;
    public float downforce = 500f;

    // Start is called before the first frame update
    void Start()
    {
        car_rb = GetComponent<Rigidbody>();

        foreach(Axle axle in axles){
            axle.leftWheel.ConfigureVehicleSubsteps(1, 12, 15);
            axle.rightWheel.ConfigureVehicleSubsteps(1, 12, 15);
        }
    }

    private void AntiRoll(Axle axle)
    {
        // Handle AntiRoll
        bool leftHit, rightHit;
        float leftTravel = 1f, rightTravel = 1f;
        float antirollForce;
        WheelHit hit;
        leftHit = axle.leftWheel.GetGroundHit(out hit);
        if(leftHit){
            leftTravel = (-axle.leftWheel.transform.InverseTransformPoint(hit.point).y - axle.leftWheel.radius) / axle.leftWheel.suspensionDistance;
        }
        rightHit = axle.rightWheel.GetGroundHit(out hit);
        if(rightHit){
            rightTravel = (-axle.rightWheel.transform.InverseTransformPoint(hit.point).y - axle.rightWheel.radius) / axle.rightWheel.suspensionDistance;
        }
        antirollForce = (leftTravel - rightTravel) * axle.antirollStiffness;

        if(leftHit){
            car_rb.AddForceAtPosition(transform.up * -antirollForce, axle.leftWheel.transform.position);
        }
        if(rightHit){
            car_rb.AddForceAtPosition(transform.up * antirollForce, axle.rightWheel.transform.position);
        }
    }

    void FixedUpdate()
    {
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");

        car_rb.AddForce(-transform.up* downforce);

        foreach(Axle axle in axles){
            // Handle Steering
            if (axle.steers){
                axle.leftWheel.steerAngle = horiz * maxRotationAngle;
                axle.rightWheel.steerAngle = horiz * maxRotationAngle;
            }
            // Handle Acceleration
            if (axle.powered){
                axle.leftWheel.motorTorque = vert * maxTorque;
                axle.rightWheel.motorTorque = vert * maxTorque;
            }
            AntiRoll(axle);
        }
    }
}