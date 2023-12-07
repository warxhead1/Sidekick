﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sidekick.Modules.Wealth.Models;

#nullable disable

namespace Sidekick.Modules.Wealth.Migrations
{
    [DbContext(typeof(WealthDbContext))]
    [Migration("20231120003516_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.12");

            modelBuilder.Entity("Sidekick.Modules.Wealth.Models.FullSnapshot", b =>
                {
                    b.Property<long>("Date")
                        .HasColumnType("INTEGER");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Total")
                        .HasColumnType("REAL");

                    b.HasKey("Date");

                    b.ToTable("FullSnapshots");
                });

            modelBuilder.Entity("Sidekick.Modules.Wealth.Models.Item", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("Category")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GemLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Icon")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ItemLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("MapTier")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MaxLinks")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.Property<string>("StashId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Total")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Sidekick.Modules.Wealth.Models.Stash", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<long>("LastUpdate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Parent")
                        .HasColumnType("TEXT");

                    b.Property<double>("Total")
                        .HasColumnType("REAL");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Stashes");
                });

            modelBuilder.Entity("Sidekick.Modules.Wealth.Models.StashSnapshot", b =>
                {
                    b.Property<long>("Date")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StashId")
                        .HasColumnType("TEXT");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Total")
                        .HasColumnType("REAL");

                    b.HasKey("Date", "StashId");

                    b.ToTable("StashSnapshots");
                });
#pragma warning restore 612, 618
        }
    }
}