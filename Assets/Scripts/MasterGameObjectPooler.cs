using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MasterGameObjectPooler {

    /* Manages <GameObjectPooler> instances */

        // Fields
    private List<GameObjectPooler> objectPoolers = new List<GameObjectPooler>();
    private GameObject masterHolder; // A place to cleanly store the objectPoolers

        // Constructors
    public MasterGameObjectPooler(string name) {
        this.masterHolder = new GameObject(name);
    }

        // Methods
    private int prefabIndex(string prefabName) { // Look for a GameObjectPooler that uses a prefab named <prefabName>
        return this.objectPoolers.IndexOf(objectPoolers.Where(x => (x.getPrefab().name == prefabName)).FirstOrDefault());
    }

    public void free(GameObject obj) { // Free a single object
        int index = prefabIndex(obj.name.Split(char.Parse("_"))[0]); // By convention, the object MUST be named "<prefabName>_<whatever>"
        if (index == -1) Debug.Log("[Warning] :: [MasterGameObjectPooler] :: Trying to free an object (called '" + obj.name + "') that does not belong here.");
        else this.objectPoolers[index].free(obj); // The <objectPooler> will free the element
    }
    public void freeAll(GameObject prefab) { // Free all objects of a given prefab
        int index = prefabIndex(prefab.name);
        if (index == -1) Debug.Log("[Warning] :: [MasterGameObjectPooler] :: Trying to free an prefab (called '" + prefab.name + "') that does not belong here.");
        else this.objectPoolers[index].freeAll();
    }
    public void freeAll() { // Free all objects of all prefabs
        foreach (GameObjectPooler objectPooler in this.objectPoolers)
            objectPooler.freeAll();
    }

    public GameObject get(GameObject prefab) {
        int index = prefabIndex(prefab.name);
        if (index == -1) { // First time this prefab is encountered : Create a new <ObjectPooler> to handle it
            GameObjectPooler newGameObjectPooler = new GameObjectPooler(prefab);
            newGameObjectPooler.setParent(this.masterHolder.transform); // Nest the <ObjectPooler> for clarity
            this.objectPoolers.Add(newGameObjectPooler);
            index = this.objectPoolers.Count - 1; // By construction, the relevant <objectPooler> is now the last of the list
        }
        return this.objectPoolers[index].get(); // The appropriate <objectPooler> will get the element
    }
}
