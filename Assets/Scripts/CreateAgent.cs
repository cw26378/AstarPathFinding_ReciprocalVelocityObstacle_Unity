using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAgent : MonoBehaviour {
    float height = 60.0f;
    // Use this for initialization
    public GameObject new_RVO_agent;
    public GameObject new_agentAir;
    public int N_Air = 0;
    public List<GameObject> listAirAgents = new List<GameObject>();


    //public bool IsAdded = false;
    private bool SpawnIsDone;
    private bool IsGround;
    private bool IsAir;

    /*void Start()
    {
        SpawnIsDone = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().SpawnIsDone;
        IsGround = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().ToAddGround;
        IsAir = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().ToAddAir;
    
    }
    */

    /*
    void PushAwayBFS( GameObject newAgent, List<GameObject> AgentList)
    {
        Vector2 newAgent2D = new Vector2(newAgent.transform.position.x, newAgent.transform.position.z);
        if (AgentList.Count == 0)
            return;
        // calculate distance to agent
        foreach(var agent in AgentList)
        {
            Vector2 currentAgent2D = new Vector2(agent.transform.position.x, agent.transform.position.z);
            float distToNewAgent = Vector2.Distance(currentAgent2D, newAgent2D);

            Debug.Log("new agent is at " + newAgent2D.ToString());
            Debug.Log("current agent is at " + currentAgent2D.ToString());
            Debug.Log("distance in between: " + distToNewAgent.ToString());

            agent.GetComponent<RVO_Agent>().distToNew = distToNewAgent;

            float delta = newAgent.GetComponent<CharacterController>().radius + agent.GetComponent<CharacterController>().radius - distToNewAgent;

            Debug.Log("space margin for drop the agent(if positive, need to move the current agent):"+ delta.ToString());


            if (delta >= 0)
            {
                Debug.Log("too crowded! need to move to give room for new agent ...");
                Vector2 dirMove = (currentAgent2D - newAgent2D).normalized;
                Debug.Log("direction to move:" + dirMove.ToString());

                agent.transform.Translate( delta * new Vector3(dirMove.x, 0, dirMove.y));
            }

        }
    }
    */
	void FixedUpdate () {
        Vector3 spawnTarget;

        SpawnIsDone = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().SpawnIsDone;
        IsGround = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().ToAddGround;
        IsAir = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().ToAddAir;
        //IsAdded = false;

        if(Input.GetMouseButtonDown(0) && SpawnIsDone == false){
            Camera cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                spawnTarget = hit.point;

                Debug.Log("hit.point is: "+ hit.point.ToString());

                GameObject newAgent;
                if (IsGround && IsAir)
                {
                    Debug.Log("Error! Cannot be air and ground at the same time...Please select one...");
                    return;
                }

                else if (IsGround)
                {
                    if (AstarPath.active.GetNearest(hit.point).node.Walkable == false)
                    {
                        // Only ground units have the limit of walkable or not
                        Debug.Log("Cannot deploy units on non-walkable region!");
                        return;
                    }

                    Debug.Log("New Ground Unit Deployed!");
                    //IsAdded = true;
                    newAgent = Instantiate(new_RVO_agent, new Vector3(spawnTarget.x, transform.position.y, spawnTarget.z), Quaternion.Euler(0, 0, 0)) as GameObject;
                    //Using the idea of breadth first search "ripple out" the inserted new agent
                    //PushAwayBFS(newAgent, GameObject.FindGameObjectWithTag("RVO_sim").GetComponent<RVO_Simulator>().rvoGameObjs);

                }
                else if(IsAir){
                    Debug.Log("New Air Unit Deployed");
                    spawnTarget.y = height;
                    newAgent = Instantiate(new_agentAir, new Vector3(spawnTarget.x, spawnTarget.y, spawnTarget.z), Quaternion.Euler(0, 0, 0)) as GameObject;
                    N_Air += 1;
                    newAgent.GetComponent<AirAgent_Move>().current_index = N_Air;
                    listAirAgents.Add(newAgent);
                }
                else{
                    Debug.Log("Nothing is generated");
                }
                //newAgent.cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();;
            }
        }
		
	}
}
