using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [SerializeField] GameObject previousButton;
    [SerializeField] GameObject nextButton;
    int sceneID = 0;
    int sceneCount;
    #region Singleton
    public static UIManager instance;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
    }

    private void Update()
    {
        if (sceneID == 0)
        {
            previousButton.SetActive(false);
        }
        if (sceneID >= sceneCount-1)
        {
            nextButton.SetActive(false);
        }
        
    }
    public void MoveToNextScene()
    {
        sceneID++;
        SceneManager.LoadScene(sceneID);
        previousButton.SetActive(true);
    }
    public void MoveToPreviousScene()
    {
        sceneID--;
        SceneManager.LoadScene(sceneID);
        nextButton.SetActive(true);
    }
}
