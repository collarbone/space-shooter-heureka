﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Timeoutable {

	[SerializeField]
	private float _speed = 10.0f;

	[SerializeField]
	private float _rotationSpeed = 5.0f;
	
	[SerializeField]
	private Transform _transformLimits;

    private List<Collectable> _collectables = new List<Collectable>();//TODO add timestamp, so we can 

	private bool _hasInput = false;

	private int _id = 0;
	public int Id {
		set {_id = value; }
		get { return _id; }
	}

	private GameConstants.PlayerKeys _keys = new GameConstants.PlayerKeys(0);

	public void setPlayerKeys(GameConstants.PlayerKeys keys) {
		_keys = keys;
	}

	public override void Update() {
		base.Update();
	}

	public override float getTimeout() {
		return GameConstants.PLAYER_IDLE_TIMEOUT;
	}
	
	public override bool shouldReset() {
		return _hasInput;
	}

	public void addCollectable(Collectable collectable) {
		//TODO use these on update / input, etc
		//TODO remove it automatically after timeout, run out of ammo, etc
		Debug.Log (string.Format ("SpaceShip {0}: Adding collectable of type: {1} + points {2}", Id, collectable.Type, collectable.Points));
		_collectables.Add(collectable);
		ScoreManager.Instance.addPoints(Id, collectable.Points);

        if (collectable.Type == Collectable.CollectableType.Weapon) {
			GetComponent<WeaponLauncher>().addWeapon(collectable);
		}
		else if (collectable.Type == Collectable.CollectableType.Enlarge) {
			//TODO tween
			//TODO this should also scale ammunition. and perhaps the exlosion? :D
			Vector3 curScale = this.gameObject.transform.localScale;
			this.gameObject.transform.localScale = 2.0f * curScale;
		}
		else if (collectable.Type == Collectable.CollectableType.SpeedUp) {
			InsultManager.Instance.insultAboutSpeed(Id, _collectables); 
		}
	}

	void FixedUpdate() {
		float horizontal = Input.GetAxis(_keys.HorizontalAxis);
		float vertical = Input.GetAxis(_keys.VerticalAxis);
		_hasInput = horizontal > 0.00001f || vertical > 0.000001f;

		Rigidbody rb = GetComponent<Rigidbody> ();

		float speedFactor = getSpeedFactor();

		float rotationSpeed = speedFactor * _rotationSpeed; 
		rb.transform.Rotate(new Vector3(0.0f, rotationSpeed * horizontal, 0.0f));
		rb.transform.eulerAngles = new Vector3(0.0f, rb.transform.rotation.eulerAngles.y, 0.0f);

		//Vector3 movement = new Vector3 (horizontal, 0.0f, vertical);
		//TODO fix: move towards rotation head
		Vector3 movement = new Vector3 (0.0f, 0.0f, vertical);
		//let's remove the velocities in all other directions as the movement direction
		rb.velocity = new Vector3(0.0f, 0.0f, rb.velocity.z);

		float currentSpeed = speedFactor * _speed;

		rb.transform.position += transform.forward * Time.deltaTime * currentSpeed * vertical;
		rb.transform.position = Vector3.Scale(new Vector3(1.0f, 0.0f, 1.0f), rb.transform.position);

		//limit the position to stay inside the game area. not 100% mathematically correct but good enough
		Collider shipCollider = GetComponent<Collider>();
		Vector3 shipExtents = 1.1f * shipCollider.bounds.extents;//works without the factor, but put it just-in-case

		float xMin = -1*(_transformLimits.localScale.x / 2) + shipExtents.x;
		float xMax = (_transformLimits.localScale.x / 2) - shipExtents.x;
		float zMin = -1*(_transformLimits.localScale.z / 2) + shipExtents.z;
		float zMax = (_transformLimits.localScale.z / 2) - shipExtents.z;
		rb.position = new Vector3(
			Mathf.Clamp (rb.position.x, xMin, xMax), 
			0.0f, 
			Mathf.Clamp (rb.position.z, zMin, zMax));
	}

	private float getSpeedFactor() {
		float speed = 1.0f;
		foreach (Collectable c in _collectables) {
			speed *= c.SpeedUpFactor;
		}
		return speed;
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "projectile") {
			ProjectileBehaviour projectile = other.gameObject.GetComponent<ProjectileBehaviour>();
			if (projectile.Source != this.gameObject) {
				int sourceId = projectile.SourceId;
				ScoreManager.Instance.addPoints(sourceId, GameConstants.POINTS_FOR_KILL);

				Destroy(other.gameObject);
				GameManager.Instance.destroyWithExplosion(this.gameObject);
			}
		}
		else if (other.tag == "spaceship") {
			GameManager.Instance.destroyWithExplosion(this.gameObject);
			GameManager.Instance.destroyWithExplosion(other.gameObject, true, false);
        }
	}

	public bool hasInput() {
		return _hasInput;
	}
}
