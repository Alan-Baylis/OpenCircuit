using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FPSMeter : NetworkBehaviour {

	public uint targetFramerate = 60;
	public float opacity = 0.5f;
	public double trackRate = 0.3;

	private double deltaTime;
	private double msec;
	private double fps;
	private double barLevel;
//	private double aiTime;
	private Texture2D boxColor;
	private GUIStyle boxStyle;

	void Start () {
		boxColor = new Texture2D(1, 1);
	}
	
	void Update () {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
//		msec = deltaTime * 1000;
		fps = 1f / deltaTime;
		barLevel += (getFPSMeterPosition(fps, targetFramerate) -barLevel) *trackRate;
//		aiTime = ((Bases) GlobalConfig.globalConfig.gamemode).getRobotTiming() * 1000.0;
	}

	public void OnGUI() {
//		string text = string.Format("{0:0.0} ms ({1:0.} fps) {2:0.}ms/sec / {3:0.0}% AI ", msec, fps, aiTime, ((aiTime / fps)/msec) *100.0);
		string text = string.Format("{0:0.0} ms ({1:0.} fps) ", msec, fps);
		GUI.Label(new Rect(0, 0, 250, 20), text);

		drawFPSMeter(fps, 20);
	}

	public void drawFPSMeter(double fps, int yPos) {
		float meterTargetWidth = 100;
		float meterHeight = 18;
		yPos += 1;
		setBoxStyle();
		GUI.depth = -1;

		// draw background
		Color color = new Color(0.75f, 0.75f, 0.75f, opacity /2);
		drawBox(new Rect(0, yPos - 1, meterTargetWidth * 2, 2), color);
		drawBox(new Rect(0, yPos +meterHeight -1, meterTargetWidth * 2, 2), color);
		drawBox(new Rect(0, yPos, meterTargetWidth * 2, meterHeight), new Color(0, 0, 0, opacity / 2));

		// draw FPS level
		float fpsClamped = Mathf.Max(0, Mathf.Min(1, (float)barLevel) * 1.5f -0.5f);
		Color barColor = new Color(0.2f + 0.8f * (1 - fpsClamped), 0.2f + 0.8f * fpsClamped, 0.2f, opacity);
		drawBox(new Rect(0, yPos, (float)barLevel *meterTargetWidth, meterHeight), barColor);
		
		// draw target mark
		drawBox(new Rect(meterTargetWidth - 1, yPos, 2, meterHeight),
			new Color(0.75f, 0.75f, 0.75f, opacity));
	}

	
	private void drawBox(Rect position, Color color) {
		setBoxColor(color);
		GUI.Box(position, GUIContent.none, boxStyle);
	}
	
	private static double getFPSMeterPosition(double fps, double targetFPS) {
		if (fps < targetFPS)
			return fps /targetFPS;
		else
			return 2 - System.Math.Pow(2, -(fps - targetFPS) /targetFPS);
	}

	private void setBoxColor(Color color) {
		boxColor.SetPixel(0, 0, color);
		boxColor.Apply();
	}

	private void setBoxStyle() {
		if (boxStyle != null)
			return;
		boxStyle = new GUIStyle(GUI.skin.box);
		boxStyle.margin = new RectOffset(0, 0, 0, 0);
		boxStyle.padding = new RectOffset(0, 0, 0, 0);
		boxStyle.overflow = new RectOffset(0, 0, 0, 0);
		boxStyle.border = new RectOffset(0, 0, 0, 0);
		boxStyle.contentOffset = Vector2.zero;
		boxStyle.normal.background = boxColor;
	}
}
