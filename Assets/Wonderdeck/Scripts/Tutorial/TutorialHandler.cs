using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private TutorialBlackjackLogic tutorialBlackjackLogic;
    [SerializeField] private List<DialogueScriptableObject> DialoguesToTutorialStep;

    private ITutorialService _tutorialService;

    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    private void Start()
    {
        _tutorialService.IsTutorial = true;
        _tutorialService.TutorialStepIncreased += IncreaseTutorialStep;
        StartCoroutine(DelayTutorialStart());
    }

    private void OnDestroy()
    {
        _tutorialService.TutorialStepIncreased -= IncreaseTutorialStep;
    }


    private IEnumerator DelayTutorialStart()
    {
        yield return new WaitForSeconds(1.5f);
        HandleDialogue();
        tutorialBlackjackLogic.OnTutorialStepConfirmed(_tutorialService.TutorialStep);
        _tutorialService.OnActiveTutorialStepChanged();
    }

    
    private void IncreaseTutorialStep(object sender, EventArgs e)
    {
        
        
        StartCoroutine(DelayNextStep());
    }

    private IEnumerator DelayNextStep()
    {
        yield return new WaitForSeconds(1f);
        _tutorialService.TutorialStep++;
        if (_tutorialService.TutorialStep == 23)
        {
            EndTutorial();
            yield break;
        }
        
        HandleDialogue();
        tutorialBlackjackLogic.OnTutorialStepConfirmed(_tutorialService.TutorialStep);
        _tutorialService.OnActiveTutorialStepChanged();
    }

    private void EndTutorial()
    {
        _tutorialService.IsTutorial = false;
        _tutorialService.TutorialStep = 0;
        _tutorialService.ArbiterCards.Clear();
        _tutorialService.FirstPlayerItems.Clear();
        _tutorialService.HasHiddenCard = false;
        _tutorialService.SecondPlayerItems.Clear();
        _tutorialService.FirstPlayerCards.Clear();
        SceneManager.LoadScene("Menu");
    }


    private void HandleDialogue()
    {
        DialogueScriptableObject foundDialogue = default;
        for (int i = 0; i < DialoguesToTutorialStep.Count; i++)
        {
            var dialogue = DialoguesToTutorialStep[i];
            if (dialogue.TutorialStepIndex != _tutorialService.TutorialStep)
                continue;
            foundDialogue = dialogue;
            break;
        }

        if (foundDialogue == null)
        {
            Debug.LogWarning($"Dialogue not found for index {_tutorialService.TutorialStep}");
            return;
        }
        
        _tutorialService.PlayDialogue(foundDialogue);
    }
}
