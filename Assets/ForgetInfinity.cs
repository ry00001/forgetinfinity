﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System;

public class ForgetInfinity : MonoBehaviour {
	public KMSelectable[] Buttons;
	public KMSelectable SubmitButton;
	public KMSelectable ResetButton;
	public KMBombModule Module;
	public KMBombInfo Info;
	public TextMesh Screen;

	public static string[] ignoredModules = null;

	private int[] code = {0, 0, 0, 0, 0};
	private int codeIndex = 0;

	private bool solved = false;
	private bool bossMode = false;
	private bool canForget = false;

	private List<List<int>> stages = new List<List<int>>();
	private int stagePtr = 0;
	private int solveStagePtr = 0;
	private bool firstSolve = true;

	private int lastThing = 0;

	// Use this for initialization
	void Awake() {
		if (ignoredModules == null)
			ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("Forget Infinity", new string[]{
				"Forget Me Not",     //Mandatory to prevent unsolvable bombs.
				"Forget Everything", //Cruel FMN.
				"Turn The Key",      //TTK is timer based, and stalls the bomb if only it and FMN are left.
				"Souvenir",          //Similar situation to TTK, stalls the bomb.
				"The Time Keeper",   //Again, timilar to TTK.
				"Alchemy",
				"Forget This",
				"Simon's Stages",
				"Timing is Everything",
				"Forget Infinity" // Also mandatory to prevent unsolvable bombs.
			});
	}

	List<int> GenerateRandom() {
		var t = new List<int> ();
		for (int i = 0; i < 5; i++) {
			t.Add (UnityEngine.Random.Range (1, 6));
		}
		return t;
	}

	void Start () {
		Module.OnActivate += delegate {
			BeginForgetting ();
		};
		Buttons [0].OnInteract += delegate{Handle1();return false;};
		Buttons [1].OnInteract += delegate{Handle2();return false;};
		Buttons [2].OnInteract += delegate{Handle3();return false;};
		Buttons [3].OnInteract += delegate{Handle4();return false;};
		Buttons [4].OnInteract += delegate{Handle5();return false;};
		SubmitButton.OnInteract += delegate {
			Submit ();
			return false;
		};
		ResetButton.OnInteract += delegate {
			Reset ();
			return false;
		};
	}

	void BeginForgetting() {
		Debug.Log ("[Forget Infinity] Module activated...! Let's forget!");
		canForget = true;
		updateScreen (new[]{ 0, 0, 0, 0, 0 });
	}
	
	// Update is called once per frame
	void Update () {
		CheckForNewSolves ();
	}

	string ListString(List<string> a) {
		var sb = new StringBuilder ();
		foreach(var j in a) {
			sb.Append (j + " ");
		}
		return sb.ToString ();
	}

	string ListString(List<int> a) {
		var sb = new StringBuilder ();
		foreach(var j in a) {
			sb.Append (j.ToString() + " ");
		}
		return sb.ToString ();
	}

	void NextStage() {
		Debug.Log ("[Forget Infinity] advancing stage!");
		var rand = GenerateRandom ();
		stages.Add (rand);
		stagePtr++;
		updateScreen (rand.ToArray());
		Debug.Log ("[Forget Infinity] we are now on stage " + stagePtr.ToString());
		Debug.Log ("[Forget Infinity] next stage is: " + ListString(rand));
	}

	void CheckForNewSolves() {
		if (solved)
			return;
		if (bossMode)
			return;
		if (!canForget)
			return;
		var solvables = Info.GetSolvableModuleNames ().Where (a => !ignoredModules.Contains (a));
		var list1 = Info.GetSolvedModuleNames ().Where(a => solvables.Contains(a));
		if (list1.Count () >= solvables.Count() && !firstSolve) {
			Debug.Log ("[Forget Infinity] all non-ignored solvables solved. activating boss mode.");
			bossMode = true;
			updateScreen (new[] { 0, 0, 0, 0, 0 });
			return;
		}
		if (list1.Count() != lastThing) {
			NextStage ();
			firstSolve = false;
		}
		lastThing = list1.Count ();
	}

	void Submit() {
		SubmitButton.AddInteractionPunch ();
		if (!bossMode) {
			Debug.Log ("[Forget Infinity] boss mode not active. Strike! (submit button)");
			Module.HandleStrike ();
			return;
		}
		var stg = stages[solveStagePtr];
		Debug.Log (ListString (stg));
		Debug.Log ("solve stage ptr = " + solveStagePtr.ToString ());
		Debug.Log ("stage count = " + stages.Count ());
		for (int i = 0; i < 5; i++) {
			if (code [i] != stg[i]) {
				Debug.Log ("[Forget Infinity] Code is different from the expected input of " + ListString (stg) + ". Strike!");
				Module.HandleStrike ();
				Reset ();
				return;
			}
		}
		if (stages.Count()-1 <= solveStagePtr) {
			solved = true;
			Module.HandlePass ();
			Screen.text = "XXXXX";
			return;
		}
		solveStagePtr++;
		for (int i=0; i<5; i++) this.code [i] = 0;
		this.codeIndex = 0;
		updateScreen ();
	}

	void updateScreen(int[] a) {
		string b = "";
		for (int i = 0; i < 5; i++) {
			b += a[i].ToString();
		}
		Screen.text = b;
	}

	void updateScreen() {
		updateScreen (code);
	}

	void Number(int button) {
		if (solved)
			return;
		if (!bossMode) {
			Debug.Log ("[Forget Infinity] boss mode not active. Strike! (button "+button.ToString()+")");
			Module.HandleStrike ();
			return;
		}
		Debug.Log ("[Forget Infinity] button " + button.ToString () + " pushed");
		if (this.codeIndex == this.code.Length)
			return;
		if (this.codeIndex < 5) {
			this.code [this.codeIndex++] = button;
		}
		updateScreen ();
	}

	void Handle1() {
		Buttons[0].AddInteractionPunch ();
		Number (1);
	}

	void Handle2() {
		Buttons[1].AddInteractionPunch ();
		Number (2);
	}

	void Handle3() {
		Buttons[2].AddInteractionPunch ();
		Number (3);
	}

	void Handle4() {
		Buttons[3].AddInteractionPunch ();
		Number (4);
	}

	void Handle5() {
		Buttons[4].AddInteractionPunch ();
		Number (5);
	}

	void Reset() {
		if (solved)
			return;
		if (!bossMode) {
			Debug.Log ("[Forget Infinity] boss mode not active. Strike! (reset button)");
			Module.HandleStrike ();
			return;
		}
		for (int i=0; i<5; i++) this.code [i] = 0;
		this.codeIndex = 0;
		this.solveStagePtr = 0;
		updateScreen ();
	}
}
