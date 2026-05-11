using UnityEngine;
using System;

public class CooldownDecorator : BTNode
{
    private BTNode child;
    private float cooldownTime;
    private float lastEndTime = -999f;

    public CooldownDecorator(string name, BTNode child, float cooldownTime) : base(name)
    {
        this.child = child;
        this.cooldownTime = cooldownTime;
    }

    public override BTStatus Tick()
    {
        if (Time.time - lastEndTime < cooldownTime)
            return BTStatus.Failure;

        BTStatus status = child.Tick();

        if (status != BTStatus.Running)
            lastEndTime = Time.time;

        return status;
    }
}

public class TimeoutDecorator : BTNode
{
    private BTNode child;
    private float timeoutDuration;
    private float startTime = -1f;

    public TimeoutDecorator(string name, BTNode child, float timeoutDuration) : base(name)
    {
        this.child = child;
        this.timeoutDuration = timeoutDuration;
    }

    public override BTStatus Tick()
    {
        if (startTime < 0f)
            startTime = Time.time;

        if (Time.time - startTime > timeoutDuration)
        {
            startTime = -1f;
            return BTStatus.Failure;
        }

        BTStatus status = child.Tick();

        if (status != BTStatus.Running)
            startTime = -1f;

        return status;
    }
}

public class ConditionalAbortDecorator : BTNode
{
    private BTNode child;
    private Func<bool> condition;

    public ConditionalAbortDecorator(string name, BTNode child, Func<bool> condition) : base(name)
    {
        this.child = child;
        this.condition = condition;
    }

    public override BTStatus Tick()
    {
        if (!condition())
            return BTStatus.Failure;

        return child.Tick();
    }
}