using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    public class User : IdentityUser
    {
        public User()
        {
        }

        public User(string id, string userName, string firstName, string lastName,
            string email, string phoneNumber, DateTime dob)
        {
            Id = id;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Dob = dob;
        }

        [Required(ErrorMessage = "First Name is required.")]
        [MaxLength(50, ErrorMessage = "First Name is at most 50 charactes.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [MaxLength(50, ErrorMessage = "Last Name is at most 50 charactes.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "BirthDay is required.")]
        public DateTime Dob { get; set; }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", this.FirstName, this.LastName);
            }
        }

        [Range(0, int.MaxValue, ErrorMessage = "Number of KnowledgeBases must be non-negative.")]
        public int? NumberOfKnowledgeBases { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Number of Votes must be non-negative.")]
        public int? NumberOfVotes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Number of Reports must be non-negative.")]
        public int? NumberOfReports { get; set; }

        public virtual IList<ActivityLog> ActivityLogs { get; set; }
    }
}
