using MMDIKBakerLibrary.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MMDIKBakerTest
{


    /// <summary>
    ///MatrixTest のテスト クラスです。すべての
    ///MatrixTest 単体テストをここに含めます
    ///</summary>
    [TestClass()]
    public class MatrixTest
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
        ///Decompose のテスト
        ///</summary>
        [TestMethod()]
        public void DecomposeTest()
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix target = new Matrix();
            Vector3[] scaleTestPatterns = { Vector3.One, new Vector3(5, 1, 1), new Vector3(1, 5, 1), new Vector3(1, 1, 5), new Vector3(5, 5, 1), new Vector3(5, 1, 5), new Vector3(1, 5, 5), new Vector3(5, 5, 5) };
            Quaternion[] rotationTestPatterns = { Quaternion.Identity, Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0.5m, 0.5m, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0, 0.5m, 0.5m), 0.72m) };
            Vector3[] transrationTestPatterns = { Vector3.Zero, Vector3.One, new Vector3(5, 1, 1), new Vector3(1, 5, 1), new Vector3(1, 1, 5), new Vector3(5, 5, 1), new Vector3(5, 1, 5), new Vector3(1, 5, 5), new Vector3(5, 5, 5) };

            foreach (Vector3 scaleTestPattern in scaleTestPatterns)
            {
                foreach (Quaternion rotationTestPattern in rotationTestPatterns)
                {
                    foreach (Vector3 transrationTestPattern in transrationTestPatterns)
                    {
                        Matrix.Compose(scaleTestPattern, rotationTestPattern, transrationTestPattern, out target);
                        rotationTestPattern.Normalize();
                        target.Decompose(out scale, out rotation, out translation);
                        Vector3 scaleExpected = MathHelper.Round(scaleTestPattern, 5);
                        scale = MathHelper.Round(scale, 5);
                        Quaternion rotationExpected = MathHelper.Round(rotationTestPattern, 5);
                        rotation = MathHelper.Round(rotation, 5);
                        Vector3 translationExpected = MathHelper.Round(transrationTestPattern, 5);
                        translation = MathHelper.Round(translation, 5);
                        Assert.AreEqual(scaleExpected.X, scale.X);
                        Assert.AreEqual(scaleExpected.Y, scale.Y);
                        Assert.AreEqual(scaleExpected.Z, scale.Z);
                        Assert.AreEqual((double)rotationExpected.X, (double)rotation.X);
                        Assert.AreEqual(rotationExpected.Y, rotation.Y);
                        Assert.AreEqual(rotationExpected.Z, rotation.Z);
                        Assert.AreEqual(rotationExpected.W, rotation.W);
                        Assert.AreEqual(translationExpected.X, translation.X);
                        Assert.AreEqual(translationExpected.Y, translation.Y);
                        Assert.AreEqual(translationExpected.Z, translation.Z);
                    }
                }
            }
        }

        /// <summary>
        ///Invert のテスト
        ///</summary>
        [TestMethod()]
        public void InvertTest()
        {
            Matrix matrix;
            Matrix result; // TODO: 適切な値に初期化してください
            Vector3[] scaleTestPatterns = { Vector3.One, new Vector3(5, 1, 1), new Vector3(1, 5, 1), new Vector3(1, 1, 5), new Vector3(5, 5, 1), new Vector3(5, 1, 5), new Vector3(1, 5, 5), new Vector3(5, 5, 5) };
            Quaternion[] rotationTestPatterns = { Quaternion.Identity, Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0.5m, 0.5m, 0), 0.72m), Quaternion.CreateFromAxisAngle(new Vector3(0, 0.5m, 0.5m), 0.72m) };
            Vector3[] transrationTestPatterns = { Vector3.Zero, Vector3.One, new Vector3(5, 1, 1), new Vector3(1, 5, 1), new Vector3(1, 1, 5), new Vector3(5, 5, 1), new Vector3(5, 1, 5), new Vector3(1, 5, 5), new Vector3(5, 5, 5) };

            foreach (Vector3 scaleTestPattern in scaleTestPatterns)
            {
                foreach (Quaternion rotationTestPattern in rotationTestPatterns)
                {
                    foreach (Vector3 transrationTestPattern in transrationTestPatterns)
                    {
                        rotationTestPattern.Normalize();
                        Matrix.Compose(scaleTestPattern, rotationTestPattern, transrationTestPattern, out matrix);
                        Matrix temp;
                        Matrix.Invert(ref matrix, out temp);
                        Matrix.Invert(ref temp, out result);
                        matrix = MathHelper.Round(matrix, 5);
                        result = MathHelper.Round(result, 5);
                        Assert.IsTrue(Math.Abs(matrix.M11 - result.M11) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M12 - result.M12) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M13 - result.M13) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M14 - result.M14) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M21 - result.M21) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M22 - result.M22) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M23 - result.M23) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M24 - result.M24) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M31 - result.M31) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M32 - result.M32) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M33 - result.M33) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M34 - result.M34) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M41 - result.M41) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M42 - result.M42) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M43 - result.M43) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M44 - result.M44) < 0.001m);
                        Matrix.Multiply(ref temp, ref matrix, out result);
                        matrix = Matrix.Identity;
                        result = MathHelper.Round(result, 5);
                        Assert.IsTrue(Math.Abs(matrix.M11 - result.M11) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M12 - result.M12) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M13 - result.M13) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M14 - result.M14) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M21 - result.M21) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M22 - result.M22) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M23 - result.M23) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M24 - result.M24) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M31 - result.M31) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M32 - result.M32) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M33 - result.M33) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M34 - result.M34) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M41 - result.M41) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M42 - result.M42) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M43 - result.M43) < 0.001m);
                        Assert.IsTrue(Math.Abs(matrix.M44 - result.M44) < 0.001m);
                        
                    }
                }
            }
        }
    }
}
