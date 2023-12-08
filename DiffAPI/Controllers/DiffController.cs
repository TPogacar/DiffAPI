using Microsoft.AspNetCore.Mvc;
using DiffAPI.ViewModels;
using System.Text;
using System.Drawing;
using System.Buffers.Text;

namespace DiffAPI.Controllers
{
    [Produces("application/json")]  // it only returns jsan data type
    [Consumes("application/json")]  // it accepts only json data type
    [Route("v1/diff")]  // we define the begining of the route
    public class DiffController : ControllerBase
    {
        // let's asume that the amout of saved data will be low and thet we want to keep the data only until the application will be closed, so we can store the data in the dictionary
        // store all input data in the form of: {id : {position : content}}
        public static Dictionary<int, Dictionary<Enums.Position, string>> dictStoredData = new Dictionary<int, Dictionary<Enums.Position, string>>();

        public void ResetDictStoredData()
        {
            dictStoredData = new Dictionary<int, Dictionary<Enums.Position, string>>();
        }


        public DiffController()
        {

        }


        #region definitions of processes
        /// <summary>
        /// Saves the given json data on the "left".
        /// </summary>
        /// <param name="id">identificator</param>
        /// <param name="jsonData">data for saving on the "left"</param>
        /// <returns></returns>
        /// <response code="201 Created">Data was saved</response>
        /// <response code="400 Bad Request">Data was bad (for example: null)</response>
		[HttpPost]
        [Route("{id}/left/{jsonData}")]
        public IActionResult Left(int id, string jsonData)
        {
            // validation of the input
            if (!IsJsonValid(jsonData))
            {
                return BadRequest("400 Bad Request");
            }

            return SaveData(id, Enums.Position.Left, DecodeBase64ToString(jsonData));
        }

        /// <summary>
        /// Saves the given json data on the "right".
        /// </summary>
        /// <param name="id">identificator</param>
        /// <param name="jsonData">json data for saving on the "right"</param>
        /// <returns></returns>
        /// <response code="201 Created">Data was saved</response>
        /// <response code="400 Bad Request">Data was bad (for example: null)</response>
		[HttpPost]
        [Route("{id}/right/{jsonData}")]
        public IActionResult Right(int id, string jsonData)
        {
            // validation of the input
            if (!IsJsonValid(jsonData))
            {
                return BadRequest("400 Bad Request");
            }

            return SaveData(id, Enums.Position.Right, DecodeBase64ToString(jsonData));
        }

        /// <summary>
        /// Request the comparison between "left" and "right" data with the same id.
        /// </summary>
        /// <param name="id">identificator</param>
        /// <returns></returns>
        /// <response code="400 Bad Request">given id is bad (for example is null)</response>
        /// <response code="404 Not Found">Either "left" or "right" side was not found at selected id</response>
        /// <response code="200 OK">Gives information on where are the differences between "left" and "right"</response>
		[HttpGet]
        [Route("{id}")]
        public IActionResult Diff(int id)
        {
            // validation of existance of "left" and "right" side
            if (!AreLeftAndRightExistanceValid(id))
            {
                return NotFound("404 Not Found");
            }

            // let's asume that in general we will not make comparisons between two same datas more than once, we do not save results of following comparison
            return Ok(GetComparison(id));
        }
        #endregion


        #region methods

        #region decode base64 string
        public bool IsJsonValid(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                return false;
            }

            // is jsonData base64
            int len = jsonData.Length;
            if (len % 4 != 0)  // lets asume that all inpud data has to be of the correct length (multiple of 4
            {
                return false;
            }

            Span<byte> buffer = new Span<byte>(new byte[len]);
            return Convert.TryFromBase64String(jsonData, buffer, out _);
        }

        public string DecodeBase64ToString(string str)
        {
            string decodedStr = Encoding.UTF8.GetString(Convert.FromBase64String(str));

            StringBuilder stringBuilder = new StringBuilder();
            var chars = decodedStr.ToCharArray();

            foreach (char c in chars)
            {
                stringBuilder.Append(((int)c).ToString("x"));
            }

            return stringBuilder.ToString();
        }
        #endregion

        /// <summary>
        /// validates that "left" and "right" side exist
        /// </summary>
        /// <param name="id">identificator</param>
        /// <returns></returns>
        public bool AreLeftAndRightExistanceValid(int id)
        {
            // at least one from "left" or "right" does not exist
            if (dictStoredData.ContainsKey(id))
            {
                Dictionary<Enums.Position, string> content = dictStoredData[id];
                if (content.ContainsKey(Enums.Position.Left) &&
                    content.ContainsKey(Enums.Position.Right))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Method for saving the data.
        /// </summary>
        /// <param name="id">identificator</param>
        /// <param name="position">data is on "left" or "right"</param>
        /// <param name="jsonData">Data for saving</param>
        /// <returns></returns>
        /// <response code="201 Created">Data was saved</response>
        public IActionResult SaveData(int id, Enums.Position position, string jsonData)
        {
            // if data with selected id and position does not exist yet => we save new one
            // else, we update the current data
            if (!dictStoredData.ContainsKey(id))
            {
                dictStoredData[id] = new Dictionary<Enums.Position, string>();
            }
            dictStoredData[id][position] = jsonData;

            return StatusCode(201, "201 Created");
        }

        /// <summary>
        /// It returnes comparison between "left" and "right" data at given id
        /// </summary>
        /// <param name="id">identificator</param>
        /// <returns></returns>
        public OutputForm GetComparison(int id)
        {
            OutputForm outputForm = new OutputForm();

            string left = dictStoredData[id][Enums.Position.Left];
            string right = dictStoredData[id][Enums.Position.Right];

            if (left.Length != right.Length)
            {
                outputForm.DiffResultType = "SizeDoNotMatch";
                return outputForm;
            }

            // check if they are equal
            List<Diffs> lstDiffs = GetDiffs(left, right);

            if (lstDiffs.Any())
            {
                outputForm.DiffResultType = "ContentDoNotMatch";
                outputForm.Diffs = lstDiffs;
                return outputForm;
            }

            // they are equal
            outputForm.DiffResultType = "Equals";
            return outputForm;
        }

        /// <summary>
        /// returns list of `Diffs` of `left` and `right` string that are of the same length and not `null`
        /// </summary>
        /// <param name="left">string that is compared</param>
        /// <param name="right">string that is compared with - has potential diffs</param>
        /// <returns>list of all diffs</returns>
        public List<Diffs> GetDiffs(string left, string right)
        {
            List<Diffs> diffs = new List<Diffs>();

            int thisDiffOffset = -1;
            int thisDiffLength = -1;

            for (int offset = 0; offset < left.Length; offset++)
            {
                if (left[offset].Equals(right[offset]))
                {
                    // there was a diff before this matching element
                    if (thisDiffLength != -1)
                    {
                        diffs.Add(new Diffs()
                        {
                            Length = thisDiffLength,
                            Offset = thisDiffOffset
                        });

                        thisDiffLength = -1;
                        thisDiffOffset = -1;
                    }
                }
                else  // there is a diff
                {
                    // first element of this diff
                    if (thisDiffOffset == -1)
                    {
                        thisDiffOffset = offset;
                        thisDiffLength = 1;
                    }
                    else
                    {
                        thisDiffLength++;
                    }
                }
            }

            // chack, if the last elements were a part of diff
            if (thisDiffOffset != -1)
            {
                diffs.Add(new Diffs()
                {
                    Length = thisDiffLength,
                    Offset = thisDiffOffset
                });
            }

            return diffs;
        }
        #endregion
    }
}
