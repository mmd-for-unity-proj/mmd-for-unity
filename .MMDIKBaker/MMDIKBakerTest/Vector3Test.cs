using MMDIKBakerLibrary.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MMDIKBakerTest
{
    
    
    /// <summary>
    ///Vector3Test のテスト クラスです。すべての
    ///Vector3Test 単体テストをここに含めます
    ///</summary>
    [TestClass()]
    public class Vector3Test
    {


        private TestContext testContextInstance;

        /// <summary>
        ///現在のテストの実行についての情報および機能を
        ///提供するテスト コンテキストを取得または設定します。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 追加のテスト属性
        // 
        //テストを作成するときに、次の追加属性を使用することができます:
        //
        //クラスの最初のテストを実行する前にコードを実行するには、ClassInitialize を使用
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //クラスのすべてのテストを実行した後にコードを実行するには、ClassCleanup を使用
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //各テストを実行する前にコードを実行するには、TestInitialize を使用
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //各テストを実行した後にコードを実行するには、TestCleanup を使用
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Transform のテスト
        ///</summary>
        [TestMethod()]
        public void TransformTest()
        {
            Quaternion[] rotationTestPatterns = { Quaternion.Identity, Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0.5m, 0.5m, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0, 0.5m, 0.5m), 0.72m) };
            Vector3[] transrationTestPatterns = { Vector3.Zero, Vector3.One, new Vector3(5, 1, 1), new Vector3(1, 5, 1), new Vector3(1, 1, 5), new Vector3(5, 5, 1), new Vector3(5, 1, 5), new Vector3(1, 5, 5), new Vector3(5, 5, 5) };

            foreach (var rot in rotationTestPatterns)
            {
                foreach (var vec in transrationTestPatterns)
                {
                    Vector3 value = vec, result;
                    Quaternion rotation = rot;
                    Vector3.Transform(ref value, ref rotation, out result);
                    Microsoft.Xna.Framework.Vector3 value2 = new Microsoft.Xna.Framework.Vector3((float)vec.X, (float)vec.Y, (float)vec.Z), actual_xna;
                    Microsoft.Xna.Framework.Quaternion rotation2 = new Microsoft.Xna.Framework.Quaternion((float)rot.X, (float)rot.Y, (float)rot.Z, (float)rot.W);
                    Microsoft.Xna.Framework.Vector3.Transform(ref value2, ref rotation2, out actual_xna);
                    Vector3 acutual = new Vector3((decimal)actual_xna.X, (decimal)actual_xna.Y, (decimal)actual_xna.Z);
                    Assert.IsTrue(Math.Abs(result.X - acutual.X) < 0.001m);
                    Assert.IsTrue(Math.Abs(result.Y - acutual.Y) < 0.001m);
                    Assert.IsTrue(Math.Abs(result.Z - acutual.Z) < 0.001m);

                }
            }
        }
    }
}
