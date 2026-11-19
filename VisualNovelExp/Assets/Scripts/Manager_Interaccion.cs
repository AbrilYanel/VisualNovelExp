
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Manager_Interaccion : MonoBehaviour
{
    public static Manager_Interaccion I;
    bool primeraEscena;

    void Awake()
    {
        I = this;
        // Solo la primera escena de la partida resetea el progreso
        if (!Partida.Iniciada)
        {
            Partida.Iniciada = true;
            primeraEscena = true;
            if (playerProgress) playerProgress.Resetear();
        }
    }

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
    public int recompensaFallo = 5;
    private Interaccion_NPC npcActual;
    public Panel_Vocabulario panelVocabulario;

    [Header("Entrevista NPC9")]
    public Manager_Camara managerCamara;
    public Manager_Entrevista managerEntrevista;
    public Nodo_Dialogo nodoEntregaExitosa;
    public Nodo_Dialogo nodoEntregaMala;

    // --- NUEVO: aborto ---
    [Header("Aborto Minijuego")]
    [Tooltip("Permite salir con ESC de cualquier minijuego")]
    public bool permitirAbortoConEsc = true;
    [Tooltip("Texto opcional que se muestra en los minijuegos: 'ESC para salir'")]
    public TextMeshProUGUI textoAyudaSalir; // puedes arrastrar un TMP en cada UI de minijuego, o dejarlo null
    public KeyCode teclaAborto = KeyCode.Escape;

    // control interno
    int minijuegoActivo = 0;
    // 1=Emparejar,2=Minijuego2,3=VoF,4=Memorama,5=KanaRush,6=Escritura,8=Quiz
    int minijuegoActivoParaReintento = 0;
    Nodo_Dialogo nodoPostEntrega = null;

    [Header("Retratos en el diálogo")]
    public RectTransform ladoIzquierdo;
    public RectTransform ladoDerecho;
    public Image retratoNPC;               // Image del NPC dentro del panel de diálogo
    public RectTransform retratoJugador;   // objeto con CharacterLookRenderer + Retrato_Personaje + CanvasGroup
    [Range(0f, 1f)] public float opacidadQuienEscucha = 0.55f;
    [Tooltip("Marco_Retrato del HUD: se oculta mientras hay un diálogo abierto")]
    public GameObject retratoHUD;

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (choicePanel) choicePanel.SetActive(false);
        CerrarTodasUIs();
        ActualizarUIProgreso();
        if (primeraEscena) EventosJuego.Emitir("juego_inicio");
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
        minijuegoActivo = 0; // <--- importante: resetea flag
    }

    // ---------- UPDATE con ABORTO ----------
    void Update()
    {
        // --- ABORTO MINIJUEGO con ESC ---
        if (permitirAbortoConEsc && minijuegoActivo != 0)
        {
            // No interceptar ESC si el jugador está escribiendo en un InputField (Escritura Inversa)
            // TMP_InputField captura ESC para salir del campo, lo dejamos pasar primero
            bool inputFieldActivo = EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;

            // Si hay InputField activo, requiere Ctrl+ESC para abortar, para no chocar con “salir del input”
            bool abortar = false;
            if (inputFieldActivo)
            {
                // Ctrl+ESC fuerza salida aunque estés escribiendo
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(teclaAborto))
                    abortar = true;
            }
            else
            {
                if (Input.GetKeyDown(teclaAborto))
                    abortar = true;
            }

            if (abortar)
            {
                AbandonarMinijuego();
                return;
            }
        }

        // diálogo typewriter click-to-skip
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            if (dialogueText) dialogueText.text = currentSentence;
            isTyping = false;
            if (currentNode.endsDialogue)
            {
                if (nodoPostEntrega != null) { var t = nodoPostEntrega; nodoPostEntrega = null; StartDialogue(t); }
                else EndDialogue();
            }
            else if (currentNode.hasChoices) ShowChoices();
            else if (currentNode.nextNode != null) StartDialogue(currentNode.nextNode);
            else EndDialogue();
        }
    }

    /// <summary>
    /// Cierra el minijuego actual sin marcar éxito ni fallo.
    /// El NPC queda NO completado, el jugador puede ir a la tienda y volver.
    /// </summary>
    public void AbandonarMinijuego()
    {
        Debug.LogWarning($"[Interaccion] Abortando minijuego {minijuegoActivo} por ESC – volviendo al mundo");
        EventosJuego.Emitir("minijuego_abandono", minijuegoActivo.ToString());
        if (managerVoF) managerVoF.Detener();
        if (managerMemorama) managerMemorama.Detener();
        if (managerMinijuego) managerMinijuego.Detener();
        if (managerMinijuego2) managerMinijuego2.Detener();
        if (managerKanaRush) managerKanaRush.Detener();
        if (managerEscritura) managerEscritura.Detener();
        if (managerQuiz) managerQuiz.Detener();
        StopAllCoroutines();

        // cerrar UIs
        CerrarTodasUIs();

        // limpiar estados
        minijuegoActivoParaReintento = 0;
        // minijuegoActivo ya se pone a 0 en CerrarTodasUIs()

        // feedback opcional
        if (textoAyudaSalir != null)
            StartCoroutine(FlashTexto($"{textoAyudaSalir.text}", 0.3f));

        // Volver al juego – SIN marcar NPC como completado, SIN monedas
        // Opción A: volver directo al mundo
        EndDialogue();

        // Opción B: si preferís ir al nodo Fail para mostrar “¿Reintentar?”,
        // comentá EndDialogue() arriba y descomentá esto:
        /*
        minijuegoActivoParaReintento = minijuegoActivo;
        Nodo_Dialogo nodoFail = null;
        switch (lastMinigame) { ... } // usa el último minijuegoActivo guardado antes del reset
        if (nodoFail != null) StartDialogue(nodoFail); else EndDialogue();
        */
    }

    IEnumerator FlashTexto(string msg, float t)
    {
        yield return null;
    }

    bool EstaEnMinijuego()
    {
        return minijuegoActivo != 0;
    }

    // ---------- diálogo base ----------
    public void StartDialogue(Nodo_Dialogo node)
    {
        BloquearJugador();
        currentNode = node;
        EjecutarEfectosNodo(node);
        if (dialoguePanel) dialoguePanel.SetActive(true);
        MostrarRetratos(node);
        if (retratoHUD) retratoHUD.SetActive(false);
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
            bool exitosa = managerCamara.puntajeEntrevista >= managerCamara.puntajeMinimoExito; var director = FindObjectOfType<NPC_Director>();
            if (director) director.CompletarMision();
            nodoPostEntrega = exitosa ? nodoEntregaExitosa : nodoEntregaMala;
            if (playerProgress != null && !playerProgress.festivalCompletado)
            {
                playerProgress.festivalCompletado = true;
                EventosJuego.Emitir("festival_completado", exitosa ? "exitosa" : "mala");
                if (Mensaje_UI.I)
                    Mensaje_UI.I.Mostrar("¡La entrevista llegó a tiempo para el festival! Puedes seguir explorando el barrio.", 6f);
            }
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
            if (nodoPostEntrega != null) { var temp = nodoPostEntrega; nodoPostEntrega = null; StartDialogue(temp); yield break; }
            EndDialogue(); yield break;
        }
        if (currentNode.hasChoices) ShowChoices();
        else if (currentNode.nextNode != null) { yield return new WaitForSeconds(0.8f); StartDialogue(currentNode.nextNode); }
    }

    void ShowChoices()
    {
        if (!currentNode.hasChoices) return;
        if (choicePanel) choicePanel.SetActive(true);
        if (buttonText1) buttonText1.text = currentNode.option1Text;
        if (buttonText2) buttonText2.text = currentNode.option2Text;
        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();
        button1.onClick.AddListener(() => ChooseOption(1));
        button2.onClick.AddListener(() => ChooseOption(2));
    }

    void ChooseOption(int option)
    {
        if (choicePanel) choicePanel.SetActive(false);
        var nextNode = option == 1 ? currentNode.option1Next : currentNode.option2Next;
        if (nextNode != null)
        {
            if (nextNode.startsMinigame)
            {
                currentNode = nextNode;
                bool hayPalabras = nextNode.vocabulario != null && nextNode.vocabulario.Count > 0;
                string efectos = TextoEfectosNecesidades();
                if (panelVocabulario != null && (hayPalabras || efectos != ""))
                {
                    if (dialoguePanel) dialoguePanel.SetActive(false);
                    panelVocabulario.Mostrar(nextNode.vocabulario, efectos, StartMinigame);
                }
                else StartMinigame();
                return;
            }
            if (nextNode.reintentarMinijuego) { ReintentarMinijuego(); return; }
            StartDialogue(nextNode);
        }
    }

    // ---------- Necesidades -> rendimiento ----------
    float baseTiempoKana = -1f;
    float multMonedasPartida = 1f;  // se guarda al empezar cada minijuego
    int baseErroresMemorama = -1, baseErroresEmparejar = -1;

    void AplicarNecesidadesAMinijuegos()
    {
        multMonedasPartida = 1f;
        var nec = Manager_Necesidades.I;
        if (nec == null) return;
        multMonedasPartida = nec.MultiplicadorMonedas;

        // Hambre: tiempo de Kana Rush
        if (managerKanaRush)
        {
            if (baseTiempoKana < 0f) baseTiempoKana = managerKanaRush.tiempoPorPregunta;
            managerKanaRush.tiempoPorPregunta = baseTiempoKana * Mathf.Lerp(0.7f, 1f, nec.hambre / 100f);
        }

        // Energía: errores permitidos
        int ajuste = nec.energia >= 75f ? 1 : (nec.energia < 50f ? -1 : 0);
        if (managerMemorama)
        {
            if (baseErroresMemorama < 0) baseErroresMemorama = managerMemorama.erroresMaximos;
            managerMemorama.erroresMaximos = Mathf.Max(1, baseErroresMemorama + ajuste);
        }
        if (managerMinijuego)
        {
            if (baseErroresEmparejar < 0) baseErroresEmparejar = managerMinijuego.erroresMaximos;
            managerMinijuego.erroresMaximos = Mathf.Max(1, baseErroresEmparejar + ajuste);
        }
    }

    // Solo arma el texto de los efectos, para mostrarlo en la ficha
    string TextoEfectosNecesidades()
    {
        var nec = Manager_Necesidades.I;
        if (nec == null) return "";
        var efectos = new List<string>();
        if (nec.hambre < 50f) efectos.Add("Con hambre: menos tiempo en los desafíos con reloj.");
        if (nec.energia >= 75f) efectos.Add("Descansado: +1 error permitido.");
        else if (nec.energia < 50f) efectos.Add("Cansado: −1 error permitido.");
        if (nec.TodoAlto) efectos.Add("¡En plena forma: +50 % de monedas!");
        else if (nec.diversion <= 0f) efectos.Add("Aburrido: ganarás la mitad de monedas.");
        return string.Join("\n", efectos);
    }
    // ---------- MINIJUEGOS ----------

    void StartMinigame()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (retratoHUD) retratoHUD.SetActive(true);
        if (choicePanel) choicePanel.SetActive(false);
        BloquearJugador();
        CerrarTodasUIs();
        AplicarNecesidadesAMinijuegos();

        var tipo = currentNode.tipoMinijuego;
        if (tipo == Nodo_Dialogo.TipoMinijuego.Minijuego1) tipo = Nodo_Dialogo.TipoMinijuego.Emparejar;
        if (tipo == Nodo_Dialogo.TipoMinijuego.Minijuego2) tipo = Nodo_Dialogo.TipoMinijuego.FillBlank;
        EventosJuego.Emitir("minijuego_inicio", tipo.ToString());

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
                Debug.Log("[Interaccion] Entrevista se inicia con R, no desde StartMinigame");
                EndDialogue();
                break;
            default:
                Debug.LogWarning("Minijuego no implementado: " + tipo);
                EndDialogue();
                break;
        }

        // mostrar hint ESC
        if (minijuegoActivo != 0)
        {
            Debug.Log($"[Interaccion] Minijuego {minijuegoActivo} iniciado – presiona ESC para salir");
            if (textoAyudaSalir != null)
            {
                textoAyudaSalir.gameObject.SetActive(true);
                textoAyudaSalir.text = "ESC para salir  •  TAB para journal";
            }
        }
    }

    public void OnMinigameFinished(bool success)
    {
        int juegoTerminado = minijuegoActivo;
        CerrarTodasUIs(); // esto pone minijuegoActivo = 0
        EventosJuego.Emitir(success ? "minijuego_exito" : "minijuego_fallo", juegoTerminado.ToString());

        // Cada minijuego terminado, ganado o perdido, ocupa una franja del día
        if (Manager_Dia.I) Manager_Dia.I.UsarFranja("minijuego");

        float mult = multMonedasPartida;
        bool esTrabajo = npcActual != null && npcActual.repetible;

        if (success)
        {
            if (playerProgress != null)
            {
                if (esTrabajo)
                {
                    playerProgress.GanarMonedas(Mathf.RoundToInt(npcActual.pagoTrabajo * mult));
                }
                else
                {
                    playerProgress.CompletarInteraccion();
                    playerProgress.GanarMonedas(Mathf.RoundToInt(recompensaMonedas * mult));
                }
                ActualizarUIProgreso();
            }
            if (npcActual != null && !esTrabajo) npcActual.MarcarCompletado();

            Nodo_Dialogo nodoOk = null;
            switch (juegoTerminado)
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
            // Consuelo solo la primera vez que se pierde con cada NPC (y nunca en el trabajo)
            string clave = "consuelo_" + (npcActual != null ? npcActual.name : juegoTerminado.ToString());
            if (playerProgress != null && !esTrabajo && Partida.vistos.Add(clave))
            {
                playerProgress.GanarMonedas(Mathf.RoundToInt(recompensaFallo * mult));
                ActualizarUIProgreso();
            }

            minijuegoActivoParaReintento = juegoTerminado;
            Nodo_Dialogo nodoFail = null;
            switch (juegoTerminado)
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
        if (Manager_Dia.I != null && Manager_Dia.I.EsDeNoche)
        {
            if (Mensaje_UI.I) Mensaje_UI.I.Mostrar("Ya es de noche. Vuelve a intentarlo mañana.", 3f);
            EndDialogue();
            return;
        }
        CerrarTodasUIs();
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (retratoHUD) retratoHUD.SetActive(true);
        if (choicePanel) choicePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        minijuegoActivo = minijuegoActivoParaReintento;
        AplicarNecesidadesAMinijuegos();

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
        EventosJuego.Emitir("minijuego_reintento", minijuegoActivo.ToString());
    }

    public void BloquearJugador()
    {
        EstadoUI.Bloqueada = true;
        if (cameraController) cameraController.enabled = false;
        if (Player_Movement) Player_Movement.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LiberarJugador()
    {
        EstadoUI.Bloqueada = false;
        EstadoUI.FrameLiberada = Time.frameCount;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cameraController) cameraController.enabled = true;
        if (Player_Movement) Player_Movement.enabled = true;
    }


    void MostrarRetratos(Nodo_Dialogo node)
    {
        if (node == null) return;

        Sprite sprite = node.retratoNPC != null ? node.retratoNPC
                      : (npcActual != null ? npcActual.retratoDialogo : null);
        bool npcIzquierda = npcActual != null && npcActual.retratoALaIzquierda;
        bool hayNPC = sprite != null;

        if (retratoNPC != null)
        {
            retratoNPC.sprite = sprite;
            retratoNPC.preserveAspect = true;
            retratoNPC.raycastTarget = false;
            retratoNPC.gameObject.SetActive(hayNPC);
            Colocar(retratoNPC.rectTransform, npcIzquierda ? ladoIzquierdo : ladoDerecho);
            retratoNPC.color = new Color(1f, 1f, 1f, node.hablaJugador ? opacidadQuienEscucha : 1f);
        }

        if (retratoJugador != null)
        {
            retratoJugador.gameObject.SetActive(true);
            Colocar(retratoJugador, npcIzquierda ? ladoDerecho : ladoIzquierdo);
            var grupo = retratoJugador.GetComponent<CanvasGroup>();
            if (grupo)
            {
                grupo.blocksRaycasts = false;
                grupo.alpha = (node.hablaJugador || !hayNPC) ? 1f : opacidadQuienEscucha;
            }
        }
    }

    static void Colocar(RectTransform rt, RectTransform lado)
    {
        if (rt == null || lado == null || rt.parent == lado) return;
        rt.SetParent(lado, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void EndDialogue()
    {
        if (retratoHUD) retratoHUD.SetActive(true);
        CerrarTodasUIs();
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (choicePanel) choicePanel.SetActive(false);
        LiberarJugador();
        minijuegoActivo = 0;
        minijuegoActivoParaReintento = 0;
    }

    public void ActualizarUIProgreso()
    {
        if (textoNivel != null && playerProgress != null)
            textoNivel.text = $"Nivel {playerProgress.nivelActual} — {playerProgress.interaccionesCompletadas}/{playerProgress.interaccionesPorNivel} interacciones";
        foreach (var npc in FindObjectsOfType<Interaccion_NPC>())
            npc.ActualizarIndicador();
    }

    public void SetNPCActual(Interaccion_NPC npc) { npcActual = npc; }

    // Botón UI opcional "Salir"
    public void BotonSalirMinijuego() { AbandonarMinijuego(); }
    public void SalirDelDialogo() { EndDialogue(); }
}
