using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Pathfinding;
using RVO;

public class RVO_AirAgentMove : MonoBehaviour {



    public Camera cam;

    public bool isAirBourne;
    public float air_height = 60.0f;

    [SerializeField]
    Vector3 target;
    //int currentNodeIndex = 0;
    int agentIndex = -1;

    RVO_SimulatorAir simulatorAir = null;
    //private List<Vector3> pathNodes = null;
    //bool isAbleToStart = false;
    //float thresholdToNode = 4.0f;

    //Seeker seeker;
    //Path path;
    CharacterController characterController;

    // Use this for initialization
    void Start()
    {
        //currentNodeIndex = 0;
        simulatorAir = GameObject.FindGameObjectWithTag("RVO_simAir").GetComponent<RVO_SimulatorAir>();
        characterController = GetComponent<CharacterController>();
        target = transform.position + transform.forward * 0.1f;
        //Debug.Log("agent position at Start(): " + transform.position.ToString());
        agentIndex = simulatorAir.addAgentToSimulator(transform.position, gameObject);
        //Debug.Log("Air simulator should have only one agent. True (0) or False(non-zero)? " + agentIndex.ToString());
        //isAbleToStart = true;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("New click activated! Current target at click: " + Input.mousePosition.ToString());

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                
                // if the clicked point is within range of world
                target = new Vector3(hit.point.x, air_height, hit.point.z);

                //Debug.Log("current target AFTER update: " + target.ToString());
                //seeker.StartPath(transform.position, target, OnPathComplete);
            }
        }

        if (agentIndex != -1)
        {
            RVO.Vector2 agent_update = simulatorAir.getAgentPosition(agentIndex);

            //Debug.Log("Current 2D corrdinates of air unit = " + agent_update.ToString());
            Vector3 moveTowards = new Vector3(agent_update.x(), transform.position.y, agent_update.y());
            //Vector3 velocity = (moveTowards - transform.position).normalized * speed;
            transform.position = moveTowards;
            //Debug.Log("Current Position of air unit = " + transform.position.ToString());
            // Slow down smoothly upon approaching the end of the path
            // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
            //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

            //Debug.Log("Current WayPoint = " + path.vectorPath[currentNodeIndex].ToString());
            //Debug.Log("Current Position = " + transform.position.ToString());

            //Debug.Log("Distance to move = " + velocity.ToString());

            //characterController.SimpleMove(velocity);
        }


    }
    //obtain the next Path node point 
    public RVO.Vector2 goalVector()
    {
        Vector3 goal_vector = (target - transform.position);
        return new RVO.Vector2(goal_vector.x, goal_vector.z);
    }
}
