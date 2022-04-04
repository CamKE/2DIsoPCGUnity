using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private LevelManager levelManager;

    [SerializeField]
    private LevelCameraController levelCameraController;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void recenterCamera()
    {
        levelCameraController.recenterCamera();
    }

    public void zoomIn()
    {
        levelCameraController.zoomIn();
    }

    public void zoomOut()
    {
        levelCameraController.zoomOut();
    }

    public void generateLevel()
    {
        levelManager.generate();
    }

    public void randomlyGenerateLevel()
    {

    }

    public void demoLevel()
    {

    }
}
