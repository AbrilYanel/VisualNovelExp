using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Manager_Interaccion : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Opciones")]
    public GameObject choicePanel;
    public Button button1;
    public Button button2;
    public TextMeshProUGUI buttonText1;
    public TextMeshProUGUI buttonText2;

    [Header("Settings")]
    public float typingSpeed = 0.03f;

    private string currentSentence;
    private bool isTyping = false;
    public MonoBehaviour cameraController;

    private Nodo_Dialogo currentNode;
    void Start()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
    }

    void Update()
    {
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            isTyping = false;
            ShowChoices();
        }
    }
    //Inicia el diálogo
    public void StartDialogue(Nodo_Dialogo node)
    {
        if (isTyping) return;
        currentNode = node;

        dialoguePanel.SetActive(true);
        choicePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraController.enabled = false;

        nameText.text = node.speakerName;
        currentSentence = node.sentence;

        StopAllCoroutines();
        StartCoroutine(TypeSentence());
    }
    IEnumerator TypeSentence()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in currentSentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        //Cuando termina el texto mostrar opciones
        ShowChoices();
    }

    void ShowChoices()
    {
        if (!currentNode.hasChoices) return;

        choicePanel.SetActive(true);

        buttonText1.text = currentNode.option1Text;
        buttonText2.text = currentNode.option2Text;

        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();

        button1.onClick.AddListener(() => ChooseOption(1));
        button2.onClick.AddListener(() => ChooseOption(2));
    }

    void ChooseOption(int option)
    {
        choicePanel.SetActive(false);

        Nodo_Dialogo nextNode = null;

        if (option == 1)
            nextNode = currentNode.option1Next;
        else
            nextNode = currentNode.option2Next;

        if (nextNode != null)
        {
            StartDialogue(nextNode);
        }
        else if (currentNode.startsMinigame)
        {
            StartMinigame();
        }
    }

    void StartMinigame()
    {
        Debug.Log("Inicia minijuego de aprender 'neko'");
    }
}
