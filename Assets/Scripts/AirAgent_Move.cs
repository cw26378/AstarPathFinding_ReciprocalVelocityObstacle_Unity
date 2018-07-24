using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirAgent_Move : MonoBehaviour {


    public Camera cam;

    public GameObject leadAirAgent;
    public float air_height = 60.0f;
    public float speed_target;

    [SerializeField]
    Vector3 target;
    //int currentNodeIndex = 0;
    bool goalSelected = false;
    private bool rallyIsReady = false;
    private int numAirUnits = 0;
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
        //simulatorAir = GameObject.FindGameObjectWithTag("RVO_simAir").GetComponent<RVO_SimulatorAir>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        leadAirAgent = GameObject.FindGameObjectWithTag("AirLead");

        characterController = GetComponent<CharacterController>();
        target = transform.position + transform.forward * 0.1f;
        //Debug.Log("Air agent position at Start(): " + transform.position.ToString());
        //Debug.Log("Air agent target position at Start(): " + target.ToString());
        //agentIndex = simulatorAir.addAgentToSimulator(transform.position, gameObject);
        //Debug.Log("Air simulator should have only one agent. True (0) or False(non-zero)? " + agentIndex.ToString());
        //isAbleToStart = true;

        Physics.gravity = Vector3.zero;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        rallyIsReady = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().SpawnIsDone;
        numAirUnits = GameObject.Find("AI").GetComponent<CreateAgent>().N_Air;
        if (rallyIsReady)
        {

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("New click activated! Current target at click: " + Input.mousePosition.ToString());

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    goalSelected = true;
                    // if the clicked point is within range of world
                    target = new Vector3(hit.point.x, air_height, hit.point.z);

                    //Debug.Log("current target AFTER update: " + target.ToString());
                    //seeker.StartPath(transform.position, target, OnPathComplete);
                }
            }
            /*if (goalSelected && (Vector3.Distance(leadAirAgent.transform.position, target) > characterController.radius * 0.5f))
            {
                //Debug.Log("Before move, position = " + transform.position.ToString());
                //every air agent uses the same goal vector since they can all be shifted together
                Vector3 goal_vector = target - leadAirAgent.transform.position;

                Vector3 velocity = new Vector3(goal_vector.x, 0.0f, goal_vector.z).normalized * speed_target;
                //Debug.Log("Velocity of the current move = " + velocity.ToString());

                //Debug.Log("Distance to target = " + Vector3.Distance(transform.position, target).ToString());

                transform.position += velocity * Time.deltaTime;
                //Debug.Log("after move, position = " + transform.position.ToString());
            }
            */
            if (goalSelected)
            {
                Debug.Log("Lead Agent is at: " + leadAirAgent.transform.position.ToString());
                if (Vector3.Distance(transform.position, target) > characterController.radius * numAirUnits)
                {
                    Vector3 dir_vector = target - transform.position;
                    Vector3 velocity = new Vector3(dir_vector.x, 0.0f, dir_vector.z).normalized * speed_target;
                    transform.position += velocity * Time.deltaTime;

                }
                else if (Vector3.Distance(leadAirAgent.transform.position, target) > characterController.radius * 0.5f)
                {
                    //Debug.Log("Before move, position = " + transform.position.ToString());
                    //every air agent uses the same goal vector since they can all be shifted together
                    Vector3 goal_vector = target - leadAirAgent.transform.position;

                    Vector3 velocity = new Vector3(goal_vector.x, 0.0f, goal_vector.z).normalized * speed_target;
                    //Debug.Log("Velocity of the current move = " + velocity.ToString());

                    //Debug.Log("Distance to target = " + Vector3.Distance(transform.position, target).ToString());

                    transform.position += velocity * Time.deltaTime;
                    //Debug.Log("after move, position = " + transform.position.ToString());
                }


            }

        }

    }

}
