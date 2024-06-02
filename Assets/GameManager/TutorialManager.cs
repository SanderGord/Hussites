using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep
{
    public string Instruction; // Текст инструкции
    public Action StepAction; // Действие, которое выполняется на этом шаге
    public Func<bool> CompletionCondition; // Условие завершения шага
}

public class Tutorial : MonoBehaviour
{
    private List<TutorialStep> steps;
    private bool isEnded;

    [SerializeField] private List<MapRegion> regionsToVisit;
    [SerializeField] private FightWindow fightWindow;
    private Queue<MapRegion> visitQueue;

    private int previousInfantryCount;
    private int previousRecruitsCount;
    private int previousTurnsCount;

    [SerializeField] private GameManager gameManager;
    private int currentStepIndex;

    private void Start()
    {
        SetupTutorial();
        ShowCurrentStep();
    }

    private void SetupTutorial()
    {
        foreach (var el in regionsToVisit)
            visitQueue.Enqueue(el);
        previousRecruitsCount = gameManager.player.army.RecruitsCount;
        previousInfantryCount = gameManager.player.army.InfantryOutside.Count;
        previousTurnsCount = TurnManager.Instance.TurnsCount;

        steps = new List<TutorialStep>
        {
            // Первый шаг: Переместиться на первый указанный регион
            new()
            {
                Instruction = "Переместитесь на соседнюю провинцию",
                StepAction = () => Debug.Log("фокус и стрелка на регион"),
                CompletionCondition = PlayerMovedToNextRegion
            },
            // Второй шаг: Нанять новобранцев
            new()
            {
                Instruction = "Наймите своих первых новобранцев",
                StepAction = () => Debug.Log("стрелка на новобранцев"),
                CompletionCondition = PlayerHiredRecruits
            },
            // Третий шаг: Сдать ход
            new()
            {
                Instruction = "Чтобы пойти дальше, вам нужно завершить ход. Сделайте это",
                StepAction = () => Debug.Log("\"зажечь\" или переместить стрелочку на кнопку сдать ход"),
                CompletionCondition = PlayerEndedTurn
            },
            // Четвертый шаг: Переместиться на второй указанный регион
            new()
            {
                Instruction = "Двигайтесь в Прагу.",
                StepAction = () => Debug.Log("фокус и стрелка на регион"),
                CompletionCondition = PlayerMovedToNextRegion
            },
            // Пятый шаг: Улучшить новобранцев до пехоты
            new()
            {
                Instruction = "Улучшите новобранцев до пехоты.",
                StepAction = () => Debug.Log("стрелка на кнопку улучшить"),
                CompletionCondition = PlayerUpgradedRecruits
            },
            // Шестой шаг: Сдать ход
            new()
            {
                Instruction = "Сдайте ход.",
                StepAction = () => Debug.Log("\"зажечь\" стрелочку на кнопку сдать ход"),
                CompletionCondition = PlayerEndedTurn
            },
            // Седьмой шаг: Внезапное нападение
            new()
            {
                Instruction = "На вас нападают! После боя обучение будет завершено.",
                StepAction = () => Debug.Log("Вызвать FightWindow.ShowFightWindow"),
                CompletionCondition = () => true
            }
        };
    }

    private void ShowCurrentStep()
    {
        if (currentStepIndex < steps.Count)
        {
            // Показать инструкцию текущего шага
            DisplayInstruction(steps[currentStepIndex].Instruction);

            // Выполнить действие текущего шага
            steps[currentStepIndex].StepAction?.Invoke();
        }
        else
        {
            isEnded = true;
            // TODO: потушить окошко с инструкциями
        }
    }

    private void Update()
    {
        if (!isEnded && steps[currentStepIndex].CompletionCondition())
        {
            NextStep();
        }
    }

    private void NextStep()
    {
        currentStepIndex++;
        ShowCurrentStep();
    }

    private void DisplayInstruction(string instruction)
    {
        // TODO: логика отображения инструкции на экране
        Debug.Log(instruction);
    }

    private bool PlayerMovedToNextRegion()
    {
        if ((visitQueue.Peek().Position - gameManager.currentRegion.Position).magnitude > 0.1f)
            return false;
        visitQueue.Dequeue();
        return true;
    }

    private bool PlayerHiredRecruits()
    {
        if (previousRecruitsCount == gameManager.player.army.RecruitsCount)
            return false;
        previousRecruitsCount = gameManager.player.army.RecruitsCount;
        return true;
    }

    private bool PlayerEndedTurn()
    {
        if (previousTurnsCount == TurnManager.Instance.TurnsCount)
            return false;
        previousTurnsCount = TurnManager.Instance.TurnsCount;
        return true;
    }

    private bool PlayerUpgradedRecruits()
    {
        if (previousInfantryCount == gameManager.player.army.InfantryOutside.Count)
            return false;
        previousInfantryCount = gameManager.player.army.InfantryOutside.Count;
        return true;
    }
}