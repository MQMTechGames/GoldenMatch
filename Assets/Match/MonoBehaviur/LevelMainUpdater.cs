using UnityEngine;
using System.Collections;

/**
 * This class is responsible to init the life cycle of a level
 **/
public class LevelMainUpdater : MonoBehaviour
{
	public ILevelMain levelMain;

	public void setLevelMain(ILevelMain iLevelMain)
	{
		levelMain = iLevelMain;
	}

	void Start()
	{
		levelMain.startLevel();
	}

	void Update()
	{
		levelMain.Update();
	}
}
