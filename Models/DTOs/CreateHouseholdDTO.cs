using System.ComponentModel.DataAnnotations;

public class CreateHouseholdDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public int AdminUserId { get; set; }
}
