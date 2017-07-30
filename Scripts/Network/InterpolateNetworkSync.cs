using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolateNetworkSync : Photon.MonoBehaviour {

    public SyncType syncType = SyncType.PositionAndRotation;

    bool syncPosition { get { return syncType == SyncType.PositionAndRotation || syncType == SyncType.PositionOnly; } }
    bool syncRotation { get { return syncType == SyncType.PositionAndRotation || syncType == SyncType.RotationOnly; } }

    public enum SyncType
    {
        PositionAndRotation,
        PositionOnly,
        RotationOnly
    }

    internal struct State
    {
        internal double timestamp;
        internal Vector3 pos;
        internal Quaternion rot;
    }

    // We store twenty states with "playback" information
    private State[] m_BufferedState = new State[20];

    // Keep track of what slots are used
    private int m_TimestampCount;

    double interpolationDelay = 0.15;

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            if (syncPosition)
            {
                Vector3 pos = transform.localPosition;
                stream.Serialize(ref pos);
            }

            if (syncRotation)
            {
                Quaternion rot = transform.localRotation;
                stream.Serialize(ref rot);
            }
        }
        else if (stream.isReading)
        {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if (syncPosition)
                stream.Serialize(ref pos);

            if (syncRotation)
                stream.Serialize(ref rot);

            // Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
            for (int i = m_BufferedState.Length - 1; i >= 1; i--)
            {
                m_BufferedState[i] = m_BufferedState[i - 1];
            }

            // Save currect received state as 0 in the buffer, safe to overwrite after shifting
            State state;
            state.timestamp = info.timestamp;
            state.pos = pos;
            state.rot = rot;

            m_BufferedState[0] = state;

            // Increment state count but never exceed buffer size
            m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);

            // Check integrity, lowest numbered state in the buffer is newest and so on
            for (int i = 0; i < m_TimestampCount - 1; i++)
            {
                if (m_BufferedState[i].timestamp < m_BufferedState[i + 1].timestamp)
                {
                    Debug.Log("State inconsistent");
                }
            }
        }
    }

    void Update()
    {
        if (photonView.isMine || !PhotonNetwork.inRoom)
        {
            return;     // if this object is under our control, we don't need to apply received position-updates 
        }
        double currentTime = PhotonNetwork.time;
        double interpolationTime = currentTime - this.interpolationDelay;

        // We have a window of InterpolationDelay where we basically play back old updates.
        // By having InterpolationDelay the average ping, you will usually use interpolation.
        // And only if no more data arrives we will use the latest known position.

        // Use interpolation, if the interpolated time is still "behind" the update timestamp time:
        if (m_BufferedState[0].timestamp > interpolationTime)
        {
            for (int i = 0; i < m_TimestampCount; i++)
            {
                // Find the state which matches the interpolation time (time+0.1) or use last state
                if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
                {
                    // The state one slot newer (<100ms) than the best playback state
                    State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
                    // The best playback state (closest to 100 ms old (default time))
                    State lhs = m_BufferedState[i];

                    // Use the time between the two slots to determine if interpolation is necessary
                    double diffBetweenUpdates = rhs.timestamp - lhs.timestamp;
                    float t = 0.0f;
                    // As the time difference gets closer to 100 ms t gets closer to 1 in 
                    // which case rhs is only used
                    if (diffBetweenUpdates > 0.0001)
                    {
                        t = (float)((interpolationTime - lhs.timestamp) / diffBetweenUpdates);
                    }

                    // if t=0 => lhs is used directly
                    if (syncPosition)
                        transform.localPosition = Vector3.Lerp(lhs.pos, rhs.pos, t);
                    if (syncRotation)
                        transform.localRotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                    return;
                }
            }
        }
        else
        {
            // If our interpolation time catched up with the time of the latest update:
            // Simply move to the latest known position.

            State latest = this.m_BufferedState[0];

            if (syncPosition)
                transform.localPosition = Vector3.Lerp(transform.localPosition, latest.pos, Time.deltaTime * 20);
            if (syncRotation)
                transform.localRotation = latest.rot;
        }
    }
}
