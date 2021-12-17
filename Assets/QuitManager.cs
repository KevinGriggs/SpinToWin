//The class simply checks if the player has hit escape and closes the game if so
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitManager : MonoBehaviour
{
    //checks if the player has hit escape and closes the game if so
    void Update()
    {
        if(Input.GetAxis("Quit") == 1)
        {
            Application.Quit();
        }
    }
}
