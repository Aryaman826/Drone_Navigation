// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class camerafollows : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }

using UnityEngine;
using System.Collections;

public class camerafollows : MonoBehaviour{

    private Transform ourDrone;
    void Awake(){
        ourDrone = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private Vector3 velocitycamerafollows;
    public float angle;
    public Vector3 behindPosition = new Vector3(0,2,-4);
    void FixedUpdate(){
        transform.position = Vector3.SmoothDamp(transform.position, ourDrone.transform.TransformPoint(behindPosition) + 
        Vector3.up*Input.GetAxis("Vertical"),ref velocitycamerafollows,0.1f);
        transform.rotation = Quaternion.Euler(new Vector3(angle , ourDrone.GetComponent<DroneMovementScript>().currentYRotation,0));
    }
}
