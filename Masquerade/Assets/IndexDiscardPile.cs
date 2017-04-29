using UnityEngine;
using System.Collections.Generic;
using BeardedManStudios.Network;


public class IndexDiscardPile : SimpleNetworkedMonoBehavior {

	public CardRenderer Renderer;

	public List<byte> Contents = new List<byte>();

	public void Start()
	{
		Renderer.Index = CardIndex.EMPTY_SLOT;
		Renderer.RefreshCardImage();
	}

	public void AddIndex(byte index)
	{
		Contents.Add(index);

		Renderer.Index = TopCard;
		Renderer.RefreshCardImage();
		Sync();
	}

	public void RemoveIndex(byte index)
	{
		Contents.Remove(index);

		Renderer.Index = TopCard;

		Renderer.RefreshCardImage();
		Sync();

	}

	public void Clear()
	{
		Contents.Clear();
		Renderer.Index = CardIndex.EMPTY_SLOT;
		Renderer.RefreshCardImage();
		Sync();
	}

	public byte TopCard
	{
		get
		{
			if (Contents.Count > 0)
				return Contents[Contents.Count - 1];
			else
				return CardIndex.EMPTY_SLOT;
		}
	}

	public void Sync()
	{
		RPC("SyncContentsRPC", Contents.ToArray());
	}

	[BRPC]
	void SyncContentsRPC(byte[] contents)
	{
		Contents.Clear();
		Contents.AddRange(contents);
		Renderer.Index = TopCard;
		Renderer.RefreshCardImage();
	}
}
