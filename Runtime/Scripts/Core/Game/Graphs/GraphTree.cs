using System;
using System.Collections;
using System.Collections.Generic;
using core.graphs;
using UnityEditor;
using UnityEngine;

namespace core.graphs
{
    [CreateAssetMenu()]
    public class GraphTree : ScriptableObject
    {
        public Node rootNode;
        public Node.State treeState = Node.State.Running;
        public List<Node> nodes = new List<Node>();

        public Node.State Update()
        {
            if(rootNode.state == Node.State.Running)
                treeState = rootNode.Update();
            
            return treeState;
        }

        public Node CreateNode(Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            nodes.Add(node);
            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();

            return node;
        }

        public void DeleteNode(Node node)
        {
            nodes.Remove(node);
            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parentNode, Node childNode)
        {
            DecoratorNode decorator = parentNode as DecoratorNode;

            if(decorator != null)
            {
                decorator.child = childNode;
            }

            CompositeNode composite = parentNode as CompositeNode;

            if(composite != null)
            {
                composite.children.Add(childNode);
            }
        }

        public void RemoveChild(Node parentNode, Node childNode)
        {
            DecoratorNode decorator = parentNode as DecoratorNode;

            if(decorator != null)
            {
                decorator.child = null;
            }

            CompositeNode composite = parentNode as CompositeNode;

            if(composite != null)
            {
                composite.children.Remove(childNode);
            }
        }

        public List<Node> GetChildren(Node parentNode)
        {
            List<Node> nodes = new List<Node>();

            DecoratorNode decorator = parentNode as DecoratorNode;

            if(decorator != null && decorator.child != null)
            {
                nodes.Add(decorator.child);
            }

            CompositeNode composite = parentNode as CompositeNode;

            if(composite != null)
            {
                nodes.AddRange(composite.children);
            }

            return nodes;
        }
    }
}