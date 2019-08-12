using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * This GamePlayManager handles target framerate, scene changes and other
 * "backend" stuff what affects the game itself and not gameobjects.
 */
public class GamePlayManager : MonoBehaviour
{
    void Start()
    {
        // TODO: target framerate is 30 by default now. Increase it to 60
        // after game has more meat around bones to tweak stuff better.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        // Restart scene by pressing F5 DEBUG PURPOSES ONLY
        // TODO: lock this behind debug mode or just delete it
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
