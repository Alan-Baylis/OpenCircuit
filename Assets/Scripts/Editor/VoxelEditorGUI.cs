using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Vox.VoxelEditor))]
public class VoxelEditorGUI : Editor {

	protected const string numForm = "##,0.000";
	protected const string numFormInt = "##,#";
	protected static readonly GUIContent[] modes = {new GUIContent("Manage"), new GUIContent("Sculpt"), new GUIContent("Mask")};
	protected static readonly GUIContent[] brushes = {new GUIContent("Sphere"), new GUIContent("Rectangle"), new GUIContent("Smooth")};
	protected static readonly GUIContent[] generationModes = {new GUIContent("Flat"), new GUIContent("Sphere"), new GUIContent("Procedural"), new GUIContent("Heightmaps")};
	
	private GUIStyle labelBigFont = null;
	private GUIStyle foldoutBigFont = null;
	private GUIStyle buttonBigFont = null;
	private GUIStyle tabsBigFont = null;

	// generation parameters
	private bool setupGeneration;
	private int selectedGenerationMode;
	private bool showSubstances;

	private bool showMasks;
	private bool showStatistics;
    private VoxelEditorParameters generationParameters;

	[MenuItem("GameObject/3D Object/Voxel Object")]
	public static void createVoxelObject() {
		Vox.VoxelEditor.createEmpty();
	}

	public void OnEnable() {
		setupGeneration = false;
		showSubstances = false;
		showMasks = true;
		showStatistics = false;
	}

	public override void OnInspectorGUI() {
		labelBigFont = new GUIStyle(GUI.skin.label);
		labelBigFont.margin = new RectOffset(labelBigFont.margin.left, labelBigFont.margin.right, labelBigFont.margin.top +10, labelBigFont.margin.bottom);
		labelBigFont.fontSize = 16;
		foldoutBigFont = new GUIStyle(EditorStyles.foldout);
		foldoutBigFont.margin = new RectOffset(foldoutBigFont.margin.left, foldoutBigFont.margin.right, foldoutBigFont.margin.top +10, foldoutBigFont.margin.bottom);
		foldoutBigFont.fontSize = 16;
		foldoutBigFont.alignment = TextAnchor.LowerLeft;
		buttonBigFont = new GUIStyle(GUI.skin.button);
		buttonBigFont.fontSize = 14;
		tabsBigFont = new GUIStyle(GUI.skin.button);
		tabsBigFont.fixedHeight = 30;
		
		Vox.VoxelEditor editor = (Vox.VoxelEditor)target;

		if (editor.generating()) {
			string label = "Generating Skin...";
			EditorGUI.ProgressBar(GUILayoutUtility.GetRect(new GUIContent(label), GUI.skin.button),
				VoxelEditorProgressController.getGenerationProgress(editor), label);
			Repaint();
			return;
		} else if (editor.meshGenTime > 0) {
			MonoBehaviour.print("Total Mesh Gen Time: " +editor.meshGenTime);
			MonoBehaviour.print("Total Mesh Gen Array Fill Time: " +editor.meshGenArrayTime);
			MonoBehaviour.print("Total Mesh Gen Count: " +editor.meshGenCount);
			MonoBehaviour.print("Mesh Gen Average: " +editor.meshGenTime /editor.meshGenCount);
			MonoBehaviour.print("Total Mesh Apply Time: " +editor.meshApplyTime);
			MonoBehaviour.print("Total Mesh Apply Count: " +editor.meshApplyCount);
			MonoBehaviour.print("Mesh Apply Average: " +editor.meshApplyTime /editor.meshApplyCount);
			editor.meshGenTime = 0;
			editor.meshGenArrayTime = 0;
			editor.meshGenCount = 0;
			editor.meshApplyTime = 0;
			editor.meshApplyCount = 0;
		}

		serializedObject.Update();

		if (setupGeneration) {
			doGenerationGUI(editor);
			return;
		} else {
			if (!editor.hasVoxelData())
				GUI.enabled = false;
			editor.selectedMode = GUILayout.Toolbar(editor.selectedMode, modes, tabsBigFont, GUILayout.Height(30));
			GUI.enabled = true;

			switch (editor.selectedMode) {
			case 0:
				doManageGUI(editor);
				break;
			case 1:
				doSculptGUI(editor);
				break;
			case 2:
				doMaskGUI(editor);
				break;
			}
		}

		// finally, apply the changes
		serializedObject.ApplyModifiedProperties();
	}

	public void OnSceneGUI() {
		Vox.VoxelEditor editor = (Vox.VoxelEditor)target;
		if (editor.selectedMode != 1)
			return;

		int controlId = GUIUtility.GetControlID(FocusType.Passive);
		switch(Event.current.GetTypeForControl(controlId)) {
		case EventType.MouseDown:
			if (Event.current.button == 0) {
				GUIUtility.hotControl = controlId;
				applyBrush(editor, HandleUtility.GUIPointToWorldRay(Event.current.mousePosition));
				Event.current.Use();
			}
			break;

		case EventType.MouseUp:
			if (Event.current.button == 0) {
				GUIUtility.hotControl = 0;
				Event.current.Use();
			}
			break;
		case EventType.MouseMove:
			SceneView.RepaintAll();
			break;
		case EventType.KeyDown:
			//TODO: I'm not sure what this does, but my code needs an equivalent -Brian
//			if (UnityEngine.Event.current.keyCode == KeyCode.Escape)
//				editor.pathPoints = null;
			break;
		}
	}

	protected void doMaskGUI(Vox.VoxelEditor editor) {
		editor.maskDisplayAlpha = doSliderFloatField("Mask Display Transparency", editor.maskDisplayAlpha, 0, 1);

		// mask list
		showMasks = doBigFoldout(showMasks, "Masks");
		if (showMasks) {
			SerializedProperty voxelMasks = serializedObject.FindProperty("masks");
			// EditorGUILayout.PropertyField(voxelMasks, new GUIContent("Sculpting Masks"), true);
			InspectorList.doArrayGUISimple(ref voxelMasks);
		}
	}

	protected void doSculptGUI(Vox.VoxelEditor editor) {
		// brush ghost
		editor.ghostBrushAlpha = doSliderFloatField("Brush Ghost Opacity", editor.ghostBrushAlpha, 0, 1);
		
		editor.gridEnabled = EditorGUILayout.Toggle("Snap to Grid", editor.gridEnabled);
        if (editor.gridEnabled) {
            ++EditorGUI.indentLevel;
            editor.gridUseVoxelUnits = EditorGUILayout.Toggle("Use Voxel Units", editor.gridUseVoxelUnits);
			if (editor.gridUseVoxelUnits) {
				float voxelSize = editor.width / (1 << editor.maximumDetail);
                editor.gridSize = EditorGUILayout.FloatField("Grid Spacing (Voxels)", editor.gridSize /voxelSize) *voxelSize;
			} else {
                editor.gridSize = EditorGUILayout.FloatField("Grid Spacing (Meters)", editor.gridSize);
            }
			--EditorGUI.indentLevel;
        }

        // brush list
		GUILayout.Label("Brush", labelBigFont);
        editor.selectedBrush = GUILayout.Toolbar(editor.selectedBrush, brushes, GUILayout.MinHeight(20));

		// brush substance type
		string[] substances = new string[editor.voxelSubstances.Length];
		for(int i=0; i<substances.Length; ++i)
			substances[i] = editor.voxelSubstances[i].name;

		// brush size
		switch(editor.selectedBrush) {
		case 0:
            GUILayout.Label("Hold 'Shift' to subtract.");
			editor.sphereBrushSize = doSliderFloatField("Sphere Radius (m)", editor.sphereBrushSize, 0, maxBrushSize(editor));
			editor.sphereSubstanceOnly = GUILayout.Toggle(editor.sphereSubstanceOnly, "Change Substance Only");
			GUILayout.Label("Substance", labelBigFont);
			editor.sphereBrushSubstance = (byte)GUILayout.SelectionGrid(editor.sphereBrushSubstance, substances, 1);
			break;

		case 1:
			GUILayout.Label("Hold 'Shift' to subtract.");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Dimensions (m)");
			editor.cubeBrushDimensions.x = EditorGUILayout.FloatField(editor.cubeBrushDimensions.x);
			editor.cubeBrushDimensions.y = EditorGUILayout.FloatField(editor.cubeBrushDimensions.y);
			editor.cubeBrushDimensions.z = EditorGUILayout.FloatField(editor.cubeBrushDimensions.z);
			GUILayout.EndHorizontal();
			
			editor.cubeSubstanceOnly = GUILayout.Toggle(editor.cubeSubstanceOnly, "Change Substance Only");
			GUILayout.Label("Substance", labelBigFont);
			editor.cubeBrushSubstance = (byte)GUILayout.SelectionGrid(editor.cubeBrushSubstance, substances, 1);
			break;

		case 2:
			editor.smoothBrushSize = doSliderFloatField("Radius (m)", editor.smoothBrushSize, 0, maxBrushSize(editor));
			editor.smoothBrushStrength = doSliderFloatField("Strength", editor.smoothBrushStrength, 0, 5);
			editor.smoothBrushBlurRadius = EditorGUILayout.IntField("Blur Radius", editor.smoothBrushBlurRadius);
			break;
		}

		// PATH GUI
		if(editor.selectedBrush != 0 && editor.selectedBrush != 1)
			return;
		GUILayout.Label("Path Tool", labelBigFont);

		editor.currentBrushGroup =
			(GameObject) EditorGUILayout.ObjectField("Brush group", editor.currentBrushGroup, typeof(GameObject), true);
		if (editor.currentBrushGroup != null && editor.currentBrushGroup.transform.childCount > 0) {
			if (GUILayout.Button("Clear Path")) {
				editor.currentBrushGroup = null;
			} else if (GUILayout.Button("Apply Path")) {
				Vox.LocalMutator mut = (Vox.LocalMutator)buildMutator(editor, editor.currentBrushGroup.transform.GetChild(0).position);
				if (editor.currentBrushGroup.transform.childCount > 1) {
					Vector3[] points = new Vector3[editor.currentBrushGroup.transform.childCount];
					for (int i = 0; i < editor.currentBrushGroup.transform.childCount; ++i) {
						points[i] = editor.currentBrushGroup.transform.GetChild(i).position;
					}


					new Vox.LineMutator(points, mut).apply(editor);
				} else {
					mut.apply(editor);
				}
			}
		} else {
			GUILayout.Label("Hold 'Control' to start a path.");

		}
	}

	protected void doManageGUI(Vox.VoxelEditor editor) {

		// actions
		GUILayout.Label ("Actions", labelBigFont);
		if (GUILayout.Button("Generate New")) {
			setupGeneration = true;
			generationParameters = new VoxelEditorParameters();
            generationParameters.setFrom(editor);
        }
		if (editor.hasVoxelData()) {
			if (GUILayout.Button("Erase")) {
				if (EditorUtility.DisplayDialog("Erase Voxels?", "Are you sure you want to erase all voxel data?", "Erase", "Cancel")) {
					editor.wipe();
				}
			}
			if (GUILayout.Button(editor.hasRenderers()? "Reskin": "Skin", buttonBigFont) && validateSubstances(editor)) {
				editor.generateRenderers();
			}
			if (editor.hasRenderers() && GUILayout.Button("Clear Skin") && validateSubstances(editor)) {
				editor.clearRenderers();
			}
			if (GUILayout.Button("Compress Data")) {
				Vox.Voxel v;
				int count = editor.getHead().canSimplify(out v);
				editor.dirty = true;
				EditorUtility.DisplayDialog("Compression Result", "Compression removed " +count +" nodes from the voxel tree.", "OK");
			}
			if (GUILayout.Button("Clean Artifacts")) {
				Vox.Voxel v;
				int count = editor.getHead().cleanArtifacts(out v, editor.getHead(), 0, editor.maximumDetail, 0, 0, 0);
				editor.dirty = true;
				EditorUtility.DisplayDialog("Artifact Cleaning Result", "Artifact cleaning removed " +count +" artifacts from the voxel tree.", "OK");
			}
			if (GUILayout.Button("Export")) {
				editor.export(EditorUtility.SaveFilePanel("Choose File to Export To", "", "Voxels", "vox"));
			}
		}
		if (GUILayout.Button("Import")) {
			if (!editor.import(EditorUtility.OpenFilePanel("Choose File to Import From", "", "vox"))) {
				EditorUtility.DisplayDialog("Wrong Voxel Format", "The file you chose was an unknown or incompatible voxel format version.", "OK");
			}
		}

		if (!editor.hasVoxelData())
			return;

		GUILayout.Label("Properties", labelBigFont);

		doGeneralPropertiesGUI(editor);

		// TODO: implement LOD and uncomment this
//		// LOD
//		SerializedProperty useLod = ob.FindProperty("useLod");
//		EditorGUILayout.PropertyField(useLod, new GUIContent("Use Level of Detail"));
//		if (useLod.boolValue) {
//			++EditorGUI.indentLevel;
//			SerializedProperty lodDetail = ob.FindProperty("lodDetail");
//			EditorGUILayout.PropertyField(lodDetail, new GUIContent("Target Level of Detail"));
//			if (lodDetail.floatValue > 1000)
//				lodDetail.floatValue = 1000;
//			else if (lodDetail.floatValue < 0.1f)
//				lodDetail.floatValue = 0.1f;
//
//			SerializedProperty curLodDetail = ob.FindProperty("curLodDetail");
//			if (Application.isPlaying) {
//				EditorGUILayout.PropertyField(curLodDetail, new GUIContent("Current Level of Detail"));
//			} else {
//				EditorGUILayout.PropertyField(curLodDetail, new GUIContent("Starting Level of Detail"));
//			}
//
//			if (curLodDetail.floatValue > 1000)
//				curLodDetail.floatValue = 1000;
//			else if (curLodDetail.floatValue < 0.1f)
//				curLodDetail.floatValue = 0.1f;
//			--EditorGUI.indentLevel;
//		}

		// do substances
		doSubstancesGUI(serializedObject);


		// show statistics
		showStatistics = doBigFoldout(showStatistics, "Statistics");
		if (showStatistics) {
			EditorGUILayout.LabelField("Chunk Count: " + editor.renderers.Count);
			doTreeSizeGUI(editor);
			EditorGUILayout.LabelField("Vertex Count: " + editor.vertexCount);
			EditorGUILayout.LabelField("Triangle Count: " + editor.triangleCount);
		}
	}

	protected void doGenerationGUI(Vox.VoxelEditor editor) {

		GUILayout.Label ("Properties", labelBigFont);
		doTreeSizeGUI(generationParameters);

		// general properties
		doGeneralPropertiesGUI(generationParameters);

		// substances
        doSubstancesGUI(serializedObject);

        // generation mode
        GUILayout.Label("Generation Mode", labelBigFont);
		selectedGenerationMode = GUILayout.Toolbar(selectedGenerationMode, generationModes);
		switch(selectedGenerationMode) {
		case 0:
			doFlatGenerationGUI();
			break;
		case 1:
			doSphereGenerationGUI();
			break;
		case 2:
			doProceduralGenerationGUI();
			break;
		case 3:
			doHeightmapGenerationGUI();
			break;
		}

		// confirmation
		GUILayout.Label ("Confirmation", labelBigFont);
		if (GUILayout.Button("Generate", buttonBigFont) && validateSubstances(editor)) {
			if (EditorUtility.DisplayDialog("Generate Voxels?", "Are you sure you want to generate the voxel terain from scratch?  Any previous work will be overriden.", "Generate", "Cancel")) {
				generateVoxels(editor);
			}
		}
		if (GUILayout.Button("Cancel Generation", buttonBigFont)) {
			setupGeneration = false;
		}
		EditorGUILayout.Separator();
	}

	protected void doTreeSizeGUI(Vox.VoxelEditor editor) {
		// world detail
		EditorGUILayout.LabelField("Voxel Power", editor.maximumDetail.ToString());

		long dimension = 1 << editor.maximumDetail;
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Voxels Per Side", dimension.ToString(numFormInt));
		EditorGUILayout.LabelField("Max Voxel Count", Mathf.Pow(dimension, 3).ToString(numFormInt));
		--EditorGUI.indentLevel;
		EditorGUILayout.Separator();

		// world dimension
		EditorGUILayout.LabelField("World Size (m)", editor.width.ToString());
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("World Area", Mathf.Pow(editor.width / 1000, 2).ToString(numForm) + " square km");
		EditorGUILayout.LabelField("World Volume", Mathf.Pow(editor.width / 1000, 3).ToString(numForm) + " cubic km");
		--EditorGUI.indentLevel;
		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Voxel Size", (editor.width / dimension).ToString(numForm) + " m");
		EditorGUILayout.Separator();
	}

	protected void doTreeSizeGUI(VoxelEditorParameters editor) {
		// world detail
		int maxDetail = EditorGUILayout.IntField("Voxel Power", editor.maxDetail);
		if (maxDetail > 16)
			maxDetail = 16;
		else if (maxDetail < 4)
			maxDetail = 4;
		editor.maxDetail = (byte)maxDetail;

		long dimension = 1 << editor.maxDetail;
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Voxels Per Side", dimension.ToString(numFormInt));
		EditorGUILayout.LabelField("Max Voxel Count", Mathf.Pow(dimension, 3).ToString(numFormInt));
		--EditorGUI.indentLevel;
		EditorGUILayout.Separator();

		// world dimension
		editor.baseSize = EditorGUILayout.FloatField("World Size (m)", editor.baseSize);
		if (editor.baseSize < 0)
			editor.baseSize = 0;
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("World Area", Mathf.Pow(editor.baseSize / 1000, 2).ToString(numForm) + " square km");
		EditorGUILayout.LabelField("World Volume", Mathf.Pow(editor.baseSize / 1000, 3).ToString(numForm) + " cubic km");
		--EditorGUI.indentLevel;
		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Voxel Size", (editor.baseSize / dimension).ToString(numForm) + " m");
		EditorGUILayout.Separator();
	}

	protected void doSubstancesGUI(SerializedObject ob) {
		SerializedProperty voxelSubstances = ob.FindProperty("voxelSubstances");
		showSubstances = doBigFoldout(showSubstances, "Substances");
		if (showSubstances)
			InspectorList.doArrayGUISimple(ref voxelSubstances);
		ob.ApplyModifiedProperties();
	}

	protected void doGeneralPropertiesGUI(Vox.VoxelEditor editor) {
		bool createColliders = EditorGUILayout.Toggle(new GUIContent("Generate Colliders"), editor.createColliders);
		bool useStaticMeshes = EditorGUILayout.Toggle(new GUIContent("Use Static Meshes"), editor.useStaticMeshes);
		bool saveMeshes = EditorGUILayout.Toggle(new GUIContent("Save Meshes To Scene"), editor.saveMeshes);
		bool reduceMeshes = EditorGUILayout.Toggle(new GUIContent("Reduce Meshes"), editor.reduceMeshes);
		float reductionAmount = editor.reductionAmount;
		if (editor.reduceMeshes) {
			reductionAmount = doSliderFloatField("Mesh Reduction Level", editor.reductionAmount, 0, 0.5f);
		}
		byte maxDetail = (byte)EditorGUILayout.IntField(new GUIContent("Voxel Power"), editor.maximumDetail);
		if (maxDetail != editor.maximumDetail || createColliders != editor.createColliders ||
			saveMeshes != editor.saveMeshes || reductionAmount != editor.reductionAmount ||
			useStaticMeshes != editor.useStaticMeshes || reduceMeshes != editor.reduceMeshes) {
			if (maxDetail != editor.maximumDetail) {
				editor.maximumDetail = maxDetail;
				editor.setupLookupTables();
			}
			editor.createColliders = createColliders;
			editor.useStaticMeshes = useStaticMeshes;
			editor.saveMeshes = saveMeshes;
            editor.reduceMeshes = reduceMeshes;
			editor.reductionAmount = reductionAmount;
			editor.clearRenderers();
		}
	}

	protected void doGeneralPropertiesGUI(VoxelEditorParameters editor) {
		editor.createColliders = EditorGUILayout.Toggle(new GUIContent("Generate Colliders"), editor.createColliders);
		editor.useStaticMeshes = EditorGUILayout.Toggle(new GUIContent("Use Static Meshes"), editor.useStaticMeshes);
		editor.saveMeshes = EditorGUILayout.Toggle(new GUIContent("Save Meshes To Scene"), editor.saveMeshes);
		editor.reduceMeshes = EditorGUILayout.Toggle(new GUIContent("Reduce Meshes"), editor.reduceMeshes);
		if (editor.reduceMeshes) {
			editor.reductionAmount = doSliderFloatField("Mesh Reduction Level", editor.reductionAmount, 0, 0.5f);
		}
	}

	protected void doFlatGenerationGUI() {
		generationParameters.heightPercentage = System.Math.Max(System.Math.Min(
			EditorGUILayout.FloatField("Height Percentage", generationParameters.heightPercentage), 100f), 0f);
	}

	protected void doSphereGenerationGUI() {
		generationParameters.spherePercentage = System.Math.Max(System.Math.Min(
			EditorGUILayout.FloatField("Sphere Radius Percentage", generationParameters.spherePercentage), 100f), 0f);
    }

	protected void doProceduralGenerationGUI() {
		generationParameters.maxChange = EditorGUILayout.FloatField("Roughness", generationParameters.maxChange);
		if (generationParameters.maxChange > 5)
			generationParameters.maxChange = 5;
		else if (generationParameters.maxChange < 0.01f)
			generationParameters.maxChange = 0.01f;
        generationParameters.proceduralSeed = EditorGUILayout.IntField("Random Seed", generationParameters.proceduralSeed);
    }

    protected void doHeightmapGenerationGUI() {
        serializedObject.Update();

		SerializedProperty heightmaps = serializedObject.FindProperty("heightmaps");
		EditorGUILayout.PropertyField(heightmaps, new GUIContent("Height Maps"), true);
		SerializedProperty heightmapSubstances = serializedObject.FindProperty("heightmapSubstances");
		EditorGUILayout.PropertyField(heightmapSubstances, new GUIContent("Height Map Substances"), true);
        serializedObject.ApplyModifiedProperties();
        generationParameters.heightmaps = ((Vox.VoxelEditor)this.target).heightmaps;
        generationParameters.heightmapSubstances = ((Vox.VoxelEditor)this.target).heightmapSubstances;

    }

    public void generateVoxels(Vox.VoxelEditor editor) {
        editor.wipe();
        generationParameters.setTo(editor);
        editor.initialize();
        switch (selectedGenerationMode) {
        case 0:
            editor.setToHeight();
            break;
        case 1:
			editor.setToSphere();
            break;
        case 2:
            editor.setToProcedural();
            break;
		case 3:
			editor.heightmaps = generationParameters.heightmaps;
			editor.heightmapSubstances = generationParameters.heightmapSubstances;
			editor.setToHeightmap();
			break;
        }
        editor.generateRenderers();
        setupGeneration = false;
    }

    protected void applyBrush(Vox.VoxelEditor editor, Ray mouseLocation) {
		// get point clicked on
		System.Nullable<Vector3> point = editor.getBrushPoint(mouseLocation);
		if (point == null)
			return;

		// check if control pressed.  If so, add point to pathList
		if(editor.isPathing()) {
			editor.addPathPoint(point.Value);
			return;
		}

		// check for showPositionHandles
		if (editor.showPositionHandles && editor.isSelectedBrushPathable()
			&& editor.currentBrushGroup != null && editor.currentBrushGroup.transform.childCount > 0)
			return;

		// create mutator
		Vox.Mutator mutator = buildMutator(editor, point.Value);

		// apply mutator
		if (mutator == null)
			return;
		Vox.LocalMutator localMutator = mutator as Vox.LocalMutator;
		if (localMutator != null && editor.currentBrushGroup != null && editor.currentBrushGroup.transform.childCount > 0) {
			editor.addPathPoint(point.Value);

			Vector3[] points = new Vector3[editor.currentBrushGroup.transform.childCount];
			for (int i = 0; i < editor.currentBrushGroup.transform.childCount; ++i) {
				points[i] = editor.currentBrushGroup.transform.GetChild(i).position;
			}


			mutator = new Vox.LineMutator(points, localMutator);
			editor.currentBrushGroup = null;
		}
		mutator.apply(editor);
	}

	protected Vox.Mutator buildMutator(Vox.VoxelEditor editor, Vector3 point) {
		// check for subtraction mode
		byte opacity = byte.MaxValue;
		if (editor.isSubtracting()) {
			opacity = byte.MinValue;
		}

		// create mutator (and maybe apply)
		switch (editor.	selectedBrush) {
			case 0:
				Vox.SphereMutator sphereMod = new Vox.SphereMutator(point, editor.sphereBrushSize, new Vox.Voxel(editor.sphereBrushSubstance, opacity));
				sphereMod.overwriteShape = !editor.sphereSubstanceOnly;
				return sphereMod;
			case 1:
				Vox.CubeMutator cubeMod = new Vox.CubeMutator(editor, point, editor.cubeBrushDimensions, new Vox.Voxel(editor.cubeBrushSubstance, opacity), true);
				cubeMod.overwriteShape = !editor.cubeSubstanceOnly;
				return cubeMod;
			default:
				Vox.BlurMutator blurMod = new Vox.BlurMutator(editor, point, editor.smoothBrushSize, editor.smoothBrushStrength);
				blurMod.blurRadius = editor.smoothBrushBlurRadius;
				return blurMod;
		}
	}

	protected bool validateSubstances(Vox.VoxelEditor editor) {
		if (editor.voxelSubstances == null || editor.voxelSubstances.Length < 1) {
			EditorUtility.DisplayDialog("Invalid Substances", "There must be at least one voxel substance defined to generate the voxel object.", "OK");
			return false;
		}
		int i = 1;
		foreach(Vox.VoxelSubstance sub in editor.voxelSubstances) {
			if (sub.renderMaterial == null || sub.blendMaterial == null) {
				EditorUtility.DisplayDialog("Invalid Substance", "Substance " +i +", " +sub.name +", must have a render material and a blend material set.", "OK");
				return false;
			}
			++i;
		}
		return true;
	}

	protected static float maxBrushSize(Vox.VoxelEditor editor) {
		return Mathf.Max(editor.voxelSize() *20, 50);
    }

    protected class VoxelEditorParameters {
		public float baseSize = 32;
		public byte maxDetail = 6;
		// public byte isoLevel = 127;
		// public float lodDetail = 1;
		// public bool useLod = false;
		// public GameObject trees;
		// public float treeDensity = 0.02f;
		// public float treeSlopeTolerance = 5;
		// public float curLodDetail = 10f;
		public float spherePercentage;
		public float heightPercentage;
		public float maxChange;
        public int proceduralSeed;
        public bool createColliders = true;
		public bool useStaticMeshes = true;
		public bool saveMeshes = false;
		public bool reduceMeshes = false;
		public float reductionAmount = 0;
		public Texture2D[] heightmaps;
		public byte[] heightmapSubstances;

        public void setFrom(Vox.VoxelEditor editor) {
            baseSize = editor.width;
            maxDetail = editor.maximumDetail;
            maxChange = editor.maxChange;
            proceduralSeed = editor.proceduralSeed;
            createColliders = editor.createColliders;
            useStaticMeshes = editor.useStaticMeshes;
            saveMeshes = editor.saveMeshes;
            reduceMeshes = editor.reduceMeshes;
            reductionAmount = editor.reductionAmount;
			heightPercentage = editor.heightPercentage;
			spherePercentage = editor.spherePercentage;
        }

		public void setTo(Vox.VoxelEditor editor) {
			editor.width = baseSize;
            editor.maximumDetail = maxDetail;
			editor.maxChange = maxChange;
			editor.proceduralSeed = proceduralSeed;
            editor.createColliders = createColliders;
            editor.useStaticMeshes = useStaticMeshes;
            editor.saveMeshes = saveMeshes;
            editor.reduceMeshes = reduceMeshes;
            editor.reductionAmount = reductionAmount;
			editor.heightPercentage = heightPercentage;
			editor.spherePercentage = spherePercentage;
		}
	}

	protected bool doBigFoldout(bool foldedOut, string label) {
		return EditorGUI.Foldout(GUILayoutUtility.GetRect(new GUIContent(label), foldoutBigFont), foldedOut, label, true, foldoutBigFont);
	}

	protected float doSliderFloatField(string label, float value, float min, float max) {
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, GUILayout.ExpandWidth(false));
		float newValue = value;
		newValue = GUILayout.HorizontalSlider(newValue, min, max);
		newValue = Mathf.Max(Mathf.Min(EditorGUILayout.FloatField(newValue, GUILayout.MaxWidth(64)), max), min);
		GUILayout.EndHorizontal();
		return newValue;
	}

}
