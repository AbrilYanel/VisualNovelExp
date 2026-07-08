

using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialogue/Node")]
public class Nodo_Dialogo : ScriptableObject
{
    public string speakerName;
    [TextArea] public string sentence;

    public bool hasChoices;
    public string option1Text;
    public string option2Text;
    public Nodo_Dialogo option1Next;
    public Nodo_Dialogo option2Next;

    public bool startsMinigame;
    public bool endsDialogue;
    public Nodo_Dialogo nextNode;

    public enum TipoMinijuego
    {
        // Legacy – mantenidos para no romper nodos viejos
        Minijuego1,   // = Emparejar
        Minijuego2,   // = FillBlank / WordOrder

        // Nuevos – Paso 5
        VerdaderoFalso,   // NPC1 – fila a
        Memorama,         // NPC2 – fila ka
        Emparejar,        // NPC3 – fila sa
        FillBlank,        // NPC4 – fila ta
        KanaRush,         // NPC5 – fila na
        EscrituraInversa, // NPC6 – fila ha
        WordOrder,        // NPC7 – fila ma
        QuizCortesia,     // NPC8 – fila ya
        Entrevista        // NPC9 – fila ra
    }
    public TipoMinijuego tipoMinijuego;

    public bool reintentarMinijuego;

    // Eventos misión
    public bool daCamara;
    public bool entregaEntrevista;
    public bool daPermiso;
}
