﻿// <auto-generated />
using KsSelect.Samples.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace KsSelect.Samples.Migrations
{
    [DbContext(typeof(SampleDbContext))]
    partial class KsSelectContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("KsSelect.Samples.Models.BookLocalization", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("BookId")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Language")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.ToTable("BooksLocalization");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            BookId = 1L,
                            Description = "descrizione in italiano libro 1",
                            Language = "IT",
                            Title = "Titolo italiano libro 1"
                        },
                        new
                        {
                            Id = 2L,
                            BookId = 2L,
                            Description = "descrizione in italiano libro 2",
                            Language = "IT",
                            Title = "Titolo italiano libro 2"
                        },
                        new
                        {
                            Id = 3L,
                            BookId = 3L,
                            Description = "descrizione in italiano libro 3",
                            Language = "IT",
                            Title = "Titolo italiano libro 3"
                        },
                        new
                        {
                            Id = 4L,
                            BookId = 4L,
                            Description = "descrizione in italiano libro 4",
                            Language = "IT",
                            Title = "Titolo italiano libro 4"
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

            modelBuilder.Entity("KsSelect.Samples.Models.BookLocalization", b =>
                {
                    b.HasOne("KsSelect.Samples.Models.Book", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");
                });

            modelBuilder.Entity("KsSelect.Samples.Models.Author", b =>
                {
                    b.Navigation("Books");
                });
#pragma warning restore 612, 618
        }
    }
}
