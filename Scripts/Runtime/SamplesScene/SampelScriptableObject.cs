using UnityEngine;
using UnityEngine.UI;

namespace DG_with_Localization
{
    [CreateAssetMenu(fileName = "SampelScriptableObject", menuName = "Dialog Graph/SampelScriptableObject")]
    public class SampelScriptableObject : ScriptableObject
    {
        private bool testValue = false;

        public bool TestBooleanMethod() => true;
        public bool TestBooleanMethod(bool test) => test;
        public bool CompareStringTest(string s1, string s2) => string.Equals(s1, s2);
        public bool CompareObjectTest(UnityEngine.Object o1, UnityEngine.Object o2) => UnityEngine.Object.Equals(o1, o2);

        public void SetValueTest(bool value) => testValue = value;
        public bool GetValueTest() => testValue;

        public void TestMethodProperty(string v) => Debug.Log(v);
        public void TestMethodProperty(int v) => Debug.Log(v);
        public void TestMethodProperty(float v) => Debug.Log(v);
        public void TestMethodProperty(bool v) => Debug.Log(v);
        public void TestMethodProperty(UnityEngine.Object v) => Debug.Log(v.name);
        public void TestMethodProperty(Image img) => Debug.Log(img.name);

        public void TestSubstr(string s1, string s2) => Debug.Log(s1 + s2);
    }
}
