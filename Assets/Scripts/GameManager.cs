using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get { return _instance; } }

    public GameObject PlayerPrefab;
    public List<Color> PlayerColors = new List<Color>();

    enum GameState
    {
        TitleScreen = 0,
        InGame,
        EndScreen
    }

    private static GameManager _instance = null;
    private GameState currentState = GameState.TitleScreen;
    private GameObject currentLevel;
    private List<Player> players = new List<Player>();

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    // Use this for initialization
    void Start () {
        currentLevel = GameObject.Find("Level");
	}
	
	// Update is called once per frame
	void Update () {
		
        switch(currentState)
        {
            case GameState.TitleScreen:
                if (SceneManager.sceneCount == 1)
                {
                    currentLevel.transform.Translate(Vector3.up * 20);
                    NavCamera.Instance.transform.Translate(Vector3.up * 20);

                    SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
                }
                break;

            case GameState.InGame:
                break;

            case GameState.EndScreen:
                if (SceneManager.sceneCount == 0)
                {
                    SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
                }
                break;
        }
	}

    public void InitGame()
    {
        SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

        Destroy(currentLevel);
        
        //Tween Camera
        NavCamera.Instance.MoveToNewLocation(Vector2.zero);

        currentState = GameState.InGame;
    }

    public void StartGame()
    {
        if (currentLevel == null)
            currentLevel = GameObject.Find("Level");

        //Instantiate players
        //Find spawn points
        GameObject p1SP = null;
        GameObject p2SP = null;

        foreach (Transform child in currentLevel.transform)
        {
            if (child.name.ToLower().Contains("spawnpoint_p1"))
            {
                p1SP = child.gameObject;
            }

            if (child.name.ToLower().Contains("spawnpoint_p2"))
            {
                p2SP = child.gameObject;
            }
        }

        InstantiatePlayer(p1SP.transform.position, 1);
        InstantiatePlayer(p2SP.transform.position, 2);
    }

    void InstantiatePlayer(Vector3 position, int pNum)
    {
        GameObject p = Instantiate(PlayerPrefab, position, Quaternion.identity, currentLevel.transform);

        Player player = p.GetComponent<Player>();
        player.playerNumber = pNum;

        p.GetComponent<SpriteRenderer>().color = PlayerColors[pNum - 1];

        players.Add(player);
    }
}
