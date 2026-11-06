using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;

namespace Pocs.Aspire.Infrastructure.Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    internal const string EmailUniqueIndexName = "IX_User_Email";

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
            .HasMaxLength(FirstName.MaxLength)
            .HasConversion(domain => domain.Value, entity => FirstName.From(entity));

        builder.Property(e => e.LastName)
            .HasColumnName("LastName")
            .IsRequired()
            .HasMaxLength(LastName.MaxLength)
            .HasConversion(domain => domain.Value, entity => LastName.From(entity));

        builder.Property(e => e.Email)
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(Email.MaxLength)
            .HasConversion(domain => domain.Value, entity => Email.From(entity));

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName(EmailUniqueIndexName);
    }
}
