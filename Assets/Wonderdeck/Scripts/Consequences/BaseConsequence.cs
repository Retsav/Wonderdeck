using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseConsequence
{
    public virtual void Init() => Zenject.ProjectContext.Instance.Container.Inject(this);
    public abstract void ApplyConsequence();
    public abstract void RemoveConsequence();
}
