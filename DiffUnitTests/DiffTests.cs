using DiffAPI.Controllers;
using DiffAPI.Enums;
using DiffAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace DiffTests
{
    [TestClass]
    public class DiffTests
    {
        #region methods
        private bool AreDictionariesEqual(Dictionary<int, Dictionary<Position, string>> dict1, Dictionary<int, Dictionary<Position, string>> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (int key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                {
                    return false;
                }

                if (dict1[key].Count != dict2[key].Count)
                {
                    return false;
                }

                foreach (Position key2 in dict1[key].Keys)
                {
                    if (!dict2[key].ContainsKey(key2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        private bool AreListDiffsEqual(List<Diffs> list1, List<Diffs> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }
            foreach (Diffs diff in list1)
            {
                if (!list2.Contains(diff))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region AreLeftAndRightExistanceValid
        [TestMethod]
        public void AreLeftAndRightExistanceValid_BothNull()
        {
            DiffController controller = new DiffController();
            bool result = controller.AreLeftAndRightExistanceValid(1);
            Assert.IsFalse(result);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void AreLeftAndRightExistanceValid_LeftNullRightNotNull()
        {
            DiffController controller = new DiffController();
            controller.Right(1, "AAA=");

            bool result = controller.AreLeftAndRightExistanceValid(1);
            Assert.IsFalse(result);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void AreLeftAndRightExistanceValid_LeftNotNullRightNull()
        {
            DiffController controller = new DiffController();
            controller.Left(1, "AAAA");

            bool result = controller.AreLeftAndRightExistanceValid(1);
            Assert.IsFalse(result);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void AreLeftAndRightExistanceValid_LeftNotNullRightNotNull()
        {
            DiffController controller = new DiffController();
            controller.Left(1, "AAAA");
            controller.Right(1, "AAA=");

            bool result = controller.AreLeftAndRightExistanceValid(1);
            Assert.IsTrue(result);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void AreLeftAndRightExistanceValid_LeftEmptyRightNotNull()
        {
            DiffController controller = new DiffController();
            controller.Left(1, "");
            controller.Right(1, "AAA=");

            bool result = controller.AreLeftAndRightExistanceValid(1);
            Assert.IsFalse(result);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void AreLeftAndRightExistanceValid_LeftEmptyRightEmpty()
        {
            DiffController controller = new DiffController();
            controller.Left(1, "");
            controller.Right(1, "");

            bool result = controller.AreLeftAndRightExistanceValid(1);
            Assert.IsFalse(result);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void AreLeftAndRightExistanceValid_LeftNotNullRightEmpty()
        {
            DiffController controller = new DiffController();
            controller.Left(1, "AAA=");
            controller.Right(1, "");

            bool result = controller.AreLeftAndRightExistanceValid(1);
            Assert.IsFalse(result);

            controller.ResetDictStoredData();
        }
        #endregion

        #region IsJsonValid
        [TestMethod]
        public void IsJsonValid_EmptyString()
        {
            DiffController controller = new DiffController();
            bool result = controller.IsJsonValid("");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsJsonValid_NotRightLength()
        {
            DiffController controller = new DiffController();
            bool result = controller.IsJsonValid("A");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsJsonValid_InvalidChar()
        {
            DiffController controller = new DiffController();
            bool result = controller.IsJsonValid("AA%=");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsJsonValid_ValidOncePadded()
        {
            DiffController controller = new DiffController();
            bool result = controller.IsJsonValid("AAA=");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsJsonValid_ValidTwicePadded()
        {
            DiffController controller = new DiffController();
            bool result = controller.IsJsonValid("AA==");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsJsonValid_ValidThreeTimesPadded()
        {
            DiffController controller = new DiffController();
            bool result = controller.IsJsonValid("A===");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsJsonValid_ValidNotPadded()
        {
            DiffController controller = new DiffController();
            bool result = controller.IsJsonValid("AAAA");
            Assert.IsTrue(result);
        }
        #endregion

        #region DecodeBase64ToString
        [TestMethod]
        public void DecodeBase64ToString_1()
        {
            DiffController controller = new DiffController();
            string result = controller.DecodeBase64ToString("AAAAAA==");
            Assert.AreEqual(result, "0000");
        }

        [TestMethod]
        public void DecodeBase64ToString_2()
        {
            DiffController controller = new DiffController();
            string result = controller.DecodeBase64ToString("AQABAQ==");
            Assert.AreEqual(result, "1011");
        }

        [TestMethod]
        public void DecodeBase64ToString_3()
        {
            DiffController controller = new DiffController();
            string result = controller.DecodeBase64ToString("AAA=");
            Assert.AreEqual(result, "00");
        }
        #endregion

        #region SaveData
        [TestMethod]
        public void SaveData_newLeft()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult result = controller.SaveData(1, Position.Left, "00");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Left] = "00";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void SaveData_oldLeft()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult _ = controller.SaveData(1, Position.Left, "00");  // assume based on previous test, that this works
            IActionResult result = controller.SaveData(1, Position.Left, "0000");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Left] = "0000";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void SaveData_newRight()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult result = controller.SaveData(1, Position.Right, "00");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Right] = "00";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void SaveData_oldRight()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult _ = controller.SaveData(1, Position.Right, "00");  // assume based on previous test, that this works
            IActionResult result = controller.SaveData(1, Position.Right, "0000");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Right] = "0000";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void SaveData_newLeftWithOldRight()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult _ = controller.SaveData(1, Position.Right, "0000");
            IActionResult result = controller.SaveData(1, Position.Left, "00");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Left] = "00";
            dictData[1][Position.Right] = "0000";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void SaveData_oldLeftWithOldRight()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult x = controller.SaveData(1, Position.Right, "1000");
            IActionResult _ = controller.SaveData(1, Position.Left, "00");
            IActionResult result = controller.SaveData(1, Position.Left, "0000");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Left] = "0000";
            dictData[1][Position.Right] = "1000";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void SaveData_differentId()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult x = controller.SaveData(1, Position.Right, "1000");
            IActionResult _ = controller.SaveData(1, Position.Left, "00");
            IActionResult result = controller.SaveData(2, Position.Left, "0000");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Left] = "00";
            dictData[1][Position.Right] = "1000";
            dictData[2] = new Dictionary<Position, string>();
            dictData[2][Position.Left] = "0000";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }
        #endregion

        #region GetDiffs
        [TestMethod]
        public void GetDiffs_equal()
        {
            // test return value
            DiffController controller = new DiffController();
            List<Diffs> result = controller.GetDiffs("00", "00");

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void GetDiffs()
        {
            // test return value
            DiffController controller = new DiffController();
            List<Diffs> result = controller.GetDiffs("0000", "1011");

            List<Diffs> diffs = [new Diffs() { Offset = 0, Length = 1 }, new Diffs() { Offset = 2, Length = 2 }];

            Assert.IsNotNull(result);
            Assert.IsTrue(AreListDiffsEqual(diffs, result));
        }
        #endregion

        #region GetComparison
        [TestMethod]
        public void GetComparison_SizeDoNotMatch()
        {
            DiffController controller = new DiffController();
            IActionResult x = controller.SaveData(1, Position.Left, "00");
            IActionResult y = controller.SaveData(1, Position.Right, "100");
            OutputForm result = controller.GetComparison(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.DiffResultType, "SizeDoNotMatch");
            Assert.IsNull(result.Diffs);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void GetComparison_Equals()
        {
            DiffController controller = new DiffController();
            IActionResult x = controller.SaveData(1, Position.Left, "100");
            IActionResult y = controller.SaveData(1, Position.Right, "100");
            OutputForm result = controller.GetComparison(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.DiffResultType, "Equals");
            Assert.IsNull(result.Diffs);

            controller.ResetDictStoredData();
        }

        [TestMethod]
        public void GetComparison_ContentDoNotMatch()
        {
            DiffController controller = new DiffController();
            IActionResult x = controller.SaveData(1, Position.Left, "0000");
            IActionResult y = controller.SaveData(1, Position.Right, "1011");
            OutputForm result = controller.GetComparison(1);

            List<Diffs> diffs = [new Diffs() { Offset = 0, Length = 1 }, new Diffs() { Offset = 2, Length = 2 }];

            Assert.IsNotNull(result);
            Assert.AreEqual(result.DiffResultType, "ContentDoNotMatch");
            Assert.IsNotNull(result.Diffs);
            Assert.IsTrue(AreListDiffsEqual(result.Diffs, diffs));

            controller.ResetDictStoredData();
        }
        #endregion

        #region Left
        [TestMethod]
        public void Left_empty()
        {
            DiffController controller = new DiffController();
            IActionResult result = controller.Left(1, "");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [TestMethod]
        public void Left_new()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult result = controller.Left(1, "AAA=");
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Left] = "00";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }
        #endregion

        #region Diff
        [TestMethod]
        public void Diff_notFound()
        {
            DiffController controller = new DiffController();
            IActionResult result = controller.Diff(1);
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [TestMethod]
        public void Diff_ok()
        {
            // test return value
            DiffController controller = new DiffController();
            IActionResult x = controller.Left(1, "AAAAAA==");
            IActionResult y = controller.Right(1, "AQABAQ==");
            IActionResult result = controller.Diff(1);
            ObjectResult objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);

            // test change of stored data
            Dictionary<int, Dictionary<Position, string>> dictData = new Dictionary<int, Dictionary<Position, string>>();
            dictData[1] = new Dictionary<Position, string>();
            dictData[1][Position.Left] = "0000";
            dictData[1][Position.Right] = "1011";
            Assert.IsTrue(AreDictionariesEqual(dictData, DiffController.dictStoredData));

            controller.ResetDictStoredData();
        }
        #endregion
    }
}