using Assets.Scripts.Interactions.Abstract;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
public abstract class TriggerInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public KeyActiveType keyType { get; set; }
    [SerializeField] private TMP_Text keyView;
    private Regex Regex = new Regex(@"[a-zA-Z]{1,}");

    public virtual void Active(InputBinding input)
    {
        var keyName = Regex.Matches(input.path);
        keyView.text = $"PRESS {keyName[1]} {keyType}";
    }

    public virtual void Deactive()
    {
    }

    public virtual void Interact()
    {
    }
}
