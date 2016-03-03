using UnityEngine;
using System.Collections.Generic;

public class ResponseStore<T>
{

	public Response<T>[] Responses;
	
	public ResponseStore()
	{
		Responses = new Response<T>[byte.MaxValue];
	}


	public Response<T> Add()
	{
		//Should maybe be random open instead of first open?
		for(byte i = 0; i <= byte.MaxValue; i++)
		{
			if (Responses[i] == null)
				Responses[i] = new Response<T>(i);

			if(Responses[i].FlagSafeToOverwrite)
			{
				Responses[i].Set();
				return Responses[i];
            }
		}
		throw new System.ArgumentOutOfRangeException("Out of space in ResponseStore.");
	}
}
