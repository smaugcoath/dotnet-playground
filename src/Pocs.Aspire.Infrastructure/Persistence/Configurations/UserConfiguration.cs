using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;

namespace Pocs.Aspire.Infrastructure.Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("Id")
            .IsRequired()
            .HasConversion(domain => domain.Value, entity => UserId.From(entity));

        builder.Property(e => e.FirstName)
            .HasColumnName("FirstName")
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(domain => domain.Value, entity => FirstName.From(entity));

        builder.Property(e => e.LastName)
            .HasColumnName("LastName")
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(domain => domain.Value, entity => LastName.From(entity));

        builder.Property(e => e.Email)
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(domain => domain.Value, entity => Email.From(entity));

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName(User.EmailUniqueIndexName);
    }
}
