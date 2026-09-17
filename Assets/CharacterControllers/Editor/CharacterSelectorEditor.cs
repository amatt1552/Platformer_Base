using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterSelector))]
public class CharacterSelectorEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        // Draw the default inspector variables first
        DrawDefaultInspector();

        CharacterSelector selector = (CharacterSelector)target;

        // Create the dropdown menu in the inspector
        selector.UpdateCharacters();
        selector.selectedIndex = EditorGUILayout.Popup("Select Current Player", selector.selectedIndex, selector.charactersString);
        selector.SelectCharacter();

        // Save the selection changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
