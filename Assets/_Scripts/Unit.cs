using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Heroes/Create New Unit")]
public class Unit : ScriptableObject
{
    [SerializeField] private Sprite unitSprite = null;
    [SerializeField] private Mesh unitMesh = null;
    [Header("Attributes")]
    [SerializeField] private int attack = 1;
    [SerializeField] private int defense = 1;
    [SerializeField] private int healthPoints = 10;
    [SerializeField] private int speed = 4;
    [SerializeField] private int initiative = 1;
    [SerializeField] private Vector2Int damageRange;

    public Sprite Sprite { get { return unitSprite; } }
    public Mesh Mesh { get { return unitMesh; } }

    // Attributes
    public int Attack { get { return attack; } }
    public int Defense { get { return defense; } }
    public int HP { get { return healthPoints; } }
    public int Speed { get { return speed; } }
    public int Initiative { get { return initiative; } }
    public Vector2Int DamageRange {get { return damageRange; } }



}
