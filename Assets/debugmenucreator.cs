using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugMenuCreator : MonoBehaviour
{
    [Header("Auto-Create Debug Menu")]
    [SerializeField] private bool createOnStart = false;
    
    // References that will be created
    private GameObject canvas;
    private GameObject debugPanel;
    private Button teleportStartBtn;
    private Button teleportEndBtn;
    private Button teleportCheckpointBtn;
    private Button resetGameBtn;
    private Dropdown checkpointDropdown;
    private Button closeBtn;
    
    void Start()
    {
        if (createOnStart)
        {
            CreateDebugMenuUI();
        }
    }
    
    [ContextMenu("Create Debug Menu UI")]
    public void CreateDebugMenuUI()
    {
        // Find or create Canvas
        canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            CreateCanvas();
        }
        
        // Create the debug menu structure
        CreateDebugPanel();
        CreateTitle();
        CreateButtons();
        CreateCheckpointSection();
        CreateCloseButton();
        
        // Setup DebugMenu component
        SetupDebugMenuComponent();
        
        Debug.Log("Debug Menu UI created successfully!");
    }
    
    private void CreateCanvas()
    {
        // Create Canvas
        canvas = new GameObject("Canvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem if needed
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
    
    private void CreateDebugPanel()
    {
        // Create main panel
        debugPanel = new GameObject("DebugMenuPanel");
        debugPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = debugPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 500);
        rect.anchoredPosition = Vector2.zero;
        
        // Add background
        Image bg = debugPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Add layout
        VerticalLayoutGroup layout = debugPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 10;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }
    
    private void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(debugPanel.transform, false);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "DEBUG MENU";
        titleText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        LayoutElement le = titleObj.AddComponent<LayoutElement>();
        le.preferredHeight = 40;
    }
    
    private void CreateButtons()
    {
        // Teleport to Start Button
        teleportStartBtn = CreateButton("Teleport to Start", debugPanel.transform);
        
        // Teleport to End Button
        teleportEndBtn = CreateButton("Teleport to End", debugPanel.transform);
        
        // Reset Game Button
        resetGameBtn = CreateButton("Reset Game", debugPanel.transform);
    }
    
    private void CreateCheckpointSection()
    {
        // Create checkpoint container
        GameObject checkpointContainer = new GameObject("CheckpointContainer");
        checkpointContainer.transform.SetParent(debugPanel.transform, false);
        
        HorizontalLayoutGroup hLayout = checkpointContainer.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = false;
        
        LayoutElement containerLE = checkpointContainer.AddComponent<LayoutElement>();
        containerLE.preferredHeight = 40;
        
        // Create dropdown
        GameObject dropdownObj = new GameObject("CheckpointDropdown");
        dropdownObj.transform.SetParent(checkpointContainer.transform, false);
        
        Image dropdownBg = dropdownObj.AddComponent<Image>();
        dropdownBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        checkpointDropdown = dropdownObj.AddComponent<Dropdown>();
        
        // Setup dropdown template
        CreateDropdownTemplate(dropdownObj);
        
        // Setup dropdown text
        GameObject label = new GameObject("Label");
        label.transform.SetParent(dropdownObj.transform, false);
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.offsetMin = new Vector2(10, 0);
        labelRect.offsetMax = new Vector2(-25, 0);
        
        Text labelText = label.AddComponent<Text>();
        labelText.text = "Select Checkpoint";
        labelText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;
        
        checkpointDropdown.captionText = labelText;
        
        // Create arrow
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrow.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchoredPosition = new Vector2(-15, 0);
        
        Text arrowText = arrow.AddComponent<Text>();
        arrowText.text = "▼";
        arrowText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        arrowText.color = Color.white;
        arrowText.alignment = TextAnchor.MiddleCenter;
        
        // Teleport to Checkpoint Button
        teleportCheckpointBtn = CreateButton("Go", checkpointContainer.transform);
        LayoutElement btnLE = teleportCheckpointBtn.GetComponent<LayoutElement>();
        btnLE.preferredWidth = 80;
    }
    
    private void CreateDropdownTemplate(GameObject dropdownObj)
    {
        // Create template
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);
        template.SetActive(false);
        
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);
        
        Image templateBg = template.AddComponent<Image>();
        templateBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        ScrollRect scrollRect = template.AddComponent<ScrollRect>();
        
        // Create viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.pivot = new Vector2(0, 1);
        
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        
        // Create content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 28);
        
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.childForceExpandWidth = true;
        
        ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create item
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        
        LayoutElement itemLE = item.AddComponent<LayoutElement>();
        itemLE.minHeight = 20;
        
        Toggle itemToggle = item.AddComponent<Toggle>();
        
        // Item background
        GameObject itemBg = new GameObject("Item Background");
        itemBg.transform.SetParent(item.transform, false);
        
        RectTransform itemBgRect = itemBg.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.sizeDelta = Vector2.zero;
        
        Image itemBgImage = itemBg.AddComponent<Image>();
        itemBgImage.color = new Color(0.2f, 0.2f, 0.2f, 0);
        
        // Item checkmark
        GameObject itemCheckmark = new GameObject("Item Checkmark");
        itemCheckmark.transform.SetParent(item.transform, false);
        
        RectTransform checkRect = itemCheckmark.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0, 0.5f);
        checkRect.anchorMax = new Vector2(0, 0.5f);
        checkRect.sizeDelta = new Vector2(20, 20);
        checkRect.anchoredPosition = new Vector2(10, 0);
        
        Image checkImage = itemCheckmark.AddComponent<Image>();
        checkImage.color = Color.white;
        
        // Item label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        
        RectTransform labelRect = itemLabel.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(30, 0);
        labelRect.offsetMax = new Vector2(-10, 0);
        
        Text itemText = itemLabel.AddComponent<Text>();
        itemText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        itemText.color = Color.white;
        itemText.alignment = TextAnchor.MiddleLeft;
        
        // Setup toggle
        itemToggle.targetGraphic = itemBgImage;
        itemToggle.graphic = checkImage;
        itemToggle.isOn = false;
        
        // Setup scroll rect
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        
        // Setup dropdown
        checkpointDropdown.template = templateRect;
        checkpointDropdown.itemText = itemText;
    }
    
    private void CreateCloseButton()
    {
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(debugPanel.transform, false);
        
        RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(30, 30);
        closeRect.anchoredPosition = new Vector2(-10, -10);
        
        Image closeBg = closeBtnObj.AddComponent<Image>();
        closeBg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        closeBtn = closeBtnObj.AddComponent<Button>();
        
        GameObject closeText = new GameObject("Text");
        closeText.transform.SetParent(closeBtnObj.transform, false);
        
        RectTransform textRect = closeText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Text txt = closeText.AddComponent<Text>();
        txt.text = "X";
        txt.font = Font.CreateDynamicFontFromOSFont("Arial", 18);
        txt.fontSize = 18;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
    }
    
    private Button CreateButton(string text, Transform parent)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        button.colors = colors;
        
        LayoutElement le = buttonObj.AddComponent<LayoutElement>();
        le.preferredHeight = 40;
        
        GameObject buttonText = new GameObject("Text");
        buttonText.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = buttonText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Text txt = buttonText.AddComponent<Text>();
        txt.text = text;
        txt.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        txt.fontSize = 16;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        
        return button;
    }
    
    private void SetupDebugMenuComponent()
    {
        // Add DebugMenu component if not present
        DebugMenu debugMenu = GetComponent<DebugMenu>();
        if (debugMenu == null)
        {
            debugMenu = gameObject.AddComponent<DebugMenu>();
        }
        
        // Auto-assign UI references using reflection (Unity Editor only)
        #if UNITY_EDITOR
        SerializedObject so = new SerializedObject(debugMenu);
        
        so.FindProperty("debugMenuPanel").objectReferenceValue = debugPanel;
        so.FindProperty("teleportStartButton").objectReferenceValue = teleportStartBtn;
        so.FindProperty("teleportEndButton").objectReferenceValue = teleportEndBtn;
        so.FindProperty("teleportCheckpointButton").objectReferenceValue = teleportCheckpointBtn;
        so.FindProperty("resetGameButton").objectReferenceValue = resetGameBtn;
        so.FindProperty("checkpointDropdown").objectReferenceValue = checkpointDropdown;
        
        so.ApplyModifiedProperties();
        #endif
        
        // Setup close button
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() => {
                debugPanel.SetActive(false);
                Time.timeScale = 1f;
            });
        }
        
        // Hide panel initially
        debugPanel.SetActive(false);
    }
}

// Editor script for menu item
#if UNITY_EDITOR
public class DebugMenuCreatorEditor : Editor
{
    [MenuItem("GameObject/UI/Debug Menu", false, 10)]
    static void CreateDebugMenu(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("DebugMenuManager");
        DebugMenuCreator creator = go.AddComponent<DebugMenuCreator>();
        creator.CreateDebugMenuUI();
        
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create Debug Menu");
        Selection.activeObject = go;
    }
}
#endif