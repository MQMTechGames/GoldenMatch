using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BTRandData
{
	public float  			_probStart;
	public float  			_probability;
	public BTNode 			_node;

	public BTRandData(float probStart, float probability, BTNode node)
	{
		_probStart = probStart;
		_probability = probability;
		_node = node;
	}

	public bool getIsInRange(float rand)
	{
		float normRand = rand - _probStart;

		if (normRand >= 0 && normRand <= _probability) {
			return true;
		}

		return false;
	}

	public float getProbabilityEnd()
	{
		return _probStart + _probability;
	}
}

public class BTNodeRand : BTNode
{
	List<BTRandData> 	 _nodes			= null;
	bool 				_calculateProbs;
	bool 				_isInit;
	
	public BTNodeRand(BT tree, string name)
		:base(tree, name, BTNodeType.BT_NODE_RANDOM)
	{
		_nodes = new List<BTRandData> ();
		_calculateProbs = false;
	}
	
	public void addNode(BTNode node)
	{
		node._parent = this;
		
		BTRandData priotityNode = new BTRandData (0.0f, 0.0f, node);
		_nodes.Add (priotityNode);

		_calculateProbs = true;
	}

	// Probabily must be normalized
	public void addNode(BTNode node, float propability)
	{
		node._parent = this;

		int count = _nodes.Count;

		float probStart = 0.0f;
		if (count > 0) {
			probStart = _nodes[count -1].getProbabilityEnd();
		}

		BTRandData priotityNode = new BTRandData (probStart, propability, node);
		_nodes.Add (priotityNode);
	}
	
	public override void onStart ()
	{
		_isInit = false;

		if (_calculateProbs) {
			setupDefaultProbability ();
		}
	}

	public override BTNode getBacktrackingNode()
	{
		if (null != _parent) {
			return _parent.getBacktrackingNode();
		}
		
		return this;
	}

	void setupDefaultProbability()
	{
		float numNodes = (float)_nodes.Count;
		float equalProb = 1.0f / numNodes;
		
		float probStart = 0.0f;
		foreach (BTRandData data in _nodes) {
			data._probStart = probStart;
			data._probability = equalProb;
			
			probStart += equalProb;
		}
	}
	
	public override BTNodeResponse Update ()
	{
		if (_isInit) {
			return BTNodeResponse.LEAVE;
		}
		_isInit = true;

		float r = Random.Range (0.0f, 1.0f);

		foreach (BTRandData node in _nodes)
		{
			bool isInRange = node.getIsInRange(r);

			if(isInRange) {
				_tree.setCurrentNode( node._node );

				return BTNodeResponse.STAY;
			}
		}

		return BTNodeResponse.LEAVE;
	}
}
