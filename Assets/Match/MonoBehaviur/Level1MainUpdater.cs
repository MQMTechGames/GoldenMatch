using UnityEngine;
using System.Collections;

/**
 * This class is responsible to init the life cycle of a level
 **/
public class Level1MainUpdater : LevelMainUpdater
{
	Level1MainUpdater()
	{
		ILevelMain levelMain = new Level1Main ();
		setLevelMain (levelMain);
	}
}
