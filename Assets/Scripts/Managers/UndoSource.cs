using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using MyBox;
using System.Reflection;
using System;

[RequireComponent(typeof(PhotonView))]
public class UndoSource : MonoBehaviour
{
    public Dictionary<string, MethodInfo> methodDictionary = new();
    [ReadOnly] public PhotonView pv;

    public void MultiFunction(string methodName, RpcTarget affects, object[] parameters = null)
    {
        if (!methodDictionary.ContainsKey(methodName))
            AddToMethodDictionary(methodName);

        MethodInfo info = methodDictionary[methodName];

        if (PhotonNetwork.IsConnected)
        {
            pv.RPC(info.Name, affects, parameters);
        }
        else if (affects != RpcTarget.Others)
        {
            if (info.ReturnType == typeof(IEnumerator))
                StartCoroutine((IEnumerator)info.Invoke(this, parameters));
            else
                info.Invoke(this, parameters);
        }
    }

    public void MultiFunction(string methodName, Photon.Realtime.Player specificPlayer = null, object[] parameters = null)
    {
        if (!methodDictionary.ContainsKey(methodName))
            AddToMethodDictionary(methodName);

        MethodInfo info = methodDictionary[methodName];

        if (PhotonNetwork.IsConnected && specificPlayer != null)
            pv.RPC(info.Name, specificPlayer, parameters);
        else if (info.ReturnType == typeof(IEnumerator))
            StartCoroutine((IEnumerator)info.Invoke(this, parameters));
        else
            info.Invoke(this, parameters);
    }

    protected virtual void AddToMethodDictionary(string methodName)
    {
    }

    protected void FindMethod(Type type, string methodName)
    {
        MethodInfo method = null;
        Type currentType = type;

        if (methodDictionary.ContainsKey(methodName))
            return;

        try
        {
            while (currentType != null && currentType != typeof(UndoSource) && method == null)
            {
                method = currentType.GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
                );

                currentType = currentType.BaseType;
            }
            if (method != null && (method.ReturnType == typeof(void) || method.ReturnType == typeof(IEnumerator)))
            {
                methodDictionary.Add(methodName, method);
            }
        }
        catch (ArgumentException) { }
        catch
        {
            Debug.LogError($"{this.name}: {methodName} failed");
        }
    }
}