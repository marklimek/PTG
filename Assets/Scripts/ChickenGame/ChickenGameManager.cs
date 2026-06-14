using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ChickenColor
{
    DarkBrown,
    LightBrown,
    White,
    Blue
}

public enum SpecialPower
{
    Shield,
    Speed,
    Teleportation
}

public enum WheelType
{
    Teleport,
    Double,
    Loss,
    Passage,
    Hint,
    Rewind
}

public class ChickenGameManager : MonoBehaviour
{
    public static ChickenGameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int numberOfChickens = 20;
    [SerializeField] private int numberOfWheelsPerType = 2;

    [Header("Prefabs & Materials (Optional, will auto-create if null)")]
    public Material darkBrownMat;
    public Material lightBrownMat;
    public Material whiteMat;
    public Material blueMat;
    
    public Material teleportWheelMat;
    public Material doubleWheelMat;
    public Material lossWheelMat;
    public Material passageWheelMat;
    public Material hintWheelMat;
    public Material rewindWheelMat;

    [Header("UI Canvas References (Will auto-setup if null)")]
    public Canvas gameCanvas;
    public Text scoreText;
    public Text powerText;
    public Text activeBuffsText;
    public GameObject gameOverPanel;
    public Text gameOverTitleText;
    public Text gameOverStatsText;
    public Button restartButton;

    private int score = 0;
    private int maxPossiblePoints = 0;
    private SpecialPower playerPower;
    private bool hasDoublePoints = false;
    private int wallPassCharges = 0;
    private int hintIntersectionsRemaining = 0;

    private int[] spawnedChickens = new int[4];
    private int[] collectedChickens = new int[4];
    private Text[] trackerCountTexts = new Text[4];

    private List<Vector2Int> intersectionHistory = new List<Vector2Int>();
    private Vector2Int lastRecordedIntersection = new Vector2Int(-1, -1);

    private MazeLoader mazeLoader;
    public GameObject playerObj;
    public GameObject introPanel;
    private PlayerMovement playerMovement;

    private GameObject hintArrowInstance;
    private Vector2Int lastPlayerCell = new Vector2Int(-1, -1);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mazeLoader = FindFirstObjectByType<MazeLoader>();
        playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }

        if (mazeLoader != null)
        {
            mazeLoader.OnMazeGenerated += InitializeGame;
        }
        else
        {
            Debug.LogError("ChickenGameManager: MazeLoader not found in scene!");
        }

        SetupMaterials();
        SetupUI();
    }

    private void Update()
    {
        if (playerObj == null) return;

        Vector2Int playerCell = GetCellFromPosition(playerObj.transform.position);
        
        if (playerCell != lastPlayerCell)
        {
            OnPlayerEnterCell(playerCell);
            lastPlayerCell = playerCell;
        }

        if (mazeLoader != null)
        {
            int targetR = mazeLoader.mazeRows - 1;
            int targetC = mazeLoader.mazeColumns - 1;
            if (playerCell.x == targetR && playerCell.y == targetC)
            {
                EndGameSuccess();
            }
        }

        if (playerPower == SpecialPower.Teleportation && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.T)))
        {
            TriggerTeleportPower();
        }

        UpdateHintVisuals();
    }

    private void SetupMaterials()
    {
        if (darkBrownMat == null) darkBrownMat = CreateFlatColorMaterial("DarkBrown", new Color(0.36f, 0.20f, 0.09f));
        if (lightBrownMat == null) lightBrownMat = CreateFlatColorMaterial("LightBrown", new Color(0.68f, 0.44f, 0.22f));
        if (whiteMat == null) whiteMat = CreateFlatColorMaterial("White", Color.white);
        if (blueMat == null) blueMat = CreateFlatColorMaterial("Blue", new Color(0.12f, 0.53f, 0.90f));

        if (teleportWheelMat == null) teleportWheelMat = CreateFlatColorMaterial("TeleportWheel", new Color(0.63f, 0.13f, 0.94f));
        if (doubleWheelMat == null) doubleWheelMat = CreateFlatColorMaterial("DoubleWheel", Color.green);
        if (lossWheelMat == null) lossWheelMat = CreateFlatColorMaterial("LossWheel", Color.red);
        if (passageWheelMat == null) passageWheelMat = CreateFlatColorMaterial("PassageWheel", new Color(1.0f, 0.64f, 0.0f));
        if (hintWheelMat == null) hintWheelMat = CreateFlatColorMaterial("HintWheel", Color.magenta);
        if (rewindWheelMat == null) rewindWheelMat = CreateFlatColorMaterial("RewindWheel", Color.cyan);
    }

    private Material CreateFlatColorMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Legacy Shaders/Diffuse");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader);
        mat.name = name;
        mat.color = color;
        return mat;
    }

    private void SetupUI()
    {
        if (gameCanvas == null)
        {
            Canvas existing = FindFirstObjectByType<Canvas>();
            if (existing != null) gameCanvas = existing;
        }

        if (gameCanvas != null)
        {
            if (scoreText != null)
            {
                Transform legendPanel = gameCanvas.transform.Find("LegendContainer/LegendPanel");
                if (legendPanel != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Transform row = legendPanel.Find("Row_" + i);
                        if (row != null)
                        {
                            Transform counter = row.Find("Counter");
                            if (counter != null)
                            {
                                trackerCountTexts[i] = counter.GetComponent<Text>();
                            }
                        }
                    }
                }

                if (introPanel != null)
                {
                    Button startBtn = introPanel.GetComponentInChildren<Button>();
                    if (startBtn != null)
                    {
                        startBtn.onClick.RemoveAllListeners();
                        startBtn.onClick.AddListener(CloseIntroPanel);
                    }
                }

                if (restartButton != null)
                {
                    restartButton.onClick.RemoveAllListeners();
                    restartButton.onClick.AddListener(RestartGame);
                }

                Debug.Log("ChickenGameManager: Successfully wired pre-existing Scene UI!");
                return;
            }

            Transform hpObj = gameCanvas.transform.Find("HP");
            if (hpObj != null)
            {
                hpObj.gameObject.SetActive(false);
            }

            Transform statWindow = gameCanvas.transform.Find("Stat window");
            if (statWindow != null)
            {
                statWindow.gameObject.SetActive(false);
            }

            scoreText = CreateTextElement("ScoreText", gameCanvas.transform, "Punkty: 0", new Vector2(30, -30), 24, Color.white);
            scoreText.alignment = TextAnchor.MiddleLeft;
            RectTransform rt = scoreText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(30, -30);

            powerText = CreateTextElement("PowerText", gameCanvas.transform, "Moc specjalna: Losowanie...", new Vector2(30, -70), 20, Color.yellow);
            powerText.alignment = TextAnchor.MiddleLeft;
            RectTransform rtP = powerText.GetComponent<RectTransform>();
            rtP.anchorMin = new Vector2(0, 1);
            rtP.anchorMax = new Vector2(0, 1);
            rtP.pivot = new Vector2(0, 1);
            rtP.anchoredPosition = new Vector2(30, -70);

            activeBuffsText = CreateTextElement("BuffsText", gameCanvas.transform, "Aktywne efekty: Brak", new Vector2(30, -110), 18, Color.cyan);
            activeBuffsText.alignment = TextAnchor.UpperLeft;
            RectTransform rtB = activeBuffsText.GetComponent<RectTransform>();
            rtB.anchorMin = new Vector2(0, 1);
            rtB.anchorMax = new Vector2(0, 1);
            rtB.pivot = new Vector2(0, 1);
            rtB.anchoredPosition = new Vector2(30, -110);
            rtB.sizeDelta = new Vector2(800, 120); 

            GameObject trackerContainer = new GameObject("LegendContainer", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            trackerContainer.transform.SetParent(gameCanvas.transform, false);
            RectTransform containerRt = trackerContainer.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(1, 1);
            containerRt.anchorMax = new Vector2(1, 1);
            containerRt.pivot = new Vector2(1, 1);
            containerRt.sizeDelta = new Vector2(440, 320); 
            containerRt.anchoredPosition = new Vector2(-40, -40);

            UnityEngine.UI.Image containerImg = trackerContainer.GetComponent<UnityEngine.UI.Image>();
            containerImg.color = new Color(0.9f, 0.72f, 0.08f, 1f); 

            GameObject trackerPanel = new GameObject("LegendPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            trackerPanel.transform.SetParent(trackerContainer.transform, false);
            RectTransform trackerRt = trackerPanel.GetComponent<RectTransform>();
            trackerRt.anchorMin = Vector2.zero;
            trackerRt.anchorMax = Vector2.one;
            trackerRt.pivot = new Vector2(0.5f, 0.5f);
            trackerRt.sizeDelta = new Vector2(-6, -6); 
            trackerRt.anchoredPosition = Vector2.zero;

            UnityEngine.UI.Image trackerImg = trackerPanel.GetComponent<UnityEngine.UI.Image>();
            trackerImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f); 

            Text trackerTitle = CreateTextElement("TrackerTitle", trackerPanel.transform, "<b><color=gold>🎯 CEL: KURCZAKI</color></b>", new Vector2(20, -15), 24, Color.white);
            trackerTitle.alignment = TextAnchor.UpperLeft;
            trackerTitle.supportRichText = true;
            RectTransform titleRt = trackerTitle.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0, 1);
            titleRt.anchoredPosition = new Vector2(20, -15);
            titleRt.sizeDelta = new Vector2(-40, 35);

            string[] names = { "Ciemnobrązowy", "Jasnobrązowy", "Biały", "Niebieski" };
            Color[] visualColors = {
                new Color(0.36f, 0.20f, 0.09f), // Dark Brown
                new Color(0.68f, 0.44f, 0.22f), // Light Brown
                Color.white,                    // White
                new Color(0.12f, 0.53f, 0.90f)  // Blue
            };
            int[] pts = { 1, 1, 1, 3 };

            for (int i = 0; i < 4; i++)
            {
                float rowY = -60 - (i * 60); 

                GameObject rowObj = new GameObject("Row_" + i, typeof(RectTransform));
                rowObj.transform.SetParent(trackerPanel.transform, false);
                RectTransform rowRt = rowObj.GetComponent<RectTransform>();
                rowRt.anchorMin = new Vector2(0, 1);
                rowRt.anchorMax = new Vector2(1, 1);
                rowRt.pivot = new Vector2(0, 1);
                rowRt.anchoredPosition = new Vector2(0, rowY);
                rowRt.sizeDelta = new Vector2(-20, 52); 

                GameObject chickenIcon = new GameObject("ChickenIcon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                chickenIcon.transform.SetParent(rowObj.transform, false);
                RectTransform iconRt = chickenIcon.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0, 0.5f);
                iconRt.anchorMax = new Vector2(0, 0.5f);
                iconRt.pivot = new Vector2(0, 0.5f);
                iconRt.anchoredPosition = new Vector2(20, 0);
                iconRt.sizeDelta = new Vector2(40, 30); 

                UnityEngine.UI.Image iconImg = chickenIcon.GetComponent<UnityEngine.UI.Image>();
                iconImg.color = visualColors[i];

                // red comb on the icon
                GameObject combObj = new GameObject("Comb", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                combObj.transform.SetParent(chickenIcon.transform, false);
                RectTransform combRt = combObj.GetComponent<RectTransform>();
                combRt.anchorMin = new Vector2(0.5f, 1);
                combRt.anchorMax = new Vector2(0.5f, 1);
                combRt.pivot = new Vector2(0.5f, 0);
                combRt.anchoredPosition = new Vector2(-3, 0);
                combRt.sizeDelta = new Vector2(10, 8);
                combObj.GetComponent<UnityEngine.UI.Image>().color = Color.red;

                // yellow beak on the icon
                GameObject beakObj = new GameObject("Beak", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                beakObj.transform.SetParent(chickenIcon.transform, false);
                RectTransform beakRt = beakObj.GetComponent<RectTransform>();
                beakRt.anchorMin = new Vector2(1, 0.5f);
                beakRt.anchorMax = new Vector2(1, 0.5f);
                beakRt.pivot = new Vector2(0, 0.5f);
                beakRt.anchoredPosition = new Vector2(0, -1);
                beakRt.sizeDelta = new Vector2(8, 5);
                beakObj.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;

                string labelText = string.Format("<b><color=#{0}>{1}</color></b> <color=#999999>({2}p)</color>", ColorUtility.ToHtmlStringRGB(visualColors[i]), names[i], pts[i]);
                Text label = CreateTextElement("Label", rowObj.transform, labelText, new Vector2(80, 0), 20, Color.white);
                label.alignment = TextAnchor.MiddleLeft;
                label.supportRichText = true;
                RectTransform labelRt = label.GetComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0, 0);
                labelRt.anchorMax = new Vector2(0.5f, 1);
                labelRt.pivot = new Vector2(0, 0.5f);
                labelRt.anchoredPosition = new Vector2(80, 0);
                labelRt.sizeDelta = new Vector2(210, 0);

                Text counterText = CreateTextElement("Counter", rowObj.transform, "0 / 0", new Vector2(-20, 0), 22, Color.white);
                counterText.alignment = TextAnchor.MiddleRight;
                counterText.supportRichText = true;
                counterText.fontStyle = FontStyle.Bold;
                RectTransform counterRt = counterText.GetComponent<RectTransform>();
                counterRt.anchorMin = new Vector2(1, 0);
                counterRt.anchorMax = new Vector2(1, 1);
                counterRt.pivot = new Vector2(1, 0.5f);
                counterRt.anchoredPosition = new Vector2(-20, 0);
                counterRt.sizeDelta = new Vector2(110, 0);

                trackerCountTexts[i] = counterText;
            }

            GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(gameCanvas.transform, false);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            Image panelImg = panel.GetComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.85f);
            panel.SetActive(false);
            gameOverPanel = panel;

            gameOverTitleText = CreateTextElement("GameOverTitle", panel.transform, "KONIEC GRY", new Vector2(0, 150), 42, Color.gold);
            gameOverTitleText.alignment = TextAnchor.MiddleCenter;
            RectTransform gameOverTitleRt = gameOverTitleText.GetComponent<RectTransform>();
            gameOverTitleRt.anchorMin = new Vector2(0.5f, 0.5f);
            gameOverTitleRt.anchorMax = new Vector2(0.5f, 0.5f);
            gameOverTitleRt.pivot = new Vector2(0.5f, 0.5f);
            gameOverTitleRt.anchoredPosition = new Vector2(0, 150);

            gameOverStatsText = CreateTextElement("GameOverStats", panel.transform, "Zebrane punkty: 0 / 100\nTrofeum: Brązowe", new Vector2(0, 20), 24, Color.white);
            gameOverStatsText.alignment = TextAnchor.MiddleCenter;
            RectTransform statsRt = gameOverStatsText.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0.5f, 0.5f);
            statsRt.anchorMax = new Vector2(0.5f, 0.5f);
            statsRt.pivot = new Vector2(0.5f, 0.5f);
            statsRt.anchoredPosition = new Vector2(0, 20);

            GameObject btnObj = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(panel.transform, false);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.5f);
            btnRt.anchorMax = new Vector2(0.5f, 0.5f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.sizeDelta = new Vector2(250, 50);
            btnRt.anchoredPosition = new Vector2(0, -100);

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            Text btnTxt = CreateTextElement("BtnText", btnObj.transform, "Zagraj Ponownie", Vector2.zero, 18, Color.white);
            btnTxt.alignment = TextAnchor.MiddleCenter;
            RectTransform btnTxtRt = btnTxt.GetComponent<RectTransform>();
            btnTxtRt.anchorMin = Vector2.zero;
            btnTxtRt.anchorMax = Vector2.one;
            btnTxtRt.pivot = new Vector2(0.5f, 0.5f);
            btnTxtRt.sizeDelta = Vector2.zero;
            btnTxtRt.anchoredPosition = Vector2.zero;

            restartButton = btnObj.GetComponent<Button>();
            restartButton.onClick.AddListener(RestartGame);

            GameObject intro = new GameObject("IntroPanel", typeof(RectTransform), typeof(Image));
            intro.transform.SetParent(gameCanvas.transform, false);
            RectTransform introRt = intro.GetComponent<RectTransform>();
            introRt.anchorMin = new Vector2(0.5f, 0.5f);
            introRt.anchorMax = new Vector2(0.5f, 0.5f);
            introRt.pivot = new Vector2(0.5f, 0.5f);
            introRt.sizeDelta = new Vector2(1150, 820); 
            introRt.anchoredPosition = Vector2.zero;

            Image introImg = intro.GetComponent<Image>();
            introImg.color = new Color(0, 0, 0, 0.94f);

            Text introTitle = CreateTextElement("IntroTitle", intro.transform, "<b><color=gold>LABIRYNT KURCZAKÓW</color></b>", new Vector2(0, 240), 54, Color.white);
            introTitle.alignment = TextAnchor.MiddleCenter;
            introTitle.supportRichText = true;
            RectTransform introTitleRt = introTitle.GetComponent<RectTransform>();
            introTitleRt.anchorMin = new Vector2(0.5f, 0.5f);
            introTitleRt.anchorMax = new Vector2(0.5f, 0.5f);
            introTitleRt.pivot = new Vector2(0.5f, 0.5f);
            introTitleRt.sizeDelta = new Vector2(1050, 70);
            introTitleRt.anchoredPosition = new Vector2(0, 310);

            string introTextStr = 
                "<b><color=gold><size=30>Cel gry:</size></color></b>\n" +
                "Ukończ labirynt z jak największą liczbą punktów. Zbieraj inne kurczaki chodzące po korytarzach i dojdź do złotego pucharu 🏆 na końcu labiryntu!\n\n" +
                "<b><color=gold><size=30>Moc Specjalna:</size></color></b>\n" +
                "Na starcie otrzymasz losową moc (widoczną w lewym górnym rogu ekranu). Jeśli wylosowałeś Teleportację, użyj <b>SPACJI</b> lub klawisza <b>T</b>, by przenieść się do najbliższego kurczaka.\n\n" +
                "<b><color=gold><size=30>Przeszkadzajki (Koła na ziemi):</size></color></b>\n" +
                "Nadeptywanie na świecące kręgi wyzwala efekty specjalne (np. tarcza chroni przed stratą punktów, kompas wskazuje właściwą drogę, przejście pozwala przenikać przez ściany!).\n\n" +
                "<b><color=gold><size=30>Sterowanie:</size></color></b> WASD / Strzałki do ruchu, Myszka do rozglądania się.";

            Text introContent = CreateTextElement("IntroContent", intro.transform, introTextStr, new Vector2(0, -10), 24, Color.white);
            introContent.alignment = TextAnchor.UpperLeft;
            introContent.supportRichText = true;
            introContent.horizontalOverflow = HorizontalWrapMode.Wrap;
            introContent.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform introContentRt = introContent.GetComponent<RectTransform>();
            introContentRt.anchorMin = new Vector2(0.5f, 0.5f);
            introContentRt.anchorMax = new Vector2(0.5f, 0.5f);
            introContentRt.pivot = new Vector2(0.5f, 0.5f);
            introContentRt.sizeDelta = new Vector2(1050, 540); 
            introContentRt.anchoredPosition = new Vector2(0, -10);

            GameObject startBtnObj = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            startBtnObj.transform.SetParent(intro.transform, false);
            RectTransform startBtnRt = startBtnObj.GetComponent<RectTransform>();
            startBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
            startBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
            startBtnRt.pivot = new Vector2(0.5f, 0.5f);
            startBtnRt.sizeDelta = new Vector2(380, 70); 
            startBtnRt.anchoredPosition = new Vector2(0, -320);

            Image startBtnImg = startBtnObj.GetComponent<Image>();
            startBtnImg.color = new Color(0.15f, 0.55f, 0.15f, 1f);

            Text startBtnTxt = CreateTextElement("StartBtnText", startBtnObj.transform, "ROZPOCZNIJ GRĘ", Vector2.zero, 26, Color.white);
            startBtnTxt.alignment = TextAnchor.MiddleCenter;
            startBtnTxt.fontStyle = FontStyle.Bold;
            RectTransform startBtnTxtRt = startBtnTxt.GetComponent<RectTransform>();
            startBtnTxtRt.anchorMin = Vector2.zero;
            startBtnTxtRt.anchorMax = Vector2.one;
            startBtnTxtRt.pivot = new Vector2(0.5f, 0.5f);
            startBtnTxtRt.sizeDelta = Vector2.zero;
            startBtnTxtRt.anchoredPosition = Vector2.zero;

            Button startButton = startBtnObj.GetComponent<Button>();
            startButton.onClick.AddListener(CloseIntroPanel);

            introPanel = intro;
            introPanel.SetActive(false); 
        }
    }

    private Text CreateTextElement(string name, Transform parent, string initText, Vector2 pos, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(parent, false);
        Text text = textObj.GetComponent<Text>();
        
        try
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (System.Exception)
        {
            try
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch (System.Exception)
            {
                Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Length > 0)
                {
                    text.font = fonts[0];
                }
            }
        }

        if (text.font == null)
        {
            text.font = FindFirstObjectByType<Font>();
        }
        text.text = initText;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 40);
        rt.anchoredPosition = pos;

        return text;
    }

    private void InitializeGame()
    {
        Debug.Log("ChickenGameManager: Initializing Maze Game Assets!");

        AssignRandomPower();

        SpawnExitVisual();

        SpawnChickensAndWheels();

        if (mazeLoader != null)
        {
            mazeLoader.OnMazeGenerated -= InitializeGame;
        }

        UpdateHUD();

        if (introPanel != null)
        {
            introPanel.SetActive(true);
            if (playerMovement != null) playerMovement.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CloseIntroPanel()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(false);
            if (playerMovement != null) playerMovement.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void AssignRandomPower()
    {
        int rand = Random.Range(0, 3);
        playerPower = (SpecialPower)rand;

        if (playerPower == SpecialPower.Speed)
        {
            if (playerMovement != null)
            {
                playerMovement.MoveSpeed = playerMovement.MoveSpeed * 2f;
                Debug.Log("ChickenGameManager: Speed Power assigned! Player speed is now " + playerMovement.MoveSpeed);
            }
        }
        else if (playerPower == SpecialPower.Shield)
        {
            Debug.Log("ChickenGameManager: Shield Power assigned! Point losses are halved.");
        }
        else if (playerPower == SpecialPower.Teleportation)
        {
            Debug.Log("ChickenGameManager: Teleportation Power assigned! Press Space or T to teleport to nearest chicken.");
        }

        UpdateHUD();
    }

    private void SpawnExitVisual()
    {
        int targetR = mazeLoader.mazeRows - 1;
        int targetC = mazeLoader.mazeColumns - 1;
        float size = mazeLoader.size;
        Vector3 exitPos = new Vector3(targetR * size, -0.5f, targetC * size);

        GameObject trophyPlaceholder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trophyPlaceholder.name = "EXIT_TROPHY_GOAL";
        trophyPlaceholder.transform.position = exitPos;
        trophyPlaceholder.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        
        Renderer r = trophyPlaceholder.GetComponent<Renderer>();
        r.material = CreateFlatColorMaterial("TrophyGold", Color.yellow);
        
        trophyPlaceholder.AddComponent<SpinAndBob>();

        Collider col = trophyPlaceholder.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void SpawnChickensAndWheels()
    {
        int rows = mazeLoader.mazeRows;
        int cols = mazeLoader.mazeColumns;
        float size = mazeLoader.size;

        List<Vector2Int> spawnableCells = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if ((r == 0 && c == 0) || (r == rows - 1 && c == cols - 1))
                {
                    continue;
                }
                spawnableCells.Add(new Vector2Int(r, c));
            }
        }

        for (int i = 0; i < spawnableCells.Count; i++)
        {
            Vector2Int temp = spawnableCells[i];
            int randIndex = Random.Range(i, spawnableCells.Count);
            spawnableCells[i] = spawnableCells[randIndex];
            spawnableCells[randIndex] = temp;
        }

        int cellIndex = 0;

        int spawnCount = Mathf.Min(numberOfChickens, spawnableCells.Count);
        maxPossiblePoints = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            if (cellIndex >= spawnableCells.Count) break;
            Vector2Int cell = spawnableCells[cellIndex++];
            Vector3 worldPos = new Vector3(cell.x * size, -1.1f, cell.y * size);

            float roll = Random.value;
            ChickenColor chickenColor;
            int points;
            Material mat;

            if (roll < 0.35f)
            {
                chickenColor = ChickenColor.DarkBrown;
                points = 1;
                mat = darkBrownMat;
            }
            else if (roll < 0.70f)
            {
                chickenColor = ChickenColor.LightBrown;
                points = 1;
                mat = lightBrownMat;
            }
            else if (roll < 0.90f)
            {
                chickenColor = ChickenColor.White;
                points = 1;
                mat = whiteMat;
            }
            else
            {
                chickenColor = ChickenColor.Blue;
                points = 3;
                mat = blueMat;
            }

            maxPossiblePoints += points;
            spawnedChickens[(int)chickenColor]++;

            SpawnChickenVisual(worldPos, chickenColor, points, mat, cell);
        }

        WheelType[] wheelTypes = (WheelType[])System.Enum.GetValues(typeof(WheelType));
        foreach (WheelType type in wheelTypes)
        {
            for (int w = 0; w < numberOfWheelsPerType; w++)
            {
                if (w >= numberOfWheelsPerType) break;
                if (cellIndex >= spawnableCells.Count) break;

                Vector2Int cell = spawnableCells[cellIndex++];
                Vector3 worldPos = new Vector3(cell.x * size, -1.48f, cell.y * size);

                Material wheelMat = GetWheelMaterial(type);
                SpawnWheelVisual(worldPos, type, wheelMat);
            }
        }
    }

    private void SpawnChickenVisual(Vector3 position, ChickenColor color, int points, Material mat, Vector2Int startCell)
    {
        GameObject chicken = new GameObject("Chicken_" + color);
        chicken.transform.position = position;

        Rigidbody rb = chicken.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(chicken.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
        body.GetComponent<Renderer>().material = mat;
        Destroy(body.GetComponent<Collider>()); 

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(chicken.transform);
        head.transform.localPosition = new Vector3(0f, 0.35f, 0.2f);
        head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        head.GetComponent<Renderer>().material = mat;
        Destroy(head.GetComponent<Collider>());

        GameObject beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beak.name = "Beak";
        beak.transform.SetParent(chicken.transform);
        beak.transform.localPosition = new Vector3(0f, 0.35f, 0.42f);
        beak.transform.localScale = new Vector3(0.12f, 0.08f, 0.15f);
        beak.GetComponent<Renderer>().material = CreateFlatColorMaterial("BeakYellow", Color.yellow);
        Destroy(beak.GetComponent<Collider>());

        GameObject comb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        comb.name = "Comb";
        comb.transform.SetParent(chicken.transform);
        comb.transform.localPosition = new Vector3(0f, 0.55f, 0.15f);
        comb.transform.localScale = new Vector3(0.08f, 0.15f, 0.18f);
        comb.GetComponent<Renderer>().material = CreateFlatColorMaterial("CombRed", Color.red);
        Destroy(comb.GetComponent<Collider>());

        BoxCollider trigger = chicken.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0, 0.1f, 0);
        trigger.size = new Vector3(1.8f, 1.2f, 1.8f);

        CollectableChicken cc = chicken.AddComponent<CollectableChicken>();
        cc.Initialize(color, points, startCell);
    }

    private void SpawnWheelVisual(Vector3 position, WheelType type, Material mat)
    {
        GameObject wheel = new GameObject("Wheel_" + type);
        wheel.transform.position = position;

        Rigidbody rb = wheel.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disk.name = "Disk";
        disk.transform.SetParent(wheel.transform);
        disk.transform.localPosition = Vector3.zero;
        disk.transform.localRotation = Quaternion.identity; 
        disk.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
        disk.GetComponent<Renderer>().material = mat;
        Destroy(disk.GetComponent<Collider>());

        wheel.AddComponent<SpinOnly>();

        BoxCollider trigger = wheel.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.2f, 1.2f, 1.2f);

        InteractiveWheel iw = wheel.AddComponent<InteractiveWheel>();
        iw.Initialize(type);
    }

    private Material GetWheelMaterial(WheelType type)
    {
        switch (type)
        {
            case WheelType.Teleport: return teleportWheelMat;
            case WheelType.Double: return doubleWheelMat;
            case WheelType.Loss: return lossWheelMat;
            case WheelType.Passage: return passageWheelMat;
            case WheelType.Hint: return hintWheelMat;
            case WheelType.Rewind: return rewindWheelMat;
            default: return whiteMat;
        }
    }

    public void OnChickenCollected(ChickenColor color, int pts)
    {
        int index = (int)color;
        if (index >= 0 && index < 4)
        {
            collectedChickens[index]++;
        }
        
        AddPoints(pts);
    }

    public void AddPoints(int pts)
    {
        int pointsToAdd = pts;
        if (hasDoublePoints)
        {
            pointsToAdd *= 2;
            hasDoublePoints = false;
        }

        score += pointsToAdd;
        Debug.Log("ChickenGameManager: Added " + pointsToAdd + " points! Current score: " + score);
        UpdateHUD();
    }

    public void OnCollectWheel(WheelType type)
    {
        Debug.Log("ChickenGameManager: Collected wheel: " + type);

        Color effectColor = Color.white;
        string effectText = "";

        switch (type)
        {
            case WheelType.Teleport:
                effectColor = new Color(0.63f, 0.13f, 0.94f);
                effectText = "LOSOWA TELEPORTACJA! 🌀";
                TeleportPlayerToRandomCell();
                break;

            case WheelType.Double:
                effectColor = Color.green;
                effectText = "PODWÓJNE PUNKTY! 🌟";
                hasDoublePoints = true;
                break;

            case WheelType.Loss:
                effectColor = Color.red;
                int loss = 2;
                if (playerPower == SpecialPower.Shield)
                {
                    loss = 1;
                    effectText = "-1 PUNKT! 🛡️ (Tarcza!)";
                    Debug.Log("ChickenGameManager: Shield active! Score loss halved to 1.");
                }
                else
                {
                    effectText = "-2 PUNKTY! 💔";
                }
                score -= loss;
                if (score < 0) score = 0;
                break;

            case WheelType.Passage:
                effectColor = new Color(1.0f, 0.64f, 0.0f);
                effectText = "+1 PRZEJŚCIE PRZEZ ŚCIANY! 🧱";
                wallPassCharges++;
                break;

            case WheelType.Hint:
                effectColor = Color.magenta;
                effectText = "KOMPAS AKTYWOWANY! 🧭";
                hintIntersectionsRemaining = 2;
                break;

            case WheelType.Rewind:
                effectColor = Color.cyan;
                effectText = "COFNIĘCIE SKRZYŻOWANIA! ⏪";
                TriggerRewind();
                break;
        }

        if (playerObj != null)
        {
            ChickenCollectionEffect.CreateWheelEffect(playerObj.transform.position, effectColor, effectText);
        }

        UpdateHUD();
    }

    private void TeleportPlayerToRandomCell()
    {
        if (playerObj == null || mazeLoader == null) return;

        int r = Random.Range(0, mazeLoader.mazeRows);
        int c = Random.Range(0, mazeLoader.mazeColumns);
        float size = mazeLoader.size;

        Vector3 targetPos = new Vector3(r * size, 0.5f, c * size);
        
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        playerObj.transform.position = targetPos;
        if (cc != null) cc.enabled = true;

        Debug.Log("ChickenGameManager: Player teleported to cell " + r + ", " + c);
    }

    private void TriggerTeleportPower()
    {
        if (playerObj == null) return;

        CollectableChicken[] chickens = FindObjectsByType<CollectableChicken>(FindObjectsSortMode.None);
        if (chickens == null || chickens.Length == 0)
        {
            Debug.Log("ChickenGameManager: No chickens left to teleport to!");
            return;
        }

        CollectableChicken nearest = null;
        float minDist = float.MaxValue;
        Vector3 playerPos = playerObj.transform.position;

        foreach (CollectableChicken cc in chickens)
        {
            float dist = Vector3.Distance(playerPos, cc.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = cc;
            }
        }

        if (nearest != null)
        {
            Vector3 targetPos = nearest.transform.position + Vector3.up * 0.5f;
            
            CharacterController charCtrl = playerObj.GetComponent<CharacterController>();
            if (charCtrl != null) charCtrl.enabled = false;
            playerObj.transform.position = targetPos;
            if (charCtrl != null) charCtrl.enabled = true;

            Debug.Log("ChickenGameManager: Special Power used! Teleported to nearest chicken at " + targetPos);
        }
    }

    private void OnPlayerEnterCell(Vector2Int cell)
    {
        if (IsIntersection(cell))
        {
            if (lastRecordedIntersection != cell)
            {
                intersectionHistory.Add(cell);
                lastRecordedIntersection = cell;
                Debug.Log("ChickenGameManager: Recorded intersection at cell " + cell);
            }
        }
    }

    private bool IsIntersection(Vector2Int cell)
    {
        if (mazeLoader == null) return false;
        
        int r = cell.x;
        int c = cell.y;
        if (r < 0 || r >= mazeLoader.mazeRows || c < 0 || c >= mazeLoader.mazeColumns) return false;

        int openDirections = 0;

        if (r > 0)
        {
            if (mazeLoader.mazeCells[r - 1, c].southWall == null) openDirections++;
        }
        else if (mazeLoader.mazeCells[r, c].northWall == null)
        {
            openDirections++;
        }

        if (r < mazeLoader.mazeRows - 1)
        {
            if (mazeLoader.mazeCells[r, c].southWall == null) openDirections++;
        }

        if (c < mazeLoader.mazeColumns - 1)
        {
            if (mazeLoader.mazeCells[r, c].eastWall == null) openDirections++;
        }

        if (c > 0)
        {
            if (mazeLoader.mazeCells[r, c - 1].eastWall == null) openDirections++;
        }
        else if (mazeLoader.mazeCells[r, c].westWall == null)
        {
            openDirections++;
        }

        return openDirections >= 3;
    }

    private void TriggerRewind()
    {
        if (playerObj == null) return;

        Vector2Int rewindCell = new Vector2Int(0, 0);

        if (intersectionHistory.Count > 1)
        {
            int lastIndex = intersectionHistory.Count - 1;
            Vector2Int currentInCell = GetCellFromPosition(playerObj.transform.position);

            if (intersectionHistory[lastIndex] == currentInCell)
            {
                intersectionHistory.RemoveAt(lastIndex); 
            }

            if (intersectionHistory.Count > 0)
            {
                int prevIndex = intersectionHistory.Count - 1;
                rewindCell = intersectionHistory[prevIndex];
                intersectionHistory.RemoveAt(prevIndex);
            }
        }
        else if (intersectionHistory.Count == 1)
        {
            rewindCell = intersectionHistory[0];
            intersectionHistory.Clear();
        }

        float size = mazeLoader.size;
        Vector3 targetPos = new Vector3(rewindCell.x * size, 0.5f, rewindCell.y * size);

        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        playerObj.transform.position = targetPos;
        if (cc != null) cc.enabled = true;

        lastRecordedIntersection = rewindCell;
        Debug.Log("ChickenGameManager: Rewind Wheel triggered! Teleported back to intersection " + rewindCell);
    }

    public bool HasDoublePointsBuff()
    {
        return hasDoublePoints;
    }

    public bool HasWallPassCharge()
    {
        return wallPassCharges > 0;
    }

    public void ConsumeWallPassCharge()
    {
        if (wallPassCharges > 0)
        {
            wallPassCharges--;
            UpdateHUD();
            Debug.Log("ChickenGameManager: Wall passage charge consumed! Remaining: " + wallPassCharges);
        }
    }

    private void UpdateHUD()
    {
        if (scoreText != null) scoreText.text = "Punkty: " + score;
        if (powerText != null)
        {
            string powerName = "Brak";
            if (playerPower == SpecialPower.Shield) powerName = "Tarcza 🛡️ (1/2 straty)";
            else if (playerPower == SpecialPower.Speed) powerName = "Przyspieszenie ⚡ (2x prędkość)";
            else if (playerPower == SpecialPower.Teleportation) powerName = "Teleportacja 🌀 [Space / T]";
            powerText.text = "Moc specjalna: " + powerName;
        }

        if (activeBuffsText != null)
        {
            string buffs = "";
            if (hasDoublePoints) buffs += "🌟 x2 Punkty (Następny Kurczak)\n";
            if (wallPassCharges > 0) buffs += "🧱 Przejście przez ściany: x" + wallPassCharges + "\n";
            if (hintIntersectionsRemaining > 0) buffs += "🧭 Podpowiedzi na skrzyżowaniach: x" + hintIntersectionsRemaining + "\n";
            
            if (buffs == "") buffs = "Aktywne efekty: Brak";
            else buffs = "Aktywne efekty:\n" + buffs;

            activeBuffsText.text = buffs;
        }

        for (int i = 0; i < 4; i++)
        {
            if (trackerCountTexts[i] != null)
            {
                if (spawnedChickens[i] > 0 && collectedChickens[i] == spawnedChickens[i])
                {
                    trackerCountTexts[i].text = "<color=#32cd32><b>" + collectedChickens[i] + "</b> / " + spawnedChickens[i] + " ✓</color>";
                }
                else
                {
                    trackerCountTexts[i].text = "<b>" + collectedChickens[i] + "</b> / " + spawnedChickens[i];
                }
            }
        }
    }

    private void UpdateHintVisuals()
    {
        if (mazeLoader == null || playerObj == null) return;

        if (hintIntersectionsRemaining <= 0)
        {
            if (hintArrowInstance != null)
            {
                Destroy(hintArrowInstance);
                hintArrowInstance = null;
            }
            return;
        }

        Vector2Int playerCell = GetCellFromPosition(playerObj.transform.position);
        Vector2Int exitCell = new Vector2Int(mazeLoader.mazeRows - 1, mazeLoader.mazeColumns - 1);

        List<Vector2Int> path = FindPath(playerCell, exitCell);
        if (path == null || path.Count < 2)
        {
            if (hintArrowInstance != null) Destroy(hintArrowInstance);
            return;
        }

        Vector2Int targetIntersectionCell = new Vector2Int(-1, -1);
        int nextCellPathIndex = -1;

        for (int i = 0; i < path.Count; i++)
        {
            if (IsIntersection(path[i]))
            {
                targetIntersectionCell = path[i];
                nextCellPathIndex = i + 1;
                break;
            }
        }

        if (playerCell == targetIntersectionCell)
        {
        }

        Vector2Int currentCell = playerCell;
        Vector2Int nextCell = path[1];

        float size = mazeLoader.size;
        Vector3 arrowPos = new Vector3(currentCell.x * size, -0.2f, currentCell.y * size);
        Vector3 targetPos = new Vector3(nextCell.x * size, -0.2f, nextCell.y * size);
        Vector3 dir = (targetPos - arrowPos).normalized;

        if (hintArrowInstance == null)
        {
            hintArrowInstance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hintArrowInstance.name = "HINT_ARROW";
            hintArrowInstance.transform.localScale = new Vector3(0.15f, 0.4f, 0.4f);
            hintArrowInstance.GetComponent<Renderer>().material = CreateFlatColorMaterial("HintGreen", Color.green);
            Destroy(hintArrowInstance.GetComponent<Collider>());
        }

        hintArrowInstance.transform.position = playerObj.transform.position + Vector3.up * -0.5f + dir * 1.5f;
        hintArrowInstance.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);

        if (IsIntersection(playerCell) && playerCell != lastRecordedIntersection)
        {
            hintIntersectionsRemaining--;
            UpdateHUD();
            Debug.Log("ChickenGameManager: Passed intersection! Remaining hint charges: " + hintIntersectionsRemaining);
        }
    }

    private void EndGameSuccess()
    {
        if (gameOverPanel == null) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovement != null) playerMovement.enabled = false;

        gameOverPanel.SetActive(true);

        string trophy = "Brązowe 🥉";
        Color trophyColor = new Color(0.8f, 0.5f, 0.2f); 

        float pct = maxPossiblePoints > 0 ? (float)score / maxPossiblePoints : 0f;

        if (pct >= 0.8f)
        {
            trophy = "Złote 🥇";
            trophyColor = Color.yellow;
        }
        else if (pct >= 0.5f)
        {
            trophy = "Srebrne 🥈";
            trophyColor = Color.gray;
        }

        gameOverTitleText.text = "LABIRYNT UKOŃCZONY!";
        gameOverTitleText.color = trophyColor;

        gameOverStatsText.text = "Twój wynik: " + score + " / " + maxPossiblePoints + " punktów (" + Mathf.RoundToInt(pct * 100f) + "%)\nOtrzymujesz Trofeum: " + trophy;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        if (mazeLoader == null) return null;
        if (start == end) return new List<Vector2Int> { start };

        Queue<List<Vector2Int>> queue = new Queue<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(new List<Vector2Int> { start });
        visited.Add(start);

        while (queue.Count > 0)
        {
            List<Vector2Int> path = queue.Dequeue();
            Vector2Int current = path[path.Count - 1];

            if (current == end)
            {
                return path;
            }

            List<Vector2Int> neighbors = GetOpenNeighbors(current);
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    List<Vector2Int> newPath = new List<Vector2Int>(path);
                    newPath.Add(neighbor);
                    queue.Enqueue(newPath);
                }
            }
        }

        return null; 
    }

    private List<Vector2Int> GetOpenNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (mazeLoader == null) return neighbors;

        int r = cell.x;
        int c = cell.y;
        int maxRows = mazeLoader.mazeRows;
        int maxCols = mazeLoader.mazeColumns;

        if (r > 0)
        {
            if (mazeLoader.mazeCells[r - 1, c].southWall == null)
            {
                neighbors.Add(new Vector2Int(r - 1, c));
            }
        }

        if (r < maxRows - 1)
        {
            if (mazeLoader.mazeCells[r, c].southWall == null)
            {
                neighbors.Add(new Vector2Int(r + 1, c));
            }
        }

        if (c < maxCols - 1)
        {
            if (mazeLoader.mazeCells[r, c].eastWall == null)
            {
                neighbors.Add(new Vector2Int(r, c + 1));
            }
        }

        if (c > 0)
        {
            if (mazeLoader.mazeCells[r, c - 1].eastWall == null)
            {
                neighbors.Add(new Vector2Int(r, c - 1));
            }
        }

        return neighbors;
    }

    public Vector2Int GetCellFromPosition(Vector3 pos)
    {
        if (mazeLoader == null) return Vector2Int.zero;
        float size = mazeLoader.size;
        int r = Mathf.RoundToInt(pos.x / size);
        int c = Mathf.RoundToInt(pos.z / size);
        r = Mathf.Clamp(r, 0, mazeLoader.mazeRows - 1);
        c = Mathf.Clamp(c, 0, mazeLoader.mazeColumns - 1);
        return new Vector2Int(r, c);
    }
}

public class SpinAndBob : MonoBehaviour
{
    private float spinSpeed = 100f;
    private float bobSpeed = 3f;
    private float bobAmount = 0.15f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}

public class SpinOnly : MonoBehaviour
{
    private float spinSpeed = 100f;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}
