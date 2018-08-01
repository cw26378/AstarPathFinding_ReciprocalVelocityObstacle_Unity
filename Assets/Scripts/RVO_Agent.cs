using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using RVO;

public class RVO_Agent : MonoBehaviour {

    public Camera cam;


    [SerializeField]
    Vector3 target;
    int currentNodeIndex = 0;
    int agentIndex = -1;

    RVO_Simulator simulator = null;
    private List<Vector3> pathNodes = new List<Vector3>();
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

        yield return StartCoroutine(StartPaths());
            
        agentIndex = simulator.addAgentToSimulator(transform.position, gameObject, pathNodes);
    }
    IEnumerator StartPaths()
    {
        seeker = gameObject.GetComponent<Seeker>();
        target = transform.position + transform.forward * simulator.speed_target * 0.01f;
        path = seeker.StartPath(transform.position, new Vector3(target.x, transform.position.y, target.z), OnPathComplete);
        yield return StartCoroutine(path.WaitForPath());
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
            // Yay, now we can get a Vector3 representation of the path from p.vectorPath
            path = p;
            pathNodes = p.vectorPath;

            currentNodeIndex = 0;
        }
    }

	// Update is called once per frame
	void FixedUpdate () 
    {
        rallyIsReady = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().SpawnIsDone;
        //UseAStar = true;

        if (!rallyIsReady)
        {
            //Debug.Log("Is there new unit created? "+ GameObject.Find("AI").GetComponent<CreateAgent>().IsAdded.ToString());
            CheckPushAway();
        }
        if (rallyIsReady)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("New click activated! Current target at click: " + Input.mousePosition.ToString());

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.point;
                    Debug.Log("current target AFTER update: " + target.ToString());

                    //depending on the estimated length of path, decide if A* or potential field is used

                    // A* algorithm time complexity is O(b^d) ~ O((pathlength)^3)
                    float pathLengthEstimated = (Vector3.Distance(transform.position, new Vector3(target.x, transform.position.y, target.z)) / AstarPath.active.data.gridGraph.nodeSize) ;
                    int complexityEstimated = (int) Mathf.Pow(pathLengthEstimated, 3);

                    if(simulator.agentPositions.Count * complexityEstimated > AstarPath.active.data.gridGraph.Depth * AstarPath.active.data.gridGraph.Width)
                    {
                        Debug.Log("A* is not efficient due to long path or large number of agents...");

                        //update the pathnodes based on potential field method.
                        //generate the potential map based on destination target and gridgraph

                        var gridgraph = AstarPath.active.data.gridGraph;
                        int[,] potentialmap = new int[gridgraph.Depth, gridgraph.Width];

                        //generate potential map
                        potentialmap = GameObject.Find("PotentialMap").GetComponent<PotentialMapSet>().GetPotentialMap(target, gridgraph);

                        //check the potentialmap
                        //Debug.Log("potential map is obtained. Above origin, the potential is like : " + potentialmap[48, 49].ToString());
                        //Debug.Log("potential Map is obtained. Below origin, the potential is like : " + potentialmap[51, 49].ToString());

                        int[,] padded_potentialmap = GameObject.Find("PotentialMap").GetComponent<PotentialMapSet>().PadMap(potentialmap);

                        // visualize the potential map real-time(TODO)


                        // generate the field map by doing the gradient of padded potential map
                        UnityEngine.Vector2[,] fieldmap = GameObject.Find("PotentialMap").GetComponent<PotentialMapSet>().GetFieldMap(padded_potentialmap);

                        //Debug.Log("FieldMap is obtained. Above origin, the vector is like : " + fieldmap[49, 49].ToString());
                        //Debug.Log("FieldMap is obtained. Below origin, the vector is like : " + fieldmap[50, 49].ToString());

                        pathNodes = GameObject.Find("PotentialMap").GetComponent<PotentialMapSet>().GetPathFromFieldMap(transform.position, new Vector3(target.x, transform.position.y, target.z), fieldmap);
                        currentNodeIndex = 0;

                    }
                    else
                    {
                        seeker.StartPath(transform.position, new Vector3(target.x, transform.position.y, target.z), OnPathComplete);
                        //Debug.Log("new path AFTER update has waypoint list length: " + path.vectorPath.Count.ToString()); 
                    }

                }
            }

            if (path == null || pathNodes == null)
            {
                Debug.Log("No available path found!");
                return;

            }
            if (currentNodeIndex >= (pathNodes.Count - 1))
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

                //characterController.SimpleMove(velocity);
            }
        }	
	}

    //obtain the next Path node point 
    public RVO.Vector2 nextPathNode()
    {
        Vector3 node_pos;
        if (pathNodes == null)
        {
            // if the pathNodes is empty, return current position
            node_pos = transform.position;
            return new RVO.Vector2(node_pos.x, node_pos.z);

        }
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
