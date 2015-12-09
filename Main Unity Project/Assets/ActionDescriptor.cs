using UnityEngine;
using System.Collections;

public class ActionDescriptor {

	public byte ActorIndex = CardIndex.EMPTY_SLOT;
	public CardAction Type = CardAction.UNSET;
	public byte TargetIndex = CardIndex.EMPTY_SLOT;
	public ActionDescriptor() { }
	public ActionDescriptor(byte actorIndex, CardAction type, byte targetIndex)
	{
		ActorIndex = actorIndex;
		Type = type;
		TargetIndex = targetIndex;

	}
	public bool Complete
	{
		get
		{
			switch(Type)
			{
				case CardAction.UNSET:
					return false;
				case CardAction.ATTACK:
					return ActorIndex != CardIndex.EMPTY_SLOT && TargetIndex != CardIndex.EMPTY_SLOT;
				case CardAction.FLIP:
					return ActorIndex != CardIndex.EMPTY_SLOT;
				case CardAction.ACTIVATE:
					return ActorIndex != CardIndex.EMPTY_SLOT;
				default:
					return false;
			}
		}
	}

}
