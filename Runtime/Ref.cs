using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 

[System.Serializable]
[HideReferenceObjectPicker]
public class Ref<T>
{
    public T Value;

    public Ref() { }
    public Ref(T value) { Value = value; }

    public static implicit operator T(Ref<T> r) => r.Value;
    public static implicit operator Ref<T>(T v) => new(v);


}

