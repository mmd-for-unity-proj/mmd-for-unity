using MMDIKBakerLibrary.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MMDIKBakerTest
{
    
    
    /// <summary>
    ///QuaternionTest のテスト クラスです。すべての
    ///QuaternionTest 単体テストをここに含めます
    ///</summary>
    [TestClass()]
    public class QuaternionTest
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
        ///Slerp のテスト
        ///</summary>
        [TestMethod()]
        public void SlerpTest()
        {
            Quaternion[] QuaternionPatterns = { /*Quaternion.Identity,*/ Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0.5m, 0.5m, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0, 0.5m, 0.5m), 0.72m) };
            decimal[] ratePatterns = { 0m, 0.2m, 0.25m, 0.6m, 0.8m, 1m };

            foreach (Quaternion q1 in QuaternionPatterns)
            {
                foreach (Quaternion q2 in QuaternionPatterns)
                {
                    foreach (decimal rate in ratePatterns)
                    {
                        Quaternion quaternion1 = q1, quaternion2 = q2;
                        Quaternion result;
                        quaternion1.Normalize();
                        quaternion2.Normalize();
                        Quaternion.Slerp(ref quaternion1, ref quaternion2, rate, out result);
                        Microsoft.Xna.Framework.Quaternion xnaq1 = new Microsoft.Xna.Framework.Quaternion((float)q1.X, (float)q1.Y, (float)q1.Z, (float)q1.W);
                        Microsoft.Xna.Framework.Quaternion xnaq2 = new Microsoft.Xna.Framework.Quaternion((float)q2.X, (float)q2.Y, (float)q2.Z, (float)q2.W);
                        Microsoft.Xna.Framework.Quaternion xnaactual;
                        Microsoft.Xna.Framework.Quaternion.Slerp(ref xnaq1, ref xnaq2, (float)rate, out xnaactual);
                        Quaternion actual = new Quaternion { X = (decimal)xnaactual.X, Y = (decimal)xnaactual.Y, Z = (decimal)xnaactual.Z, W = (decimal)xnaactual.W };
                        actual.Normalize();
                        result.Normalize();
                        Assert.AreEqual(Math.Abs(actual.X- result.X)<0.01m, true);
                        Assert.AreEqual(Math.Abs(actual.Y - result.Y) < 0.01m, true);
                        Assert.AreEqual(Math.Abs(actual.Z- result.Z)<0.01m, true);
                        Assert.AreEqual(Math.Abs(actual.W- result.W) < 0.01m, true);
                    }
                }
            }
        }

        /// <summary>
        ///Multiply のテスト
        ///</summary>
        [TestMethod()]
        public void MultiplyTest()
        {
            Quaternion[] QuaternionPatterns = { Quaternion.Identity, Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0.5m, 0.5m, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0, 0.5m, 0.5m), 0.72m) };
            foreach (Quaternion q1 in QuaternionPatterns)
            {
                foreach (Quaternion q2 in QuaternionPatterns)
                {
                    Quaternion quaternion1 = q1, quaternion2 = q2;
                    Quaternion result;
                    quaternion1.Normalize();
                    quaternion2.Normalize();
                    Quaternion.Multiply(ref quaternion1, ref quaternion2, out result);
                    Microsoft.Xna.Framework.Quaternion xnaq1 = new Microsoft.Xna.Framework.Quaternion((float)q1.X, (float)q1.Y, (float)q1.Z, (float)q1.W);
                    Microsoft.Xna.Framework.Quaternion xnaq2 = new Microsoft.Xna.Framework.Quaternion((float)q2.X, (float)q2.Y, (float)q2.Z, (float)q2.W);
                    Microsoft.Xna.Framework.Quaternion xnaactual;
                    Microsoft.Xna.Framework.Quaternion.Multiply(ref xnaq1, ref xnaq2, out xnaactual);
                    Quaternion actual = new Quaternion { X = (decimal)xnaactual.X, Y = (decimal)xnaactual.Y, Z = (decimal)xnaactual.Z, W = (decimal)xnaactual.W };
                    actual.Normalize();
                    result.Normalize();
                    Assert.IsTrue(Math.Abs(actual.X - result.X) < 0.01m);
                    Assert.IsTrue(Math.Abs(actual.Y - result.Y) < 0.01m);
                    Assert.IsTrue(Math.Abs(actual.Z - result.Z) < 0.01m);
                    Assert.IsTrue(Math.Abs(actual.W - result.W) < 0.01m);
                }
            }

        }
    }
}
