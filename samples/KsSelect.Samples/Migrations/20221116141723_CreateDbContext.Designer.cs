﻿// <auto-generated />
using KsSelect.Samples.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace KsSelect.Samples.Migrations
{
    [DbContext(typeof(SampleDbContext))]
    [Migration("20221116141723_CreateDbContext")]
    partial class CreateDbContext
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("KsSelect.Samples.Models.Author", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Authors");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            FirstName = "Sthepen",
                            LastName = "King"
                        },
                        new
                        {
                            Id = 2L,
                            FirstName = "Neil ",
                            LastName = "Gaiman"
                        });
                });

            modelBuilder.Entity("KsSelect.Samples.Models.Book", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Books");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            AuthorId = 1L,
                            Description = "lorem ipsum dolor",
                            Title = "IT"
                        },
                        new
                        {
                            Id = 2L,
                            AuthorId = 1L,
                            Description = "lorem ipsum dolor",
                            Title = "Misery"
                        },
                        new
                        {
                            Id = 3L,
                            AuthorId = 1L,
                            Description = "lorem ipsum dolor",
                            Title = "Cujo"
                        },
                        new
                        {
                            Id = 4L,
                            AuthorId = 2L,
                            Description = "lorem ipsum dolor",
                            Title = "American Gods"
                        });
                });

            modelBuilder.Entity("KsSelect.Samples.Models.Book", b =>
                {
                    b.HasOne("KsSelect.Samples.Models.Author", "Author")
                        .WithMany("Books")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");
                });

            modelBuilder.Entity("KsSelect.Samples.Models.Author", b =>
                {
                    b.Navigation("Books");
                });
#pragma warning restore 612, 618
        }
    }
}
