using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Application.Common.Interfaces;

namespace UserService.Infrastructure.Persistence;

public partial class UserDbContextScaffolded : DbContext, IUserDbContext
{
    public UserDbContextScaffolded(DbContextOptions<UserDbContextScaffolded> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
