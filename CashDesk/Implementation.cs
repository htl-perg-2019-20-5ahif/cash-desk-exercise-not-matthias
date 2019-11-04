using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashDesk
{
    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        private CashDeskContext dataContext;

        private void ThrowIfNotInitialized()
        {
            if (dataContext == null)
            {
                throw new InvalidOperationException("Not initialized");
            }
        }

        /// <inheritdoc />
        public Task InitializeDatabaseAsync()
        {
            if (dataContext != null)
            {
                throw new InvalidOperationException("Already initialized");
            }

            dataContext = new CashDeskContext();

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentException();
            }

            if (await dataContext.Members.AnyAsync(m => m.LastName == lastName))
            {
                throw new DuplicateNameException();
            }

            //
            // Add new member and save the context
            //
            var newMember = new Member { Birthday = birthday, FirstName = firstName, LastName = lastName };
            _ = await dataContext.Members.AddAsync(newMember);
            _ = await dataContext.SaveChangesAsync();

            return newMember.MemberNumber;
        }

        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber)
        {
            ThrowIfNotInitialized();

            //
            // Find the member
            //
            var member = await dataContext.Members.FirstOrDefaultAsync(m => m.MemberNumber == memberNumber);
            if (member == default)
            {
                throw new ArgumentException();
            }

            // 
            // Delete the member
            // 
            _ = dataContext.Members.Remove(member);
            _ = await dataContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            ThrowIfNotInitialized();

            //
            // Check if a valid membership exists
            //
            if (await dataContext.Memberships.AnyAsync(
                    m => m.Member.MemberNumber == memberNumber
                    && DateTime.Now >= m.Begin
                    && DateTime.Now <= m.End))
            {
                throw new AlreadyMemberException();
            }

            // 
            // Add membership and save the context
            // 
            var membership = new Membership
            {
                Member = await dataContext.Members.FirstOrDefaultAsync(m => m.MemberNumber == memberNumber),
                Begin = DateTime.Now,
                End = DateTime.MaxValue
            };
            _ = await dataContext.Memberships.AddAsync(membership);
            _ = await dataContext.SaveChangesAsync();

            return membership;
        }

        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            ThrowIfNotInitialized();

            //
            // Find valid membership
            //
            var membership = await dataContext.Memberships.FirstOrDefaultAsync(
                    m => m.Member.MemberNumber == memberNumber
                    && DateTime.Now >= m.Begin
                    && DateTime.Now <= m.End);
            if (membership == default)
            {
                throw new NoMemberException();
            }

            // 
            // Cancel it by setting the end date to now and save the context
            // 
            membership.End = DateTime.Now;
            _ = await dataContext.SaveChangesAsync();

            return membership;
        }

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            ThrowIfNotInitialized();

            //
            // Find the member
            //
            var member = await dataContext.Members.FirstOrDefaultAsync(m => m.MemberNumber == memberNumber);
            if (member == default)
            {
                throw new ArgumentException();
            }

            //
            // Find valid membership
            //
            var membership = await dataContext.Memberships.FirstOrDefaultAsync(
                    m => m.Member.MemberNumber == memberNumber
                    && DateTime.Now >= m.Begin
                    && DateTime.Now <= m.End);
            if (membership == default)
            {
                throw new NoMemberException();
            }

            //
            // Create new deposit and save the context
            //
            var deposit = new Deposit
            {
                Membership = membership,
                Amount = amount
            };
            _ = await dataContext.Deposits.AddAsync(deposit);
            _ = await dataContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            ThrowIfNotInitialized();


            return default;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (dataContext != null)
            {
                dataContext.Dispose();
                dataContext = null;
            }
        }
    }
}
