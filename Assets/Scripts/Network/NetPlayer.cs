using System;
using LiveKit;
using UnityEngine;

public class NetPlayer
{
    public delegate void MoveHandler( Vector3 position, Vector3 velocity, float rotationY );

    public MoveHandler Move;

    public delegate void AnimationChangeHandler( int id, float value );

    public AnimationChangeHandler AnimationChange;
    public Action Disconnected;

    public Participant Participant;

    public delegate Vector3 GetPositionHandler();

    public GetPositionHandler GetPosition;
}