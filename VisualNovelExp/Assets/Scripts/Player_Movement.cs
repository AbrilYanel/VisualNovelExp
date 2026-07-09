using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Movement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float gravedad = -9.81f;
    public float gravityMultiplier = 2f;

    private Vector3 velocidadVertical;
    private CharacterController controller;
    private Vector3 moveDirection;

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        interaccionManager = FindObjectOfType<Manager_Interaccion>();
        tiendaManager = FindObjectOfType<Manager_Tienda>();
        journalManager = FindObjectOfType<Manager_Journal>();
    }

    void Update()
    {
        // --- Movimiento normal ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (controller.isGrounded)
        {
            velocidadVertical.y = -2f;
        }
        else
        {
            velocidadVertical.y += gravedad * gravityMultiplier * Time.deltaTime;
        }

        controller.Move(move * speed * Time.deltaTime);
        controller.Move(velocidadVertical * Time.deltaTime);

        // --- Reinicio ---
        HandleReinicio();
    }

    void HandleReinicio()
    {
       
        // Si tenés permiso de entrevista y todavía no la completaste,
        // dejamos que NPC_Entrevistado use la R.
        var camara = Manager_Camara.Instance;
        if (camara != null && camara.permisoObtenido && !camara.entrevistaCompletada)
        {
            // Estamos en "modo entrevista disponible" -> no interceptar R simple
            // Sí permitir Ctrl+R forzado
            if (permitirCtrlRInstantaneo && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(teclaReiniciar))
            {
                ReiniciarJuego();
            }
            return;
        }

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
}
