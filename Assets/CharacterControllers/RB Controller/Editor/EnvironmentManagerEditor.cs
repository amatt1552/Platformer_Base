using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnvironmentManager))]

public class EnvironmentManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();
        EnvironmentManager myLightingSetter = (EnvironmentManager)target;
        if (GUILayout.Button("Update"))
        {
            myLightingSetter.UpdateEnvironmentSettings();
        }
    }
}
