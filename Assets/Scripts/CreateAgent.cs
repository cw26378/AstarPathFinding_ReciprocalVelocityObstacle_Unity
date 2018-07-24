using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAgent : MonoBehaviour {
    float height = 60.0f;
    // Use this for initialization
    public GameObject new_RVO_agent;
    public GameObject new_agentAir;
    public int N_Air = 0;
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

    // Update is called once per frame
	void FixedUpdate () {
        Vector3 spawnTarget;

        SpawnIsDone = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().SpawnIsDone;
        IsGround = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().ToAddGround;
        IsAir = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_ButtonControl>().ToAddAir;

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
                    newAgent = Instantiate(new_RVO_agent, new Vector3(spawnTarget.x, transform.position.y, spawnTarget.z), Quaternion.Euler(0, 0, 0)) as GameObject;
                }
                else if(IsAir){
                    Debug.Log("New Air Unit Deployed");
                    spawnTarget.y = height;
                    newAgent = Instantiate(new_agentAir, new Vector3(spawnTarget.x, spawnTarget.y, spawnTarget.z), Quaternion.Euler(0, 0, 0)) as GameObject;
                    N_Air += 1;
                }
                else{
                    Debug.Log("Nothing is generated");
                }
                //newAgent.cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();;
            }
        }
		
	}
}
