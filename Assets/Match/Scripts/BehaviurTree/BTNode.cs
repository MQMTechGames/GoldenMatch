using System.Collections;
using System.Collections.Generic;

public abstract class BTNode
{
	public string 		_name;
	public BTNode 		_parent;
	public BT 			_tree = null;

	public BTNodeType	_type = BTNodeType.BTNODE_NO_TYPE;

	public BTNode(BT tree, string name, BTNodeType type)
	{
		_tree = tree; 
		_name = name;
		_type = type;

		_tree.registerNode (this);
	}

	public abstract void onStart ();
	public abstract BTNode getBacktrackingNode ();
	public abstract BTNodeResponse Update ();
}
