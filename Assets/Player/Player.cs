﻿using UnityEngine;
using TMPro;
using System.Collections.Generic;
using JetBrains.Annotations;
using System;
using System.Linq;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    // Позиция игрока на карте и последняя дружественная позиция
    public MapRegion position;
    public MapRegion startPosition;

    // Массивы с классами юнитов разных родов войск
    private readonly string[] _infarnyUnitTypes = { "Новобранцы", "Пехота" };
    private readonly string[] _cavarlyUnitTypes = { "Кавалерия" };
    private readonly string[] _rangedUnitTypes = { "Дальний бой" };

    // Свойства для хранения валюты
    public int Piety { get; private set; }
    public int Gold { get; private set; }

    // Списки для хранения количества войск
    public const int MaxArmySize = 20;
    public int _recruitsCount { get; private set; } = 0;
    private List<MeleeSoldier> _infantryUnits = new List<MeleeSoldier>();
    public List<MeleeSoldier> infantryOutside{ get => _infantryUnits.ToList(); }

    private List<MeleeSoldier> _cavalryUnits = new List<MeleeSoldier>();
    public List<MeleeSoldier> cavalryOutside { get => _cavalryUnits.ToList(); }

    private List<DistantSoldier> _rangedUnits = new List<DistantSoldier>();
    public List<DistantSoldier> rangedUnitsOutside { get => _rangedUnits.ToList(); }

    public int ArmySize
    {
        get => _recruitsCount + _infantryUnits.Count +
               _cavalryUnits.Count + _rangedUnits.Count;
    }

    // Ссылки на текст для отображения в интерфейсе
    public TextMeshProUGUI pietyText;
    public TextMeshProUGUI goldText;

    // Метод для установки начальных значений
    public void Initialize(int startingPiety, int startingGold)
    {
        Piety = startingPiety;
        Gold = startingGold;

        position = startPosition;
    }

    // Метод для установки начальных значений из сохранения
    public void InitializeFromSave(ProgressData data)
    {
        Gold = data.Gold;
        Piety = data.Piety;
        position = GameObject.Find(data.Position).GetComponent<MapRegion>();
        _recruitsCount = data.RecruitsData;

        _infantryUnits = data.InfantryUnitsData.Select(x => UnitsInit.InitInfantrySoldiers(x))
            .ToList();
        _cavalryUnits = data.CavalryUnitsData.Select(x => UnitsInit.InitCavalrySoldiers(x))
            .ToList();
        _rangedUnits = data.DistantUnitsData.Select(x => UnitsInit.InitCrossbowSoldiers(x))
            .ToList();
    }

    private void InitializeUnitDictionary(Dictionary<string, int> unitDict, string[] unitTypes)
    {
        foreach (var unitType in unitTypes) unitDict.Add(unitType, 0);
    }

    // Метод для изменения благочестия
    public void ModifyPiety(int amount)
    {
        Piety += amount;
    }

    // Метод для изменения золота
    public void ModifyGold(int amount)
    {
        Gold += amount;
    }

    // Метод для изменения количества войск
    // Методы для найма и улучшения войск
    public bool CanHireRecruits(int amount)
    {
        return ArmySize + amount <= MaxArmySize;
    }

    public void HireRecruits(int amount)
    {
        if (!CanHireRecruits(amount))
        {
            throw new Exception("Вы не можете нанимать новобранцев сверх лимита");
        }

        _recruitsCount += amount;
    }

    public bool CanTrainRecruits()
    {
        return _recruitsCount > 0;
    }

    public void TrainRecruit(string unitType)
    {
        if (!CanTrainRecruits())
        {
            Debug.LogWarning("Нет новобранцев для обучения");
        }

        _recruitsCount--;

        switch (unitType)
        {
            case "Пехота":
                _infantryUnits.Add(UnitsInit.InitInfantrySoldiers());
                break;
            case "Кавалерия":
                _cavalryUnits.Add(UnitsInit.InitCavalrySoldiers());
                break;
            case "Дальний бой":
                _rangedUnits.Add(UnitsInit.InitCrossbowSoldiers());
                break;
        }
    }

    // Метод для обновления отображения в интерфейсе
    private void UpdateUI()
    {
        pietyText.text = Piety.ToString();
        goldText.text = Gold.ToString();
        UpdateRecruitsText();
        UpdateUnitText(_infantryUnits, "Пехота");
        UpdateUnitText(_cavalryUnits, "Кавалерия");
        UpdateUnitText(_rangedUnits, "Дальний бой");
    }

    private void UpdateRecruitsText()
    {
        var unitTextMesh = GameObject.Find($"Text (Новобранцы)").GetComponent<TextMeshProUGUI>();
        if (unitTextMesh != null) 
            unitTextMesh.text = (_recruitsCount * 250).ToString();
    }
    
    private void UpdateUnitText<T>(List<T> Units, string unitType) where T: ISoldier
    {
        var unitTextMesh = GameObject.Find($"Text ({unitType})").GetComponent<TextMeshProUGUI>();
        if (unitTextMesh != null)
        {
            var soldiersNumber = Units.Select(x => x.NumberOfPeople).Sum();
            var soldiersMaxNumber = Units.Select(x => x.MaxPeopleNumber).Sum();
            unitTextMesh.text = soldiersNumber.ToString() + "/" + soldiersMaxNumber.ToString();
        }
    }

    private void Update()
    {
        if (position != null)
        {
            transform.position = position.Position;
        }

        UpdateUI();
        SaveTest();
    }

    public void SaveTest()
    {
        if (UnityEngine.Input.GetKeyUp(KeyCode.J))
        {
            HireRecruits(4);
            TrainRecruit("Пехота");
            TrainRecruit("Пехота");
            TrainRecruit("Кавалерия");
            TrainRecruit("Дальний бой");
            Gold = 49992;
            Piety = 38;
        }
    }
}