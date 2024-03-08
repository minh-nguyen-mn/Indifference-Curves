using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public MessageController messageControl;
    public GameObject sceneChanger;
    private string nextScene;
    private bool readyToAdvance = false;



    public void Ready(string scene)
    {
        readyToAdvance = true;
        nextScene = scene;
        return;
    }



    public void Advance()
    {
        if (readyToAdvance == true)
        {
            SceneManager.LoadScene(nextScene);
        }
        return;
        
    }

    
}