using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CashDesk
{
    public class Member : IMember
    {
        [Key]
        public int MemberNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        public List<Membership> Memberships { get; set; }
    }

    public class Membership : IMembership
    {
        public int MembershipID { get; set; }

        [Required]
        public Member Member { get; set; }

        [Required]
        public DateTime Begin { get; set; }

        [Required]
        public DateTime End { get; set; }

        public List<Deposit> Deposits { get; set; }

        IMember IMembership.Member => Member;
    }

    public class Deposit : IDeposit
    {
        public int DepositID { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [Required]
        [Range(0, Double.MaxValue)]
        public decimal Amount { get; set; }

        IMembership IDeposit.Membership => Membership;
    }

    public class DepositStatistics : IDepositStatistics
    {
        public IMember Member { get; set; }

        public int Year { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
