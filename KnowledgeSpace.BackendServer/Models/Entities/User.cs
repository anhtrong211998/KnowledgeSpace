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

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTime Dob { get; set; }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", this.FirstName, this.LastName);
            }
        }

        [Range(0, int.MaxValue)]
        public int? NumberOfKnowledgeBases { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int? NumberOfVotes { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int? NumberOfReports { get; set; } = 0;

        public virtual IList<ActivityLog> ActivityLogs { get; set; }
    }
}
