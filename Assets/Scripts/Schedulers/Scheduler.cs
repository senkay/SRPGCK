using UnityEngine;
using System.Collections.Generic;

//TODO: Schedulers may also need to be responsible for scheduling timed attacks, 
//      on seconds or on CT or end of turn or end of phase or whatever, according to the scheduler.

public class Scheduler : MonoBehaviour {
	[HideInInspector]
	public List<Character> characters;
	[HideInInspector]
	public Character activeCharacter;
	
	public void AddCharacter(Character c) {
		if(!characters.Contains(c)) {
			characters.Add(c);
		}
	}
	
	public void RemoveCharacter(Character c) {
		characters.Remove(c);
	}
	
	public virtual void Activate(Character c, object context=null) {
		activeCharacter = c;
		c.SendMessage("Activate", context, SendMessageOptions.RequireReceiver);
	}
	
	public virtual void Deactivate(Character c=null, object context=null) {
		if(c == null) { c = activeCharacter; }
		if(activeCharacter == c) { activeCharacter = null; }
		c.SendMessage("Deactivate", context, SendMessageOptions.RequireReceiver);
	}
	
	public virtual float GetMaximumTraversalDistance() {
		return float.MaxValue;
	}
	
	public virtual void EndMovePhase(Character c) {

	}
	
	public virtual void Start () {
	
	}
	
	public virtual void CharacterMoved(Character c, Vector3 src, Vector3 dest) {
		
	}
	public virtual void CharacterMovedTemporary(Character c, Vector3 src, Vector3 dest) {
		
	}
	
	public virtual void Update () {
		if(activeCharacter != null && activeCharacter.isActive) { return; }
		if(activeCharacter != null && !activeCharacter.isActive) { Deactivate(activeCharacter); }
	}
}
