using UnityEngine;
using System.Collections;

public class Response<T> {
	
	public byte Index
	{
		get;
		private set;
	}

	public bool FlagCompleted
	{
		get;
		private set;
	}

	public bool FlagSafeToOverwrite
	{
		get;
		private set;
	}

	public bool FlagWaiting
	{
		get { return !(FlagCompleted || FlagSafeToOverwrite); }
	}
	public T Result;


	public Response(byte index)
	{
		Index = index;
		FlagSafeToOverwrite = true;
		FlagCompleted = false;
	}

	public bool Set()
	{
		if (FlagSafeToOverwrite)
		{
			FlagSafeToOverwrite = false;
			FlagCompleted = false;
			Result = default(T);
			return true;
		}
		else
		{
			CustomConsole.LogError("Don't Set() responses until you Recycle() them.");
			return false;
		}
	}

	public bool Fill(T result)
	{
		if(FlagWaiting)
		{

			FlagCompleted = true;
			Result = result;
			return true;
		}
		else
		{
			CustomConsole.LogError("Can't Fill() a response unless it has been Set() but not yet Filled.");
			return false;
		}
	}

	public bool Recycle()
	{
		FlagSafeToOverwrite = true;
		return FlagCompleted; //Returning false isn't an error. Sometimes you want to Recycle a response that hasn't been answered.
	}
}
