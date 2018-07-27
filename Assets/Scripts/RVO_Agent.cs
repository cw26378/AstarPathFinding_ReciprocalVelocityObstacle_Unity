using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using RVO;

public class RVO_Agent : MonoBehaviour {

    public Camera cam;
    public float distToNew;

    [SerializeField]
    Vector3 target;
    int currentNodeIndex = 0;
    int agentIndex = -1;

    RVO_Simulator simulator = null;
    private List<Vector3> pathNodes = new List<Vector3>();
    bool isAbleToStart = false;
    private bool rallyIsReady = false;
    float thresholdToNode = 4.0f;

    Seeker seeker;
    Path path;
    CharacterController characterController;

    // Use IEnumerator for initialization

    IEnumerator Start()
    {
        currentNodeIndex = 0;
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        simulator = GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>();
        characterController = GetComponent<CharacterController>();
        // rallyIsReady= GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().SpawnIsDone;
        // pathNodes = new List<Vector3>();
        yield return StartCoroutine(StartPaths());
        agentIndex = simulator.addAgentToSimulator(transform.position, gameObject, pathNodes);
        //Debug.Log("current agent index: "+agentIndex);
        //isAbleToStart = true;
    }
    IEnumerator StartPaths()
    {
        seeker = gameObject.GetComponent<Seeker>();
        target = transform.position + transform.forward * simulator.speed_target * 0.01f;
        //target = Input.mousePosition;
        path = seeker.StartPath(transform.position, new Vector3(target.x, transform.position.y, target.z), OnPathComplete);
        yield return StartCoroutine(path.WaitForPath());
    }

    // no use of IEnumerator
    /*
    void Start()
    {
        currentNodeIndex = 0;
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        simulator = GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>();
        characterController = GetComponent<CharacterController>();

        //StartPaths();

        seeker = gameObject.GetComponent<Seeker>();
        target = transform.position + transform.forward * simulator.speed_target * 0.1f;
        //target = Input.mousePosition;
        path = seeker.StartPath(transform.position, new Vector3(target.x, transform.position.y, target.z), OnPathComplete);


        agentIndex = simulator.addAgentToSimulator(transform.position, gameObject, pathNodes);

        //isAbleToStart = true;
    }
    */
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
            pathNodes = p.vectorPath;

            currentNodeIndex = 0;
        }
    }

	// Update is called once per frame
	void FixedUpdate () 
    {
        rallyIsReady = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().SpawnIsDone;
        if (!rallyIsReady)
        {
            //Debug.Log("Is there new unit created? "+ GameObject.Find("AI").GetComponent<CreateAgent>().IsAdded.ToString());
            CheckPushAway();
        }
        if (rallyIsReady)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("New click activated! Current target at click: " + Input.mousePosition.ToString());

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.point;
                    //Debug.Log("current target AFTER update: " + target.ToString());
                    seeker.StartPath(transform.position, new Vector3(target.x, transform.position.y, target.z), OnPathComplete);
                    //Debug.Log("new path AFTER update has waypoint list length: " + path.vectorPath.Count.ToString());
                }
            }

            if (path == null)
            {
                Debug.Log("No available path found!");
                return;

            }
            if (currentNodeIndex >= (path.vectorPath.Count - 1))
            {
                //Debug.Log("Path Finding ended!");
                return;
            }
            if (agentIndex != -1)
            {
                RVO.Vector2 agent_update = simulator.getAgentPosition(agentIndex);
                Vector3 moveTowards = new Vector3(agent_update.x(), transform.position.y, agent_update.y());
                //Vector3 velocity = (moveTowards - transform.position).normalized * speed;
                transform.position = moveTowards;
                // Slow down smoothly upon approaching the end of the path
                // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
                //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

                //Debug.Log("Current WayPoint = " + path.vectorPath[currentNodeIndex].ToString());
                //Debug.Log("Current Position = " + transform.position.ToString());

                //Debug.Log("Distance to move = " + velocity.ToString());

                //characterController.SimpleMove(velocity);
            }
        }	
	}
    //obtain the next Path node point 
    public RVO.Vector2 nextPathNode()
    {
        Vector3 node_pos;
        if(currentNodeIndex < pathNodes.Count)
        {
            node_pos = pathNodes[currentNodeIndex];
            float distance = Vector3.Distance(node_pos, transform.position);

            if (distance < thresholdToNode)
            {
                currentNodeIndex++;
                node_pos = pathNodes[currentNodeIndex];
            }
        }
        else{
            // last node of the A* path
            node_pos = pathNodes[pathNodes.Count - 1];
        }
        return new RVO.Vector2(node_pos.x, node_pos.z);
    }

    void CheckPushAway()
    {
        //List<GameObject> NearbyAgentList  = new List<GameObject>();
        foreach(var agent in GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>().rvoGameObjs)
        {
            float agentDist = Vector3.Distance(agent.transform.position, transform.position);
            if (agentDist > 0 &&  agentDist < (characterController.radius + agent.GetComponent < CharacterController>().radius) * 1.25f)
            {
                Vector3 dirMove = (transform.position - agent.transform.position).normalized;
                if (AstarPath.active.GetNearest(transform.position + dirMove * Time.deltaTime).node.Walkable == true)
                {
                    transform.Translate(dirMove * Time.deltaTime);
                }
                    
            }
        }

        // the tranform need update in the agentPositionlist
        if (agentIndex != -1)
        {
           GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>().agentPositions[agentIndex] = new RVO.Vector2(transform.position.x, transform.position.z);
        }

    }
}
