﻿using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionTraverser : Traverser
{
    public new QuestCanvas nodeCanvas;

    public SectorManager.SectorLoadDelegate traverserLimiterDelegate;
    public MissionTraverser(QuestCanvas canvas) : base(canvas)
    {
        nodeCanvas = canvas;
        startNodeName = "StartMissionNode";
    }

    public override void StartQuest()
    {
        // If the quest has been started, continue
        nodeCanvas.missionName = findRoot().missionName;
        if(lastCheckpointName == (nodeCanvas.missionName + "_complete")) 
        {
            return;
        }
        Debug.Log(lastCheckpointName + nodeCanvas.missionName);
        base.StartQuest();
        SectorManager.OnSectorLoad += ((val) => {if(traverserLimiterDelegate != null) traverserLimiterDelegate.Invoke(val);});
        if(currentNode == null) TaskManager.Instance.RemoveTraverser(this);
    }

    public override bool activateCheckpoint(string CPName)
    {
        // If the quest has been started, continue
        lastCheckpointName = CPName;
        nodeCanvas.missionName = findRoot().missionName;
        if(CPName == (nodeCanvas.missionName + "_complete")) 
        {
            Debug.Log("Mission: " + nodeCanvas.missionName + " already complete. Not activating checkpoint or starting.");
            PlayerCore.Instance.cursave.missions.Find((mission) => mission.name == nodeCanvas.missionName).status 
                = Mission.MissionStatus.Complete;
            return true;
        }
        if(CPName == null || CPName == "") return false;
        nodeCanvas.missionName = findRoot().missionName;
        if(base.activateCheckpoint(CPName)) return true;
        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            var node = nodeCanvas.nodes[i];
            if (node is StartTaskNode && (node as StartTaskNode).taskName == CPName)
            {
                (node as StartTaskNode).forceTask = true;
                currentNode = node;
                return true;
            }
        }
        Debug.LogWarning("Could not find checkpoint: " + CPName + " " + nodeCanvas.missionName);
        return false;
    }

    public new StartMissionNode findRoot()
    {
        return base.findRoot() as StartMissionNode; // root node of a QuestCanvas must be a StartMissionNode
    }

    public virtual void ActivateTask(string ID)
    {
        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            var node = nodeCanvas.nodes[i] as StartTaskNode;
            if (node && node.taskID == ID)
            {
                node.StartTask();
            }
        }
    }

    public override void SetNode(Node node)
    {
        Debug.Log("Mission Canvas " + nodeCanvas.missionName + " now setting node: " + node);
        if(node is StartDialogueNode)
        {
            (node as StartDialogueNode).state = NodeEditorGUI.NodeEditorState.Mission;
        }
        if(node is DialogueNode)
            (node as DialogueNode).state = NodeEditorGUI.NodeEditorState.Mission;
        if(node is EndDialogue)
            (node as EndDialogue).state = NodeEditorGUI.NodeEditorState.Mission;
        base.SetNode(node);
    }

    protected override void Traverse()
    {
        while (true)
        {
            Debug.Log("Mission Canvas " +  nodeCanvas.missionName + " now traversing: " + currentNode);
            if(currentNode is StartDialogueNode)
            {
                (currentNode as StartDialogueNode).state = NodeEditorGUI.NodeEditorState.Mission;
            }
            if(currentNode is DialogueNode)
                (currentNode as DialogueNode).state = NodeEditorGUI.NodeEditorState.Mission;
            if(currentNode is EndDialogue)
                (currentNode as EndDialogue).state = NodeEditorGUI.NodeEditorState.Mission;
            if (currentNode == null)
                return;
            int outputIndex = currentNode.Traverse();
            if (outputIndex == -1)
                break;
            if (!currentNode.outputKnobs[outputIndex].connected())
                break;
            currentNode = currentNode.outputKnobs[outputIndex].connections[0].body;
        }
    }
}