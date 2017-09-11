using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    private int winner = 0;

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
	}
	
	// Update is called once per frame
	void Update () {
		
        switch(currentState)
        {
            case GameState.TitleScreen:
                if (NavCamera.Instance.currentState != NavCamera.CameraState.Static)
                    return;

                if (currentLevel == null)
                {
                    foreach (GameObject go in gameObject.scene.GetRootGameObjects())
                    {
                        if (go.name.ToLower().Contains("level"))
                            currentLevel = go;
                    }

                    if (!currentLevel.activeSelf)
                        currentLevel.SetActive(true);

                    if (SceneManager.sceneCount == 1)
                    {
                        currentLevel.transform.Translate(Vector3.up * 20);
                        NavCamera.Instance.transform.Translate(Vector3.up * 20);

                        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
                    }

                    currentLevel.GetComponentInChildren<Button>().onClick.AddListener(InitGame);
                }
                break;

            case GameState.InGame:
                if (NavCamera.Instance.currentState != NavCamera.CameraState.Static)
                    return;

                if (currentLevel == null)
                    StartGame();

                if (CheckIfPlayersAlive())
                    UpdateUI();
                else
                    EndGame();
                break;

            case GameState.EndScreen:
                if (currentLevel == null)
                {
                    foreach (GameObject go in gameObject.scene.GetRootGameObjects())
                    {
                        if (go.name.ToLower().Contains("level"))
                            currentLevel = go;
                    }

                    if (!currentLevel.activeSelf)
                        currentLevel.SetActive(true);

                    if (SceneManager.sceneCount == 1)
                    {
                        currentLevel.transform.Translate(Vector3.down * 20);
                        NavCamera.Instance.transform.Translate(Vector3.down * 20);

                        SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
                    }
                    
                    UpdateWinnerText();

                    currentLevel.GetComponentInChildren<Button>().onClick.AddListener(MainMenu);
                }
                break;
        }
	}

    public void InitGame()
    {
        if (SceneManager.sceneCount > 1)
            SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

        Destroy(currentLevel);
        
        //Tween Camera
        NavCamera.Instance.MoveToNewLocation(Vector2.zero);

        currentState = GameState.InGame;
    }

    void StartGame()
    {
        if (currentLevel == null)
            currentLevel = GameObject.Find("Level");

        SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);

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

    void EndGame()
    {
        SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

        Destroy(currentLevel);
        currentState = GameState.EndScreen;
    }

    void InstantiatePlayer(Vector3 position, int pNum)
    {
        GameObject p = Instantiate(PlayerPrefab, position, Quaternion.identity, currentLevel.transform);

        Player player = p.GetComponent<Player>();
        player.playerNumber = pNum;

        p.GetComponent<SpriteRenderer>().color = PlayerColors[pNum - 1];

        players.Add(player);
    }

    void UpdateUI()
    {
        Transform canvas = currentLevel.GetComponentInChildren<Canvas>(true).transform;

        foreach(Transform child in canvas)
        {
            if (child.name.ToLower().Contains("p1hp"))
            {
                child.GetComponent<Text>().text = "Player 1 HP: " + players[0].HP;
            }

            if (child.name.ToLower().Contains("p2hp"))
            {
                child.GetComponent<Text>().text = "Player 2 HP: " + players[1].HP;
            }
        }
    }

    bool CheckIfPlayersAlive()
    {
        int win = 1;
        foreach (Player player in players)
        {
            if (player == null)
            {
                winner = win;
                return false;
            }

            win = (win + 1) % 2;
        }

        return true;
    }

    void UpdateWinnerText()
    {
        if (currentLevel == null)
            return;

        Transform canvas = currentLevel.GetComponentInChildren<Canvas>(true).transform;

        foreach (Transform child in canvas)
        {
            if (child.name.ToLower().Contains("winnertext"))
            {
                Text winnerText = child.GetComponent<Text>();
                winnerText.text = "Player " + (winner + 1) + " Vectory!!!";
                winnerText.color = PlayerColors[winner];
            }
        }
    }

    public void MainMenu()
    {
        //Tween Camera
        NavCamera.Instance.MoveToNewLocation(Vector2.zero);

        SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

        Destroy(currentLevel);

        currentState = GameState.TitleScreen;

        players.Clear();
        winner = 0;
    }
}
