using System.ComponentModel.DataAnnotations;

namespace CosmeticChecker.API.DTOs
{
    public class PendingReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ReviewStatusDto
    {
        public int Id { get; set; }
        public bool IsApproved { get; set; }
        public string Status => IsApproved ? "Approved" : "Rejected";
        public string RejectionReason { get; set; }
        public DateTime? ModeratedDate { get; set; }
        public string ModeratedBy { get; set; }
    }

    public class RejectedReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? ModeratedDate { get; set; }
        public string ModeratedBy { get; set; }
    }

    public class ApprovedReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } 
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModeratedDate { get; set; }
        public string ModeratedBy { get; set; }
    }
}

