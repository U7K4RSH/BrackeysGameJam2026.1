using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MiniGridGame : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int size = 3;
    [SerializeField] private int cellSize = 100;
    [SerializeField] private int spacing = 8;

    [Header("Initial state")]
    [Tooltip("If true, grid will be randomized on Start; otherwise all OFF.")]
    [SerializeField] private bool randomizeStart = true;

    [Header("UI parent (optional)")]
    [Tooltip("If empty, a Canvas will be created at runtime.")]
    [SerializeField] private Canvas parentCanvas;
    private bool createdCanvas = false;

    private bool[,] grid;
    private Button[,] buttons;
    private GameObject panel;

    private void Start()
    {
        if (size < 1) size = 3;
        EnsureEventSystem();
        EnsureCanvas();
        CreateGridUI();
        InitializeGrid();
        RefreshButtons();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

    private void EnsureCanvas()
    {
        if (parentCanvas != null) return;
        var existing = Canvas.FindAnyObjectByType<Canvas>();
        if (existing != null)
        {
            parentCanvas = existing;
            return;
        }

        var go = new GameObject("MiniGameCanvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        parentCanvas = canvas;
        createdCanvas = true;
        DontDestroyOnLoad(go);
    }

    // Close and clean up the runtime-created UI and gameobject.
    public void Close()
    {
        if (panel != null)
        {
            Destroy(panel);
            panel = null;
        }

        if (createdCanvas && parentCanvas != null)
        {
            Destroy(parentCanvas.gameObject);
            parentCanvas = null;
            createdCanvas = false;
        }

        Destroy(this.gameObject);
    }

    private void CreateGridUI()
    {
        // panel
        panel = new GameObject("MiniGridPanel");
        panel.transform.SetParent(parentCanvas.transform, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size * cellSize + (size - 1) * spacing + 20, size * cellSize + (size - 1) * spacing + 20);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        var img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.45f);

        var gridLayout = panel.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(spacing, spacing);
        gridLayout.padding = new RectOffset(10, 10, 10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = size;

        buttons = new Button[size, size];
        grid = new bool[size, size];

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                var btnGO = new GameObject($"Cell_{r}_{c}");
                btnGO.transform.SetParent(panel.transform, false);
                var imgComp = btnGO.AddComponent<Image>();
                imgComp.color = Color.gray;
                var btn = btnGO.AddComponent<Button>();

                // label (TextMeshPro)
                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(btnGO.transform, false);
                var labelRT = labelGO.AddComponent<RectTransform>();
                labelRT.anchorMin = Vector2.zero;
                labelRT.anchorMax = Vector2.one;
                labelRT.offsetMin = Vector2.zero;
                labelRT.offsetMax = Vector2.zero;
                var tmp = labelGO.AddComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 24;
                tmp.text = "";

                int rr = r, cc = c;
                btn.onClick.AddListener(() => OnCellClicked(rr, cc));

                buttons[r, c] = btn;
            }
        }
    }

    private void InitializeGrid()
    {
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                grid[r, c] = false;

        if (randomizeStart)
        {
            // create a solvable-ish random start by applying random toggles
            var rnd = new System.Random();
            int moves = rnd.Next(3, 7);
            for (int m = 0; m < moves; m++)
            {
                int r = rnd.Next(0, size);
                int c = rnd.Next(0, size);
                ApplyToggleEffect(r, c);
            }
        }
    }

    private void OnCellClicked(int row, int col)
    {
        // toggle clicked cell once
        ToggleCell(row, col);

        // toggle others in same row
        for (int c = 0; c < size; c++) if (c != col) ToggleCell(row, c);

        // toggle others in same column
        for (int r = 0; r < size; r++) if (r != row) ToggleCell(r, col);

        RefreshButtons();
        if (CheckWin()) OnWin();
    }

    private void ApplyToggleEffect(int row, int col)
    {
        // used for random initialization (same rule as player)
        ToggleCell(row, col);
        for (int c = 0; c < size; c++) if (c != col) ToggleCell(row, c);
        for (int r = 0; r < size; r++) if (r != row) ToggleCell(r, col);
    }

    private void ToggleCell(int r, int c)
    {
        grid[r, c] = !grid[r, c];
    }

    private void RefreshButtons()
    {
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                var btn = buttons[r, c];
                if (btn == null) continue;
                var img = btn.GetComponent<Image>();
                img.color = grid[r, c] ? Color.green : Color.gray;
                var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = grid[r, c] ? "ON" : "";
            }
        }
    }

    private bool CheckWin()
    {
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                if (!grid[r, c]) return false;
        return true;
    }

    private void OnWin()
    {
        Debug.Log("Mini-game complete! All buttons are ON.");
        // Notify RoomManager to re-enable lights in the exit room
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.SetExitRoomLightsEnabled(true);
            Debug.Log("Notified RoomManager to re-enable exit room lights.");
        }
    }

    // Public control methods
    public void ResetRandom()
    {
        InitializeGrid();
        RefreshButtons();
    }

    public void RevealAll()
    {
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                grid[r, c] = true;
        RefreshButtons();
        OnWin();
    }
}
