using System.ComponentModel.DataAnnotations;

namespace CosmeticChecker.API.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int ProductId { get; set; }
    public string Text { get; set; }
    public int Rating { get; set; }
    public DateTime CreationDate { get; set; }
}

public class CreateReviewRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Text { get; set; }
}