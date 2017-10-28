using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
	// LevelManager level;

	// Make event-listener system for noise
	public delegate void NoiseAction(Tile source, int intensity);
	public static event NoiseAction UponNoise;

	/*
	void Awake()
	{
		level = LevelManager.instance;
	}
	*/

	// triggering
	public void makeNoise (Tile source, int intensity){
		if (UponNoise != null) {
			UponNoise (source, intensity);
		}
	}

}