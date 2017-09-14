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
    private static int numPlayers = 0;
    [SerializeField]
    private GameState currentState = GameState.TitleScreen;
    private GameObject currentLevel;
    private List<Player> players = new List<Player>();
    private int winner = 0;
    private Dictionary<int, Text> HPTexts = new Dictionary<int, Text>();

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

                    foreach (Button button in currentLevel.GetComponentsInChildren<Button>())
                    {
                        if (button.gameObject.name.ToLower().Contains("2p"))
                            button.onClick.AddListener(Init2PGame);

                        if (button.gameObject.name.ToLower().Contains("4p"))
                            button.onClick.AddListener(Init4PGame);
                    }
                }
                break;

            case GameState.InGame:
                if (NavCamera.Instance.currentState != NavCamera.CameraState.Static)
                    return;

                if (currentLevel == null)
                {
                    if (numPlayers == 0)
                    {
                        Debug.Log("numPlayers is 0. Calculating num of players...");
                        numPlayers = GameObject.Find("Level").GetComponentsInChildren<Text>(true).Length;
                    }

                    if (numPlayers == 2)
                        StartGame(2);
                    else if (numPlayers == 4)
                        StartGame(4);
                }

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
        InitScene();

        currentLevel.transform.Translate(Vector3.up * 20);
        NavCamera.Instance.transform.Translate(Vector3.up * 20);

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

        numPlayers = 2;

        //StartGame(2);
    }

    void InitScene()
    {
        currentLevel.name += "Prev";
        Destroy(currentLevel);

        //Tween Camera
        NavCamera.Instance.MoveToNewLocation(Vector2.zero);

        currentState = GameState.InGame;
    }

    public void Init4PGame()
    {
        InitScene();

        currentLevel.transform.Translate(Vector3.up * 20);
        NavCamera.Instance.transform.Translate(Vector3.up * 20);

        SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);

        numPlayers = 4;

        //StartGame(4);
    }

    void StartGame(int numPlayers)
    {
        if (SceneManager.sceneCount > 1)
            SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

        currentLevel = GameObject.Find("Level");

        SceneManager.LoadSceneAsync(SceneManager.sceneCountInBuildSettings - 1, LoadSceneMode.Additive);

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

        FindUIElements();
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
        foreach(int pNum in HPTexts.Keys)
        {
            Text hpText;
            HPTexts.TryGetValue(pNum, out hpText);

            if (players[pNum - 1] != null)
            {
                hpText.text = "Player " + pNum + " HP: " + players[pNum - 1].HP;
            }
            else
            {
                hpText.text = "Player " + pNum + " HP: 0";
            }
        }
    }

    void FindUIElements()
    {
        Transform canvas = currentLevel.GetComponentInChildren<Canvas>(true).transform;

        foreach (Transform child in canvas)
        {
            int playerNum = 0;

            if (child.name.ToLower().Contains("p1hp") && players[0] != null)
            {
                //child.GetComponent<Text>().text = "Player 1 HP: " + players[0].HP;
                playerNum = 1;
            }

            if (child.name.ToLower().Contains("p2hp") && players[1] != null)
            {
                //child.GetComponent<Text>().text = "Player 2 HP: " + players[1].HP;
                playerNum = 2;
            }

            if (child.name.ToLower().Contains("p3hp") && players[2] != null)
            {
                //child.GetComponent<Text>().text = "Player 3 HP: " + players[2].HP;
                playerNum = 3;
            }

            if (child.name.ToLower().Contains("p4hp") && players[3] != null)
            {
                //child.GetComponent<Text>().text = "Player 4 HP: " + players[3].HP;
                playerNum = 4;
            }

            HPTexts.Add(playerNum, child.GetComponent<Text>());
        }
    }

    void ClearUIElements()
    {
        HPTexts.Clear();
    }

    bool CheckIfPlayersAlive()
    {
        int numPlayersDead = 0;

        bool[] alivePlayers = new bool[numPlayers];
        int i = 0;

        foreach (Player player in players)
        {
            if (player == null)
            {
                alivePlayers[i] = false;
                numPlayersDead++;
            }
            else
                alivePlayers[i] = true;
            
            i++;
        }

        if (numPlayersDead >= numPlayers - 1)
        {
            for(int j = 0; j < numPlayers; j++)
            {
                if (alivePlayers[j])
                    winner = j;
            }
            return false;
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
        ClearUIElements();
        winner = 0;
    }

    public Player GetPlayer(int playerNum)
    {
        return players[playerNum - 1];
    }
}
