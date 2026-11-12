using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class Player_Movement : MonoBehaviour
{
    // =====================================================================
    //  VELOCIDADES
    // =====================================================================
    [Header("Velocidades (m/s)")]
    public float walkSpeed = 2.2f;
    public float runSpeed = 4.5f;
    [Tooltip("Caminar hacia atrás es más lento que hacia adelante")]
    [Range(0.3f, 1f)] public float multiplicadorRetroceso = 0.7f;
    [Tooltip("Caminar de costado es un poco más lento")]
    [Range(0.3f, 1f)] public float multiplicadorLateral = 0.9f;
    [Tooltip("Si está activo, solo se puede correr cuando te movés hacia adelante")]
    public bool soloCorrerAdelante = true;

    [Header("Suavizado del movimiento")]
    [Tooltip("Qué tan rápido arrancás (m/s²). Más alto = más 'seco'")]
    public float aceleracion = 10f;
    [Tooltip("Qué tan rápido frenás (m/s²)")]
    public float desaceleracion = 14f;
    [Tooltip("Control en el aire (1 = igual que en el suelo)")]
    [Range(0f, 1f)] public float controlEnAire = 0.4f;

    // =====================================================================
    //  GRAVEDAD
    // =====================================================================
    [Header("Gravedad")]
    public float gravedad = -9.81f;
    public float gravityMultiplier = 2f;
    [Tooltip("Fuerza hacia abajo mientras estás en el suelo (evita que 'flote' en bordes y rampas)")]
    public float fuerzaPegadoSuelo = 2f;
    public float velocidadTerminal = 30f;

    // =====================================================================
    //  TAMAÑO DEL JUGADOR
    // =====================================================================
    [Header("Tamaño del jugador (Character Controller)")]
    [Tooltip("Si está activo, el script setea las medidas del Character Controller de forma segura. " +
             "IMPORTANTE: dejá la escala del Player en (1,1,1) y cambiá el tamaño desde acá.")]
    public bool controlarMedidasDesdeScript = true;
    [Tooltip("Altura total de la cápsula en metros")]
    public float alturaJugador = 1.7f;
    [Tooltip("Radio de la cápsula en metros")]
    public float radioJugador = 0.3f;
    [Tooltip("Altura máxima de escalón/cordón que puede subir")]
    public float alturaEscalon = 0.3f;
    [Tooltip("Si el Player tiene un CapsuleCollider extra, le copia el tamaño del Character Controller")]
    public bool sincronizarCapsuleCollider = true;

    // =====================================================================
    //  CÁMARA
    // =====================================================================
    [Header("Cámara (primera persona)")]
    [Tooltip("Cámara hija del Player. Si lo dejás vacío busca una Camera entre los hijos.")]
    public Transform camaraJugador;
    [Tooltip("Coloca la cámara a la altura de los ojos según el tamaño de la cápsula")]
    public bool ajustarAlturaCamara = true;
    [Tooltip("Distancia desde el techo de la cápsula hasta los ojos")]
    public float distanciaOjosAlTecho = 0.12f;
    [Tooltip("Distancia mínima de render de la cámara. El valor por defecto de Unity (0.3) es demasiado para un jugador chico: " +
             "corta paredes y suelo cercanos. 0 = no modificar.")]
    public float nearClipCamara = 0.05f;
    [Tooltip("Balanceo suave de la cámara al caminar")]
    public bool usarHeadBob = true;
    public float amplitudBobVertical = 0.035f;
    public float amplitudBobLateral = 0.02f;
    [Tooltip("Cuánto más fuerte es el balanceo al correr")]
    public float multiplicadorBobCorrer = 1.5f;

    // =====================================================================
    //  SONIDO DE PASOS
    // =====================================================================
    public enum ModoPasos { PasoIndividual, LoopContinuo }

    [Header("Sonido de pasos")]
    [Tooltip("PasoIndividual: un clip CORTO por cada paso (0.2 a 0.6 s), lo más realista.\n" +
             "LoopContinuo: un clip largo con varios pasos que se repite mientras caminás y se apaga con un fundido al detenerte.")]
    public ModoPasos modoPasos = ModoPasos.LoopContinuo;
    [Tooltip("AudioSource que reproduce los pasos. Si lo dejás vacío se crea uno automáticamente.")]
    public AudioSource audioPasos;
    [Tooltip("Arrastrá acá los clips de pasos. Se elige uno al azar (sin repetir el anterior).")]
    public AudioClip[] sonidosPasos;
    [Tooltip("Sonido al caer desde una altura (opcional)")]
    public AudioClip sonidoAterrizaje;
    [Tooltip("Solo LoopContinuo: clip largo de caminata. Si lo dejás vacío usa el primer clip de 'Sonidos Pasos'.")]
    public AudioClip loopCaminata;
    [Tooltip("Solo LoopContinuo: segundos que tarda en apagarse el sonido al detenerte")]
    public float fundidoSalida = 0.15f;
    [Tooltip("Solo LoopContinuo: segundos que tarda en subir el sonido al empezar a caminar")]
    public float fundidoEntrada = 0.08f;
    [Tooltip("Solo LoopContinuo: pitch al correr a máxima velocidad (el sonido se acelera)")]
    [Range(1f, 2f)] public float pitchLoopCorrer = 1.3f;
    [Range(0f, 1f)] public float volumenCaminar = 0.5f;
    [Range(0f, 1f)] public float volumenCorrer = 0.8f;
    [Range(0f, 1f)] public float volumenAterrizaje = 0.8f;
    [Range(0.5f, 1.5f)] public float pitchMinimo = 0.92f;
    [Range(0.5f, 1.5f)] public float pitchMaximo = 1.08f;
    [Tooltip("Metros entre un paso y el siguiente al caminar")]
    public float longitudPasoCaminar = 0.9f;
    [Tooltip("Metros entre un paso y el siguiente al correr")]
    public float longitudPasoCorrer = 1.4f;
    [Tooltip("Por debajo de esta velocidad no suenan pasos")]
    public float velocidadMinimaPasos = 0.3f;
    [Tooltip("Velocidad de caída mínima para que suene el aterrizaje")]
    public float velocidadMinimaAterrizaje = 4f;

    // =====================================================================
    //  REINICIO RÁPIDO
    // =====================================================================
    [Header("Reinicio rápido")]
    [Tooltip("Tecla para reiniciar la partida")]
    public KeyCode teclaReiniciar = KeyCode.R;
    [Tooltip("Requiere mantener presionado para evitar choques con la Entrevista (que también usa R)")]
    public bool requiereMantener = true;
    public float tiempoMantener = 1.2f;
    [Tooltip("Si es true, también acepta Ctrl+R instantáneo")]
    public bool permitirCtrlRInstantaneo = true;

    // referencias para no pisar otras UIs
    private Manager_Interaccion interaccionManager;
    private Manager_Tienda tiendaManager;
    private Manager_Journal journalManager;

    private float holdTimer = 0f;
    private bool mostrandoCuenta = false;

    // --- estado interno ---
    private CharacterController controller;
    private CapsuleCollider capsuleExtra;
    private Vector3 velocidadVertical;
    private Vector2 velocidadLocal;          // x = lateral, y = adelante/atrás
    private bool estabaEnSuelo = true;

    private float pasosRecorridos;           // contador continuo de pasos (también maneja el head bob)
    private int ultimoPasoSonado;
    private int ultimoClipPaso = -1;
    private float intensidadBob;
    private Vector3 camaraPosBase;

    // =====================================================================
    //  CICLO DE VIDA
    // =====================================================================
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        capsuleExtra = GetComponent<CapsuleCollider>();

        AplicarMedidas(true);
        AvisarSiHayEscala();

        // AudioSource de pasos (2D, porque el oyente es el propio jugador)
        if (audioPasos == null)
        {
            audioPasos = gameObject.AddComponent<AudioSource>();
            audioPasos.playOnAwake = false;
            audioPasos.spatialBlend = 0f;
        }

        // Cámara
        if (camaraJugador == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) camaraJugador = cam.transform;
        }

        if (camaraJugador != null)
        {
            camaraPosBase = camaraJugador.localPosition;

            Camera camComp = camaraJugador.GetComponent<Camera>();
            if (camComp != null && nearClipCamara > 0f)
                camComp.nearClipPlane = nearClipCamara;

            if (ajustarAlturaCamara && controller != null)
            {
                camaraPosBase.y = controller.center.y + controller.height * 0.5f - distanciaOjosAlTecho;
                camaraJugador.localPosition = camaraPosBase;
            }
        }
    }

    void Start()
    {
        interaccionManager = FindObjectOfType<Manager_Interaccion>();
        tiendaManager = FindObjectOfType<Manager_Tienda>();
        journalManager = FindObjectOfType<Manager_Journal>();
    }

    void OnDisable()
    {
        // Si otro script desactiva el movimiento (diálogos, minijuegos), no dejamos el audio sonando
        if (audioPasos != null) audioPasos.Stop();

        intensidadBob = 0f;
        if (camaraJugador != null) camaraJugador.localPosition = camaraPosBase;
    }

    void OnValidate()
    {
        // Se ejecuta al tocar valores en el Inspector: mantiene todo coherente y
        // permite ver la cápsula cambiar en la Scene sin darle Play.
        walkSpeed = Mathf.Max(0f, walkSpeed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
        alturaJugador = Mathf.Max(0.5f, alturaJugador);
        radioJugador = Mathf.Clamp(radioJugador, 0.1f, alturaJugador * 0.5f);
        alturaEscalon = Mathf.Max(0.01f, alturaEscalon);
        if (pitchMaximo < pitchMinimo) pitchMaximo = pitchMinimo;

        AplicarMedidas(false);
    }

    void Update()
    {
        // Si el controller está desactivado no intentamos moverlo (evita el warning de consola)
        if (controller != null && controller.enabled)
            MoverJugador();
        else if (modoPasos == ModoPasos.LoopContinuo)
            ActualizarLoopPasos(false, 0f);

        HandleReinicio();
    }

    // =====================================================================
    //  CHARACTER CONTROLLER: MEDIDAS SEGURAS
    // =====================================================================
    /// <summary>
    /// Aplica altura, radio, skin width y step offset respetando las reglas de Unity.
    /// Unity valida cada propiedad por separado, así que el Step Offset se baja primero
    /// y se sube al final; de lo contrario aparece "Step Offset must be less or equal to..."
    /// y el controller queda inactivo.
    /// </summary>
    void AplicarMedidas(bool reiniciarController)
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (controller == null || !controlarMedidasDesdeScript) return;

        float radio = Mathf.Max(0.05f, radioJugador);
        float altura = Mathf.Max(radio * 2f, alturaJugador);

        // 1) Step Offset mínimo mientras cambiamos la cápsula
        controller.stepOffset = 0.01f;

        // 2) Radio primero y altura después (la altura nunca puede ser < 2 * radio)
        controller.radius = radio;
        controller.height = altura;

        // Unity recomienda un Skin Width de ~10% del radio (0.08 con una cápsula chica traba el movimiento)
        controller.skinWidth = Mathf.Max(0.01f, radio * 0.1f);

        // 3) Step Offset final, siempre dentro del límite "alto + radio * 2" (con la escala aplicada)
        float escalaY = Mathf.Abs(transform.lossyScale.y);
        float escalaXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
        float limite = controller.height * escalaY + controller.radius * escalaXZ * 2f;
        controller.stepOffset = Mathf.Min(alturaEscalon, limite * 0.5f);

        // 4) CapsuleCollider extra (si existe) con el mismo tamaño
        if (capsuleExtra == null) capsuleExtra = GetComponent<CapsuleCollider>();
        if (sincronizarCapsuleCollider && capsuleExtra != null)
        {
            capsuleExtra.direction = 1; // eje Y
            capsuleExtra.radius = controller.radius;
            capsuleExtra.height = controller.height;
            capsuleExtra.center = controller.center;
        }

        // 5) Si había quedado "inactivo" por un error anterior, lo reiniciamos
        if (reiniciarController && Application.isPlaying && controller.enabled)
        {
            controller.enabled = false;
            controller.enabled = true;
        }
    }

    void AvisarSiHayEscala()
    {
        Vector3 s = transform.lossyScale;
        if (Mathf.Abs(s.x - 1f) > 0.01f || Mathf.Abs(s.y - 1f) > 0.01f || Mathf.Abs(s.z - 1f) > 0.01f)
        {
            Debug.LogWarning("[Player_Movement] El Player tiene escala " + s +
                             ". El tamaño final = medidas de la cápsula × escala. " +
                             "Recomendado: escala (1,1,1) y cambiar el tamaño con 'Altura Jugador' / 'Radio Jugador'.", this);
        }
    }

    // =====================================================================
    //  MOVIMIENTO
    // =====================================================================
    void MoverJugador()
    {
        float dt = Time.deltaTime;
        bool enSuelo = controller.isGrounded;

        // --- Input (raw: el suavizado lo hacemos nosotros) ---
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f); // las diagonales no son más rápidas
        bool hayInput = input.sqrMagnitude > 0.01f;

        bool correr = Input.GetKey(KeyCode.LeftShift) && hayInput && (!soloCorrerAdelante || input.y > 0.1f);
        float velocidadBase = correr ? runSpeed : walkSpeed;

        Vector2 objetivo = new Vector2(
            input.x * velocidadBase * multiplicadorLateral,
            input.y * velocidadBase * (input.y < 0f ? multiplicadorRetroceso : 1f));

        // --- Aceleración / desaceleración ---
        float tasa = hayInput ? aceleracion : desaceleracion;
        if (!enSuelo) tasa *= controlEnAire;
        velocidadLocal = Vector2.MoveTowards(velocidadLocal, objetivo, tasa * dt);

        // --- Gravedad ---
        if (enSuelo)
        {
            // Aterrizaje: la velocidad vertical todavía guarda la caída antes de resetearla
            if (!estabaEnSuelo && velocidadVertical.y < -velocidadMinimaAterrizaje && Time.timeSinceLevelLoad > 0.5f)
                ReproducirAterrizaje();

            if (velocidadVertical.y < 0f)
                velocidadVertical.y = -fuerzaPegadoSuelo;
        }
        else
        {
            velocidadVertical.y += gravedad * gravityMultiplier * dt;
            velocidadVertical.y = Mathf.Max(velocidadVertical.y, -velocidadTerminal);
        }
        estabaEnSuelo = enSuelo;

        // --- Un solo Move por frame (isGrounded queda más estable) ---
        Vector3 movimiento = transform.right * velocidadLocal.x + transform.forward * velocidadLocal.y;
        controller.Move((movimiento + velocidadVertical) * dt);

        // --- Pasos y cámara, usando la velocidad REAL (si chocás con una pared no suenan pasos) ---
        Vector3 velReal = controller.velocity;
        velReal.y = 0f;
        ActualizarPasosYCamara(velReal.magnitude, controller.isGrounded, dt);
    }

    // =====================================================================
    //  PASOS + HEAD BOB
    // =====================================================================
    void ActualizarPasosYCamara(float velHoriz, bool enSuelo, float dt)
    {
        bool caminando = enSuelo && velHoriz > velocidadMinimaPasos;
        float factorCorrer = Mathf.InverseLerp(walkSpeed, runSpeed, velHoriz);

        if (caminando)
        {
            // La distancia entre pasos crece al correr
            float longitud = Mathf.Max(0.1f, Mathf.Lerp(longitudPasoCaminar, longitudPasoCorrer, factorCorrer));
            pasosRecorridos += velHoriz * dt / longitud;

            int pasoActual = Mathf.FloorToInt(pasosRecorridos);
            if (pasoActual > ultimoPasoSonado)
            {
                ultimoPasoSonado = pasoActual;
                if (modoPasos == ModoPasos.PasoIndividual)
                    ReproducirPaso(factorCorrer);
            }
        }

        if (modoPasos == ModoPasos.LoopContinuo)
            ActualizarLoopPasos(caminando, factorCorrer);

        // Head bob sincronizado con los pasos: la cabeza baja justo cuando el pie toca el suelo
        if (usarHeadBob && camaraJugador != null)
        {
            intensidadBob = Mathf.MoveTowards(intensidadBob, caminando ? 1f : 0f, dt * 5f);
            float mult = intensidadBob * Mathf.Lerp(1f, multiplicadorBobCorrer, factorCorrer);
            float fase = pasosRecorridos * Mathf.PI * 2f;

            Vector3 offset = new Vector3(
                Mathf.Sin(fase * 0.5f) * amplitudBobLateral * mult,   // balanceo de lado a lado (1 ciclo cada 2 pasos)
                -Mathf.Cos(fase) * amplitudBobVertical * mult,        // sube y baja (1 ciclo por paso)
                0f);

            camaraJugador.localPosition = camaraPosBase + offset;
        }
    }

    void ReproducirPaso(float factorCorrer)
    {
        if (audioPasos == null || sonidosPasos == null || sonidosPasos.Length == 0) return;

        int indice = Random.Range(0, sonidosPasos.Length);
        if (sonidosPasos.Length > 1 && indice == ultimoClipPaso)
            indice = (indice + 1 + Random.Range(0, sonidosPasos.Length - 1)) % sonidosPasos.Length;
        ultimoClipPaso = indice;

        AudioClip clip = sonidosPasos[indice];
        if (clip == null) return;

        audioPasos.pitch = Random.Range(pitchMinimo, pitchMaximo);
        audioPasos.PlayOneShot(clip, Mathf.Lerp(volumenCaminar, volumenCorrer, factorCorrer));
    }

    void ReproducirAterrizaje()
    {
        if (sonidoAterrizaje == null) return;

        // Independiente del AudioSource de pasos (que puede estar en fundido o con otro pitch)
        AudioSource.PlayClipAtPoint(sonidoAterrizaje, transform.position, volumenAterrizaje);
    }

    /// <summary>
    /// Modo LoopContinuo: el clip se repite mientras caminás y, al detenerte, el volumen baja
    /// en 'fundidoSalida' segundos y recién ahí se frena (sin corte seco).
    /// </summary>
    void ActualizarLoopPasos(bool caminando, float factorCorrer)
    {
        if (audioPasos == null) return;

        float dt = Time.unscaledDeltaTime;

        if (caminando)
        {
            AudioClip clip = loopCaminata;
            if (clip == null && sonidosPasos != null && sonidosPasos.Length > 0)
                clip = sonidosPasos[0];
            if (clip == null) return;

            if (!audioPasos.isPlaying)
            {
                audioPasos.clip = clip;
                audioPasos.loop = true;
                audioPasos.volume = 0f;
                audioPasos.Play();
            }

            float volumenObjetivo = Mathf.Lerp(volumenCaminar, volumenCorrer, factorCorrer);
            audioPasos.volume = Mathf.MoveTowards(audioPasos.volume, volumenObjetivo, dt / Mathf.Max(0.01f, fundidoEntrada));
            audioPasos.pitch = Mathf.Lerp(1f, pitchLoopCorrer, factorCorrer);
        }
        else if (audioPasos.isPlaying && audioPasos.loop)
        {
            audioPasos.volume = Mathf.MoveTowards(audioPasos.volume, 0f, dt / Mathf.Max(0.01f, fundidoSalida));
            if (audioPasos.volume <= 0.001f)
                audioPasos.Stop();
        }
    }

    // =====================================================================
    //  REINICIO (sin cambios de lógica)
    // =====================================================================
    void HandleReinicio()
    {

        // Si tenés permiso de entrevista y todavía no la completaste,
        // dejamos que NPC_Entrevistado use la R.
        var camara = Manager_Camara.Instance;
        /* if (camara != null && camara.permisoObtenido && !camara.entrevistaCompletada)
         {
             // Estamos en "modo entrevista disponible" -> no interceptar R simple
             // Sí permitir Ctrl+R forzado
             if (permitirCtrlRInstantaneo && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(teclaReiniciar))
             {
                 ReiniciarJuego();
             }
             return;
         }*/

        // 3. Input reinicio
        if (permitirCtrlRInstantaneo && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(teclaReiniciar))
        {
            ReiniciarJuego();
            return;
        }

        if (!requiereMantener)
        {
            if (Input.GetKeyDown(teclaReiniciar))
                ReiniciarJuego();
            return;
        }

        // Mantener presionado
        if (Input.GetKey(teclaReiniciar))
        {
            holdTimer += Time.unscaledDeltaTime;
            if (!mostrandoCuenta && holdTimer > 0.3f)
            {
                mostrandoCuenta = true;
                Debug.Log("[Reinicio] Mantené R para reiniciar...");
            }
            if (holdTimer >= tiempoMantener)
            {
                ReiniciarJuego();
            }
        }
        else
        {
            if (holdTimer > 0.3f)
                Debug.Log("[Reinicio] Cancelado");
            holdTimer = 0f;
            mostrandoCuenta = false;
        }

        // feedback opcional en consola cada 0.4s
        if (mostrandoCuenta && Time.frameCount % 24 == 0)
        {
            float restante = Mathf.Max(0f, tiempoMantener - holdTimer);
            Debug.Log($"[Reinicio] Soltá para cancelar... {restante:F1}s");
        }
    }

    void ReiniciarJuego()
    {
        Debug.LogWarning("[Player_Movement] REINICIANDO PARTIDA...");

        // 1. Resetear ScriptableObjects persistentes
        var progress = FindObjectOfType<Manager_Interaccion>()?.playerProgress;
        if (progress != null) progress.Resetear();

        var kanaInv = FindObjectOfType<Manager_Tienda>()?.kanaInventario;
        if (kanaInv != null) kanaInv.Reiniciar();

        var journalData = FindObjectOfType<Manager_Journal>()?.journalData;
        if (journalData != null) journalData.LimpiarTodo();

        var cam = Manager_Camara.Instance;
        if (cam != null) cam.Resetear();

        // 2. Resetear NPCs completados (por reflexión, porque 'completado' es private)
        var npcs = FindObjectsOfType<Interaccion_NPC>();
        foreach (var npc in npcs)
        {
            // invocar MarcarCompletado(false) no existe, así que usamos reflexión para limpiar el flag
            var f = typeof(Interaccion_NPC).GetField("completado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) f.SetValue(npc, false);
            npc.ActualizarIndicador();
        }

        holdTimer = 0f;


        Scene escena = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escena.buildIndex);
    }

    // Acceso rápido desde UI de debug
    [ContextMenu("Reiniciar ahora")]
    void ReiniciarDesdeInspector() => ReiniciarJuego();

    [ContextMenu("Aplicar medidas al Character Controller")]
    void AplicarMedidasDesdeInspector() => AplicarMedidas(false);
}