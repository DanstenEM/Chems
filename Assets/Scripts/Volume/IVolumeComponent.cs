using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public interface IVolumeComponent
{
    public float duration { get; set; }
    public Volume _volume { get; set; }
    public void Execute();
}
