using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DynamicMenu : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public AudioSource audioSource;

    public AudioClip mainMenuMusic;
    public AudioClip gameOverMusic;
    public AudioClip enemyChasingMusic;
    public AudioClip finalRoomMusic;

    private Stack<string> menuStack = new Stack<string>();
    private Dictionary<string, List<string>> menuData;
    private Dictionary<string, AudioClip> soundtrackMusicMapping;

    private void Start()
    {
        // Initialize menu data
        menuData = new Dictionary<string, List<string>>
        {
            { "Root", new List<string> { "Start New Game", "Settings", "Soundtrack" } },
            { "Settings", new List<string> { "Music", "Sound Effects" } },
            { "Soundtrack", new List<string> { "Track1: Main Menu", "Track2: Game Over", "Track3: Enemy Chasing", "Track4: Final Room" } },
        };

        // Map "Soundtrack" menu items to audio clips
        soundtrackMusicMapping = new Dictionary<string, AudioClip>
        {
            {  "Track1: Main Menu", mainMenuMusic },
            { "Track2: Game Over", gameOverMusic },
            { "Track3: Enemy Chasing", enemyChasingMusic },
            { "Track4: Final Room", finalRoomMusic },
        };
        PlayDefaultMusic();


        // Start at the root menu
        UpdateMenu("Root");
    }

    private void UpdateMenu(string currentMenu)
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Add a Back button if not at the root
        if (menuStack.Count > 0)
        {
            GameObject backButton = Instantiate(buttonPrefab, buttonContainer);
            backButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
            backButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateMenu(menuStack.Pop());
            });
        }

        // Add buttons for current menu items
        if (menuData.ContainsKey(currentMenu))
        {
            foreach (var item in menuData[currentMenu])
            {
                GameObject button = Instantiate(buttonPrefab, buttonContainer);
                button.GetComponentInChildren<TextMeshProUGUI>().text = item;
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (item == "Start New Game")
                    {
                        // SceneManager.LoadScene("level1_area1");
                        Debug.Log("Starting game");
                        return;
                    }
                    if (currentMenu == "Soundtrack" && soundtrackMusicMapping.ContainsKey(item))
                    {
                        ChangeMenuMusic(soundtrackMusicMapping[item]);
                    }
                  
                    // Navigate to submenu if it exists
                    if (menuData.ContainsKey(item))
                    {
                        menuStack.Push(currentMenu); // Save current menu
                        UpdateMenu(item);
                    }
                    else
                    {
                        Debug.Log("Selected: " + item); // Handle selection
                    }
                });
            }
        }
    }

    public void ChangeMenuMusic(AudioClip newMusic)
    {
        if (audioSource.clip == newMusic) return; // Avoid restarting the same track

        audioSource.Stop();
        audioSource.clip = newMusic;
        audioSource.Play();
    }

    private void PlayDefaultMusic()
    {
        if (audioSource != null && mainMenuMusic != null)
        {
            audioSource.clip = mainMenuMusic;
            audioSource.loop = true; // Loop the main menu music
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource or Main Menu Music is not assigned!");
        }
    }
}
