using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BTNodeWhile : BTNode
{
	BTNode  		_node = null;
	BTCondition _condition = null;
	
	public BTNodeWhile(BT tree, string name, BTCondition condition)
		:base(tree, name, BTNodeType.BTNODE_WHILE)
	{
		_condition = condition;
	}
	
	public void addNode(BTNode node)
	{
		_node = node;
		node._parent = this;
	}
	
	public override void onStart ()
	{
	}
	
	public override BTNode getBacktrackingNode()
	{
		return this;
	}
	
	public override BTNodeResponse Update ()
	{
		bool passed = true;
		if (null != _condition) {
			passed = _condition ();
		}
		
		if (passed) {
			_tree.setCurrentNode(_node);
			
			return BTNodeResponse.STAY;
		}
		
		return BTNodeResponse.LEAVE;
	}
}
