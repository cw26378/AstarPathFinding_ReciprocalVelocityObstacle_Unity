using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PlayerMovement : MonoBehaviour {
    public Camera cam;
    public float speed;
    public 
    //public Transform target;
    Vector3 target;
    Seeker seeker;
    Path path;
    CharacterController characterController;
    int indexWayPoint;

    float thresholdToWayPoint = 4.5f;
	// Use this for initialization
	public void Start () {
        //get Seeker and CharacterController
        seeker = GetComponent<Seeker>();
        characterController = GetComponent<CharacterController>();

        //new path request
        //
        target = Input.mousePosition;
        //target = transform;
        //Debug.Log("the initial target is at: " + target.position.ToString());
        //seeker.StartPath(transform.position, target.position, OnPathComplete);
        seeker.StartPath(transform.position, transform.position + transform.forward * speed * 0.1f, OnPathComplete);
        //Debug.Log("initial target at Start: " + target.ToString());
        //DebugPathVector(path);
	}
    //Function to check if path is complete
    public void DebugPathVector(Path p){
        if (p == null){
            Debug.Log("path = null!");
            return;
        }
        if (p.error)
        {
            Debug.Log("path error:" + p.error);
            return;
        }
        for (int i = 0; i < p.vectorPath.Count; i++){
            Debug.Log("i-th element of path: " + p.vectorPath[i].ToString());
            
        }
    }
    public void OnPathComplete(Path p)
    {
        // We got our path back
        if (p.error)
        {
            // Nooo, a valid path couldn't be found
            Debug.Log("the error is at StartPath!" + p.error);
        }
        else
        {
            // Yay, now we can get a Vector3 representation of the path
            // from p.vectorPath
            path = p;
            indexWayPoint = 0;
        }
    }

	// Update is called once per fixed delta-time
	void FixedUpdate () {
        
        //Debug.Log("current mouse at click: " + Input.mousePosition.ToString());

        if(Input.GetMouseButtonDown(0)){
            
            //Debug.Log("New click activated! Current target at click: " + Input.mousePosition.ToString());
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)){
                
                // if the clicked point is within range of world
                target = hit.point;
                //Debug.Log("current target AFTER update: " + target.ToString());
                seeker.StartPath(transform.position, target, OnPathComplete);
                //Debug.Log("new path AFTER update has waypoint list length: " + path.vectorPath.Count.ToString());
                }
            }
         
        if (path == null)
        {
            Debug.Log("No available path found!");
            return;

        }
        if (indexWayPoint > (path.vectorPath.Count - 1))
        {
            Debug.Log("Path Finding ended!");
            return;
        }

        Vector3 velocity = (path.vectorPath[indexWayPoint] - transform.position).normalized * speed;
        // Slow down smoothly upon approaching the end of the path
        // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
        //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;
        //Debug.Log("Current WayPoint = " + path.vectorPath[indexWayPoint].ToString());
        //Debug.Log("Current Position = " + transform.position.ToString());

        //Debug.Log("Distance to move = " + velocity.ToString());

        characterController.SimpleMove(velocity);

        if (Vector3.Distance(transform.position, path.vectorPath[indexWayPoint]) <= thresholdToWayPoint){
            // close enough to current way point, time to move on!
            //Debug.Log("Move forward by one WayPoint! Now at WayPoint #" + indexWayPoint.ToString());
            indexWayPoint += 1;                    

        }

		
	}

}
