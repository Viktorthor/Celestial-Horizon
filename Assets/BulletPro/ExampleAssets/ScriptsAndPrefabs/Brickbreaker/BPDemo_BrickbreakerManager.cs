using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_BrickbreakerManager : MonoBehaviour {

	List<BPDemo_BreakableBlock> blocks;
	public static BPDemo_BrickbreakerManager instance;
	
	[Header("References")]
	public GameObject blockPrefab;
	public Transform upperLeftBlock;
	public BulletEmitter[] playerEmitters;

	[Header("Layout")]
	public int columns;
	public int rows;
	public Vector2 blockSize, spacing;

	void Awake()
	{
		if (instance)
		{
			Destroy(gameObject);
			return;
		}

		instance = this;

		blocks = new List<BPDemo_BreakableBlock>();
		for (int i = 0; i < columns; i++)
			for (int j = 0; j < rows; j++)
			{
				float x = upperLeftBlock.position.x + i * (blockSize.x + spacing.x);
				float y = upperLeftBlock.position.y - j * (blockSize.y + spacing.y);
				Vector3 pos = new Vector3(x, y, upperLeftBlock.position.z);
				GameObject go = GameObject.Instantiate(blockPrefab, pos, Quaternion.identity) as GameObject;
				blocks.Add(go.GetComponent<BPDemo_BreakableBlock>());
				go.name = string.Format("Block ({0}, {1})", i, j);
				go.transform.SetParent(upperLeftBlock);
			}
	}

	public void RegisterBlock(BPDemo_BreakableBlock block)
	{
		if (blocks.Contains(block)) return;
		blocks.Add(block);
	}

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			for (int i = 0; i < playerEmitters.Length; i++)
				playerEmitters[i].Kill();

			for (int i = 0; i < blocks.Count; i++)
				blocks[i].Respawn();
		}
	}
}
