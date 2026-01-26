using Assets.Scripts.Interactions.Abstract;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[SelectionBase]
public abstract class TriggerInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public KeyActiveType keyType { get; set; }
    [SerializeField] private TMP_Text keyView;
    [SerializeField] private Transform parent;
    private HintCameraOrbit hintCameraOrbit;
    private Regex Regex = new Regex(@"[a-zA-Z]{1,}");

    [Inject]
    public void Construct(HintCameraOrbit hintCameraOrbit)
    {
        this.hintCameraOrbit = hintCameraOrbit;
    }

    public virtual void Active(InputBinding input)
    {
        var keyName = Regex.Matches(input.path);

        keyView.gameObject.SetActive(true);
        keyView.text = $"PRESS {keyName[1]} {keyType}";
    }

    public virtual void Deactive()
    {
        keyView.gameObject.SetActive(false);
    }

    public virtual void Interact()
    {
    }
}
