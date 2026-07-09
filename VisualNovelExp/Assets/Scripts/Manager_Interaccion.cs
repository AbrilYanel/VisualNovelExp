

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Manager_Interaccion : MonoBehaviour
{
    [Header("UI Diálogo")]
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

    public MonoBehaviour cameraController;
    public MonoBehaviour Player_Movement;

    private Nodo_Dialogo currentNode;
    private string currentSentence;
    private bool isTyping = false;

    // --- Minijuegos ---
    [Header("Minijuego Emparejar (legacy / NPC3)")]
    public GameObject minigameUI;
    public Manager_Minijuego managerMinijuego;
    public Nodo_Dialogo nodoSuccess1;
    public Nodo_Dialogo nodoFail1;

    [Header("Minijuego 2 FillBlank/WordOrder (NPC4 / NPC7)")]
    public GameObject minijuego2UI;
    public Manager_Minijuego2 managerMinijuego2;
    public Nodo_Dialogo nodoSuccess2;
    public Nodo_Dialogo nodoFail2;

    [Header("Minijuego Verdadero/Falso NPC1")]
    public GameObject vofUI;
    public Manager_VerdaderoFalso managerVoF;
    public Nodo_Dialogo nodoSuccessVoF;
    public Nodo_Dialogo nodoFailVoF;
    public List<Manager_VerdaderoFalso.PreguntaVoF> preguntasVoF_NPC1;

    [Header("Minijuego Memorama NPC2")]
    public GameObject memoramaUI;
    public Manager_Memorama managerMemorama;
    public Nodo_Dialogo nodoSuccessMemorama;
    public Nodo_Dialogo nodoFailMemorama;

    // inyección Minijuego2
    [Header("Inyección Minijuego2")]
    public List<PreguntaData> preguntasFillBlank_NPC4;
    public List<PreguntaData> preguntasWordOrder_NPC7;

    [Header("Progreso")]
    public PlayerProgress playerProgress;
    public TextMeshProUGUI textoNivel;
    public int recompensaMonedas = 20;
    private Interaccion_NPC npcActual;

    [Header("Entrevista")]
    public Manager_Camara managerCamara;
    public Manager_Entrevista managerEntrevista;
    public Nodo_Dialogo nodoEntregaExitosa;
    public Nodo_Dialogo nodoEntregaMala;

    private int minijuegoActivo = 0; // 1=Emparejar,2=Minijuego2,3=VoF,4=Memorama
    private int minijuegoActivoParaReintento = 0;
    private Nodo_Dialogo nodoPostEntrega = null;

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (choicePanel) choicePanel.SetActive(false);
        if (playerProgress) playerProgress.Resetear();
        ActualizarUIProgreso();
    }

    void Update()
    {
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            isTyping = false;
            if (currentNode.endsDialogue) EndDialogue();
            else if (currentNode.hasChoices) ShowChoices();
            else if (currentNode.nextNode != null) StartDialogue(currentNode.nextNode);
            else EndDialogue();
        }
    }

    public void StartDialogue(Nodo_Dialogo node)
    {
        if (cameraController) cameraController.enabled = false;
        if (Player_Movement) Player_Movement.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (isTyping) return;
        currentNode = node;
        EjecutarEfectosNodo(node);
        dialoguePanel.SetActive(true);
        choicePanel.SetActive(false);
        nameText.text = node.speakerName;
        currentSentence = node.sentence;
        StopAllCoroutines();
        StartCoroutine(TypeSentence());
    }

    void EjecutarEfectosNodo(Nodo_Dialogo node)
    {
        if (node.daCamara && managerCamara != null) managerCamara.RecibirCamara();
        if (node.daPermiso && managerCamara != null) managerCamara.ObtenerPermiso();
        if (node.entregaEntrevista && managerCamara != null && managerEntrevista != null)
        {
            bool exitosa = managerCamara.puntajeEntrevista >= managerEntrevista.entrevistaData.puntajeMinimoExito;
            var director = FindObjectOfType<NPC_Director>();
            if (director) director.CompletarMision();
            nodoPostEntrega = exitosa ? nodoEntregaExitosa : nodoEntregaMala;
        }
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
            yield return new WaitForSeconds(0.8f);
            if (nodoPostEntrega != null) { var temp = nodoPostEntrega; nodoPostEntrega = null; StartDialogue(temp); yield break; }
            EndDialogue(); yield break;
        }
        if (currentNode.hasChoices) ShowChoices();
        else if (currentNode.nextNode != null) { yield return new WaitForSeconds(0.8f); StartDialogue(currentNode.nextNode); }
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
        Nodo_Dialogo nextNode = option == 1 ? currentNode.option1Next : currentNode.option2Next;
        if (nextNode != null)
        {
            if (nextNode.startsMinigame) { currentNode = nextNode; StartMinigame(); return; }
            if (nextNode.reintentarMinijuego) { ReintentarMinijuego(); return; }
            StartDialogue(nextNode);
        }
    }

    // ---------------- MINIJUEGOS ----------------
    void StartMinigame()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (cameraController) cameraController.enabled = false;
        if (Player_Movement) Player_Movement.enabled = false;

        var tipo = currentNode.tipoMinijuego;

        // Legacy compat
        if (tipo == Nodo_Dialogo.TipoMinijuego.Minijuego1) tipo = Nodo_Dialogo.TipoMinijuego.Emparejar;
        if (tipo == Nodo_Dialogo.TipoMinijuego.Minijuego2) tipo = Nodo_Dialogo.TipoMinijuego.FillBlank;

        switch (tipo)
        {
            case Nodo_Dialogo.TipoMinijuego.VerdaderoFalso:
                minijuegoActivo = 3;
                if (vofUI) vofUI.SetActive(true);
                if (managerVoF != null)
                {
                    if (preguntasVoF_NPC1 != null && preguntasVoF_NPC1.Count > 0)
                        managerVoF.SetPreguntas(preguntasVoF_NPC1);
                    managerVoF.Iniciar();
                }
                break;

            case Nodo_Dialogo.TipoMinijuego.Memorama:
                minijuegoActivo = 4;
                if (memoramaUI) memoramaUI.SetActive(true);
                if (managerMemorama != null) managerMemorama.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.Emparejar:
                minijuegoActivo = 1;
                if (minigameUI) minigameUI.SetActive(true);
                if (managerMinijuego != null) managerMinijuego.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.FillBlank:
                minijuegoActivo = 2;
                if (managerMinijuego2 != null && preguntasFillBlank_NPC4 != null)
                    managerMinijuego2.SetPreguntas(preguntasFillBlank_NPC4);
                if (minijuego2UI) minijuego2UI.SetActive(true);
                if (managerMinijuego2 != null) managerMinijuego2.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.WordOrder:
                minijuegoActivo = 2;
                if (managerMinijuego2 != null && preguntasWordOrder_NPC7 != null)
                    managerMinijuego2.SetPreguntas(preguntasWordOrder_NPC7);
                if (minijuego2UI) minijuego2UI.SetActive(true);
                if (managerMinijuego2 != null) managerMinijuego2.Iniciar();
                break;

            default:
                Debug.LogWarning($"TipoMinijuego {tipo} aún no implementado, usando Emparejar fallback");
                minijuegoActivo = 1;
                if (minigameUI) minigameUI.SetActive(true);
                if (managerMinijuego != null) managerMinijuego.Iniciar();
                break;
        }
    }

    public void OnMinigameFinished(bool success)
    {
        // cerrar todas las UIs
        if (minigameUI) minigameUI.SetActive(false);
        if (minijuego2UI) minijuego2UI.SetActive(false);
        if (vofUI) vofUI.SetActive(false);
        if (memoramaUI) memoramaUI.SetActive(false);

        if (success)
        {
            if (playerProgress != null)
            {
                playerProgress.CompletarInteraccion();
                playerProgress.GanarMonedas(recompensaMonedas);
                ActualizarUIProgreso();
            }
            if (npcActual != null) npcActual.MarcarCompletado();

            // rutear al nodo éxito correcto
            switch (minijuegoActivo)
            {
                case 3: if (nodoSuccessVoF) StartDialogue(nodoSuccessVoF); else EndDialogue(); break;
                case 4: if (nodoSuccessMemorama) StartDialogue(nodoSuccessMemorama); else EndDialogue(); break;
                case 1: if (nodoSuccess1) StartDialogue(nodoSuccess1); else EndDialogue(); break;
                case 2: if (nodoSuccess2) StartDialogue(nodoSuccess2); else EndDialogue(); break;
                default: EndDialogue(); break;
            }
        }
        else
        {
            minijuegoActivoParaReintento = minijuegoActivo;
            switch (minijuegoActivo)
            {
                case 3: if (nodoFailVoF) StartDialogue(nodoFailVoF); else EndDialogue(); break;
                case 4: if (nodoFailMemorama) StartDialogue(nodoFailMemorama); else EndDialogue(); break;
                case 1: if (nodoFail1) StartDialogue(nodoFail1); else EndDialogue(); break;
                case 2: if (nodoFail2) StartDialogue(nodoFail2); else EndDialogue(); break;
                default: EndDialogue(); break;
            }
        }
    }

    public void ReintentarMinijuego()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (cameraController) cameraController.enabled = false;
        if (Player_Movement) Player_Movement.enabled = false;

        // re-lanza según el último que falló
        minijuegoActivo = minijuegoActivoParaReintento;
        switch (minijuegoActivo)
        {
            case 3:
                if (vofUI) vofUI.SetActive(true);
                if (managerVoF) managerVoF.Iniciar();
                break;
            case 4:
                if (memoramaUI) memoramaUI.SetActive(true);
                if (managerMemorama) managerMemorama.Iniciar();
                break;
            case 1:
                if (minigameUI) minigameUI.SetActive(true);
                if (managerMinijuego) managerMinijuego.Iniciar();
                break;
            case 2:
                if (minijuego2UI) minijuego2UI.SetActive(true);
                if (managerMinijuego2) managerMinijuego2.Iniciar();
                break;
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cameraController) cameraController.enabled = true;
        if (Player_Movement) Player_Movement.enabled = true;
    }

    public void ActualizarUIProgreso()
    {
        if (textoNivel != null && playerProgress != null)
            textoNivel.text = $"Nivel {playerProgress.nivelActual} — {playerProgress.interaccionesCompletadas}/{playerProgress.interaccionesPorNivel} interacciones";
        foreach (var npc in FindObjectsOfType<Interaccion_NPC>())
            npc.ActualizarIndicador();
    }

    public void SetNPCActual(Interaccion_NPC npc) { npcActual = npc; }
}
