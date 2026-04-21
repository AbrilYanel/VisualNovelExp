using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaccion_NPC : MonoBehaviour
{
    public Manager_Interaccion dialogueManager;

    public Nodo_Dialogo startNode; 

    public void Interact()
    {
        dialogueManager.StartDialogue(startNode);
    }
}
