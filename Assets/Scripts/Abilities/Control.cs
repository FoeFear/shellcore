﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : PassiveAbility
{

    const float healthAddition = 200;
    const float damageAddition = 200;

    List<Entity> boosted = new List<Entity>();

    protected override void Awake()
    {
        base.Awake();
        ID = 34;
    }
    public override void SetDestroyed(bool input)
    {
        for (int i = 0; i < boosted.Count; i++)
        {
            var entity = boosted[i];
            var maxHealths = entity.GetMaxHealth();
            maxHealths[0] -= healthAddition * abilityTier;
            var healths = entity.GetHealth();
            healths[0] -= healthAddition * abilityTier;
            entity.damageAddition -= damageAddition;
        }
        Entity.OnEntitySpawn -= EntitySpawn;
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (!(AIData.entities[i] is Turret) && AIData.entities[i].faction == Core.faction && AIData.entities[i] != Core) // All drones or only ones owned by the player?
            {
                var entity = AIData.entities[i];
                var maxHealths = entity.GetMaxHealth();
                maxHealths[0] += healthAddition * abilityTier;
                var healths = entity.GetHealth();
                healths[0] += healthAddition * abilityTier;
                entity.damageAddition += damageAddition;
                boosted.Add(entity);
            }
        }
        Entity.OnEntitySpawn += EntitySpawn;
    }

    void EntitySpawn(Entity entity)
    {
        if (!(entity is Turret) && entity.faction == Core.faction && entity != Core)
        {
            var maxHealths = entity.GetMaxHealth();
            maxHealths[0] += healthAddition * abilityTier;
            var healths = entity.GetHealth();
            healths[0] += healthAddition * abilityTier;
            entity.damageAddition += damageAddition;
            boosted.Add(entity);
        }
    }

    private void OnDestroy()
    {
        if (!isDestroyed)
        {
            for (int i = 0; i < boosted.Count; i++)
            {
                var entity = boosted[i];
                if (!entity)
                    continue;
                var maxHealths = entity.GetMaxHealth();
                maxHealths[0] -= healthAddition * abilityTier;
                var healths = entity.GetHealth();
                healths[0] -= healthAddition * abilityTier;
                entity.damageAddition -= damageAddition;
            }

            Entity.OnEntitySpawn -= EntitySpawn;
        }
    }
}
