// ResearchAnalytics.cs - Sistema de logs para investigación
// Pon en: Assets/_CharacterEditor/Scripts/Research/ResearchAnalytics.cs
// No necesita estar en escena, se auto-crea y persiste entre escenas (DontDestroyOnLoad)

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResearchAnalytics : MonoBehaviour
{
    public static ResearchAnalytics Instance { get; private set; }

    [Header("Config")]
    public bool logToConsole = true;
    public bool autoSaveOnQuit = true;
    public string folderName = "research_logs";

    // Datos de sesión
    private DateTime sessionStartTime; // Cuando se instanció por primera vez
    private DateTime editorOpenTime;
    private DateTime editorCloseTime;
    private DateTime gameplayScreenOnTime;
    private DateTime firstInputTime;

    private int totalEditorInteractions = 0;
    private List<string> editorInteractionLogs = new List<string>();
    private string firstInputType = "";

    private bool hasLoggedEditorOpen = false;
    private bool hasLoggedEditorClose = false;
    private bool hasLoggedGameplayOn = false;
    private bool hasLoggedFirstInput = false;

    private float realtimeSessionStart;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sessionStartTime = DateTime.UtcNow;
        realtimeSessionStart = Time.realtimeSinceStartup;
        Debug.Log($"[Research] Sesión iniciada: {sessionStartTime:O}");
    }

    // ========== API PÚBLICA ==========

    public static void LogEditorOpen()
    {
        if (Instance == null) CreateInstance();
        if (Instance.hasLoggedEditorOpen) return; // evita duplicados si recarga escena

        Instance.editorOpenTime = DateTime.UtcNow;
        Instance.hasLoggedEditorOpen = true;
        Instance.totalEditorInteractions = 0;
        Instance.editorInteractionLogs.Clear();

        string log = $"[EDITOR_OPEN] {Instance.editorOpenTime:yyyy-MM-dd HH:mm:ss.fff} | UTC:{Instance.editorOpenTime:O} | elapsed:{Instance.GetElapsed():F3}s";
        Instance.LogInternal(log);
    }

    public static void LogEditorInteraction(string category, string itemId)
    {
        if (Instance == null) CreateInstance();
        Instance.totalEditorInteractions++;

        DateTime now = DateTime.UtcNow;
        double elapsed = Instance.GetElapsed();
        double sinceEditorOpen = (now - Instance.editorOpenTime).TotalSeconds;

        string log = $"[EDITOR_INTERACTION] {now:yyyy-MM-dd HH:mm:ss.fff} | +{sinceEditorOpen:F3}s desde EDITOR_OPEN | total:{Instance.totalEditorInteractions} | category:{category} | item:{itemId} | elapsedSession:{elapsed:F3}s";
        Instance.editorInteractionLogs.Add(log);
        Instance.LogInternal(log);
    }

    public static void LogEditorClose()
    {
        if (Instance == null) CreateInstance();
        if (Instance.hasLoggedEditorClose) return;

        Instance.editorCloseTime = DateTime.UtcNow;
        Instance.hasLoggedEditorClose = true;

        double duration = (Instance.editorCloseTime - Instance.editorOpenTime).TotalSeconds;
        double elapsed = Instance.GetElapsed();

        string log = $"[EDITOR_CLOSE] {Instance.editorCloseTime:yyyy-MM-dd HH:mm:ss.fff} | UTC:{Instance.editorCloseTime:O} | duration:{duration:F3}s | totalInteractions:{Instance.totalEditorInteractions} | elapsedSession:{elapsed:F3}s";
        Instance.LogInternal(log);
    }

    public static void LogGameplayScreenOn()
    {
        if (Instance == null) CreateInstance();
        if (Instance.hasLoggedGameplayOn) return;

        Instance.gameplayScreenOnTime = DateTime.UtcNow;
        Instance.hasLoggedGameplayOn = true;

        double elapsed = Instance.GetElapsed();
        double sinceEditorClose = Instance.hasLoggedEditorClose ? (Instance.gameplayScreenOnTime - Instance.editorCloseTime).TotalSeconds : -1;

        string log = $"[GAMEPLAY_SCREEN_ON] {Instance.gameplayScreenOnTime:yyyy-MM-dd HH:mm:ss.fff} | UTC:{Instance.gameplayScreenOnTime:O} | elapsedSession:{elapsed:F3}s | sinceEditorClose:{(sinceEditorClose >= 0 ? sinceEditorClose.ToString("F3") + "s" : "N/A")}";
        Instance.LogInternal(log);
    }

    public static void LogFirstInput(string inputType)
    {
        if (Instance == null) CreateInstance();
        if (Instance.hasLoggedFirstInput) return; // Solo el primer input

        Instance.firstInputTime = DateTime.UtcNow;
        Instance.firstInputType = inputType;
        Instance.hasLoggedFirstInput = true;

        double elapsed = Instance.GetElapsed();
        double sinceGameplay = Instance.hasLoggedGameplayOn ? (Instance.firstInputTime - Instance.gameplayScreenOnTime).TotalSeconds : -1;

        string log = $"[FIRST_INPUT] {Instance.firstInputTime:yyyy-MM-dd HH:mm:ss.fff} | UTC:{Instance.firstInputTime:O} | type:{inputType} | elapsedSession:{elapsed:F3}s | sinceGameplayOn:{(sinceGameplay >= 0 ? sinceGameplay.ToString("F3") + "s" : "N/A")}";
        Instance.LogInternal(log);

        // Opcional: guarda automáticamente cuando ocurre el primer input si querés
        // Instance.SaveRunToFile();
    }

    public static void SaveRunToFile()
    {
        if (Instance == null) return;
        Instance.SaveInternal();
    }

    public static void ResetSession()
    {
        if (Instance == null) return;
        Instance.ResetInternal();
    }

    // ========== INTERNOS ==========

    private void LogInternal(string message)
    {
        if (logToConsole) Debug.Log(message);
    }

    private double GetElapsed()
    {
        return Time.realtimeSinceStartup - realtimeSessionStart;
    }

    private void SaveInternal()
    {
        try
        {
            string folderPath = Path.Combine(Application.persistentDataPath, folderName);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = $"run_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{SystemInfo.deviceUniqueIdentifier.Substring(0, 6)}.txt";
            string filePath = Path.Combine(folderPath, fileName);

            List<string> lines = new List<string>();
            lines.Add($"=== RESEARCH RUN LOG ===");
            lines.Add($"SESSION_ID: {sessionStartTime:yyyyMMdd_HHmmss}");
            lines.Add($"SESSION_START: {sessionStartTime:O}");
            lines.Add($"DEVICE: {SystemInfo.deviceModel} | {SystemInfo.operatingSystem}");
            lines.Add($"BUILD_PATH: {folderPath}");
            lines.Add($"----------------------------------------");
            lines.Add($"EDITOR_OPEN: {(hasLoggedEditorOpen ? editorOpenTime.ToString("O") : "NOT_LOGGED")}");

            if (editorInteractionLogs.Count > 0)
            {
                lines.Add($"--- EDITOR_INTERACTIONS ({totalEditorInteractions}) ---");
                lines.AddRange(editorInteractionLogs);
            }
            else
            {
                lines.Add($"EDITOR_INTERACTIONS: 0 (none logged)");
            }

            if (hasLoggedEditorClose)
            {
                double editorDuration = (editorCloseTime - editorOpenTime).TotalSeconds;
                lines.Add($"EDITOR_CLOSE: {editorCloseTime:O} | duration:{editorDuration:F3}s | interactions:{totalEditorInteractions}");
            }
            else
            {
                lines.Add($"EDITOR_CLOSE: NOT_LOGGED");
            }

            lines.Add($"GAMEPLAY_SCREEN_ON: {(hasLoggedGameplayOn ? gameplayScreenOnTime.ToString("O") : "NOT_LOGGED")}");
            lines.Add($"FIRST_INPUT: {(hasLoggedFirstInput ? $"{firstInputTime:O} | type:{firstInputType}" : "NOT_LOGGED")}");

            lines.Add($"----------------------------------------");
            lines.Add($"SUMMARY:");
            if (hasLoggedEditorOpen && hasLoggedEditorClose)
            {
                double ed = (editorCloseTime - editorOpenTime).TotalSeconds;
                lines.Add($"editorDuration_seconds={ed:F3}");
            }
            lines.Add($"editorTotalInteractions={totalEditorInteractions}");
            if (hasLoggedGameplayOn && hasLoggedFirstInput)
            {
                double ttf = (firstInputTime - gameplayScreenOnTime).TotalSeconds;
                lines.Add($"timeToFirstInput_seconds={ttf:F3}");
            }
            if (hasLoggedEditorOpen && hasLoggedGameplayOn)
            {
                double totalToGameplay = (gameplayScreenOnTime - editorOpenTime).TotalSeconds;
                lines.Add($"editorOpen_to_gameplay_seconds={totalToGameplay:F3}");
            }
            lines.Add($"fileGenerated_at={DateTime.UtcNow:O}");
            lines.Add($"=== END LOG ===");

            File.WriteAllLines(filePath, lines);
            Debug.Log($"[Research] Archivo guardado en: {filePath}");

            // También guarda un master log que acumula todas las runs
            string masterPath = Path.Combine(folderPath, "_master_log.csv");
            bool masterExists = File.Exists(masterPath);
            using (StreamWriter sw = new StreamWriter(masterPath, true))
            {
                if (!masterExists)
                {
                    sw.WriteLine("session_id,editor_open,editor_close,editor_duration_s,editor_interactions,gameplay_on,first_input,first_input_type,time_to_first_input_s,editor_to_gameplay_s");
                }
                string editorOpenStr = hasLoggedEditorOpen ? editorOpenTime.ToString("O") : "";
                string editorCloseStr = hasLoggedEditorClose ? editorCloseTime.ToString("O") : "";
                double edDur = (hasLoggedEditorOpen && hasLoggedEditorClose) ? (editorCloseTime - editorOpenTime).TotalSeconds : 0;
                string gameplayStr = hasLoggedGameplayOn ? gameplayScreenOnTime.ToString("O") : "";
                string firstInputStr = hasLoggedFirstInput ? firstInputTime.ToString("O") : "";
                double ttf = (hasLoggedGameplayOn && hasLoggedFirstInput) ? (firstInputTime - gameplayScreenOnTime).TotalSeconds : 0;
                double eToG = (hasLoggedEditorOpen && hasLoggedGameplayOn) ? (gameplayScreenOnTime - editorOpenTime).TotalSeconds : 0;

                sw.WriteLine($"{sessionStartTime:yyyyMMdd_HHmmss},{editorOpenStr},{editorCloseStr},{edDur:F3},{totalEditorInteractions},{gameplayStr},{firstInputStr},{firstInputType},{ttf:F3},{eToG:F3}");
            }

        }
        catch (Exception e)
        {
            Debug.LogError($"[Research] Error guardando archivo: {e}");
        }
    }

    private void ResetInternal()
    {
        hasLoggedEditorOpen = false;
        hasLoggedEditorClose = false;
        hasLoggedGameplayOn = false;
        hasLoggedFirstInput = false;
        totalEditorInteractions = 0;
        editorInteractionLogs.Clear();
        firstInputType = "";
        sessionStartTime = DateTime.UtcNow;
        realtimeSessionStart = Time.realtimeSinceStartup;
        Debug.Log("[Research] Sesión reseteada");
    }

    private static void CreateInstance()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("ResearchAnalytics");
        Instance = go.AddComponent<ResearchAnalytics>();
        DontDestroyOnLoad(go);
    }

    void OnApplicationQuit()
    {
        if (autoSaveOnQuit)
        {
            SaveInternal();
        }
    }

    // Para test en editor
    [ContextMenu("Save Now (Test)")]
    public void ContextSave() => SaveInternal();

    [ContextMenu("Print Path")]
    public void PrintPath()
    {
        string path = Path.Combine(Application.persistentDataPath, folderName);
        Debug.Log($"Ruta de logs: {path}");
        Application.OpenURL("file://" + path);
    }
}
