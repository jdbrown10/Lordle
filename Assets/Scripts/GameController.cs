using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // Our different colors that we will use
    private Color colorCorrect = new Color(0.3254902f, 0.5529412f, 0.3058824f);
    private Color colorIncorrectPlace = new Color(0.7098039f, 0.6235294f, 0.2313726f);
    private Color colorUnused = new Color(0.2039216f, 0.2039216f, 0.2f);

    // The sprite that will be used when a box "cleared"
    public Sprite clearedWordBoxSprite;

    // Reference to the player controller script
    public PlayerController playerController;
    
    private int amountOfRows = 5;
    
    // List with all the words
    private List<string> dictionary = new List<string>();

    // List with words that can be chosen as correct words
    private List<string> guessingWords = new List<string>();

    public string correctWord;

    // All wordboxes
    public List<Transform> wordBoxes = new List<Transform>();

    // Current wordbox that we're inputting in
    public int currentWordBox;

    // The current row that we're currently at
    public int currentRow;

    // How many characters are there per row
    private int charactersPerRowCount = 5;

    public GameObject popup;

    private Coroutine popupRoutine;

    // Start is called before the first frame update
    void Start()
    {
        // Populate the dictionary
        AddWordsToList("Assets/Resources/dictionary.txt", dictionary);

        // Populate the guessing words
        AddWordsToList("Assets/Resources/wordlist.txt", guessingWords);

        // Choose a random correct word
        correctWord = GetRandomWord();
    }  

    string GetRandomWord()
    {
        string randomWord = guessingWords[Random.Range(0, guessingWords.Count)];
        Debug.Log(randomWord);
        return randomWord;
    }

    public void AddLetterToWordBox(string letter)
    {
        if (currentRow > amountOfRows)
        {
            Debug.Log("No more rows available");
            return;
        }

        int currentlySelectedWordbox = (currentRow * charactersPerRowCount) + currentWordBox;

        if (wordBoxes[(currentRow * charactersPerRowCount) + currentWordBox].GetChild(0).GetComponent<Text>().text == "")
        {
            wordBoxes[(currentRow * charactersPerRowCount) + currentWordBox].GetChild(0).GetComponent<Text>().text = letter;
        }

        if ((currentRow * charactersPerRowCount)+currentWordBox < (currentRow * charactersPerRowCount) + 4)
        {
            currentWordBox++;
        }
    }


    void AddWordsToList(string path, List<string> listOfWords)
    {
        // Read the text from the file
        StreamReader reader = new StreamReader(path);
        string text = reader.ReadToEnd();

        // Output the text to the console
        Debug.Log(text);

        // Separate them for each ',' character
        char[] separator = { ',' };
        string[] singleWords = text.Split(separator);

        // Add everyone of them to the list provided as a variable
        foreach (string newWord in singleWords)
        {
            listOfWords.Add(newWord);
        }

        // Close the reader
        reader.Close();
    }

    public void RemoveLetterFromWordBox()
    {
        if (currentRow > amountOfRows)
        {
            Debug.Log("No more rows available");
            return;
        }
        int currentlySelectedWordbox = (currentRow * charactersPerRowCount) + currentWordBox;

        // If the text in the current wordbox is empty, go back a step and clear the one
        // that comes after
        if (wordBoxes[currentlySelectedWordbox].GetChild(0).GetComponent<Text>().text == "")
        {
            if (currentlySelectedWordbox > ((currentRow * charactersPerRowCount)))
            {
                // Step back
                currentWordBox--;
            }
            // Update the variable
            currentlySelectedWordbox = (currentRow * charactersPerRowCount) + currentWordBox;

            wordBoxes[currentlySelectedWordbox].GetChild(0).GetComponent<Text>().text = "";
        }
        else
        {
            // If it wasn't empty, we clear the one selected instead
            wordBoxes[currentlySelectedWordbox].GetChild(0).GetComponent<Text>().text = "";
        }

    }

    public void SubmitWord()
    {
        if (currentRow > amountOfRows)
        {
            Debug.Log("No more rows available");
            // Let the player know that they lost,
            // and what the correct word was,
            // this popup does not disappear
            ShowPopup("You Lost!\n" + "Correct word was:" + correctWord, 0f, true);
            return;
        }
        // The players guess
        string guess = "";

        for (int i = (currentRow * charactersPerRowCount); i < (currentRow * charactersPerRowCount) + currentWordBox + 1; i++)
        {
            // Add each letter to the players guess
            guess += wordBoxes[i].GetChild(0).GetComponent<Text>().text.ToLower();
        }

        // Return if the answer did not contain exactly 5 letters
        if (guess.Length != 5)
        {
            Debug.Log("Answer too short, must be 5 letters");
             // Let the player know that the submitted word is too short
            ShowPopup("Answer too short, must be 5 letters", 2f, false);
            return;
        }

        // All words are in lowercase, so let's convert the guess to that as well
        guess = guess.ToLower();

        // Check if the word exists in the dictionary 
        bool wordExists = false;
        foreach (var word in dictionary)
        {
            if (guess == word)
            {
                wordExists = true;
                break;
            }
        }
        // If it didn't exist in the dictionary, does it exist in the other list
        if (wordExists == false)
        {
            foreach (var word in guessingWords)
            {
                if (guess == word)
                {
                    wordExists = true;
                    break;
                }
            }
        }
        if (wordExists == false)
        {
            // Let the player know that the submitted word is too short
            ShowPopup("Word does not exist in dictionary!", 2f, false);
            return;
        }


        // Output the guess to the console
        Debug.Log("Player guess:" + guess);
        CheckWord(guess);
        
        // If the guess was correct, output that the player has won into the console
        if (guess == correctWord)
        {
            // Let the player know that they won!
            // This popup stays forever as well
            ShowPopup("You win!", 0f, true);
            Debug.Log("Correct word!");
        }
        else
        {
            // If the guess was incorrect, go to the next row
            Debug.Log("Wrong, guess again!");
            // Restart at the leftmost character
            currentWordBox = 0;
            currentRow++;
        }
    }

    void CheckWord(string guess)
    {
        // Set up variables
        char[] playerGuessArray = guess.ToCharArray();
        string tempPlayerGuess = guess;
        char[] correctWordArray = correctWord.ToCharArray();
        string tempCorrectWord = correctWord;

        // Swap correct characters with '0'
        for (int i = 0; i < 5; i++)
        {
            if (playerGuessArray[i] == correctWordArray[i])
            {
                // Correct place
                playerGuessArray[i] = '0';
                correctWordArray[i] = '0';
            }
        }

        // Update the information
        tempPlayerGuess = "";
        tempCorrectWord = "";
        for (int i = 0; i < 5; i++)
        {
            tempPlayerGuess += playerGuessArray[i];
            tempCorrectWord += correctWordArray[i];
        }

        // Check for characters in wrong place, but correct letter
        for (int i = 0; i < 5; i++)
        {
            if (tempCorrectWord.Contains(playerGuessArray[i].ToString()) && playerGuessArray[i] != '0')
            {
                char playerCharacter = playerGuessArray[i];
                playerGuessArray[i] = '1';
                tempPlayerGuess = "";
                for (int j = 0; j < 5; j++)
                {
                    tempPlayerGuess += playerGuessArray[j];
                }

                // Update the correct word string with a '.'
                // so that we only check for the correct amount of characters.
                int index = tempCorrectWord.IndexOf(playerCharacter, 0);
                correctWordArray[index] = '.';
                tempCorrectWord = "";
                for (int j = 0; j < 5; j++)
                {
                    tempCorrectWord += correctWordArray[j];
                }
            }
        }

        // Set the fallback color to gray
        Color newColor = colorUnused;

        // Go through the players answer and color each button and wordbox accordingly
        for (int i = 0; i < 5; i++)
        {

            if (tempPlayerGuess[i] == '0')
            {
                // Correct placement
                newColor = colorCorrect;
            }
            else if (tempPlayerGuess[i] == '1')
            {
                // Correct character, wrong placement
                newColor = colorIncorrectPlace;
            }
            else
            {
                // Character not used
                newColor = colorUnused;
            }

            // Reference variable
            Image currentWordboxImage = wordBoxes[i + (currentRow * charactersPerRowCount)].GetComponent<Image>();

            // Change the sprite
            currentWordboxImage.sprite = clearedWordBoxSprite;

            // Set the color of the wordbox to the "new color"
            currentWordboxImage.color = newColor;

            // Set the color of the keyboard character to the "new color", only if it's "better" than the previous one

            // Saving a variable for the current keyboard image
            Image keyboardImage = playerController.GetKeyboardImage(guess[i].ToString());

            // Always possible to set the correct placement color
            if (newColor == colorCorrect)
            {
                keyboardImage.color = newColor;
            }

            // Only set the colorIncorrectPlace if it's not the colorCorrect
            if (newColor == colorIncorrectPlace && keyboardImage.color != colorCorrect)
            {
                keyboardImage.color = newColor;
            }

            // Only set the unused color if it's not colorIncorrectPlace and colorCorrect
            if (newColor == colorUnused && keyboardImage.color != colorCorrect && keyboardImage.color != colorIncorrectPlace)
            {
                keyboardImage.color = newColor;
            }

        }
    }

    void ShowPopup(string message, float duration, bool stayForever)
    {
        // If a popup routine exists, we should stop that first,
        // this makes sure that not 2 coroutines can run at the same time.
        // Since we are using the same popup for every message, we only want one of these coroutines to run at any time
        if (popupRoutine != null)
        {
            StopCoroutine(popupRoutine);
        }
        popupRoutine = StartCoroutine(ShowPopupRoutine(message, duration, stayForever));
    }

    IEnumerator ShowPopupRoutine(string message, float duration, bool stayForever = false)
    {
        // Set the message of the popup
        popup.transform.GetChild(0).GetComponent<Text>().text = message;
        // Activate the popup
        popup.SetActive(true);
        // If it should stay forever or not
        if (stayForever)
        {
            while (true)
            {
                yield return null;
            }
        }
        // Wait for the duration time
        yield return new WaitForSeconds(duration);
        // Deactivate the popup
        popup.SetActive(false);
    }
}