﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SGF;

using Kurisu.Service.UIManager.Example;

public class AppMain : MonoBehaviour {

	// Use this for initialization

	void Start () {
        Debugger.EnableLog = true;

        Example ex = new Example();
        ex.Init();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
