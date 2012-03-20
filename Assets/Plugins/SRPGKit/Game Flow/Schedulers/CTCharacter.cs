using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("")]
public class CTCharacter : MonoBehaviour {
	[SerializeField]
	protected bool _hasMoved=false;
	[SerializeField]
	protected bool _hasActed=false;

	Character character;

	void FindCharacter() {
		if(character == null) {
			character = GetComponent<Character>();
		}
	}

	virtual public float CT {
		get {
			FindCharacter();
			return character.GetStat("ct");
		}
		set {
			FindCharacter();
			character.SetBaseStat("ct", value);
		}
	}
	virtual public float Speed {
		get {
			FindCharacter();
			return character.GetStat("speed");
		}
		set {
			FindCharacter();
			character.SetBaseStat("speed", value);
		}
	}

	virtual public bool HasMoved {
		get {
			return _hasMoved;
		}
		set {
			_hasMoved = value;
		}
	}

	virtual public bool HasActed {
		get {
			return _hasActed;
		}
		set {
			_hasActed = value;
		}
	}

	virtual public void Tick() {
		if(CT < MaxCT) {
			float speed = character.GetStat("speed");
			character.SetBaseStat("ct", Mathf.Min(CT+speed, MaxCT));
			foreach(StatusEffect se in character.StatusEffects) {
				se.Tick(se.ticksInLocalTime ? speed : 1);
			}
		}
	}

	virtual public float PerActivationCTCost {
		get {
			return ((CTScheduler)GetComponent<Character>().map.scheduler).defaultPerActivationCTCost;
		}
	}
	virtual public float PerMoveCTCost {
		get {
			return ((CTScheduler)GetComponent<Character>().map.scheduler).defaultPerMoveCTCost;
		}
	}
	virtual public float PerActionCTCost {
		get {
			return ((CTScheduler)GetComponent<Character>().map.scheduler).defaultPerActionCTCost;
		}
	}
	virtual public float PerTileCTCost {
		get {
			return ((CTScheduler)GetComponent<Character>().map.scheduler).defaultPerTileCTCost;
		}
	}
	virtual public float MaxCT {
		get {
			return ((CTScheduler)GetComponent<Character>().map.scheduler).defaultMaxCT;
		}
	}
}
