using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenericMysteryGame : MonoBehaviour
{
    public TMP_Dropdown[] dropdowns; // Array of dropdowns for each category
    public Button submitButton;
    public TMP_Text feedbackText;

    private List<string> solution = new List<string>(); // Correct solution
    private List<List<string>> options = new List<List<string>>(); // Possible guesses for each category
    private int attempts = 0;
    private const int maxAttempts = 3;

    public void InitializeGame(List<string> solutionArray, List<List<string>> possibleGuesses)
    {
        // Assign solution and options
        solution = solutionArray;
        options = possibleGuesses;

        // Validate input
        if (solution.Count != dropdowns.Length || options.Count != dropdowns.Length)
        {
            Debug.LogError("Mismatch between solution/options and number of dropdowns!");
            return;
        }

        // Populate dropdowns dynamically
        for (int i = 0; i < dropdowns.Length; i++)
        {
            dropdowns[i].ClearOptions();
            Debug.Log($"Populating Dropdown {i} with options: {string.Join(", ", options[i])}");

            dropdowns[i].AddOptions(options[i]);
        }

        // Reset game state
        attempts = 0;
        feedbackText.text = "Solve the mystery!";
        submitButton.interactable = true;
    }

    private void Start()
    {
        // Attach the Submit button listener
        submitButton.onClick.AddListener(CheckAnswer);

        // Example initialization (can be replaced by dynamic setup)
        InitializeGame(
            new List<string> { "Miss Scarlet", "Knife", "Library" }, // Solution
            new List<List<string>> // Options
            {
                new List<string> { "Miss Scarlet", "Colonel Mustard", "Professor Plum" },
                new List<string> { "Knife", "Candlestick", "Revolver" },
                new List<string> { "Library", "Kitchen", "Ballroom" }
            }
        );
    }

    private void CheckAnswer()
    {
        // Get user selections
        List<string> userSelections = new List<string>();
        foreach (var dropdown in dropdowns)
        {
            userSelections.Add(dropdown.options[dropdown.value].text);
        }

        // Increment attempt count
        attempts++;

        // Check if user selections match the solution
        if (AreSelectionsCorrect(userSelections))
        {
            feedbackText.text = "Success! You solved the mystery.";
            submitButton.interactable = false; // Disable further attempts
            return;
        }

        // If incorrect and attempts are exhausted
        if (attempts >= maxAttempts)
        {
            feedbackText.text = $"Failure! You've used all {maxAttempts} attempts.";
            submitButton.interactable = false; // Disable further attempts
            return;
        }

        // If incorrect but attempts remain
        feedbackText.text = $"Incorrect! Attempts left: {maxAttempts - attempts}";
    }

    private bool AreSelectionsCorrect(List<string> userSelections)
    {
        for (int i = 0; i < solution.Count; i++)
        {
            if (userSelections[i] != solution[i])
            {
                return false;
            }
        }
        return true;
    }
}
