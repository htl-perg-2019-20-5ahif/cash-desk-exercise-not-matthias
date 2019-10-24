﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashDesk
{
    #region Model
    /// <summary>
    /// Represents a club member
    /// </summary>
    public interface IMember
    {
        /// <summary>
        /// Uniquely identifies a club member.
        /// </summary>
        /// <remarks>
        /// When adding new members, this number should be auto-generated by the system.
        /// </remarks>
        int MemberNumber { get; }

        /// <summary>
        /// First name of club member
        /// </summary>
        /// <remarks>
        /// This is a mandatory field. The max. length is 100 characters.
        /// </remarks>
        string FirstName { get; }

        /// <summary>
        /// Last name of club member
        /// </summary>
        /// <remarks>
        /// This is a mandatory field. The max. length is 100 characters. Note that
        /// the last name has to be unique in the entire database. Two members with the same
        /// last name are not allowed.
        /// </remarks>
        string LastName { get; }

        /// <summary>
        /// Member's birthday
        /// </summary>
        /// <remarks>
        /// This is a mandatory field.
        /// </remarks>
        DateTime Birthday { get; }
    }

    /// <summary>
    /// Represents a club membership
    /// </summary>
    /// <remarks>
    /// Whenever a new member joins the club, a new membership record is created.
    /// If a member cancels her membership, the <see cref="End"/> property
    /// is set. A member who has cancelled can re-join later. In that case, a new
    /// membership record will be created.
    /// 
    /// A membership without a value in <see cref="End"/> is called an "active membership".
    /// 
    /// The membership record with a set <see cref="End"/> must be the last membership
    /// record for this member.
    /// </remarks>
    public interface IMembership
    {
        /// <summary>
        /// References the member for this membership
        /// </summary>
        /// <remarks>
        /// This is a mandatory field.
        /// </remarks>
        IMember Member { get; }

        /// <summary>
        /// Begin of the membership
        /// </summary>
        /// <remarks>
        /// This is a mandatory field.
        /// </remarks>
        DateTime Begin { get; }

        /// <summary>
        /// End of the membership
        /// </summary>
        /// <remarks>
        /// This is an optional field. If this field is set, its value has to be greater than <see cref="Begin"/>.
        /// </remarks>
        DateTime End { get; }
    }

    /// <summary>
    /// Represents a deposit of a member
    /// </summary>
    /// <remarks>
    /// Club members have to pay membership fees. This entity stores each deposit of club members.
    /// New deposity can only be made on active memberships.
    /// </remarks>
    public interface IDeposit
    {
        /// <summary>
        /// References the membership that this deposit is for
        /// </summary>
        /// <remarks>
        /// This is a mandatory field.
        /// </remarks>
        IMembership Membership { get; }

        /// <summary>
        /// Deposited amount of money
        /// </summary>
        /// <remarks>
        /// This is a mandatory field. The amount must be greater than 0.
        /// </remarks>
        decimal Amount { get; }
    }

    public interface IDepositStatistics
    {
        IMember Member { get; }

        int Year { get; }

        decimal TotalAmount { get; }
    }
    #endregion

    #region Exceptions
    public class AlreadyMemberException : Exception
    {
        public AlreadyMemberException(string message) : base(message) { }

        public AlreadyMemberException(string message, Exception innerException) : base(message, innerException) { }

        public AlreadyMemberException() { }
    }

    public class NoMemberException : Exception
    {
        public NoMemberException(string message) : base(message) { }

        public NoMemberException(string message, Exception innerException) : base(message, innerException) { }

        public NoMemberException() { }
    }

    public class DuplicateNameException : Exception
    {
        public DuplicateNameException(string message) : base(message) { }

        public DuplicateNameException(string message, Exception innerException) : base(message, innerException) { }

        public DuplicateNameException() { }
    }
    #endregion 

    /// <summary>
    /// Implements the data access layer
    /// </summary>
    public interface IDataAccess : IDisposable
    {
        /// <summary>
        /// Initializes the data access layer
        /// </summary>
        /// <remarks>
        /// A user has to call this method before calling any other method of the class.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has already been called
        /// </exception>
        Task InitializeDatabaseAsync();

        /// <summary>
        /// Adds a new member
        /// </summary>
        /// <seealso cref="IMember"/>
        /// <returns>
        /// Number of the new member
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        /// <exception cref="ArgumentException">
        /// At least one of the parameters contains an invalid value. The exception's <see cref="Exception.Message"/>
        /// property has to contain details about the error.
        /// </exception>
        /// <exception cref="DuplicateNameException">
        /// Member with the same last name already exists.
        /// </exception>
        Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday);

        /// <summary>
        /// Delets a member
        /// </summary>
        /// <param name="memberNumber">Number of the member to delete</param>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Unknown <paramref name="memberNumber"/>.
        /// </exception>
        /// <remarks>
        /// This method deletes all data that is stored for this member (including
        /// memberships and deposits).
        /// </remarks>
        Task DeleteMemberAsync(int memberNumber);

        /// <summary>
        /// Adds a membership record for the specified member
        /// </summary>
        /// <returns>
        /// Created membership record
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        /// <exception cref="AlreadyMemberException">
        /// The member is already an active member.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Unknown <paramref name="memberNumber"/>.
        /// </exception>
        /// <remarks>
        /// The new membership starts at the time of calling this method.
        /// </remarks>
        Task<IMembership> JoinMemberAsync(int memberNumber);

        /// <summary>
        /// Ends the membership for the specified member
        /// </summary>
        /// <returns>
        /// Updated membership record with <see cref="IMembership.End"/> set.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        /// <exception cref="NoMemberException">
        /// The member is currently not an active member.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Unknown <paramref name="memberNumber"/>.
        /// </exception>
        /// <remarks>
        /// The membership ends at the time of calling this method.
        /// </remarks>
        Task<IMembership> CancelMembershipAsync(int memberNumber);

        /// <summary>
        /// Deposit the specified amount for the specified member
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Unknown <paramref name="memberNumber"/> or invalid value in <paramref name="amount"/>.
        /// </exception>
        /// <exception cref="NoMemberException">
        /// The member is currently not an active member.
        /// </exception>
        Task DepositAsync(int memberNumber, decimal amount);

        /// <summary>
        /// Gets statistics about deposits per member.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="InitializeDatabaseAsync"/> has not been called before
        /// </exception>
        Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync();
    }
}