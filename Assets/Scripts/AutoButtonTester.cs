using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Automated testing tool that randomly presses Collider2DButton components.
/// </summary>
public class AutoButtonTester : MonoBehaviour
{
    [Header("Pressing Settings")]
    [Tooltip("How often a button press occurs (presses per second).")]
    public float pressingSpeed = 1f;
    [Tooltip("How long each press lasts in seconds.")]
    public float pressDuration = 0.1f;
    [Tooltip("Delay between consecutive presses in seconds.")]
    public float delayBetweenPresses = 0.5f;
    [Tooltip("Randomness factor applied to press intervals and durations.")]
    [Range(0f, 1f)]
    public float randomness = 0.1f;

    [Header("UI References")]
    public Canvas testerCanvas; // Canvas used for configuration UI
    public Button startButton;
    public Button stopButton;
    public Text statsText;

    private readonly Dictionary<Collider2DButton, int> pressCounts = new Dictionary<Collider2DButton, int>();
    private readonly List<Collider2DButton> cachedButtons = new List<Collider2DButton>();

    private Camera mainCamera;
    private bool isTesting;
    private float nextStatUpdate;
    private float enterPressTime;
    private int enterPressCount;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (testerCanvas == null)
        {
            CreateRuntimeCanvas();
        }
        stopButton.gameObject.SetActive(false);
        testerCanvas.gameObject.SetActive(false);
        EnsureEventSystemExists();
    }

    private void Update()
    {
        CheckActivationInput();

        if (isTesting) return;

        // dynamic button discovery while idle
        UpdateCachedButtons();
    }

    private void CheckActivationInput()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            float now = Time.unscaledTime;
            if (now - enterPressTime < 0.75f)
            {
                enterPressCount++;
            }
            else
            {
                enterPressCount = 1;
            }
            enterPressTime = now;
            if (enterPressCount >= 3)
            {
                ToggleTesterCanvas();
                enterPressCount = 0;
            }
        }
    }

    private void ToggleTesterCanvas()
    {
        testerCanvas.gameObject.SetActive(!testerCanvas.gameObject.activeSelf);
    }

    private void EnsureEventSystemExists()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    private void CreateRuntimeCanvas()
    {
        testerCanvas = new GameObject("AutoTesterCanvas").AddComponent<Canvas>();
        testerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        testerCanvas.gameObject.AddComponent<CanvasScaler>();
        testerCanvas.gameObject.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(testerCanvas.transform, false);
        Image panel = panelObj.AddComponent<Image>();
        panel.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(300, 180);
        prt.anchoredPosition = Vector2.zero;

        startButton = CreateButton(panelObj.transform, "Start", new Vector2(0, -50));
        stopButton = CreateButton(panelObj.transform, "Stop", new Vector2(0, -90));
        stopButton.gameObject.SetActive(false);

        statsText = CreateText(panelObj.transform, "Stats", new Vector2(0, 50));
    }

    private Button CreateButton(Transform parent, string label, Vector2 pos)
    {
        GameObject btnObj = new GameObject(label + "Button");
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();
        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(140, 30);
        rt.anchoredPosition = pos;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.color = Color.black;
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        return btn;
    }

    private Text CreateText(Transform parent, string initial, Vector2 pos)
    {
        GameObject txtObj = new GameObject("StatsText");
        txtObj.transform.SetParent(parent, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.alignment = TextAnchor.UpperLeft;
        txt.text = initial;
        txt.color = Color.white;
        RectTransform rt = txt.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 60);
        rt.anchoredPosition = pos;
        return txt;
    }

    private void Start()
    {
        startButton.onClick.AddListener(StartTesting);
        stopButton.onClick.AddListener(StopTesting);
    }

    private void StartTesting()
    {
        isTesting = true;
        startButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(true);
        UpdateCachedButtons();
        StartCoroutine(PressLoop());
    }

    private void StopTesting()
    {
        isTesting = false;
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
    }

    private IEnumerator PressLoop()
    {
        while (isTesting)
        {
            if (cachedButtons.Count == 0)
            {
                UpdateCachedButtons();
            }

            Collider2DButton button = GetRandomPressableButton();
            if (button != null)
            {
                yield return PressButton(button);
            }

            float delay = delayBetweenPresses * Random.Range(1f - randomness, 1f + randomness);
            yield return new WaitForSeconds(delay);
        }
    }

    private void UpdateCachedButtons()
    {
        cachedButtons.Clear();
        Collider2DButton[] allButtons = FindObjectsOfType<Collider2DButton>(true);
        foreach (var b in allButtons)
        {
            if (b == null) continue;
            if (!IsButtonOnTesterUI(b) && IsButtonPressable(b))
            {
                cachedButtons.Add(b);
                if (!pressCounts.ContainsKey(b))
                    pressCounts.Add(b, 0);
            }
        }
    }

    private bool IsButtonOnTesterUI(Collider2DButton button)
    {
        return button.GetComponentInParent<Canvas>() == testerCanvas;
    }

    private bool IsButtonPressable(Collider2DButton button)
    {
        if (button == null) return false;
        if (!button.gameObject.activeInHierarchy) return false;
        Collider2D collider = button.GetComponent<Collider2D>();
        if (collider == null || !collider.enabled) return false;

        Vector3 worldPos = collider.bounds.center;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0) return false; // behind camera

        // check visibility by raycast to collider
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider != collider) return false;
        return true;
    }

    private Collider2DButton GetRandomPressableButton()
    {
        for (int attempts = 0; attempts < cachedButtons.Count; attempts++)
        {
            var b = cachedButtons[Random.Range(0, cachedButtons.Count)];
            if (b == null)
            {
                cachedButtons.Remove(b);
                continue;
            }
            if (IsButtonPressable(b))
            {
                return b;
            }
        }
        return null;
    }

    private IEnumerator PressButton(Collider2DButton button)
    {
        if (button == null) yield break;

        pressCounts[button]++;
        float pressTime = pressDuration * Random.Range(1f - randomness, 1f + randomness);

        var eventData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(button.gameObject, eventData, ExecuteEvents.pointerDownHandler);
        yield return new WaitForSeconds(pressTime);
        ExecuteEvents.Execute(button.gameObject, eventData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, eventData, ExecuteEvents.pointerClickHandler);

        if (Time.time >= nextStatUpdate)
        {
            nextStatUpdate = Time.time + 5f;
            UpdateStatsText();
        }
    }

    private void UpdateStatsText()
    {
        statsText.text = "Button Presses:\n";
        foreach (var kvp in pressCounts)
        {
            statsText.text += kvp.Key.name + ": " + kvp.Value + "\n";
        }
    }
}


