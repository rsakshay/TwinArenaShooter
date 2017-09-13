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
    private static int numPlayers = 2;
    [SerializeField]
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

                    currentLevel.GetComponentInChildren<Canvas>(true).gameObject.SetActive(true);

                    if (SceneManager.sceneCount == 1)
                    {
                        currentLevel.transform.Translate(Vector3.up * 20);
                        NavCamera.Instance.transform.Translate(Vector3.up * 20);

                        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
                    }

                    currentLevel.GetComponentInChildren<Button>().onClick.AddListener(Init2PGame);
                }
                break;

            case GameState.InGame:
                if (NavCamera.Instance.currentState != NavCamera.CameraState.Static)
                    return;

                if (currentLevel == null)
                    if (numPlayers == 2)
                        StartGame(2);
                    else if (numPlayers == 4)
                        StartGame(4);

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

                    currentLevel.GetComponentInChildren<Canvas>(true).gameObject.SetActive(true);

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

    public void Init2PGame()
    {
        if (SceneManager.sceneCount > 1)
            SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

        Destroy(currentLevel);
        
        //Tween Camera
        NavCamera.Instance.MoveToNewLocation(Vector2.zero);

        currentState = GameState.InGame;

        numPlayers = 2;
    }

    public void Init4PGame()
    {
        if (SceneManager.sceneCount > 1)
            SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

        Destroy(currentLevel);

        //Tween Camera
        NavCamera.Instance.MoveToNewLocation(Vector2.zero);

        currentState = GameState.InGame;

        numPlayers = 4;
    }

    void StartGame(int numPlayers)
    {
        if (currentLevel == null)
            currentLevel = GameObject.Find("Level");

        SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);

        //Instantiate players
        //Find spawn points
        //GameObject p1SP = null;
        //GameObject p2SP = null;

        List<Transform> spawnPoints = new List<Transform>();

        foreach (Transform child in currentLevel.transform)
        {
            if (child.name.ToLower().Contains("spawnpoint"))
            {
                spawnPoints.Add(child);
            }

            //if (child.name.ToLower().Contains("spawnpoint_p1"))
            //{
            //    p1SP = child.gameObject;
            //}

            //if (child.name.ToLower().Contains("spawnpoint_p2"))
            //{
            //    p2SP = child.gameObject;
            //}

            if (child.name.ToLower().Contains("canvas"))
            {
                child.gameObject.SetActive(true);
            }
        }

        //InstantiatePlayer(p1SP.transform.position, 1);
        //InstantiatePlayer(p2SP.transform.position, 2);

        for (int i = 1; i <= numPlayers; i++)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count - 1);

            Transform spawnPt = spawnPoints[spawnIndex];
            InstantiatePlayer(spawnPt.transform.position, i);

            spawnPoints.RemoveAt(spawnIndex);
        }
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
                winnerText.text = "Player " + (winner + 1) + " Victory!!!";
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

    public Player GetPlayer(int playerNum)
    {
        return players[playerNum - 1];
    }
}
