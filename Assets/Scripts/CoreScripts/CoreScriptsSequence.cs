using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsManager;
using System.Linq;

// TODO: Add the Start and Sector triggers
// TODO: Multiple CoreScripts files
public class CoreScriptsSequence : MonoBehaviour
{
    public enum InstructionCommand
    {
        SetInteraction,
        Log,
        Call,
        ConditionBlock,
        SpawnEntity,
        SetVariable,
        AddIntValues,
        ConcatenateValues,
        StartCutscene,
        FinishCutscene,
        SetPath,
        Rotate,
        PassiveDialogue
    }
    public struct Instruction
    {
        public InstructionCommand command;
        public string arguments;
        public Sequence sequence;
    }


    public struct Sequence
    {
        public List<Instruction> instructions;
    }

    private struct MissionTrigger
    {
        public string name;
        public string rank;
        public List<string> prerequisites;
        public Sequence sequence;
    }

    private static readonly List<char> specialChars = new List<char>()
    {
        ';',
        '?',
        ':',
        '+',
        '=',
        '{',
        '}',
        '[',
        ']',
        '|',
        '\\',
        '/',
        '*',
        '&',
        '^',
        '%',
        '#',
        '@',
        '!',
        '`',
        '~',
    };


    // TODO: Read out of the global variables array for world base properties
    public static string GetArgument(string arguments, string key, bool rawValue = false)
    {
        if (specialChars.Exists(e => key.Contains(e)))
        {
            throw new System.Exception("Argument values cannot have special characters except for underscores in them.");
        }
        var args = arguments.Split(";");
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != key) continue;

            var val = args[i+1];
            if (rawValue) return val;
            else if (val.StartsWith("$$$") && SaveHandler.instance.GetSave().coreScriptsGlobalVarNames != null)
            {
                
                var index = SaveHandler.instance.GetSave().coreScriptsGlobalVarNames.IndexOf(val.Substring(3));
                if (index >= 0) 
                    return SaveHandler.instance.GetSave().coreScriptsGlobalVarValues[index];
            }
            else if (val.StartsWith("$$") && CoreScriptsManager.instance.globalVariables != null)
            {
                return CoreScriptsManager.instance.globalVariables[val.Substring(2)];
            }
            else return val;
        }
        return null;
    }

    public static string AddArgument(string arguments, string key, string value)
    {
        if (specialChars.Exists(e => key.Contains(e)) || specialChars.Exists(e => value.Contains(e)))
        {
            throw new System.Exception("Argument values cannot have semicolons or commas in them.");
        }
        if (string.IsNullOrEmpty(arguments)) arguments = $"{key};{value}";
        else arguments += $";{key};{value}";
        return arguments;
    }

    public static void RunSequence (Sequence seq, Context context)
    {
        var traverser = CoreScriptsManager.instance;
        foreach (var inst in seq.instructions)
        {
            switch (inst.command)
            {
                case InstructionCommand.SetInteraction:
                    InteractAction action = new InteractAction();
                    var entityID = GetArgument(inst.arguments, "entityID");
                    action.action = new UnityEngine.Events.UnityAction(() =>
                        {
                            switch (context.type)
                            {
                                case TriggerType.Mission:
                                    TaskManager.Instance.SetSpeakerID(entityID);
                                    break;
                                default:
                                    DialogueSystem.Instance.SetSpeakerID(entityID);
                                    break;
                            }
                            
                            DialogueSystem.StartDialogue(traverser.dialogues[GetArgument(inst.arguments, "dialogueID")], null, context);
                        });

                    switch (context.type)
                    {
                        case TriggerType.Mission:
                            TaskManager.Instance.PushInteractionOverrides(entityID, action, null, context);
                            break;
                        default:
                            DialogueSystem.Instance.PushInteractionOverrides(entityID, action, null);
                            break;
                    }
                    break;
                case InstructionCommand.Call:
                    var s = traverser.GetFunction(GetArgument(inst.arguments, "name"));
                    RunSequence(s, context);
                    break;
                case InstructionCommand.ConditionBlock:
                    var cb = traverser.conditionBlocks[int.Parse(GetArgument(inst.arguments, "ID"))];
                    CoreScriptsCondition.ExecuteConditionBlock(cb, context);
                    break;
                case InstructionCommand.SpawnEntity:
                    SpawnEntity(
                        GetArgument(inst.arguments, "entityID"), 
                        GetArgument(inst.arguments, "forceCharacterTeleport") != "false",
                        GetArgument(inst.arguments, "flagName"),
                        GetArgument(inst.arguments, "blueprintJSON"),
                        int.Parse(GetArgument(inst.arguments, "faction")),
                        GetArgument(inst.arguments, "name"));
                    break;
                case InstructionCommand.Log:
                    Debug.LogWarning(GetArgument(inst.arguments, "message"));
                    break;
                case InstructionCommand.SetVariable:
                    var variableName = GetArgument(inst.arguments, "name", true);
                    var variableValue = GetArgument(inst.arguments, "value");
                    SetVariable(variableName, variableValue);
                    break;
                case InstructionCommand.AddIntValues:
                    var val1 = int.Parse(GetArgument(inst.arguments, "val1"));
                    var val2 = int.Parse(GetArgument(inst.arguments, "val2"));
                    var name = GetArgument(inst.arguments, "name", true);
                    SetVariable(name, val1+val2+"");
                    break;
                case InstructionCommand.ConcatenateValues:
                    var v1 = GetArgument(inst.arguments, "val1");
                    var v2 = GetArgument(inst.arguments, "val2");
                    var varName = GetArgument(inst.arguments, "name", true);
                    SetVariable(varName, v1+v2);
                    break;
                case InstructionCommand.SetPath:
                    entityID = GetArgument(inst.arguments, "entityID");
                    var rotateWhileMoving = GetArgument(inst.arguments, "rotateWhileMoving") != "false";
                    var customMass = GetArgument(inst.arguments, "customMass") == null ? -1 : float.Parse(GetArgument(inst.arguments, "customMass"));
                    var flagName = GetArgument(inst.arguments, "flagName");

                    SetPath.Execute(entityID, rotateWhileMoving, customMass, flagName, inst.sequence, context);
                    break;
                case InstructionCommand.Rotate:
                    entityID = GetArgument(inst.arguments, "entityID");
                    var targetID = GetArgument(inst.arguments, "targetID");
                    var angle = GetArgument(inst.arguments, "angle");

                    Rotate.Execute(entityID, targetID, angle, inst.sequence, context);
                    break;
                case InstructionCommand.StartCutscene:
                    Cutscene.StartCutscene();
                    break;
                case InstructionCommand.FinishCutscene:
                    Cutscene.FinishCutscene();
                    break;
                case InstructionCommand.PassiveDialogue:
                    entityID = GetArgument(inst.arguments, "entityID");
                    var text = CoreScriptsManager.instance.GetLocalMapString(GetArgument(inst.arguments, "text"));
                    var soundType = GetArgument(inst.arguments, "soundType");
                    var onlyShowIfInParty = GetArgument(inst.arguments, "onlyShowIfInParty") == "true";

                    PassiveDialogue.Execute(entityID, text, soundType, onlyShowIfInParty);
                    break;
            }
        }
    }

    private static void SetVariable(string variableName, string variableValue)
    {
        if (variableName.StartsWith("$$$"))
        {
            var names = SaveHandler.instance.GetSave().coreScriptsGlobalVarNames;
            var vals = SaveHandler.instance.GetSave().coreScriptsGlobalVarValues;
            var key = variableName.Substring(3);
            var index = names.IndexOf(key);
            if (index >= 0) vals[index] = variableValue;
            else
            {
                names.Add(key);
                vals.Add(variableValue);
            }
        }
        else if (variableName.StartsWith("$$"))
        {
            var dict = CoreScriptsManager.instance.globalVariables;
            var key = variableName.Substring(2);
            if (dict.ContainsKey(key))
            {
                dict[key] = variableValue;
            }
            else dict.Add(key, variableValue);
        }
    }

    public static Sequence ParseSequence(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var seq = new Sequence();
        seq.instructions = new List<Instruction>();

        List<string> stx = null;
        bool skipToComma = false;
        int brax = 0;

        List<string> standardInstructions = new List<string>();

        foreach (string instType in Enum.GetNames(typeof(InstructionCommand)))
        {
            if (instType != "Call" && instType != "ConditionBlock")
            {
                standardInstructions.Add(instType);
            }
        }

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            if (standardInstructions.Exists(s => lineSubstr.StartsWith(s)))
            {
                seq.instructions.Add(ParseInstruction(i, line, blocks));
            }
            else if (lineSubstr.StartsWith("Call"))
            {
                var funcName = lineSubstr.Substring(5);
                funcName = funcName.Substring(0, funcName.IndexOf(")"));
                var inst = new Instruction();
                inst.arguments = AddArgument(inst.arguments, "name", funcName);
                inst.command = InstructionCommand.Call;
                seq.instructions.Add(inst);
            }
            else if (lineSubstr.StartsWith("ConditionBlock"))
            {
                var b = CoreScriptsCondition.ParseConditionBlock(i, line, blocks);
                blocks.Add(b.ID, b);
                var inst = new Instruction();
                inst.command = InstructionCommand.ConditionBlock;
                inst.arguments = AddArgument(inst.arguments, "ID", b.ID+"");
                seq.instructions.Add(inst);
            }
        }

        return seq;
    }

    // TODO: Move condition type to be declaration instead of type= style element
    private static Instruction ParseInstruction(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var substr = line.Substring(index).Split("(")[0].Trim();
        var inst = new Instruction();
        Enum.TryParse<InstructionCommand>(substr, out inst.command);
        inst.arguments = "";
        bool skipToComma = true;
        List<string> stx = null;
        int brax = 0;

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);

            var name = "";
            var val = "";
            GetNameAndValue(lineSubstr, out name, out val);
            if (name == "sequence")
            {
                inst.sequence = ParseSequence(i, line, blocks);
                continue;
            }

            inst.arguments = AddArgument(inst.arguments, name, val);
        }

        return inst;
    }

    public static void GetNameAndValue(string line, out string name, out string val, bool continueThroughScopes = false)
    {
        if (!line.Contains("="))
        {
            name = val = "";
            return;
        }

        var brackets = 0;
        name = line.Split("=")[0];
        val = line.Split("=")[1];
        int minIndex = val.Length;

        var commaExists = val.IndexOf(',') != -1;

        for (int i = 0; i < val.Length; i++)
        {
            if (val[i] == '(') brackets++;
            else if (val[i] == ')') brackets--;

            if (brackets > 0 && continueThroughScopes) continue;

            if (val[i] == ',' || (!commaExists && val[i] == ')')) 
            {
                minIndex = i;
                break;
            }
        }

        var n = name;
        var v = val;
        val = val.Substring(0, minIndex);
        if (specialChars.Exists(e => n.Contains(e)))
        {
            throw new System.Exception($"Attribute names or values cannot have semicolons or commas in them: {n}");
        }

        
        if (specialChars.Exists(e => v.Contains(e)))
        {
            throw new System.Exception($"Attribute names or values cannot have semicolons or commas in them: {v}");
        }
    }

    private static void SpawnEntity(string entityID, bool forceCharacterTeleport, string flagName, string blueprintJSON, int faction, string name)
    {
        Vector2 coords = new Vector2();
        for (int i = 0; i < AIData.flags.Count; i++)
        {
            if (AIData.flags[i].name == flagName)
            {
                coords = AIData.flags[i].transform.position;
                break;
            }
        }

        foreach (var data in SectorManager.instance.characters)
        {
            if (data.ID != entityID) continue;
            Debug.Log("Spawn Entity ID given matches with a character name! Spawning character...");

            foreach (var oj in AIData.entities)
            {
                if (oj && oj.ID == data.ID)
                {
                    Debug.Log("Character already found. Not spawning.");
                    if (forceCharacterTeleport)
                    {
                        if (!(oj is AirCraft airCraft)) continue;
                        airCraft.Warp(coords, false);
                    }

                    return;
                }
            }

            var characterBlueprint = SectorManager.TryGettingEntityBlueprint(data.blueprintJSON);
            Sector.LevelEntity entityData = new Sector.LevelEntity
            {
                faction = data.faction,
                name = data.name,
                position = coords,
                ID = data.ID
            };
            SectorManager.instance.SpawnEntity(characterBlueprint, entityData);
            return;
        }


        Debug.Log($"Spawn Entity ID ( {entityID} ) does not correspond with a character. Performing normal operations.");
        EntityBlueprint blueprint = SectorManager.TryGettingEntityBlueprint(blueprintJSON);

        if (blueprint)
        {
            Sector.LevelEntity entityData = new Sector.LevelEntity
            {
                faction = faction,
                name = name,
                position = coords,
                ID = entityID
            };
            var entity = SectorManager.instance.SpawnEntity(blueprint, entityData);
            if (DevConsoleScript.fullLog)
            {
                Debug.Log(entity.transform.position + " " + entity.spawnPoint);
            }

            entity.name = name;
        }
        else
        {
            Debug.LogWarning("Blueprint not found!");
        }
    }
}