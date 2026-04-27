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
    public MonoBehaviour Player_Movement;

    private Nodo_Dialogo currentNode;
    [Header("Minijuego")]
    public GameObject minigameUI;
    public Manager_Minijuego managerMinijuego; 
    public Nodo_Dialogo nodoSuccess;
    public Nodo_Dialogo nodoFail;

    [Header("Minijuego 2")]
    public GameObject minijuego2UI;
    public Manager_Minijuego2 managerMinijuego2;

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

      
        if (currentNode.endsDialogue)
        {
            yield return new WaitForSeconds(1f); // opcional (para que se lea)
            EndDialogue();
            yield break;
        }

      
        if (currentNode.hasChoices)
        {
            ShowChoices();
        }
       
        else if (currentNode.nextNode != null)
        {
            yield return new WaitForSeconds(1f);
            StartDialogue(currentNode.nextNode);
        }
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
               
                if (nextNode.startsMinigame)
                {
                    StartMinigame();
                    return;
                }

                
                StartDialogue(nextNode);
        }
    }

    void StartMinigame()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (cameraController != null) cameraController.enabled = false;
        if (Player_Movement != null) Player_Movement.enabled = false;

        if (currentNode.tipoMinijuego == Nodo_Dialogo.TipoMinijuego.Minijuego1)
        {
            minigameUI.SetActive(true);
            managerMinijuego.Iniciar();
        }
        else
        {
            minijuego2UI.SetActive(true);
            managerMinijuego2.Iniciar();
        }
    }

    public void OnMinigameFinished(bool success)
    {
        minigameUI.SetActive(false);

        if (success)
        {
            StartDialogue(nodoSuccess);
        }
        else
        {
            StartDialogue(nodoFail);
        }
    }
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraController != null)
            cameraController.enabled = true;

        if (Player_Movement != null)
            Player_Movement.enabled = true;
    }
}
