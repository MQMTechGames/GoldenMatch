using UnityEngine;
using System.Collections;

struct TransformUtils
{
	public static float kDegreeInRadians = Mathf.PI / 180.0f;

	public static float rotateToPointStep(Transform transform, Vector3 point, float step, bool useRigidbody = true)
	{
		float angle = MathUtils.getAngleToPoint(transform, point);

		float finalRotationAngle = angle * step;
		if(Mathf.Abs(angle) < Mathf.Abs(finalRotationAngle)) {
			finalRotationAngle = angle;
		}

		Vector3 up = new Vector3(0,1,0);

		Quaternion deltaQuat = Quaternion.AngleAxis(MathUtils.toDegree(finalRotationAngle), up);
		Quaternion newQuat = transform.rotation * deltaQuat;

		if(useRigidbody) {
			transform.rigidbody.MoveRotation(transform.rotation * deltaQuat);
		} else {
			transform.Rotate(up, angle);
		}

		return angle;
	}
}
