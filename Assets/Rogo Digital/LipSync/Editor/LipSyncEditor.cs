using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System;

using RogoDigital;
using RogoDigital.Lipsync;

[CustomEditor(typeof(LipSync))]
public class LipSyncEditor : Editor {
	private LipSync lsTarget;

	private string[] blendables;
	private SerializedObject serializedTarget;
	private int markerTab = 0;
	private bool saving = false;
	private string savingName = "";

	private AnimBool showBoneOptions;
	private AnimBool showPlayOnAwake;
	private AnimBool showFixedFrameRate;

	private Editor blendSystemEditor;

	private int blendSystemNumber = 0;
	private List<System.Type> blendSystems;
	private List<string> blendSystemNames;
	private BlendSystemButton.Reference[] blendSystemButtons = new BlendSystemButton.Reference[0];

	private Texture2D logo;

	private Texture2D guideAI;
	private Texture2D guideE;
	private Texture2D guideEtc;
	private Texture2D guideFV;
	private Texture2D guideL;
	private Texture2D guideMBP;
	private Texture2D guideO;
	private Texture2D guideU;
	private Texture2D guideWQ;

	private Texture2D presetsIcon;

	private GUIStyle miniLabelDark;

	private SerializedProperty audioSource;
	private SerializedProperty restTime;
	private SerializedProperty restHoldTime;
	private SerializedProperty phonemeCurveGenerationMode;
	private SerializedProperty playOnAwake;
	private SerializedProperty loop;
	private SerializedProperty defaultClip;
	private SerializedProperty defaultDelay;
	private SerializedProperty scaleAudioSpeed;
	private SerializedProperty animationTimingMode;
	private SerializedProperty frameRate;
	private SerializedProperty useBones;
	private SerializedProperty boneUpdateAnimation;
	private SerializedProperty onFinishedPlaying;

	private float versionNumber = 1.1f;
	private List<Texture2D> guides; 

	void OnEnable () {
		logo = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Dark/logo_component.png");

		guideAI = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/AI.png");
		guideE = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/E.png");
		guideEtc = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/etc.png");
		guideFV = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/FV.png");
		guideL = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/L.png");
		guideMBP = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/MBP.png");
		guideO = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/O.png");
		guideU = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/U.png");
		guideWQ = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Guides/WQ.png");

		presetsIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Dark/presets.png");

		if(!EditorGUIUtility.isProSkin){
			logo = (Texture2D)EditorGUIUtility.Load("Rogo Digital/Lipsync/Light/logo_component.png");
			presetsIcon = (Texture2D)EditorGUIUtility.Load("Rogo Digital/LipSync/Light/presets.png");
		}

		guides = new List<Texture2D>(){
			guideAI,
			guideE,
			guideU,
			guideO,
			guideEtc,
			guideFV,
			guideL,
			guideMBP,
			guideWQ,
			Texture2D.blackTexture
		};

		Undo.undoRedoPerformed += OnUndoRedoPerformed;

		lsTarget = (LipSync)target;
		lsTarget.reset.AddListener(OnEnable);
		if (lsTarget.blendSystem == null) lsTarget.blendSystem = lsTarget.GetComponent<BlendSystem>();

		if(lsTarget.lastUsedVersion < versionNumber) {
			AutoUpdate(lsTarget.lastUsedVersion);
			lsTarget.lastUsedVersion = versionNumber;
		}

		serializedTarget = new SerializedObject(target);
		FindBlendSystems();

		audioSource = serializedTarget.FindProperty("audioSource");
		restTime = serializedTarget.FindProperty("restTime");
		restHoldTime = serializedTarget.FindProperty("restHoldTime");
		phonemeCurveGenerationMode = serializedTarget.FindProperty("phonemeCurveGenerationMode");
		playOnAwake = serializedTarget.FindProperty("playOnAwake");
		loop = serializedTarget.FindProperty("loop");
		defaultClip = serializedTarget.FindProperty("defaultClip");
		defaultDelay = serializedTarget.FindProperty("defaultDelay");
		scaleAudioSpeed = serializedTarget.FindProperty("scaleAudioSpeed");
		animationTimingMode = serializedTarget.FindProperty("animationTimingMode");
		frameRate = serializedTarget.FindProperty("frameRate");
		useBones = serializedTarget.FindProperty("useBones");
		boneUpdateAnimation = serializedTarget.FindProperty("boneUpdateAnimation");
		onFinishedPlaying = serializedTarget.FindProperty("onFinishedPlaying");

		showBoneOptions = new AnimBool(lsTarget.useBones , Repaint);
		showPlayOnAwake = new AnimBool(lsTarget.playOnAwake , Repaint);
		showFixedFrameRate = new AnimBool(lsTarget.animationTimingMode == LipSync.AnimationTimingMode.FixedFrameRate, Repaint);

		CreateBlendSystemEditor();

		if(lsTarget.blendSystem != null){
			if(lsTarget.blendSystem.isReady){
				GetBlendShapes();
				blendSystemButtons = GetBlendSystemButtons();
			}
		}

		if(lsTarget.phonemes == null || lsTarget.phonemes.Count < 9){
			lsTarget.phonemes = new List<PhonemeShape>();
			
			for(int A = 0 ; A < 10 ; A++){
				lsTarget.phonemes.Add(new PhonemeShape((Phoneme)A));
			}
			
			EditorUtility.SetDirty(lsTarget);
			serializedTarget.SetIsDifferentCacheDirty();
		}else if(lsTarget.phonemes.Count == 9){
			//Update pre 0.4 characters with the new Rest phoneme
			lsTarget.phonemes.Add(new PhonemeShape(Phoneme.Rest));
		}
	}

	void OnDisable () {
		Undo.undoRedoPerformed -= OnUndoRedoPerformed;

		if(lsTarget.blendSystem != null){
			if(lsTarget.blendSystem.isReady){
				foreach(Shape shape in lsTarget.phonemes){
					for(int blendable = 0 ; blendable < shape.weights.Count ; blendable++){
						lsTarget.blendSystem.SetBlendableValue(shape.blendShapes[blendable] , 0);
					}
				}
			}
		}

		if(LipSyncEditorExtensions.currentToggle > -1 && lsTarget.useBones){
			foreach(Shape shape in lsTarget.phonemes){
				foreach(BoneShape bone in shape.bones){
					if(bone.bone != null){
						bone.bone.localPosition = bone.neutralPosition;
						bone.bone.localEulerAngles = bone.neutralRotation;
					}
				}
			}
		}

		LipSyncEditorExtensions.currentToggle = -1;
	}

	void ChangeBlendSystem() {
		Undo.RecordObject(lsTarget, "Change Blend System");

		if (lsTarget.GetComponent<BlendSystem>() != null) {
			if (blendSystems[blendSystemNumber] == null) {
				BlendSystem[] oldSystems = lsTarget.GetComponents<BlendSystem>();
				foreach (BlendSystem system in oldSystems) {
					system.OnBlendSystemRemoved();
					DestroyImmediate(blendSystemEditor);
					Undo.DestroyObjectImmediate(system);
				}
			} else if (blendSystems[blendSystemNumber] != lsTarget.GetComponent<BlendSystem>().GetType()) {
				BlendSystem[] oldSystems = lsTarget.GetComponents<BlendSystem>();
				foreach (BlendSystem system in oldSystems) {
					system.OnBlendSystemRemoved();
					DestroyImmediate(blendSystemEditor);
					Undo.DestroyObjectImmediate(system);
				}

				Undo.AddComponent(lsTarget.gameObject, blendSystems[blendSystemNumber]);
				lsTarget.blendSystem = lsTarget.GetComponent<BlendSystem>();
				CreateBlendSystemEditor();
			}
		} else if (blendSystems[blendSystemNumber] != null) {
			Undo.AddComponent(lsTarget.gameObject, blendSystems[blendSystemNumber]);
			lsTarget.blendSystem = lsTarget.GetComponent<BlendSystem>();
			CreateBlendSystemEditor();
		}
	}

	void OnUndoRedoPerformed() {
		lsTarget.blendSystem = lsTarget.GetComponent<BlendSystem>();
		DestroyImmediate(blendSystemEditor);
		CreateBlendSystemEditor();

		if (LipSyncEditorExtensions.oldToggle > -1 && lsTarget.useBones) {
			if (markerTab == 0) {
				foreach (BoneShape boneshape in lsTarget.phonemes[LipSyncEditorExtensions.oldToggle].bones) {
					if (boneshape.bone != null) {
						boneshape.bone.localPosition = boneshape.neutralPosition;
						boneshape.bone.localEulerAngles = boneshape.neutralRotation;
					}
				}
			}
		}

		if (markerTab == 0) {
			foreach (PhonemeShape shape in lsTarget.phonemes) {
				foreach (int blendable in shape.blendShapes) {
					lsTarget.blendSystem.SetBlendableValue(blendable, 0);
				}
			}
		}

		if (LipSyncEditorExtensions.currentToggle > -1) {
			if (markerTab == 0) {
				for (int b = 0; b < lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].blendShapes.Count; b++) {
					lsTarget.blendSystem.SetBlendableValue(lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].blendShapes[b], lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].weights[b]);
				}
			}
		}
	}

	public override void OnInspectorGUI () {
		if(serializedTarget == null){
			OnEnable();
		}

		if(miniLabelDark == null) {
			miniLabelDark = new GUIStyle(EditorStyles.miniLabel);
			miniLabelDark.normal.textColor = Color.black;
		}

		serializedTarget.Update();

		EditorGUI.BeginDisabledGroup(saving);
		Rect fullheight = EditorGUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box(logo ,  GUIStyle.none);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		if (blendSystems == null) {
			FindBlendSystems();
		}
		if (blendSystems.Count == 0) {
			EditorGUILayout.Popup("Blend System", 0, new string[] { "No BlendSystems Found" });
		} else {
			if (lsTarget.blendSystem == null) {
				EditorGUI.BeginChangeCheck();
				blendSystemNumber = EditorGUILayout.Popup("Blend System", blendSystemNumber, blendSystemNames.ToArray(), GUIStyle.none);
				if (EditorGUI.EndChangeCheck()) {
					ChangeBlendSystem();
				}
				GUI.Box(new Rect(EditorGUIUtility.labelWidth + GUILayoutUtility.GetLastRect().x, GUILayoutUtility.GetLastRect().y, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height), "Select a BlendSystem", EditorStyles.popup);
			} else {
				EditorGUI.BeginChangeCheck();
				blendSystemNumber = EditorGUILayout.Popup("Blend System", blendSystemNumber, blendSystemNames.ToArray());
				if (EditorGUI.EndChangeCheck()) {
					ChangeBlendSystem();
				}
			}
		}
		if (lsTarget.blendSystem == null) {
			GUILayout.Label("No BlendSystem Selected");
			EditorGUILayout.HelpBox("A Blend System is required to use LipSync.", MessageType.Info);
		}

		EditorGUILayout.Space();
		if (lsTarget.blendSystem != null) {
			if (blendSystemEditor == null) CreateBlendSystemEditor();
			blendSystemEditor.OnInspectorGUI();
			if (!lsTarget.blendSystem.isReady) {
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Continue", GUILayout.MaxWidth(200))) {
					lsTarget.blendSystem.OnVariableChanged();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
			}
		}

		if(lsTarget.blendSystem != null){
			if(lsTarget.blendSystem.isReady){
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(audioSource , new GUIContent ("Audio Source" , "AudioSource to play dialogue from."));

				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(useBones , new GUIContent("Use Bone Transforms" , "Allow BoneShapes to be added to phoneme poses. This enables the use of bone based facial animation."));
				showBoneOptions.target = lsTarget.useBones;
				if(EditorGUILayout.BeginFadeGroup(showBoneOptions.faded)){
					EditorGUILayout.PropertyField(boneUpdateAnimation , new GUIContent("Account for Animation" , "If true, will calculate relative bone positions/rotations each frame. Improves results when using animation, but will cause errors when not."));
					EditorGUILayout.Space();
				}
				FixedEndFadeGroup(showBoneOptions.faded);
				EditorGUILayout.Space();
				if(blendSystemButtons.Length > 0 && blendSystemButtons.Length < 3) {	
					Rect buttonPanel = EditorGUILayout.BeginHorizontal();
					EditorGUI.HelpBox(new Rect(buttonPanel.x , buttonPanel.y-4 , buttonPanel.width , buttonPanel.height+8) , "BlendSystem Commands:" , MessageType.Info);
					GUILayout.FlexibleSpace();
					DrawBlendSystemButtons();
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}else if(blendSystemButtons.Length >= 3) {	
					Rect buttonPanel = EditorGUILayout.BeginHorizontal();
					EditorGUI.HelpBox(new Rect(buttonPanel.x , buttonPanel.y-4 , buttonPanel.width , buttonPanel.height+8) , "BlendSystem Commands:" , MessageType.Info);
					GUILayout.FlexibleSpace();
					DrawBlendSystemButtons();
					GUILayout.Space(5);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}
				int oldTab = markerTab;

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUI.BeginDisabledGroup(true);
				markerTab = GUILayout.Toolbar(markerTab , new GUIContent[]{new GUIContent("Phonemes") , new GUIContent("Emotions") , new GUIContent("Gestures")} , GUILayout.MaxWidth(400) , GUILayout.MinHeight(23));
				EditorGUI.EndDisabledGroup();
				Rect presetRect = EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button(new GUIContent(presetsIcon , "Presets") , GUILayout.MaxWidth(32)  , GUILayout.MinHeight(23))){
					GenericMenu menu = new GenericMenu();

					string[] directories = Directory.GetDirectories(Application.dataPath , "Presets" , SearchOption.AllDirectories);

					bool noItems = true;
					foreach(string directory in directories) {
						foreach(string file in Directory.GetFiles(directory)){
							if(Path.GetExtension(file).ToLower() == ".asset"){
								LipSyncPreset preset = AssetDatabase.LoadAssetAtPath<LipSyncPreset>("Assets" + file.Substring((Application.dataPath).Length));
								if(preset != null){
									noItems = false;
									menu.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(file)) , false , LoadPreset , file);
								}
							}
						}

						string[] subdirectories = Directory.GetDirectories(directory);
						foreach(string subdirectory in subdirectories){
							foreach(string file in Directory.GetFiles(subdirectory)){
								if(Path.GetExtension(file).ToLower() == ".asset"){
									LipSyncPreset preset = AssetDatabase.LoadAssetAtPath<LipSyncPreset>("Assets" + file.Substring((Application.dataPath).Length));
									if(preset != null){
										noItems = false;
										menu.AddItem(new GUIContent(Path.GetFileName(subdirectory)+"/"+Path.GetFileNameWithoutExtension(file)) , false , LoadPreset , file);
									}
								}
							}
						}
					}


					if(noItems)menu.AddDisabledItem(new GUIContent("No Presets Found"));

					menu.AddSeparator("");
					menu.AddItem(new GUIContent("Save New Preset") , false , NewPreset);
					if (AssetDatabase.FindAssets("t:BlendShapePreset").Length > 0) {
						menu.AddDisabledItem(new GUIContent("Old-style presets found. Convert them to use."));
					}

					menu.DropDown(presetRect);
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.Space(10);
				GUILayout.Label("Upgrade to LipSync Pro to unlock Emotions and Gestures, along with many more features!");
				if(markerTab != oldTab){
					if(oldTab == 0){
						foreach(PhonemeShape phoneme in lsTarget.phonemes){
							foreach(int shape in phoneme.blendShapes){
								lsTarget.blendSystem.SetBlendableValue(shape , 0);
							}
						}
					}
					if(LipSyncEditorExtensions.currentTarget == lsTarget) LipSyncEditorExtensions.currentToggle = -1;
				}

				if(markerTab == 0){
					int a = 0;
					foreach(PhonemeShape phoneme in lsTarget.phonemes){
						this.DrawShapeEditor(lsTarget.blendSystem, lsTarget.useBones, phoneme, phoneme.phoneme.ToString() + " Phoneme", a);
						a++;
					}
				}

				EditorGUILayout.Space();
				GUILayout.Box("General Animation Settings" , EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(animationTimingMode, new GUIContent("Timing Mode" , "How animations are sampled: AudioPlayback uses the audioclip, CustomTimer uses a framerate independent timer, FixedFrameRate is framerate dependent."));
				showFixedFrameRate.target = lsTarget.animationTimingMode == LipSync.AnimationTimingMode.FixedFrameRate;
				if (EditorGUILayout.BeginFadeGroup(showFixedFrameRate.faded)) {
					EditorGUILayout.PropertyField(frameRate, new GUIContent("Frame Rate", "The framerate to play the animation at."));
				}
				EditorGUILayout.EndFadeGroup();
				EditorGUILayout.PropertyField(playOnAwake , new GUIContent ("Play On Awake" , "If checked, the default clip will play when the script awakes."));
				showPlayOnAwake.target = lsTarget.playOnAwake;
				if(EditorGUILayout.BeginFadeGroup(showPlayOnAwake.faded)){
					EditorGUILayout.PropertyField(defaultClip , new GUIContent ("Default Clip" , "The clip to play on awake."));
					EditorGUILayout.PropertyField(defaultDelay , new GUIContent ("Default Delay" , "The delay between the scene starting and the clip playing."));
				}
				EditorGUILayout.EndFadeGroup();
				EditorGUILayout.PropertyField(loop, new GUIContent("Loop Clip", "If true, will make any played clip loop when it finishes."));
				EditorGUILayout.PropertyField(scaleAudioSpeed , new GUIContent ("Scale Audio Speed" , "Whether or not the speed of the audio will be slowed/sped up to match Time.timeScale."));
				EditorGUILayout.Space();
				GUILayout.Box("Phoneme Animation Settings" , EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(restTime , new GUIContent ("Rest Time" , "If there are no phonemes within this many seconds of the previous one, a rest will be inserted."));
				EditorGUILayout.PropertyField(restHoldTime , new GUIContent ("Pre-Rest Hold Time" , "The time, in seconds, a shape will be held before blending when a rest is inserted."));
				EditorGUILayout.PropertyField(phonemeCurveGenerationMode, new GUIContent("Phoneme Curve Generation Mode", "How tangents are generated for animations. Tight is more accurate, Loose is more natural."));
				EditorGUILayout.Space();
				
				GUILayout.Space(20);

				EditorGUILayout.PropertyField(onFinishedPlaying);

				if (LipSyncEditorExtensions.oldToggle != LipSyncEditorExtensions.currentToggle && LipSyncEditorExtensions.currentTarget == lsTarget) {

					if (LipSyncEditorExtensions.oldToggle > -1) {
						if (markerTab == 0) {
							if (lsTarget.useBones) {
								foreach (BoneShape boneshape in lsTarget.phonemes[LipSyncEditorExtensions.oldToggle].bones) {
									if (boneshape.bone != null) {
										boneshape.bone.localPosition = boneshape.neutralPosition;
										boneshape.bone.localEulerAngles = boneshape.neutralRotation;
									}
								}
							}

							foreach (PhonemeShape shape in lsTarget.phonemes) {
								foreach (int blendable in shape.blendShapes) {
									lsTarget.blendSystem.SetBlendableValue(blendable, 0);
								}
							}
						}
					}

					if (LipSyncEditorExtensions.currentToggle > -1) {
						if (markerTab == 0) {
							if (lsTarget.useBones) {
								foreach (BoneShape boneshape in lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].bones) {
									if (boneshape.bone != null) {
										boneshape.bone.localPosition = boneshape.endPosition;
										boneshape.bone.localEulerAngles = boneshape.endRotation;
									}
								}
							}

							for (int b = 0; b < lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].blendShapes.Count; b++) {
								lsTarget.blendSystem.SetBlendableValue(lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].blendShapes[b], lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].weights[b]);
							}
						}
					}

					LipSyncEditorExtensions.oldToggle = LipSyncEditorExtensions.currentToggle;
				}

				if (GUI.changed) {
					if (blendables == null) {
						GetBlendShapes();
					}

					if (LipSyncEditorExtensions.currentToggle > -1 && LipSyncEditorExtensions.currentTarget == lsTarget) {
						if (markerTab == 0) {
							for (int b = 0; b < lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].blendShapes.Count; b++) {
								lsTarget.blendSystem.SetBlendableValue(lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].blendShapes[b], lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].weights[b]);
							}
						}
					}

					EditorUtility.SetDirty(lsTarget);
					serializedTarget.SetIsDifferentCacheDirty();
				}
			}else{
				EditorGUILayout.HelpBox(lsTarget.blendSystem.notReadyMessage , MessageType.Warning);
			}
		}
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndVertical();

		if(saving){
			GUI.Box(new Rect(40 , fullheight.y+(fullheight.height/2)-60 , fullheight.width-80 , 120) , "" , (GUIStyle)"flow node 0");
			GUI.Box(new Rect(50 , fullheight.y+(fullheight.height/2)-50 , fullheight.width-100 , 20) , "Create New Preset" , EditorStyles.label);
			GUI.Box(new Rect(50 , fullheight.y+(fullheight.height/2)-20 , 80 , 20) , "Preset Path" , EditorStyles.label);
			savingName = EditorGUI.TextField(new Rect(140 , fullheight.y+(fullheight.height/2)-20 , fullheight.width-290 , 20) , "" , savingName);

			if(GUI.Button(new Rect(fullheight.width-140 , fullheight.y+(fullheight.height/2)-20 , 80 , 20) , "Browse")){
				GUI.FocusControl("");
				string newPath = EditorUtility.SaveFilePanelInProject("Chose Preset Location" , "New Preset" , "asset" , "");

				if(newPath != "") {
					savingName = newPath.Substring("Assets/".Length);
				}
			}
			if(GUI.Button(new Rect(100 , fullheight.y+(fullheight.height/2)+15 , (fullheight.width/2)-110 , 25) , "Cancel")){
				GUI.FocusControl("");
				savingName = "";
				saving = false;
			}
			if(GUI.Button(new Rect((fullheight.width/2)+10 , fullheight.y+(fullheight.height/2)+15 , (fullheight.width/2)-110 , 25) , "Save")){
				if(!Path.GetDirectoryName(savingName).Contains("Presets")){
					EditorUtility.DisplayDialog("Invalid Path" , "Presets must be saved in a folder called Presets, or a subfolder of one." , "OK");
					return;
				}else if(!Directory.Exists(Application.dataPath+"/"+Path.GetDirectoryName(savingName))){
					EditorUtility.DisplayDialog("Directory Does Not Exist" , "The directory "+Path.GetDirectoryName(savingName)+" does not exist." , "OK");
					return;
				}else if(!Path.HasExtension(savingName)){
					savingName = Path.GetDirectoryName(savingName)+"/"+Path.GetFileNameWithoutExtension(savingName)+".asset";
				}else if(Path.GetExtension(savingName) != ".asset"){
					savingName = Path.GetDirectoryName(savingName)+"/"+Path.GetFileNameWithoutExtension(savingName)+".asset";
				}

				LipSyncPreset preset = ScriptableObject.CreateInstance<LipSyncPreset>();
				preset.phonemeShapes = new LipSyncPreset.PhonemeShapeInfo[lsTarget.phonemes.Count];

				// Add phonemes
				for (int p = 0; p < lsTarget.phonemes.Count; p ++){
					LipSyncPreset.PhonemeShapeInfo phonemeInfo = new LipSyncPreset.PhonemeShapeInfo();
					phonemeInfo.phoneme = lsTarget.phonemes[p].phoneme;
					phonemeInfo.blendables = new LipSyncPreset.BlendableInfo[lsTarget.phonemes[p].blendShapes.Count];
					phonemeInfo.bones = new LipSyncPreset.BoneInfo[lsTarget.phonemes[p].bones.Count];

					// Add blendables
					for (int b = 0; b < lsTarget.phonemes[p].blendShapes.Count; b++) {
						LipSyncPreset.BlendableInfo blendable = new LipSyncPreset.BlendableInfo();
						blendable.blendableNumber = lsTarget.phonemes[p].blendShapes[b];
						blendable.blendableName = blendables[lsTarget.phonemes[p].blendShapes[b]];
						blendable.weight = lsTarget.phonemes[p].weights[b];

						phonemeInfo.blendables[b] = blendable;
					}

					// Add bones
					for (int b = 0; b < lsTarget.phonemes[p].bones.Count; b++) {
						LipSyncPreset.BoneInfo bone = new LipSyncPreset.BoneInfo();
						bone.name = lsTarget.phonemes[p].bones[b].bone.name;
						bone.localPosition = lsTarget.phonemes[p].bones[b].endPosition;
						bone.localRotation = lsTarget.phonemes[p].bones[b].endRotation;
						bone.lockPosition = lsTarget.phonemes[p].bones[b].lockPosition;
						bone.lockRotation = lsTarget.phonemes[p].bones[b].lockRotation;

						string path = "";
						Transform level = lsTarget.phonemes[p].bones[b].bone.parent;
						while (level != null) {
							path += level.name+"/";
							level = level.parent;
						}
						bone.path = path;

						phonemeInfo.bones[b] = bone;
					}

					preset.phonemeShapes[p] = phonemeInfo;
				}

				AssetDatabase.CreateAsset(preset , "Assets/" + savingName);
				AssetDatabase.Refresh();
				savingName = "";
				saving = false;
			}
		}

		serializedTarget.ApplyModifiedProperties();
	}

	

	void DrawBlendSystemButtons (){
		foreach(BlendSystemButton.Reference button in blendSystemButtons) {
			if(GUILayout.Button(button.displayName , GUILayout.Height(20) , GUILayout.MinWidth(120))) {
				button.method.Invoke(lsTarget.blendSystem , null);
			}
		}
	}

	BlendSystemButton.Reference[] GetBlendSystemButtons () {
		MethodInfo[] methods = lsTarget.blendSystem.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance);
		BlendSystemButton.Reference[] buttons = new BlendSystemButton.Reference[0];

		int buttonLength = 0;
		for(int m = 0 ; m < methods.Length ; m++) {
			BlendSystemButton[] button = (BlendSystemButton[])methods[m].GetCustomAttributes(typeof(BlendSystemButton) , false);
			if(button.Length > 0){
				buttonLength++;
			}
		}

		if(buttonLength > 0) {
			buttons = new BlendSystemButton.Reference[buttonLength];
			int b = 0;
			for(int m = 0 ; m < methods.Length ; m++) {
				BlendSystemButton[] button = (BlendSystemButton[])methods[m].GetCustomAttributes(typeof(BlendSystemButton) , false);
				if(button.Length > 0){
					buttons[b] = new BlendSystemButton.Reference(button[0].displayName , methods[m]);
					b++;
				}
			}
		}

		return buttons;
	}

	void OnSceneGUI () {
		if (markerTab == 0 && LipSyncEditorExtensions.currentToggle >= 0) {
			Handles.BeginGUI();
			GUI.Box(new Rect(Screen.width - 256, Screen.height - 246, 256, 256), guides[LipSyncEditorExtensions.currentToggle], GUIStyle.none);
			Handles.EndGUI();
		}

		// Bone Handles
		if (lsTarget.useBones && LipSyncEditorExtensions.currentToggle >= 0 && LipSyncEditorExtensions.currentTarget == lsTarget) {
			BoneShape bone = null;
			if (markerTab == 0) {
				if (LipSyncEditorExtensions.selectedBone < lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].bones.Count && lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].bones.Count > 0) {
					bone = lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].bones[LipSyncEditorExtensions.selectedBone];
				} else {
					return;
				}
			}
			if (bone.bone == null)
				return;

			if (Tools.current == Tool.Move) {
				Undo.RecordObject(bone.bone, "Move");

				Vector3 change = Handles.PositionHandle(bone.bone.position, bone.bone.rotation);
				if (change != bone.bone.position) {
					bone.bone.position = change;
					bone.endPosition = bone.bone.localPosition;
				}
			} else if (Tools.current == Tool.Rotate) {
				Undo.RecordObject(bone.bone, "Rotate");
				Quaternion change = Handles.RotationHandle(bone.bone.rotation, bone.bone.position);
				if (change != bone.bone.rotation) {
					bone.bone.rotation = change;
					bone.endRotation = bone.bone.localEulerAngles;
				}
			} else if (Tools.current == Tool.Scale) {
				Undo.RecordObject(bone.bone, "Scale");
				Vector3 change = Handles.ScaleHandle(bone.bone.localScale, bone.bone.position, bone.bone.rotation, HandleUtility.GetHandleSize(bone.bone.position));
				if (change != bone.bone.localScale) {
					bone.bone.localScale = change;
				}
			}
			
		}
	}

	void LoadPreset (object data) {
		string file = (string)data;
		if(file.EndsWith(".asset" , true , null)){
			LipSyncPreset preset = AssetDatabase.LoadAssetAtPath<LipSyncPreset>("Assets" + file.Substring((Application.dataPath).Length));

			if(preset != null){
				List<PhonemeShape> newPhonemes = new List<PhonemeShape>();

				// Phonemes
				for (int shape = 0; shape < preset.phonemeShapes.Length; shape++) {
					newPhonemes.Add(new PhonemeShape(preset.phonemeShapes[shape].phoneme));

					for (int blendable = 0; blendable < preset.phonemeShapes[shape].blendables.Length; blendable++) {
						int finalBlendable = preset.FindBlendable(preset.phonemeShapes[shape].blendables[blendable], lsTarget.blendSystem);
						if (finalBlendable >= 0) {
							newPhonemes[shape].blendShapes.Add(finalBlendable);
							newPhonemes[shape].weights.Add(preset.phonemeShapes[shape].blendables[blendable].weight);
						}
					}

					for (int bone = 0; bone < preset.phonemeShapes[shape].bones.Length; bone++) {
						BoneShape newBone = new BoneShape();
						newBone.bone = preset.FindBone(preset.phonemeShapes[shape].bones[bone] , lsTarget.transform);
						newBone.SetNeutral();
						newBone.endPosition = preset.phonemeShapes[shape].bones[bone].localPosition;
						newBone.endRotation = preset.phonemeShapes[shape].bones[bone].localRotation;
						newBone.lockPosition = preset.phonemeShapes[shape].bones[bone].lockPosition;
						newBone.lockRotation = preset.phonemeShapes[shape].bones[bone].lockRotation;

						newPhonemes[shape].bones.Add(newBone);
					}
				}

				lsTarget.phonemes = newPhonemes;

				for(int bShape = 0 ; bShape < lsTarget.blendSystem.blendableCount ; bShape++){
					lsTarget.blendSystem.SetBlendableValue(bShape , 0);
				}

				if(markerTab == 0){
					if (LipSyncEditorExtensions.currentToggle >= 0) {
						int b=0;
						foreach (int shape in lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].blendShapes) {
							lsTarget.blendSystem.SetBlendableValue(shape, lsTarget.phonemes[LipSyncEditorExtensions.currentToggle].weights[b]);
							b++;
						}
					}
				}
			}
		}
	}

	void NewPreset () {
		saving = true;
		savingName = "Rogo Digital/LipSync/Presets/New Preset.asset";
	}

	void GetBlendShapes () {
		if(lsTarget.blendSystem.isReady){
			blendables = lsTarget.blendSystem.GetBlendables();
		}
	}

	void FindBlendSystems() {
		blendSystems = new List<System.Type>();
		blendSystemNames = new List<string>();

		blendSystems.Add(null);
		blendSystemNames.Add("None");

		foreach (System.Type t in typeof(BlendshapeBlendSystem).Assembly.GetTypes()) {
			if (t.IsSubclassOf(typeof(BlendSystem))) {
				blendSystems.Add(t);
				blendSystemNames.Add(LipSyncEditorExtensions.AddSpaces(t.Name));
			}
		}

		if (lsTarget.blendSystem != null) {
			blendSystemNumber = blendSystems.IndexOf(lsTarget.blendSystem.GetType());
		}
	}

	void CreateBlendSystemEditor() {
		if (lsTarget.blendSystem != null) {
			lsTarget.blendSystem = lsTarget.GetComponent<BlendSystem>();
			if (lsTarget.blendSystem != null) {
				blendSystemEditor = (BlendSystemEditor)Editor.CreateEditor(lsTarget.blendSystem);
			}
		}
	}

	public static void FixedEndFadeGroup (float value) {
		if(value == 0f || value == 1f) {
			return;
		}
		EditorGUILayout.EndFadeGroup();
	}

	private void AutoUpdate (float oldVersion) {
		// Used for additional future-proofing
		switch(oldVersion.ToString()) {
		case "0":
			// Anything Pre-0.6
			if(EditorUtility.DisplayDialog("LipSync has been updated." , "This character was last used with an old version of LipSync prior to 0.6. The recommended values for Rest Time and Pre-Rest Hold Time have been changed to 0.2 and 0.1 respectively. Do you want to change these values automatically?" , "Yes" , "No")){
				lsTarget.restTime = 0.2f;
				lsTarget.restHoldTime = 0.1f;
			}
			break;
		}
	}
}