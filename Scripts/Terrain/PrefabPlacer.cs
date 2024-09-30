using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Class responsible for creating prefabs based on the spawnables.
/// </summary>
public static class PrefabPlacer
{
    /// <summary>
    /// Creates prefabs asynchronously at a given position based on the provided spawnable object.<br/>
    /// The method places the prefab on the surface of a chunk, aligns it with the normal of the hit point, and applies random rotation and scaling within the specified range.<br/>
    /// If the spawnable is water, it will be placed at the specified water height.<br/>
    /// The method uses Addressables for async instantiation and invokes a callback upon completion.
    /// </summary>
    /// <param name="spawnable">The spawnable object containing information about the prefab and placement settings.</param>
    /// <param name="chunkPos">The position of the chunk where the prefab should be placed.</param>
    /// <param name="parent">The parent transform for the instantiated prefab.</param>
    /// <param name="chunkNum">The chunk number used to verify the correct placement of the prefab.</param>
    /// <param name="meshSettings">The mesh settings that determine the size and height of the terrain.</param>
    /// <param name="onComplete">The callback function that is invoked after the prefab is instantiated. It receives the created GameObject as a parameter.</param>
    public static void CreatePrefabs(Spawnable spawnable, Vector2 chunkPos, Transform parent, int chunkNum, MeshSettings meshSettings, Action<GameObject> onComplete)
    {
        
        if (!spawnable.isWater)
        {
            float randomX = UnityEngine.Random.Range(chunkPos.x + meshSettings.meshWorldSize/2, chunkPos.x - meshSettings.meshWorldSize/2);
            float randomZ = UnityEngine.Random.Range(chunkPos.y + meshSettings.meshWorldSize/2, chunkPos.y - meshSettings.meshWorldSize/2);
            
            float sampleX = randomX;
            float sampleZ = randomZ;

            Vector3 rayStart = new Vector3(sampleX, spawnable.maxWorldHeight, sampleZ);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                
                if (hit.collider.gameObject.name == "Chunk " + chunkNum && hit.point.y > spawnable.minWorldHeight)
                {
                    // Use InstantiateAsync to create the prefab asynchronously
                    AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(spawnable.prefabPath);

                    // Register a callback when the instantiation is complete
                    handle.Completed += (asyncHandle) =>
                    {
                        if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            GameObject newSpawnable = asyncHandle.Result;
                            newSpawnable.transform.parent = parent;
                            newSpawnable.transform.position = hit.point;
                            newSpawnable.transform.Rotate(Vector3.up, UnityEngine.Random.Range(spawnable.rotationRange.x, spawnable.rotationRange.y), Space.Self);
                            newSpawnable.transform.rotation = Quaternion.Lerp(newSpawnable.transform.rotation, newSpawnable.transform.rotation * Quaternion.FromToRotation(newSpawnable.transform.up, hit.normal), spawnable.rotateTowardsNormal);
                            newSpawnable.transform.localScale = new Vector3
                            (
                                UnityEngine.Random.Range(spawnable.minScale.x, spawnable.maxScale.x),
                                UnityEngine.Random.Range(spawnable.minScale.y, spawnable.maxScale.y),
                                UnityEngine.Random.Range(spawnable.minScale.z, spawnable.maxScale.z)
                            );
                            
                            newSpawnable.SetActive(true);
                            
                            // Invoke the callback to return the instantiated object
                            onComplete?.Invoke(newSpawnable);
                        }
                        else
                        {
                            Debug.LogError("Failed to instantiate prefab asynchronously.");
                            onComplete?.Invoke(null);
                        }
                    };
                }
            }
            else
            {
                // No valid hit, return null via the callback
                onComplete?.Invoke(null);
            }
        }
        else if (spawnable.isWater)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(spawnable.prefabPath);

            handle.Completed += (asyncHandle) =>
            {
                if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject newSpawnable = asyncHandle.Result;

                    newSpawnable.transform.parent = parent;
                    newSpawnable.transform.position = new Vector3(chunkPos.x, spawnable.waterWorldHeight, chunkPos.y);

                    newSpawnable.SetActive(true);

                    // Invoke the callback to return the instantiated object
                    onComplete?.Invoke(newSpawnable);
                }
                else
                {
                    Debug.LogError("Failed to instantiate prefab asynchronously.");
                    onComplete?.Invoke(null);
                }
            };
        }
    }
}
