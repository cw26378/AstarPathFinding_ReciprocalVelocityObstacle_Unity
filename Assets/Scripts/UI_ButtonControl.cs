using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ButtonControl : MonoBehaviour {

    public bool SpawnIsDone = false;
    public bool ToAddGround = false;
    public bool ToAddAir = false;
	
	public void ChangeAddGround () {

        ToAddGround = true;
        ToAddAir = false;
        SpawnIsDone = false;

	}

    public void ChangeAddAir()
    {

        ToAddAir = true;
        ToAddGround = false;
        SpawnIsDone = false;
    }

    public void ReadyForRally()
    {

        SpawnIsDone = true;
        ToAddGround = false;
        ToAddAir = false;
        GameObject.Find("Canvas").GetComponentsInChildren<Button>()[1].interactable = false;
        GameObject.Find("Canvas").GetComponentsInChildren<Button>()[2].interactable = false;
    }
}
