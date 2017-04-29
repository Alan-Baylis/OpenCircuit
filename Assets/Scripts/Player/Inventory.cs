using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Player/Inventory")]
public class Inventory : NetworkBehaviour {

    public Vector2 iconDimensions;
    public float iconSpacing;
    public float itemCircleRadius;
    public Texture2D itemBackground;
	public GameObject[] startingItemPrefabs;

	protected Player player;
	protected Dictionary<System.Type, List<Item>> items = new Dictionary<System.Type, List<Item>>();
	protected Item equipped;
    protected System.Type[] slots = new System.Type[3];
    protected int selecting = -1;
    protected int highlighted;
	protected List<System.Type> unselectedItems = new List<System.Type>();
    protected Vector2 mousePos;
	protected List<System.Type> contextStack = new List<System.Type>();
	[HideInInspector]
	public bool sprinting;

	private readonly List<GameObject> toTake = new List<GameObject>();

	[ServerCallback]
	void Awake() {
		foreach(GameObject itemPrefab in startingItemPrefabs) {
			GameObject instantiatedItem = Instantiate(itemPrefab);
			NetworkServer.SpawnWithClientAuthority(instantiatedItem.gameObject, getPlayer().clientController.gameObject);
			toTake.Add(instantiatedItem);
		}
	}

	[ServerCallback]
	void Start() {
		foreach(GameObject item in toTake) {
			take(item);
			RpcTake(item.GetComponent<NetworkIdentity>().netId);
		}
	}

    public void OnGUI() {
        if (selecting < 0)
            return;
        showSlottedItems();
        showUnequippedItems(selecting);
    }

	public Player getPlayer() {
		if(player == null) {
			player = GetComponent<Player>();
		}
		return player;
	}

	public void setSprinting(bool sprint) {
		sprinting = sprint;
	}

    public bool take(GameObject itemObject) {
        Item item = itemObject.GetComponent<Item>();
		if (item == null)
            return false;
        return take(item);
    }

    public bool take(Item item) {
		addItem(item);
		item.setParent(netId);
        item.onTake(this);
        return true;
    }

	[ClientRpc]
	public void RpcTake(NetworkInstanceId item) {
		take(ClientScene.FindLocalObject(item));
	}

	public bool canTake(GameObject itemObject) {
		return itemObject.GetComponent<Item>() != null;
	}

    public void drop(Item item) {
        removeItem(item);
        item.onDrop(this);
    }

    public void equip(int slot) {
        if (slots[slot] == null)
            return;
        Item newEquiped = getItem(slots[slot]);
        if (equipped != null && newEquiped.GetType() == equipped.GetType()) {
            unequip();
            return;
        }
        equip(newEquiped);
    }

    public void equip(Item item) {
        unequip();
        equipped = item;
        equipped.onEquip(this);
    }

    public void unequip() {
        if (equipped == null)
            return;
        equipped.onUnequip(this);
        equipped = null;
    }

    public void useEquipped() {
		if (inContext()) {
			getItem(contextStack[0]).beginInvoke(this);
			return;
		}
        if (selecting >= 0) {
            slots[selecting] = (highlighted < 0)? null: unselectedItems[highlighted];
            mousePos = Vector3.zero;
			return;
		}
        if (equipped != null)
            equipped.beginInvoke(this);
    }

	public void stopUsingEquiped() {
		if (inContext()) {
			getItem(contextStack[0]).endInvoke(this);
			return;
		}
		if (equipped != null)
			equipped.endInvoke(this);
	}

	public void reloadEquipped() {
		if (equipped != null) {
			AbstractGun gun = equipped as AbstractGun;
			if (gun != null)
				gun.reload();
		}
	}

	public void pushContext(System.Type contextItem) {
		contextStack.Insert(0, contextItem);
		if (equipped != null)
			equipped.onUnequip(this);
		getItem(contextItem).onEquip(this);
		selecting = -1;
    }

	public void popContext(System.Type contextItem) {
		if (contextStack.Count > 0 && contextStack [0] == contextItem) {
			contextStack.RemoveAt (0);
			getItem(contextItem).onUnequip(this);
			if (equipped != null)
				equipped.onEquip(this);
		}
	}

    public void doSelect(int slot) {
        if (selecting != slot) {
            if (slot < 0)
                equip(selecting);
            selecting = slot;
            mousePos = Vector2.zero;
        }
        if (selecting < 0)
            return;
        unselectedItems.Clear();
		foreach(System.Type type in items.Keys)
			if (!type.IsAssignableFrom(typeof(ContextItem)))
				unselectedItems.Add(type);
        foreach(System.Type type in slots)
            if (type != null)
                unselectedItems.Remove(type);
    }

    public void moveMouse(Vector2 amount) {
        mousePos += amount;
		mousePos = Vector2.ClampMagnitude(mousePos, itemCircleRadius);
		highlighted = getCircleSelection(unselectedItems.Count, mousePos);
    }

    public bool isSelecting() {
        return selecting >= 0;
    }

	public bool contains(System.Type itemType) {
		return items.ContainsKey(itemType);
	}

	public bool contains(Item item) {
		return getItemList(item.GetType()).Contains(item);
	}

	public bool inContext() {
		return contextStack.Count > 0;
    }

	public void doStep(float strength) {
		if (equipped != null)
			equipped.doStep(strength);
	}

	public List<T> getItemsExtending<T>() where T : Item {
		List<T> results = new List<T>();
		foreach (List<Item> itemList in items.Values) {
			foreach(Item item in itemList) {
				if(item.GetType().IsSubclassOf(typeof(T))) {
					results.Add((T)item);
				}
			}
		}
		return results;
	}

	public Item getEquipped() {
		if (contextStack.Count > 0)
			return getItem(contextStack[0]);
		return equipped;
	}

    protected void showSlottedItems() {
        float offset = (Screen.width / 2f) - (iconDimensions.x * 1.5f + iconSpacing);
        for (int i = 0; i < slots.Length; ++i) {
            if (slots[i] != null) {
                Item item = getItem(slots[i]);
                if (item.icon != null) {
                    Rect pos = new Rect(offset, 0, iconDimensions.x, iconDimensions.y);
                    GUI.DrawTexture(pos, item.icon, ScaleMode.ScaleToFit);
                }
			}
			offset += iconDimensions.x + iconSpacing;
        }
    }

    protected void showUnequippedItems(int slot) {

        // draw backdrop
        float backdropWidth = (iconDimensions.x +itemCircleRadius) *3;
        GUI.color = new Color(1, 1, 1, 0.8f);
        GUI.DrawTexture(new Rect((Screen.width -backdropWidth) /2, (Screen.height -backdropWidth) /2, backdropWidth, backdropWidth), itemBackground, ScaleMode.ScaleToFit);
        GUI.color = Color.white;

        // draw unselected items
        Vector2 center = (new Vector2(Screen.width, Screen.height) - iconDimensions) /2 + new Vector2(-mousePos.x, mousePos.y) *0.1f;
        double angle = 0.5 *Mathf.PI;
        double angleDiff = 2.0 *Mathf.PI / unselectedItems.Count;
        int i=0;
        foreach(System.Type itemType in unselectedItems) {
            Vector2 offset = new Vector2(Mathf.Cos((float)angle), Mathf.Sin((float)angle)) *itemCircleRadius;
            float size = i==highlighted? 2: 1;
            Rect pos = new Rect(center.x +offset.x, center.y +offset.y, iconDimensions.x *size, iconDimensions.y *size);
            GUI.DrawTexture(pos, getItem(itemType).icon, ScaleMode.ScaleToFit);
            angle += angleDiff;
            ++i;
        }

        // draw slotted item
        if (slots[slot] != null) {
            Rect pos = new Rect(center.x, center.y, iconDimensions.x, iconDimensions.y);
            GUI.DrawTexture(pos, getItem(slots[slot]).icon, ScaleMode.ScaleToFit);
        }
    }

    protected int getCircleSelection(int itemCount, Vector2 mousePosition) {
		if (itemCount < 1 || mousePosition.sqrMagnitude < (itemCircleRadius *itemCircleRadius) /4)
            return -1;
        double angle = Mathf.Atan2(mousePosition.x, mousePosition.y) +Mathf.PI;
        return ((int)(angle /(2.0*Mathf.PI /itemCount) +0.5)) %itemCount;
    }

    protected Item getItem(System.Type type) {
        List<Item> list = items[type];
        if (list == null)
            return null;
        return list[0];
    }

    protected void addItem(Item item) {
        System.Type type = item.GetType();
        List<Item> list = getItemList(type);
        list.Add(item);
        items[type] = list;
    }

    protected Item removeItem(System.Type type) {
        List<Item> list = getItemList(type);
        if (list.Count > 1) {
            Item item = list[list.Count];
            list.RemoveAt(list.Count);
            items[type] = list;
            return item;
        }
        items[type] = null;
        return (list.Count == 1) ? list[0] : null;
    }

    protected bool removeItem(Item item) {
        System.Type type = item.GetType();
        List<Item> list = getItemList(type);
        bool removed = list.Remove(item);
        if (list.Count < 1)
            items[type] = null;
        else
            items[type] = list;
        return removed;
    }

    protected List<Item> getItemList(System.Type type) {
        List<Item> list;
        if (items.TryGetValue(type, out list))
            return list;
        return new List<Item>();
    }
}
