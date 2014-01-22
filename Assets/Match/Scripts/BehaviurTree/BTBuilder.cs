using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NodePrefab
{
	public BTNodeType 	_type;
	public BTCondition _condition;
	public BTCallback 	_callback;
	public float 		_probability;

	public NodePrefab(BTNodeType type, BTCondition condition, BTCallback callback, float probability )
	{
		_type = type;
	     _condition = condition;
		_callback = callback;;
		_probability = probability;
	}
}

public class BTBuilder
{
	BT 								_tree = null;

	Dictionary<string, NodePrefab> 	_nodePrefabs = null;
	bool							_isPrefab = false;
	string							_prefabName = "";

	private void savePrefab(BTNodeType type, BTCondition condition, BTCallback callback, float probability)
	{
		if(_nodePrefabs.ContainsKey(_prefabName)) {
			return;
		}

		NodePrefab nodeBuilderData = new NodePrefab(type, condition, callback, probability);
		_nodePrefabs.Add(_prefabName, nodeBuilderData);
	}

	private BTBuilder(string name)
	{
		createBT(name);
		_nodePrefabs = new Dictionary<string, NodePrefab>();
	}

	private BTBuilder(BT tree, string name)
	{
		if(null == tree) {
			createBT(name);
		} else {
			_tree = tree;

			DebugUtils.log("[BTBuilder]: Expanding tree " + tree._name);
		}
	}

	private void createBT(string name)
	{
		_tree = new BT ();
		_tree._name = name;
	}

	public static BTBuilder create(string name)
	{
		return new BTBuilder (name);
	}

	public static BTBuilder create(BT tree)
	{
		return new BTBuilder (tree, tree._name);
	}

	public BTBuilder getBT(out BT tree)
	{
		tree = _tree;
		return this;
	}

	public BTBuilder prefab(string prefabName)
	{
		_isPrefab = true;
		_prefabName = prefabName;

		return this;
	}

	public BTBuilder instantiate(string parentName, string name, string prefabName)
	{
		NodePrefab nodePrefab = null;
		_nodePrefabs.TryGetValue(prefabName, out nodePrefab);

		DebugUtils.assert(null != nodePrefab, "NodePrefab " + prefabName + " does NOT exist");

		addNode(parentName, name, nodePrefab._type, nodePrefab._condition, nodePrefab._callback, nodePrefab._probability);

		return this;
	}

	public BTBuilder addNode(string parentNodename, BTBuilderExtension btBuilderExtension)
	{
		btBuilderExtension(parentNodename, this);

		return this;
	}

	public BTBuilder addNode(string parentName, string name, BTNodeType type )
	{
		return addNode (parentName, name, type, null);
	}

	public BTBuilder addNode(string parentName, string name, BTNodeType type, BTCondition condition )
	{
		return addNode (parentName, name, type, condition, null);
	}

	public BTBuilder addNode(string parentName, string name, BTNodeType type, BTCondition condition, BTCallback callback )
	{
		return addNode (parentName, name, type, condition, callback, -1);
	}

	public BTBuilder addNode(string parentName, string name, BTNodeType type, float probability )
	{
		return addNode (parentName, name, type, null, null, probability);
	}

	public BTBuilder addNode(string parentName, string name, BTNodeType type, BTCondition condition, BTCallback callback, float probability )
	{
		if(_isPrefab) {
			savePrefab(type, condition, callback, probability);
			_isPrefab = false;

			return this;
		}

		// create node
		BTNode node = null;

		switch (type) {
			case BTNodeType.BTNODE_LEAF:
				DebugUtils.assert(callback!= null, "A callback is needed for the leafNode " + name);
				node = new BTNodeLeaf(_tree, name, callback);
			break;
				
			case BTNodeType.BTNODE_CONDITION:
				node = new BTNodeCondition(_tree, name, condition);
			break;

			case BTNodeType.BTNODE_WHILE:
			node = new BTNodeWhile(_tree, name, condition);
			break;
				
			case BTNodeType.BTNODE_RANDOM:
				node = new BTNodeRand(_tree, name);
			break;
				
			case BTNodeType.BTNODE_PRIORITY:
				node = new BTNodePriority(_tree,name);
			break;
				
			case BTNodeType.BTNODE_SEQUENCE:
				node = new BTNodeSequence(_tree,name);
			break;
				
			case BTNodeType.BTNODE_PARALLEL:
				node = new BTNodeParallel(_tree, name);
			break;
		}

		DebugUtils.assert (null != node, "Node type not implemented for the node " + name);

		// Add to tree or parent node
		if (null == parentName) {
			_tree.setRootNode(node);
			return this;
		}

		BTNode parent = _tree.getNodeByName (parentName);
		DebugUtils.assert (null != parent, "parent node not found when adding the node " + name);
		addNodeToNode (parent, node, condition, callback, probability);

		return this;
	}

	private void addNodeToNode(BTNode parent, BTNode node, BTCondition condition, BTCallback callback, float probability)
	{
		if (null == node) {
			_tree.setRootNode(node);
			
			return;
		}

		switch (parent._type) {
			case BTNodeType.BTNODE_LEAF:
			DebugUtils.assert(false, "not allowed to add nodes in the leaf node " + node._name);
			break;

			case BTNodeType.BTNODE_CONDITION:
				((BTNodeCondition)parent).addNode(node);
			break;

			case BTNodeType.BTNODE_WHILE:
				((BTNodeWhile)parent).addNode(node);
			break;

			case BTNodeType.BTNODE_RANDOM:
				if(probability >= 0) {
					((BTNodeRand)parent).addNode(node, probability);
	     		} else {
					((BTNodeRand)parent).addNode(node);
				}
			break;

			case BTNodeType.BTNODE_PRIORITY:
				((BTNodePriority)parent).addNode(node, condition);
			break;

			case BTNodeType.BTNODE_SEQUENCE:
				((BTNodeSequence)parent).addNode(node, condition);
			break;

			case BTNodeType.BTNODE_PARALLEL:
				((BTNodeParallel)parent).addNode(node);
			break;
		}
	}
}
