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

    public enum TipoMinijuego { Minijuego1, Minijuego2 }
    public TipoMinijuego tipoMinijuego;

    public bool reintentarMinijuego;

    public bool daCamara;           // activa la cámara al terminar este nodo
    public bool entregaEntrevista;
    public bool daPermiso;
}