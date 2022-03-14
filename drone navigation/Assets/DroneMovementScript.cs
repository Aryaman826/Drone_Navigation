using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;

public class DroneMovementScript : Agent{

    public Rigidbody ourDrone;
    
    // void Awake(){
    //     ourDrone = GetComponent<Rigidbody>();
        
    // }
    // public var falloffStartHeight = 50;
    // public var falloffRange = 10;
    [SerializeField] private Transform targetTransform; 
    public override void CollectObservations(VectorSensor sensor){
        
        sensor.AddObservation(transform.position);
        sensor.AddObservation(targetTransform.position);

    }
    public override void Initialize(){
        ourDrone = GetComponent<Rigidbody>();
    }
    public override void OnActionReceived(ActionBuffers actions ){
        if (Mathf.FloorToInt(actions.DiscreteActions[0]) == 1)  
            MovementForward(); 
        if (Mathf.FloorToInt(actions.DiscreteActions[1]) == 1)  
            MovementUpDown(); 
        Debug.Log(actions.DiscreteActions[0]);   
    }


    private void OnTriggerEnter(Collider other){
        // collision c
        SetReward(1f);
        EndEpisode();
    }
    public override void OnEpisodeBegin(){
        transform.position = new Vector3(0f,10f,-83.3f);

    }
    // public override void Heuristic(float[] actionsOut)  
    // {  
    // actionsOut[0] = 0;

    // if (Input.GetKey(//key))
    //     Do some work  
    // }

    void FixedUpdate(){
        MovementUpDown();
        MovementForward();
        Rotation();
        ClampingSpeedValues();
        Swerve();
        
        var falloffStartHeight = 50;
        var falloffRange = 10;
        var altitude = transform.position.y;
        var aboveFalloffHeight = Mathf.Clamp(altitude - falloffStartHeight , 0 , falloffRange);
        var falloffScalar = 1 - aboveFalloffHeight / falloffRange;

        ourDrone.AddRelativeForce(Vector3.up * upForce * falloffScalar);
        //ourDrone.AddRelativeForce(Vector3.up * upForce);

        ourDrone.rotation = Quaternion.Euler(
            new Vector3(tiltAmountForward,currentYRotation,tiltAmountSideways)
        );
    }

    public float upForce;
    void MovementUpDown(){
        if(Mathf.Abs(Input.GetAxis("Vertical"))>0.2f || Mathf.Abs(Input.GetAxis("Horizontal"))>0.2f){
            if(Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.K)){
                ourDrone.velocity = ourDrone.velocity;
            }
            if(!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) && !Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.L)){
                ourDrone.velocity = new Vector3(ourDrone.velocity.x,Mathf.Lerp(ourDrone.velocity.y,0,Time.deltaTime*5),ourDrone.velocity.z);
                upForce = 281;
            }
            if(!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) && (Input.GetKey(KeyCode.J) || (Input.GetKey(KeyCode.L)))){
                ourDrone.velocity = new Vector3(ourDrone.velocity.x,Mathf.Lerp(ourDrone.velocity.y,0,Time.deltaTime * 5),ourDrone.velocity.z);
                upForce = 110;
            }
            if(Input.GetKey(KeyCode.J) || (Input.GetKey(KeyCode.L))){
                upForce = 410;
            }
        }
        if(Mathf.Abs(Input.GetAxis("Vertical")) < 0.2f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f){
            upForce = 135;
        }

        if(Input.GetKey(KeyCode.I)){
            upForce=450;
            if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f){
                upForce = 500;
            }
        }
        else if(Input.GetKey(KeyCode.K)){
            upForce = -200;
        }
        else if((!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K)) && (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.2f )){
            upForce = 98.1f;
        }
    }

    private float movementForwardSpeed = 500.0f;
    private float tiltAmountForward = 0;
    private float tiltVelocityForward;
    void MovementForward(){
        if(Input.GetAxis("Vertical")!=0){
            ourDrone.AddRelativeForce(Vector3.forward * Input.GetAxis("Vertical")*movementForwardSpeed);
            tiltAmountForward = Mathf.SmoothDamp(tiltAmountForward,20*Input.GetAxis("Vertical"),ref tiltVelocityForward,0.1f);
        }
    }

    private float wantedYRotation;
    [HideInInspector]public float currentYRotation;
    private float rotateAmountByKeys = 2.5f;
    private float rotationYVelocity;
    void Rotation(){
        if(Input.GetKey(KeyCode.J)){
            wantedYRotation -= rotateAmountByKeys; 
        }
        if(Input.GetKey(KeyCode.L)){
            wantedYRotation += rotateAmountByKeys;
        }
        currentYRotation = Mathf.SmoothDamp(currentYRotation,wantedYRotation,ref rotationYVelocity,0.25f);
    }

    private Vector3 velocityToSmoothDampToZero;
    void ClampingSpeedValues(){
        if(Mathf.Abs(Input.GetAxis("Vertical"))>0.2f && Mathf.Abs(Input.GetAxis("Horizontal"))>0.2f){
            ourDrone.velocity = Vector3.ClampMagnitude(ourDrone.velocity,Mathf.Lerp(ourDrone.velocity.magnitude,10.0f,
            Time.deltaTime * 5f));
        }
        if(Mathf.Abs(Input.GetAxis("Vertical"))>0.2f && Mathf.Abs(Input.GetAxis("Horizontal"))<0.2f){
            ourDrone.velocity = Vector3.ClampMagnitude(ourDrone.velocity,Mathf.Lerp(ourDrone.velocity.magnitude,10.0f,
            Time.deltaTime * 5f));
        }
        if(Mathf.Abs(Input.GetAxis("Vertical"))< 0.2f && Mathf.Abs(Input.GetAxis("Horizontal"))> 0.2f){
            ourDrone.velocity = Vector3.ClampMagnitude(ourDrone.velocity,Mathf.Lerp(ourDrone.velocity.magnitude,10.0f,
            Time.deltaTime * 5f));
        }
        if(Mathf.Abs(Input.GetAxis("Vertical"))< 0.2f && Mathf.Abs(Input.GetAxis("Horizontal"))< 0.2f){
            ourDrone.velocity = Vector3.SmoothDamp(ourDrone.velocity,Vector3.zero,ref velocityToSmoothDampToZero,0.95f);
        }
    }

    private float sideMovementAmount = 300.0f;
    private float tiltAmountSideways;
    private float tiltAmountVelocity;
    void Swerve(){
        if(Mathf.Abs(Input.GetAxis("Horizontal"))>0.2f){
            ourDrone.AddRelativeForce(Vector3.right * Input.GetAxis("Horizontal")*sideMovementAmount);
            //tiltAmountSideways = Mathf.SmoothDamp -20 * Input.GetAxis("Horizontal"),ref tiltAmountVelocity , 0.1f);
        }
        else{
            tiltAmountSideways = Mathf.SmoothDamp(tiltAmountSideways,0,ref tiltAmountSideways,0.1f);
        }
    }


}

