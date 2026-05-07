using UnityEngine;
using Unity.MLAgents.Policies;

public class CameraManager : MonoBehaviour
{
    [Header("All Cameras")]
    public GameObject arenaView;
    public GameObject runnerView;
    public GameObject runnerBackView;
    public GameObject seekerView;
    public GameObject seekerBackView;

    [Header("Agent Behaviors")]
    public BehaviorParameters runnerBehavior; // Blue
    public BehaviorParameters seekerBehavior; // Orange

    private GameObject[] activeCameras;
    private int currentIndex = 0;
    private bool canSwitchCameras = false;

    void Start()
    {
        ShowArenaMenuView();
    }

    void Update()
    {
        if (!canSwitchCameras) return;

        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.K))
        {
            SwitchCamera();
        }
    }

    public void ShowArenaMenuView()
    {
        canSwitchCameras = false;
        currentIndex = 0;

        arenaView.SetActive(true);
        runnerView.SetActive(false);
        runnerBackView.SetActive(false);
        seekerView.SetActive(false);
        seekerBackView.SetActive(false);

        activeCameras = new GameObject[] { arenaView };
    }

    public void SetupCameras()
    {
        bool isSeekerHuman = seekerBehavior != null &&
                             seekerBehavior.BehaviorType == BehaviorType.HeuristicOnly;

        bool isRunnerHuman = runnerBehavior != null &&
                             runnerBehavior.BehaviorType == BehaviorType.HeuristicOnly;

        arenaView.SetActive(false);
        runnerView.SetActive(false);
        runnerBackView.SetActive(false);
        seekerView.SetActive(false);
        seekerBackView.SetActive(false);

        if (isSeekerHuman)
        {
            activeCameras = new GameObject[] { seekerBackView, seekerView, arenaView };
        }
        else if (isRunnerHuman)
        {
            activeCameras = new GameObject[] { runnerBackView, runnerView, arenaView };
        }
        else
        {
            activeCameras = new GameObject[] { arenaView, runnerBackView, seekerBackView, runnerView, seekerView };
        }

        currentIndex = 0;
        activeCameras[currentIndex].SetActive(true);
        canSwitchCameras = true;
    }

    private void SwitchCamera()
    {
        if (activeCameras == null || activeCameras.Length == 0) return;

        activeCameras[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % activeCameras.Length;
        activeCameras[currentIndex].SetActive(true);
    }
}