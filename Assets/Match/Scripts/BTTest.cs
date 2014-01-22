using UnityEngine;
using System.Collections;

public class BTTest : MonoBehaviour
{
	BT bt;

	public float maxMainSeqStoppedTime = 2;
	float mainSeqStoppedCounter;

	public float maxMainSeqRunTime = 2;
	float mainSeqRunCounter;

	public struct MyCuquiClass
	{
		public int _int;
		public float _float;
		
		public MyCuquiClass(int int1, float float1)
		{
			_int = int1;
			_float = float1;
		}
	}

	public void cuquiModifier(ref MyCuquiClass input)
	{
		input._int *= 2;
		input._float *= 2;
	}

	void Awake()
	{
		bt = new BT ();

		BTNode testSeq;
		createSequence (bt, out testSeq);

		BTNode testRand;
		createRandomNode (bt, out testRand);

		BTNode testPriority;
		createPriority (bt, out testPriority, testSeq, testRand);
		//createRandomNode (bt, out mainSeq);
		//createParallelNode (bt, out mainSeq);

		//set main seq as root node
		bt.setRootNode (testPriority);

		// init props
		initProperties ();
	}

	void initProperties()
	{
		mainSeqStoppedCounter = 0;
		mainSeqRunCounter = 0;
	}

	//--------
	void createPriority(BT bt, out BTNode oNode, BTNode sequence, BTNode rand)
	{
		// create demo sequence
		oNode = new BTNodePriority (bt, "testPriorityNode");
		
		BTNodeLeaf firstPriorityNode = new BTNodeLeaf (bt, "firstPriorityNode", ()=> {
			Debug.Log("firstPriorityNode"); 

			return BTNodeResponse.LEAVE;
		});

		BTNode secondPriorityNode = sequence;
		if (null == secondPriorityNode) {
			secondPriorityNode = new BTNodeLeaf (bt, "secondPriorityNode", () => {
					Debug.Log ("secondPriorityNode"); 

					return BTNodeResponse.LEAVE;
			});
		}

		BTNode thirdPriorityNode = rand;
		if (null == thirdPriorityNode) {
			thirdPriorityNode = new BTNodeLeaf (bt, "thirdPriorityNode", ()=> {
				Debug.Log("thirdPriorityNode"); 
				
				return BTNodeResponse.LEAVE;
			});
		}

		((BTNodePriority)oNode).addNode (firstPriorityNode, () => {
			float r = Random.Range (0, 1.0f);
			if (r > 0.65f) {
				return true;
			}
			return false;
		});

		((BTNodePriority)oNode).addNode (secondPriorityNode, () => {
			float r = Random.Range (0, 1.0f);
			if (r > 0.50f) {
				return true;
			}
			return false;
		});

		((BTNodePriority)oNode).addNode (thirdPriorityNode, null);
	}

	//-------
	void createSequence(BT bt, out BTNode oNode)
	{
		// create demo sequence
		oNode = new BTNodeSequence (bt, "testSequenceNode");
		
		// create leaf nodes for the sequence
		BTNodeLeaf firstSeqNode = new BTNodeLeaf (bt, "firstSeqNode", ()=> {
			Debug.Log("firstSeqNode"); 
			
			return BTNodeResponse.LEAVE;
		});
		
		BTNodeLeaf secondSeqNode = new BTNodeLeaf (bt, "secondSeqNode", ()=> {
			Debug.Log("secondSeqNode"); 
			
			return BTNodeResponse.LEAVE;
		});
		
		BTNodeLeaf thirdSeqNode = new BTNodeLeaf (bt, "thirdSeqNode", ()=> {
			Debug.Log("thirdSeqNode"); 
			
			return BTNodeResponse.LEAVE;
		});

		((BTNodeSequence)oNode).addNode (firstSeqNode);
		((BTNodeSequence)oNode).addNode (secondSeqNode);
		((BTNodeSequence)oNode).addNode (thirdSeqNode);
	}

	void createRandomNode(BT bt, out BTNode oNode)
	{
		// create demo sequence
		oNode = new BTNodeRand (bt, "testRandomNode");
		
		// create leaf nodes for the sequence
		BTNodeLeaf randomOne = new BTNodeLeaf (bt, "randomOne", ()=> {
			Debug.Log("randomOne"); 
			
			return BTNodeResponse.LEAVE;
		});

		BTNodeLeaf randomSecond = new BTNodeLeaf (bt, "randomSecond", ()=> {
			Debug.Log("randomSecond"); 
			
			return BTNodeResponse.LEAVE;
		});

		BTNodeLeaf randomThird = new BTNodeLeaf(bt, "randomThird", ()=> {
			Debug.Log("randomThird"); 
			
			return BTNodeResponse.LEAVE;
		});
		
		((BTNodeRand)oNode).addNode (randomOne);
		((BTNodeRand)oNode).addNode (randomSecond);
		((BTNodeRand)oNode).addNode (randomThird);
	}

	void createParallelNode(BT bt, out BTNode oNode)
	{
		// create demo sequence
		oNode = new BTNodeParallel (bt, "testParallelNode");
		
		BTNodeLeaf parallelOne = new BTNodeLeaf (bt, "parallelOne", ()=> {
			Debug.Log("parallelOne"); 
			
			return BTNodeResponse.LEAVE;
		});
		
		BTNodeLeaf parallelSecond = new BTNodeLeaf (bt, "parallelSecond", ()=> {
			Debug.Log("parallelSecond"); 
			
			return BTNodeResponse.LEAVE;
		});
		
		BTNodeLeaf parallelThird = new BTNodeLeaf(bt, "parallelThird", ()=> {
			Debug.Log("parallelThird"); 
			
			return BTNodeResponse.LEAVE;
		});
		
		((BTNodeParallel)oNode).addNode (parallelOne);
		((BTNodeParallel)oNode).addNode (parallelSecond);
		((BTNodeParallel)oNode).addNode (parallelThird);
	}

	void Start()
	{
	}
	
	void Update()
	{
		bt.Update ();
	}
}
