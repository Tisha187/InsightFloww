using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InsightFlow.Core.Models;

public partial class InsightFlowAiContext : DbContext
{
    public InsightFlowAiContext()
    {
    }

    public InsightFlowAiContext(DbContextOptions<InsightFlowAiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AgentToolExecution> AgentToolExecutions { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Dataset> Datasets { get; set; }

    public virtual DbSet<DatasetColumn> DatasetColumns { get; set; }

    public virtual DbSet<ForecastPoint> ForecastPoints { get; set; }

    public virtual DbSet<ForecastRun> ForecastRuns { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source =(localdb)\\MSSQLLocalDB;Initial Catalog=InsightFlowAI;Integrated Security=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgentToolExecution>(entity =>
        {
            entity.HasIndex(e => new { e.ConversationId, e.CreatedAtUtc }, "IX_AgentToolExecutions_Conversation_Created");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CompletedAtUtc).HasPrecision(3);
            entity.Property(e => e.CreatedAtUtc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_AgentToolExecutions_CreatedAtUtc");
            entity.Property(e => e.ErrorCode).HasMaxLength(100);
            entity.Property(e => e.StartedAtUtc).HasPrecision(3);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Started", "DF_AgentToolExecutions_Status");
            entity.Property(e => e.ToolName).HasMaxLength(150);

            entity.HasOne(d => d.ChatMessage).WithMany(p => p.AgentToolExecutions)
                .HasForeignKey(d => d.ChatMessageId)
                .HasConstraintName("FK_AgentToolExecutions_ChatMessages");

            entity.HasOne(d => d.Conversation).WithMany(p => p.AgentToolExecutions)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_AgentToolExecutions_Conversations");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_AspNetRoleClaims_AspNetRoles");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex").IsUnique();

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_AspNetUserRoles_AspNetRoles"),
                    l => l.HasOne<AspNetUser>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_AspNetUserRoles_AspNetUsers"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AspNetUserClaims_AspNetUsers");
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AspNetUserLogins_AspNetUsers");
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AspNetUserTokens_AspNetUsers");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasIndex(e => new { e.ConversationId, e.CreatedAtUtc, e.Id }, "IX_ChatMessages_Conversation_Created");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAtUtc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ChatMessages_CreatedAtUtc");
            entity.Property(e => e.Role).HasMaxLength(30);

            entity.HasOne(d => d.Conversation).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_ChatMessages_Conversations");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasIndex(e => e.DatasetId, "IX_Conversations_DatasetId").HasFilter("([DatasetId] IS NOT NULL)");

            entity.HasIndex(e => new { e.UserId, e.UpdatedAtUtc }, "IX_Conversations_User_Updated").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAtUtc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Conversations_CreatedAtUtc");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasDefaultValue("New conversation", "DF_Conversations_Title");
            entity.Property(e => e.UpdatedAtUtc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Conversations_UpdatedAtUtc");

            entity.HasOne(d => d.User).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversations_AspNetUsers");

            entity.HasOne(d => d.Dataset).WithMany(p => p.Conversations)
                .HasPrincipalKey(p => new { p.Id, p.UserId })
                .HasForeignKey(d => new { d.DatasetId, d.UserId })
                .HasConstraintName("FK_Conversations_Datasets_Owner");
        });

        modelBuilder.Entity<Dataset>(entity =>
        {
            entity.HasIndex(e => new { e.Id, e.UserId }, "UQ_Datasets_Id_UserId").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAtUtc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Datasets_CreatedAtUtc");
            entity.Property(e => e.FileFormat).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OriginalFileName).HasMaxLength(260);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Uploaded", "DF_Datasets_Status");
            entity.Property(e => e.StorageKey).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Datasets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Datasets_AspNetUsers");
        });

        modelBuilder.Entity<DatasetColumn>(entity =>
        {
            entity.HasIndex(e => e.DatasetId, "IX_DatasetColumns_DatasetId");

            entity.HasIndex(e => new { e.DatasetId, e.OrdinalPosition }, "UQ_DatasetColumns_Dataset_Position").IsUnique();

            entity.Property(e => e.ColumnName).HasMaxLength(128);
            entity.Property(e => e.DataType).HasMaxLength(30);
            entity.Property(e => e.SemanticRole).HasMaxLength(30);

            entity.HasOne(d => d.Dataset).WithMany(p => p.DatasetColumns)
                .HasForeignKey(d => d.DatasetId)
                .HasConstraintName("FK_DatasetColumns_Datasets");
        });

        modelBuilder.Entity<ForecastPoint>(entity =>
        {
            entity.HasIndex(e => new { e.ForecastRunId, e.ForecastDate }, "UQ_ForecastPoints_Run_Date").IsUnique();

            entity.Property(e => e.LowerBound).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.PredictedValue).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UpperBound).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.ForecastRun).WithMany(p => p.ForecastPoints)
                .HasForeignKey(d => d.ForecastRunId)
                .HasConstraintName("FK_ForecastPoints_ForecastRuns");
        });

        modelBuilder.Entity<ForecastRun>(entity =>
        {
            entity.HasIndex(e => new { e.DatasetId, e.CreatedAtUtc }, "IX_ForecastRuns_Dataset_Created").IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.CreatedAtUtc }, "IX_ForecastRuns_User_Created").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CompletedAtUtc).HasPrecision(3);
            entity.Property(e => e.CreatedAtUtc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ForecastRuns_CreatedAtUtc");
            entity.Property(e => e.DateColumn).HasMaxLength(128);
            entity.Property(e => e.Frequency).HasMaxLength(20);
            entity.Property(e => e.ModelVersion).HasMaxLength(100);
            entity.Property(e => e.StartedAtUtc).HasPrecision(3);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending", "DF_ForecastRuns_Status");
            entity.Property(e => e.TargetColumn).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.ForecastRuns)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ForecastRuns_AspNetUsers");

            entity.HasOne(d => d.Dataset).WithMany(p => p.ForecastRuns)
                .HasPrincipalKey(p => new { p.Id, p.UserId })
                .HasForeignKey(d => new { d.DatasetId, d.UserId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ForecastRuns_Datasets_Owner");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
