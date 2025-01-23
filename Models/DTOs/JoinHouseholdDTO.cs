using System.ComponentModel.DataAnnotations;

public class JoinHouseholdDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string JoinCode { get; set; }
}
