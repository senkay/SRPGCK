using UnityEngine;
using System.Collections;

public class CTScheduler : Scheduler {
	public float defaultMaxCT = 100;
	public float defaultPerTileCTCost = 0;
	public float defaultPerMoveCTCost = 30;
	public float defaultPerActionCTCost = 40;
	public float defaultPerActivationCTCost = 30;
	
	public bool coalesceCTDecrements = false;
	[SerializeField]
	protected float pendingCTDecrement = 0;
		
	override public void Start () {
		base.Start();
	}
	
	override public void AddCharacter(Character c) {
		if(!c.HasStat("ct")) {
			Debug.LogError("CT-scheduled character "+c+" must have CT stat.");
			return;
		}
		if(!c.HasStat("speed")) {
			Debug.LogError("CT-scheduled character "+c+" must have speed stat.");
			return;
		}
		base.AddCharacter(c);
		if(c.gameObject.GetComponent<CTCharacter>() == null) {
			c.gameObject.AddComponent<CTCharacter>();
		}
	}
	
	override public void Activate(Character c, object ctx=null) {
		base.Activate(c, ctx);
		pendingCTDecrement = 0;
		CTCharacter ctc = c.GetComponent<CTCharacter>();
		ctc.HasMoved = false;
		ctc.HasActed = false;
		//(for now): ON `activate`, MOVE
//		Debug.Log("activate"); 
	}
	
	override public void Deactivate(Character c, object ctx=null) {
		base.Deactivate(c, ctx);
		//reduce c's CT by base turn cost (30)
		CTCharacter ctc = c.GetComponent<CTCharacter>();
		float cost = ctc.PerActivationCTCost;
		if(coalesceCTDecrements) {
			pendingCTDecrement += cost;
			ctc.CT = Mathf.Max(ctc.CT-pendingCTDecrement, 0);
			pendingCTDecrement = 0;
		} else {
			ctc.CT = Mathf.Max(ctc.CT-cost, 0);
		}
	}
	//TODO: replace me with a "skill finished" callback
	override public void SkillApplied(Skill s) {
		base.SkillApplied(s);
		if(s.character != null) {
			CTCharacter ctc = s.character.GetComponent<CTCharacter>();
			if(s is MoveSkill) {
				//reduce c's CT by any-movement cost (30)
				float cost = ctc.PerMoveCTCost;
				if(coalesceCTDecrements) {
					pendingCTDecrement += cost;
				} else {
					ctc.CT = Mathf.Max(ctc.CT-cost, 0);
				}
				ctc.HasMoved = true;		
			} else {
				float cost = ctc.PerActionCTCost;
				if(coalesceCTDecrements) {
					pendingCTDecrement += cost;
				} else {
					ctc.CT = Mathf.Max(ctc.CT-cost, 0);
				}
				ctc.HasActed = true;
			}
		}
	}	
	
	override public void CharacterMoved(Character c, Vector3 src, Vector3 dest) {
		base.CharacterMoved(c, src, dest);
		//reduce c's CT by per-tile movement cost (0)
		//FIXME: multiply by number of tiles traversed, needs pathnode
		CTCharacter ctc = c.GetComponent<CTCharacter>();
		float cost = ctc.PerTileCTCost;
		if(coalesceCTDecrements) {
			pendingCTDecrement += cost;
		} else {
			ctc.CT = Mathf.Max(ctc.CT-cost, 0);
		}
	}
	//after c acts, reduce c's CT by per-act cost (40)
		
	public override void FixedUpdate () {
		base.FixedUpdate();
		//if there is no active unit
		if(activeCharacter == null) {
		  //TODO: take the first scheduled attack with CT > 100 and trigger it
			
			//else, take the first unit with CT > 100, if any, and activate it
			foreach(Character c in characters) {
				CTCharacter ctc = c.GetComponent<CTCharacter>();
				float maxCT = ctc.MaxCT;
				if(ctc.CT >= maxCT) {
					ctc.CT = maxCT;
					Activate(c);
					return;
				}
			}
			//TODO: else, tick up every attack by their effective speed
			
			//and tick up CT on everybody by their effective speed
			foreach(Character c in characters) {
				CTCharacter ctc = c.GetComponent<CTCharacter>();
/*				Debug.Log("Tick up by "+speed);*/
				ctc.Tick();
			}
		}
		//otherwise, do nothing
	}
}
