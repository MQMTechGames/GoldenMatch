using UnityEngine;
using System.Collections;

struct MathUtils
{
	public static float kDegreeInRadians = Mathf.PI / 180.0f;

	public static float kEpsilon = 1e-4f;

	public static float getAngleToPoint(Transform transform, Vector3 point )
	{
		return getAngleToPoint(transform.position, transform.forward, transform.right, point);
	}

	public static float getAngleToPoint(Vector3 pos, Vector3 front, Vector3 right, Vector3 point )
	{
		Vector3 delta = point - pos;
		delta.Normalize();

		front.Normalize();

		float frontToDeltaProj = Vector3.Dot(front, delta);
		float rightToDeltaProj = Vector3.Dot(right, delta);
		
		float angle = Mathf.Atan2(rightToDeltaProj, frontToDeltaProj);

		return angle;
	}

	public static float getDistanceToPoint(Transform transform, Vector3 point)
	{
		Vector3 delta = point - transform.position;

		return delta.magnitude;
	}

    public static Vector3 getDirection(Vector3 a, Vector3 b)
    {
        Vector3 delta = b - a;

        return delta;
    }

    public static Vector3 getDirection(Transform a, Vector3 b)
    {
        Vector3 delta = b - a.position;

        return delta;
    }

    public static Vector3 getDirection(Transform a, Transform b)
    {
        Vector3 delta = b.position - a.position;

        return delta;
    }

	public static float dot(Transform transform, Vector3 point)
	{
		Vector3 delta = point - transform.position;

		delta.Normalize();
		Vector3 front = transform.forward;
		front = transform.rotation * front;
		front.Normalize();

		float dotProd = Vector3.Dot(delta, front);

		//DebugUtils.log("[DotProduct is: " + dotProd);
		return dotProd;
	}

	public static float sign(float num)
	{
		return num > 0.0f ? 1.0f : -1.0f;
	}

	public static float clamp(float num)
	{
		return num < 0.0f ? 0.0f : num > 1.0f ? 1.0f : num;
	}

	public static float toRadians(float degree)
	{
		return (Mathf.PI / 180.0f) * degree;
	}

	public static float toDegree(float radians)
	{
		return (180.0f/Mathf.PI) * radians;
	}
}
