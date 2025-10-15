using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ExtentionMethods
{
	/// <summary>
	/// multiplies each component of the this Vector by the multiplied Vector.
	/// </summary>
	/// <param name="thisVector"></param>
	/// <param name="multipliedVector"></param>
	/// <returns></returns>
	public static Vector3 Multiply(this Vector3 thisVector, Vector3 multipliedVector)
	{
		Vector3 returnedVector = new Vector3(thisVector.x * multipliedVector.x, thisVector.y * multipliedVector.y, thisVector.z * multipliedVector.z);
		return returnedVector;
	}

	public static Vector3 Divide(this Vector3 thisVector, Vector3 dividedVector)
	{
		Vector3 returnedVector;
		//x
		if (dividedVector.x != 0)
		{
			returnedVector.x = thisVector.x / dividedVector.x;
		}
		else
		{
			returnedVector.x = 0;
		}
		//y
		if (dividedVector.y != 0)
		{
			returnedVector.y = thisVector.y / dividedVector.y;
		}
		else
		{
			returnedVector.y = 0;
		}
		//z
		if (dividedVector.z != 0)
		{
			returnedVector.z = thisVector.z / dividedVector.z;
		}
		else
		{
			returnedVector.z = 0;
		}
		return returnedVector;
	}

	public static Vector3 Raw(this Vector3 thisVector)
	{
		return thisVector.Divide(thisVector.Abs());
	}

	public static Vector3 Abs(this Vector3 thisVector)
	{
		Vector3 returnedVector;
		returnedVector.x = Mathf.Abs(thisVector.x);
		returnedVector.y = Mathf.Abs(thisVector.y);
		returnedVector.z = Mathf.Abs(thisVector.z);
		return returnedVector;
	}

	/// <summary>
	/// recursively finds a parent with a target type. to pick a type do typeof(Type). by default it tries this 10 times. The maximum amount you can set tries to is 20.
	/// </summary>
	/// <param name="tries"></param>
	/// <param name="transfom"></param>
	/// <returns></returns>
	public static GameObject FindParentOfType(this Transform targetTrans, Type targetType, int tries = 10)
	{
		//sets max amount of tries so no overflow
		if (tries > 20)
		{
			tries = 20;
		}
		//deduct the amount of attempts so no overflow
		tries--;

		//check if there even is a parent or if tries have run out
		if (targetTrans.parent == null || tries < 0)
		{
			Debug.LogWarning("Could not find parent with type " + targetType + ". Tries remaining: " + tries);
			return null;
		}

		//check if I found it
		if (targetTrans.parent.GetComponent(targetType))
		{
			//Debug.Log("Found " + targetTrans.parent.name);
			return targetTrans.parent.gameObject;
		}

		//set a new transform
		targetTrans = targetTrans.parent;

		//try again
		//Debug.Log("again..");
		return FindParentOfType(targetTrans, targetType, tries);
		
	}

	public static AudioSource PitchShift(this AudioSource source, float min = 0.8f, float max = 1.2f) 
	{
		source.pitch = UnityEngine.Random.Range(min, max);
		return source;
	}
}
