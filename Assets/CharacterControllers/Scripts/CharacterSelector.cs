using Cinemachine;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [HideInInspector]
    public int selectedIndex;
    public GameObject[] characters;
    [HideInInspector]
    public string[] charactersString;
    public CinemachineVirtualCamera virtualCamera;
    private Transform CameraTarget;
    void Start()
    {
        if(Application.isPlaying) 
        {
            SelectCharacter();
        }
    }

    public void UpdateCharacters()
    {
        //characters = GameObject.FindGameObjectsWithTag("Player");
        charactersString = new string[characters.Length];
        for (int i = 0; i < charactersString.Length; i++)
        {
            charactersString[i] = characters[i].ToString();
        }
    }
    public void SelectCharacter() 
    {
        characters[selectedIndex].SetActive(true);
        for (int i = 0; i < characters.Length; i++)
        {
            if(characters[selectedIndex] != characters[i])
                characters[i].SetActive(false);
        }
        CameraTarget = characters[selectedIndex].transform.Find("Camera Target");
        virtualCamera.Follow = CameraTarget;
    }
}
