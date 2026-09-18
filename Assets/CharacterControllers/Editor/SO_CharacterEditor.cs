#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(SO_Character))]
public class SO_CharacterEditor : Editor
{
    //public override void OnInspectorGUI()
    //{
    //    // Draw the default inspector variables first
    //    DrawDefaultInspector();
    //    SO_Character character = (SO_Character)target;
    //    JumpComboSetting[] defaultJumpCombos = new JumpComboSetting[3];
    //    defaultJumpCombos[0] = new JumpComboSetting(JumpType.normalJump, 0, 1);
    //    defaultJumpCombos[1] = new JumpComboSetting(JumpType.meduimJump, 1, 1);
    //    defaultJumpCombos[2] = new JumpComboSetting(JumpType.highJump, 2, 1);
    //    character.JumpComboInit(defaultJumpCombos);
    //}
}
#endif