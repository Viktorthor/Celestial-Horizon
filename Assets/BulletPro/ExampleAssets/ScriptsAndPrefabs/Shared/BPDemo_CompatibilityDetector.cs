using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_CompatibilityDetector : MonoBehaviour
{
	public Image image;

	void Start()
	{
		image.color = SystemInfo.supportsComputeShaders ? Color.green : Color.red;
	}
}
