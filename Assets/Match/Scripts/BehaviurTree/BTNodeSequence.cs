using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BTSequenceCondition
{
	public BTCondition  _condition = null;
	public BTNode 			_node = null;
	
	public BTSequenceCondition(BTCondition condition, BTNode node)
	{
		_condition = condition;
		_node = node;
	}
}

public class BTNodeSequence : BTNode
{
	List<BTSequenceCondition> 	 _nodes			= null;
	int 						_currNodePos	= -1;

	public BTNodeSequence(BT tree, string name)
		:base(tree, name, BTNodeType.BTNODE_SEQUENCE)
	{
		_nodes = new List<BTSequenceCondition> ();
	}

	public void addNode(BTNode node)
	{
		addNode (node, null);
	}

	public void addNode(BTNode node, BTCondition condition)
	{
		node._parent = this;
		
		BTSequenceCondition seqCond = new BTSequenceCondition (condition, node);
		_nodes.Add (seqCond);
	}

	public override void onStart ()
	{
		_currNodePos = -1;
	}

	public override BTNode getBacktrackingNode()
	{
		return this;
	}

	public override BTNodeResponse Update ()
	{
		// select current node
		if (_currNodePos >= _nodes.Count -1) {
				return BTNodeResponse.LEAVE;
		} else {
			_currNodePos++;
		}

		BTSequenceCondition currNodeInSeq = _nodes [_currNodePos];

		// Check whether the condition exist and is false to skip this currNode
		if (   null != currNodeInSeq._condition
			&& false == currNodeInSeq._condition ()
		   ) {
			return BTNodeResponse.STAY;
		}

		// Update current node
		_tree.setCurrentNode (currNodeInSeq._node);

		return BTNodeResponse.STAY;
	}
}
