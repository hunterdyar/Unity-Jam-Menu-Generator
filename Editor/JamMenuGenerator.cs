using System;
using System.Collections.Generic;
using Blooper.MenuGenerator.Editor;
using Blooper.MenuGenerator.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;


public class JamMenuGenerator : EditorWindow
{
	[SerializeField] public Canvas rootCanvas;
	[SerializeField] private bool replaceLastGenerated = true;

	[SerializeField] private MenuOption[] menuOptions;

	[SerializeField] private MenuColorTheme theme;


	private SerializedObject thisAsSerialized;
	private SerializedProperty rootCanvasProp;
	private SerializedProperty menuOptionsProp;
	private SerializedProperty themeProp;

	private bool showGenOptions;
	[SerializeField] private bool CreateMenuTitleText = true;
	[SerializeField] private bool CreateSubPanelTitleText = true;

	//private generation cache variables
	private MenuManager _manager;
	private MenuPanel _defaultPanel;
	private RectTransform _otherPanelsRoot;

	[MenuItem("Window/Jam Menu Generator")]
	public static void ShowWindow()
	{
		JamMenuGenerator wnd = GetWindow<JamMenuGenerator>();
		wnd.titleContent = new GUIContent("Jam Menu Generator");
	}

	private void OnEnable()
	{
		thisAsSerialized = new SerializedObject(this);
		rootCanvasProp = thisAsSerialized.FindProperty(nameof(rootCanvas));
		menuOptionsProp = thisAsSerialized.FindProperty(nameof(menuOptions));
		themeProp = thisAsSerialized.FindProperty(nameof(theme));
		ResetTheme();
	}

	private void OnGUI()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(new GUIContent("Root Canvas: "));
		rootCanvas = (Canvas)EditorGUILayout.ObjectField(rootCanvas, typeof(Canvas), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(menuOptionsProp, new GUIContent("Menu: "), true);
		EditorGUI.indentLevel++;
		showGenOptions = EditorGUILayout.BeginFoldoutHeaderGroup(showGenOptions, "Generation Options");
		if (showGenOptions)
		{
			replaceLastGenerated = EditorGUILayout.Toggle("Replace Previous", replaceLastGenerated);
			EditorGUILayout.PropertyField(themeProp, new GUIContent("Style Options"), true);
			EditorGUILayout.Space();
			CreateMenuTitleText = EditorGUILayout.Toggle("Include Menu Title Text", CreateMenuTitleText);
			CreateSubPanelTitleText = EditorGUILayout.Toggle("Include Subpanel Title Text", CreateSubPanelTitleText);
		}

		EditorGUI.indentLevel--;
		EditorGUILayout.EndFoldoutHeaderGroup();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Set Default Menu"))
		{
			SetDefaultMenu();
		}

		if (GUILayout.Button("Reset Style"))
		{
			ResetTheme();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();

		if (GUILayout.Button("Generate", GUILayout.Height(50)))
		{
			Generate();
		}
	}

	public void SetDefaultMenu()
	{
		menuOptions = new[]
		{
			new MenuOption("Start Game", MenuOptionType.LoadSceneButton),
			new MenuOption("Settings", MenuOptionType.SubPanel),
			new MenuOption("Exit", MenuOptionType.QuitButton)
		};


		thisAsSerialized.Update();
	}

	public void ResetTheme()
	{
		theme = new MenuColorTheme();
		theme.backgroundColor = Color.white;
		theme.panelBackgroundColor = Color.gray;
		theme.textColor = Color.black;
		theme.buttonImage = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
		thisAsSerialized.Update();
	}

	public void Generate()
	{
		var allPanels = new List<MenuPanel>();
		//0. Validate
		if (rootCanvas == null)
		{
			rootCanvas = GameObject.FindObjectOfType<Canvas>();
			if (rootCanvas == null)
			{
				EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
				//Deselect to clear the rename Canvas?
			}

			//okay NOW try.
			rootCanvas = GameObject.FindObjectOfType<Canvas>();

			//try to find canvas.
			//Show warning and/or create canvas
			if (rootCanvas == null)
			{
				//still!?
				Debug.LogWarning("Failed to create Canvas. Create a canvas and set it in the window.");
				return;
			}
		}

		//1. Clear.
		if (replaceLastGenerated && _manager != null)
		{
			DestroyImmediate(_manager.gameObject);
		}

		//2. Create root menuManager.
		CreateMenuManager();

		//Create background image as its own object. Do it first so its the first child, and behind the others.
		var image = CreateBackgroundImageChild(_manager.transform, theme.backgroundColor);

		//3. Create defaultPanel
		//todo: move children to CreateSubMenuPanel
		_defaultPanel = CreateMainMenuPanel("Main Menu", _manager.transform as RectTransform);
		_manager.SetDefaultPanel(_defaultPanel);

		//5. loop through MenuOptions and generate buttons and panels (if needed)
		_otherPanelsRoot = new GameObject().AddComponent<RectTransform>();
		_otherPanelsRoot.name = "Sub Panels";
		_otherPanelsRoot.SetParent(_manager.transform);
		SetTransformToFullStretch(_otherPanelsRoot);

		//for each menu option make its buttons (and panel objects if needed)
		foreach (var option in menuOptions)
		{
			switch (option.optionType)
			{
				case MenuOptionType.LoadSceneButton:
				case MenuOptionType.QuitButton:
				case MenuOptionType.ClosePanel:
					//create button. Pass in the menuOption and it will configure itself.
					CreateSceneButton(_defaultPanel.transform, _defaultPanel, option);
					break;

				case MenuOptionType.SubPanel:
					//create sub panel, and its button
					var panel = CreateSubPanel(option, _otherPanelsRoot, out var contentParent);
					CreateSceneButton(_defaultPanel.transform, panel, option); //create button to open panel
					allPanels.Add(panel);
					break;
				case MenuOptionType.Text:
					//create child text object
					var textObject = new GameObject().AddComponent<RectTransform>();
					textObject.SetParent(_defaultPanel.transform);
					var text = AddTextMeshComponent(textObject.gameObject);
					text.text = option.optionName;
					textObject.gameObject.name = option.optionName + " Text";
					textObject.gameObject.AddComponent<LayoutElement>();

					break;
				case MenuOptionType.Image:
					var imageObject = new GameObject().AddComponent<RectTransform>();
					imageObject.SetParent(_defaultPanel.transform);
					var imageItem = imageObject.gameObject.AddComponent<Image>();
					imageItem.sprite = option.image;
					imageObject.gameObject.name = "Image";
					var layoutElement = imageObject.gameObject.AddComponent<LayoutElement>();
					//these settings are something of a hack to get good-enough default sizes.
					layoutElement.preferredWidth = option.image.bounds.extents.x * rootCanvas.referencePixelsPerUnit;
					layoutElement.preferredHeight = option.image.bounds.extents.y * rootCanvas.referencePixelsPerUnit;
					break;
			}
		}

		_manager.SetAllSubPanels(allPanels.ToArray());
		_manager.ResetPanels(); //disable all children
	}

	//GENERATION Utility Below
	private MenuButton CreateSceneButton(Transform parent, MenuPanel panel, MenuOption option)
	{
		//create default button as child of panel and configure it accordingly
		var g = new GameObject().AddComponent<RectTransform>();

		//create image
		var image = g.gameObject.AddComponent<Image>();
		image.sprite = theme.buttonImage;
		image.type = Image.Type.Sliced;

		//Create button
		var button = g.gameObject.AddComponent<MenuButton>(); //this adds a Button component because of [RequireComponent]
		button.SetFromOption(option);
		button.SetPanel(panel);
		button.SetMenuManager(_manager);
		g.transform.SetParent(parent);
		g.name = option.optionName + " Button";

		//Create layout element
		var layoutElement = g.gameObject.AddComponent<LayoutElement>();
		layoutElement.minWidth = 50;
		layoutElement.minHeight = 30;
		layoutElement.preferredWidth = 300;
		layoutElement.preferredHeight = 50;

		//Create child text object.
		var t = new GameObject().AddComponent<RectTransform>();
		t.gameObject.name = option.optionName + " Button Text";
		t.SetParent(g);
		SetTransformToFullStretch(t);
		var tmp = t.gameObject.AddComponent<TextMeshProUGUI>();
		tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
		tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
		tmp.autoSizeTextContainer = true;
		tmp.text = option.optionName;
		tmp.color = theme.textColor;
		return button;
	}

	private void AddVerticalLayoutGroupComponent(GameObject gameObject)
	{
		var layout = gameObject.AddComponent<VerticalLayoutGroup>();
		layout.childForceExpandHeight = false;
		layout.childForceExpandWidth = false;
		layout.childControlHeight = true;
		layout.childControlWidth = true;
		layout.childAlignment = TextAnchor.UpperCenter;
		var padding = new RectOffset();
		padding.top = 100;
		padding.bottom = 100;
		layout.padding = padding;
		layout.spacing = 20;
	}

	private TextMeshProUGUI AddTextMeshComponent(GameObject gameObject)
	{
		var text = gameObject.AddComponent<TextMeshProUGUI>();
		text.fontSize = 36;
		text.alignment = TextAlignmentOptions.Center;
		text.color = theme.textColor;
		return text;
	}

	private void CreateMenuManager()
	{
		_manager = new GameObject().AddComponent<RectTransform>().gameObject.AddComponent<MenuManager>();
		_manager.gameObject.name = "Menu";
		_manager.transform.SetParent(rootCanvas.transform);
		SetTransformToFullStretch(_manager.transform as RectTransform);
		//set color of background
	}

	//Todo: change to extension method
	private void SetTransformToFullStretch(RectTransform t)
	{
		t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
		t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
		t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
		t.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
		t.anchorMin = Vector2.zero;
		t.anchorMax = Vector2.one;
		t.pivot = new Vector2(0.5f, 0.5f);
	}

	private MenuPanel CreateMainMenuPanel(string panelName, Transform parent)
	{
		var panel = new GameObject().AddComponent<RectTransform>().gameObject.AddComponent<MenuPanel>();
		panel.gameObject.name = panelName + " Panel";
		panel.transform.SetParent(parent);
		SetTransformToFullStretch(panel.transform as RectTransform);

		//Add default layout group
		AddVerticalLayoutGroupComponent(panel.gameObject);

		if (CreateMenuTitleText)
		{
			//add title text child
			var title = new GameObject().AddComponent<RectTransform>();
			title.transform.SetParent(panel.transform);
			title.name = panelName + " Title";
			var textMesh = AddTextMeshComponent(title.gameObject);
			textMesh.text = panelName;
			textMesh.fontSize = 64;
		}

		return panel;
	}

	private Image CreateBackgroundImageChild(Transform parent, Color color)
	{
		var image = new GameObject().AddComponent<RectTransform>().gameObject.AddComponent<Image>();
		image.transform.SetParent(parent.transform);
		image.color = theme.panelBackgroundColor;
		image.gameObject.name = parent.gameObject.name + " Background";
		SetTransformToFullStretch(image.transform as RectTransform);
		image.color = color;
		return image;
	}

	private MenuPanel CreateSubPanel(MenuOption option, Transform parent, out Transform contentParent)
	{
		var panel = new GameObject().AddComponent<RectTransform>();
		panel.gameObject.name = option.optionName + " Panel";
		panel.transform.SetParent(_otherPanelsRoot);
		SetTransformToFullStretch(panel);
		var menuPanel = panel.gameObject.AddComponent<MenuPanel>();

		//Create bgImage object
		var image = CreateBackgroundImageChild(panel, theme.panelBackgroundColor);

		//create content object
		RectTransform content = new GameObject().AddComponent<RectTransform>();
		content.gameObject.name = option.optionName + " Content";
		contentParent = content;
		content.transform.SetParent(panel);
		SetTransformToFullStretch(content.transform as RectTransform);
		AddVerticalLayoutGroupComponent(content.gameObject);

		if (CreateSubPanelTitleText)
		{
			//add title text child
			var title = new GameObject().AddComponent<RectTransform>();
			title.transform.SetParent(content);
			title.name = option.optionName + " Title";
			AddTextMeshComponent(title.gameObject).text = option.optionName;
		}

		//Create close button
		CreateSceneButton(contentParent, menuPanel, MenuOption.CloseButtonMenuOption); //create button to close panel

		//done!
		return menuPanel;
	}
}