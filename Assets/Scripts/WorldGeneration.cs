using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    // Configurable Parameters
    [SerializeField] private bool _randomizeSeed;
    [SerializeField] private int _seed;
    [SerializeField] private int _chunkSize;
    [SerializeField] private int _chunkLoadRadius;
    [SerializeField] private GameObject _craterPrefab;
    [SerializeField] private Transform _craterParent;
    [SerializeField] private Transform _player;

    [System.Serializable]
    private struct CraterType
    {
        public string name;
        public float spawnBlockRadius;
        public float spawnProbability;
        public Sprite[] spriteOptions;
    }
    [SerializeField] private CraterType[] _craterTypes;

    // Internal variables
    private struct CraterCandidate
    {
        public CraterType type;
        public Vector2Int absolutePos;
    }

    private Vector2Int _previousPlayerChunk;
    private Dictionary<Vector2Int, List<GameObject>> _loadedCraters;

    // Unity methods
    private void Awake()
    {
        if (_randomizeSeed)
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
        }
    }

    private void Start()
    {
        _loadedCraters = new();
        _previousPlayerChunk = new Vector2Int(
            (int)System.Math.Floor(_player.position.x / _chunkSize),
            (int)System.Math.Floor(_player.position.y / _chunkSize)
        );
        for (int x = -1*_chunkLoadRadius; x <= _chunkLoadRadius; x++)
        {
            for (int y = -1*_chunkLoadRadius; y <= _chunkLoadRadius; y++)
            {
                GenChunk(new Vector2Int(_previousPlayerChunk.x + x, _previousPlayerChunk.y + y));
            }
        }
    }

    private void Update()
    {
        Vector2Int playerChunk = new Vector2Int(
            (int)System.Math.Floor(_player.position.x / _chunkSize),
            (int)System.Math.Floor(_player.position.y / _chunkSize)
        );

        if (playerChunk != _previousPlayerChunk)
        {
            HashSet<Vector2Int>[] chunkDiff = ChunkDiff(_previousPlayerChunk, playerChunk);
            // generate new chunks
            foreach (var chunk in chunkDiff[0])
            {
                GenChunk(chunk);
            }
            // cull old chunks
            foreach (var chunk in chunkDiff[1])
            {
                foreach (var craterObject in _loadedCraters[chunk])
                {
                    Destroy(craterObject);
                }
                _loadedCraters.Remove(chunk);
            }
        }

        _previousPlayerChunk = playerChunk;
    }


    // Class methods
    private void GenChunk(Vector2Int chunkCoord)
    {
        int chunkSeed = HashChunkCoord(chunkCoord);
        Random.State originalState = Random.state;
        Random.InitState(chunkSeed);

        List<CraterCandidate> generatedCraters = new();

        foreach (var candidate in ChunkCraterCandidates(chunkCoord))
        {
            if (IsValidCraterLocation(chunkCoord, candidate, generatedCraters))
            {
                GameObject newCrater = SpawnCrater(candidate);
                if (!_loadedCraters.ContainsKey(chunkCoord))
                {
                    _loadedCraters[chunkCoord] = new();
                }
                _loadedCraters[chunkCoord].Add(newCrater);
                generatedCraters.Add(candidate);
            }
        }

        Random.state = originalState;
    }

    private List<CraterCandidate> ChunkCraterCandidates(Vector2Int chunkCoord)
    {
        int chunkSeed = HashChunkCoord(chunkCoord);
        Random.State originalState = Random.state;
        Random.InitState(chunkSeed);

        List<CraterCandidate> candidates = new();

        foreach (var type in _craterTypes)
        {
            while (Random.Range(0f, 1f) < type.spawnProbability)
            {
                Vector2Int localSpawnPos = new Vector2Int(Random.Range(0, _chunkSize), Random.Range(0, _chunkSize));
                Vector2Int absSpawnPos = new Vector2Int(chunkCoord.x * _chunkSize + localSpawnPos.x, chunkCoord.y * _chunkSize + localSpawnPos.y);
                candidates.Add(new CraterCandidate { type = type, absolutePos = absSpawnPos });
            }
        }

        Random.state = originalState;

        return candidates;
    }

    private bool IsValidCraterLocation(Vector2Int chunkCoord, CraterCandidate candidate, List<CraterCandidate> generatedCraters)
    {
        // special case to avoid spawning craters on the lander
        if (System.Math.Abs(candidate.absolutePos.x) < 20 + candidate.type.spawnBlockRadius && System.Math.Abs(candidate.absolutePos.y) < 20 + candidate.type.spawnBlockRadius)
        {
            return false;
        }

        Vector2Int[] superiorChunkOffsets = { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1), new Vector2Int(-1, 0) };
        List<CraterCandidate> allCandidatesToAvoid = new(generatedCraters);
        foreach (var chunkOffset in superiorChunkOffsets)
        {
            allCandidatesToAvoid.AddRange(ChunkCraterCandidates(new Vector2Int(chunkCoord.x + chunkOffset.x, chunkCoord.y + chunkOffset.y)));
        }

        bool isValid = true;
        foreach (var avoidCandidate in allCandidatesToAvoid)
        {
            if (Vector2Int.Distance(avoidCandidate.absolutePos, candidate.absolutePos) < avoidCandidate.type.spawnBlockRadius + candidate.type.spawnBlockRadius)
            {
                isValid = false;
                break;
            }
        }
        return isValid;
    }

    private GameObject SpawnCrater(CraterCandidate candidate)
    {
        Sprite sprite = candidate.type.spriteOptions[Random.Range(0, candidate.type.spriteOptions.Length)];
        
        GameObject newCrater = Instantiate(
            _craterPrefab,
            new Vector3(candidate.absolutePos.x, candidate.absolutePos.y, 0f),
            Quaternion.identity,
            _craterParent
        );
        newCrater.GetComponent<SpriteRenderer>().sprite = sprite;
        newCrater.GetComponent<PolygonCollider2D>().CreateFromSprite(sprite);
        return newCrater; 
    }

    private int HashChunkCoord(Vector2Int chunkCoord)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + _seed;
            hash = hash * 23 + chunkCoord.x;
            hash = hash * 23 + chunkCoord.y;
            return hash;
        }
    }

    private HashSet<Vector2Int>[] ChunkDiff(Vector2Int oldCenter, Vector2Int newCenter)
    {
        HashSet<Vector2Int> oldChunks = new();
        HashSet<Vector2Int> newChunks = new();

        for (int x = -1*_chunkLoadRadius; x <= _chunkLoadRadius; x++)
        {
            for (int y = -1*_chunkLoadRadius; y <= _chunkLoadRadius; y++)
            {
                oldChunks.Add(new Vector2Int(oldCenter.x + x, oldCenter.y + y));
                newChunks.Add(new Vector2Int(newCenter.x + x, newCenter.y + y));
            }
        }

        HashSet<Vector2Int> newChunksToLoad = newChunks.Except(oldChunks).ToHashSet();
        HashSet<Vector2Int> oldChunksToCull = oldChunks.Except(newChunks).ToHashSet();

        HashSet<Vector2Int>[] result = { newChunksToLoad, oldChunksToCull };
        return result;
    }

}