// GameplayFirstInputTracker.cs - Detecta el primer WASD o clic
// Pon en: Assets/_CharacterEditor/Scripts/Research/GameplayFirstInputTracker.cs
// Pon este script en tu MainScene, en el mismo GameObject que tiene CharacterLoaderUI o en el Player

using UnityEngine;

public class GameplayFirstInputTracker : MonoBehaviour
{
    [Header("Config")]
    public bool detectWASD = true;
    public bool detectMouseClick = true;
    public bool detectAnyKey = false; // Si activas esto, detecta cualquier tecla, no solo WASD
    public bool detectGamepad = true;

    private bool hasDetected = false;

    void Update()
    {
        if (hasDetected) return;

        // WASD
        if (detectWASD)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                Trigger(Input.inputString != "" ? Input.inputString : GetWASDKey());
                return;
            }
        }

        // Mouse click
        if (detectMouseClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Trigger("MouseLeft");
                return;
            }
            if (Input.GetMouseButtonDown(1))
            {
                Trigger("MouseRight");
                return;
            }
        }

        // Cualquier tecla (útil para testing)
        if (detectAnyKey)
        {
            if (Input.anyKeyDown)
            {
                Trigger("AnyKey:" + Input.inputString);
                return;
            }
        }

        // Gamepad básico (opcional)
        if (detectGamepad)
        {
            // Detecta movimiento de joystick o botones
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.5f)
            {
                Trigger("GamepadStick");
                return;
            }
            if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1"))
            {
                Trigger("GamepadButton");
                return;
            }
        }

        // Nuevo Input System (si lo usás, descomenta)
        /*
        #if ENABLE_INPUT_SYSTEM
        // Aquí iría tu lógica del nuevo Input System si lo usás
        #endif
        */
    }

    private string GetWASDKey()
    {
        if (Input.GetKeyDown(KeyCode.W)) return "W";
        if (Input.GetKeyDown(KeyCode.A)) return "A";
        if (Input.GetKeyDown(KeyCode.S)) return "S";
        if (Input.GetKeyDown(KeyCode.D)) return "D";
        return "WASD";
    }

    private void Trigger(string inputType)
    {
        hasDetected = true;
        ResearchAnalytics.LogFirstInput(inputType);
        Debug.Log($"[Research] FIRST_INPUT detectado: {inputType}");
        // Desactiva este script para ahorrar performance
        enabled = false;
    }

    // Para poder resetear en tests
    public void ResetTracker()
    {
        hasDetected = false;
        enabled = true;
    }
}
