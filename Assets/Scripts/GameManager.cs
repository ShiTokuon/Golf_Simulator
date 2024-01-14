using EasyTransition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TransitionSettings transitionSettings;
    public float loadDelay;

    public void LoadScene(string sceneName)
    {
        TransitionManager.Instance().Transition(sceneName, transitionSettings, loadDelay);
    }
}
