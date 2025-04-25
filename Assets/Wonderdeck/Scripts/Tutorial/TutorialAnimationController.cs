using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;




    public void DrawCardAnimation()
    {
        animator.SetTrigger("Draw");
    }

    public void EndTurnAnimation()
    {
        animator.SetTrigger("Draw");
    }
}

