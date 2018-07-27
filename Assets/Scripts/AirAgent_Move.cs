using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirAgent_Move : MonoBehaviour {


    public Camera cam;

    public GameObject leadAirAgent;
    public float air_height = 60.0f;
    public float speed_target;
    public int current_index = -1;

    [SerializeField]
    Vector3 target;
    //int currentNodeIndex = 0;
    bool goalSelected = false;
    //bool firstRally = false;
    private bool rallyIsReady = false;
    private int numAirUnits = 0;
    //private List<Vector3> pathNodes = null;
    //bool isAbleToStart = false;
    //float thresholdToNode = 4.0f;
    Vector3 init_position;
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

        init_position = transform.position;
        Physics.gravity = Vector3.zero;
    }

    void CheckPushAwayAir()
    {   if (GameObject.Find("AI").GetComponent<CreateAgent>().listAirAgents.Count == 0)
            return;

        foreach (var agent in GameObject.Find("AI").GetComponent<CreateAgent>().listAirAgents)
        {
            float agentDist = Vector3.Distance(agent.transform.position, transform.position);
            //Debug.Log("the two radii together: " + (characterController.radius + agent.GetComponent<CharacterController>().radius));
            if (agentDist > 0 && agentDist < (characterController.radius + agent.GetComponent<CharacterController>().radius) * 1.0f)
            {
                Vector3 dirMove = (transform.position - agent.transform.position).normalized;
                Debug.Log("Air units get too close, start to push away each other");
                transform.Translate(dirMove * speed_target * Time.deltaTime);
            }
        }

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
                //Debug.Log("New click activated! Current target at click: " + Input.mousePosition.ToString());

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    goalSelected = true;

                    // if the clicked point is within range of world
                    target = new Vector3(hit.point.x, air_height, hit.point.z);

                }
            }
            if (goalSelected)
            {
                init_position = transform.position;
                if (Vector3.Distance(transform.position, target) > characterController.radius * 1.0f)
                {
                    Vector3 goal_vector = target - transform.position;

                    Vector3 velocity = new Vector3(goal_vector.x, 0.0f, goal_vector.z).normalized * speed_target;

                    transform.position += velocity * Time.deltaTime;

                    CheckPushAwayAir();
                }
                if(Vector3.Distance(transform.position, init_position) < characterController.radius * 0.1f) // && Vector3.Distance(transform.position, target) < characterController.radius * 2.0f
                {
                    Debug.Log("Start to see jitter...");
                    goalSelected = false;
                }

            }

            CheckPushAwayAir();
            /*
            if (goalSelected)
            {
                //Debug.Log("Lead Agent is at: " + leadAirAgent.transform.position.ToString());
                // target can be slightly different according to the index of agent
                if (Vector3.Distance(transform.position, target) > characterController.radius * numAirUnits)
                {
                    Vector3 dir_vector = target - init_position;
                    Debug.Log("current dir vector:" + dir_vector.ToString());
                    Vector3 velocity = new Vector3(dir_vector.x, 0.0f, dir_vector.z).normalized * speed_target;
                    transform.Translate(velocity * Time.deltaTime);

                }
                if (Vector3.Distance(leadAirAgent.transform.position, transform.position) > characterController.radius * 5.0f)
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
            */



        }

    }

}
