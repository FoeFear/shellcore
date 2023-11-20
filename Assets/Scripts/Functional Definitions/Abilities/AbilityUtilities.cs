using System.Text;
using UnityEngine;

public enum AbilityID
{
    Unused,
    SpeedThrust,
    ShellBoost,
    MainBullet,
    Beam,
    Bullet,
    Cannon,
    Missile,
    Torpedo,
    Laser,
    SpawnDrone,
    CoreHeal,
    Energy,
    Speed,
    SiegeBullet,
    SpeederBullet,
    Harvester,
    ShellRegen,
    ShellMax,
    EnergyRegen,
    EnergyMax,
    Command,
    CoreRegen,
    CoreMax,
    Stealth,
    DamageBoost,
    AreaRestore,
    PinDown,
    Retreat,
    Absorb,
    ActiveShellRegen,
    ActiveCoreRegen,
    ActiveEnergyRegen,
    Disrupt,
    Control,
    InvertTractor,
    Bomb,
    Ion,
    Flak,
    Rocket,
    YardWarp,
    Unload,
    HealAura,
    SpeedAura,
    EnergyAura,
    ChainBeam,
    SpeederMissile,
    Radar
}

public static class AbilityUtilities
{
    public static Sprite GetAbilityImageByID(int ID, string secondaryData)
    {
        if (ID == 0)
        {
            return null;
        }

        if (ID == 10)
        {
            DroneSpawnData data = DroneUtilities.GetDroneSpawnDataByShorthand(secondaryData);
            return DroneUtilities.GetAbilitySpriteBySpawnData(data);
        }

        return ResourceManager.GetAsset<Sprite>("AbilitySprite" + ID);
    }

    public static Sprite GetAbilityImage(Ability ability)
    {
        var ID = ability.GetID();
        if (ID == 0)
        {
            return null;
        }

        if (ID == 10)
        {
            return DroneUtilities.GetAbilitySpriteBySpawnData((ability as SpawnDrone).spawnData);
        }

        return ResourceManager.GetAsset<Sprite>("AbilitySprite" + ID);
    }

    public static AbilityHandler.AbilityTypes GetAbilityTypeByID(int ID)
    {
        switch (ID)
        {
            case 10:
                return AbilityHandler.AbilityTypes.Spawns;
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 14:
            case 15:
            case 36:
            case 37:
            case 38:
            case 39:
            case 45:
            case 46:
                return AbilityHandler.AbilityTypes.Weapons;
            case 1:
            case 2:
            case 11:
            case 12:
            case 24:
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
            case 32:
            case 33:
            case 35:
            case 40:
            case 41:
            case 47:
                return AbilityHandler.AbilityTypes.Skills;
            case 13:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 34:
            case 42:
            case 43:
            case 44:
                return AbilityHandler.AbilityTypes.Passive;
            case 0:
            default:
                return AbilityHandler.AbilityTypes.None;
        }
    }

    public static string GetDescriptionByID(int ID, int tier, string secondaryData)
    {
        switch (ID)
        {
            case 0:
                return "Does nothing.";
            case 1:
                return $"+{SpeedThrust.boost * tier} speed for {SpeedThrust.duration} seconds.";
            case 2:
                return $"Instantly heal {HealthHeal.heals[0] * tier} shell.";
            case 3:
                return $"Projectile that deals {MainBullet.GetDamage(tier)} damage. \nStays with you no matter what.";
            case 4:
                return $"Instant attack that deals {Beam.beamDamage * tier} damage.";
            case 5:
                return $"Projectile that deals {Bullet.bulletDamage * tier} damage.";
            case 6:
                return $"Instant attack that deals {Cannon.cannonDamage * tier} damage.";
            case 7:
                return $"Slow homing projectile that deals {Missile.missileDamage * tier} damage.";
            case 8:
                return $"Slow projectile that deals {Torpedo.torpedoDamage * tier} damage to ground entities.";
            case 9:
                return $"Rapidly fires 5 projectiles that deal {Laser.laserDamage * tier} damage.";
            case 10:
                if (string.IsNullOrEmpty(secondaryData))
                {
                    return "Spawns a drone.";
                }
                DroneSpawnData data = DroneUtilities.GetDroneSpawnDataByShorthand(secondaryData);
                return DroneUtilities.GetDescriptionByType(data.type);
            case 11:
                return $"Instantly heal {HealthHeal.heals[1] * tier} core.";
            case 12:
                return $"Instantly heal {HealthHeal.heals[2] * tier} energy.";
            case 13:
                return $"+{Speed.boost * tier} speed.";
            case 14:
                return $"Long range projectile that deals {1000 * tier} damage.";
            case 15:
                return $"Projectile that deals {100 * tier} damage.";
            case 17:
                return $"Passively increases shell regen by {ShellRegen.regens[0] * tier} points.";
            case 18:
                return $"Passively increases maximum shell by {ShellMax.maxes[0] * tier} points.";
            case 19:
                return $"Passively increases energy regen by {ShellRegen.regens[2] * tier} points.";
            case 20:
                return $"Passively increases maximum energy by {ShellMax.maxes[2] * tier} points.";
            case 21:
                return $"Passively increases the maximum allowed number of controlled units by {Command.commandUnitIncrease * Mathf.Max(1, tier)}.";
            case 22:
                return $"Passively increases core regen by {ShellRegen.regens[1] * tier} points.";
            case 23:
                return $"Passively increases maximum core by {ShellMax.maxes[1] * tier} points.";
            case 24:
                return "Become invisible to enemies.";
            case 25:
                return $"All weapon damage increased by {Mathf.Round(DamageBoost.damageFactor * Mathf.Max(1, tier) * 100)}%.";
            case 26:
                return $"Instantly heals nearby allies by {AreaRestore.heal * Mathf.Max(1, tier)} shell.";
            case 27:
                return "Immobilizes the target.";
            case 28:
                return "Respawn at base.";
            case 29:
                return "Absorb damage and turn it into energy.";
            case 30:
                return $"Temporarily increase shell regen by {ActiveRegen.healAmounts[0] * tier} per second.";
            case 31:
                return $"Temporarily increase core regen by {ActiveRegen.healAmounts[1] * tier} per second.";
            case 32:
                return $"Temporarily increase energy regen by {ActiveRegen.healAmounts[2] * tier} per second.";
            case 33:
                return "Disrupt enemy ability cooldowns.";
            case 34:
                return $"Gives allies additional {Control.baseControlFractionBoost * 100 * tier}% shell and {Mathf.Round(Control.damageFactor * Mathf.Max(1, tier) * 100)}% weapon damage.";
            case 35:
                return "Temporarily pulls you to your tractor target and allows you to tractor most entities.";
            case 36:
                return $"Stationary projectile that deals {Bomb.bombDamage * tier} damage. \nProjectile lasts {5} seconds.";
            case 37:
                return $"Slow moving beam that deals {IonLineController.damageC * tier} damage per second for 5 seconds. "
                       + $"\nBeam costs {IonLineController.energyC * tier} energy per second.";
            case 38:
                return $"Fires at least 1 projectile at different targets that each deal {Flak.bulletDamage * tier} damage.\nCan fire at as many drones as are in range, and disables them for 3 seconds each.";
            case 39:
                return $"Slow projectile that deals {Rocket.bulletDamage * tier} to air stations.";
            case 40:
                return "Warps your currently held part directly into your inventory.";
            case 41:
                return $"Temporarily reduces Global Cooldown by a factor of {1}/{Mathf.Max(tier, 1)+1}.";
            case 45:
                return $"Instant attack that deals {Beam.beamDamage * tier} damage to multiple targets.";
            case 46:
                return $"Slow homing projectile that deals {Missile.missileDamage * tier} damage plus more if the target was moving.";
            case 47:
                return "Marks unknown sectors on the map, or a random sector if all have been found.";
            default:
                return "Description unset";
        }
    }

    public static string GetDescription(Ability ability)
    {
        switch (ability.GetID())
        {
            case 10:
                return DroneUtilities.GetDescriptionByType((ability as SpawnDrone).spawnData.type);
            default:
                return GetDescriptionByID(ability.GetID(), ability.GetTier(), "");
        }
    }

    public static string GetShooterByID(int ID, string data = null)
    {
        switch (ID)
        {
            case 0:
            case 13:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 34:
            case 42:
            case 43:
            case 44:
                return null;
            case 4:
            case 45:
                if (data == "beamgroundshooter_sprite")
                {
                    return "beamgroundshooter_sprite";
                }
                else if (data == "beam_station_shooter")
                {
                    return "beam_station_shooter";
                }
                else
                {
                    return "beamshooter_sprite";
                }
            case 5:
            case 15:
                return "bulletshooter_sprite";
            case 6:
                return "cannonshooter_sprite";
            case 7:
            case 46:
                if (data == "missile_station_shooter")
                {
                    return "missile_station_shooter";
                }
                else
                {
                    return "missileshooter_sprite";
                }
            case 8:
                return "torpedoshooter_sprite";
            case 9:
                return "lasershooter_sprite";
            case 38:
                return "flakshooter_sprite";
            case 36:
                return "bombshooter_sprite";
            case 14:
                if (data == "siegegroundshooter_sprite")
                {
                    return "siegegroundshooter_sprite";
                }
                else if (data == "bulletshooter_sprite")
                {
                    return "bulletshooter_sprite";
                }
                else
                {
                    return "siegeshooter_sprite";
                }
            case 2:
            case 30:
                return "ability_indicator_shell";
            case 11:
            case 12:
            case 31:
            case 32:
                return "ability_indicator_core";
            case 28:
                return "ability_indicator_retreat";
            case 25:
                return "ability_indicator_damage_boost";
            case 27:
                return "ability_indicator_pin_down";
            case 29:
                return "ability_indicator_absorb_field";
            case 39:
                return "rocketshooter_sprite";
            case 37:
                if (data == "ion_station_shooter")
                {
                    return "ion_station_shooter";
                }
                else
                {
                return "ionshooter_sprite";
                }
            case 40:
                return "ability_indicator_yard_warp";
            default:
                return "ability_indicator";
        }
    }

    public static string GetAbilityNameByID(int ID, string secondaryData)
    {
        switch (ID)
        {
            case 0:
                return "None";
            case 1:
                return "Speed Thrust";
            case 2:
                return "Shell Boost";
            case 3:
                return "Main Bullet";
            case 4:
                return "Beam";
            case 5:
                return "Bullet";
            case 6:
                return "Cannon";
            case 7:
                return "Missile";
            case 8:
                return "Torpedo";
            case 9:
                return "Laser";
            case 10:
                if (secondaryData == null)
                {
                    return "Spawn Drone";
                }

                DroneSpawnData data = DroneUtilities.GetDroneSpawnDataByShorthand(secondaryData);
                return DroneUtilities.GetAbilityNameByType(data.type);
            case 11:
                return "Core Heal";
            case 12:
                return "Energy";
            case 13:
                return "Speed";
            case 14:
                return "Siege Bullet";
            case 15:
                return "Speeder Bullet";
            case 16:
                return "Harvester";
            case 17:
                return "Shell Regen";
            case 18:
                return "Shell Max";
            case 19:
                return "Energy Regen";
            case 20:
                return "Energy Max";
            case 21:
                return "Command";
            case 22:
                return "Core Regen";
            case 23:
                return "Core Max";
            case 24:
                return "Stealth";
            case 25:
                return "Damage Boost";
            case 26:
                return "Area Restore";
            case 27:
                return "Pin Down";
            case 28:
                return "Retreat";
            case 29:
                return "Absorb Field";
            case 30:
                return "Shell Regen";
            case 31:
                return "Core Regen";
            case 32:
                return "Energy Regen";
            case 33:
                return "Disrupt";
            case 34:
                return "Control";
            case 35:
                return "Invert Tractor";
            case 36:
                return "Bomb";
            case 37:
                return "Ion";
            case 38:
                return "Flak";
            case 39:
                return "Rocket";
            case 40:
                return "Yard Warp";
            case 41:
                return "Unload";
            case 42:
                return "Heal Aura";
            case 43:
                return "Speed Aura";
            case 44:
                return "Energy Aura";
            case 45:
                return "Chain Beam";
            case 46:
                return "Speeder Missile";
            case 47:
                return "Radar";
            default:
                return "Name unset";
        }
    }

    public static string GetAbilityName(Ability ability)
    {
        switch (ability.GetID())
        {
            case 10:
                return DroneUtilities.GetAbilityNameByType((ability as SpawnDrone).spawnData.type);
            default:
                return GetAbilityNameByID(ability.GetID(), "");
        }
    }

    public static bool AbilityIsGasBoostable(int ID)
    {
        return GetAbilityTypeByID(ID) != AbilityHandler.AbilityTypes.Passive;
    }


    public static string GasBoostDescription(int ID)
    {
        var builder = new StringBuilder();
        builder.Append("GAS BOOSTED: ");
        switch ((AbilityID)ID)
        {
            case AbilityID.DamageBoost:
                builder.Append("Damage Boost applies a flat damage increase of 200 to projectiles.");
                break;
            case AbilityID.Bullet:
                builder.Append("Bullet pierces through struck enemies once.");
                break;
            case AbilityID.Cannon:
                builder.Append("Weapon deals 250% more damage 10% of the time.");
                break;
            case AbilityID.Ion:
                builder.Append("Weapon adjustment speed doubled.");
                break;
            case AbilityID.Flak:
                builder.Append("Projectiles temporarily disable drones.");
                break;
            case AbilityID.Stealth:
                builder.Append("Enables regeneration while stealthed.");
                break;
            case AbilityID.Beam:
                builder.Append("Weapon strikes 2 more times from where it last struck.");
                break;
            default:
                builder.Append("Ability cooldown lowered by 25%.");
                break;
        }
        return builder.ToString();
    }


    public static Ability AddAbilityToGameObjectByID(GameObject obj, int ID, string data = null, int tier = 0)
    {
        Ability ability = null;
        switch (ID)
        {
            case 1:
                ability = obj.AddComponent<SpeedThrust>();
                break;
            case 2:
                ability = obj.AddComponent<HealthHeal>();
                ((HealthHeal)ability).type = HealthHeal.HealingType.shell;
                ((HealthHeal)ability).Initialize();
                break;
            case 3:
                Debug.LogWarning("Main bullet should be intrinsically added!");
                ability = obj.AddComponent<MainBullet>();
                break;
            case 4:
                ability = obj.AddComponent<Beam>();
                break;
            case 5:
                ability = obj.AddComponent<Bullet>();
                break;
            case 6:
                ability = obj.AddComponent<Cannon>();
                break;
            case 7:
                ability = obj.AddComponent<Missile>();
                if (data == "missile_station_shooter")
                {
                    ((Missile)ability).category = Entity.EntityCategory.All;
                    ((Missile)ability).terrain = Entity.TerrainType.All;
                }

                break;
            case 8:
                ability = obj.AddComponent<Torpedo>();
                break;
            case 9:
                ability = obj.AddComponent<Laser>();
                break;
            case 10:
                ability = obj.AddComponent<SpawnDrone>();
                ((SpawnDrone)ability).spawnData = DroneUtilities.GetDroneSpawnDataByShorthand(data);
                ((SpawnDrone)ability).Init();
                break;
            case 11:
                ability = obj.AddComponent<HealthHeal>();
                ((HealthHeal)ability).type = HealthHeal.HealingType.core;
                ((HealthHeal)ability).Initialize();
                break;
            case 12:
                ability = obj.AddComponent<HealthHeal>();
                ((HealthHeal)ability).type = HealthHeal.HealingType.energy;
                ((HealthHeal)ability).Initialize();
                break;
            case 13:
                ability = obj.AddComponent<Speed>();
                break;
            case 14:
                ability = obj.AddComponent<SiegeBullet>();
                break;
            case 15:
                ability = obj.AddComponent<SpeederBullet>();
                break;
            case 16:
                ability = obj.AddComponent<Harvester>();
                break;
            case 17:
                ability = obj.AddComponent<ShellRegen>();
                (ability as ShellRegen).index = 0;
                (ability as ShellRegen).Initialize();
                break;
            case 18:
                ability = obj.AddComponent<ShellMax>();
                (ability as ShellMax).index = 0;
                (ability as ShellMax).Initialize();
                break;
            case 19:
                ability = obj.AddComponent<ShellRegen>();
                (ability as ShellRegen).index = 2;
                (ability as ShellRegen).Initialize();
                break;
            case 20:
                ability = obj.AddComponent<ShellMax>();
                (ability as ShellMax).index = 2;
                (ability as ShellMax).Initialize();
                break;
            case 21:
                ability = obj.AddComponent<Command>();
                break;
            case 22:
                ability = obj.AddComponent<ShellRegen>();
                (ability as ShellRegen).index = 1;
                (ability as ShellRegen).Initialize();
                break;
            case 23:
                ability = obj.AddComponent<ShellMax>();
                (ability as ShellMax).index = 1;
                (ability as ShellMax).Initialize();
                break;
            case 24:
                ability = obj.AddComponent<Stealth>();
                break;
            case 25:
                ability = obj.AddComponent<DamageBoost>();
                break;
            case 26:
                ability = obj.AddComponent<AreaRestore>();
                break;
            case 27:
                ability = obj.AddComponent<PinDown>();
                break;
            case 28:
                ability = obj.AddComponent<Retreat>();
                break;
            case 29:
                ability = obj.AddComponent<AbsorptionField>();
                break;
            case 30:
                ability = obj.AddComponent<ActiveRegen>();
                (ability as ActiveRegen).index = 0;
                (ability as ActiveRegen).Initialize();
                break;
            case 31:
                ability = obj.AddComponent<ActiveRegen>();
                (ability as ActiveRegen).index = 1;
                (ability as ActiveRegen).Initialize();
                break;
            case 32:
                ability = obj.AddComponent<ActiveRegen>();
                (ability as ActiveRegen).index = 2;
                (ability as ActiveRegen).Initialize();
                break;
            case 33:
                ability = obj.AddComponent<Disrupt>();
                break;
            case 34:
                ability = obj.AddComponent<Control>();
                break;
            case 35:
                ability = obj.AddComponent<InvertTractor>();
                break;
            case 36:
                ability = obj.AddComponent<Bomb>();
                break;
            case 37:
                ability = obj.AddComponent<Ion>();
                break;
            case 38:
                ability = obj.AddComponent<Flak>();
                break;
            case 39:
                ability = obj.AddComponent<Rocket>();
                break;
            case 40:
                ability = obj.AddComponent<YardWarp>();
                break;
            case 41:
                ability = obj.AddComponent<Unload>();
                break;
            case 42:
                ability = obj.AddComponent<TowerAura>();
                (ability as TowerAura).type = TowerAura.AuraType.Heal;
                (ability as TowerAura).Initialize();
                break;
            case 43:
                ability = obj.AddComponent<TowerAura>();
                (ability as TowerAura).type = TowerAura.AuraType.Speed;
                (ability as TowerAura).Initialize();
                break;
            case 44:
                ability = obj.AddComponent<TowerAura>();
                (ability as TowerAura).type = TowerAura.AuraType.Energy;
                (ability as TowerAura).Initialize();
                break;
            case 45:
                ability = obj.AddComponent<ChainBeam>();
                break;
            case 46:
                ability = obj.AddComponent<SpeederMissile>();
                break;
            case 47:
                ability = obj.AddComponent<Radar>();
                break;
        }

        if (ability)
        {
            ability.SetTier(tier);
        }

        return ability;
    }
}
