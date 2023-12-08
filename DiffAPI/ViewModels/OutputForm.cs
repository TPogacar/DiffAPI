using DiffAPI.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace DiffAPI.ViewModels
{
    /// <summary>
    /// string DiffResultType,  List<Diffs>? Diffs
    /// </summary>
    public class OutputForm
    {
        //public int Id { get; set; }

        [Required]
        public string DiffResultType { get; set; }

        public List<Diffs>? Diffs { get; set; }
    }
}
