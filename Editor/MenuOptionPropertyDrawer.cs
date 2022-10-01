using Blooper.MenuGenerator.Runtime;
using UnityEditor;
using UnityEngine;

namespace Blooper.MenuGenerator.Editor
{
	[CustomPropertyDrawer(typeof(MenuOption))]
	public class MenuOptionPropertyDrawer : PropertyDrawer
	{
		private int visiblePropCount = 1;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			visiblePropCount = 0;
			var optionProp = property.FindPropertyRelative("optionName");
			var optType = property.FindPropertyRelative("optionType");
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			//position = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			// Calculate rects
			var firstRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			var secondRect = new Rect(position.x , position.y+firstRect.height, position.width, EditorGUIUtility.singleLineHeight);
			var thirdRect = new Rect(position.x, secondRect.y + secondRect.height, position.width, EditorGUIUtility.singleLineHeight);
			
			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			EditorGUI.PropertyField(firstRect,optionProp, new GUIContent("Name:"));
			visiblePropCount++;
			optType.enumValueIndex = (int)(MenuOptionType)EditorGUI.EnumPopup(secondRect, new GUIContent("Type"), (MenuOptionType)optType.enumValueIndex);
			visiblePropCount++;
			if ((MenuOptionType)optType.enumValueIndex == MenuOptionType.Image)
			{
				EditorGUI.PropertyField(thirdRect,property.FindPropertyRelative("image"), GUIContent.none);
				visiblePropCount++;
			}else if ((MenuOptionType)optType.enumValueIndex == MenuOptionType.LoadSceneButton)
			{
				EditorGUI.PropertyField(thirdRect,property.FindPropertyRelative("scene"), new GUIContent("Scene Name"));
				visiblePropCount++;
			}

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight*visiblePropCount;
		}
	}
}