using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.Networking;

public class AmmoPickupTest {

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
	[UnityTest]
	[PrebuildSetup(typeof(NetworkSetup))]
	public IEnumerator testAmmoPickup() {
		LogAssert.Expect(LogType.Log, "EMPTY EFFECT");
		// Use the Assert class to test conditions.
		// yield to skip a frame

		var gameObject = new GameObject();
		AmmoPickup ammo = gameObject.AddComponent<AmmoPickup>();
		AssaultRifle rifle = addRifleToPlayer(PlayModeTestUtility.createPlayer());
		NetworkServer.Spawn(rifle.gameObject);
		yield return null;
		rifle.clearAmmo();
		yield return null;
		ammo.OnTriggerEnter(rifle.GetComponentInParent<Collider>());
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.True(ammo == null);
	}

	[UnityTest]
	[PrebuildSetup(typeof(NetworkSetup))]
	public IEnumerator testAmmoPickup_NoRifle() {
		// Use the Assert class to test conditions.
		// yield to skip a frame

		var gameObject = new GameObject();
		AmmoPickup ammo = gameObject.AddComponent<AmmoPickup>();
		Player player = PlayModeTestUtility.createPlayer();
		yield return null;
		ammo.OnTriggerEnter(player.GetComponent<Collider>());
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.True(ammo != null);
	}

	private AssaultRifle addRifleToPlayer(Player player) {
		GameObject rifleObject = new GameObject();
		rifleObject.transform.parent = player.transform;
		return rifleObject.AddComponent<AssaultRifle>();
	}

}
