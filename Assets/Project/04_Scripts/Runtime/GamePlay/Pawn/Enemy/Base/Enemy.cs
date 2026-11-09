using UnityEngine;

public abstract class Enemy<TStats> : EnemyBase where TStats : SO_ConfigEnemy
{
    [SerializeField] private TStats stats;

    public TStats Stats => stats;
    public override SO_ConfigEnemy StatsBase => stats;
}