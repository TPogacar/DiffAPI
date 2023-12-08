using System.ComponentModel.DataAnnotations;

namespace DiffAPI.ViewModels
{
    /// <summary>
    /// int Offset, int Length
    /// </summary>
    public class Diffs
    {
        [Required]
        public int Offset { get; set; }
        [Required]
        public int Length { get; set; }


        public override bool Equals(object? obj)
        {
            if (obj == null ||
                !obj.GetType().Equals(typeof(Diffs)))
            {
                return false;
            }
            return Length.Equals((obj as Diffs).Length) &&
                Offset.Equals((obj as Diffs).Offset);
        }
    }
}
