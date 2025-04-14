﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WSPWeatherService.Persistence;

#nullable disable

namespace WSPWeatherService.Persistence.Migrations
{
    [DbContext(typeof(WeatherDbContext))]
    [Migration("20250414232929_OptimizeIndexesForAggregations")]
    partial class OptimizeIndexesForAggregations
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WSPWeatherService.Persistence.Models.MeasurementEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWSEQUENTIALID()");

                    b.Property<string>("Station")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<double>("Value")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp");

                    b.HasIndex("Type", "Timestamp");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("Type", "Timestamp"), new[] { "Value" });

                    b.HasIndex("Station", "Timestamp", "Type")
                        .IsUnique();

                    b.ToTable("Measurements");
                });
#pragma warning restore 612, 618
        }
    }
}
