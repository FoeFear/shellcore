﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The base class of every "being" in the game.
/// </summary>
public class Entity : MonoBehaviour {

    public ShellPart shell;
    protected static int maxLayer = 1; // the maximum sorting group layer of all entities
    protected SortingGroup group;
    protected float[] maxHealth; // maximum health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected float[] regenRate; // regeneration rate of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected Ability[] abilities; // abilities
    protected Rigidbody2D entityBody; // entity to modify with this script
    protected Collider2D hitbox; // the hitbox of the entity (excluding extra parts)
    protected TargetingSystem targeter; // the TargetingSystem of the entity
    protected bool isInCombat; // whether the entity is in combat or not
    protected bool isBusy; // whether the entity is busy or not
    protected bool isDead; // whether the entity is currently dead or not
    protected float busyTimer; // the time since the entity was last set to busy
    protected float combatTimer; // the time since the entity was last set into combat
    protected float deathTimer; // the time since the entity last died;
    protected GameObject explosionCirclePrefab; // prefabs for death explosion
    protected GameObject explosionLinePrefab;
    protected List<ShellPart> parts; // List containing all parts of the entity
    protected Sprite coreSprite; // sprites for shell and core
    protected Sprite shellSprite;
    protected Sprite minimapSprite; // sprite for minimap
    public float[] currentHealth; // current health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    public int faction; // What side the entity belongs to (0 = green, 1 = red, 2 = blue...) //TODO: get this from a file?
    public EntityBlueprint blueprint; // blueprint of entity containing parts
    public Vector3 spawnPoint;
    public Dialogue dialogue; // dialogue of entity TODO: maybe move to shellcore
    protected bool isDraggable; // is the entity draggable?
    protected Draggable draggable; // associated draggable
    private bool initialized; // is the entity safe to call update() on?
    public EntityCategory category = EntityCategory.Unset; // these two fields will be changed via hardcoding in child class files

    public string entityName;
    public enum TerrainType // terrain type of entity
    {
        Ground,
        Air,
        All,
        Unset
    }

    public enum EntityCategory // category of entity (carriers, outposts and bunkers are stations, everything else are units)
    {
        Station,
        Unit,
        All,
        Unset
    }

    TerrainType terrain = TerrainType.Unset;
    public TerrainType Terrain { get { return terrain; } protected set { terrain = value; } }

    // TODO: Respawn animation

    /// <summary>
    /// Generate shell parts in the blueprint, change ship stats accordingly
    /// </summary>
    protected virtual void BuildEntity()
    {
        // all created entities should have blueprints!
        if (!blueprint) Debug.Log(this + " does not have a blueprint! EVERY constructed entity should have one!");

        // Remove possible old parts from list
        parts.Clear();
        maxHealth = new float[] { 100, 100, 300 }; // hardcoded base healths of 100

        if (!GetComponent<SortingGroup>())
        {
            group = gameObject.AddComponent<SortingGroup>();
            group.sortingOrder = ++maxLayer;
        }

        if (!transform.Find("Shell Sprite")) // no shell in hierarchy yet? no problem
        {
            GameObject childObject = new GameObject("Shell Sprite"); // create the child gameobject
            childObject.transform.SetParent(transform, false); // set to child
            PolygonCollider2D collider = childObject.AddComponent<PolygonCollider2D>(); // add collider
            collider.isTrigger = true; // do not allow "actual" collisions
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>(); // add renderer
            renderer.sortingOrder = 100; // hardcoded max shell sprite value TODO: change this to being dynamic with the other parts
            if (blueprint)
            { // check if it contains a blueprint (it should)
                renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
            }
            else renderer.sprite = ResourceManager.GetAsset<Sprite>("core1_shell"); // set to default shellcore sprite
            ShellPart part = childObject.AddComponent<ShellPart>();
            part.detachible = false;
            shell = part;
        }
        if (!explosionCirclePrefab)
        {
            explosionCirclePrefab = new GameObject("Explosion Circle");
            explosionCirclePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionCirclePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionCirclePrefab.AddComponent<DrawCircleScript>();
            explosionCirclePrefab.SetActive(false);
        }
        if (!explosionLinePrefab)
        {
            explosionLinePrefab = new GameObject("Explosion Line");
            explosionLinePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionLinePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionLinePrefab.AddComponent<DrawLineScript>();
            explosionLinePrefab.SetActive(false);
        }
        if (!GetComponent<SpriteRenderer>())
        {
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.material = ResourceManager.GetAsset<Material>("material_color_swap");
            renderer.color = FactionColors.colors[faction];
            if (blueprint)
            { // check if it contains a blueprint (it should)
                if (blueprint.coreSpriteID == "" && blueprint.intendedType == EntityBlueprint.IntendedType.ShellCore)
                {
                    Debug.Log(this + "'s blueprint does not contain a core sprite ID!"); 
                    // check if the blueprint does not contain a core sprite ID (it should) 
                }
                renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
            }
            else renderer.sprite = ResourceManager.GetAsset<Sprite>("core1_light");
            renderer.sortingOrder = 101;
        }
        if (!GetComponent<Rigidbody2D>())
        {
            entityBody = gameObject.AddComponent<Rigidbody2D>();
            entityBody.gravityScale = 0;
            entityBody.drag = 10;
            entityBody.angularDrag = 100;
        }
        if (!GetComponent<Collider2D>())
        {
            hitbox = gameObject.AddComponent<PolygonCollider2D>();
            hitbox.isTrigger = true;
        }        
        if(!transform.Find("Minimap Image"))
        {
            GameObject childObject = new GameObject("Minimap Image");
            childObject.transform.SetParent(transform, false);
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
            childObject.AddComponent<MinimapLockRotationScript>();
        }
        GetComponent<Rigidbody2D>().mass = 1; // reset mass
        //For shellcores, create the tractor beam
        // Create shell parts
        if (blueprint != null)
        {
            for (int i = 0; i < blueprint.parts.Count; i++)
            {

                // lol

                EntityBlueprint.PartInfo part = blueprint.parts[i];
                PartBlueprint partBlueprint = ResourceManager.GetAsset<PartBlueprint>(part.partID);

                GameObject partObject = ShellPart.BuildPart(partBlueprint);
                ShellPart shellPart = partObject.GetComponent<ShellPart>();

                //Add an ability to the part:

                string shooterID = null;

                switch (part.abilityType)
                {
                    case Ability.AbilityType.None:
                        break;
                    case Ability.AbilityType.MainBullet: // Shouldn't happen
                        Debug.Log("Main bullet added to a part! This is a ShellCore only ability!");
                        var mainBullet = partObject.AddComponent<MainBullet>();
                        mainBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                        shooterID = "bulletshooter_sprite";
                        break;
                    case Ability.AbilityType.Bullet:
                        var bullet = partObject.AddComponent<Bullet>();
                        bullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                        shooterID = "bulletshooter_sprite";
                        break;
                    case Ability.AbilityType.SiegeBullet:
                        var siege = partObject.AddComponent<SiegeBullet>();
                        siege.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                        shooterID = "bulletshooter_sprite";
                        break;
                    case Ability.AbilityType.Beam:
                        var beam = partObject.AddComponent<Beam>();
                        beam.SetMaterial(ResourceManager.GetAsset<Material>("white_material"));
                        shooterID = "beamshooter_sprite";
                        break;
                    case Ability.AbilityType.Bomb:
                        break;
                    case Ability.AbilityType.Cannon:
                        var cannon = partObject.AddComponent<Cannon>();
                        cannon.effectPrefab = ResourceManager.GetAsset<GameObject>("cannonfire");
                        shooterID = "cannonshooter_sprite";
                        break;
                    case Ability.AbilityType.Missile:
                        var missile = partObject.AddComponent<Missile>();
                        missile.missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
                        if (part.spawnID != "missile_station_shooter")
                        {
                            shooterID = "missileshooter_sprite";
                        }
                        else
                        {
                            shooterID = "missile_station_shooter";
                            missile.category = EntityCategory.All;
                            missile.terrain = TerrainType.All;
                        }
                        break;
                    case Ability.AbilityType.Torpedo:
                        var torpedo = partObject.AddComponent<Torpedo>();
                        torpedo.bulletPrefab = ResourceManager.GetAsset<GameObject>("torpedo_prefab");
                        shooterID = "torpedoshooter_sprite";
                        break;
                    case Ability.AbilityType.ShellBoost:
                        HealthHeal shellboost = partObject.AddComponent<HealthHeal>();
                        shellboost.type = HealthHeal.HealingType.shell;
                        shooterID = "ability_indicator";
                        shellboost.Initialize();
                        break;
                    case Ability.AbilityType.CoreHeal:
                        HealthHeal coreboost = partObject.AddComponent<HealthHeal>();
                        coreboost.type = HealthHeal.HealingType.core;
                        shooterID = "ability_indicator";
                        coreboost.Initialize();
                        break;
                    case Ability.AbilityType.SpeedThrust:
                        partObject.AddComponent<SpeedThrust>();
                        shooterID = "ability_indicator";
                        break;
                    case Ability.AbilityType.PinDown:
                        break;
                    case Ability.AbilityType.EnergyBoost:
                        HealthHeal energyboost = partObject.AddComponent<HealthHeal>();
                        energyboost.type = HealthHeal.HealingType.energy;
                        shooterID = "ability_indicator";
                        energyboost.Initialize();
                        break;
                    case Ability.AbilityType.Harvester:
                        partObject.AddComponent<Harvester>();
                        shooterID = "ability_indicator";
                        break;
                    case Ability.AbilityType.SpeederBullet:
                        var speedBullet = partObject.AddComponent<SpeederBullet>();
                        speedBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                        shooterID = "bulletshooter_sprite";
                        break;
                    case Ability.AbilityType.Laser:
                        var laser = partObject.AddComponent<Laser>();
                        laser.bulletPrefab = ResourceManager.GetAsset<GameObject>("laser_prefab");
                        shooterID = "lasershooter_sprite";
                        break;
                    case Ability.AbilityType.SpawnDrone:
                        var spawn = partObject.AddComponent<SpawnDrone>();
                        spawn.spawnData = ResourceManager.GetAsset<DroneSpawnData>(part.spawnID);
                        spawn.Init();
                        shooterID = "ability_indicator";
                        break;
                    case Ability.AbilityType.Speed:
                        partObject.AddComponent<Speed>();
                        break;
                    default:
                        break;
                }

                

                // Add shooter
                if (shooterID != null)
                {
                    var shooter = new GameObject("Shooter");
                    shooter.transform.SetParent(partObject.transform);
                    shooter.transform.localPosition = Vector3.zero;
                    var shooterSprite = shooter.AddComponent<SpriteRenderer>();
                    shooterSprite.sprite = ResourceManager.GetAsset<Sprite>(shooterID);
                    shooterSprite.sortingOrder = 102;
                    shellPart.shooter = shooter;
                }

                partObject.transform.SetParent(transform, false);
                partObject.transform.SetAsFirstSibling();
                partObject.transform.localEulerAngles = new Vector3(0, 0, part.rotation);
                partObject.transform.localPosition = new Vector3(part.location.x, part.location.y, 0);
                SpriteRenderer sr = partObject.GetComponent<SpriteRenderer>();
                // sr.flipX = part.mirrored; this doesn't work, it does not flip the collider hitbox
                var tmp = partObject.transform.localScale;
                tmp.x = part.mirrored ? -1 : 1;
                partObject.transform.localScale = tmp;
                sr.sortingOrder = i + 2;
                entityBody.mass += partBlueprint.mass;
                maxHealth[0] += partBlueprint.health / 2;
                maxHealth[1] += partBlueprint.health / 4;

                var weaponAbility = partObject.GetComponent<WeaponAbility>();
                if (weaponAbility)
                {

                    // if the terrain and category wasn't preset set to the enitity's properties

                    if(weaponAbility.terrain == TerrainType.Unset)
                        weaponAbility.terrain = Terrain;
                    if(weaponAbility.category == EntityCategory.Unset)
                        weaponAbility.category = category;
                }
                    

                parts.Add(partObject.GetComponent<ShellPart>());
            }
        }
        Transform shellSprite = shell.transform;
        if(shellSprite)
        {
            parts.Add(shellSprite.GetComponent<ShellPart>());
        }
        ConnectedTreeCreator();

        currentHealth[0] = maxHealth[0];
        currentHealth[1] = maxHealth[1];
        currentHealth[2] = maxHealth[2];
        regenRate[0] = 100;
        regenRate[2] = 75;
        // Add abilities
        abilities = GetComponentsInChildren<Ability>();
    }
   
     public bool GetIsDead() {
        return isDead; // is dead
    }

    /// <summary>
    /// Helper method for death animation and state changing
    /// </summary>
    protected virtual void OnDeath() {
        // set death and immobility
        isDead = true;
        SetIntoCombat();
        deathTimer = 0; // reset death timer
        transform.Find("Minimap Image").GetComponent<SpriteRenderer>().enabled = false; // remove from minimap

        ResourceManager.PlayClipByID("clip_explosion1", transform.position);

        for(int i = 0; i < parts.Count; i++)
        {
            parts[i].SetCollectible((parts[i] != shell) && Random.Range(0F,5) > 2.5F && !(this as PlayerCore));
            parts[i].Detach();
        }

        GameObject tmp = Instantiate(explosionCirclePrefab); // instantiate circle explosion
        tmp.SetActive(true);
        tmp.transform.SetParent(transform, false);
        tmp.GetComponent<DrawCircleScript>().Initialize();
        Destroy(tmp, 2); // destroy explosions after 2 seconds
        for (int i = 0; i < 3; i++) { // instantiate line explosions
            tmp = Instantiate(explosionLinePrefab);
            tmp.SetActive(true);
            tmp.transform.SetParent(transform, false);
            tmp.GetComponent<DrawLineScript>().Initialize();
            Destroy(tmp, 2); // destroy explosions after 2 seconds
        }

        if(GameObject.Find("SectorManager")) 
            GameObject.Find("SectorManager").GetComponent<BattleZoneManager>().UpdateCounters();

    }

    protected virtual void PostDeath() 
    {
        Destroy(gameObject);
    }

    virtual protected void Awake() {
        // initialize instance fields
        currentHealth = new float[3];
        maxHealth = new float[3];
        regenRate = new float[3];
        parts = new List<ShellPart>();
        isBusy = false;
        targeter = new TargetingSystem(transform); // create the associated targeting system for this craft
        isInCombat = false;

        if (!GetComponent<Draggable>() && (this as Drone || this as Tank || this as Turret))
        {
            draggable = gameObject.AddComponent<Draggable>();
        }
        else if (GetComponent<Draggable>() && !draggable)
        {
            Debug.Log("Draggable was added to an entity manually, " +
                "it should be added automatically by setting isDraggable to true!");
        }

        AirCraftAI.entities.Add(this);
    }

    protected virtual void OnDestroy()
    {
        AirCraftAI.entities.Remove(this);
    }

    virtual protected void Start()
    {
        BuildEntity(); // Generate shell parts around the entity
        transform.position = spawnPoint;
        GetComponentInChildren<MinimapLockRotationScript>().Initialize(); // initialize the minimap dot
        targeter.SetTarget(null);
        //transform.rotation = Quaternion.identity; // reset rotation
        GetComponent<SpriteRenderer>().enabled = true; // enable sprite renderer
        busyTimer = 0; // reset busy timer
        initialized = true;
    }

    protected virtual void Update() {
        if(initialized) TickState(); // tick state
    }

    /// <summary>
    /// Helper method for Tick() that automatically adds health based on passed regen rates
    /// </summary>
    /// <param name="currentHealth">current health</param>
    /// <param name="regenRate">regen rate</param>
    /// <param name="maxHealth">the maximum value this health can have</param>
    protected void RegenHealth(ref float currentHealth, float regenRate, float maxHealth) {
        if (currentHealth + (regenRate * Time.deltaTime) > maxHealth) // if it would overheal
        {
            currentHealth = maxHealth; // set current health to max health
        }
        else
        {
            currentHealth += regenRate * Time.deltaTime; // add regenerated health
        }
    }

    /// <summary>
    /// Handles death and used for overriding
    /// </summary>
    virtual protected void DeathHandler()
    {
        if (currentHealth[1] <= 0 && !isDead)
        { // craft has been killed
            OnDeath(); // call death helper method
        }
    }
    /// <summary>
    /// Used to update the state of the craft- regeneration, timers, etc
    /// </summary>
    protected void TickState() {
        DeathHandler();
        if (isDead) // if the craft is dead
        {
            deathTimer += Time.deltaTime; // add time since last frame
            if (deathTimer >= 0.5F) // hardcoded based on animation (what?)
            {
                GetComponent<SpriteRenderer>().enabled = false; // disable craft sprite
                if(this as PlayerCore && (deathTimer > 2)) {
                    ((PlayerCore)this).alerter.showMessage("Respawning in " + (5 - (int)deathTimer) + " second"
                    + (5 - deathTimer > 1 ? "s." : "."));
                }
            }
            if (deathTimer >= 5F)
            {
                PostDeath();
            }
        }
        else { // not dead, continue normal state changing
            // regenerate shell and energy
            RegenHealth(ref currentHealth[0], regenRate[0], maxHealth[0]); 
            RegenHealth(ref currentHealth[2], regenRate[2], maxHealth[2]);

            // check if busy state changing is due
            if (busyTimer > 5)
            {
                isBusy = false; // change state if it is
            }
            else busyTimer += Time.deltaTime; // otherwise continue ticking timer

            // check if combat state changing is due
            if (combatTimer > 5)
            {
                isInCombat = false; // change state if it is
            }
            else combatTimer += Time.deltaTime; // otherwise continue ticking timer
        }
    }

    public virtual void RemovePart(ShellPart part)
    {
        if(part.GetComponent<Ability>())
        {
            part.GetComponent<Ability>().SetDestroyed(true);
        }
        entityBody.mass -= part.partMass;
        Domino(part);
        part.Detach();
        parts.Remove(part);
    }

    /// <summary>
    /// Make the craft busy
    /// </summary>
    public void MakeBusy() {
        isBusy = true; // set to true
        busyTimer = 0; // reset timer
    }

    /// <summary>
    /// Get whether the craft is busy or not
    /// </summary>
    /// <returns>true if the craft is busy, false otherwise</returns>
    public bool GetIsBusy() {
        return isBusy; // is busy
    }

    /// <summary>
    /// Set the craft into combat
    /// </summary>
    public void SetIntoCombat() {
        isInCombat = true; // set these to true
        isBusy = true;
        busyTimer = 0; // reset timers
        combatTimer = 0;
    }

    /// <summary>
    /// Get whether the craft is in combat or not
    /// </summary>
    /// <returns>true if the craft is in combat, false otherwise</returns>
    public bool GetIsInCombat()
    {
        return isInCombat; // is in combat
    }

    /// <summary>
    /// Get all the abilities of the craft by searching through all the parts
    /// </summary>
    /// <returns>All the abilities attached to the craft</returns>
    public Ability[] GetAbilities() {
        return abilities; 
        // create this array during start since it's likely that we'll be calling this multiple times
    }

    /// <summary>
    /// Get the targeting system of this craft
    /// </summary>
    /// <returns>the targeting system of the craft</returns>
    public TargetingSystem GetTargetingSystem() {
        return targeter; // get targeting system
    }

    /// <summary>
    /// Get the current health array of the craft
    /// </summary>
    /// <returns>the current health array of the craft</returns>
    public float[] GetHealth() {
        return currentHealth; // get current health
    }

    /// <summary>
    /// Get the maximum health array of the craft
    /// </summary>
    /// <returns>the maximum health array of the craft</returns>
    public float[] GetMaxHealth() {
        return maxHealth; // get max health
    }

    /// <summary>
    /// Removes health from the shell and/or core based on the passed piercing factor and current health
    /// </summary>
    /// <param name="amount">The amount of damage to do</param>
    /// <param name="shellPiercingFactor">The factor of damage that pierces through the shell into the core</param>
    public void TakeDamage(float amount, float shellPiercingFactor) {
        if (amount > 0) SetIntoCombat();
        currentHealth[0] -= amount * (1 - shellPiercingFactor); // subtract amount from shell
        if (currentHealth[0] < 0) { // if shell has dipped below 0
            currentHealth[1] += currentHealth[0]; // remove excess from core
            currentHealth[0] = 0; // set shell to zero
        }
        currentHealth[1] -= amount * shellPiercingFactor; // remove the rest of the damage from the core
        if (currentHealth[1] < 0) currentHealth[1] = 0;
        currentHealth[0] = currentHealth[0] > maxHealth[0] ? maxHealth[0] : currentHealth[0];
        currentHealth[1] = currentHealth[1] > maxHealth[1] ? maxHealth[1] : currentHealth[1];
    }

    /// <summary>
    /// Removes energy from the craft
    /// </summary>
    /// <param name="amount">The amount of energy to remove</param>
    public void TakeEnergy(float amount) {
        currentHealth[2] -= amount; // remove energy
        currentHealth[2] = currentHealth[2] > maxHealth[2] ? maxHealth[2] : currentHealth[2];
    }

    private void ConnectedTreeCreator() {
        foreach(ShellPart part in parts) 
        {
            if(part == shell) continue;

            // attach all core-connected parts to the shell as well
            if(part.GetComponent<SpriteRenderer>().bounds.Intersects(GetComponent<SpriteRenderer>().bounds)) {
                part.parent = shell;
                shell.children.Add(part);
            }
        }
        ConnectedTreeHelper(shell);
    }

    private void ConnectedTreeHelper(ShellPart parent) {
        foreach(ShellPart part in parts) 
        {
            if(part.parent != null || part == parent || part == shell) continue;
            if(part.IsAdjacent(parent)) {
                part.parent = parent;
                parent.children.Add(part);
            }
        }
        foreach(ShellPart part in parent.children) {
            ConnectedTreeHelper(part);
        }
    }
    private void DominoHelper(ShellPart parent) {
        foreach(ShellPart part in parent.children.ToArray()) {
            if(part) 
            {
                RemovePart(part);
            }
        }
    }

    private void Domino(ShellPart part) {
        if(part.parent) {
            part.parent.children.Remove(part);
        }
        DominoHelper(part);
    }
}
