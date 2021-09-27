using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameObjectPooler {

    /* Stores and manages <GameObject> instances to be used-and-reused instead of destroyed-and-reinstanciated */

        // Fields
    private List<GameObject> objects = new List<GameObject>(); // <GameObjects> to be managed
    private GameObject holder; // A place to store all <GameObjects> cleanly
    private GameObject prefab; // Specification of the <GameObjects> we deal with

        // Constructors
    public GameObjectPooler(GameObject _prefab) {
        this.prefab = _prefab;
        this.holder = new GameObject(this.prefab.name);
    }

        // Getters and Setters
    public GameObject getPrefab() { return this.prefab; }
    public void setParent(Transform _parent) { this.holder.transform.parent = _parent; }

        // Methods
    public void free(GameObject obj) { // Free one <GameObject>
        if (this.objects.Count == 0) return;
        int index = objects.IndexOf(obj); // Identify where the object is stored
        if (index == -1) Debug.Log("[Warning] :: [Object Pooler] :: Trying to free an object that did not belong here.");
        else objects[index].SetActive(false); // Find it, free it
    }
    public void freeAll() { // Free all active <GameObjects>
        if (this.objects.Count == 0) return;
        foreach (GameObject obj in this.objects.Where(x => x.activeSelf))
            obj.SetActive(false);
    }

    public GameObject get() { // Recycle an un-used <GameObject> or create a new one if necessary
        int index = this.objects.IndexOf(this.objects.Where(x => !x.activeSelf).FirstOrDefault()); // Look for an un-used (i.e. inactive) object
        if (index == -1) {  // No inactive object is found : Create a new <GameObject>
            GameObject newObject = Object.Instantiate(this.prefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            newObject.name = this.holder.name + "_" + this.objects.Count; // By convention, the object MUST be named "<prefabName>_<whatever>" (for the MetaGameObjectPooler)
            newObject.transform.parent = this.holder.transform; // Nest the new <GameObject> for clarity
            this.objects.Add(newObject);
            index = this.objects.Count - 1; // By construction, the relevant <GameObject> is now the last of the list
        }
        else this.objects[index].SetActive(true); // Only activate the <GameObject> if necessary
        return this.objects[index];
    }
}
