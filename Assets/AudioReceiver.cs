using System.Collections.Generic;
using UnityEngine;

public class AudioReceiver : MonoBehaviour
{
    public int numberOfRays = 360;
    public float rayDistance = 10f;
    public LayerMask bounceLayers;
    public LayerMask audioSourceLayer;
    public LayerMask audioReceiverLayer;
    public float maxAudioDistance = 200f;
    [SerializeField, Min(0)] public int maxBounces = 1;
    public float pulseInterval = 0.1f;
    private float pulseTimer;

    public List<GameObject> audioSources = new List<GameObject>();
    private Dictionary<GameObject, int> audioSourceLOSCount;

    // echo
    private int totalBouncesFromCast = 0;
    private int echoRayCount = 0;
    private double totalDistanceOfEchoRays = 0;


    // audio muffling
    [SerializeField] private float minCutoffFrequency = 500f;
    [SerializeField] private float maxCutoffFrequency = 22000f;
    

    private void OnValidate()
    {
        audioSourceLOSCount = new Dictionary<GameObject, int>();
        foreach(var source in audioSources)
        {
            audioSourceLOSCount.Add(source, 0); // init all to 0
        }
    }

    public void AddAudioSource(GameObject source)
    {
        audioSourceLOSCount.Add(source, 0);
    }

    public bool RemoveAudioSource(GameObject source)
    {
        try
        {
            return audioSourceLOSCount.Remove(source);
        }
        catch (System.Exception)
        {
            return false;
            throw;
        }
    }

    private void CastRays(Vector3 origin)
    {
        List<Vector3> directions = GenerateRayDirections(numberOfRays);

        totalBouncesFromCast = 0;
        echoRayCount = 0;
        totalDistanceOfEchoRays = 0;

        foreach(var dir in directions)
        {
            CastBouncingRay(origin, dir, maxBounces, rayDistance);
            AudioSourceLOS(origin); // check if player directly has los on audio source instead of just on ray bounce
        }

        // compute echo stuff
        Debug.Log($"{totalBouncesFromCast} bounces, {echoRayCount}/{totalBouncesFromCast} were echo rays ({(float)echoRayCount / totalBouncesFromCast * 100}%).\nThe average ray length was {totalDistanceOfEchoRays / echoRayCount}");

        foreach(var kvp in audioSourceLOSCount) // compute source los stuff
        {
            float percent = GetMufflePercent(kvp.Value);
            UpdateAudio(kvp.Key, Mathf.Clamp01(percent));
        }
        
        /*
        need to reset the LOS rays of each individual audio source
        */
        foreach(var source in audioSources)
        {
            try
            {
                audioSourceLOSCount[source] = 0;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }

    private float GetMufflePercent(int losRays)
    {
        return (float)losRays / numberOfRays;
    }

    private void Update()
    {
        pulseTimer += Time.deltaTime;
        if (pulseTimer >= pulseInterval)
        {
            pulseTimer = 0f;

            Vector3 origin = transform.position;
            CastRays(origin);
        }
    }

    private void UpdateAudio(GameObject source, float percent)
    {
        if(source.TryGetComponent<AudioLowPassFilter>(out var muffleFilter))
        {
           float cutoff = Mathf.Lerp(minCutoffFrequency, maxCutoffFrequency, percent);
           muffleFilter.cutoffFrequency = cutoff;
           Debug.Log($"Set {source.name} cutoff to: {cutoff}");
        }

        if(source.TryGetComponent<AudioEchoFilter>(out var echoFilter))
        {
            double averageRayLength = totalDistanceOfEchoRays / echoRayCount;
            float rayHitPercent = (float)echoRayCount / totalBouncesFromCast;
            echoFilter.wetMix = rayHitPercent; // change wet mix to echo % (rays over bounces)
            echoFilter.delay = Mathf.Lerp(0f, 50f, (float)averageRayLength / 50f);
            echoFilter.decayRatio = Mathf.Lerp(0.3f, 0.7f, rayHitPercent);
        }
    }

    List<Vector3> GenerateRayDirections(int count)
    {
        List<Vector3> directions = new List<Vector3>();

        float offset = 2f / count;
        float increment = Mathf.PI * (3f - Mathf.Sqrt(5f)); // golden angle

        for(int i = 0; i < count; i++)
        {
            float y = (i * offset) - 1 + (offset / 2f);
            float r = Mathf.Sqrt(1f - y * y);

            float phi = i * increment;

            float x = Mathf.Cos(phi) * r;
            float z = Mathf.Sin(phi) * r;

            // y = 0; // 2d switch

            directions.Add(new Vector3(x, y, z));
        }

        return directions;
    }

    /*
    Adding echo to these rays
    1. on bounce check for line of sight to the receiver
    2. record length of each ray that has line of sight
        record what % of rays have line of sight
    */
    public void CastBouncingRay(Vector3 origin, Vector3 dir, int bouncesRemaining, float dist)
    {
        if(dist <= 0) return;

        RaycastHit hit;

        // ray hit bounceable surface
        if(Physics.Raycast(origin, dir, out hit, dist, bounceLayers))
        {
            Debug.DrawLine(origin, hit.point, Color.white, 2f, false);
            totalBouncesFromCast++;

            float distToHit = Vector3.Distance(origin, hit.point);
            dist -= distToHit;

            AudioSourceLOS(hit.point); // check for los to any audio source
            EchoLOS(hit.point); // check for los back to receiver for echo

            if(bouncesRemaining > 0 && dist > 0)
            {
                Vector3 newDir = Vector3.Reflect(dir, hit.normal);
                Vector3 newOrigin = hit.point + newDir * 0.01f;
                CastBouncingRay(newOrigin, newDir, bouncesRemaining - 1, dist);
            }
            else
            {
                Debug.DrawLine(origin, hit.point, Color.red, 2f, false);
                // no bounces or no dist
            }
        }
        else
        {
            // ray did not hit
        }
    }

    /*
    Checks only a single audio source, need to:
    1. keep list of audio source transforms in the level
    2. check LOS to all audio sources in the list
    3. adjust muffle of each source
    */
    public bool AudioSourceLOS(Vector3 pos)
    {
        foreach(var source in audioSources)
        {
            Vector3 dir = (source.transform.position - pos).normalized;
            float distance = Vector3.Distance(pos, source.transform.position);

            Collider[] overlaps = Physics.OverlapSphere(pos, 0.01f, bounceLayers);
            Vector3 newPos = pos;
            if(overlaps.Length > 0)
            {
                newPos -= dir * 0.1f;
                distance = Vector3.Distance(newPos, source.transform.position);
            }

            RaycastHit hit;
            if(Physics.Raycast(newPos, dir, out hit, distance, bounceLayers | audioSourceLayer))
            {
                if(IsInLayerMask(hit.transform.gameObject, audioSourceLayer))
                {
                    if(hit.transform.gameObject == source || hit.transform.IsChildOf(source.transform))
                    {
                        // hit audio source
                        Debug.DrawLine(newPos, source.transform.position, Color.green, 2f, false);
                        audioSourceLOSCount[source] ++;
                        return true;
                    }
                }
                else
                {
                    // hit wall first
                }
            }
        }
        return false;
    }

    public bool EchoLOS(Vector3 pos)
    {
        Vector3 dir = (transform.position - pos).normalized;
        float distance = Vector3.Distance(pos, transform.position);

        Collider[] overlaps = Physics.OverlapSphere(pos, 0.01f, bounceLayers);
        Vector3 newPos = pos;
        if(overlaps.Length > 0)
        {
            newPos -= dir * 0.1f;
            distance = Vector3.Distance(newPos, transform.position);
        }

        RaycastHit hit;
        if(Physics.Raycast(newPos, dir, out hit, distance, bounceLayers | audioReceiverLayer))
        {
            if(IsInLayerMask(hit.transform.gameObject, audioReceiverLayer))
            {
                if(hit.transform == transform || hit.transform.IsChildOf(transform)) // if hit this
                {
                    Debug.DrawLine(newPos, transform.position, Color.blue, 2f, false);
                    echoRayCount++;
                    totalDistanceOfEchoRays += Vector3.Distance(newPos, transform.position);
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }

}
