using UnityEngine;

[System.Serializable]
public class MoveSkill : Skill {
	public bool lockToGrid=true;
	
	override public bool isPassive { get { return false; } }
	
	public bool supportKeyboard = true;	
	public bool supportMouse = true;
	public float indicatorCycleLength=1.0f;
	public bool requireConfirmation = false;
	[HideInInspector]
	public bool awaitingConfirmation = false;
	protected float firstClickTime = -1;
	protected float doubleClickThreshold = 0.3f;
	
	public bool RequireConfirmation { 
		get { return requireConfirmation; } 
		set { requireConfirmation = value; }
	}
	
	public bool AwaitingConfirmation {
		get { return awaitingConfirmation; }
		set { awaitingConfirmation = value; }
	}
	
	public ActionStrategy Strategy { get { return moveStrategy; } }
	public MoveExecutor Executor { get { return moveExecutor; } }
	
	//strategy
	public ActionStrategy moveStrategy;
	public float ZDelta { get { return GetParam("range.z", character.GetStat("jump", 3)); } }
	public float XYRange { get { return GetParam("range.xy", character.GetStat("move", 5)); } }
	
	//executor
	[HideInInspector]
	public MoveExecutor moveExecutor;
	public bool performTemporaryMoves = false;
	public bool animateTemporaryMovement=false;
	public float XYSpeed = 12;
	public float ZSpeedUp = 15;
	public float ZSpeedDown = 20;
	
	public override void Start() {
		base.Start();
		if(moveStrategy == null) {
			moveStrategy = new ActionStrategy();
		}
		moveExecutor = new MoveExecutor();
		Executor.lockToGrid = lockToGrid;
		Strategy.owner = this;
		Executor.owner = this;
	}
	
	public override void ActivateSkill() {
		base.ActivateSkill();

		Strategy.owner = this;
		Strategy.zRangeDownMin = 0;
		Strategy.zRangeDownMax = ZDelta;
		Strategy.zRangeUpMin = 0;
		Strategy.zRangeUpMax = ZDelta;
		Strategy.xyRangeMin = 0;
		Strategy.xyRangeMax = XYRange;
	
		Executor.owner = this;	
		Executor.lockToGrid = lockToGrid;
		Executor.animateTemporaryMovement = animateTemporaryMovement;
		Executor.XYSpeed = XYSpeed;
		Executor.ZSpeedUp = ZSpeedUp;
		Executor.ZSpeedDown = ZSpeedDown;	
		Strategy.Activate();
		Executor.Activate();

		PresentMoves();
	}	
	
	protected virtual void PresentMoves() {
		
	}

	public override void DeactivateSkill() {
		if(!isActive) { return; }
		Strategy.Deactivate();
		Executor.Deactivate();
		base.DeactivateSkill();
	}
	
	public override void Update() {
		base.Update();
		if(!isActive) { return; }
		Strategy.owner = this;
		Executor.owner = this;
		Strategy.Update();
		Executor.Update();	
	}

	public override void Reset() {
		base.Reset();
		skillName = "Move";
		skillSorting = -1;
	}
	public override void Cancel() {
		if(!isActive) { return; }
		Executor.Cancel();
		base.Cancel();
	}
	
	public virtual void TemporaryMove(Vector3 tc) {
		TemporaryMoveToPathNode(new PathNode(tc, null, 0));
	}

	public virtual void IncrementalMove(Vector3 tc) {
		IncrementalMoveToPathNode(new PathNode(tc, null, 0));
	}
	
	public virtual void PerformMove(Vector3 tc) {
		PerformMoveToPathNode(new PathNode(tc, null, 0));
	}
	
	public virtual void TemporaryMoveToPathNode(PathNode pn) {
		MoveExecutor me = Executor;
		me.TemporaryMoveTo(pn, delegate(Vector3 src, PathNode endNode, bool finishedNicely) {
			scheduler.CharacterMovedTemporary(
				character, 
				map.InverseTransformPointWorld(src), 
				map.InverseTransformPointWorld(endNode.pos)
			);
		});
	}

	public virtual void IncrementalMoveToPathNode(PathNode pn) {
		MoveExecutor me = Executor;
		me.IncrementalMoveTo(pn, delegate(Vector3 src, PathNode endNode, bool finishedNicely) {
/*			Debug.Log("moved from "+src);*/
			scheduler.CharacterMovedIncremental(
				character, 
				src, 
				endNode.pos
			);
		});
	}
	
	public virtual void PerformMoveToPathNode(PathNode pn) {
		MoveExecutor me = Executor;
		me.MoveTo(pn, delegate(Vector3 src, PathNode endNode, bool finishedNicely) {
			scheduler.CharacterMoved(
				character, 
				map.InverseTransformPointWorld(src), 
				map.InverseTransformPointWorld(endNode.pos)
			);
			ApplySkill();
		});
	}
}