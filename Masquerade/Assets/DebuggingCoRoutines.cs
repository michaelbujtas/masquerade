using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DebuggingCoRoutines : MonoBehaviour {

	void Start () {
		Debug.Log("Call as a function: ");
		TheOriginalFunction();
		//Debug.Log("Call as a coroutine: ");
		//StartCoroutine(ThatFunctionAsACoroutine());
		Debug.Log("Fixed maybe?: ");
		StartCoroutine(ThisWrapperFixesIt());

	}
	
	void TheOriginalFunction()
	{
		List<System.Action<bool>> callbacks = new List<System.Action<bool>>();

		for (int i = 0; i < 3; i++)
		{
			int iCopy = i;
			callbacks.Add((coroutineReturn) => { Debug.Log("callback " + iCopy + " hit"); });
		}

		foreach (System.Action<bool> c in callbacks)
		{
			c(false);
		}
	}


	void ThisOneIsNotSupposedToWork()
	{
		List<System.Action<bool>> callbacks = new List<System.Action<bool>>();

		for (int i = 0; i < 3; i++)
		{
			callbacks.Add((coroutineReturn) => { Debug.Log("callback " + i + " hit"); });
		}

		foreach (System.Action<bool> c in callbacks)
		{
			c(false);
		}
	}

	IEnumerator ThatFunctionAsACoroutine()
	{
		List<System.Action<bool>> callbacks = new List<System.Action<bool>>();

		for (int i = 0; i < 3; i++)
		{
			int iCopy = i;
			callbacks.Add((coroutineReturn) => { Debug.Log("callback " + iCopy + " hit"); });
		}

		foreach (System.Action<bool> c in callbacks)
		{
			c(false);
			yield return null;
		}
	}

	IEnumerator ThisWrapperFixesIt()
	{
		List<System.Action<bool>> callbacks = new List<System.Action<bool>>();

		for (int i = 0; i < 3; i++)
		{
			System.Action wrapper = () =>
			{
				int iCopy = i;
				callbacks.Add((coroutineReturn) => { Debug.Log("callback " + iCopy + " hit"); });
			};
            wrapper();
         }
			

		foreach (System.Action<bool> c in callbacks)
		{
			c(false);
			yield return null;
		}
	}

	IEnumerator TestCoroutineReturns()
	{
		List<bool> allIsFinished = new List<bool>();


		for (int i = 0; i < 3; i++)
		{
			System.Action wrapper = () =>
			{
				int iCopy = i;
				allIsFinished.Add(false);
				StartCoroutine(TestCoroutineReturns2(i, (coroutineReturn) => { Debug.Log("action " + iCopy + " returned " + coroutineReturn); allIsFinished[iCopy] = true; }));
			};
			wrapper();
		}

		while (allIsFinished.Contains(false))
		{
			Debug.Log("yield return 1");
			yield return null;
		}
		Debug.Log("response 1");

	}

	IEnumerator TestCoroutineReturns2(int whichCopy, System.Action<bool> callback)
	{
		Debug.Log("starting subroutine number " + whichCopy);
		yield return null;
		yield return null;
		yield return null;

		Debug.Log("ending subroutine number " + whichCopy);
		callback(true);
	}
}
