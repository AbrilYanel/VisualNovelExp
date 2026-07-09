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
    public GameObject choicePanel;
    public Button button1;
    public Button button2;
    public TextMeshProUGUI buttonText1;
    public TextMeshProUGUI buttonText2;
    public float typingSpeed = 0.03f;

    public MonoBehaviour cameraController;
    public MonoBehaviour Player_Movement;

    private Nodo_Dialogo currentNode;
    private string currentSentence;
    private bool isTyping = false;

    // --- Minijuegos ---
    [Header("Emparejar NPC3")]
    public GameObject minigameUI;
    public Manager_Minijuego managerMinijuego;
    public Nodo_Dialogo nodoSuccessEmparejar;
    public Nodo_Dialogo nodoFailEmparejar;

    [Header("FillBlank / WordOrder NPC4 / NPC7")]
    public GameObject minijuego2UI;
    public Manager_Minijuego2 managerMinijuego2;
    public Nodo_Dialogo nodoSuccess2;
    public Nodo_Dialogo nodoFail2;
    public List<PreguntaData> preguntasFillBlank_NPC4;
    public List<PreguntaData> preguntasWordOrder_NPC7;

    [Header("VoF NPC1")]
    public GameObject vofUI;
    public Manager_VerdaderoFalso managerVoF;
    public Nodo_Dialogo nodoSuccessVoF;
    public Nodo_Dialogo nodoFailVoF;
    public List<Manager_VerdaderoFalso.PreguntaVoF> preguntasVoF_NPC1;

    [Header("Memorama NPC2")]
    public GameObject memoramaUI;
    public Manager_Memorama managerMemorama;
    public Nodo_Dialogo nodoSuccessMemorama;
    public Nodo_Dialogo nodoFailMemorama;

    [Header("Kana Rush NPC5")]
    public GameObject kanaRushUI;
    public Manager_KanaRush managerKanaRush;
    public Nodo_Dialogo nodoSuccessKanaRush;
    public Nodo_Dialogo nodoFailKanaRush;

    [Header("Escritura Inversa NPC6")]
    public GameObject escrituraUI;
    public Manager_EscrituraInversa managerEscritura;
    public Nodo_Dialogo nodoSuccessEscritura;
    public Nodo_Dialogo nodoFailEscritura;

    [Header("Quiz Cortesía NPC8")]
    public GameObject quizCortesiaUI;
    public Manager_QuizCortesia managerQuiz;
    public Nodo_Dialogo nodoSuccessQuiz;
    public Nodo_Dialogo nodoFailQuiz;
    public List<Manager_QuizCortesia.PreguntaCortesia> preguntasQuiz_NPC8;

    [Header("Progreso")]
    public PlayerProgress playerProgress;
    public TextMeshProUGUI textoNivel;
    public int recompensaMonedas = 20;
    private Interaccion_NPC npcActual;

    [Header("Entrevista NPC9")]
    public Manager_Camara managerCamara;
    public Manager_Entrevista managerEntrevista;
    public Nodo_Dialogo nodoEntregaExitosa;
    public Nodo_Dialogo nodoEntregaMala;

    // control interno
    int minijuegoActivo = 0;
    // 1=Emparejar,2=Minijuego2,3=VoF,4=Memorama,5=KanaRush,6=Escritura,8=Quiz
    int minijuegoActivoParaReintento = 0;
    Nodo_Dialogo nodoPostEntrega = null;

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (choicePanel) choicePanel.SetActive(false);
        CerrarTodasUIs();
        if (playerProgress) playerProgress.Resetear();
        ActualizarUIProgreso();
    }

    void CerrarTodasUIs()
    {
        if (minigameUI) minigameUI.SetActive(false);
        if (minijuego2UI) minijuego2UI.SetActive(false);
        if (vofUI) vofUI.SetActive(false);
        if (memoramaUI) memoramaUI.SetActive(false);
        if (kanaRushUI) kanaRushUI.SetActive(false);
        if (escrituraUI) escrituraUI.SetActive(false);
        if (quizCortesiaUI) quizCortesiaUI.SetActive(false);
    }

    // ---------- diálogo base (igual que antes) ----------
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
        currentNode = node;
        EjecutarEfectosNodo(node);
        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (choicePanel) choicePanel.SetActive(false);
        if (nameText) nameText.text = node.speakerName;
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
        if (dialogueText) dialogueText.text = "";
        foreach (char letter in currentSentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        if (currentNode.endsDialogue)
        {
            yield return new WaitForSeconds(0.8f);
            if (nodoPostEntrega != null) { var t = nodoPostEntrega; nodoPostEntrega = null; StartDialogue(t); yield break; }
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
        var nextNode = option == 1 ? currentNode.option1Next : currentNode.option2Next;
        if (nextNode != null)
        {
            if (nextNode.startsMinigame) { currentNode = nextNode; StartMinigame(); return; }
            if (nextNode.reintentarMinijuego) { ReintentarMinijuego(); return; }
            StartDialogue(nextNode);
        }
    }

    // ---------- MINIJUEGOS ----------
    void StartMinigame()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (choicePanel) choicePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (cameraController) cameraController.enabled = false;
        if (Player_Movement) Player_Movement.enabled = false;
        CerrarTodasUIs();

        var tipo = currentNode.tipoMinijuego;
        // legacy compat
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
                if (managerMemorama) managerMemorama.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.Emparejar:
                minijuegoActivo = 1;
                if (minigameUI) minigameUI.SetActive(true);
                if (managerMinijuego) managerMinijuego.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.FillBlank:
                minijuegoActivo = 2;
                if (managerMinijuego2 && preguntasFillBlank_NPC4 != null)
                    managerMinijuego2.SetPreguntas(preguntasFillBlank_NPC4);
                if (minijuego2UI) minijuego2UI.SetActive(true);
                if (managerMinijuego2) managerMinijuego2.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.WordOrder:
                minijuegoActivo = 2;
                if (managerMinijuego2 && preguntasWordOrder_NPC7 != null)
                    managerMinijuego2.SetPreguntas(preguntasWordOrder_NPC7);
                if (minijuego2UI) minijuego2UI.SetActive(true);
                if (managerMinijuego2) managerMinijuego2.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.KanaRush:
                minijuegoActivo = 5;
                if (kanaRushUI) kanaRushUI.SetActive(true);
                if (managerKanaRush) managerKanaRush.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.EscrituraInversa:
                minijuegoActivo = 6;
                if (escrituraUI) escrituraUI.SetActive(true);
                if (managerEscritura) managerEscritura.Iniciar();
                break;

            case Nodo_Dialogo.TipoMinijuego.QuizCortesia:
                minijuegoActivo = 8;
                if (quizCortesiaUI) quizCortesiaUI.SetActive(true);
                if (managerQuiz)
                {
                    if (preguntasQuiz_NPC8 != null && preguntasQuiz_NPC8.Count > 0)
                        managerQuiz.SetPreguntas(preguntasQuiz_NPC8);
                    managerQuiz.Iniciar();
                }
                break;

            case Nodo_Dialogo.TipoMinijuego.Entrevista:
                // la entrevista se lanza vía NPC_Entrevistado con R, no desde aquí
                Debug.Log("[Interaccion] Entrevista se inicia con R, no desde StartMinigame");
                EndDialogue();
                break;

            default:
                Debug.LogWarning("Minijuego no implementado: " + tipo);
                EndDialogue();
                break;
        }
    }

    public void OnMinigameFinished(bool success)
    {
        CerrarTodasUIs();

        if (success)
        {
            if (playerProgress != null)
            {
                playerProgress.CompletarInteraccion();
                playerProgress.GanarMonedas(recompensaMonedas);
                ActualizarUIProgreso();
            }
            if (npcActual != null) npcActual.MarcarCompletado();

            Nodo_Dialogo nodoOk = null;
            switch (minijuegoActivo)
            {
                case 3: nodoOk = nodoSuccessVoF; break;
                case 4: nodoOk = nodoSuccessMemorama; break;
                case 1: nodoOk = nodoSuccessEmparejar; break;
                case 2: nodoOk = nodoSuccess2; break;
                case 5: nodoOk = nodoSuccessKanaRush; break;
                case 6: nodoOk = nodoSuccessEscritura; break;
                case 8: nodoOk = nodoSuccessQuiz; break;
            }
            if (nodoOk != null) StartDialogue(nodoOk); else EndDialogue();
        }
        else
        {
            minijuegoActivoParaReintento = minijuegoActivo;
            Nodo_Dialogo nodoFail = null;
            switch (minijuegoActivo)
            {
                case 3: nodoFail = nodoFailVoF; break;
                case 4: nodoFail = nodoFailMemorama; break;
                case 1: nodoFail = nodoFailEmparejar; break;
                case 2: nodoFail = nodoFail2; break;
                case 5: nodoFail = nodoFailKanaRush; break;
                case 6: nodoFail = nodoFailEscritura; break;
                case 8: nodoFail = nodoFailQuiz; break;
            }
            if (nodoFail != null) StartDialogue(nodoFail); else EndDialogue();
        }
    }

    public void ReintentarMinijuego()
    {
        CerrarTodasUIs();
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        minijuegoActivo = minijuegoActivoParaReintento;

        switch (minijuegoActivo)
        {
            case 3: if (vofUI) vofUI.SetActive(true); if (managerVoF) managerVoF.Iniciar(); break;
            case 4: if (memoramaUI) memoramaUI.SetActive(true); if (managerMemorama) managerMemorama.Iniciar(); break;
            case 1: if (minigameUI) minigameUI.SetActive(true); if (managerMinijuego) managerMinijuego.Iniciar(); break;
            case 2: if (minijuego2UI) minijuego2UI.SetActive(true); if (managerMinijuego2) managerMinijuego2.Iniciar(); break;
            case 5: if (kanaRushUI) kanaRushUI.SetActive(true); if (managerKanaRush) managerKanaRush.Iniciar(); break;
            case 6: if (escrituraUI) escrituraUI.SetActive(true); if (managerEscritura) managerEscritura.Iniciar(); break;
            case 8: if (quizCortesiaUI) quizCortesiaUI.SetActive(true); if (managerQuiz) managerQuiz.Iniciar(); break;
        }
    }

    void EndDialogue()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (choicePanel) choicePanel.SetActive(false);
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
