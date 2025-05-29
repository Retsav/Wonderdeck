using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardVisual : MonoBehaviour
{
    public string cardID;
    public PlayerType owner;
    public MeshRenderer cardMeshRenderer;
    public GameObject particleSystemGameObject;
    public Renderer faceRenderer;

    public void Dissolve()
    {
        for (int i = 0; i < faceRenderer.materials.Length; i++)
        {
            faceRenderer.materials[i].SetFloat("_Dissolve", 0f);
            DOVirtual.Float(0f, 1f, 1f, v => faceRenderer.materials[i].SetFloat("_Dissolve", v))
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
