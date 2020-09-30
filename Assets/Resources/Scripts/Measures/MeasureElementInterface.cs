using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMeasureData<T>
{
    void AddElement(T elt);
}

public interface IMeasureManager
{
    //void AddData(GameObject obj);
    void Calculate();
    void Display();
    string SaveInstance();
}


public class MeasureElementInterface : MonoBehaviour
{

}

