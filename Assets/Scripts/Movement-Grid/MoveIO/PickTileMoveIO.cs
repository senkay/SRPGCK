using UnityEngine;
using System.Collections.Generic;

public class PickTileMoveIO : MoveIO {
	public bool supportKeyboard = true;
	public bool supportMouse = true;
	
	GridOverlay overlay;
	
	float firstClickTime = -1;
	float doubleClickThreshold = 0.3f;
	
	bool cycleIndicatorZ = false;
	float indicatorCycleT=0;
	public float indicatorCycleLength=1.0f;
	
	float lastIndicatorKeyboardMove=0;
	float indicatorKeyboardMoveThreshold=0.3f;
	
	Vector2 indicatorXY=Vector2.zero;
	float indicatorZ=0;
	
	override public void Start() {
		base.Start();
	}
	
	override public void Update () {
		base.Update();
		if(character == null || !character.isActive) { return; }
		if(!isActive) { return; }
		//self.isActive?
		if(supportMouse && Input.GetMouseButton(0)) {
			cycleIndicatorZ = false;
			Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 hitSpot;
			bool inside = overlay.Raycast(r, out hitSpot);
			hitSpot.x = Mathf.Floor(hitSpot.x)+0.5f;
			hitSpot.y = Mathf.Floor(hitSpot.y)+0.5f;
			hitSpot.z = Mathf.Floor(hitSpot.z+0.5f);
			indicatorXY = new Vector2(hitSpot.x, hitSpot.y);
			indicatorZ = hitSpot.z;
			if(Time.time-firstClickTime > doubleClickThreshold) {
				firstClickTime = Time.time;
			} else {
				firstClickTime = -1;
				Vector3 indicatorSpot = new Vector3(indicatorXY.x, indicatorXY.y, indicatorZ);
				if(Input.GetMouseButtonDown(0) && inside && overlay.ContainsPosition(indicatorSpot)) {
					PathNode pn = overlay.PositionAt(indicatorSpot);
					if(pn != null && pn.canStop) {
						PerformMove(indicatorSpot);
					}
				}
			}
		}
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		if(supportKeyboard && (h != 0 || v != 0) && Time.time-lastIndicatorKeyboardMove > indicatorKeyboardMoveThreshold) {
			cycleIndicatorZ = true;
			indicatorCycleT = 0;
			float dx = (h == 0 ? 0 : Mathf.Sign(h));
			float dy = (v == 0 ? 0 : Mathf.Sign(v));
			if(indicatorXY.x+dx >= 0 && indicatorXY.y+dy >= 0 &&
				 map.HasTileAt((int)(indicatorXY.x+dx), (int)(indicatorXY.y+dy))) {
				lastIndicatorKeyboardMove = Time.time;
				indicatorXY.x += dx;
				indicatorXY.y += dy;
				indicatorZ = map.NearestZLevel((int)indicatorXY.x, (int)indicatorXY.y, (int)indicatorZ);
			}
		}
		if(supportKeyboard && Input.GetButtonDown("Confirm")) {
			Vector3 indicatorSpot = new Vector3(indicatorXY.x, indicatorXY.y, indicatorZ);
			PathNode pn = overlay.PositionAt(indicatorSpot);
			if(pn != null && pn.canStop) {
				PerformMove(indicatorSpot);
			}
		}
		if(cycleIndicatorZ) {
			indicatorCycleT += Time.deltaTime;
			if(indicatorCycleT >= indicatorCycleLength) {
				indicatorCycleT -= indicatorCycleLength;
				indicatorZ = map.NextZLevel((int)indicatorXY.x, (int)indicatorXY.y, (int)indicatorZ, true);
			}
		}
		if(overlay != null) {
			MapTile t = map.TileAt((int)indicatorXY.x, (int)indicatorXY.y, (int)indicatorZ);
			if(t != null) {
				overlay.selectedPoint = new Vector4((int)indicatorXY.x, (int)indicatorXY.y, t.z, t.maxZ);
			}
		}
	}
	
	override protected void PresentMoves() {
		base.PresentMoves();
		Debug.Log("present");
		GridMoveStrategy ms = GetComponent<GridMoveStrategy>();
		PathNode[] destinations = ms.GetValidMoves();
		Vector3 charPos = map.InverseTransformPointWorld(transform.position);
		overlay = map.PresentGridOverlay(
			"move", this.gameObject.GetInstanceID(), 
			new Color(0.2f, 0.3f, 0.9f, 0.7f),
			new Color(0.4f, 0.6f, 0.9f, 0.85f),
			destinations
		);
		cycleIndicatorZ = false;
		indicatorXY = new Vector2(Mathf.Floor(charPos.x)+0.5f, Mathf.Floor(charPos.y)+0.5f);
		indicatorZ = map.NearestZLevel((int)indicatorXY.x, (int)indicatorXY.y, (int)Mathf.Floor(charPos.z));
		MapTile t = map.TileAt((int)indicatorXY.x, (int)indicatorXY.y, (int)indicatorZ);
		overlay.selectedPoint = new Vector4((int)indicatorXY.x, (int)indicatorXY.y, t.z, t.maxZ);
	}
	
	override protected void FinishMove() {
		overlay = null;
		if(map.IsShowingOverlay("move", this.gameObject.GetInstanceID())) {
			map.RemoveOverlay("move", this.gameObject.GetInstanceID());
		}	
		base.FinishMove();
	}
}
