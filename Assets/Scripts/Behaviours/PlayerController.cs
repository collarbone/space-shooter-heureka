﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Boundary
{
	public float xMin, xMax, zMin, zMax;
}

public class PlayerController : MonoBehaviour {

	[SerializeField]
	private float _speed = 10.0f;

	[SerializeField]
	private float _rotationSpeed = 5.0f;
	
	[SerializeField]
	private Boundary _boundary;//TODO remove and use the boundary gameobject in the scene

	private List<Collectable> _collectables = new List<Collectable>();

	private int _id = 0;
	public int Id {
		set {_id = value; }
		get { return _id; }
	}

	private GameConstants.PlayerKeys _keys = new GameConstants.PlayerKeys(0);

	public void setPlayerKeys(GameConstants.PlayerKeys keys) {
		_keys = keys;
	}

	public void addCollectable(Collectable collectable) {
		//TODO use these on update / input, etc
		//TODO remove it automatically after timeout, run out of ammo, etc
		_collectables.Add(collectable);
		Debug.Log (string.Format ("SpaceShip {0}: Adding collectable of type: {1}", Id, collectable.Type));

		if (collectable.Type == Collectable.CollectableType.Weapon) {
			GetComponent<WeaponLauncher>().addWeapon(collectable);
		}
	}

	void FixedUpdate() {
		float horizontal = Input.GetAxis(_keys.HorizontalAxis);
		float vertical = Input.GetAxis(_keys.VerticalAxis);

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

		rb.position = new Vector3(
			Mathf.Clamp (rb.position.x, _boundary.xMin, _boundary.xMax), 
			0.0f, 
			Mathf.Clamp (rb.position.z, _boundary.zMin, _boundary.zMax));

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
			Destroy(other.gameObject);
			GameManager.Instance.destroyWithExplosion(this.gameObject);
		}
		else if (other.tag == "spaceship") {
			GameManager.Instance.destroyWithExplosion(other.gameObject);
			GameManager.Instance.destroyWithExplosion(this.gameObject);
		}
	}
}
