using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Heroes/Create New Hero")]
public class Hero : ScriptableObject
{
    [SerializeField] private int attack = 1;
    [SerializeField] private int defense = 1;
    [SerializeField] private int spellpower = 1;
    [SerializeField] private int knowledge = 1;
    [SerializeField] private float movementPerDay = 400;
    [SerializeField] private Sprite heroSprite = null;
    [SerializeField] public Unit starterUnit = null;
    [SerializeField] public int starterUnitCount = 5;

    public float MovementPerDay { get { return movementPerDay; } }
    public Sprite Sprite { get { return heroSprite; } }

    public static Hero CreateInstanceOf(Hero hero)
    {
        Hero instance = CreateInstance<Hero>();

        instance.name = hero.name + "-Instanced";
        instance.movementPerDay = hero.movementPerDay;
        instance.heroSprite = hero.heroSprite;
        instance.starterUnit = hero.starterUnit;
        instance.starterUnitCount = hero.starterUnitCount;
        instance.attack = hero.attack;
        instance.defense = hero.defense;
        instance.spellpower = hero.spellpower;
        instance.knowledge = hero.knowledge;

        return instance;
    }
}
