using System;
using UnityEngine;
using UnityEngine.UI;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

[RequireComponent(typeof (Text))]
public class BPDemo_BulletCounter : MonoBehaviour
{
    const string display = "{0} bullets";
    private Text text;

    private void Start()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        text.text = string.Format(display, BulletPoolManager.instance.currentAmountOfBullets);
    }
}